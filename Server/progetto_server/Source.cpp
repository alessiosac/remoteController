#include "Include.h"
#include "Function.h"
#pragma comment(lib, "oleaut32.lib")

//DEFINE
#define N_BIT_COLOR 32

// Funzione per ricevere le informazioni riguardo ai porcessi attivi
BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam);

//Funzioni del thread
void cambio_focus(Store&);
void open_close_prg(Store&);

//variabile globale
std::list<Applications> lista;
std::map<HWND,std::vector<BYTE>> mymap;

int main()
{
	Store s;
	if (s.create() == -1) {
		// errore nella creazione del socket, il programma si arresta
		return -1;
	}

	//due cicli while : uno per server sempre attivo, uno per chiudere le connessioni in caso di errori, o in caso di chiusura del socket
	while (true) {
		while (s.ret_start()) {

			printf("In attesa di connessioni...\n");
			//accetto la connessione
			if (s.accept_connection() == -1) {
				printf("Errore durante l'accept: %d\n", WSAGetLastError());
				printf("Connessione terminata\n");
				break;
			}

			printf("Connection accettata -> %s\n", inet_ntoa(s.ret_ip()));

			EnumWindows(EnumWindowsProc, (LPARAM)nullptr);

			crea_json(lista, "",NULL, std::ref(s));

			std::thread t1(cambio_focus, std::ref(s));
			std::thread t2(open_close_prg, std::ref(s));
			recv_input(std::ref(s));
			t1.join();
			t2.join();
		}
		printf("Connessione chiusa con -> %s\n\n\n", inet_ntoa(s.ret_ip()));
		s.termina_client();
		lista.clear();
		mymap.clear();
	}
	s.termina_master();
	system("pause");
	return 0;

}

BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM ptr)
{
	char title[5000];
	bool focus = false;
	int len;
	//per le window che hanno un icona
	if (!::IsWindowVisible(hwnd)) {
		return TRUE;
	}
	//lunghezza del nome
	len = (int)GetWindowTextLength(hwnd);
	if (len <= 0) {
		return TRUE;
	}
	// prendo il nome della window
	GetWindowText(hwnd, title, sizeof(title));
	//se l icona è gia stata salvata precedentemente, non la "ricreo" piu, ma la prendo dalla mymap
	if(mymap.find(hwnd)!=mymap.end()){
		std::vector<BYTE> data=mymap.find(hwnd)->second;
		lista.push_back(Applications(hwnd, title, data, len + 1));
		data.clear();
		return TRUE;
	}
	HICON hIcon = NULL;
	// cerco l icona della window  con parametri diversi, finche non ne trovo una
	HICON iconHandle = (HICON)::SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
	if (!iconHandle) {
		iconHandle = (HICON)::SendMessage(hwnd, WM_GETICON, ICON_SMALL2, 0);
	}
	if (!iconHandle) {
		iconHandle = (HICON)::SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
	}
	if (!iconHandle) {
		iconHandle = (HICON)::GetClassLongPtr(hwnd, GCL_HICON);
	}
	if (!iconHandle) {
		iconHandle = (HICON)::GetClassLongPtr(hwnd, GCL_HICONSM);
	}
	//std::vector<BYTE> data = SaveIcon(iconHandle);  "icona brutta"
	std::vector<BYTE> data = SaveIcon(iconHandle,N_BIT_COLOR); // "icona bella"
	//se non trovo nessu icona, continuo la "ricerca" delle app attive
	if (data.empty()) {
		return TRUE;
	}
	//se l icona c'é, inserisco la hwnd nella lista 
	lista.push_back(Applications(hwnd, title, data, len + 1));
	//inserisco l icona nella mappa
	mymap.insert(std::pair<HWND,std::vector<BYTE>>(hwnd,data));
	data.clear();
	return TRUE;
}

// funzione del thread t1 per notificare
// l'aprirsi od il chiudersi di una window
void open_close_prg(Store& s) {
	std::list<Applications> tmp;
	std::list<Applications> tmp_new;
	while (s.ret_start()) {
		//copio la lista in tmp
		tmp = lista;
		//pulisco la lista
		lista.clear();
		//ricreo la lista 
		EnumWindows(EnumWindowsProc, (LPARAM)nullptr);
		//copio la nuova lista in tmp_new
		tmp_new = lista;
		//trasferisco gli elementi da tmp_new a tmp, it per sapere da quale elemento partire(in questo caso tutta la lista, quindi il punta al primo elemento)
		std::list<Applications>::iterator it = tmp.begin();
		tmp.splice(it, tmp_new);
		//pulisco tmp_new
		tmp_new.clear();
		//ordino la lista; vedere operator< in application.cpp ; in modo da avere handle uguali "vicine tra di loro"
		tmp.sort();
		// elimino gli elementi doppi, ; vedere operator== in application.cpp; se l elemento é doppio, viene contrassegnato con elimina=true
		tmp.unique();
		//elimino dalla lista gli elementi con elimina=true
		tmp.remove_if(elimina);
		//la lista ora contiene solo gli elementi  appena aperti o chiusi
		//se la lista contiene qualche elemento lo invio, altrimenti ricomincio
		if (!tmp.empty()) {
			// funzione per json, con elemeti da inviare con enumerazione
			crea_json(tmp, "",NULL, s);
		}
		tmp.clear();
	}
	return;
}

// funzione del thread t1 per notificare
// il cambiamento del focus
void cambio_focus(Store& s) {
	char name[5000];
	char name1[5000];
	HWND focus_prec = NULL;
	HWND focus = NULL;
	std::list<Applications> tmp; //lista vuota da passare alla funzione
	std::string title_prec;
	while (s.ret_start()) {
		GetWindowText((focus = GetForegroundWindow()), name, sizeof(name));
		std::string title(name);
		//la handle e il nome dell app devono essere cambiati 
		if (((focus_prec != focus) || (title!=title_prec) ) ) { //&& focus!=NULL && title !=""
			crea_json(tmp, title,focus, s);
			//aggiorno hwnd e titolo del vecchio focus, con quello appena preso
			focus_prec = focus;
			title_prec=title;
		}
	}
	return;
}




