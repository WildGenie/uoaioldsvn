#ifndef UOMLCALLIBRATIONS_INCLUDED
#define UOMLCALLIBRATIONS_INCLUDED

#include "ALLOCATION.h"
#include "ASMParser.h"
#include "DebuggingTools.h"

typedef struct
{
	/*unsigned int (*pSendHook)(BYTE * tosend);
	void (__stdcall * pHandlePacketHook)(BYTE * received);
	
	unsigned int (*pOriginalSend)(BYTE * tosend);
	void (__stdcall * pOriginalHandlePacket)(BYTE * received);
	
	unsigned int pPacketInfo;//packet sizes
	
	void (__stdcall * pOriginalSendb)(unsigned int para,unsigned int parb);
	void (__stdcall * pSendbHook)(unsigned int para,unsigned int parb);

	void (__stdcall * pOriginalSendc)(unsigned int para);
	void (__stdcall * pSendcHook)(unsigned int para);
	
	void (__stdcall * OriginalConnLossHandler)(unsigned int par);
	void (__stdcall * ConnLossHook)(unsigned int par);*/

	unsigned int pSendHook;
	unsigned int pHandlePacketHook;
	
	unsigned int (* pOriginalSend)(BYTE * tosend);
	void (__stdcall * pOriginalHandlePacket)(BYTE * received);
	
	unsigned int pPacketInfo;//packet sizes
	
	unsigned int pOriginalSendb;
	unsigned int pSendbHook;

	unsigned int pOriginalSendc;
	unsigned int pSendcHook;
	
	void (__stdcall * pOriginalConnLossHandler)(unsigned int par);
	unsigned int pConnLossHook;

	unsigned int pPlayer;

	unsigned int pPlayerStatus;
	
	unsigned int pItemList;
	unsigned int pEventMacro;
	unsigned int pBLoggedIn;
	unsigned int pLastSkill;
	unsigned int pLastSpell;
	unsigned int pLastObject;
	unsigned int pLTargetID;
	unsigned int pLTargetKind;
	unsigned int pLTargetX;
	unsigned int pLTargetY;
	unsigned int pLTargetZ;
	unsigned int pLTargetTile;
	unsigned int pSockObject;

	unsigned int pAlwaysRun;
	unsigned int pBTargCurs;
	unsigned int pFindItemByID;
	unsigned int pLastObjectType;
	unsigned int pLastObjectFunc;
	unsigned int pLoginCrypt;
	unsigned int pSend;
	unsigned int pRecv;	
	unsigned int pCalculatePath;
	unsigned int pStartOfPath;
	unsigned int pBDoPathfind;
	unsigned int pWalkPath;
	unsigned int pInvertPath;

	unsigned int pGumpList;
	unsigned int pFindStatusGump;

	unsigned int pTextOut;

	unsigned int pJournalStart;

	unsigned int pGumpOpener;

	unsigned int pOnTarget;
	unsigned int pDefaultOnTarget;

	unsigned int pSocketObjectVtbl;

	unsigned int pWinMain;

	unsigned int pPatchPos2;
	
	unsigned int pPacketHandlerFunc;

	unsigned int pFacet;

	unsigned int pPatchPos3;
	unsigned int pPatchPos4;

	unsigned int pHoldingID;

	unsigned int pShowItemPropertyGump;

	unsigned int PortTable;
	unsigned int IPTable;
	unsigned int IPIndex;

	unsigned int pSkillCaps;
	unsigned int pSkillLocks;
	unsigned int pSkillRealValues;
	unsigned int pSkillModifiedValues;

	unsigned int pCharName;
	unsigned int pServerName;

	unsigned int pAllocate;//do we need this?, [ccall, 1 parameter: size]
	unsigned int pStatusGumpConstructor;//[stdthiscall, pThis=allocated unconstructed gump, 2 parameters : mobile * pMobile, boolean IsPlayer]
	unsigned int pShowGump;//[aka AddToGumpList(pParent,boolean) ... the boolean is ignored, client always uses 1 ... stdthiscall... pThis=gump]

	unsigned int pPartyMembers;//array of 10 x struct partymember{ID,char[32] name} (i think... event though name should typically be only 30 bytes)

	unsigned int pLoadFromIndexedFile;//indexed mul file access

	unsigned int pStaticTileData;//lookup pStaticTileData + 40*type {int flags at offset 0, byte weight at offset 4, short animationtype at offset 0xC, byte height at offset 0x12, char[20] name at offset 0x13
	unsigned int pLandTileData;//lookup pLandTileData + 28*type {int flags at offset 0, short tiletype (see art) at offset 4, char[20] name at offset 6}
	
	unsigned int pLoadFromMulFile;//non-indexed mul file access

	unsigned int logincryptpatchpos;
	unsigned int logincryptpatchtarget;

	unsigned int recvcryptpatchpos;
	unsigned int recvcryptpatchtarget;

	unsigned int sendcryptpatchpos;
	unsigned int sendcryptpatchtarget;

	unsigned int pPropGumpID;
	unsigned int pPropsAndNameGet;

	unsigned int pGetName;
	unsigned int pGetProperties;

	unsigned int pPropInitA;
	unsigned int pPropInitB;

	unsigned int itempropgumpduplicate;

	unsigned int pDoGetName;
	unsigned int pDoGetProperties;

	unsigned int pStringObjectInit;
	unsigned int pDefaultNameString;
	unsigned int pDefaultPropertiesString;

	unsigned int pMapInfo;

	//item object fields
	unsigned int oItemX;
	unsigned int oItemY;
	unsigned int oItemZ;
	unsigned int oItemType;
	unsigned int oItemTypeIncrement;
	unsigned int oItemDirection;
	unsigned int oItemID;
	unsigned int oItemContainer;
	unsigned int oItemNext;
	//unsigned int oItemPrevious;
	unsigned int oItemContentsNext;
	unsigned int oItemContentsPrevious;
	unsigned int oItemContents;
	unsigned int oItemGump;
	unsigned int oItemColor;
	unsigned int oItemHighlightColor;
	unsigned int oItemReputation;
	unsigned int oItemStackCount;

	unsigned int oItemFlags;

	//item vtbl entries
	unsigned int oItemIsMobile;

	//multi
	unsigned int oMultiType;
	unsigned int oMultiContents;

	//mobile object fields
	unsigned int oMobileStatus;
	unsigned int oMobileLayers;
	
	unsigned int oMobileWarMode;
	unsigned int oMobileEnemy;
	unsigned int oMobileRunning;
	
	//status gump fields
	unsigned int oStatusName;
	
	//journal fields
	unsigned int oJournalNext;
	unsigned int oJournalPrevious;
	unsigned int oJournalText;
	unsigned int oJournalColor;
	unsigned int oPaperdollTitle;

	//gump vtbl entries
	unsigned int oGumpClosable;
	unsigned int oGumpClose;
	unsigned int oGumpClick;
	unsigned int oGumpFocus;
	unsigned int oGumpWriteChar;

	//gump fields
	unsigned int oGumpID;//0x50
	unsigned int oGumpType;
	unsigned int oGumpNext;
	unsigned int oGumpPrevious;
	unsigned int oGumpSubGumpFirst;
	unsigned int oGumpSubGumpLast;
	unsigned int oGumpX;
	unsigned int oGumpY;
	unsigned int oGumpWidth;
	unsigned int oGumpHeight;
	unsigned int oGumpName;
	unsigned int oGumpItem;
	
	//button gump
	unsigned int oButtonGumpOnClick;

	//input control gump
	unsigned int oGumpText;

	//unicode input control gump
	unsigned int oGumpUnicodeText;

	//generic gump
	unsigned int oGenericGumpID;
	unsigned int GenericGumpType;
	unsigned int oGumpElements;
	unsigned int oGumpSubElements;
	//generic gump element
	unsigned int oGumpElementType;
	unsigned int oGumpElementNext;
	unsigned int oGumpElementID;
	unsigned int oGumpElementSelected;
	unsigned int oGumpElementText;
	unsigned int oGumpElementTooltip;
	unsigned int oGumpElementX;
	unsigned int oGumpElementY;
	unsigned int oGumpElementWidth;
	unsigned int oGumpElementHeight;
	unsigned int oGumpElementReleasedType;
	unsigned int oGumpElementPressedType;
	//generic gump element vtbl
	unsigned int oGumpElementClick;

} CallibrationInfo;

CallibrationInfo * CallibrateUOClient();

#endif