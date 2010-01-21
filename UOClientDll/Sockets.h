#ifndef SOCKETS_INCLUDED
#define SOCKETS_INCLUDED

#include <windows.h>
#include <winsock.h>
#include "ALLOCATION.h"
#include "BinaryTree.h"
#include "Synchronization.h"

#define SOCKET_BACKLOG 10

typedef struct ClientSocketStruct ClientSocket;

typedef void (*ClientsChangeHandler)(ClientSocket * newclient);

typedef struct ServerSocketStruct
{
	SOCKET socket;
	BinaryTree * clients;
	SyncObject * synclock;
	unsigned short port;
	HANDLE ServerThreadHandle;
	unsigned int ServerThreadID;
	ClientsChangeHandler onnewclient;
	ClientsChangeHandler onclientclose;
} ServerSocket;

typedef struct ClientSocketStruct
{
	SOCKET socket;
	SyncObject * synclock;
	struct sockaddr_in addressinfo;
	ServerSocket * onserver;
} ClientSocket;

ClientSocket * CreateClient(char * server,unsigned short port);
ServerSocket * CreateServer(unsigned short port, BOOL loca,ClientsChangeHandler onnewclient,ClientsChangeHandler onclientclose);
BOOL CSend(ClientSocket * toclient,BYTE * tosend,unsigned int length);
BOOL CRecv(ClientSocket * fromclient,BYTE * buffer,unsigned int * length);
BOOL CBroadcast(ServerSocket * fromserver,BYTE * buffer,unsigned int length);
void CloseServer(ServerSocket * toclose);
void CloseClient(ClientSocket * toclose);
int CHasData(ClientSocket * tocheck);


#endif