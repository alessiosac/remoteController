#include "Applications.h"



Applications::Applications(HWND hwnd, char* name, std::vector<BYTE> icona, int len)
{
	this->hwnd = hwnd;
	title = name;
	icon.swap(icona);
	elimina = false;
}


Applications::~Applications()
{
}

HWND Applications::ret_hwnd()
{
	return hwnd;
}

std::string Applications::ret_title()
{
	return title;
}


std::vector<BYTE> Applications::ret_icon()
{
	return icon;
}

bool Applications::ret_elimina()
{
	return elimina;
}

bool Applications::operator<(const Applications &a1)
{
	if (this->hwnd > a1.hwnd) {
		return true;
	}
	return false;
}

bool Applications::operator==(const Applications &a1)
{
	if (this->hwnd == a1.hwnd && this->title == a1.title) {
		elimina = true;
		return true;
	}
	return false;
}

Json::Value Applications::ToJson() const {
	Json::Value value(Json::objectValue);
	value["title"] = title;
	value["handle"]= (int)hwnd;
	value["icon"] = base64_encode(&icon[0], icon.size());
	return value;
}