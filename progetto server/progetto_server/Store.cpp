#include "Store.h"
#include "prep_socket.h"


Store::Store()
{
	start.store(true);
	master=INVALID_SOCKET;
	client=INVALID_SOCKET;
}


Store::~Store()
{
}

int Store::create()
{
	//passo per riferimento
	return setup_s(master, client);
}

int Store::accept_connection()
{
	addrlen = sizeof(new_client);
	return client = accept(master, (struct sockaddr *) &new_client, &addrlen);
}

IN_ADDR Store::ret_ip()
{
	return new_client.sin_addr;
}

std::mutex & Store::ret_mutex()
{
	return m;
}

SOCKET & Store::ret_client()
{
	return client;
}

void Store::error()
{
	start.store(false);
	return;
}

bool Store::ret_start()
{
	return start.load();
}

void Store::termina_client()
{
	closesocket(client);
	start.store(true);
	return;
}

void Store::termina_master()
{
	closesocket(master);
	WSACleanup();
	return;
}

void Store::Send_json(std::string str)
{
	int len = str.length() + 1;
	int l = htonl(len);
	char *buffer = NULL;
	//alloco e controllo che abbia buon fine altrimenti esco e disconnetto
	if((buffer=new char[len])==0){
		printf("ATTENZIONE!! Memoria non allocata per il file json.");
		start.store(false);
		return;
	}
	memcpy(buffer, str.c_str(), len);
	//prendo il lock e invio prima la lunghezza del file e poi il file json stesso
	std::lock_guard<std::mutex> lg(m);
	if (send(client, (const char*)&l, 4, 0) == SOCKET_ERROR) {
		printf("Send fallita, errore: %d\n", WSAGetLastError());
		start.store(false);
	}
	else if(send(client, buffer,len, 0) == SOCKET_ERROR){
		printf("Send fallita, errore: %d\n", WSAGetLastError());
		start.store(false);
	}
	delete[]buffer;
	return;
}

int Store::recv_n(int totread, char * buffer, int len_buffer, bool flag)
{
	int nletti = 0;
	int n;
	//ricevo esattamente n-caratteri(tot-read)
	while (nletti < totread) {
		if ((n = recv(client, buffer + nletti, len_buffer - nletti, 0)) <= 0) {
			printf("Recv fallita, errore: %d\n", WSAGetLastError());
			start.store(false);
			return -1; // errore per funzione chiamante 
		}
		nletti += n;
	}
	if (flag) {
		buffer[nletti] = '\0';
	}
	return nletti;
}
