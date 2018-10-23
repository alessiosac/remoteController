#include "Function.h"

struct ICONDIRENTRY
{
	UCHAR nWidth;
	UCHAR nHeight;
	UCHAR nNumColorsInPalette; // 0 if no palette
	UCHAR nReserved; // should be 0
	WORD nNumColorPlanes; // 0 or 1
	WORD nBitsPerPixel;
	ULONG nDataLength; // length in bytes
	ULONG nOffset; // offset of BMP or PNG data from beginning of file
};

/*
std::vector<BYTE> SaveIcon(HICON hIcon) {
	std::vector<BYTE> data;

	// Create the IPicture intrface
	PICTDESC desc = { sizeof(PICTDESC) };
	desc.picType = PICTYPE_ICON;
	desc.icon.hicon = hIcon;
	IPicture* pPicture = 0;
	HRESULT hr = OleCreatePictureIndirect(&desc, IID_IPicture, FALSE, (void**)&pPicture);
	if (FAILED(hr)) return data;

	// Create a stream and save the image
	IStream* pStream = 0;
	CreateStreamOnHGlobal(0, TRUE, &pStream);
	LONG cbSize = 0;
	hr = pPicture->SaveAsFile(pStream, TRUE, &cbSize);

	// Write the stream content to the file
	std::vector<BYTE> icon(cbSize);
	if (!FAILED(hr)) {
		HGLOBAL hBuf = 0;
		GetHGlobalFromStream(pStream, &hBuf);
		void* buffer = GlobalLock(hBuf);
		memcpy(&icon[0], buffer, cbSize);
		GlobalUnlock(buffer);
	}

	// Cleanup
	pStream->Release();
	pPicture->Release();
	return icon;
}*/

class CGdiHandle
{
public:
	CGdiHandle(HGDIOBJ handle) : m_handle(handle) {};
	~CGdiHandle() { DeleteObject(m_handle); };
private:
	HGDIOBJ m_handle;
};

