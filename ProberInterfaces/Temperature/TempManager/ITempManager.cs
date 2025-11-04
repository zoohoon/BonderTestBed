
using ProberErrorCode;
using ProberInterfaces.Temperature.Chiller;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProberInterfaces.Temperature.TempManager
{
    public interface ITempManager : IFactoryModule, IDisposable, INotifyPropertyChanged
                                    , IParamNode, IModule, IHasSysParameterizable
    {
        ITempModule TempModule { get; }

        ITempCommInfoParam TempCommInfoParam { get; }
        Dictionary<double, double> Dic_HeaterOffset { get; }
        Dictionary<double, ITCParamArgs> Dic_TCParam { get; }
        bool CheckingTCTempTable { get; }
        double PV { get; }
        double SV { get; }
        double MV { get; }
        double mSetPoint { get; set; }
        bool ChangeEnable { get; }

        bool Connect(string serialPort, byte UnitIdentifier);
        void Disconnect();

        EventCodeEnum StartModule();
        //void UpdateProc();

        double ReadMV();
        double ReadPV();
        double ReadSetV();
        long ReadStatus();
        long Get_OutPut_State();
        void SetRemote_OFF(object notUsed);
        void SetRemote_ON(object notUsed);
        void SetTempWithOption(double value); //Set Temp with PID, Offset
        void Set_SV(double value); //Only Setting SV
        void Set_PB(double value);
        void Set_IT(double value);
        void Set_DT(double value);
        void Set_Offset(double value);
        void Set_OutPut_OFF(object notUsed);
        void Set_OutPut_ON(object notUsed);
        void SaveTCGainCurtemp(double mSVPb, double mSViT, double mSVdE);
        void SaveTCGainSVtemp(int SVTemp, double mSVPb, double mSViT, double mSVdE);
        void ClearInputAlarm();
        void TempManagerStateTransition(TempManagerState state);
        double GetTempOffset(double temperature);

        IPID LoadTCGainSVtemp(double tmpsv);

        //For Emul Mode
        void ChangeCurTemp(double temp);
    }

    public interface ITempUpdateEventArgs
    {
        double PV { get; set; }
        double SV { get; set; }
        double MV { get; set; }
    }

    public interface ITempCommInfoParam
    {
        Element<bool> IsAttached { get; set; }

        Element<string> SerialPort { get; set; }

        Element<byte> Unitidentifier { get; set; }

        Element<bool> Init_WriteEnable { get; set; }

        Element<int> GetCurTempLoopTime { get; set; }

        Element<int> HCType { get; set; }

        Element<int> SetTemp { get; set; }
    }

    public interface ITCParamArgs
    {
        Element<double> Pb { get; set; }
        Element<double> iT { get; set; }
        Element<double> dE { get; set; }
    }

    public interface ITempModule : IDisposable, IFactoryModule
    {

        bool Connect(string serialPort, byte UnitIdentifier);
        ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0);
        double ReadMV();
        double ReadPV();
        double ReadSetV();
        long ReadStatus();
        long Get_OutPut_State();
        void SetRemote_OFF(object notUsed);
        void SetRemote_ON(object notUsed);
        void SetSV(double value);
        void Set_PB(double value);
        void Set_IT(double value);
        void Set_DT(double value);
        void Set_Offset(double value);
        void Set_OutPut_OFF(object notUsed);
        void Set_OutPut_ON(object notUsed);
        void WriteSingleRegister(int data, int value);

        //For Emul
        void ChangeCurTemp(double temp);
    }

    public enum TempManagerStateEnum
    {
        DISCONNECTED = 0,
        DISCONNECTING,
        CONN_WRT_ENABLE,
        CONN_WRT_DISABLE,
        ERROR
    }

    public interface TempManagerState
    {
        TempManagerStateEnum GetState();
        ITempUpdateEventArgs Get_Cur_Temp(int dataGetdelay = 0);
        bool Connect(string serialPort, byte UnitIdentifier);
        void Disconnect();
        void Dispose();
        double ReadMV();
        double ReadPV();
        double ReadSetV();
        long ReadStatus();
        long Get_OutPut_State();
        void SetRemote_OFF(object notUsed);
        void SetRemote_ON(object notUsed);
        void SetSV(double value);
        void Set_DT(double value);
        void Set_IT(double value);
        void Set_Offset(double value);
        void Set_OutPut_OFF(object notUsed);
        void Set_OutPut_ON(object notUsed);
        void Set_PB(double value);
        void WriteSingleRegister(int data, int value);
    }


    //public interface ITempCon
    //{
    //    long MV { get; set; }
    //    long PV { get; set; }
    //    long SV { get; set; }


    //    void Connect();
    //    void Disconnect();
    //    void Dispose();
    //    long Get_Cur_Temp();
    //    long ReadMV();
    //    long ReadPV();
    //    long ReadSetV();
    //    long ReadStatus();
    //    void SetRemote_OFF(object notUsed);
    //    void SetRemote_ON(object notUsed);
    //    void SetSV(double value);
    //    void Set_DT(string value);
    //    void Set_IT(string value);
    //    void Set_Offset(string value);
    //    void Set_OutPut_OFF(object notUsed);
    //    void Set_OutPut_ON(object notUsed);
    //    void Set_PB(string value);
    //}
}
