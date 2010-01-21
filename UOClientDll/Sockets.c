#include "Sockets.h"
#include "W2EDebug.h"

unsigned int sockets_initialized=0;
WSADATA wsaData;

ClientSocket * CreateClient(char * server,unsigned short port)
{
	ClientSocket * newclient;
	struct sockaddr_in clientaddress;
	struct hostent * hserver;

	if(InterlockedExchange(&sockets_initialized,1)==0)
		WSAStartup(0x101,&wsaData);

	newclient=(ClientSocket *)wAlloc(sizeof(ClientSocket));

	if((newclient->socket=socket(AF_INET,SOCK_STREAM,IPPROTO_TCP))==INVALID_SOCKET)
	{
		wFree(newclient);
		return 0;
	}

	clientaddress.sin_family=AF_INET;
	clientaddress.sin_port=htons(port);
	if(isdigit(*server))
		clientaddress.sin_addr.s_addr=inet_addr(server);
	else
	{
		if((hserver=gethostbyname(server))==0)
		{	
			closesocket(newclient->socket);
			wFree(newclient);
			return 0;
		}
		clientaddress.sin_addr=(*((struct in_addr *)((*hserver).h_addr)));
		memset(&(clientaddress.sin_zero),0,8);
	}

	if(connect(newclient->socket,(struct sockaddr *)&clientaddress,sizeof(clientaddress))==SOCKET_ERROR)
	{
		closesocket(newclient->socket);
		wFree(newclient);
		return 0;
	}

	newclient->addressinfo=clientaddress;
	newclient->synclock=CreateSyncObject(INFINITE);
	newclient->onserver=0;

	return newclient;
}

int ClientCompare(void * a,void * b)
{
	unsigned int ua=(unsigned int)a;
	unsigned int ub=(unsigned int)b;

	if(ua<ub)
		return +1;
	else if(ua>ub)
		return -1;
	else
		return 0;
}

DWORD WINAPI ServerThread(LPVOID parameter)
{
	ServerSocket * server=(ServerSocket *)parameter;
	SOCKET newclientsocket;
	struct sockaddr_in addressinfo;
	ClientSocket * newclient;
	unsigned int addressinfosize;

	addressinfosize=sizeof(addressinfo);

	while(1)
	{
		if((newclientsocket=accept(server->socket,(struct sockaddr *)&addressinfo,&addressinfosize))!=INVALID_SOCKET)
		{
			//new client
			newclient=wAlloc(sizeof(ClientSocket));
			newclient->addressinfo=addressinfo;
			newclient->socket=newclientsocket;
			newclient->synclock=CreateSyncObject(INFINITE);
			newclient->onserver=server;
			Lock(server->synclock);
			BT_insert(server->clients,(void *)newclient);
			if(server->onnewclient)
				server->onnewclient(newclient);
			Unlock(server->synclock);
		}
		else
		{
			if(WSAGetLastError()!=WSAECONNRESET)
				break;
		}
	}

	return 0;
}

ServerSocket * CreateServer(unsigned short port, BOOL local,ClientsChangeHandler onnewclient,ClientsChangeHandler onclientclose)
{
	ServerSocket * newserver;
	struct sockaddr_in serveraddr;
	int serveraddrsize;

	serveraddrsize=sizeof(serveraddr);

	if(InterlockedExchange(&sockets_initialized,1)==0)
		WSAStartup(0x101,&wsaData);

	newserver=(ServerSocket *)wAlloc(sizeof(ServerSocket));

	if((newserver->socket=socket(AF_INET,SOCK_STREAM,IPPROTO_TCP))==INVALID_SOCKET)
	{
		wFree(newserver);
		return 0;
	}

	serveraddr.sin_family=AF_INET;
	if(local==FALSE)
		serveraddr.sin_addr.s_addr=INADDR_ANY;//listen on public and local ip
	else
		serveraddr.sin_addr.s_addr=inet_addr("127.0.0.1");//localhost only

	serveraddr.sin_port=htons(port);

	if(bind(newserver->socket,(struct sockaddr *)&serveraddr,sizeof(serveraddr))!=0)
	{
		closesocket(newserver->socket);
		wFree(newserver);
		return 0;
	}

	if(getsockname(newserver->socket,(struct sockaddr *)&serveraddr,&serveraddrsize)==0)
		newserver->port=ntohs(serveraddr.sin_port);
	else
		newserver->port=port;

	if(listen(newserver->socket,SOCKET_BACKLOG)!=0)
	{
		closesocket(newserver->socket);
		wFree(newserver);
		return 0;
	}

	newserver->synclock=CreateSyncObject(INFINITE);
	newserver->clients=BT_create(ClientCompare);
	newserver->onnewclient=onnewclient;
	newserver->onclientclose=onclientclose;

	newserver->ServerThreadHandle=CreateThread(NULL,0,ServerThread,(LPVOID)newserver,0,&(newserver->ServerThreadID));

	return newserver;
}

