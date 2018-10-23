#pragma once
#include "Include.h"
class Applications
{
private:
	HWND hwnd;
	std::string title;
	std::vector<BYTE> icon;
	bool elimina;
public:
	Applications(HWND, char*, std::vector<BYTE>, int);
	~Applications();
	HWND ret_hwnd();
	std::string ret_title();
	std::vector<BYTE> ret_icon();
	bool ret_elimina();
	bool operator <(const Applications&);
	bool operator == (const Applications&);
	Json::Value Applications::ToJson() const;
};



