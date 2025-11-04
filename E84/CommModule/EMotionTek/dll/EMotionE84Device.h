//
// EMotionUniDevice.h
//
//      Copyright (c) EMOTION Tek Co., Ltd. All rights reserved.
//
// Description:
//		The C, C++ Library for EMOTION E84 device API.
//
// Author:
//		Dev YSM
//
#pragma once



#define REQ_DATASIZE	32	// The length of echo request data

#define MAX_NET_ID	16
#define _NORMAL	0
#define _TIMEOUT	1
#define _DISCONNECT 2



#define	ipsnt_SUCCESS			0
#define	ipsnt_NOT_INITIALIZED	1
#define	ipsnt_NOT_FOUND			2
#define	ipsnt_WRONG_IN_PARA		8
#define	ipsnt_TIMEOUT_ERROR		14
#define	ipsnt_WRONG_RETURN		15
#define	ipsnt_DISCONNECT		50

#define ipsnt_NET_NOT_INITIALIZED		1000
#define	ipsnt_NET_NOT_FOUND				1001
#define	ipsnt_NET_WRONG_PARAMETER		1002


#define ipnet_SUCCESS				0
#define ipnet_WRONG_IN_PARA			8
#define ipnet_TIMEOUT_ERROR			14
#define ipnet_WRONG_RETURN			15
#define ipnet_DISCONNECT			50
#define ipnet_DISCONNECTING			51

#define ipnet_NOT_INITIALIZED		1000
#define ipnet_NOT_FOUND				1001
#define ipnet_WRONG_PARAMS			1002
#define ipnet_ALREADY_INITIALIZED	1003
#define ipnet_FAIL_RIO_COM			1005

#define ipnet_BUFFER_OVERFLOW	1010

#define ipnet_ERROR_ENVIRONMENT 1020

/*-------------------------------------------------------------------------------------------*/
/*** "C" Functions for EMOTION E84 API ***/
/*-------------------------------------------------------------------------------------------*/

/*** Environment ***/
extern "C" __declspec(dllexport) void e84_Connection(int netId, int set, int* get, int* status);
extern "C" __declspec(dllexport) void e84_Get_OS_Version(int netId, int* osVersion, int* status);
extern "C" __declspec(dllexport) void e84_Get_Dll_Version(int netId, int* dllVersion, int* status);

/*** Log ***/
extern "C" __declspec(dllexport) void e84_Is_Loggable(int netId, bool* loggable, int* status);
extern "C" __declspec(dllexport) void e84_Enable_Log(int netId, bool enable, int* status, const char* logPath = NULL);

/*** Mode ***/
extern "C" __declspec(dllexport) void e84_Set_Mode_Auto(int netId, int mode, int* status);
extern "C" __declspec(dllexport) void e84_Get_Mode_Auto(int netId, int* mode, int* status);

