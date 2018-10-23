#pragma once
#include "Include.h"
class Store
{
private:
	std::mutex m;
	SOCKET master;
	SOCKET client;
	struct sockaddr_in new_client;	// client address for accept
	int addrlen;	// length of client address
	std::atomic<bool> start;
public:
	Store();
	~Store();
	int create();
	int accept_connection();
	IN_ADDR ret_ip();
	std::mutex& ret_mutex();
	SOCKET& ret_client();
	void error();
	bool ret_start();
	void termina_client();
	void termina_master();
	void Send_json(std::string str);
	int recv_n(int totread, char *buffer, int len_buffer, bool flag);
};

