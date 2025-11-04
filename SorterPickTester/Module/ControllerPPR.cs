using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LogModule;
using SorterPickTester.Net;
using SorterPickTester.Module;
using SorterPickTester.Data;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SorterPickTester.Module
{
    public class ControllerPPR
    {
        private byte[] Recv_Buf;
        private byte[] Send_Buf;

        public const int CMD_CYCLIC_COM = 0x0060;
        public const int CMD_MESSAGE_COM = 0x0061;
        public const int CMD_ERROR = 0x0062;

        //CMD_CYCLIC_COM

        //CMD_ERROR
        public const int ERR_DATAFORMAT_COMM = 0x0001;
        public const int ERR_INVALID_COMMAND_COMM = 0x0002;
        public const int ERR_COMMAND_LENGTH_COMM = 0x0003;
        public const int ERR_DAT_LEN_COMM = 0x0004;
        public const int ERR_INVALID_READ_ADDR_COMM = 0x0005;
        public const int ERR_COMMAND_TMO_COMM = 0x0006;
        public const int ERR_OTHER_COMM = 0x0007;

        private TcpServer tcpServer = new TcpServer();
        private TcpClient tcpClient = new TcpClient();

        private static System.Threading.Timer cyc_timer;        // 周期処理用タイマ.
        private LockStatement _LS;
        
        private int _iCycle_Interval;

        private bool _CycleFirstFlag;
        public bool CycleFirstFlag
        {
            get
            {
                return _CycleFirstFlag;
            }
            set
            {
                if (value != _CycleFirstFlag)
                {
                    _CycleFirstFlag = value;
                }
            }
        }

        private bool _CycleNeedUpdate;
        public bool CycleNeedUpdate
        {
            get
            {
                return _CycleNeedUpdate;
            }
            set
            {
                if (value != _CycleNeedUpdate)
                {
                    _CycleNeedUpdate = value;
                }
            }
        }

        // 通信フレーム各部データ長定義
        public const int FRAME_HEADER_LENGTH = 2;               // [HEADER]部 データ長.
        public const int FRAME_LEN_LENGTH = 2;                  // [LEN]部 データ長.
        public const int FRAME_CMD_LENGTH = 2;                  // [CMD]部 データ長.
        public const int FRAME_CYC_SND_PAYLOAD_LENGTH = 48;     // サイクリック通信送信ペイロード部データ長.
        public const int FRAME_MSG_SND_PAYLOAD_LENGTH = 4;      // サイクリック通信受信ペイロード部データ長.
        public const int FRAME_HEADER_ALL = FRAME_HEADER_LENGTH + FRAME_LEN_LENGTH + FRAME_CMD_LENGTH;      // ペイロード以外のデータ長.

        public const int FRAME_LEN_POS = 2;                     // [LEN]部 フレーム内位置.
        public const int FRAME_CMD_POS = 4;                     // [CMD]部 フレーム内位置.

        public const int FRAME_PAYLOAD_MAX = 1280;                                      // Payload 最大データ長.
        public const int FRAME_MAX_LENGTH = FRAME_HEADER_ALL + FRAME_PAYLOAD_MAX;       // フレーム最大データ長.
        public const int SND_CYCLIC_LENGTH = FRAME_HEADER_ALL + FRAME_CYC_SND_PAYLOAD_LENGTH;       // サイクリック通信 送信フレーム データ長.
        public const int SND_MESSAGE_LENGTH = FRAME_HEADER_ALL + FRAME_MSG_SND_PAYLOAD_LENGTH;      // メッセージ通信 送信フレームデータ長.
        public const int RCV_MESSAGE_LENGTH = FRAME_HEADER_LENGTH + FRAME_LEN_LENGTH + FRAME_CMD_LENGTH + FRAME_PAYLOAD_MAX;        // メッセージ通信 受信フレームデータ長.

        // データ位置定義.
        public const int CYC_MEM_W1_POS_FUNC_ORDER = 0;     // 動作命令.
        public const int CYC_MEM_W1_POS_SEQ_NUM = 2;        // シーケンス番号.
        public const int CYC_MEM_W2_POS_FUNC_SELECT = 3;    // 機能選択.
        public const int CYC_MEM_W3_POS_TARGET01 = 4;       // 目標値1.
        public const int CYC_MEM_W3_POS_TARGET02 = 6;       // 目標値2.
        public const int CYC_MEM_W3_POS_TARGET03 = 8;       // 目標値3.
        public const int CYC_MEM_W3_POS_TARGET04 = 10;      // 目標値4.
        public const int CYC_MEM_W3_POS_TARGET05 = 12;      // 目標値5.
        public const int CYC_MEM_W3_POS_TARGET06 = 14;      // 目標値6.
        public const int CYC_MEM_W3_POS_TARGET07 = 16;      // 目標値7.
        public const int CYC_MEM_W3_POS_TARGET08 = 18;      // 目標値8.
        public const int CYC_MEM_W4_POS_USR_ORG_Z = 20;     // ユーザー原点設定値Ｚ軸.
        public const int CYC_MEM_W4_POS_USR_ORG_T = 22;     // ユーザー原点設定値θ軸.
        public const int CYC_MEM_W5_POS_INCH_ORDER_Z = 24;  // インチング指令値Ｚ軸.
        public const int CYC_MEM_W5_POS_INCH_ORDER_T = 26;  // インチング指令値θ軸.
        public const int CYC_MEM_W6_POS_TRANS_ORDER = 28;   // 転送指示.
        public const int CYC_MEM_W6_POS_EVNT_NUM = 29;      // イベント番号.
        public const int CYC_MEM_W6_POS_OFST_TIME = 30;     // オフセット時間.
        public const int CYC_MEM_W7_POS_TARGET09 = 32;      // 目標値9.
        public const int CYC_MEM_W7_POS_TARGET10 = 34;      // 目標値10.
        public const int CYC_MEM_W7_POS_TARGET11 = 36;      // 目標値11.
        public const int CYC_MEM_W7_POS_TARGET12 = 38;      // 目標値12.
        public const int CYC_MEM_W8_POS_RESERVE1 = 40;      // Reserve.
        public const int CYC_MEM_W8_POS_POW_CORRECT = 42;   // 力制御補正値.
        public const int CYC_MEM_W8_POS_RESERVE2 = 43;      // Reserve.
        public const int CYC_MEM_W9_POS_RESERVE = 44;       // Reserve.

        // サイクリック通信 ビット定義定義
        public const int CYC_BIT_W1_FUNC_ORDER_NO_ORDER = 0x0000;       // 動作命令無し.
        public const int CYC_BIT_W1_FUNC_ORDER_SEQ_START = 0x0001;      // シーケンス開始.
        public const int CYC_BIT_W1_FUNC_ORDER_STOP = 0x0002;           // 動作停止.
        public const int CYC_BIT_W1_FUNC_ORDER_ALARM_RES = 0x0004;      // アラームリセット.
        public const int CYC_BIT_W1_FUNC_ORDER_HIST_RST = 0x0008;       // イベント履歴リセット.
        public const int CYC_BIT_W1_FUNC_ORDER_SERVO_OFF = 0x0010;      // 緊急サーボOFF.
        public const int CYC_BIT_W1_FUNC_ORDER_AIR_STOP = 0x0020;       // エア駆動停止.
        public const int CYC_BIT_W1_FUNC_ORDER_AIR_NEGA = 0x0040;       // エア駆動負圧.
        public const int CYC_BIT_W1_FUNC_ORDER_AIR_POSI = 0x0080;       // エア駆動正圧.
        public const int CYC_BIT_W1_FUNC_ORDER_EXT_TRIG1 = 0x0100;      // 外部イベントトリガ1.
        public const int CYC_BIT_W1_FUNC_ORDER_EXT_TRIG2 = 0x0200;      // 外部イベントトリガ2.
        public const int CYC_BIT_W1_FUNC_ORDER_EXT_TRIG3 = 0x0400;      // 外部イベントトリガ3.
        public const int CYC_BIT_W1_FUNC_ORDER_EXT_TRIG4 = 0x0800;      // 外部イベントトリガ4.

        public const int CYC_SEQ_NUM_NO_OPERATION = 0;
        public const int CYC_SEQ_NUM_SERVO_ON = 129;
        public const int CYC_SEQ_NUM_SERVO_OFF = 130;
        public const int CYC_SEQ_NUM_BRAKE_ON = 131;
        public const int CYC_SEQ_NUM_BRAKE_OFF = 132;
        public const int CYC_SEQ_NUM_HOME_RETURN = 133;
        public const int CYC_SEQ_NUM_Z_INCHING = 134;
        public const int CYC_SEQ_NUM_T_INCHING = 135;
        public const int CYC_SEQ_NUM_UPDATE_USER_Z_ORIGIN = 136;
        public const int CYC_SEQ_NUM_UPDATE_USER_T_ORIGIN = 137;
        public const int CYC_SEQ_NUM_CLEAR_USER_Z_ORIGIN = 138;
        public const int CYC_SEQ_NUM_CLEAR_USER_T_ORIGIN = 139;
        public const int CYC_SEQ_NUM_UPDATE_SENSOR_ZERO_ADJUSTMENT = 140;
        public const int CYC_SEQ_NUM_CLEAR_SENSOR_ZERO_ADJUSTMENT = 141;
        public const int CYC_SEQ_NUM_VACUUM_VALVE_ON = 142;
        public const int CYC_SEQ_NUM_VACUUM_VALVE_OFF = 143;
        public const int CYC_SEQ_NUM_RELEASE_VALUE_ON = 144;
        public const int CYC_SEQ_NUM_RELEASE_VALUE_OFF = 145;
        public const int CYC_SEQ_NUM_UPDATE_FORCECNTR_CORRECTION_VAL = 146;
        public const int CYC_SEQ_NUM_SERON_ON_Z = 147;
        public const int CYC_SEQ_NUM_SERON_ON_T = 148;
        public const int CYC_SEQ_NUM_SERON_OFF_Z = 149;
        public const int CYC_SEQ_NUM_SERON_OFF_T = 150;
        public const int CYC_SEQ_NUM_HOME_RETURN_Z = 151;
        public const int CYC_SEQ_NUM_HOME_RETURN_T = 152;

        // サイクリック通信 メモリマップ定義.
        // INPUT領域（コントローラからの情報取得）.
        // データサイズ定義.
        public const int CYC_MEM_R1_LEN_FUNC_ORDER = 2;             // 動作命令入力受付.
        public const int CYC_MEM_R1_LEN_NOW_SEQ_NUM = 1;            // 動作中シーケンス番号.
        public const int CYC_MEM_R2_LEN_CTRL_STATUS = 1;            // コントローラステータス.
        public const int CYC_MEM_R2_LEN_ZT_STATUS = 1;              // Z軸θ軸ステータス.
        public const int CYC_MEM_R2_LEN_OTHER_STATUS = 1;           // その他ステータス.
        public const int CYC_MEM_R3_LEN_ALARM = 2;                  // アラーム.
        public const int CYC_MEM_R4_LEN_Z_POS = 2;                  // Ｚ軸現在位置.
        public const int CYC_MEM_R4_LEN_Z_ORDER_POS = 2;            // Ｚ軸指令位置.
        public const int CYC_MEM_R4_LEN_Z_VEROCITY = 2;             // Ｚ軸現在速度.
        public const int CYC_MEM_R4_LEN_Z_ORDER_VERO = 2;           // Ｚ軸指令速度.
        public const int CYC_MEM_R4_LEN_Z_CURRENT = 2;              // Ｚ軸現在電流.
        public const int CYC_MEM_R4_LEN_Z_ORDER_CURRENT = 2;        // Ｚ軸指令電流.
        public const int CYC_MEM_R4_LEN_T_ROTATE_CNT = 1;           // θ軸回転回数.
        public const int CYC_MEM_R4_LEN_RESERVE1 = 1;               // Reserve.
        public const int CYC_MEM_R4_LEN_T_POS = 2;                  // θ軸現在位置.
        public const int CYC_MEM_R4_LEN_T_ORDER_POS = 2;            // θ軸指令位置.
        public const int CYC_MEM_R4_LEN_T_VEROCITY = 2;             // θ軸現在速度.
        public const int CYC_MEM_R4_LEN_T_ORDER_VERO = 2;           // θ軸指令速度.
        public const int CYC_MEM_R4_LEN_T_CURRENT = 2;              // θ軸現在電流.
        public const int CYC_MEM_R4_LEN_T_ORDER_CURRENT = 2;        // θ軸指令電流.
        public const int CYC_MEM_R4_LEN_FORCE_DATA = 2;             // 力データ.
        public const int CYC_MEM_R4_LEN_PRESS_DATA = 2;             // 圧力データ.
        public const int CYC_MEM_R4_LEN_FLOW_DATA = 2;              // 流量データ.
        public const int CYC_MEM_R4_LEN_SELECT_ITEM1 = 1;           // 選択項目.
        public const int CYC_MEM_R4_LEN_SELECT_ITEM2 = 1;           // 選択項目.
        public const int CYC_MEM_R4_LEN_SELECT_ITEM3 = 1;           // 選択項目.
        public const int CYC_MEM_R6_LEN_TRANS_ORDER = 1;            // 機能選択.
        public const int CYC_MEM_R6_LEN_FUNC_SELECT = 1;            // 転送指示.
        public const int CYC_MEM_R6_LEN_EVNT_NUM = 1;               // イベント番号.
        public const int CYC_MEM_R6_LEN_OFST_TIME = 2;              // オフセット時間.

        //Receive Packet : Index
        public const int CYC_MEM_R1_POS_FUNC_ORDER = 0;             // 動作命令入力受付
        public const int CYC_MEM_R1_POS_NOW_SEQ_NUM = 2;            // 動作中シーケンス番号
        public const int CYC_MEM_R2_POS_CTRL_STATUS = 3;            // コントローラステータス
        public const int CYC_MEM_R2_POS_ZT_STATUS = 4;              // Z軸θ軸ステータス
        public const int CYC_MEM_R2_POS_OTHER_STATUS = 5;           // その他ステータス
        public const int CYC_MEM_R3_POS_ALARM = 6;                  // アラーム
        public const int CYC_MEM_R4_POS_Z_POS = 8;                  // Z軸現在位置
        public const int CYC_MEM_R4_POS_Z_ORDER_POS = 10;           // Z軸指令位置
        public const int CYC_MEM_R4_POS_Z_VEROCITY = 12;            // Z軸現在速度
        public const int CYC_MEM_R4_POS_Z_ORDER_VERO = 14;          // Z軸指令速度
        public const int CYC_MEM_R4_POS_Z_CURRENT = 16;             // Z軸現在電流
        public const int CYC_MEM_R4_POS_Z_ORDER_CURRENT = 18;       // Z軸指令電流
        public const int CYC_MEM_R4_POS_T_ROTATE_CNT = 20;          // θ軸回転回数
        public const int CYC_MEM_R4_POS_RESERVE1 = 21;              // Reserve
        public const int CYC_MEM_R4_POS_T_POS = 22;                 // θ軸現在位置
        public const int CYC_MEM_R4_POS_T_ORDER_POS = 24;           // θ軸指令位置
        public const int CYC_MEM_R4_POS_T_VEROCITY = 26;            // θ軸現在速度
        public const int CYC_MEM_R4_POS_T_ORDER_VERO = 28;          // θ軸指令速度
        public const int CYC_MEM_R4_POS_T_CURRENT = 30;             // θ軸現在電流
        public const int CYC_MEM_R4_POS_T_ORDER_CURRENT = 32;       // θ軸指令電流
        public const int CYC_MEM_R4_POS_FORCE_DATA = 34;            // 力データ
        public const int CYC_MEM_R4_POS_PRESS_DATA = 36;            // 圧力データ
        public const int CYC_MEM_R4_POS_FLOW_DATA = 38;             // 流量データ
        public const int CYC_MEM_R4_POS_SELECT_ITEM1 = 40;          // 選択項目
        public const int CYC_MEM_R4_POS_SELECT_ITEM2 = 41;          // 選択項目
        public const int CYC_MEM_R4_POS_SELECT_ITEM3 = 42;          // 選択項目
        public const int CYC_MEM_R5_POS_FUNC_SELECT = 43;           // 機能選択
        public const int CYC_MEM_R6_POS_TRANS_ORDER = 44;           // 転送指示
        public const int CYC_MEM_R6_POS_EVNT_NUM = 45;              // イベント番号
        public const int CYC_MEM_R6_POS_OFST_TIME = 46;             // オフセット時間

        //Receive Packet : controller status
        public const byte CYC_BIT_R2_CTRL_STATUS_SEQUENCE_NOW = 0x01;       // シーケンス実行中.
        public const byte CYC_BIT_R2_CTRL_STATUS_SEQUENCE_STOP = 0x02;      // シーケンス中断.
        public const byte CYC_BIT_R2_CTRL_STATUS_TIMEOUT_ERR = 0x04;        // タイムアウトエラー.
        public const byte CYC_BIT_R2_CTRL_STATUS_RESERVE1 = 0x08;           // Reserve.
        public const byte CYC_BIT_R2_CTRL_STATUS_WAVE_DATA_ERR = 0x10;      // 波形ブロック転送エラー.
        public const byte CYC_BIT_R2_CTRL_STATUS_RESERVE2 = 0x20;           // Reserve.
        public const byte CYC_BIT_R2_CTRL_STATUS_RESERVE3 = 0x40;           // Reserve.
        public const byte CYC_BIT_R2_CTRL_STATUS_TOGGLE = 0x80;             // toggle.

        //Receive Packet : z/t status
        public const byte CYC_BIT_R2_ZT_STATUS_ZT_SERVO_ON = 0x01;          // Ｚ軸θ軸サーボON状態.
        public const byte CYC_BIT_R2_ZT_STATUS_ZT_ORIGIN_OK = 0x02;         // Ｚ軸θ軸原点復帰完了状態.
        public const byte CYC_BIT_R2_ZT_STATUS_Z_WORKING = 0x04;            // Ｚ軸動作状態.
        public const byte CYC_BIT_R2_ZT_STATUS_T_WORKING = 0x08;            // θ軸動作状態.
        public const byte CYC_BIT_R2_ZT_STATUS_Z_SATURATE = 0x10;           // Ｚ軸押付力飽和.
        public const byte CYC_BIT_R2_ZT_STATUS_T_SATURATE = 0x20;           // θ軸押付力飽和.
        public const byte CYC_BIT_R2_ZT_STATUS_Z_SERVO_ON = 0x40;           // Ｚ軸サーボON状態.
        public const byte CYC_BIT_R2_ZT_STATUS_T_SERVO_ON = 0x80;           // θ軸サーボON状態.

        //Receive Packet : other status
        public const byte CYC_BIT_R2_OTHER_ATTRACT_VALVE = 0x01;            // 吸着弁状態 1：ON 0：OFF.
        public const byte CYC_BIT_R2_OTHER_RELEASE_VALVE = 0x02;            // 開放弁状態 1：ON 0：OFF.
        public const byte CYC_BIT_R2_OTHER_BRAKE = 0x04;                    // ブレーキ状態 1：ON 0：OFF.
        public const byte CYC_BIT_R2_OTHER_STEP_END1 = 0x08;                // ステップ完了1.
        public const byte CYC_BIT_R2_OTHER_STEP_END2 = 0x10;                // ステップ完了2.
        public const byte CYC_BIT_R2_OTHER_STEP_END3 = 0x20;                // ステップ完了3.
        public const byte CYC_BIT_R2_OTHER_Z_ORIGIN_OK = 0x40;              // Z軸原点復帰完了状態.
        public const byte CYC_BIT_R2_OTHER_T_ORIGIN_OK = 0x80;              // θ軸原点復帰完了状態.

        //Receive Packet : alarm
        public const short CYC_BIT_R3_ALARM_HEAD_CIR_ERR = 0x0001;          // ヘッド制御回路異常.
        public const short CYC_BIT_R3_ALARM_CTRLR_CIR_ERR = 0x0002;         // コントローラ制御回路異常.
        public const short CYC_BIT_R3_ALARM_MOTOR_GUARD = 0x0004;           // ヘッド回路・モータ保護.
        public const short CYC_BIT_R3_ALARM_HEAD_CTRLR_COM_ERR = 0x0008;    // ヘッドｰコントローラ通信異.
        public const short CYC_BIT_R3_ALARM_SERVO_ON_ERR = 0x0010;          // サーボON異常.
        public const short CYC_BIT_R3_ALARM_ORIGIN_ERR = 0x0020;            // 原点復帰異常
        public const short CYC_BIT_R3_ALARM_HEAD_ACT_ERR = 0x0040;          // ヘッド動作異常.
        public const short CYC_BIT_R3_ALARM_ORDER_ERR = 0x0080;             // 指令値異常.
        public const short CYC_BIT_R3_ALARM_PLC_COM_ERRO = 0x0100;          // PLC通信異常.

        private PickerData _pickerData = null;
        public ControllerPPR(ref PickerData pickerData)
        {
            try
            {
                _pickerData = pickerData;
                this.tcpClient.ConnectCallback += TcpClient_ConnectCallback;
                this.tcpClient.DisconnectCallback += TcpClient_DisconnectCallback;
                this.tcpClient.ReceiveCallback += TcpClient_ReceiveCallback;

                _LS = new LockStatement();

                _iCycle_Interval = 100;
                _CycleFirstFlag = false;                                // 初回送信フラグを初期化.

                Recv_Buf = new byte[RCV_MESSAGE_LENGTH];
                Send_Buf = new byte[SND_CYCLIC_LENGTH];

                cyc_timer = new System.Threading.Timer(new TimerCallback(Task_Cycle_Comm_Process), null, Timeout.Infinite, Timeout.Infinite);
            }
            catch (Exception err)
            {
                //LoggerManager.Exception(err);
            }
        }

        private bool _IsConnected = false;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set
            {
                if (value != _IsConnected)
                {
                    _IsConnected = value;
                }
            }
        }

        private void TcpClient_DisconnectCallback()
        {
            IsConnected = false;

            cyc_timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void TcpClient_ConnectCallback(System.Net.Sockets.Socket sock)
        {
            IsConnected = true;

            //             if (_bContinance)
            //             {
            //                 cyc_timer.Change(0, iCycle_Interval);
            //             }
            //             else
            //             {
            //                 cyc_timer.Change(Timeout.Infinite, Timeout.Infinite);
            //             }
        }

        public bool OnParsingPacket(byte[] aPacket, out List<byte[]> aDatas)
        {
            bool bRet = false;

            int nStart = 0;
            byte[] pktHeader = new byte[2];
            Buffer.BlockCopy(aPacket, nStart, pktHeader, 0, pktHeader.Length);
            int DataHeader = (int)(pktHeader[0] << 0 & 0xff) + (int)((pktHeader[1] & 0xff) << 8); ;
            if (DataHeader != 0xAA55)
            {
                aDatas = null;
                return bRet;
            }
            nStart += pktHeader.Length;

            byte[] pktLength = new byte[2];
            Buffer.BlockCopy(aPacket, nStart, pktLength, 0, pktLength.Length);
            int Datalen = (int)(pktLength[0] << 0 & 0xff) + (int)((pktLength[1] & 0xff) << 8);
            if ((Datalen + pktLength.Length + pktLength.Length) != aPacket.Length)
            {
                aDatas = null;
                return bRet;
            }
            nStart += pktLength.Length;

            byte[] pktCommand = new byte[2];
            Buffer.BlockCopy(aPacket, nStart, pktCommand, 0, pktCommand.Length);
            int DataCommand = (int)(pktCommand[0] << 0 & 0xff) + (int)((pktCommand[1] & 0xff) << 8);
            nStart += pktCommand.Length;

            aDatas = new List<byte[]>();
            aDatas.Add(pktHeader);
            aDatas.Add(pktLength);
            aDatas.Add(pktCommand);

            if (DataCommand == CMD_CYCLIC_COM)
            {
                byte[] pktFuncOrder = new byte[CYC_MEM_R1_LEN_FUNC_ORDER];
                Buffer.BlockCopy(aPacket, nStart, pktFuncOrder, 0, pktFuncOrder.Length);
                int DataFuncOrder = (int)(pktFuncOrder[0] << 0 & 0xff) + (int)((pktFuncOrder[1] & 0xff) << 8);
                nStart += pktFuncOrder.Length;
                Update_FuncOrder_data(DataFuncOrder);

                byte[] pktSeqNum = new byte[CYC_MEM_R1_LEN_NOW_SEQ_NUM];
                Buffer.BlockCopy(aPacket, nStart, pktSeqNum, 0, pktSeqNum.Length);
                int DataSeqNum = (int)(pktSeqNum[0] << 0 & 0xff);
                nStart += pktSeqNum.Length;
                Update_SequenceNum_data(DataSeqNum);

                byte[] pktCntrStatus = new byte[CYC_MEM_R2_LEN_CTRL_STATUS];
                Buffer.BlockCopy(aPacket, nStart, pktCntrStatus, 0, pktCntrStatus.Length);
                int DataCntrStatus = (int)(pktCntrStatus[0] << 0 & 0xff);
                nStart += pktCntrStatus.Length;
                Update_CtrlStatus_data(DataCntrStatus);

                byte[] pktZTStatus = new byte[CYC_MEM_R2_LEN_ZT_STATUS];
                Buffer.BlockCopy(aPacket, nStart, pktZTStatus, 0, pktZTStatus.Length);
                int DataZTStatus = (int)(pktZTStatus[0] << 0 & 0xff);
                nStart += pktZTStatus.Length;
                Update_ZTStatus_data(DataZTStatus);

                byte[] pktOthrStatus = new byte[CYC_MEM_R2_LEN_OTHER_STATUS];
                Buffer.BlockCopy(aPacket, nStart, pktOthrStatus, 0, pktOthrStatus.Length);
                int DataOthrStatus = (int)(pktOthrStatus[0] << 0 & 0xff);
                nStart += pktOthrStatus.Length;
                Update_OtherStatus_data(DataOthrStatus);

                byte[] pktAlarm = new byte[CYC_MEM_R3_LEN_ALARM];
                Buffer.BlockCopy(aPacket, nStart, pktAlarm, 0, pktAlarm.Length);
                int DataAlarm = (int)(pktAlarm[0] << 0 & 0xff) + (int)((pktAlarm[1] & 0xff) << 8);
                nStart += pktAlarm.Length;
                Update_Alarm_data(DataAlarm);

                aDatas.Add(pktSeqNum);
                aDatas.Add(pktCntrStatus);
                aDatas.Add(pktZTStatus);
                aDatas.Add(pktOthrStatus);
                aDatas.Add(pktAlarm);
            }
            else if (DataCommand == CMD_MESSAGE_COM)
            {
            }
            else if (DataCommand == CMD_ERROR)
            {
                byte[] pktErrCode = new byte[2];
                Buffer.BlockCopy(aPacket, 6, pktErrCode, 0, pktErrCode.Length);
                int ErrorCode = (int)(pktErrCode[0] << 0 & 0xff) + (int)((pktErrCode[1] & 0xff) << 8);
                switch (ErrorCode)
                {
                    case ERR_DATAFORMAT_COMM:
                        break;
                    case ERR_INVALID_COMMAND_COMM:
                        break;
                    case ERR_COMMAND_LENGTH_COMM:
                        break;
                    case ERR_DAT_LEN_COMM:
                        break;
                    case ERR_INVALID_READ_ADDR_COMM:
                        break;
                    case ERR_COMMAND_TMO_COMM:
                        break;
                    default:    //ERR_OTHER_COMM
                        break;
                }
                aDatas.Add(pktErrCode);
            }

            bRet = true;
            return bRet;
        }

        private void Update_SequenceNum_data(int dataSeqNum)
        {
            //throw new NotImplementedException();
        }

        private void Update_FuncOrder_data(int dataFuncOrder)
        {
            //throw new NotImplementedException();
        }

        private void Update_Alarm_data(int dataAlarm)
        {
            _pickerData.Alarm_Head_CIR_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_HEAD_CIR_ERR) > 0) ? true : false;
            _pickerData.Alarm_Ctrlr_CIR_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_CTRLR_CIR_ERR) > 0) ? true : false;
            _pickerData.Alarm_Motor_Guard = ((dataAlarm & CYC_BIT_R3_ALARM_MOTOR_GUARD) > 0) ? true : false;
            _pickerData.Alarm_Head_Ctrlr_Com_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_HEAD_CTRLR_COM_ERR) > 0) ? true : false;
            _pickerData.Alarm_Servo_On_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_SERVO_ON_ERR) > 0) ? true : false;
            _pickerData.Alarm_Origin_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_ORIGIN_ERR) > 0) ? true : false;
            _pickerData._Alarm_Head_ACT_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_HEAD_ACT_ERR) > 0) ? true : false;
            _pickerData.Alarm_Order_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_ORDER_ERR) > 0) ? true : false;
            _pickerData.Alarm_PLC_COM_ERR = ((dataAlarm & CYC_BIT_R3_ALARM_PLC_COM_ERRO) > 0) ? true : false;
        }

        private void Update_OtherStatus_data(int dataOthrStatus)
        {
            _pickerData.OtherStatus_Attract_Value = ((dataOthrStatus & CYC_BIT_R2_OTHER_ATTRACT_VALVE) > 0) ? true : false;
            _pickerData.OtherStatus_Release_Value = ((dataOthrStatus & CYC_BIT_R2_OTHER_RELEASE_VALVE) > 0) ? true : false;
            _pickerData.OtherStatus_Brake = ((dataOthrStatus & CYC_BIT_R2_OTHER_BRAKE) > 0) ? true : false;
            _pickerData.OtherStatus_Step1_End = ((dataOthrStatus & CYC_BIT_R2_OTHER_STEP_END1) > 0) ? true : false;
            _pickerData.OtherStatus_Step2_End = ((dataOthrStatus & CYC_BIT_R2_OTHER_STEP_END2) > 0) ? true : false;
            _pickerData.OtherStatus_Step3_End = ((dataOthrStatus & CYC_BIT_R2_OTHER_STEP_END3) > 0) ? true : false;
            _pickerData.OtherStatus_Origin_OK_Z = ((dataOthrStatus & CYC_BIT_R2_OTHER_Z_ORIGIN_OK) > 0) ? true : false;
            _pickerData.OtherStatus_Origin_OK_T = ((dataOthrStatus & CYC_BIT_R2_OTHER_T_ORIGIN_OK) > 0) ? true : false;
        }

        private void Update_ZTStatus_data(int dataZTStatus)
        {
            _pickerData.ZTStatus_ServoOn_ZT = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_ZT_SERVO_ON) > 0) ? true : false;
            _pickerData.ZTStatus_Origin_OK = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_ZT_ORIGIN_OK) > 0) ? true : false;
            _pickerData.ZTStatus_Working_Z = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_Z_WORKING) > 0) ? true : false;
            _pickerData.ZTStatus_Working_T = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_T_WORKING) > 0) ? true : false;
            _pickerData.ZTStatus_Saturate_Z = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_Z_SATURATE) > 0) ? true : false;
            _pickerData.ZTStatus_Saturate_T = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_T_SATURATE) > 0) ? true : false;
            _pickerData.ZTStatus_ServoOn_Z = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_Z_SERVO_ON) > 0) ? true : false;
            _pickerData.ZTStatus_ServoOn_T = ((dataZTStatus & CYC_BIT_R2_ZT_STATUS_T_SERVO_ON) > 0) ? true : false;
        }

        private void Update_CtrlStatus_data(int dataCntrStatus)
        {
            _pickerData.CntrStatus_SequenceNow = ((dataCntrStatus & CYC_BIT_R2_CTRL_STATUS_SEQUENCE_NOW) > 0) ? true : false;
            _pickerData.CntrStatus_SequenceStop = ((dataCntrStatus & CYC_BIT_R2_CTRL_STATUS_SEQUENCE_STOP) > 0) ? true : false;
            _pickerData.CntrStatus_TimeOutErr = ((dataCntrStatus & CYC_BIT_R2_CTRL_STATUS_TIMEOUT_ERR) > 0) ? true : false;
            _pickerData.CntrStatus_WaveDataErr = ((dataCntrStatus & CYC_BIT_R2_CTRL_STATUS_WAVE_DATA_ERR) > 0) ? true : false;
            _pickerData.CntrStatus_Toggle = ((dataCntrStatus & CYC_BIT_R2_CTRL_STATUS_TOGGLE) > 0) ? true : false;

            //if (_pickerData.CntrStatus_SequenceNow)
            //{
            //    if (_pickerData.Get_Sequence_Number() > 0)
            //    {
            //        _pickerData.Set_Sequence_Number(0);
            //    }
            //}
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2},", b);
            return hex.ToString();
        }

        private static ManualResetEvent mre = new ManualResetEvent(false);
        private void TcpClient_ReceiveCallback(System.Net.Sockets.Socket sock, byte[] data, int len)
        {
            List<byte[]> Params;
            bool bRet = false;
            Console.WriteLine("[RCV]," + ByteArrayToString(data));

            bRet = OnParsingPacket(data, out Params);
            if (bRet)
            {
                mre.Set();
                //Console.WriteLine("=> ==============================================================================");
                //Console.WriteLine("- Oper Cmd   : {0}", ByteArrayToString(Params[2]));
                //Console.WriteLine("- SequenceNo : {0}", ByteArrayToString(Params[3]));
                //Console.WriteLine("- CntrStatus : {0}", ByteArrayToString(Params[4]));
                //Console.WriteLine("- Z/T Status : {0}", ByteArrayToString(Params[5]));
                //Console.WriteLine("- Othr Status: {0}", ByteArrayToString(Params[6]));
                //Console.WriteLine("- Alarm      : {0}", ByteArrayToString(Params[7]));
                //Console.WriteLine("=> -----------------------------------------------------------------------------");
                //Console.WriteLine("=> Parsing OK");
                //Console.WriteLine("=> ==============================================================================");
            }
            else
                Console.WriteLine("=> Parsing NG!!!!!!!!!!!!!!!!!!!!!!");
        }

        private bool _Connected = false;
        public bool OnConnected
        {
            get { return _Connected; }
            set
            {
                if (value != _Connected)
                {
                    _Connected = value;

                    if (_Connected)
                        tcpClient.Connect(IpAddress, IpPort);
                    else
                        tcpClient.Disconnect();
                }
            }
        }

        private string _IpAddress = "192.168.10.11";
        public string IpAddress
        {
            get { return _IpAddress; }
            set
            {
                if (value != _IpAddress)
                {
                    _IpAddress = value;
                }
            }
        }

        private int _IpPort = 50000;
        public int IpPort
        {
            get { return _IpPort; }
            set
            {
                if (value != _IpPort)
                {
                    _IpPort = value;
                }
            }
        }

        private int _CyclicInterval = 100;
        public int CyclicInterval
        {
            get { return CyclicInterval; }
            set
            {
                if (value != _CyclicInterval)
                {
                    _CyclicInterval = value;
                }
            }
        }

        public bool OnMakePacket(List<byte[]> aDatas, int anSize, out byte[] aPacket)
        {
            bool bRet = false;

            //Size 확인
            int nSize = 0;
            aPacket = new byte[anSize];
            Array.Clear(aPacket, 0x0, aPacket.Length);

            foreach (byte[] param in aDatas)
            {
                nSize += param.Length;
            }
            if (nSize != anSize)
                return bRet;

            int nIndex = 0;
            foreach (byte[] param in aDatas)
            {
                if (nIndex + param.Length > anSize)
                    return bRet;

                param.CopyTo(aPacket, nIndex);
                nIndex += param.Length;
            }

            bRet = true;
            return bRet;
        }

        private void Setup_Cyclic_Frame(ref byte[] payload, ref int payload_len)
        {
            byte[] temp_byte = new byte[2];

            Array.Clear(payload, 0, SND_CYCLIC_LENGTH);

            payload_len = FRAME_CYC_SND_PAYLOAD_LENGTH;

            // W-1 動作命令. 동작 명령
            temp_byte = _pickerData.Get_Func_Order_byteArray();
            payload[CYC_MEM_W1_POS_FUNC_ORDER] = temp_byte[0];
            payload[CYC_MEM_W1_POS_FUNC_ORDER + 1] = temp_byte[1];

            // W-1 シーケンス番号. 시퀀스 번호.
            payload[CYC_MEM_W1_POS_SEQ_NUM] = _pickerData.Get_Sequence_Number();

            // W-2 機能選択. 기능 선택.
            payload[CYC_MEM_W2_POS_FUNC_SELECT] = _pickerData.Get_FuncSelect();

            // W-3 目標値 目標値1. 목표치 목표치 1
            temp_byte = _pickerData.Get_Target_01_byteArray();
            payload[CYC_MEM_W3_POS_TARGET01] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET01 + 1] = temp_byte[1];

            // W-3 目標値 目標値2. 목표치 목표치 2
            temp_byte = _pickerData.Get_Target_02_byteArray();
            payload[CYC_MEM_W3_POS_TARGET02] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET02 + 1] = temp_byte[1];

            // W-3 目標値 目標値3. 목표치 목표치 3
            temp_byte = _pickerData.Get_Target_03_byteArray();
            payload[CYC_MEM_W3_POS_TARGET03] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET03 + 1] = temp_byte[1];

            // W-3 目標値 目標値4. 목표치 목표치 4
            temp_byte = _pickerData.Get_Target_04_byteArray();
            payload[CYC_MEM_W3_POS_TARGET04] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET04 + 1] = temp_byte[1];

            // W-3 目標値 目標値5. 목표치 목표치 5
            temp_byte = _pickerData.Get_Target_05_byteArray();
            payload[CYC_MEM_W3_POS_TARGET05] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET05 + 1] = temp_byte[1];

            // W-3 目標値 目標値6. 목표치 목표치 6
            temp_byte = _pickerData.Get_Target_06_byteArray();
            payload[CYC_MEM_W3_POS_TARGET06] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET06 + 1] = temp_byte[1];

            // W-3 目標値 目標値7. 목표치 목표치 7
            temp_byte = _pickerData.Get_Target_07_byteArray();
            payload[CYC_MEM_W3_POS_TARGET07] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET07 + 1] = temp_byte[1];

            // W-3 目標値 目標値8. 목표치 목표치 8
            temp_byte = _pickerData.Get_Target_08_byteArray();
            payload[CYC_MEM_W3_POS_TARGET08] = temp_byte[0];
            payload[CYC_MEM_W3_POS_TARGET08 + 1] = temp_byte[1];

            // W-4 ユーザー原点設定値 Ｚ軸. 사용자 원점 설정값 Z축.
            temp_byte = _pickerData.Get_User_Origin_Z_byteArray();
            payload[CYC_MEM_W4_POS_USR_ORG_Z] = temp_byte[0];
            payload[CYC_MEM_W4_POS_USR_ORG_Z + 1] = temp_byte[1];

            // W-4 ユーザー原点設定値 θ軸. 사용자 원점 설정값 θ축.
            temp_byte = _pickerData.Get_User_Origin_T_byteArray();
            payload[CYC_MEM_W4_POS_USR_ORG_T] = temp_byte[0];
            payload[CYC_MEM_W4_POS_USR_ORG_T + 1] = temp_byte[1];

            // W-5 インチング指令値  Ｚ軸. 인칭 지령값 Z축.
            temp_byte = _pickerData.Get_Inching_Z_byteArray();
            payload[CYC_MEM_W5_POS_INCH_ORDER_Z] = temp_byte[0];
            payload[CYC_MEM_W5_POS_INCH_ORDER_Z + 1] = temp_byte[1];

            // W-5 インチング指令値  θ軸. 인칭 지령값 θ축.
            temp_byte = _pickerData.Get_Inching_T_byteArray();
            payload[CYC_MEM_W5_POS_INCH_ORDER_T] = temp_byte[0];
            payload[CYC_MEM_W5_POS_INCH_ORDER_T + 1] = temp_byte[1];

            // W-6 イベント波形転送 転送指示. 이벤트 파형 전송 지시.
            payload[CYC_MEM_W6_POS_TRANS_ORDER] = _pickerData.Get_Trans_Order();

            // W-6 イベント波形転送 イベント番号. 이벤트 파형 전송 이벤트 번호.
            payload[CYC_MEM_W6_POS_EVNT_NUM] = _pickerData.Get_Event_Number();

            // W-6 イベント波形転送 オフセット時間. 이벤트 파형 전송 오프셋 시간.
            temp_byte = _pickerData.Get_Offset_Time_byteArray();
            payload[CYC_MEM_W6_POS_OFST_TIME] = temp_byte[0];
            payload[CYC_MEM_W6_POS_OFST_TIME + 1] = temp_byte[1];

            // W-7 目標値２ 目標値9. 목표치 2 목표치 9
            temp_byte = _pickerData.Get_Target_09_byteArray();
            payload[CYC_MEM_W7_POS_TARGET09] = temp_byte[0];
            payload[CYC_MEM_W7_POS_TARGET09 + 1] = temp_byte[1];

            // W-7 目標値２ 目標値10. 목표치 2 목표치 10
            temp_byte = _pickerData.Get_Target_10_byteArray();
            payload[CYC_MEM_W7_POS_TARGET10] = temp_byte[0];
            payload[CYC_MEM_W7_POS_TARGET10 + 1] = temp_byte[1];

            // W-7 目標値２ 目標値11. 목표치 2 목표치 11
            temp_byte = _pickerData.Get_Target_11_byteArray();
            payload[CYC_MEM_W7_POS_TARGET11] = temp_byte[0];
            payload[CYC_MEM_W7_POS_TARGET11 + 1] = temp_byte[1];

            // W-7 目標値２ 目標値12. 목표치 2 목표치 12
            temp_byte = _pickerData.Get_Target_12_byteArray();
            payload[CYC_MEM_W7_POS_TARGET12] = temp_byte[0];
            payload[CYC_MEM_W7_POS_TARGET12 + 1] = temp_byte[1];

            // W-8 機能設定. 기능 설정.
            DataConvert.Copy_SByte_to_ByteAllay(_pickerData.Get_Power_Correction(), ref payload, CYC_MEM_W8_POS_POW_CORRECT);
        }

        public byte[] Send_PPR_Command(byte[] payload_data, int payload_len)
        {
            byte[] Msg_Buf = new byte[FRAME_HEADER_ALL + payload_len];

            byte[] ByteBuf = new byte[2];
            int iData_Length = payload_len + FRAME_CMD_LENGTH;
            int iFrame_Len;                                      // フレーム全体のデータ長.

            // メッセージのヘッダ部をセット.
            Msg_Buf[0] = 0x55;
            Msg_Buf[1] = 0xAA;

            // [LEN]に値をセット.
            ByteBuf = DataConvert.Integer2Bytes_to_ByteArray(iData_Length);
            Msg_Buf[2] = ByteBuf[0];
            Msg_Buf[3] = ByteBuf[1];

            // [CMD]に値をセット.
            ByteBuf = DataConvert.Integer2Bytes_to_ByteArray(CMD_CYCLIC_COM);
            Msg_Buf[4] = ByteBuf[0];
            Msg_Buf[5] = ByteBuf[1];

            // Payloadデータを、送信バッファにコピー.
            Buffer.BlockCopy(payload_data, 0, Msg_Buf, FRAME_HEADER_ALL, payload_len);

            // 送信フレームのデータ長を算出.
            iFrame_Len = FRAME_HEADER_ALL + payload_len;

            try
            {
                // 送信処理を実行.
                this.OnSend(Msg_Buf);
            }
            catch
            {
                // 例外発生時は上位にthrow.
                throw;
            }

            return Msg_Buf;
        }

        public bool OnSend(byte[] packet)
        {
            if (IsConnected)
            {
                tcpClient.Send(packet);

                Console.WriteLine("[SND]," + ByteArrayToString(packet));
                return true;
            }
            return false;
        }

        private bool _bContinance = false;
        public void SetContinuance(bool bEnable)
        {
            _bContinance = bEnable;

            if (_bContinance)
            {
                cyc_timer.Change(0, _iCycle_Interval);
            }
            else
            {
                cyc_timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Send_PPR_Command_Single()
        {
            if (IsConnected && !_bContinance)
            {
                try
                {
                    //// ユーザ入力値 Parameterクラスに反映.
                    //Set_UserInput_to_Param();

                    // 通信処理を別タスクで実行.
                    Task SingleSendTask = new Task(() => { Task_Single_CycComm_Process(); });

                    // タスクスタート.
                    SingleSendTask.Start();
                }
                catch (Exception ex)
                {
                    // ユーザにエラーを通知.
                    MessageBox.Show(ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int iBuf_Select_Order = CYC_BIT_W1_FUNC_ORDER_NO_ORDER;    // エッジ生成時 機能指令情報一時退避領域.
        private byte[] Snd_Payload_Data = new byte[SND_CYCLIC_LENGTH];     // ペイロードデータ格納領域.
        private int Snd_Payload_Len;                                            // ペイロード領域データ長.

        private void Task_Cycle_Comm_Process(object state)
        {
            lock (_LS)
            {
                if (_CycleFirstFlag)       // 初回送信フラグONの場合.
                {
                    Console.WriteLine("==============> OnCommandCycle()");

                    // 連続送信初回時、エッジ生成の為に0を送信する。ユーザ入力値を一時退避.
                    iBuf_Select_Order = _pickerData.Get_Func_Order();

                    // 0をセット.
                    _pickerData.Set_Func_Order(CYC_BIT_W1_FUNC_ORDER_NO_ORDER);

                    // 送信データ 構築 周期送信初回の場合、エッジ生成の為動作指令OFFを一回送信.
                    Setup_Cyclic_Frame(ref Snd_Payload_Data, ref Snd_Payload_Len);

                    // エッジ生成の為に退避していた動作命令(指定bit=Hi)の情報を、Parameterに再セット.
                    _pickerData.Set_Func_Order(iBuf_Select_Order);

                    // 初回送信フラグをクリア.
                    CycleFirstFlag = false;
                    CycleNeedUpdate = false;
                }
                else
                {
                    // 送信データ 構築.
                    Setup_Cyclic_Frame(ref Snd_Payload_Data, ref Snd_Payload_Len);
                    CycleNeedUpdate = true;
                }

                // PPR側から通信途絶が発生した場合、例外となるため、切断時の処理をtry/catchで実装.
                try
                {
                    mre.Reset();
                    // コマンド送信 送信したフレームデータを戻り値で受け取る.
                    Send_Buf = Send_PPR_Command(Snd_Payload_Data, Snd_Payload_Len);

                    if (mre.WaitOne(5000))
                    {
                        if (CycleNeedUpdate)
                        {
                            _pickerData.Set_Func_Order_bit_OFF(iBuf_Select_Order);
                            if (_pickerData.Get_Func_Order() == 0)
                            {
                                _pickerData.Set_Func_Order_All_bit_OFF();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 例外発生時の処理。通信途絶が発生した場合、以下の処理が実行される.
                    // 連続送信を中止させる為、タイマを停止.
                    cyc_timer.Change(Timeout.Infinite, Timeout.Infinite);

                    // ユーザにエラーを通知.
                    MessageBox.Show(ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } // lock(LS) block end.
        }

        /// <summary>
        /// 単発送信メイン処理(別タスク動作) .
        /// </summary>
        /// <remarks>
        /// 単発としているが、エッジが発生するように、２回連続で送信する。
        /// １回目(Lo)の送信後、エコーバックが返ってくることを確認して２回目(Hi)を送信する。
        /// 通信待ち受けなどで処理を止めるため、Formとは別のタスクで動作させる。
        /// </remarks>
        private void Task_Single_CycComm_Process()
        {
            bool bProcess_Skip_flag = false;

            lock (_LS)
            {
                try
                {
                    // ユーザが入力した動作命令情報を退避.
                    iBuf_Select_Order = _pickerData.Get_Func_Order();

                    // エッジ生成の為、エッジ手前のbit=Loをセット.
                    _pickerData.Set_Func_Order(CYC_BIT_W1_FUNC_ORDER_NO_ORDER);

                    // サイクリック通信 フレームデータセット.
                    Setup_Cyclic_Frame(ref Snd_Payload_Data, ref Snd_Payload_Len);

                    // 送信処理実行.
                    byte[] send_data = Send_PPR_Command(Snd_Payload_Data, Snd_Payload_Len);

                    if (mre.WaitOne(5000))
                    {
                        _pickerData.Set_Func_Order_bit_OFF(iBuf_Select_Order);
                        if (_pickerData.Get_Func_Order() == 0)
                        {
                            _pickerData.Set_Func_Order_All_bit_OFF();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // エラーダイアログ表示.
                    MessageBox.Show(ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    //// 通信状態リセット. 통신 상태 리셋.
                    //Reset_CommunicationState_withInvokeCheck();

                    // 以降のプロセスをスキップするため、スキップフラグON.
                    bProcess_Skip_flag = true;
                }

                if (!bProcess_Skip_flag)      // 一回目の通信失敗時はスキップ.
                {
                    // 送信間隔保証の為、スリープ;
                    Thread.Sleep(/*CNST.SEND_INTERVAL*/10);

                    try
                    {
                        // エッジ生成の為に退避していた動作命令(指定bit = Hi)のデータを、Parameterに再セット.
                        _pickerData.Set_Func_Order(iBuf_Select_Order);

                        // 送信フレームを構築.
                        Setup_Cyclic_Frame(ref Snd_Payload_Data, ref Snd_Payload_Len);

                        mre.Reset();
                        // 送信処理実行.
                        byte[] send_data = Send_PPR_Command(Snd_Payload_Data, Snd_Payload_Len);

                        if (mre.WaitOne(5000))
                        {
                            _pickerData.Set_Func_Order_bit_OFF(iBuf_Select_Order);
                            if (_pickerData.Get_Func_Order() == 0)
                            {
                                _pickerData.Set_Func_Order_All_bit_OFF();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //// 通信状態をリセット.
                        //Reset_CommunicationState_withInvokeCheck();

                        // ユーザにエラーを通知.
                        MessageBox.Show(ex.Message, "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
