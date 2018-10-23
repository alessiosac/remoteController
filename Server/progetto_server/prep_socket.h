#pragma once
//#undef UNICODE	//attenzione magari tolgo

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>

#include <stdio.h>
#include <iostream>
#include <tchar.h>


#pragma comment (lib, "Ws2_32.lib")

#define DEFAULT_PORT "27016"

int setup_s(SOCKET &master, SOCKET &client) {
	
	WSADATA wsaData;
	struct sockaddr_in server;
	//The WSAStartup function initiates use of the Winsock DLL by a process.
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
		printf("WSAStartup failed with error: %d\n", GetLastError());
		return -1;
	}
	//Create socket for master
	if ((master = socket(PF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET) {
		printf("Error creating socket: %d\n", GetLastError());
		return -1;
	}

	// Create address for master socket.
	memset(&server, 0, sizeof(server));
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = INADDR_ANY;
	server.sin_port = htons((u_short)atoi(DEFAULT_PORT));

	// Bind address to the socket .
	if (bind(master, (struct sockaddr *)&server, sizeof(server)) == SOCKET_ERROR) {
		printf("can't bind to %d port: %d\n", server.sin_port, WSAGetLastError());
		return -1;
	}
	// Make socket passive.
	if (listen(master, SOMAXCONN) == SOCKET_ERROR) {
		printf("Error listening on %d port: %d\n", server.sin_port, WSAGetLastError());
		return -1;
	}
	return 0;
};