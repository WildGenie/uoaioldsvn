#define BUILDING_DLL
#include "UOMLVersionInfo.h"
#include "DebuggingTools.h"
#include <stdio.h>

unsigned int splitpath(char * tosplit,char * cpath, char * cexe)
{
	unsigned int count;
	unsigned int totallen=strlen(tosplit);
	unsigned int i;

	i=totallen;
	count=0;
	while(i>0)
	{
		if(tosplit[i]=='\\')
			break;
		else
			count++;
		i--;
	}
	if((count>0)&&(count<totallen))
	{
		strncpy(cpath,tosplit,totallen-count+1);
		cpath[totallen-count]='\0';
		strncpy(cexe,tosplit+totallen-count+1,count-1);
		cexe[count-1]='\0';
		strcat(cpath,"\\");
		return TRUE;
	}
	else
		return FALSE;
}

UO2DClientVersion Get2DClientVersion(unsigned int pid)
{
	char * cpath;
	char * cexe;
	VS_FIXEDFILEINFO * vfi;
	UO2DClientVersion ver;

	ver.v1=0;
	ver.v2=0;
	ver.v3=0;
	ver.v4=0;

	cpath=GetExeModuleFilePath(pid);
	cexe=wAlloc(256);
	
	if(splitpath(cpath,cpath,cexe))
	{
		if(vfi=GetClientVersionInfo(cpath,cexe))
		{
			ver.v1=HIWORD(vfi->dwProductVersionMS);
			ver.v2=LOWORD(vfi->dwProductVersionMS);
			ver.v3=HIWORD(vfi->dwProductVersionLS);
			ver.v4=LOWORD(vfi->dwProductVersionLS);
			wFree(vfi);
		}
	}

	wFree(cpath);
	wFree(cexe);

	return ver;
}

UOCLIENTDLLAPI char * GetClientVersion(unsigned int pid)
{
	char * toreturn;

	UO2DClientVersion ver;

	ver=Get2DClientVersion(pid);

	if(ver.v1==0)
		return 0;
	else
	{
		toreturn=wAlloc(256);
		toreturn[0]='\0';
		sprintf(toreturn,"%u.%u.%u.%u",ver.v1,ver.v2,ver.v3,ver.v4);
		return toreturn;
	}
}

VS_FIXEDFILEINFO * GetClientVersionInfo(char * cpath,char * cexe)
{
	void * versiondata;
	unsigned int versionsize;
	VS_FIXEDFILEINFO * versioninfo;
	VS_FIXEDFILEINFO * versioninfocopy;
	unsigned int size;

	char * path=wAlloc(1024);

	path[0]='\0';
	
	strcat(path,cpath);
	strcat(path,cexe);
	
	if(versionsize=GetFileVersionInfoSizeA(path,0))
	{
		versiondata=wAlloc(versionsize);
		if(GetFileVersionInfoA(path,0,versionsize,versiondata))
		{
			if(VerQueryValueA(versiondata,"\\",&(void *)versioninfo,&size))
			{
				if(size==sizeof(VS_FIXEDFILEINFO))
				{
					versioninfocopy=wAlloc(sizeof(VS_FIXEDFILEINFO));
					wCopy(versioninfocopy,versioninfo,sizeof(VS_FIXEDFILEINFO));
					wFree(versiondata);
					wFree(path);
					return versioninfocopy;
				}
			}
		}
		wFree(versiondata);
	}
	wFree(path);
	return 0;
}