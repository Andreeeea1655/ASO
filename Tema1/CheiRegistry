//Să se citească toate subcheile unei chei (la alegere) din Registry și să se afișeze la ieșirea standard - HKEY_CURRENT_USER\Software
#include <iostream>
#include <Windows.h>

int main()
{
	HKEY hKey;
	long lResult = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software", 0, KEY_READ, &hKey); //deschide cheia specificată în registry
	if (lResult != ERROR_SUCCES)
	{
		std::cout<< "Eroare la deschidera cheii\n";
		return 1;
	}

	char szName[256];
	DWORD dwIndex=0;
	DWORD dwNameSize;

	std::cout << "Subchei:\n";
	while (true) {
		dwNameSize = sizeof(name);
		lResult = RegEnumKeyEx(hKey, dwIndex, szName, &dwNameSize, NULL, NULL, NULL, NULL);
		if (lResult == ERROR_NO_MORE_ITEMS) break;
		if (lResult == ERROR_SUCCESS) {
			std::cout << "- " << szName << "\n";
		}
		else {
			std::cout << "Eroare la citire\n";
		}
		dwIndex++;
	}
	RegCloseKey(hKey);
	return 0;
}
