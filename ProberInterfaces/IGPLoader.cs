using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProberInterfaces
{
    public static class GPLoaderDef
    {
        public const int FoupCount = 3;
        public const int FTCount = 9;
        public const int CardBufferCount = 22;
        public const int BufferCount = 5;
        public const int StageCount = 12;
    }
    //TYPE ST_Int_PLCToPC :
    //    STRUCT
    //    nState                  : UINT;
    //	nCtrlErrID				: UDINT;
    //	nCtrlState				: UDINT;
    //	nCSTIO					: UDINT;
    //	nRobotState				: UINT;
    //	nRobotCSTPos			: UINT;
    //	nRobotPreAPos			: UINT;
    //	nRobotStagePos			: UINT;
    //	nRobotWaferSlotPos		: UINT;

    //	nCSTWaferCnt			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UDINT;
    //	nCSTWaferState			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF ARRAY[gcn_MinCnt..3] OF UDINT;

    //	nPreAState				: UDINT;

    //	nStageState_1			: UDINT;
    //	nStageState_2			: UDINT;

    //	nLX_State				: UINT;
    //	nLZM_State				: UINT;
    //	nLZS_State				: UINT;
    //	nLW_State				: UINT;
    //	nLT_State				: UINT;
    //	nLUD_State				: UINT;
    //	nLUU_State				: UINT;
    //	nLCC_State				: UINT;

    //	nLX_Pos					: DINT;
    //	nLZM_Pos				: DINT;
    //	nLZS_Pos				: DINT;
    //	nLW_Pos					: DINT;
    //	nLT_Pos					: DINT;
    //	nLUD_Pos				: DINT;
    //	nLUU_Pos				: DINT;
    //	nLCC_Pos				: DINT;

    //	nLX_Velo				: DINT;
    //	nLZM_Velo				: DINT;
    //	nLZS_Velo				: DINT;
    //	nLW_Velo				: DINT;
    //	nLT_Velo				: DINT;
    //	nLUD_Velo				: DINT;
    //	nLUU_Velo				: DINT;
    //	nLCC_Velo				: DINT;
    //END_STRUCT
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class stCDXIn
    {
        ////TYPE ST_Int_PLCToPC :
        //public UInt16 nState;              // : UINT;

        //public UInt16 ui16Dummy1;

        //public UInt32 nCtrlErrID;			//: UDINT;
        //public UInt32 nCtrlState;			//	: UDINT;
        //public UInt32 nCSTIO;				//	: UDINT;
        //public ushort nRobotState;			// : UINT;
        //public ushort nRobotCSTPos;		// : UINT;
        //public ushort nRobotPreAPos;			// : UINT;
        //public ushort nRobotStagePos;		// : UINT;
        //public ushort nRobotWaferSlotPos;      // : UINT;

        //public UInt16 ui16Dummy2;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        //public UInt32[] nCSTWaferCnt;	    // : ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UDINT;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        //public UInt32[] nCSTWaferState;      // : ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF ARRAY[gcn_MinCnt..3] OF UDINT;

        //public UInt32 nPreAState;				// : UDINT;

        //public UInt32 nStageState_1;			// : UDINT;
        //public UInt32 nStageState_2;			// : UDINT;

        //public ushort nLX_State;				// : UINT;
        //public ushort nLZM_State;				// : UINT;
        //public ushort nLZS_State;				// : UINT;
        //public ushort nLW_State;				// : UINT;
        //public ushort nLT_State;				// : UINT;
        //public ushort nLUD_State;				// : UINT;
        //public ushort nLUU_State;				// : UINT;
        //public ushort nLCC_State;				// : UINT;

        //public Int32 nLX_Pos;					// : DINT;
        //public Int32 nLZM_Pos;				    // : DINT;
        //public Int32 nLZS_Pos;				    // : DINT;
        //public Int32 nLW_Pos;				    // : DINT;
        //public Int32 nLT_Pos;					// : DINT;
        //public Int32 nLUD_Pos;				    // : DINT;
        //public Int32 nLUU_Pos;				    // : DINT;
        //public Int32 nLCC_Pos;				    // : DINT;

        //public Int32 nLX_Velo;				    // : DINT;
        //public Int32 nLZM_Velo;				// : DINT;
        //public Int32 nLZS_Velo;				// : DINT;
        //public Int32 nLW_Velo;				    // : DINT;
        //public Int32 nLT_Velo;				    // : DINT;
        //public Int32 nLUD_Velo;				// : DINT;
        //public Int32 nLUU_Velo;				// : DINT;
        //public Int32 nLCC_Velo;                // : DINT;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        //public ushort[] nFC_State;              //: UINT;		

        //public UInt16 ui16Dummy3;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        //public Int32[] nFC_Pos;				    //: DINT;		
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        //public Int32[] nFC_Velo;                //: DINT;		
        #region // v3.1
        //TYPE ST_Int_PLCToPC :
        //STRUCT
        public UInt16 nState;              // : UINT;
        public UInt16 ui16Dummy1;

        public UInt32 nCtrlState;           //	: UDINT;
        //nCSTCtrlState			: UDINT;
        public UInt32 nCSTCtrlState;           //	: UDINT;

        // nLX_DriveErrCode		: UINT;                   
        public UInt16 nLX_DriveErrCode;
        // nLZM_DriveErrCode		: UINT;
        public UInt16 nLZM_DriveErrCode;
        // nLZS_DriveErrCode		: UINT;
        public UInt16 nLZS_DriveErrCode;
        // nLW_DriveErrCode		: UINT;
        public UInt16 nLW_DriveErrCode;

        // nLT_DriveErrCode		: UINT;
        public UInt16 nLT_DriveErrCode;

        // nLUD_DriveErrCode		: UINT;
        public UInt16 nLUD_DriveErrCode;

        // nLUU_DriveErrCode		: UINT;
        public UInt16 nLUU_DriveErrCode;

        // nLCC_DriveErrCode		: UINT;
        public UInt16 nLCC_DriveErrCode;

        // nFoup_DriveErrCode		: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public UInt16[] nFoup_DriveErrCode;
        public UInt16 ui16Dummy2;
        // nInput1_State			: UDINT;
        public UInt32 Input1_State;
        // nInput2_State			: UDINT;
        public UInt32 Input2_State;
        // nInput3_State			: UDINT;
        public UInt32 Input3_State;
        // nInput4_State			: UDINT;
        public UInt32 Input4_State;
        // nOutput1_State			: UDINT;
        public UInt32 Output1_State;
        // nOutput2_State			: UDINT;
        public UInt32 Output2_State;
        // nOutput3_State			: UDINT;
        public UInt32 Output3_State;
        // nRobotState				: UINT;
        public ushort nRobotState;          // : UINT;

        public ushort nSequenceErrorCode;   // : UINT;

        // ToDo: Need data type modification
        // nCST_Pos				: UINT;
        public ushort nCST_Pos;
        // nPreA_Pos				: UINT;
        public ushort nPreA_Pos;
        // nStage_Pos				: UINT;
        public ushort nStage_Pos;
        // nCSTWaferSlot_Pos		: UINT;
        public ushort nCSTWaferSlot_Pos;
        // nLTSlot_Pos				: UINT;
        public ushort nLTSlot_Pos;
        // nFixTray_Pos			: UINT;
        public ushort nFixTray_Pos;
        // nCardBuffer_Pos			: UINT;
        public ushort nCardBuffer_Pos;

        // nCSTState				: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public ushort[] nCSTState;
        public ushort ui16Dummy3;
        // nCSTWafer_Cnt			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UDINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public UInt32[] nCSTWafer_Cnt;
        // nCSTWafer_State			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UDINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public UInt32[] nCSTWafer_State;
        // nPreAState				: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public ushort[] nPreAState;
        // nPreA_DWLState			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public ushort[] nPreA_DWLState;
        // nPreA_ErrCode			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public ushort[] nPreA_ErrCode;

        // nArmIndex				: UINT;
        public ushort nArmIndex;				// : UINT;
        // nLX_State				: UINT;
        public ushort nLX_State;				// : UINT;
        // nLZM_State				: UINT;
        public ushort nLZM_State;				// : UINT;
        // nLZS_State				: UINT;
        public ushort nLZS_State;				// : UINT;
        // nLW_State				: UINT;
        public ushort nLW_State;				// : UINT;
        // nLT_State				: UINT;
        public ushort nLT_State;				// : UINT;
        // nLUD_State				: UINT;
        public ushort nLUD_State;				// : UINT;
        // nLUU_State				: UINT;
        public ushort nLUU_State;				// : UINT;
        // nLCC_State				: UINT;
        public ushort nLCC_State;               // : UINT;

        // nFoup1_State			: UINT;
        public ushort nFoup1_State;               // : UINT;
        // nFoup2_State			: UINT;
        public ushort nFoup2_State;               // : UINT;
        // nFoup3_State			: UINT;
        public ushort nFoup3_State;             // : UINT;

        // nLX_Pos					: DINT;
        // nLZM_Pos				: DINT;
        // nLZS_Pos				: DINT;
        // nLW_Pos					: DINT;
        // nLT_Pos					: DINT;
        // nLUD_Pos				: DINT;
        // nLUU_Pos				: DINT;
        // nLCC_Pos				: DINT;
        // nFoup1_Pos				: DINT;
        // nFoup2_Pos				: DINT;
        // nFoup3_Pos				: DINT;

        // nLX_Velo				: DINT;
        // nLZM_Velo				: DINT;
        // nLZS_Velo				: DINT;
        // nLW_Velo				: DINT;
        // nLT_Velo				: DINT;
        // nLUD_Velo				: DINT;
        // nLUU_Velo				: DINT;
        // nLCC_Velo				: DINT;
        // nFoup1_Velo				: DINT;
        // nFoup2_Velo				: DINT;
        // nFoup3_Velo				: DINT;

        public Int32 nLX_Pos;					// : DINT;
        public Int32 nLZM_Pos;				    // : DINT;
        public Int32 nLZS_Pos;				    // : DINT;
        public Int32 nLW_Pos;				    // : DINT;
        public Int32 nLT_Pos;					// : DINT;
        public Int32 nLUD_Pos;				    // : DINT;
        public Int32 nLUU_Pos;				    // : DINT;
        public Int32 nLCC_Pos;                  // : DINT;

        // nFoup1_Pos				: DINT;
        public Int32 nFoup1_Pos;
        // nFoup2_Pos				: DINT;
        public Int32 nFoup2_Pos;
        // nFoup3_Pos				: DINT;
        public Int32 nFoup3_Pos;

        public Int32 nLX_Velo;				    // : DINT;
        public Int32 nLZM_Velo;				// : DINT;
        public Int32 nLZS_Velo;				// : DINT;
        public Int32 nLW_Velo;				    // : DINT;
        public Int32 nLT_Velo;				    // : DINT;
        public Int32 nLUD_Velo;				// : DINT;
        public Int32 nLUU_Velo;				// : DINT;
        public Int32 nLCC_Velo;                // : DINT;

        // nFoup1_Velo				: DINT;
        public Int32 nFoup1_Velo;
        // nFoup2_Velo				: DINT;
        public Int32 nFoup2_Velo;
        // nFoup3_Velo				: DINT;
        public Int32 nFoup3_Velo;

        //END_STRUCT
        //END_TYPE
        #endregion

    }

    //TYPE ST_Int_PCtoPLC :
    //    STRUCT
    //        nMode                   : UINT;
    //	nCtrlCmd				: UDINT;

    //	nManuRobotCmd			: UINT;
    //	nManuCSTPos				: UINT;
    //	nManuPreAPos			: UINT;
    //	nManuStagePos			: UINT;
    //	nManuWaferSlotPos		: UINT;

    //	nManuPreAProcess		: UINT;
    //	nManuStageProcess		: UINT;

    //	nLX_JogCmd				: UINT;
    //	nLZ_JogCmd				: UINT;
    //	nLW_JogCmd				: UINT;
    //	nLT_JogCmd				: UINT;
    //	nLUD_JogCmd				: UINT;
    //	nLUU_JogCmd				: UINT;
    //	nLCC_JogCmd				: UINT;

    //	nLX_JogPos				: DINT;
    //	nLZ_JogPos				: DINT;
    //	nLW_JogPos				: DINT;
    //	nLT_JogPos				: DINT;
    //	nLUD_JogPos				: DINT;
    //	nLUU_JogPos				: DINT;
    //	nLCC_JogPos				: DINT;
    //END_STRUCT
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class stCDXOut
    {
        #region // v2.1
        ////TYPE ST_Int_PCtoPLC :
        //public ushort nMode;                   //: UINT;
        //public UInt32 nCtrlCmd;                  //: UDINT;

        //public ushort nManuRobotCmd;           //: UINT;
        //public ushort nManuCSTPos;             //: UINT;
        //public ushort nManuPreAPos;            //: UINT;
        //public ushort nManuStagePos;           //: UINT;
        //public ushort nManuWaferSlotPos;       //: UINT;

        //public ushort nManuPreAProcess;        //: UINT;
        //public ushort nManuStageProcess;       //: UINT;

        //public ushort nLX_JogCmd;              //: UINT;
        //public ushort nLZ_JogCmd;              //: UINT;
        //public ushort nLW_JogCmd;              //: UINT;
        //public ushort nLT_JogCmd;              //: UINT;
        //public ushort nLUD_JogCmd;             //: UINT;
        //public ushort nLUU_JogCmd;             //: UINT;
        //public ushort nLCC_JogCmd;             //: UINT;

        //public Int32  nLX_JogPos;                 //: DINT;
        //public Int32  nLZ_JogPos;                 //: DINT;
        //public Int32  nLW_JogPos;                 //: DINT;
        //public Int32  nLT_JogPos;                 //: DINT;
        //public Int32  nLUD_JogPos;                //: DINT;
        //public Int32  nLUU_JogPos;                //: DINT;
        //public Int32  nLCC_JogPos;              //: DINT;

        #endregion

        //TYPE ST_Int_PCtoPLC :
        //STRUCT
        //    nMode					: UINT;
        // nCtrlCmd				: UDINT;
        public ushort nMode;                        //: UINT;
        public UInt16 nDummy1;
        public UInt32 nCtrlCmd;                     //: UDINT;
        public UInt32 nCSTCtrlCmd;
        // nRobotCmd				: UINT;
        public ushort nRobotCmd;                    //: UINT;
        public UInt16 nDummy2;
        // nOutputCmd				: UDINT;
        public UInt32 nOutputCmd;                   //: UDINT;

        // nCSTPos					: INT;
        public short nCSTPos;                       //: INT;
        // nPreAPos				: INT;
        public short nPreAPos;                      //: INT;
        // nStagePos				: INT;
        public short nStagePos;                     //: INT;
        // nWaferSlotPos			: INT;
        public short nWaferSlotPos;                 //: INT;
        // nLTSlotPos				: INT;
        public short nLTSlotPos;                    //: INT;
        // nFixTrayPos				: INT;
        public short nFixTrayPos;                   //: INT;
        // nCardBufferPos			: INT;
        public short nCardBufferPos;                //: INT;

        // nPreAProcess			: UINT;
        public ushort nPreAProcess;                 //: UINT;
        // nPreAReset				: UINT;
        public ushort nPreAReset;                   //: UINT;

        // nPreA_DWMCmd			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF UINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public ushort[] nPreA_DWMCmd;
        // nArmIndex				: UINT;
        public ushort nArmIndex;            //: UINT;
        // nLX_JogCmd				: UINT;
        public ushort nLX_JogCmd;            //: UINT;
        // nLZ_JogCmd				: UINT;
        public ushort nLZ_JogCmd;            //: UINT;
        // nLW_JogCmd				: UINT;
        public ushort nLW_JogCmd;            //: UINT;
        // nLT_JogCmd				: UINT;
        public ushort nLT_JogCmd;            //: UINT;
        // nLUD_JogCmd				: UINT;
        public ushort nLUD_JogCmd;            //: UINT;
        // nLUU_JogCmd				: UINT;
        public ushort nLUU_JogCmd;            //: UINT;
        // nLCC_JogCmd				: UINT;
        public ushort nLCC_JogCmd;            //: UINT;
        // nFoup_JogPos			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public ushort[] nFoup_JogCmd;
        public UInt16 nDummy3;

        // nLX_JogPos				: DINT;
        public Int32 nLX_JogPos;            //: DINT;

        // nLZ_JogPos				: DINT;
        public Int32 nLZ_JogPos;            //: DINT;

        // nLW_JogPos				: DINT;
        public Int32 nLW_JogPos;            //: DINT;

        // nLT_JogPos				: DINT;
        public Int32 nLT_JogPos;            //: DINT;

        // nLUD_JogPos				: DINT;
        public Int32 nLUD_JogPos;            //: DINT;

        // nLUU_JogPos				: DINT;
        public Int32 nLUU_JogPos;            //: DINT;

        // nLCC_JogPos				: DINT;
        public Int32 nLCC_JogPos;            //: DINT;

        // nFoup_JogPos			: ARRAY[gcn_MinCnt..gcn_MaxFoupCnt] OF DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public Int32[] nFoup_JogPos;
        //END_STRUCT
        //END_TYPE

    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class stGPLoaderParam
    {
        //        TYPE ST_Int_Params :
        //STRUCT
        //    nLX_HomeOffset					: DINT;
        public Int32 nLX_HomeOffset;
        //	nLZM_HomeOffset					: DINT;
        public Int32 nLZM_HomeOffset;
        //	nLZS_HomeOffset					: DINT;
        public Int32 nLZS_HomeOffset;
        //	nLW_HomeOffset					: DINT;
        public Int32 nLW_HomeOffset;
        //	nLT_HomeOffset					: DINT;
        public Int32 nLT_HomeOffset;
        //	nLUD_HomeOffset					: DINT;
        public Int32 nLUD_HomeOffset;
        //	nLUU_HomeOffset					: DINT;
        public Int32 nLUU_HomeOffset;
        //	nLCC_HomeOffset					: DINT;
        public Int32 nLCC_HomeOffset;
        //	nFoup_1_HomeOffset				: DINT;
        public Int32 nFoup_1_HomeOffset;
        //	nFoup_2_HomeOffset				: DINT;
        public Int32 nFoup_2_HomeOffset;
        //	nFoup_3_HomeOffset				: DINT;
        public Int32 nFoup_3_HomeOffset;

        //	nLX_HomePos						: DINT;
        public Int32 nLX_HomePos;
        //	nLZM_HomePos					: DINT;
        public Int32 nLZM_HomePos;
        //	nLZS_HomePos					: DINT;
        public Int32 nLZS_HomePos;
        //	nLW_HomePos						: DINT;
        public Int32 nLW_HomePos;
        //	nLT_HomePos						: DINT;
        public Int32 nLT_HomePos;
        //	nLUD_HomePos					: DINT;
        public Int32 nLUD_HomePos;
        //	nLUU_HomePos					: DINT;
        public Int32 nLUU_HomePos;
        //	nLCC_HomePos					: DINT;
        public Int32 nLCC_HomePos;
        //	nFoup_1_HomePos					: DINT;
        public Int32 nFoup_1_HomePos;
        //	nFoup_2_HomePos					: DINT;
        public Int32 nFoup_2_HomePos;
        //	nFoup_3_HomePos					: DINT;
        public Int32 nFoup_3_HomePos;

        //	nLX_HomeVelo					: DINT;
        public Int32 nLX_HomeVelo;
        //	nLZ_HomeVelo					: DINT;
        public Int32 nLZ_HomeVelo;
        //	nLW_HomeVelo					: DINT;
        public Int32 nLW_HomeVelo;
        //	nLT_HomeVelo					: DINT;
        public Int32 nLT_HomeVelo;
        //	nLU_HomeVelo					: DINT;
        public Int32 nLU_HomeVelo;
        //	nLCC_HomeVelo					: DINT;
        public Int32 nLCC_HomeVelo;
        //	nFoup_HomeVelo					: DINT;
        public Int32 nFoup_HomeVelo;


        //	nLX_HomeImpulsVelo				: DINT;
        public Int32 nLX_HomeImpulsVelo;
        //	nLZ_HomeImpulsVelo				: DINT;
        public Int32 nLZ_HomeImpulsVelo;
        //	nLW_HomeImpulsVelo				: DINT;
        public Int32 nLW_HomeImpulsVelo;
        //	nLT_HomeImpulsVelo				: DINT;
        public Int32 nLT_HomeImpulsVelo;
        //	nLU_HomeImpulsVelo				: DINT;
        public Int32 nLU_HomeImpulsVelo;
        //	nLCC_HomeImpulsVelo				: DINT;
        public Int32 nLCC_HomeImpulsVelo;
        //	nFoup_HomeImpulsVelo			: DINT;
        public Int32 nFoup_HomeImpulsVelo;

        //	nWaferGap						: DINT;
        public Int32 nWaferGap;
        //	nWaferThick							: DINT;
        public Int32 nWaferThick;
        //	nLU_Gap							: DINT;
        public Int32 nLT_Gap;

        public Int32 nLU_Gap;

        //	nLT_StartPos					: DINT;
        public Int32 nLT_StartPos;
        //	nLT_LUC_Back_Pos				: DINT;
        public Int32 nLT_LUC_Back_Pos;
        //	nLT_LUC_For_Pos					: DINT;	
        public Int32 nLT_LUC_For_Pos;
        //	nLT_LT_PickOffset				: DINT;	
        public Int32 nLT_LT_PickOffset;
        //	nLT_LWPos						: DINT;	
        public Int32 nLT_LWPos;
        //	nFoup1_UpPos					: DINT;
        public Int32 nFoup1_UpPos;
        //	nFoup1_MappingStartPos			: DINT;
        public Int32 nFoup1_MappingStartPos;
        //	nFoup1_DownPos					: DINT;
        public Int32 nFoup1_DownPos;
        //	nFoup2_UpPos					: DINT;
        public Int32 nFoup2_UpPos;
        //	nFoup2_MappingStartPos			: DINT;
        public Int32 nFoup2_MappingStartPos;
        //	nFoup2_DownPos					: DINT;
        public Int32 nFoup2_DownPos;
        //	nFoup3_UpPos					: DINT;
        public Int32 nFoup3_UpPos;
        //	nFoup3_MappingStartPos			: DINT;
        public Int32 nFoup3_MappingStartPos;
        //	nFoup3_DownPos					: DINT;
        public Int32 nFoup3_DownPos;
        //	nLUD_HeightOffset				: DINT;
        public Int32 nLUD_HeightOffset;
        //	nLUU_HeightOffset		    	: DINT;
        public Int32 nLUU_HeightOffset;
        //	nLCC_HeightOffset				: DINT;
        public Int32 nLCC_HeightOffset;
        //	nLX_Velo						: DINT;
        public Int32 nLX_Velo;
        //	nLZ_Velo						: DINT;
        public Int32 nLZ_Velo;
        //	nLW_Velo						: DINT;
        public Int32 nLW_Velo;
        //	nLT_Velo						: DINT;
        public Int32 nLT_Velo;
        //	nLU_Velo						: DINT;
        public Int32 nLU_Velo;
        //	nLCC_Velo						: DINT;
        public Int32 nLCC_Velo;
        //	nFoup_Velo						: DINT;
        public Int32 nFoup_Velo;
        //	nTimeOut						: INT;
        public short nTimeOut;
        //END_STRUCT
        //END_TYPE
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class stGPRobotParam
    {
        //        TYPE ST_Int_Params :
        //STRUCT
        //    nLX_HomeOffset					: DINT;
        public double fLX_HomeOffset;
        //	nLZM_HomeOffset					: DINT;
        public double fLZM_HomeOffset;
        //	nLZS_HomeOffset					: DINT;
        public double fLZS_HomeOffset;
        //	nLW_HomeOffset					: DINT;
        public double fLW_HomeOffset;
        //	nLT_HomeOffset					: DINT;
        public double fLT_HomeOffset;
        //	nLUD_HomeOffset					: DINT;
        public double fLUD_HomeOffset;
        //	nLUU_HomeOffset					: DINT;
        public double fLUU_HomeOffset;
        //	nLCC_HomeOffset					: DINT;
        public double fLCC_HomeOffset;
        //	nFoup_1_HomeOffset				: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public double[] fFoup_HomeOffset = new double[GPLoaderDef.FoupCount];

        //	nLX_HomePos						: DINT;
        public double fLX_HomePos;
        //	nLZM_HomePos					: DINT;
        public double fLZM_HomePos;
        //	nLZS_HomePos					: DINT;
        public double fLZS_HomePos;
        //	nLW_HomePos						: DINT;
        public double fLW_HomePos;
        //	nLT_HomePos						: DINT;
        public double fLT_HomePos;
        //	nLUD_HomePos					: DINT;
        public double fLUD_HomePos;
        //	nLUU_HomePos					: DINT;
        public double fLUU_HomePos;
        //	nLCC_HomePos					: DINT;
        public double fLCC_HomePos;
        //	nFoup_1_HomePos					: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public double[] fFoup_HomePos = new double[GPLoaderDef.FoupCount];

        //	nLX_HomeVelo					: DINT;
        public double fLX_HomeVelo;
        //	nLZ_HomeVelo					: DINT;
        public double fLZ_HomeVelo;
        //	nLW_HomeVelo					: DINT;
        public double fLW_HomeVelo;
        //	nLT_HomeVelo					: DINT;
        public double fLT_HomeVelo;
        //	nLU_HomeVelo					: DINT;
        public double fLU_HomeVelo;
        //	nLCC_HomeVelo					: DINT;
        public double fLCC_HomeVelo;
        //	nFoup_HomeVelo					: DINT;
        public double fFoup_HomeVelo;


        //	nLX_HomeImpulsVelo				: DINT;
        public double fLX_HomeImpulsVelo;
        //	nLZ_HomeImpulsVelo				: DINT;
        public double fLZ_HomeImpulsVelo;
        //	nLW_HomeImpulsVelo				: DINT;
        public double fLW_HomeImpulsVelo;
        //	nLT_HomeImpulsVelo				: DINT;
        public double fLT_HomeImpulsVelo;
        //	nLU_HomeImpulsVelo				: DINT;
        public double fLU_HomeImpulsVelo;
        //	nLCC_HomeImpulsVelo				: DINT;
        public double fLCC_HomeImpulsVelo;
        //	nFoup_HomeImpulsVelo			: DINT;
        public double fFoup_HomeImpulsVelo;

        //	nWaferGap						: DINT;
        public double fCST_WaferGap;
        //	nWaferThick							: DINT;
        public double fCST_nWaferThick;
        //	nLT_Gap							: DINT;
        public double fLT_Gap;
        //	nLU_Gap							: DINT;
        public double fLU_Gap;

        //	nLT_StartPos					: DINT;
        public double fLT_StartPos;
        //	nLT_LUC_Back_Pos				: DINT;
        public double fLT_LUC_Back_Pos;
        //	nLT_LUC_For_Pos					: DINT;	
        public double fLT_LUC_For_Pos;
        //	nLT_LT_PickOffset				: DINT;	
        public double fLT_LT_PickOffset;
        //	nLT_LWPos						: DINT;	
        public double fLT_LT_Pos;
        //	nFoup_UpPos						: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public double[] fFoup_UpPos = new double[GPLoaderDef.FoupCount];
        //	fFoup_MappingStart_Pos			: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public double[] fFoup_MappingStart_Pos = new double[GPLoaderDef.FoupCount];
        //	fFoup_DownPos			        : DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public double[] fFoup_DownPos = new double[GPLoaderDef.FoupCount];
        //	stFixTray_Pos			        : DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FTCount)]
        public stProCoordination[] stFixTray_Pos = new stProCoordination[GPLoaderDef.FTCount];
        //	stCardBuffer_Pos		       	: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.CardBufferCount)]
        public stProCoordination[] stCardBuffer_Pos = new stProCoordination[GPLoaderDef.CardBufferCount];
        //	stCST_Pos		            	: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public stProCoordination[] stCST_Pos = new stProCoordination[GPLoaderDef.FoupCount];
        //	stPreA_Pos		    			: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.FoupCount)]
        public stProCoordination[] stPreA_Pos = new stProCoordination[GPLoaderDef.FoupCount];
        //	stStage_Pos		            	: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.StageCount)]
        public stProCoordination[] stStage_Pos = new stProCoordination[GPLoaderDef.StageCount];
        //	stStageCC_Pos	        		: DINT;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = GPLoaderDef.StageCount)]
        public stProCoordination[] stStageCC_Pos = new stProCoordination[GPLoaderDef.StageCount];

        //	nLX_Velo						: DINT;
        public double fLX_Velo;
        //	nLZ_Velo						: DINT;
        public double fLZ_Velo;
        //	nLW_Velo						: DINT;
        public double fLW_Velo;
        //	nLT_Velo						: DINT;
        public double fLT_Velo;
        //	nLU_Velo						: DINT;
        public double fLU_Velo;
        //	nLCC_Velo						: DINT;
        public double fLCC_Velo;
        //	nFoup_Velo						: DINT;
        public double fFoup_Velo;
        //	nTimeOut						: INT;
        public short fTimeOut;
        //END_STRUCT
        //END_TYPE
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class stProCoordination
    {
        //    TYPE stProCoordination :
        //STRUCT
        //    fXPos                 : DOUBLE;
        public Double fXPos;            //: DOUBLE;
        //    fZPos                 : DOUBLE;
        public Double fZPos;            //: DOUBLE;
        //	fWPos					: DOUBLE;
        public Double fWPos;            //: DOUBLE;
        //	fLUC_For_Pos					: DOUBLE;
        public Double fLUC_For_Pos;            //: DOUBLE;
        //	fLUC_Back_Pos				: DOUBLE;
        public Double fLUC_Back_Pos;            //: DOUBLE;
        //	fLT_Pos					: DOUBLE;
        public Double fLT_Pos;            //: DOUBLE;
        //	fLZ_PickOffset			: DOUBLE;
        public Double fLZ_PickOffset;            //: DOUBLE;
        //END_STRUCT
        //END_TYPE
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class stAccessParam
    {
        //    TYPE ST_ModuleAccessPos :
        //STRUCT
        //    nLX_Pos                 : DINT;
        public Int32 nLX_Pos;            //: DINT;
        //	nLZ_Pos					: DINT;
        public Int32 nLZ_Pos;            //: DINT;
        //	nLW_Pos					: DINT;
        public Int32 nLW_Pos;            //: DINT;
        //	nLU_Pos					: DINT;
        public Int32 nLU_Pos;            //: DINT;
        //	nLUC_Pos				: DINT;
        public Int32 nLUC_Pos;            //: DINT;
        //	nLT_Pos					: DINT;
        public Int32 nLT_Pos;            //: DINT;
        //	nLZ_PickOffset			: DINT;
        public Int32 nLZ_PickOffset;            //: DINT;
        //END_STRUCT
        //END_TYPE
    }




    public interface IGPLoader : INotifyPropertyChanged
    {
        stCDXIn CDXIn { get; }
        stCDXOut CDXOut { get; }
        stGPLoaderParam GPLoaderParam { get; set; }

        ObservableCollection<stAccessParam> FixTrayAccParams { get; set; }
        ObservableCollection<stAccessParam> CardBufferAccParams { get; set; }
        ObservableCollection<stAccessParam> CSTAccParams { get; set; }
        ObservableCollection<stAccessParam> PAAccParams { get; set; }
        ObservableCollection<stAccessParam> ChuckAccParams { get; set; }
        ObservableCollection<stAccessParam> CardAccParams { get; set; }
        bool LeftSaftyDoor_Lock { get; set; }
        bool RightSaftyDoor_Lock { get; set; }
        bool Front_Signal_Red { get; set; }
        bool Front_Signal_Yellow { get; set; }
        bool Front_Signal_Green { get; set; }
        bool Front_Signal_Buzzer { get; set; }
        bool Rear_Signal_Red { get; set; }
        bool Rear_Signal_Yellow { get; set; }
        bool Rear_Signal_Green { get; set; }
        bool Rear_Signal_Buzzer { get; set; }
        ErrorCodeEnum RenewData();

        ManualResetEvent SyncEvent { get; }
        bool DevConnected { get; }
        ErrorCodeEnum Connect();
        object RemoteModule { get; }
        bool AxisEnabled { get; }
        //object ReadSymbol(string symbol);
        //ErrorCodeEnum WriteSymbol(string symbol, object value);
        ErrorCodeEnum JogMove(ProbeAxisObject axisobj, double dist);
    }
    public enum EnumLoaderMode
    {
        IDLE = 0,
        ACTIVE = 1,
        HOME = 3,
        JOG = 5,
        RESET = 6,
        ERROR = 7,
    }
    public enum EnumLoaderState
    {
        IDLE = 0,
        ACTIVE = 1,
        HOME = 3,
        HOMED = 4,
        JOG = 5,
        RESET = 6,
        ERROR = 7,
    }
    public enum EnumRobotCommand
    {
        IDLE = 0,
        CST_PICK = 1,
        CST_PUT = 3,
        PA_PICK = 6,
        PA_PUT = 9,
        BUFF_PICK = 11,
        BUFF_PUT = 13,
        STAGE_PICK = 16,
        STAGE_PUT = 18,
        CC_PICK = 21,
        CC_PUT = 23,
        FT_PICK = 26,
        FT_PUT = 28,
        CARDBUFF_PICK = 31,
        CARDBUFF_PUT = 33,
    }
    public enum EnumCSTCommand
    {
        IDLE = 0,
        CST1_SCAN =1,
        CST2_SCAN = 2,
        CST3_SCAN = 4,
        CST1_LOAD = 8,
        CST2_LOAD = 16,
        CST3_LOAD = 32,
        CST1_LOAD_RESET = 64,
        CST2_LOAD_RESET = 128,
        CST3_LOAD_RESET = 256,
        CST1_UNLOAD = 512,
        CST2_UNLOAD = 1024,
        CST3_UNLOAD = 2048,
        CST1_UNLOAD_RESET = 4096,
        CST2_UNLOAD_RESET = 8192,
        CST3_UNLOAD_RESET = 16384,
        CST1_ENABLE = 32768,
        CST2_ENABLE = 62536,
        CST3_ENABLE = 131072
    }
    public enum EnumCSTState
    {
        EMPTY = 0,
        PRESENCE = 1,
        LOADING = 2,
        LOADED = 3,
        UNLOADING = 4,
        UNLOADED = 5,
        WAFEROUT = 6,
        TIMEOUTERROR =  7,
        NODETECTERROR =  8,
        RESET =  9,
    }

    public enum EnumRobotState
    {
        IDLE = 0,
        CST_PICKING = 1,
        CST_PICKED = 2,
        CST_PUTTING = 3,
        CST_PUTED = 4,
        CST_FAILED = 5,

        PA_PICKING = 6,
        PA_PICKED = 7,
        PA_PUTTING = 8,
        PA_PUTED = 9,
        PA_FAILED = 10,

        BUFF_PICKING = 11,
        BUFF_PICKED = 12,
        BUFF_PUTTING = 13,
        BUFF_PUTED = 14,
        BUFF_FAILED = 15,


        STAGE_PICKING = 16,
        STAGE_PICKED = 17,
        STAGE_PUTTING = 18,
        STAGE_PUTED = 19,
        STAGE_FAILED = 20,

        CC_PICKING = 21,
        CC_PICKED = 22,
        CC_PUTTING = 23,
        CC_PUTED = 24,
        CC_FAILED = 25,

        FT_PICKING = 26,
        FT_PICKED = 27,
        FT_PUTTING = 28,
        FT_PUTED = 29,
        FT_FAILED = 30,

        CARDBUFF_PICKING = 31,
        CARDBUFF_PICKED = 32,
        CARDBUFF_PUTTING = 33,
        CARDBUFF_PUTED = 34,
        CARDBUFF_FAILED = 35,
    }
    public enum EnumPAProcCmd
    {

    }
}
