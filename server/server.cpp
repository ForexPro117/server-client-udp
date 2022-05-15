#include <stdio.h>
#include <winsock2.h>
#include <iostream>
#include <thread>
#pragma comment(lib, "ws2_32.lib")

#pragma warning(disable: 4996)

const int size = 8;
SOCKET Connections[size];
int indexCounter = 0;
const int BUF_SIZE = 2048;
const int UDP_BUF_SIZE = 1024;

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
			s[iResult - 1] = '\0';

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

	SOCKADDR_IN serverAddress; //Структура для хранения адресов интернет протоколов
	int sizeOfAddress = sizeof(serverAddress);
	std::cout << "Enter ip address - default 127.0.0.1:";
	char ip[20];
	std::cin >> ip;
	if (strchr(ip, '.') == 0)
		strcpy(ip, "127.0.0.1");
	unsigned short port = 1111;
	serverAddress.sin_addr.s_addr = inet_addr(ip); //ip фдрес, указан localhost
	serverAddress.sin_port = htons(port); //Порт для идентификации программы
	serverAddress.sin_family = AF_INET; //Семейство интернет протоколов	



	// Принятие широковещательного пакета, соденинение с клиентом
	// Создание сокета для принятие датаграмм
	SOCKET recvSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (recvSocket == INVALID_SOCKET) {
		wprintf(L"socket failed with error %d\n", WSAGetLastError());
		return 1;
	}

	// Привязка сокета со всеми доступными интерфейсами и привязка порта
	struct sockaddr_in recvAddr;
	recvAddr.sin_family = AF_INET;
	recvAddr.sin_port = htons(port);
	recvAddr.sin_addr.s_addr = htonl(INADDR_ANY);
	int sizeOfRecvAddr = sizeof(recvAddr);

	int receivedMsg = bind(recvSocket, (SOCKADDR*)&recvAddr, sizeof(recvAddr));
	if (receivedMsg != 0) {
		wprintf(L"bind failed with error %d\n", WSAGetLastError());
		return 1;
	}

	std::cout << "Server started: " << inet_ntoa(serverAddress.sin_addr)
		<< ":" << port << std::endl;
	// Ожидание датаграмм от клиентов
	char recvBuf[UDP_BUF_SIZE];
	struct sockaddr_in clientAddr;
	int clientAddrSize = sizeof(clientAddr);

	SOCKET serverListener = socket(AF_INET, SOCK_STREAM, NULL); //Сокет для прослушивания входящих соединений
	bind(serverListener, (SOCKADDR*)&recvAddr, sizeof(recvAddr)); //Привязка сокету адреса
	listen(serverListener, SOMAXCONN); //Ожидание соединения с клиентом

	wprintf(L"Receiving datagrams...\n");

	SOCKET newConnection;
	std::thread threads[size];
	int i = 0;
	char msg[] = "hello";
	while (true) {
		receivedMsg = recvfrom(recvSocket, recvBuf, UDP_BUF_SIZE, 0, (SOCKADDR*)&clientAddr, &clientAddrSize);
		if (receivedMsg == SOCKET_ERROR) {
			wprintf(L"recvfrom failed with error %d\n", WSAGetLastError());
		}
		// Отправка клиента в индивидуальный поток
		else {
			sendto(recvSocket, msg, sizeof(msg), 0, (SOCKADDR*)&clientAddr, sizeof(clientAddr));
			newConnection = accept(serverListener, (SOCKADDR*)&recvAddr, &sizeOfRecvAddr); //Сокет для удержания соединения с клиентом
			if (newConnection == 0) //Проверка соединения
			{
				std::cout << "Error, no connection: " << inet_ntoa(recvAddr.sin_addr) << std::endl;
			}
			else {
				std::cout << "Client connected: " << inet_ntoa(recvAddr.sin_addr) << std::endl;
				Connections[i] = newConnection;
				indexCounter++;
				threads[i] = std::thread(ClientHandler, i, inet_ntoa(recvAddr.sin_addr));
				i++;
				// Надо ограничить число пользователей до 8
				// и перейти к игровой сессии
				// после чего перезапустить сервер
			}
		}
	}

	// Закрытие сокета после окончания принятия датаграмм
	wprintf(L"Finished receiving. Closing socket.\n");
	receivedMsg = closesocket(recvSocket);
	if (receivedMsg == SOCKET_ERROR) {
		wprintf(L"closesocket failed with error %d\n", WSAGetLastError());
		return 1;
	}

	// Очистка памяти и выход
	wprintf(L"Exiting.\n");
	WSACleanup();
	system("pause");
	return 0;
}


