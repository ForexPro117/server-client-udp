#include <stdio.h>
#include <winsock2.h>
#include <iostream>
#include <thread>
#pragma comment(lib, "ws2_32.lib")

#pragma warning(disable: 4996)
using namespace std;
const int gameClientCount = 256;
SOCKET Connections[gameClientCount];
int clientIndex = 0;
int clientIndexCounter = 0;
const int BUF_SIZE = 2048;
const int UDP_BUF_SIZE = 1024;
int playersLeftBuf = 8;
struct sockaddr_in clientAddr;
int clientAddrSize = sizeof(clientAddr);

SOCKET recvSocket;

string to_string(int param)
{
	string str = "";
	for (str = ""; param; param /= 10)
		str += (char)('0' + param % 10);
	reverse(str.begin(), str.end());
	return str;
}

void ClientHandler(int index, std::string ip) {
	const int maxPlayers = 8;
	
	char recvbuf[BUF_SIZE];
	while (true)
	{
		if (recv(Connections[index], recvbuf, BUF_SIZE, 0) > 0)
		{
			playersLeftBuf--;
			for (int i = 0; i < clientIndex; i++) {
				if (Connections[i] == INVALID_SOCKET) {
					continue;
				}
				send(Connections[i], (char*)&playersLeftBuf, sizeof(int), NULL);
			}
		}
		else
		{
			::closesocket(Connections[index]);
			Connections[index] = INVALID_SOCKET;
			playersLeftBuf++;
			for (int i = 0; i < clientIndex; i++) {
				if (i == index || Connections[i] == INVALID_SOCKET) {
					continue;
				}
				send(Connections[i], (char*)&playersLeftBuf, sizeof(int), NULL);
			}
			std::cout << "Client disconnected:" << ip << std::endl;
			return;
		}
	}
}

void UDPReceiver() {
	std::thread threads[gameClientCount];
	int receivedMsg;
	char msg[] = "hello";
	char recvBuf[UDP_BUF_SIZE];
	while (true) {

		receivedMsg = recvfrom(recvSocket, recvBuf, UDP_BUF_SIZE, 0, (SOCKADDR*)&clientAddr, &clientAddrSize);
		if (receivedMsg == SOCKET_ERROR) {
			wprintf(L"recvfrom failed with error %d\n", WSAGetLastError());
		}
		// Отправка клиента в индивидуальный поток
		else {
			sendto(recvSocket, (char*)&playersLeftBuf, sizeof(int), 0, (SOCKADDR*)&clientAddr, clientAddrSize);
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
	int sizeOfAdrr = sizeof(serverAddress);
	std::cout << "Enter port - default 1111:";
	int port;
	std::cin >> port;
	if (port == 0 || port > 65535)
		port = 1111;
	serverAddress.sin_addr.s_addr = INADDR_ANY; //ip фдрес, указан localhost
	serverAddress.sin_port = htons(port); //Порт для идентификации программы
	serverAddress.sin_family = AF_INET; //Семейство интернет протоколов

	// Принятие широковещательного пакета, соденинение с клиентом
	// Создание сокета для принятие датаграмм
	recvSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (recvSocket == INVALID_SOCKET) {
		wprintf(L"socket failed with error %d\n", WSAGetLastError());
		return 1;
	}
	int broadcast = 1;

	int receivedMsg = bind(recvSocket, (SOCKADDR*)&serverAddress, sizeof(serverAddress));
	if (receivedMsg != 0) {
		wprintf(L"bind failed with error %d\n", WSAGetLastError());
		return 1;
	}

	std::thread thread(UDPReceiver);

	SOCKET serverListener = socket(AF_INET, SOCK_STREAM, NULL); //Сокет для прослушивания входящих соединений
	bind(serverListener, (SOCKADDR*)&serverAddress, sizeof(serverAddress)); //Привязка сокету адреса
	listen(serverListener, SOMAXCONN); //Ожидание соединения с клиентом

	std::cout << "Server started: \n";

	// Ожидание датаграмм от клиентов
	wprintf(L"Receiving datagrams...\n");

	SOCKET newConnection;
	std::thread threads[gameClientCount];
	char msg[] = "hello";
	while (true) {
		if (clientIndex <= 1) {
			newConnection = accept(serverListener, (SOCKADDR*)&serverAddress, &sizeOfAdrr); //Сокет для удержания соединения с клиентом
			if (newConnection == 0) //Проверка соединения
			{
				std::cout << "Error, no connection: " << inet_ntoa(serverAddress.sin_addr) << std::endl;
			}
			else {
				std::cout << "Client connected: " << inet_ntoa(serverAddress.sin_addr) << std::endl;
				Connections[clientIndex] = newConnection;
				threads[clientIndex] =
					std::thread(ClientHandler, clientIndex, inet_ntoa(serverAddress.sin_addr));
				clientIndex++;
				clientIndexCounter++;
			}
		}
		else continue;
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
	return 0;
}


