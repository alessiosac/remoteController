#pragma once
#include "Applications.h"
#include "Include.h"
#include "Store.h"

//std::vector<BYTE> SaveIcon(HICON);

std::vector<BYTE> SaveIcon(HICON,int);

void crea_json(std::list<Applications>, std::string ,HWND, Store&);

bool elimina(Applications &);

void recv_input(Store&);

void readFile(const char*, std::vector<BYTE>&);