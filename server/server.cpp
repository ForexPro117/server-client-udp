#include <stdio.h>
#include <winsock2.h>
#include <iostream>
#include <thread>
#pragma comment(lib, "ws2_32.lib")

#pragma warning(disable: 4996)

const int size = 100;
SOCKET Connections[size];
int indexCounter = 0;
const int BUF_SIZE = 2048;


void ClientHandler(int index, std::string ip) {
	int iResult;
	int iPlace;
	int msg_size;
	std::string s;
	char recvbuf[BUF_SIZE];
	while (true) {
		if (recv(Connections[index], (char*)&msg_size, sizeof(int), NULL) > 0) {
			s.clear();
			iResult = 0;
			do {
				iPlace = iResult;
				iResult += recv(Connections[index], recvbuf, BUF_SIZE, 0);
				s.insert(iPlace, recvbuf);

			} while (iResult != msg_size);
			s[iResult-1] = '\0';

			for (int i = 0; i < indexCounter; i++) {
				if (i == index || Connections[i] == INVALID_SOCKET) {
					continue;
				}
				send(Connections[i], s.c_str(), msg_size, NULL);
			}
		}
		else {
			::closesocket(Connections[index]);
			Connections[index] = INVALID_SOCKET;
			std::cout << "Client disconnected:" << ip << std::endl;
			return;
		}
	}
}

int main()
{
	WSAData wsaData;
	WORD DLLVersion = MAKEWORD(2, 1);
	if (WSAStartup(DLLVersion, &wsaData) != 0) {
		std::cout << "ERROR" << std::endl;
		exit(1);
	}

	SOCKADDR_IN address; //Структура для хранения адресов интернет протоколов
	int sizeOfAddress = sizeof(address);
	std::cout << "Enter ip address - default 127.0.0.1:";
	char ip[20];
	std::cin >> ip;
	if (strchr(ip, '.') == 0)
		strcpy(ip, "127.0.0.1");
	std::cout << "Enter port - default 1111:";
	int port;
	std::cin >> port;
	if (port == 0 || port > 65535)
		port = 1111;
	address.sin_addr.s_addr = inet_addr(ip); //ip фдрес, указан localhost
	address.sin_port = htons(port); //Порт для идентификации программы
	address.sin_family = AF_INET; //Семейство интернет протоколов

	SOCKET serverListener = socket(AF_INET, SOCK_STREAM, NULL); //Сокет для прослушивания входящих соединений

	bind(serverListener, (SOCKADDR*)&address, sizeof(address)); //Привязка сокету адреса
	std::cout << "Server started:" << inet_ntoa(address.sin_addr)
		<< ":" << port << std::endl;
	listen(serverListener, SOMAXCONN); //Ожидание соединения с клиентом

	SOCKET	newConnection;

	std::thread threads[size];
	for (size_t i = 0; i < size; i++)
	{
		newConnection = accept(serverListener, (SOCKADDR*)&address, &sizeOfAddress); //Сокет для удержания соединения с клиентом
		if (newConnection == 0) //Проверка соединения
		{
			std::cout << "Error, no connection:" << inet_ntoa(address.sin_addr) << std::endl;
		}
		else {
			std::cout << "Client connected:" << inet_ntoa(address.sin_addr) << std::endl;
			Connections[i] = newConnection;
			indexCounter++;
			threads[i] = std::thread(ClientHandler, i, inet_ntoa(address.sin_addr));
		}
	}
	system("pause");

}