BOOL CSend(ClientSocket * toclient,BYTE * tosend,unsigned int length)
{
	BOOL success;

	Lock(toclient->synclock);
	success=(send(toclient->socket,tosend,length,0)!=SOCKET_ERROR);
	Unlock(toclient->synclock);

	if(!success)
		CloseClient(toclient);
	
	return success;
}

int CHasData(ClientSocket * tocheck)
{
	fd_set checkset;
	int toreturn;
	TIMEVAL timeout={0,0};

	FD_ZERO(&checkset);
	FD_SET(tocheck->socket,&checkset);

	Lock(tocheck->synclock);
	
	toreturn=select(0,&checkset,0,0,&timeout);

	Unlock(tocheck->synclock);

	if(toreturn==SOCKET_ERROR)
		CloseClient(tocheck);

	return toreturn;
}

BOOL CRecv(ClientSocket * fromclient,BYTE * buffer,unsigned int * length)
{
	unsigned int count;
	Lock(fromclient->synclock);
	count=recv(fromclient->socket,buffer,(*length),0);
	Unlock(fromclient->synclock);
	(*length)=count;
	if(count==0)
	{
		CloseClient(fromclient);
		return FALSE;
	}
	else
		return TRUE;
	
}

BOOL CBroadcast(ServerSocket * fromserver,BYTE * buffer,unsigned int length)
{
	BinaryTreeEnum * btenum;
	ClientSocket * curclient;

	Lock(fromserver->synclock);
	btenum=BT_newenum(fromserver->clients);
	while(curclient=(ClientSocket *)BT_next(btenum))
	{
		if(!CSend(curclient,buffer,length))
		{
			BT_previous(btenum);
			CloseClient(curclient);
		}
	}
	BT_enumdelete(btenum);
	Unlock(fromserver->synclock);

	return TRUE;
}

void CloseClient(ClientSocket * toclose)
{
	Lock(toclose->synclock);
	closesocket(toclose->socket);
	if(toclose->onserver)
	{
		Lock(toclose->onserver->synclock);
		BT_remove(toclose->onserver->clients,(void *)toclose);
		if(toclose->onserver->onclientclose)
			toclose->onserver->onclientclose(toclose);
		Unlock(toclose->onserver->synclock);
	}
	Unlock(toclose->synclock);
	DeleteSyncObject(toclose->synclock);
	wFree(toclose);
	return;
}

void CloseServer(ServerSocket * toclose)
{
	BinaryTreeEnum * btenum;
	ClientSocket * curclient;
	unsigned int exitcode;

	//Lock(toclose->synclock);

	closesocket(toclose->socket);
	while(GetExitCodeThread(toclose->ServerThreadHandle,&exitcode)&&(exitcode==STILL_ACTIVE))
		Sleep(0);

	Lock(toclose->synclock);

	btenum=BT_newenum(toclose->clients);
	while(curclient=(ClientSocket *)BT_next(btenum))
	{
		BT_previous(btenum);
		CloseClient(curclient);
	}
	BT_enumdelete(btenum);
	
	Unlock(toclose->synclock);

	BT_delete(toclose->clients);
	DeleteSyncObject(toclose->synclock);
	wFree(toclose);

	return;
}