/*** Auxiliary I/O ***/
extern "C" __declspec(dllexport) void e84_Config_Set_Aux_Options(int netId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_Aux_Options(int netId, int* auxInput0, int* auxInput1, int* auxInput2, int* auxInput3, int* auxInput4, int* auxInput5, int* auxOutput0, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_Reverse_Signal(int netId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_Reverse_Signal(int netId, int* auxInput0, int* auxInput1, int* auxInput2, int* auxInput3, int* auxInput4, int* auxInput5, int* auxOutput0, int* status);
extern "C" __declspec(dllexport) void e84_Set_Aux_Output_Signal(int netId, int auxOutput0, int* status);
extern "C" __declspec(dllexport) void e84_Get_Aux_Output_Signal(int netId, int* auxOutput0, int* status);
extern "C" __declspec(dllexport) void e84_Get_Aux_Signals(int netId, int* auxInputs, int* auxOutput0, int* status);

/*** Load Port Status ***/
extern "C" __declspec(dllexport) void e84_Config_Set_Use_LP1(int netId, int use, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_Use_LP1(int netId, int* use, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_Clamp(int netId, int use, int inputType, int actionType, int timer, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_Clamp(int netId, int* use, int* inputType, int* actionType, int* timer, int* status);
extern "C" __declspec(dllexport) void e84_Set_Clamp_Signal(int netId, int loadPortNo, int clampOn, int* status);
extern "C" __declspec(dllexport) void e84_Get_Clamp_Signal(int netId, int loadPortNo, int* clampOn, int* status);
extern "C" __declspec(dllexport) void e84_Set_Carrier_Signal(int netId, int loadPortNo, int carrierExist, int* status);
extern "C" __declspec(dllexport) void e84_Get_Carrier_Signal(int netId, int loadPortNo, int* carrierExist, int* status);

/*** E84 Interface ***/
extern "C" __declspec(dllexport) void e84_Reset_E84_Interface(int netId, int reset, int* status);
extern "C" __declspec(dllexport) void e84_Get_E84_Signals(int netId, int* inputSignals, int* outputSignals, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_E84_Signal_Options(int netId, int useCs1, int readyOff, int validOn, int validOff, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_E84_Signal_Options(int netId, int* useCs1, int* readyOff, int* validOn, int* validOff, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_E84_Signal_Out_Options(int netId, int sigHoAvbl, int sigReq, int sigReady, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_E84_Signal_Out_Options(int netId, int* sigHoAvbl, int* sigReq, int* sigReady, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_LightCurtain_Signal_Options(int netId, int useHoAvbl, int useEs, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_LightCurtain_Signal_Options(int netId, int* useHoAvbl, int* useEs, int* status);
extern "C" __declspec(dllexport) void e84_Set_HO_AVBL_Signal(int netId, int hoAvblOn, int* status);
extern "C" __declspec(dllexport) void e84_Get_HO_AVBL_Signal(int netId, int* hoAvblOn, int* status);
extern "C" __declspec(dllexport) void e84_Set_ES_Signal(int netId, int esSignal, int* status);
extern "C" __declspec(dllexport) void e84_Get_ES_Signal(int netId, int* esSignal, int* status);
extern "C" __declspec(dllexport) void e84_Set_e84_Signal_Out_Index(int netId, int signalIndex, int signalOn, int* status);
extern "C" __declspec(dllexport) void e84_Get_e84_Signal_Out_Index(int netId, int signalIndex, int* signalOn, int* status);

/*** Timer Options ***/
extern "C" __declspec(dllexport) void e84_Config_Set_Input_Filter_Time(int netId, int inputFilterTime, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_Input_Filter_Time(int netId, int* inputFilterTime, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_TP_Timeout(int netId, int tp1, int tp2, int tp3, int tp4, int tp5, int tp6, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_TP_Timeout(int netId, int* tp1, int* tp2, int* tp3, int* tp4, int* tp5, int* tp6, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_TD_DelayTime(int netId, int td0, int td1, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_TD_DelayTime(int netId, int* td0, int* td1, int* status);
extern "C" __declspec(dllexport) void e84_Config_Set_Heartbeat_Time(int netId, int heartBeat, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_Heartbeat_Time(int netId, int* heartBeat, int* status);

/*** Event Code ***/
extern "C" __declspec(dllexport) void e84_Get_Current_Status(int netId, int* controllerStatus, int* sequenceStep, int* sequenceSub, int* status);
extern "C" __declspec(dllexport) void e84_Get_Report_All_Status(int netId, int* controllerStatus, int* sequenceStep, int* sequenceSub, int* e84Inputs, int* e84Outputs, int* auxInputs, int* auxOutputs, int* status);
extern "C" __declspec(dllexport) void e84_Set_Clear_Event(int netId, int clear, int* status);

/*** Communication Config ***/
extern "C" __declspec(dllexport) void e84_Config_Set_Communication(int netId, int timeOut, int retry, int* status);
extern "C" __declspec(dllexport) void e84_Config_Get_Communication(int netId, int* timeOut, int* retry, int* status);

class EMotionE84Device
{
public:
	EMotionE84Device();
	~EMotionE84Device();

public:
	//*** Environment ***/
	virtual int Connect(int netId, int port, int* get) = 0;
	virtual int Disconnect() = 0;
	virtual int GetOsVersion(int* osVersion) = 0;
	virtual int GetDllVersion(int* version) = 0;

	virtual bool IsLoggable() = 0;
	virtual int EnableLog(bool enable, const char* logPath = NULL) = 0;

	/*** Mode ***/
	virtual int SetModeAuto(int mode) = 0;
	virtual int GetModeAuto(int* mode) = 0;

	/*** Auxiliary I/O ***/
	virtual int SetAuxOptions(int* auxPoints, int pointCount) = 0;
	virtual int GetAuxOptions(int* auxPoints, int pointCount) = 0;
	virtual int SetReverseAuxSignals(int* auxPoints, int pointCount) = 0;
	virtual int GetReverseAuxSignals(int* auxPoints, int pointCount) = 0;
	virtual int SetAuxOutput(int auxOutput) = 0;
	virtual int GetAuxOutput(int* auxOutput) = 0;
	virtual int GetAuxSignals(int* auxInput, int* auxOutput) = 0;

	/*** Load Port ***/
	virtual int SetEnableLP1(int enable) = 0;
	virtual int GetEnableLP1(int* enable) = 0;
	virtual int SetClampOption(int* options, int optionCount) = 0;
	virtual int GetClampOption(int* optionsBuffer, int bufferSize) = 0;
	virtual int SetClampSignal(int loadPortNo, int clampOn) = 0;
	virtual int GetClampSignal(int loadPortNo, int* clampOn) = 0;
	virtual int SetCarrierSignal(int loadPortNo, int carrierExist) = 0;
	virtual int GetCarrierSignal(int loadPortNo, int* carrierExist) = 0;

	/*** Interface ***/
	virtual int ResetE84Interface(int reset) = 0;
	virtual int GetE84Signals(int* inputSignals, int* outputSignals) = 0;
	virtual int SetE84SignalOptions(int* options, int optionCount) = 0;
	virtual int GetE84SignalOptions(int* options, int optionCount) = 0;
	virtual int SetLightCurtainSignalOptions(int* options, int optionCount) = 0;
	virtual int GetLightCurtainSignalOptions(int* options, int optionCount) = 0;
	//virtual int SetE84SignalOutOptions(int sigHoAvbl, int sigReq, int sigReady) = 0;
	//virtual int GetE84SignalOutOptions(int* sigHoAvbl, int* sigReq, int* sigReady) = 0;
	virtual int SetE84SignalOutOptions(int* options, int optionCount) = 0;
	virtual int GetE84SignalOutOptions(int* options, int optionCount) = 0;
	virtual int SetHoAvblSignal(int hoAvblOn) = 0;
	virtual int GetHoAvblSignal(int* hoAvblOn) = 0;
	virtual int SetEsSignal(int hoAvblOn) = 0;
	virtual int GetEsSignal(int* hoAvblOn) = 0;
	virtual int SetE84OutputSignalByIndex(int signalIndex, int signalOn) = 0;
	virtual int GetE84OutputSignalByIndex(int signalIndex, int* signalOn) = 0;

	/*** Timer option ***/
	virtual int SetInputFilterTime(int filterTime) = 0;
	virtual int GetInputFilterTime(int* filterTime) = 0;
	virtual int SetTpTimeOut(int* timeOut, int timeOutCount) = 0;
	virtual int GetTpTimeOut(int* timeOut, int timeOutCount) = 0;
	virtual int SetTdDelayTime(int* delayTimes, int tdCount) = 0;
	virtual int GetTdDelayTime(int* delayTimes, int tdCount) = 0;
	virtual int SetHeartBeatTime(int heartBeatTime) = 0;
	virtual int GetHeartBeatTime(int* heartBeatTime) = 0;

	/*** Event Code ***/
	virtual int GetCurrentStatus(int* status) = 0;
	virtual int GetAllStatus(int* status, int statusBufferSize) = 0;
	virtual int SetClearEvent(int clear) = 0;

	/*** Communication Config ***/
	virtual int SetCommunication(int timeOut, int retry) = 0;
	virtual int GetCommunication(int* timeOut, int* retry) = 0;
};


/// <summary>
/// Create e84 device object
/// </summary>
/// <returns>	
/// The handle(pointer) of e84 device object to be created memory space 
/// If creating the object is failed, return NULL(0) 
/// </returns>
extern "C" __declspec(dllexport) EMotionE84Device* CreateE84Device();

/// <summary>
/// Destroy e84 device object 
/// </summary>
/// <param name="e84Device"></param>
extern "C" __declspec(dllexport) void DestroyE84Device(EMotionE84Device * e84Device);

