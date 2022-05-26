﻿#include <stdio.h>
#include <winsock2.h>
#include <iostream>
#include <thread>
#include <map>
#pragma comment(lib, "ws2_32.lib")

#pragma warning(disable: 4996)
const int size = 8;
SOCKET Connections[size];
SOCKET udpSocket;
const int BUF_SIZE = 2048;
int PlayerCount = 0;


enum Packet
{
	ChatSend,
	PlayersChange,
	MakeAction,
	SendResult
};
enum Actions {
	Rock,
	Scissors,
	Paper
};
enum Result {
	Win,
	Lose,
	Draw
};
std::map<int, Actions> actionMap;
std::map<int, Result> resultMap;

void SendIntToPlayers(Packet packettype, int number)
{
	for (int i = 0; i < size; i++) {
		if (Connections[i] == INVALID_SOCKET || Connections[i] == 0) {
			continue;
		}
		send(Connections[i], (char*)&packettype, sizeof(Packet), NULL);
		send(Connections[i], (char*)&number, sizeof(int), NULL);
	}
}
void UserLeave(int index, std::string ip)
{
	PlayerCount--;
	closesocket(Connections[index]);
	Connections[index] = INVALID_SOCKET;
	actionMap.clear();
	SendIntToPlayers(PlayersChange, PlayerCount);
	SendIntToPlayers(MakeAction, actionMap.size());
	std::cout << "Client disconnected:" << ip << std::endl;
}




void ClientHandler(int index, std::string ip) {

	char password[5];

	recv(Connections[index], (char*)&password, 5, NULL);
	password[4] = '\0';
	std::string s = password;
	if (s != "4321")
	{
		char mes[] = "Access denied - invalid password!";
		std::cout << "Access denied - invalid password:" << ip << "\n";
		send(Connections[index], (char*)&mes, sizeof(mes), NULL);
		UserLeave(index, ip);
		return;
	}
	Packet	packettype = PlayersChange;
	SendIntToPlayers(packettype, PlayerCount);
	SendIntToPlayers(MakeAction, actionMap.size());

	while (true)
	{

		if (recv(Connections[index], (char*)&packettype, sizeof(Packet), NULL) > 0)
		{
			switch (packettype) {
			case ChatSend:

				break;

			case MakeAction:
				Actions action;
				recv(Connections[index], (char*)&action, sizeof(Actions), NULL);
				actionMap[index] = action;
				if (actionMap.size() > 1 && actionMap.size() == PlayerCount)
				{
					int rock=0, scissors=0, paper=0;

					for (int i = 0; i < size; i++) {
						if (Connections[i] == INVALID_SOCKET || Connections[i] == 0) {
							continue;
						}
						switch (actionMap[i])
						{
						case Actions::Rock:
							rock++;
							break;
						case Actions::Scissors:
							scissors++;
							break;
						case Actions::Paper:
							paper++;
							break;
						}
					}
					for (int i = 0; i < size; i++) {
						if (Connections[i] == INVALID_SOCKET || Connections[i] == 0) {
							continue;
						}

						
						switch (actionMap[i])//тут проверяю победил или нет
						{
						case Actions::Rock:
							if ((scissors > 0 && paper > 0)||(scissors==0&&paper==0))
								resultMap[i] = Result::Draw;
							else if (scissors > 0 && paper == 0)
								resultMap[i] = Result::Win;
							else resultMap[i] = Result::Lose;
							break;
						case Actions::Scissors:
							if ((rock > 0 && paper > 0) || (rock == 0 && paper == 0))
								resultMap[i] = Result::Draw;
							else if (paper > 0 && rock == 0)
								resultMap[i] = Result::Win;
							else resultMap[i] = Result::Lose;

							break;
						case Actions::Paper:
							if ((scissors > 0 && rock > 0) || (scissors == 0 && rock == 0))
								resultMap[i] = Result::Draw;
							else if (rock > 0 && scissors == 0)
								resultMap[i] = Result::Win;
							else resultMap[i] = Result::Lose;

							break;
						}
					}
					packettype = SendResult;
					for (int i = 0; i < size; i++) {
						if (Connections[i] == INVALID_SOCKET || Connections[i] == 0) {
							continue;
						}
						send(Connections[i], (char*)&packettype, sizeof(Packet), NULL);
						send(Connections[i], (char*)&resultMap[i], sizeof(Result), NULL);
					}

					resultMap.clear();
					actionMap.clear();
				}
				SendIntToPlayers(MakeAction, actionMap.size());
				break;
			default:
				std::cout << "Incorrect action:" << ip << "\n";
				UserLeave(index, ip);
				return;
			}

		}
		else {
			UserLeave(index, ip);
			return;
		}
	}

}



void UDPController() {
	SOCKADDR_IN clientAddress; //Структура для хранения адресов интернет протоколов
	int sizeOfAddress = sizeof(clientAddress);
	int msg;
	int receivedMsg;

	while (true) {

		receivedMsg = recvfrom(udpSocket, (char*)&msg, sizeof(int), 0, (SOCKADDR*)&clientAddress, &sizeOfAddress);
		if (receivedMsg == SOCKET_ERROR) {
			wprintf(L"recvfrom failed with error %d\n", WSAGetLastError());
		}
		else {
			sendto(udpSocket, (char*)&PlayerCount, sizeof(int), 0, (SOCKADDR*)&clientAddress, sizeOfAddress);
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
	//std::cout << "Enter port - default 1111:";
	int port;
	/*std::cin >> port;
	if (port == 0 || port > 65535)*/
	port = 1111;
	address.sin_addr.s_addr = INADDR_ANY; //ip фдрес, указан localhost
	address.sin_port = htons(port); //Порт для идентификации программы
	address.sin_family = AF_INET; //Семейство интернет протоколов

	udpSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (udpSocket == INVALID_SOCKET) {
		wprintf(L"socket failed with error %d\n", WSAGetLastError());
		return 1;
	}

	int receivedMsg = bind(udpSocket, (SOCKADDR*)&address, sizeOfAddress);
	if (receivedMsg != 0) {
		wprintf(L"bind failed with error %d\n", WSAGetLastError());
		return 1;
	}

	std::thread udpHandler(UDPController);

	SOCKET serverListener = socket(AF_INET, SOCK_STREAM, NULL); //Сокет для прослушивания входящих соединений
	bind(serverListener, (SOCKADDR*)&address, sizeof(address)); //Привязка сокету адреса

	std::cout << "Server started - port:" << port << std::endl;
	listen(serverListener, SOMAXCONN); //Ожидание соединения с клиентом

	SOCKET	newConnection;
	std::thread threads[100];

	int msg_size;

	int position = 0;
	int threadsCounter = 0;
	while (true)
	{
		newConnection = accept(serverListener, (SOCKADDR*)&address, &sizeOfAddress); //Сокет для удержания соединения с клиентом


		for (int i = 0; i < size; i++)
		{
			if (Connections[i] == INVALID_SOCKET || Connections[i] == 0)
			{
				position = i;
				break;
			}
			position = size;
		}
		if (newConnection == 0 || position == size) //Проверка соединения
		{
			std::cout << "Error, no connection:" << inet_ntoa(address.sin_addr) << std::endl;
		}
		else {

			std::cout << "Client connected:" << inet_ntoa(address.sin_addr)
				<< " position:" << position + 1 << "/" << size << std::endl;
			Connections[position] = newConnection;

			PlayerCount++;
			threads[threadsCounter] = std::thread(ClientHandler, position, inet_ntoa(address.sin_addr));
			threadsCounter++;
			position++;



		}
	}
	WSACleanup();
	system("pause");
	return 0;
}


