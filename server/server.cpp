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
	//WSAData wsaData;
	//WORD DLLVersion = MAKEWORD(2, 1);
	//if (WSAStartup(DLLVersion, &wsaData) != 0) {
	//	std::cout << "ERROR" << std::endl;
	//	exit(1);
	//}

	//SOCKADDR_IN address; //Структура для хранения адресов интернет протоколов
	//int sizeOfAddress = sizeof(address);
	//std::cout << "Enter port - default 1111:";
	//int port;
	//std::cin >> port;
	//if (port == 0 || port > 65535)
	//	port = 1111;
	//address.sin_addr.s_addr = INADDR_ANY; //ip фдрес, указан localhost
	//address.sin_port = htons(port); //Порт для идентификации программы
	//address.sin_family = AF_INET; //Семейство интернет протоколов

	//SOCKET serverListener = socket(AF_INET, SOCK_STREAM, NULL); //Сокет для прослушивания входящих соединений

	//bind(serverListener, (SOCKADDR*)&address, sizeof(address)); //Привязка сокету адреса
	//std::cout << "Server started:" << inet_ntoa(address.sin_addr)
	//	<< ":" << port << std::endl;
	//listen(serverListener, SOMAXCONN); //Ожидание соединения с клиентом

	//SOCKET	newConnection;

	//std::thread threads[size];
	//for (size_t i = 0; i < size; i++)
	//{
	//	newConnection = accept(serverListener, (SOCKADDR*)&address, &sizeOfAddress); //Сокет для удержания соединения с клиентом
	//	if (newConnection == 0) //Проверка соединения
	//	{
	//		std::cout << "Error, no connection:" << inet_ntoa(address.sin_addr) << std::endl;
	//	}
	//	else {
	//		std::cout << "Client connected:" << inet_ntoa(address.sin_addr) << std::endl;
	//		Connections[i] = newConnection;
	//		indexCounter++;
	//		threads[i] = std::thread(ClientHandler, i, inet_ntoa(address.sin_addr));
	//	}
	//}
	/*struct sockaddr_in SenderAddr;
	byte msg[256];
	SOCKET sock = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	bind(sock, (SOCKADDR*)&address, sizeof(address));
	recvfrom(sock, (char*)&msg, sizeof(msg), NULL, (sockaddr*)&SenderAddr, (int*)sizeof(SenderAddr));
	std::cout << msg;*/
	//char msg[256];
	//struct sockaddr_in SenderAddr;
	//int SenderAddrSize = sizeof(SenderAddr);

	//SOCKET sock = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	//if (sock == INVALID_SOCKET) {
	//	wprintf(L"socket failed with error %d\n", WSAGetLastError());
	//	return 1;
	//}

	//int iResult = bind(sock, (SOCKADDR*)&address, sizeof(address));
	//if (iResult != 0) {
	//	wprintf(L"bind failed with error %d\n", WSAGetLastError());
	//	return 1;
	//}
	////-----------------------------------------------
	//// Call the recvfrom function to receive datagrams
	//// on the bound socket.
	//wprintf(L"Receiving datagrams...\n");
	//iResult = recvfrom(sock, (char*)&msg, sizeof(msg), NULL, (sockaddr*)INADDR_BROADCAST, (int*)sizeof(INADDR_BROADCAST));
	//if (iResult == SOCKET_ERROR) {
	//	wprintf(L"recvfrom failed with error %d\n", WSAGetLastError());
	//}

	////-----------------------------------------------
	//// Close the socket when finished receiving datagrams
	//wprintf(L"Finished receiving. Closing socket.\n");
	//iResult = closesocket(sock);
	//if (iResult == SOCKET_ERROR) {
	//	wprintf(L"closesocket failed with error %d\n", WSAGetLastError());
	//	return 1;
	//}

	////-----------------------------------------------
	//// Clean up and exit.
	//wprintf(L"Exiting.\n");

	//system("pause");
	int iResult = 0;

	WSADATA wsaData;

	SOCKET RecvSocket;
	struct sockaddr_in RecvAddr;

	unsigned short Port = 1111;

	char RecvBuf[1024];
	int BufLen = 1024;

	struct sockaddr_in SenderAddr;
	int SenderAddrSize = sizeof(SenderAddr);

	//-----------------------------------------------
	// Initialize Winsock
	iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (iResult != NO_ERROR) {
		wprintf(L"WSAStartup failed with error %d\n", iResult);
		return 1;
	}
	//-----------------------------------------------
	// Create a receiver socket to receive datagrams
	RecvSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (RecvSocket == INVALID_SOCKET) {
		wprintf(L"socket failed with error %d\n", WSAGetLastError());
		return 1;
	}
	//-----------------------------------------------
	// Bind the socket to any address and the specified port.
	RecvAddr.sin_family = AF_INET;
	RecvAddr.sin_port = htons(Port);
	RecvAddr.sin_addr.s_addr = htonl(INADDR_ANY);

	iResult = bind(RecvSocket, (SOCKADDR*)&RecvAddr, sizeof(RecvAddr));
	if (iResult != 0) {
		wprintf(L"bind failed with error %d\n", WSAGetLastError());
		return 1;
	}
	//-----------------------------------------------
	// Call the recvfrom function to receive datagrams
	// on the bound socket.
	wprintf(L"Receiving datagrams...\n");
	iResult = recvfrom(RecvSocket,
		RecvBuf, BufLen, 0, (SOCKADDR*)&SenderAddr, &SenderAddrSize);
	if (iResult == SOCKET_ERROR) {
		wprintf(L"recvfrom failed with error %d\n", WSAGetLastError());
	}
	std::cout << inet_ntoa(SenderAddr.sin_addr);
	//-----------------------------------------------
	// Close the socket when finished receiving datagrams
	wprintf(L"Finished receiving. Closing socket.\n");
	iResult = closesocket(RecvSocket);
	if (iResult == SOCKET_ERROR) {
		wprintf(L"closesocket failed with error %d\n", WSAGetLastError());
		return 1;
	}

	//-----------------------------------------------
	// Clean up and exit.
	wprintf(L"Exiting.\n");
	WSACleanup();
	system("pause");
	return 0;
}

