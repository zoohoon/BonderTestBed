using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SorterPickTester.Data
{
    public class PickerData : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public const int MASK_2BYTE = 0x0000FFFF;       // 2BYTEデータ抽出用 マスク値.
        public const int MASK_1BYTE = 0x000000FF;       // 2BYTEデータ抽出用 マスク値.
        public const int ALL_BIT_OFF = 0x00000000;      // 全ビットOFF値.

        public void Clear_Param_AllZero()
        {
            Inching_Z = 0;
            Inching_T = 0;
            User_Origin_Z = 0;
            User_Origin_T = 0;
            Power_Correction = 0;
            Sequence_Number = 0;
            Target_01 = 0;
            Target_02 = 0;
            Target_03 = 0;
            Target_04 = 0;
            Target_05 = 0;
            Target_06 = 0;
            Target_07 = 0;
            Target_08 = 0;
            Target_09 = 0;
            Target_10 = 0;
            Target_11 = 0;
            Target_12 = 0;
        }

        // W-1 動作指令.
        private int Func_Order;
        public int Get_Func_Order() { return Func_Order; }
        public byte[] Get_Func_Order_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Func_Order); }
        public void Set_Func_Order(int param) { Func_Order = (param & MASK_2BYTE); }
        public void Set_Func_Order_bit_ON(int on_bit) { Func_Order |= (on_bit & MASK_2BYTE); }
        public void Set_Func_Order_bit_OFF(int off_bit) { Func_Order &= ((~off_bit & MASK_2BYTE)); }
        public void Set_Func_Order_All_bit_OFF() { Func_Order = ALL_BIT_OFF; }

        private byte Sequence_Number;       // シーケンス番号.
        public byte Get_Sequence_Number() { return Sequence_Number; }
        public void Set_Sequence_Number(byte param) { Sequence_Number = param; }

        // W-2 機能選択.
        private byte FuncSelect;            // 機能選択.
        public void Set_FuncSelect(byte param) { FuncSelect = param; }
        public byte Get_FuncSelect() { return FuncSelect; }
        public void Set_Func_Order_bit_Toggle(int FuncOrder_bit)
        {
            // 送信コマンド：外部イベントトリガ4.
            if (Get_Func_Order_bit(FuncOrder_bit))
            {
                Set_Func_Order_bit_OFF(FuncOrder_bit);
            }
            else
            {
                Set_Func_Order_bit_ON(FuncOrder_bit);
            }
        }
        public void Set_Func_Order_bit_On(int FuncOrder_bit)
        {
            // 送信コマンド：外部イベントトリガ4.
            if (!Get_Func_Order_bit(FuncOrder_bit))
            {
                Set_Func_Order_bit_ON(FuncOrder_bit);
            }
        }
        public void Set_Func_Order_bit_Off(int FuncOrder_bit)
        {
            // 送信コマンド：外部イベントトリガ4.
            if (Get_Func_Order_bit(FuncOrder_bit))
            {
                Set_Func_Order_bit_OFF(FuncOrder_bit);
            }
        }
        public void Reset_Func_Order_bit()
        {
            Func_Order = 0;
        }
        public bool Get_Func_Order_bit(int on_bit)
        {
            bool bRet;
            int check_bit = on_bit & MASK_2BYTE;

            if ((Func_Order & check_bit) == check_bit)
            {
                bRet = true;    // 対象ビットON → 戻り値：true.
            }
            else
            {
                bRet = false;   // 対象ビットOFF → 戻り値：false.
            }

            return bRet;
        }

        // W-3 目標値.
        private short Target_01;            // 目標値1.
        private short Target_02;            // 目標値2.
        private short Target_03;            // 目標値3.
        private short Target_04;            // 目標値4.
        private short Target_05;            // 目標値5.
        private short Target_06;            // 目標値6.
        private short Target_07;            // 目標値7.
        private short Target_08;            // 目標値8.
        public void Set_Target_01(short param) { Target_01 = param; }
        public void Set_Target_02(short param) { Target_02 = param; }
        public void Set_Target_03(short param) { Target_03 = param; }
        public void Set_Target_04(short param) { Target_04 = param; }
        public void Set_Target_05(short param) { Target_05 = param; }
        public void Set_Target_06(short param) { Target_06 = param; }
        public void Set_Target_07(short param) { Target_07 = param; }
        public void Set_Target_08(short param) { Target_08 = param; }
        public byte[] Get_Target_01_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_01); }
        public byte[] Get_Target_02_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_02); }
        public byte[] Get_Target_03_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_03); }
        public byte[] Get_Target_04_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_04); }
        public byte[] Get_Target_05_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_05); }
        public byte[] Get_Target_06_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_06); }
        public byte[] Get_Target_07_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_07); }
        public byte[] Get_Target_08_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_08); }

        // W-4 ユーザー原点設定値.
        private short User_Origin_Z;        // ユーザー原点設定値Ｚ軸.
        private short User_Origin_T;        // ユーザー原点設定値θ軸.
        public void Set_User_Origin_Z(short param) { User_Origin_Z = param; }
        public void Set_User_Origin_T(short param) { User_Origin_T = param; }
        public short Get_User_Origin_Z_short() { return User_Origin_Z; }
        public short Get_User_Origin_T_short() { return User_Origin_T; }
        public byte[] Get_User_Origin_Z_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(User_Origin_Z); }
        public byte[] Get_User_Origin_T_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(User_Origin_T); }

        // W-5 インチング指令値.
        private short Inching_Z;            // インチング指令値Ｚ軸.
        private short Inching_T;            // インチング指令値θ軸.
        public void Set_Inching_Z(short param) { Inching_Z = param; }
        public void Set_Inching_T(short param) { Inching_T = param; }
        public short Get_Inching_Z_short() { return Inching_Z; }
        public short Get_Inching_T_short() { return Inching_T; }
        public byte[] Get_Inching_Z_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Inching_Z); }
        public byte[] Get_Inching_T_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Inching_T); }

        // W-6 イベント波形転送.
        private byte Trans_Order;           // 転送指示.
        private byte Event_Number;          // イベント番号.
        private short Offset_Time;           // オフセット時間.
        public bool Set_Trans_Order(string param) { return DataConvert.Check_UserInputData(param, ref Trans_Order); }
        public bool Set_Event_Number(string param) { return DataConvert.Check_UserInputData(param, ref Event_Number); }
        public bool Set_Offset_Time(string param) { return DataConvert.Check_UserInputData(param, ref Offset_Time); }
        public byte Get_Trans_Order() { return Trans_Order; }
        public byte Get_Event_Number() { return Event_Number; }
        public short Get_Offset_Time_short() { return Offset_Time; }
        public byte[] Get_Offset_Time_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Offset_Time); }

        // W-7 目標値２.
        private short Target_09;            // 目標値9.
        private short Target_10;            // 目標値10.
        private short Target_11;            // 目標値11.
        private short Target_12;            // 目標値12.
        public void Set_Target_09(short param) { Target_09 = param; }
        public void Set_Target_10(short param) { Target_10 = param; }
        public void Set_Target_11(short param) { Target_11 = param; }
        public void Set_Target_12(short param) { Target_12 = param; }
        public short Get_Target_09_short() { return Target_09; }
        public short Get_Target_10_short() { return Target_10; }
        public short Get_Target_11_short() { return Target_11; }
        public short Get_Target_12_short() { return Target_12; }
        public byte[] Get_Target_09_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_09); }
        public byte[] Get_Target_10_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_10); }
        public byte[] Get_Target_11_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_11); }
        public byte[] Get_Target_12_byteArray() { return DataConvert.Integer2Bytes_to_ByteArray(Target_12); }

        // W-8 機能設定.
        private sbyte Power_Correction;     // 力制御補正値.
        public void Set_Power_Correction(sbyte param) { Power_Correction = param; }
        public sbyte Get_Power_Correction() { return Power_Correction; }

        //Receive Packet : control status
        private bool _CntrStatus_SequenceNow;
        public bool CntrStatus_SequenceNow
        {
            get { return _CntrStatus_SequenceNow; }
            set
            {
                if (value != _CntrStatus_SequenceNow)
                {
                    _CntrStatus_SequenceNow = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CntrStatus_SequenceStop;
        public bool CntrStatus_SequenceStop
        {
            get { return _CntrStatus_SequenceStop; }
            set
            {
                if (value != _CntrStatus_SequenceStop)
                {
                    _CntrStatus_SequenceStop = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CntrStatus_TimeOutErr;
        public bool CntrStatus_TimeOutErr
        {
            get { return _CntrStatus_TimeOutErr; }
            set
            {
                if (value != _CntrStatus_TimeOutErr)
                {
                    _CntrStatus_TimeOutErr = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CntrStatus_WaveDataErr;
        public bool CntrStatus_WaveDataErr
        {
            get { return _CntrStatus_WaveDataErr; }
            set
            {
                if (value != _CntrStatus_WaveDataErr)
                {
                    _CntrStatus_WaveDataErr = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _CntrStatus_Toggle;
        public bool CntrStatus_Toggle
        {
            get { return _CntrStatus_Toggle; }
            set
            {
                if (value != _CntrStatus_Toggle)
                {
                    _CntrStatus_Toggle = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool CntrStatus_Reservve1 = false;
        private bool CntrStatus_Reservve2 = false;
        private bool CntrStatus_Reservve3 = false;

        //Receive Packet : z/t axis status
        private bool _ZTStatus_ServoOn_ZT;
        public bool ZTStatus_ServoOn_ZT
        {
            get { return _ZTStatus_ServoOn_ZT; }
            set
            {
                if (value != _ZTStatus_ServoOn_ZT)
                {
                    _ZTStatus_ServoOn_ZT = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ZTStatus_Origin_OK;
        public bool ZTStatus_Origin_OK
        {
            get { return _ZTStatus_Origin_OK; }
            set
            {
                if (value != _ZTStatus_Origin_OK)
                {
                    _ZTStatus_Origin_OK = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ZTStatus_Working_Z;
        public bool ZTStatus_Working_Z
        {
            get { return _ZTStatus_Working_Z; }
            set
            {
                if (value != _ZTStatus_Working_Z)
                {
                    _ZTStatus_Working_Z = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ZTStatus_Working_T;
        public bool ZTStatus_Working_T
        {
            get { return _ZTStatus_Working_T; }
            set
            {
                if (value != _ZTStatus_Working_T)
                {
                    _ZTStatus_Working_T = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ZTStatus_Saturate_Z;
        public bool ZTStatus_Saturate_Z
        {
            get { return _ZTStatus_Saturate_Z; }
            set
            {
                if (value != _ZTStatus_Saturate_Z)
                {
                    _ZTStatus_Saturate_Z = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ZTStatus_Saturate_T;
        public bool ZTStatus_Saturate_T
        {
            get { return _ZTStatus_Saturate_T; }
            set
            {
                if (value != _ZTStatus_Saturate_T)
                {
                    _ZTStatus_Saturate_T = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ZTStatus_ServoOn_Z;
        public bool ZTStatus_ServoOn_Z
        {
            get { return _ZTStatus_ServoOn_Z; }
            set
            {
                if (value != _ZTStatus_ServoOn_Z)
                {
                    _ZTStatus_ServoOn_Z = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _ZTStatus_ServoOn_T;
        public bool ZTStatus_ServoOn_T
        {
            get { return _ZTStatus_ServoOn_T; }
            set
            {
                if (value != _ZTStatus_ServoOn_T)
                {
                    _ZTStatus_ServoOn_T = value;
                    RaisePropertyChanged();
                }
            }
        }

        //Receive Packet : other status
        private bool _OtherStatus_Attract_Value;
        public bool OtherStatus_Attract_Value
        {
            get { return _OtherStatus_Attract_Value; }
            set
            {
                if (value != _OtherStatus_Attract_Value)
                {
                    _OtherStatus_Attract_Value = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OtherStatus_Release_Value;
        public bool OtherStatus_Release_Value
        {
            get { return _OtherStatus_Release_Value; }
            set
            {
                if (value != _OtherStatus_Release_Value)
                {
                    _OtherStatus_Release_Value = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OtherStatus_Brake;
        public bool OtherStatus_Brake
        {
            get { return _OtherStatus_Brake; }
            set
            {
                if (value != _OtherStatus_Brake)
                {
                    _OtherStatus_Brake = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OtherStatus_Step1_End;
        public bool OtherStatus_Step1_End
        {
            get { return _OtherStatus_Step1_End; }
            set
            {
                if (value != _OtherStatus_Step1_End)
                {
                    _OtherStatus_Step1_End = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OtherStatus_Step2_End;
        public bool OtherStatus_Step2_End
        {
            get { return _OtherStatus_Step2_End; }
            set
            {
                if (value != _OtherStatus_Step2_End)
                {
                    _OtherStatus_Step2_End = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OtherStatus_Step3_End;
        public bool OtherStatus_Step3_End
        {
            get { return _OtherStatus_Step3_End; }
            set
            {
                if (value != _OtherStatus_Step3_End)
                {
                    _OtherStatus_Step3_End = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OtherStatus_Origin_OK_Z;
        public bool OtherStatus_Origin_OK_Z
        {
            get { return _OtherStatus_Origin_OK_Z; }
            set
            {
                if (value != _OtherStatus_Origin_OK_Z)
                {
                    _OtherStatus_Origin_OK_Z = value;
                    RaisePropertyChanged();
                }
            }
        }
        private bool _OtherStatus_Origin_OK_T;
        public bool OtherStatus_Origin_OK_T
        {
            get { return _OtherStatus_Origin_OK_T; }
            set
            {
                if (value != _OtherStatus_Origin_OK_T)
                {
                    _OtherStatus_Origin_OK_T = value;
                    RaisePropertyChanged();
                }
            }
        }

        //Receive Packet : alarm
        public bool _Alarm_Head_CIR_ERR;
        public bool Alarm_Head_CIR_ERR
        {
            get { return _Alarm_Head_CIR_ERR; }
            set
            {
                if (value != _Alarm_Head_CIR_ERR)
                {
                    _Alarm_Head_CIR_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_Ctrlr_CIR_ERR;
        public bool Alarm_Ctrlr_CIR_ERR
        {
            get { return _Alarm_Ctrlr_CIR_ERR; }
            set
            {
                if (value != _Alarm_Ctrlr_CIR_ERR)
                {
                    _Alarm_Ctrlr_CIR_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_Motor_Guard;
        public bool Alarm_Motor_Guard
        {
            get { return _Alarm_Motor_Guard; }
            set
            {
                if (value != _Alarm_Motor_Guard)
                {
                    _Alarm_Motor_Guard = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_Head_Ctrlr_Com_ERR;
        public bool Alarm_Head_Ctrlr_Com_ERR
        {
            get { return _Alarm_Head_Ctrlr_Com_ERR; }
            set
            {
                if (value != _Alarm_Head_Ctrlr_Com_ERR)
                {
                    _Alarm_Head_Ctrlr_Com_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_Servo_On_ERR;
        public bool Alarm_Servo_On_ERR
        {
            get { return _Alarm_Servo_On_ERR; }
            set
            {
                if (value != _Alarm_Servo_On_ERR)
                {
                    _Alarm_Servo_On_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_Origin_ERR;
        public bool Alarm_Origin_ERR
        {
            get { return _Alarm_Origin_ERR; }
            set
            {
                if (value != _Alarm_Origin_ERR)
                {
                    _Alarm_Origin_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_Head_ACT_ERR;
        public bool Alarm_Head_ACT_ERR
        {
            get { return _Alarm_Head_ACT_ERR; }
            set
            {
                if (value != _Alarm_Head_ACT_ERR)
                {
                    _Alarm_Head_ACT_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_Order_ERR;
        public bool Alarm_Order_ERR
        {
            get { return _Alarm_Order_ERR; }
            set
            {
                if (value != _Alarm_Order_ERR)
                {
                    _Alarm_Order_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
        public bool _Alarm_PLC_COM_ERR;
        public bool Alarm_PLC_COM_ERR
        {
            get { return _Alarm_PLC_COM_ERR; }
            set
            {
                if (value != _Alarm_PLC_COM_ERR)
                {
                    _Alarm_PLC_COM_ERR = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