std::vector<BYTE> SaveIcon(HICON hIcon, int nColorBits) 
{
	std::vector<BYTE> data;
	// controllo la grandezza della struttura, deve essere uguale a 12
	if (offsetof(ICONDIRENTRY, nOffset) != 12)
	{
		return data;
	}
	//Definisce una classe di oggetti di contesto di dispositivo.
	CDC dc;
	dc.Attach(::GetDC(NULL)); // assicura che DC sia rilasciato al termine della funzione

	// Open Cfile ,classe base per le classi file di Microsoft Foundation Class
	CFile file;
	char* path("icon_tmp.ico");
	if (!file.Open(path, CFile::modeWrite | CFile::modeCreate))
	{
		return data;
	}

	// Scrivo l header:
	UCHAR icoHeader[6] = { 0, 0, 1, 0, 1, 0 }; // ICO file con 1 immagine
	file.Write(icoHeader, sizeof(icoHeader));

	// prendo le informazioni dell icona:
	ICONINFO iconInfo;
	GetIconInfo(hIcon, &iconInfo);
	CGdiHandle handle1(iconInfo.hbmColor), handle2(iconInfo.hbmMask); // libera la bitmap quando finisce la funzione
	BITMAPINFO bmInfo = { 0 };
	bmInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bmInfo.bmiHeader.biBitCount = 0;    // non prende la tabella dei colori     
	//La funzione GetDIBits recupera i bit della bitmap specificati e li copia in un buffer bminfo utilizzando il formato specificato.
	if (!GetDIBits(dc, iconInfo.hbmColor, 0, 0, NULL, &bmInfo, DIB_RGB_COLORS))
	{
		file.Close();
		if (remove(path) != 0)
			printf("Error deleting file");
		return data;
	}

	//Allocare la dimensione della bitmap più lo spazio per la tabella dei colori:
	int nBmInfoSize = sizeof(BITMAPINFOHEADER);
	if (nColorBits < 24)
	{
		nBmInfoSize += sizeof(RGBQUAD) * (int)(1 << nColorBits);
	}

	CAutoVectorPtr<UCHAR> bitmapInfo;
	bitmapInfo.Allocate(nBmInfoSize);
	BITMAPINFO* pBmInfo = (BITMAPINFO*)(UCHAR*)bitmapInfo;
	memcpy(pBmInfo, &bmInfo, sizeof(BITMAPINFOHEADER));

	// prendo i dati della bitmap:
	ASSERT(bmInfo.bmiHeader.biSizeImage != 0);
	CAutoVectorPtr<UCHAR> bits;
	bits.Allocate(bmInfo.bmiHeader.biSizeImage);
	pBmInfo->bmiHeader.biBitCount = nColorBits;
	pBmInfo->bmiHeader.biCompression = BI_RGB;
	if (!GetDIBits(dc, iconInfo.hbmColor, 0, bmInfo.bmiHeader.biHeight, (UCHAR*)bits, pBmInfo, DIB_RGB_COLORS))
	{
		file.Close();
		if (remove(path) != 0)
			printf("Error deleting file");
		return data;
	}

	// prendo la maschera dei dati:
	BITMAPINFO maskInfo = { 0 };
	maskInfo.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	maskInfo.bmiHeader.biBitCount = 0;  // non prendo la tavola dei colori   
	if (!GetDIBits(dc, iconInfo.hbmMask, 0, 0, NULL, &maskInfo, DIB_RGB_COLORS))
	{
		file.Close();
		if (remove(path) != 0)
			printf("Error deleting file");
		return data;
	}
	ASSERT(maskInfo.bmiHeader.biBitCount == 1);
	CAutoVectorPtr<UCHAR> maskBits;
	maskBits.Allocate(maskInfo.bmiHeader.biSizeImage);
	CAutoVectorPtr<UCHAR> maskInfoBytes;
	maskInfoBytes.Allocate(sizeof(BITMAPINFO) + 2 * sizeof(RGBQUAD));
	BITMAPINFO* pMaskInfo = (BITMAPINFO*)(UCHAR*)maskInfoBytes;
	memcpy(pMaskInfo, &maskInfo, sizeof(maskInfo));
	if (!GetDIBits(dc, iconInfo.hbmMask, 0, maskInfo.bmiHeader.biHeight, (UCHAR*)maskBits, pMaskInfo, DIB_RGB_COLORS))
	{
		file.Close();
		if (remove(path) != 0)
			printf("Error deleting file");
		return data;
	}

	// creo la struttura con i dati corretti:
	ICONDIRENTRY dir;
	dir.nWidth = (UCHAR)pBmInfo->bmiHeader.biWidth;
	dir.nHeight = (UCHAR)pBmInfo->bmiHeader.biHeight;
	dir.nNumColorsInPalette = (nColorBits == 4 ? 16 : 0);
	dir.nReserved = 0;
	dir.nNumColorPlanes = 0;
	dir.nBitsPerPixel = pBmInfo->bmiHeader.biBitCount;
	dir.nDataLength = pBmInfo->bmiHeader.biSizeImage + pMaskInfo->bmiHeader.biSizeImage + nBmInfoSize;
	dir.nOffset = sizeof(dir) + sizeof(icoHeader);
	file.Write(&dir, sizeof(dir));

	// scrivo l header DIB  (includendo i colori):
	int nBitsSize = pBmInfo->bmiHeader.biSizeImage;
	pBmInfo->bmiHeader.biHeight *= 2; // poiche l header e sia per la maschera , che per i dati
	pBmInfo->bmiHeader.biCompression = 0;
	pBmInfo->bmiHeader.biSizeImage += pMaskInfo->bmiHeader.biSizeImage; // poiche l header e sia per la maschera , che per i dati
	file.Write(&pBmInfo->bmiHeader, nBmInfoSize);

	// scrivo i dati dell immagine:
	file.Write((UCHAR*)bits, nBitsSize);

	// scrivo la maschera:
	file.Write((UCHAR*)maskBits, pMaskInfo->bmiHeader.biSizeImage);

	file.Close();

	//copio l icona nel vettore data
	readFile(path,data);
	if (remove(path) != 0)
		printf("Error deleting file");
	return data;
}

