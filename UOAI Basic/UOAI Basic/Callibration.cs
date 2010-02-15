using System;
using libdisasm;
using Win32API;
using System.Collections.Generic;
using Tools;

namespace UOAIBasic
{
    public class UOCallibration
    {
        public static BinaryTree<uint, long> Callibrations = new BinaryTree<uint, long>();
        
        public enum CallibratedFeatures : uint
        {
            CurrentOffset=0,
            WinMain,
            NetworkObject,
            NetworkObjectVtbl,
            GeneralPurposeHookAddress,//to execute things on the client's thread, a call is made at this address for all the client's loop'd capibilities
            MacroFunction,
            MacroSwitchTable,
            Backup,
            bLoggedIn,
            pLastSkill,
            pLastSpell,
            pLastObjectID,
            pFindItemByID,
            pLastObjectFunction,
            pPlayer,
            oItemID,
            pItemList,
            oitemNext,
            oItemType,
            pLastObjectType,
            pGumpList,
            pFindStatusGump,
            p_SendPacket,
            pSendPacket,
            pBTargCurs,
            pLTargetKind,
            pLTargetTile,
            pLTargetX,
            pLTargetY,
            pLTargetZ,
            pLTargetID,
            pBAlwaysRun,
            pTextOut,
            pAllocator,
            pSysMessage,
            pJournalStart,
            oJournalNext,
            pGumpOpener,
            pStatusGumpConstructor,
            pPartyMembers,
            pShowGump,
            pOriginalSend1,
            pOriginalSend1Location,
            pOriginalSend2,
            pOriginalSend2Location,
            pOriginalSend3,
            pOriginalSend3Location,
            sendcryptpatchpos,
            recvcryptpatchpos,
            pPacketInfo,
            SendCryptPatchPos2,
            pMapInfo,
            pPacketSwitchOffsets,
            pPacketswitchIndices,
            pCastSpell,
            oIsMobile,
            pActualSend,
            pLandTileData,
            pStaticTileData,
            pLoadFromMulFile,
            pLoadFromIndexedFile,
            pCharName,
            pServer,
            Backup2,
            Backup3,
            Backup4,
            pSkillModifiedValues,
            pSkillRealValues,
            pSkillLocks,
            pSkillCaps,
            pHoldingID,
            pShowItemPropertyGump,
            pPropsAndNameGet,
            pPropGumpID,
            pDoGetProperties,
            pDefaultPropertiesString,
            IPTable,
            PortTable,
            IPIndex,
            pFacet,
            pCalculatePath,
            pStartOfPath,
            pBDoPathfind,
            pInvertPath,
            pWalkPath,
            pDefaultOnTarget,
            pOnTarget,
            LAST_FEATURE
        }

        public class CallibrationException : Exception
        {
            public CallibrationException(string message)
                : base(message)
            {
            }
            public CallibrationException(string message, CallibrationException innerexception)
                : base(message, innerexception)
            {
            }
        }

        public static CallibrationException BuildCallibrationException(Stack<string> errorstack)
        {
            string curmsg;

            if (errorstack.Count > 0)
            {
                curmsg = errorstack.Pop();
                if (errorstack.Count > 0)
                    return new CallibrationException(curmsg, BuildCallibrationException(errorstack));
                else
                    return new CallibrationException(curmsg);
            }

            return new CallibrationException("Unknown Callibration Error!");
        }

        public static void Callibrate(ProcessHandler clientprocess)
        {
            uint entrypoint;
            Stack<string> errstack = new Stack<string>();
            asmInstruction curinsn = null;
            asmChunk curchunk = null;

            entrypoint = clientprocess.MainModule.EntryPointAddress;

            if ((entrypoint == 0) || (entrypoint == 0xFFFFFFFF))
                throw new Exception("Failed to obtain entrypoint address!");

            clientprocess.Position = (long)entrypoint;

            if (System.IO.File.Exists("Callibrations.xml"))
                CallibrationFile.Load("Callibrations.xml");
            else//fall back to the embedded callibrations
                CallibrationFile.Load(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("UOAIBasic.Callibrations.xml"));

            if (!ActionList.actionlists["ActionList1"].ExecuteActionList(clientprocess, Callibrations, ref curchunk, ref curinsn, errstack))
                throw BuildCallibrationException(errstack);
        }
    }
}
