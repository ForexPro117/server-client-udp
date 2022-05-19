﻿#include <stdio.h>
#include <winsock2.h>
#include <iostream>
#include <thread>
#include <nlohmann/json.hpp>
#pragma comment(lib, "ws2_32.lib")

#pragma warning(disable: 4996)

const int size = 8;
SOCKET Connections[size];
SOCKET udpSocket;
const int BUF_SIZE = 2048;
int PlayerCount = 0;
char nicnames[size][16];

enum Packet
{
	P_UserListChange,
	P_UserMakeLeader,
	P_UserListGet,
	P_ChatSend
};


void UserLeave(int index, std::string ip)
{
	PlayerCount--;
	Packet	packettype = P_UserListChange;
	closesocket(Connections[index]);
	Connections[index] = INVALID_SOCKET;
	strcpy(nicnames[index], "null");
	nlohmann::json json{};
	json = nicnames;
	for (int i = 0; i < size; i++) {
		if (i == index || Connections[i] == INVALID_SOCKET) {
			continue;
		}
		send(Connections[i], (char*)&packettype, sizeof(Packet), NULL);
		send(Connections[i], json.dump().c_str(), json.dump().size(), NULL);
	}
	std::cout << "Client disconnected:" << ip << std::endl;
}




void ClientHandler(int index, std::string ip) {
	
	Packet	packettype = P_UserListChange;
	nlohmann::json json{};
	json = nicnames;
	for (int i = 0; i < size; i++) {
		if (Connections[i] == INVALID_SOCKET) {
			continue;
		}
		send(Connections[i], (char*)&packettype, sizeof(Packet), NULL);
		send(Connections[i], json.dump().c_str(), json.dump().size(), NULL);
	}
	
	while (true)
	{

		if (recv(Connections[index], (char*)&packettype, sizeof(Packet), NULL) > 0)
		{
			switch (packettype) {
			case P_UserListChange:
			{

			}
			break;
			case P_UserListGet:
			{
				
			}
			break;
			case P_ChatSend:
			{
				int iResult;
				int iPlace;
				int msg_size;
				std::string s;
				char recvbuf[BUF_SIZE];
				if (recv(Connections[index], (char*)&msg_size, sizeof(int), NULL) > 0) {
					s.clear();
					iResult = 0;
					do {
						iPlace = iResult;
						iResult += recv(Connections[index], recvbuf, BUF_SIZE, 0);
						s.insert(iPlace, recvbuf);

					} while (iResult != msg_size);
					s[iResult - 1] = '\0';

					for (int i = 0; i < size; i++) {
						if (i == index || Connections[i] == INVALID_SOCKET) {
							continue;
						}
						packettype = P_ChatSend;
						send(Connections[i], (char*)&packettype, sizeof(Packet), NULL);
						send(Connections[i], s.c_str(), msg_size, NULL);
					}
				}
				else {
					UserLeave(index, ip);
					return;
				}

			}
			break;
			
			default:
				break;
			}
		
		}
		else {
			UserLeave(index,ip);
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
	for (int i = 0; i < size; i++)
		strcpy(nicnames[i], "null");

	WSAData wsaData;
	WORD DLLVersion = MAKEWORD(2, 1);
	if (WSAStartup(DLLVersion, &wsaData) != 0) {
		std::cout << "ERROR" << std::endl;
		exit(1);
	}

	SOCKADDR_IN address; //Структура для хранения адресов интернет протоколов
	int sizeOfAddress = sizeof(address);
	/*std::cout << "Enter port - default 1111:";
	int port;
	std::cin >> port;
	if (port == 0 || port > 65535)*/
	int	port = 1111;
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

	std::cout << "Server started:" << inet_ntoa(address.sin_addr)
		<< ":" << port << std::endl;
	listen(serverListener, SOMAXCONN); //Ожидание соединения с клиентом

	SOCKET	newConnection;
	std::thread threads[size];
	
	int msg_size;

	for (int i = 0; i < size; i++)
	{
		newConnection = accept(serverListener, (SOCKADDR*)&address, &sizeOfAddress); //Сокет для удержания соединения с клиентом
		if (newConnection == 0) //Проверка соединения
		{
			std::cout << "Error, no connection:" << inet_ntoa(address.sin_addr) << std::endl;
		}
		else {

			std::cout << "Client connected:" << inet_ntoa(address.sin_addr) << std::endl;
			Connections[i] = newConnection;


			recv(Connections[i], (char*)&msg_size, sizeof(int), NULL);
			recv(Connections[i],nicnames[i], msg_size, 0);

			PlayerCount++;
			threads[i] = std::thread(ClientHandler, i, inet_ntoa(address.sin_addr));
		}
	}
	WSACleanup();
	system("pause");
	return 0;
}