void readFile(const char* filename, std::vector<BYTE>& data)
{
	//apro il file , scrivendo in binario
	std::ifstream input(filename, std::ios::binary);
	// copio i dati nel vettore
	std::vector<BYTE> vec((std::istreambuf_iterator<char>(input)),(std::istreambuf_iterator<char>()));
	data = vec;
	return;
} 

void crea_json(std::list<Applications> list_change, std::string name, HWND hwnd, Store&s) {
	Value program_json(objectValue), programs_json(arrayValue);
	//se nome="" allora devo inviare la lista, altrimenti invio il nome dell applicazione col focus
	if (name == "") {
		for (std::list<Applications>::iterator it = list_change.begin(); it != list_change.end(); it++) {
			programs_json.append(it->ToJson());
		}
		program_json["list"] = programs_json;
		program_json["nameF"] = name;
	}
	else {
		program_json["list"] = "";
		program_json["nameF"] = name;
		program_json["handle"]= (int)hwnd;
	}
	Json::FastWriter fastWriter;
	std::string output = fastWriter.write(program_json);
	s.Send_json(output);
	return;
};


bool elimina(Applications &a) {
	return a.ret_elimina();
}

void recv_input(Store& s) {
	Value parsedString;
	Reader reader;
	int n_tasti;
	std::string nameApplication;
	HWND focus = NULL;
	while (s.ret_start()) {		
		//leggo la grandezza del dato che ricevo
		int read=0;
		if (s.recv_n(4, (char*)&read, 4, false) == -1) { // flag false-> converto in int
			return;
		}
		//leggo il dato da ricevere
		read = ntohl(read);
		char *buffer=new char[read+1];
		if (s.recv_n(read, buffer, read, true) == -1) {// flag true-> leggo caratteri e termino la stringa con /0
			delete[]buffer;
			return;
		}
		//decodifico il json
		bool parsingSuccessfull = reader.parse(buffer, parsedString);
		if (parsingSuccessfull) {
			nameApplication = parsedString["Application"].asString();
			//controllo se il client richiede la chiusura
			if(nameApplication=="Client-Closed"){
				std::cout << "Chiusura richiesta dal Client"<< std::endl;
				s.error();
				return;
			}
			//controllo se l applicazione è in focus,in caso contrario mando Error
			char name[4096];
			GetWindowText(GetForegroundWindow(), name, sizeof(name));
			std::string title(name);
			if(nameApplication!=title){
				//error
				std::list<Applications> tmp;
				crea_json(tmp, "Error",NULL, s);
			}else{
				//combinazione di tasti
				n_tasti = parsedString["Tasti"].asInt();
				INPUT *input = new INPUT[n_tasti * 2];
				//per azzerare tutti i campi della struttura input
				memset(input, 0, sizeof(INPUT)*n_tasti * 2);
				int i = 0;
				//preparo la struttura input per mandare i comandi
				for (const Json::Value& input_cmd : parsedString["Lista"])  // iterate over "books"
				{
					input[i].type = INPUT_KEYBOARD;
					input[i].ki.wVk = input_cmd["vKey"].asInt();
					input[i].ki.wScan = input_cmd["scanCode"].asInt();
					i++;
				}
				for (i = 0; i<n_tasti; i++) {
					input[i + n_tasti].type = INPUT_KEYBOARD;
					input[i + n_tasti].ki.wVk = input[i].ki.wVk;
					input[i + n_tasti].ki.wScan = input[i].ki.wScan;
					input[i + n_tasti].ki.dwFlags = KEYEVENTF_KEYUP;
				}
				//invio i comandi all applicazione col focus ed elimino la struttura
				SendInput(n_tasti * 2, input, sizeof(INPUT));
				delete[]input;
			}
		}
		delete[]buffer;
	}
	return;
}