using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Security.Policy;
using System.Diagnostics;

namespace EMotionE84Manager
{
    class E84Device
    {
        #region Define E84 API

        public enum e84ErrorCode
        {
            E84_ERROR_SUCCESS           = 0,
            E84_ERROR_NOT_INITIALIZED   = 1,
            E84_ERROR_NOT_FOUND         = 2,
            E84_ERROR_WRONG_PARAMETER   = 8,
            E84_ERROR_TIME_OUT          = 14,
            E84_ERROR_WRONG_RETURN      = 15,
            E84_ERROR_DISCONNECT        = 50,
            E84_ERROR_DISCONNECTING     = 51,
            E84_ERROR_CONNECTED         = 1003,
        }

        /*** Environment ***/

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Connection(int NetId, int set, out int get, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_OS_Version(int NetId, out int osVersion, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Dll_Version(int NetId, out int dllVersion, out int status);

        /*** Log ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Is_Loggable(int NetId, [MarshalAs(UnmanagedType.I1)] out bool loggable, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Enable_Log(int NetId, [MarshalAs(UnmanagedType.I1)] bool enable, out int status, StringBuilder logPath);

        /*** Mode ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Mode_Auto(int NetId, int mode, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Mode_Auto(int NetId, out int mode, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Aux_Options(int NetId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Aux_Options(int NetId, out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Reverse_Signal(int NetId, int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Reverse_Signal(int NetId, out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Aux_Output_Signal(int NetId, int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Aux_Output_Signal(int NetId, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Aux_Signals(int NetId, out int auxInputs, out int auxOutput0, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Use_LP1(int NetId, int use, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Use_LP1(int NetId, out int use, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Clamp(int NetId, int use, int inputType, int actionType, int timer, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Clamp(int NetId, out int use, out int inputType, out int actionType, out int timer, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Clamp_Signal(int NetId, int loadPortNo, int clampOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Clamp_Signal(int NetId, int loadPortNo, out int clampOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Carrier_Signal(int NetId, int loadPortNo, int carrierExist, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Carrier_Signal(int NetId, int loadPortNo, out int carrierExist, out int status);

        /*** E84 Interface ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Reset_E84_Interface(int NetId, int reset, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_E84_Signals(int NetId, out int inputSignals, out int outputSignals, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_E84_Signal_Options(int NetId, int useCs1, int readyOff, int validOn, int validOff, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_E84_Signal_Options(int NetId, out int useCs1, out int readyOff, out int validOn, out int validOff, out int status);
        
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_E84_Signal_Out_Options(int NetId, int sigHoAvbl, int sigReq, int sigReady, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_E84_Signal_Out_Options(int NetId, out int sigHoAvbl, out int sigReq, out int sigReady, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_LightCurtain_Signal_Options(int NetId, int useHoAvbl, int useEs, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_LightCurtain_Signal_Options(int NetId, out int useHoAvbl, out int useEs, out int status);
                
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_HO_AVBL_Signal(int NetId, int hoAvblOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_HO_AVBL_Signal(int NetId, out int hoAvblOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_ES_Signal(int NetId, int esSignal, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_ES_Signal(int NetId, out int esSignal, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_e84_Signal_Out_Index(int NetId, int signalIndex, int signalOn, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_e84_Signal_Out_Index(int NetId, int signalIndex, out int signalOn, out int status);
        
        /*** Timer Options ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Input_Filter_Time(int NetId, int inputFilterTime, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Input_Filter_Time(int NetId, out int inputFilterTime, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_TP_Timeout(int NetId, int tp1, int tp2, int tp3, int tp4, int tp5, int tp6, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_TP_Timeout(int NetId, out int tp1, out int tp2, out int tp3, out int tp4, out int tp5, out int tp6, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_TD_DelayTime(int NetId, int td0, int td1, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_TD_DelayTime(int NetId, out int td0, out int td1, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Heartbeat_Time(int NetId, int heartBeat, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Heartbeat_Time(int NetId, out int heartBeat, out int status);


        /*** Event code ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Current_Status(int NetId, out int controllerStatus, out int sequenceStep, out int sequenceSub, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Get_Report_All_Status(int NetId, out int controllerStatus, out int sequenceStep, out int sequenceSub, out int e84Inputs, out int e84Outputs, out int auxInputs, out int auxOutputs, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Set_Clear_Event(int NetId, int clear, out int status);

        /*** Communication Config ***/
        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Set_Communication(int NetId, int timeOut, int retry, out int status);

        [DllImport("EMotionE84Device.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void e84_Config_Get_Communication(int NetId, out int timeOut, out int retry, out int status);

        #endregion

        #region Define
        /// <summary>
        /// Communication status
        /// </summary>
        public enum E84ComStatus
        {
            DISCONNECTED = 0,
            CONNECTED = 1,
            CONNECTING = 2,
        }

        public enum E84ComType
        {
            SERIAL = 0,
            UDP = 2,
        }

        public enum E84Mode
        {
            MANUAL = 0,
            AUTO = 1,
        }

        public enum E84Steps
        {
            WAIT_HO_AVBL_ON                 = 0,
            WAIT_CS_0_OR_CS_1_ON            = 1,
            WAIT_VALID_ON                   = 2,
            WAIT_TR_REQ_ON                  = 3,
            WAIT_CLAMP_OFF                  = 4,
            WAIT_BUSY_ON                    = 5,
            WAIT_CHANGING_CARRIER_STATUS    = 6,
            WAIT_BUSY_OFF                   = 7,
            WAIT_TR_REQ_OFF                 = 8,
            WAIT_COMPT_ON                   = 9,
            WAIT_VALID_OFF                  = 10,
            WAIT_COMPT_OFF                  = 11,
            WAIT_CS_0_OR_CS_1_OFF           = 12,
        }

        public enum E84SubSteps
        {
            INITITAL_STATUS                 = 0,
            AFTER_L_REQ_ON_TO_OHT           = 1,
            AFTER_CS_0_OR_CS_1_OFF_FROM_OHT = 2,
            AFTER_UL_REQ_ON_TO_OHT          = 3,
        }

        public enum E84SignalComType
        {
            WIRED_SENSOR = 0,
            PC_COMMAND   = 1,
        }

        public enum E84SignalInputIndex
        {
            VALID   = 0,
            CS_0    = 1,
            CS_1    = 2,
            AM_AVBL = 3,
            TR_REQ  = 4,
            BUSY    = 5,
            COMPT   = 6,
            CONT    = 7,
            GO      = 8,
        }

        public enum E84SignalOutputIndex
        {
            L_REQ   = 0,
            UL_REQ  = 1,
            VA      = 2,
            READY   = 3,
            VS_0    = 4,
            VS_1    = 5,
            HO_AVBL = 6,
            ES      = 7,
            SELECT  = 8,
            MODE    = 9,
        }

        public enum E84MaxCount
        {
            E84_MAX_AUX_INPUT = 6,
            E84_MAX_AUX_OUTPUT  = 1,
            E84_MAX_AUX_IO      = E84_MAX_AUX_INPUT + E84_MAX_AUX_OUTPUT,
            E84_MAX_E84_INPUT   = 9,
            E84_MAX_E84_OUTPUT  = 10,
            E84_MAX_TIMER_TP    = 6,
            E84_MAX_TIMER_TD    = 2,
            E84_MAX_LOAD_PORT   = 2,
        }

        int NetId = 0;
        E84ComType ComType = E84ComType.UDP;

        #endregion

        /// <summary>
        /// Connect to controller by UDP
        /// </summary>
        /// <param name="NetId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Connect(int netId, out int data)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            int isConnect = 1;

            try
            {
                e84_Connection(NetId, isConnect, out data, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    Debug.WriteLine("E84Device.Connect", $"Failed to connect device (Network ID: {netId.ToString()})", 0);
                }
                else
                    NetId = netId; 
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.Connect", ex.Message, 0);
                returnValue = -1;
                data = 0;
            }

            return returnValue;
        }

        /// <summary>
        /// Disconnect to controller
        /// </summary>
        /// <returns></returns>
        public int Disconnect()
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            int data;
            int isConnect = 0;

            try
            {
                e84_Connection(NetId, isConnect, out data, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    Debug.WriteLine("E84Device.Connect", $"Failed to disconnect device (Network ID: {NetId.ToString()})", 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.Connect", ex.Message, 0);
                returnValue = -1;
            }
            return returnValue;
        }

        /// <summary>
        /// Get to be loggable from API
        /// </summary>
        /// <param name="loggable">Whether the API is loggable or not loggable</param>
        /// <returns>Error Code</returns>
        public int IsLoggable(out bool loggable)
        {
            int returnValue = 0;

            try
            {
                e84_Is_Loggable(NetId, out loggable, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    Debug.WriteLine("E84Device.IsLoggable",
                            $"Failed to get to be loggable from device (Network ID:{NetId.ToString()})", 0);
                }
            }
            catch (Exception ex)
            {
                loggable = false;
                Debug.WriteLine("E84Device.IsLoggable", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Enable to be loggable to library
        /// </summary>
        /// <param name="enable">Loggable to file</param>
        /// <param name="logPath">Log path</param>
        /// <returns>Error Code</returns>
        public int EnableLog(bool enable, StringBuilder logPath)
        {
            int returnValue = 0;

            try
            {
                e84_Enable_Log(NetId, enable,out returnValue, logPath);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.EnableLog",
                            $"Failed to enable to be loggable to device (Serial, Path: {logPath.ToString()})", 0);
                    else
                        Debug.WriteLine("E84Device.EnableLog",
                            $"Failed to enable to be loggable to device (Network ID: {NetId.ToString()}, Path: {logPath.ToString()})",
                            0);
                }                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.EnableLog", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get OS version from driver
        /// </summary>
        /// <param name="version">
        /// </param>
        /// <returns>Error Code</returns>
        public int GetOsVersion(out int version)
        {
            int returnValue = 0;

            try
            {
                e84_Get_OS_Version(NetId, out version, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetOsVersion",
                            $"Failed to get version from device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetOsVersion",
                            $"Failed to get version from device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                version = 0;
                Debug.WriteLine("E84Device.GetOsVersion", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set E84 Mode
        /// </summary>
        /// <param name="mode">
        /// 0 : Manual <br/>
        /// 1 : Auto
        /// </param>
        /// <returns>Error Code</returns>
        public int SetMode(int NetId, int mode)
        {
            int returnValue = 0;

            try
            {
                e84_Set_Mode_Auto(NetId, mode, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetMode",
                            $"Failed to set mode of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetMode",
                            $"Failed to set mode of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetMode", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get E84 Mode
        /// </summary>
        /// <param name="mode">
        /// 0 : Manual <br/>
        /// 1 : Auto
        /// </param>
        /// <returns>Error Code</returns>
        public int GetMode(out int mode)
        {
            int returnValue = 0;

            try
            {
                e84_Get_Mode_Auto(NetId, out mode, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetMode",
                            $"Failed to get mode of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetMode",
                            $"Failed to get mode of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                mode = 0;
                Debug.WriteLine("E84Device.GetMode", ex.Message, 0);
                returnValue = -1;
            }


            return returnValue;
        }

        /// <summary>
        /// Set aux options (purpose of aux I/O)
        /// </summary>
        /// <param name="auxInput0">
        /// Carrier exist status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput1">
        /// Carrier exist status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput2">
        /// Auxiliary Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as auxiliary light curtain signal
        /// </param>
        /// <param name="auxInput3">
        /// Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as light curtain signal
        /// </param>
        /// <param name="auxInput4">
        /// Clamp status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput5">
        /// Clamp status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxOutput0">
        /// 0 : not used <br/>
        /// 1 : Use as General Purpose Output
        /// </param>
        /// <param name="status"></param>
        /// <returns> error code </returns>
        public int SetAuxOptions(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Set_Aux_Options(NetId, auxInput0, auxInput1, auxInput2, auxInput3, auxInput4, auxInput5, auxOutput0, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetAuxOptions",
                            $"Failed to set aux options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetAuxOptions",
                            $"Failed to set aux options of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetAuxOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get aux options (purpose of aux I/O)
        /// </summary>
        /// <param name="auxInput0">
        /// Carrier exist status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput1">
        /// Carrier exist status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput2">
        /// Auxiliary Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as auxiliary light curtain signal
        /// </param>
        /// <param name="auxInput3">
        /// Light Curtain signal <br/>
        /// 0 : not used <br/>
        /// 1 : use as light curtain signal
        /// </param>
        /// <param name="auxInput4">
        /// Clamp status of LP0 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxInput5">
        /// Clamp status of LP1 <br/>
        /// 0 : use host command <br/>
        /// 1 : use physically connected sensor
        /// </param>
        /// <param name="auxOutput0">
        /// 0 : not used <br/>
        /// 1 : Use as General Purpose Output
        /// </param>
        /// <param name="status"></param>
        /// <returns> error code </returns>
        public int GetAuxOptions(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_Aux_Options(NetId, out auxInput0, out auxInput1, out auxInput2, out auxInput3, out auxInput4, out auxInput5, out auxOutput0, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetAuxOptions",
                            $"Failed to get aux options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetAuxOptions",
                            $"Failed to get aux options of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                auxInput0 = 0;
                auxInput1 = 0;
                auxInput2 = 0;
                auxInput3 = 0;
                auxInput4 = 0;
                auxInput5 = 0;
                auxOutput0 = 0;
                Debug.WriteLine("E84Device.GetMode", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Reverse recognition of aux I/O <br/><br/>
        /// 0 : recognize low as low and high as high <br/>
        /// 1 : recognize low as high and high as low <br/>
        /// </summary>
        /// <param name="auxInput0"></param>
        /// <param name="auxInput1"></param>
        /// <param name="auxInput2"></param>
        /// <param name="auxInput3"></param>
        /// <param name="auxInput4"></param>
        /// <param name="auxInput5"></param>
        /// <param name="auxOutput0"></param>
        /// <returns>error code</returns>
        public int SetAuxReverseOption(int auxInput0, int auxInput1, int auxInput2, int auxInput3, int auxInput4, int auxInput5, int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Set_Reverse_Signal(NetId, auxInput0, auxInput1, auxInput2, auxInput3, auxInput4, auxInput5, auxOutput0, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetAuxReverseOption",
                            $"Failed to set aux reverse options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetAuxReverseOption",
                            $"Failed to set aux reverse options of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetAuxReverseOption", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check reverse recognition option of aux I/O <br/><br/>
        /// 0 : recognize low as low and high as high <br/>
        /// 1 : recognize low as high and high as low
        /// </summary>
        /// <param name="auxInput0"></param>
        /// <param name="auxInput1"></param>
        /// <param name="auxInput2"></param>
        /// <param name="auxInput3"></param>
        /// <param name="auxInput4"></param>
        /// <param name="auxInput5"></param>
        /// <param name="auxOutput0"></param>
        /// <returns>error code</returns>
        public int GetAuxReverseOption(out int auxInput0, out int auxInput1, out int auxInput2, out int auxInput3, out int auxInput4, out int auxInput5, out int auxOutput0)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_Reverse_Signal(NetId, out auxInput0, out auxInput1, out auxInput2, out auxInput3, out auxInput4, out auxInput5, out auxOutput0, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetAuxReverseOption",
                            $"Failed to get aux reverse options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetAuxReverseOption",
                            $"Failed to get aux reverse options of device (Network ID: {NetId.ToString()})", 0);
                }
            }
            catch (Exception ex)
            {
                auxInput0 = 0;
                auxInput1 = 0;
                auxInput2 = 0;
                auxInput3 = 0;
                auxInput4 = 0;
                auxInput5 = 0;
                auxOutput0 = 0;
                Debug.WriteLine("E84Device.GetAuxReverseOption", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set aux output0 <br/>
        /// 0 : off <br/>
        /// 1 : on
        /// </summary>
        /// <param name="output0">
        /// </param>
        /// <returns>Error Code</returns>
        public int SetOutput0(int output0)
        {
            int returnValue = 0;

            try
            {
                e84_Set_Aux_Output_Signal(NetId, output0, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetOutput0",
                            $"Failed to set output0 of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetOutput0",
                            $"Failed to set output0 of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetOutput0", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get aux output0 
        /// </summary>
        /// <param name="output0">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>Error Code</returns>
        public int GetOutput0(out int output0)
        {
            int returnValue = 0;

            try
            {
                e84_Get_Aux_Output_Signal(NetId, out output0, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetOutput0",
                            $"Failed to get output0 of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetOutput0",
                            $"Failed to get output0 of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                output0 = 0;
                Debug.WriteLine("E84Device.GetOutput0", ex.Message, 0);
                returnValue = -1;
            }


            return returnValue;
        }

        /// <summary>
        /// Get aux signals input, output
        /// </summary>
        /// <param name="inputSignals">
        /// bit 0 : aux input 0 (0 : off, 1: on) <br/>
        /// bit 1 : aux input 1 (0 : off, 1: on) <br/>
        /// bit 2 : aux input 2 (0 : off, 1: on) <br/>
        /// bit 3 : aux input 3 (0 : off, 1: on) <br/>
        /// bit 4 : aux input 4 (0 : off, 1: on) <br/>
        /// bit 5 : aux input 5 (0 : off, 1: on) <br/>
        /// </param>
        /// <param name="outputSignals"> aux output 0</param>
        /// <returns>error code </returns>
        public int GetAuxSignals(out int inputSignals, out int outputSignals)
        {
            int returnValue = 0;

            try
            {
                e84_Get_Aux_Signals(NetId, out inputSignals, out outputSignals, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetAuxSignals",
                            $"Failed to get aux signals of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetAuxSignals",
                            $"Failed to get aux signals of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                inputSignals = 0;
                outputSignals = 0;
                Debug.WriteLine("E84Device.GetAuxSignals", ex.Message, 0);
                returnValue = -1;
            }
            return returnValue;
        }

        /// <summary>
        /// Enable or disable load port 1
        /// </summary>
        /// <param name="use">
        /// 0 : disable <br/>
        /// 1 : enable
        /// </param>
        /// <returns> error code </returns>
        public int SetUseLp1(int use)
        {
            int returnValue = 0;
            
            try
            {
                e84_Config_Set_Use_LP1(NetId, use, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetUseLP1",
                            $"Failed to set use load port 1 of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetUseLp1",
                            $"Failed to set use load port 1 of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetUseLp1", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check if load port 1 is used 
        /// </summary>
        /// <param name="use">
        /// 0 : not used <br/>
        /// 1 : used
        /// </param>
        /// <returns> error code </returns>
        public int GetUseLp1(out int use)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Get_Use_LP1(NetId, out use, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetUseLp1",
                            $"Failed to check if load port 1 is used (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetUseLp1",
                            $"Failed to check if load port 1 is used (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                use = 0;
                Debug.WriteLine("E84Device.GetUseLp1", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Setting options of clamp  
        /// </summary>
        /// <param name="use">use or not use clamp<br/>
        /// 0 : use clamp<br/>
        /// 1 : not use clamp
        /// </param>
        /// <param name="inputType">pc command or wired sensor <br/>
        /// 0 : use signal from wired clamp sensor<br/>
        /// 1 : use clamp status command from host pc
        /// </param>
        /// <param name="actionType"> Decide whether event is occured or not when clamp is used <br/> 
        /// 0 : When Clamp use option set, Before ‘READY’ on to OHT, if clamp is not off status, event occurs <br/>
        /// 1 : When Clamp use option set, Before ‘READY’ on to OHT, E84 Controller waits until clamp off status
        /// </param>
        /// <param name="timer">time to wait for clamp off <br/>
        /// if clamp off status is not off during this time, Event will be happened
        /// </param>
        /// <returns> error code </returns>
        public int SetClampOptions(int use, int inputType, int actionType, int timer)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;
            
            try
            {
                e84_Config_Set_Clamp(NetId, use, inputType, actionType, timer, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetClampOptions",
                            $"Failed to set clamp options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetClampOptions",
                            $"Failed to set clamp options of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetClampOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check options of clamp  
        /// </summary>
        /// <param name="use">use or not use clamp<br/>
        /// 0 : use clamp<br/>
        /// 1 : not use clamp
        /// </param>
        /// <param name="inputType">pc command or wired sensor <br/>
        /// 0 : use signal from wired clamp sensor<br/>
        /// 1 : use clamp status command from host pc
        /// </param>
        /// <param name="actionType"> Decide whether event is occured or not when clamp is used <br/> 
        /// 0 : When Clamp use option set, Before ‘READY’ on to OHT, if clamp is not off status, event occurs <br/>
        /// 1 : When Clamp use option set, Before ‘READY’ on to OHT, E84 Controller waits until clamp off status
        /// </param>
        /// <param name="timer">time to wait for clamp off <br/>
        /// if clamp off status is not off during this time, Event will be happened
        /// </param>
        /// <returns> error code </returns>
        public int GetClampOptions(out int use, out int inputType, out int actionType, out int timer)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_Clamp(NetId, out use, out inputType, out actionType, out timer, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetClampOptions",
                            $"Failed to get clamp options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetClampOptions",
                            $"Failed to get clamp options of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                use = 0;
                inputType = 0;
                actionType = 0;
                timer = 0;
                Debug.WriteLine("E84Device.GetClampOptions", ex.Message, 0);
                returnValue = -1;
            }
            return returnValue;
        }

        /// <summary>
        /// on or off clamp signal
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="clampOn">on/off clamp signal <br/>
        /// 0 : off
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int SetClampSignal(int loadPortNo, int clampOn)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Set_Clamp_Signal(NetId, loadPortNo, clampOn, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetClampSignal",
                            $"Failed to set clamp signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetClampSignal",
                            $"Failed to set clamp signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetClampSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check if clamp signal is on
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="clampOn">on/off clamp signal <br/>
        /// 0 : off
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int GetClampSignal(int loadPortNo, out int clampOn)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Get_Clamp_Signal(NetId, loadPortNo, out clampOn, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetClampSignal",
                            $"Failed to get clamp signal status of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetClampSignal",
                            $"Failed to get clamp signal status of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                loadPortNo = 0;
                clampOn = 0;
                Debug.WriteLine("E84Device.GetClampSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// setting carrier exist signal
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="carrierExist">on/off clamp signal <br/>
        /// 0 : There is not carrier on the load port <br/>
        /// 1 : There is carrier on the load port
        /// </param>
        /// <returns>error code</returns>
        public int SetCarrierSignal(int loadPortNo, int carrierExist)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Set_Carrier_Signal(NetId, loadPortNo, carrierExist, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetCarrierSignal",
                            $"Failed to set carrier signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetCarrierSignal",
                            $"Failed to set carrier signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetCarrierSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check if carrier exist
        /// </summary>
        /// <param name="loadPortNo">load port number of clamp</param>
        /// <param name="carrierExist">on/off clamp signal <br/>
        /// 0 : There is not carrier on the load port <br/>
        /// 1 : There is carrier on the load port
        /// </param>
        /// <returns></returns>
        public int GetCarrierSignal(int loadPortNo, out int carrierExist)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Get_Clamp_Signal(NetId, loadPortNo, out carrierExist, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetCarrierSignal",
                            $"Failed to check carrier of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetCarrierSignal",
                            $"Failed to check carrier of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                carrierExist = 0;
                Debug.WriteLine("E84Device.GetCarrierSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Reset E84 output signals and Initialize sequence interface
        /// </summary>
        /// <param name="reset">reset or not </br>
        /// 0 : no action <br/>
        /// 1 : reset E84 interface
        /// </param>
        /// <returns>error code</returns>
        public int ResetE84Interface(int reset)
        {
            int returnValue = 0;

            try
            {
                e84_Reset_E84_Interface(NetId, reset, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetUseLP1",
                            $"Failed to reset interface of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetUseLp1",
                            $"Failed to reset interface of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.ResetE84Interface", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get E84 input signals and output signals
        /// </summary>
        /// <param name="inputSignals"> input signals (0 : off, 1: on)<br/>
        /// Bit 0 : VALID <br/>
        /// Bit 1 : CS_0 <br/>
        /// Bit 2 : CS_1 <br/>
        /// Bit 3 : AM_AVBL <br/>
        /// Bit 4 : TR_REQ <br/>
        /// Bit 5 : BUSY <br/>
        /// Bit 6 : COMPT <br/>
        /// Bit 7 : CONT <br/>
        /// Bit 8 : GO
        /// </param>
        /// <param name="outputSignals"> output signals <br/>
        /// Bit 0 : L_REQ <br/>
        /// Bit 1 : UL_REQ <br/>
        /// Bit 2 : VA <br/>
        /// Bit 3 : READY <br/>
        /// Bit 4 : VS_0 <br/>
        /// Bit 5 : VS_1 <br/>
        /// Bit 6 : HO_AVBL <br/>
        /// Bit 7 : ES <br/>
        /// Bit 8 : SELECT <br/>
        /// Bit 9 : MODE
        /// </param>
        /// <returns>error code</returns>
        public int GetE84Signals(out int inputSignals, out int outputSignals)
        {
            int returnValue = 0;

            try
            {
                e84_Get_E84_Signals(NetId, out inputSignals, out outputSignals, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetE84Signals",
                            $"Failed to get E84 signals of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetE84Signals",
                            $"Failed to get E84 signals of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                inputSignals = 0;
                outputSignals = 0;
                Debug.WriteLine("E84Device.GetE84Signals", ex.Message, 0);
                returnValue = -1;
            }
            return returnValue;
        }

        /// <summary>
        /// Set e84 signal options
        /// </summary>
        /// <param name="useCs1">use or not use cs1 <br/>
        /// 0 : not use CS1 <br/>
        /// 1 : use CS1
        /// </param>
        /// <param name="readyOff">check option of sequence BUSY off -> TR_REQ off -> COMPT on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOn">check option of sequence CS on -> VALID on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOff">check option of sequence VALID off -> COMPT off -> CS off <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/> 
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <returns>error code</returns>
        public int SetE84SignalOptions(int useCs1, int readyOff, int validOn, int validOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Set_E84_Signal_Options(NetId, useCs1, readyOff, validOn, validOff, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetE84SignalOptions",
                            $"Failed to set clamp options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetE84SignalOptions",
                            $"Failed to set clamp options of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetE84SignalOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get e84 signal options
        /// </summary>
        /// <param name="useCs1">use or not use cs1 <br/>
        /// 0 : not use CS1 <br/>
        /// 1 : use CS1
        /// </param>
        /// <param name="readyOff">check option of sequence BUSY off -> TR_REQ off -> COMPT on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOn">check option of sequence CS on -> VALID on <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/>
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <param name="validOff">check option of sequence VALID off -> COMPT off -> CS off <br/>
        /// 0 : if signals does not follow sequence, Event is occured <br/> 
        /// 1 : don't mind sequence. Controller will go to next step
        /// </param>
        /// <returns>error code</returns>
        public int GetE84SignalOptions(out int useCs1, out int readyOff, out int validOn, out int validOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_E84_Signal_Options(NetId, out useCs1, out readyOff, out validOn, out validOff, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetE84SignalOptions",
                            $"Failed to set clamp options of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetE84SignalOptions",
                            $"Failed to set clamp options of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                useCs1 = 0;
                readyOff = 0;
                validOn = 0;
                validOff = 0;
                Debug.WriteLine("E84Device.GetE84SignalOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// link signal to light curtain status
        /// </summary>
        /// <param name="hoAvbl"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is HO_AVBL when light curatin status occured
        /// </param>
        /// <param name="es"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is ES when light curatin status occured
        /// </param>
        /// <returns></returns>
        public int SetLightCurtainSignalOptions(int hoAvbl, int es)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Set_LightCurtain_Signal_Options(NetId, hoAvbl, es, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetLightCurtainSignalOptions",
                            $"Failed to set carrier signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetLightCurtainSignalOptions",
                            $"Failed to set carrier signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetLightCurtainSignalOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// check if signals are linked to light curtain status
        /// </summary>
        /// <param name="hoAvbl"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is HO_AVBL when light curatin status occured
        /// </param>
        /// <param name="es"> link or unlink hoavbl to light curtain status <br/>
        /// 0 : not use <br/>
        /// 1 : linked action signal is ES when light curatin status occured
        /// </param>
        /// <returns></returns>
        public int GetLightCuratinSignalOptions(out int hoAvbl, out int es)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_LightCurtain_Signal_Options(NetId, out hoAvbl, out es, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetLightCuratinSignalOptions",
                            $"Failed to check light curtain options (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetLightCuratinSignalOptions",
                            $"Failed to check light curtain options (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                hoAvbl = 0;
                es = 0;
                Debug.WriteLine("E84Device.GetLightCuratinSignalOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }
        public int SetE84SignalOutOptions(int sigHoAvbl, int sigReq, int sigReady)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Set_E84_Signal_Out_Options(NetId, sigHoAvbl, sigReq, sigReady, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetE84SignalOutOptions",
                            $"Failed to set carrier signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetE84SignalOutOptions",
                            $"Failed to set carrier signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetE84SignalOutOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        public int GetE84SignalOutOptions(int sigHoAvbl, int sigReq, int sigReady)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_E84_Signal_Out_Options(NetId, out sigHoAvbl, out sigReq, out sigReady, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetE84SignalOutOptions",
                            $"Failed to set carrier signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetE84SignalOutOptions",
                            $"Failed to set carrier signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.GetE84SignalOutOptions", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set HO_AVBL signal
        /// </summary>
        /// <param name="hoAvblOn">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int SetHoAvblSignal(int hoAvblOn)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Set_Use_LP1(NetId, hoAvblOn, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetHoAvblSignal",
                            $"Failed to set ho avbl signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetHoAvblSignal",
                            $"Failed to set ho avbl signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetHoAvblSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get HO_AVBL signal
        /// </summary>
        /// <param name="hoAvblOn">
        /// 0 : off
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int GetHoAvblSignal(out int hoAvblOn)
        {
            int returnValue = 0;

            try
            {
                e84_Get_HO_AVBL_Signal(NetId, out hoAvblOn, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetHoAvblSignal",
                            $"Failed to check ho avbl signal 1 is used (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetHoAvblSignal",
                            $"Failed to check ho avbl signal (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                hoAvblOn = 0;
                Debug.WriteLine("E84Device.GetHoAvblSignal", ex.Message, 0);
                returnValue = -1;
            }


            return returnValue;
        }

        /// <summary>
        /// Set Es signal
        /// </summary>
        /// <param name="esOn">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int SetEsSignal(int esOn)
        {
            int returnValue = 0;

            try
            {
                e84_Set_ES_Signal(NetId, esOn, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetEsSignal",
                            $"Failed to set ho avbl signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetEsSignal",
                            $"Failed to set ho avbl signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetEsSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get ES signal
        /// </summary>
        /// <param name="esOn">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns>error code</returns>
        public int GetEsSignal(out int esOn)
        {
            int returnValue = 0;

            try
            {
                e84_Get_ES_Signal(NetId, out esOn, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetEsSignal",
                            $"Failed to check es signal is used (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetEsSignal",
                            $"Failed to check es signal (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                esOn = 0;
                Debug.WriteLine("E84Device.GetEsSignal", ex.Message, 0);
                returnValue = -1;
            }


            return returnValue;
        }

        /// <summary>
        /// Set E84 output signal by index
        /// </summary>
        /// <param name="signalIndex">the index of E84 output signal to set
        ///  0 : L_REQ <br/>
        ///  1 : UL_REQ <br/>
        ///  2 : VA <br/>
        ///  3 : READY <br/>
        ///  4 : VS_0 <br/>
        ///  5 : VS_1 <br/>
        ///  6 : HO_AVBL <br/>
        ///  7 : ES <br/>
        ///  8 : SELECT <br/>
        ///  9 : MODE
        ///  </param>
        /// <param name="onOff">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns></returns>
        public int SetE84OutputSignal(int signalIndex, int onOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Set_e84_Signal_Out_Index(NetId, signalIndex, onOff, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetE84OutputSignal",
                            $"Failed to set E84 output signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetE84OutputSignal",
                            $"Failed to set E84 output signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetE84OutputSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check E84 output signal by index
        /// </summary>
        /// <param name="signalIndex">the index of E84 output signal to check
        ///  0 : L_REQ <br/>
        ///  1 : UL_REQ <br/>
        ///  2 : VA <br/>
        ///  3 : READY <br/>
        ///  4 : VS_0 <br/>
        ///  5 : VS_1 <br/>
        ///  6 : HO_AVBL <br/>
        ///  7 : ES <br/>
        ///  8 : SELECT <br/>
        ///  9 : MODE
        ///  </param>
        /// <param name="onOff">
        /// 0 : off <br/>
        /// 1 : on
        /// </param>
        /// <returns></returns>
        public int GetE84OutputSignal(int signalIndex, out int onOff)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Get_e84_Signal_Out_Index(NetId, signalIndex, out onOff, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetE84OutputSignal",
                            $"Failed to check E84 output signal of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetE84OutputSignal",
                            $"Failed to check E84 output signal of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                onOff = 0;
                Debug.WriteLine("E84Device.GetE84OutputSignal", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set input filtering time
        /// </summary>
        /// <param name="inputFilterTime">
        /// If signal on(or off) status is sustained during setting time, It is defined as on(or off) status (unit : 10msec)
        /// </param>
        /// <returns></returns>
        public int SetInputFilterTime(int inputFilterTime)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Set_Input_Filter_Time(NetId, inputFilterTime, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetInputFilterTime",
                            $"Failed to set input filter time of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetInputFilterTime",
                            $"Failed to set input filter time of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetInputFilterTime", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Check input filter time
        /// </summary>
        /// <param name="inputFilterTime">
        /// If signal on(or off) status is sustained during setting time, It is defined as on(or off) status (unit : 10msec)
        /// </param>
        /// <returns></returns>
        public int GetInputFilterTime(out int inputFilterTime)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Get_Input_Filter_Time(NetId, out inputFilterTime, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetInputFilterTime",
                            $"Failed to get input filter time is used (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetInputFilterTime",
                            $"Failed to get input filter time (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                inputFilterTime = 0;
                Debug.WriteLine("E84Device.GetInputFilterTime", ex.Message, 0);
                returnValue = -1;
            }


            return returnValue;
        }

        /// <summary>
        /// Set tick time of passive timer (unit : sec)
        /// </summary>
        /// <param name="tp1">passive timer 1 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp2">passive timer 2 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp3">passive timer 3 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp4">passive timer 4 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp5">passive timer 5 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <param name="tp6">passive timer 6 tick time(unit : sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time 
        /// </param>
        /// <returns></returns>
        public int SetTpTimeout(int tp1, int tp2, int tp3, int tp4, int tp5, int tp6)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Set_TP_Timeout(NetId, tp1, tp2, tp3, tp4, tp5, tp6, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetTpTimeout",
                            $"Failed to set tp time out of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetTpTimeout",
                            $"Failed to set tp time out of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetTpTimeout", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get tick time of passive timer (unit : 10 msec)
        /// </summary>
        /// <param name="tp1">passive timer 1 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp2">passive timer 2 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp3">passive timer 3 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp4">passive timer 4 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp5">passive timer 5 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <param name="tp6">passive timer 6 tick time(unit : 10msec)<br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param> 
        /// <returns></returns>
        public int GetTpTimeout(out int tp1, out int tp2, out int tp3, out int tp4, out int tp5, out int tp6)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_TP_Timeout(NetId, out tp1, out tp2, out tp3, out tp4, out tp5, out tp6, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetTpTimeout",
                            $"Failed to get tp time out of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetTpTimeout",
                            $"Failed to get tp time out of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                tp1 = 0;
                tp2 = 0;
                tp3 = 0;
                tp4 = 0;
                tp5 = 0;
                tp6 = 0;
                Debug.WriteLine("E84Device.GetTpTimeout", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set tick time of delay timer (unit : sec)
        /// </summary>
        /// <param name="td0">delay timer tick time (unit sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <param name="td1">delay timer tick time (unit sec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <returns></returns>
        public int SetTdDelayTime(int td0, int td1)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Set_TD_DelayTime(NetId, td0, td1, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetTdDelayTime",
                            $"Failed to set td delay time of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetLightCurtainSignalOptions",
                            $"Failed to set td delay time of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetTdDelayTime", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get tick time of delay timer (unit : 10msec)
        /// </summary>
        /// <param name="td0">delay timer tick time (unit 10msec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <param name="td1">delay timer tick time (unit 10msec) <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 999 : time
        /// </param>
        /// <returns></returns>
        public int GetTdDelayTime(out int td0, out int td1)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Config_Get_TD_DelayTime(NetId, out td0, out td1, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetTdDelayTime",
                            $"Failed to get td delay time of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetTdDelayTime",
                            $"Failed to get td delay time of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                td0 = 0;
                td1 = 0;
                Debug.WriteLine("E84Device.GetTdDelayTime", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Set heartbeat time <br/>
        /// </summary>
        /// <param name="heartBeatTime">
        /// this value decide minimum communication interval (unit : sec)
        /// If There are no communication within this time. Event will be happend <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 120 : time
        /// </param>
        /// <returns>error code</returns>
        public int SetHeartBeatTime(int heartBeatTime)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Set_Heartbeat_Time(NetId, heartBeatTime, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetHeartBeatTime",
                            $"Failed to set heartbeat time of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetHeartBeatTime",
                            $"Failed to set heartbeat time of device (Network ID: {NetId.ToString()})", 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetHeartBeatTime", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get heartbeat time <br/>
        /// </summary>
        /// <param name="heartBeatTime">
        /// this value decide minimum communication interval (unit : 10msec)
        /// If There are no communication within this time. Event will be happend <br/>
        /// 0 : not use this timer <br/>
        /// 1 ~ 120 : time
        /// </param>
        /// <returns>error code</returns>
        public int GetHeartBeatTime(out int heartBeatTime)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Get_Heartbeat_Time(NetId, out heartBeatTime, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetHeartBeatTime",
                            $"Failed to get input filter time is used (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetHeartBeatTime",
                            $"Failed to get input filter time (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                heartBeatTime = 0;
                Debug.WriteLine("E84Device.GetHeartBeatTime", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Get current status of device
        /// </summary>
        /// <param name="controllerStatus">
        /// 0 : manual <br/>
        /// 1 : auto <br/>
        /// above 2 : event number
        /// </param>
        /// <param name="sequenceStep">
        /// 0 : wait HO_AVBL <br/>
        /// 1 : wait CS_0 or CS_1 <br/>
        /// 2 : wait VALID on <br/>
        /// 3 : wait TR_REQ on <br/>
        /// 4 : wait CLAMP off <br/>
        /// 5 : wait BUSY on <br/>
        /// 6 : wait Changing LP Carrier Status <br/>
        /// 7 : wait BUSY off <br/>
        /// 8 : wait TR_REQ off <br/>
        /// 9 : wait COMPT on <br/>
        /// </param>
        /// <param name="sequenceSub">
        /// 0 : initial status <br/>
        /// 1 : after L_REQ On to OHT <br/>
        /// 2 : after CS_0 or CS_1 off from OHT <br/>
        /// 3 : after UL_REQ On to OHT <br/>
        /// </param>
        /// <returns></returns>
        public int GetCurrentStatus(out int controllerStatus, out int sequenceStep, out int sequenceSub)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Get_Current_Status(NetId, out controllerStatus, out sequenceStep, out sequenceSub, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetCurrentStatus",
                            $"Failed to get current status of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetCurrentStatus",
                            $"Failed to get current status of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                controllerStatus = 0;
                sequenceStep = 0;
                sequenceSub = 0;
                Debug.WriteLine("E84Device.GetCurrentStatus", ex.Message, 0);
                returnValue = -1;
            }
            return returnValue;
        }

        /// <summary>
        /// Get all status including I/O
        /// </summary>
        /// <param name="controllerStatus">
        /// 0 : manual <br/>
        /// 1 : auto <br/>
        /// above 2 : event number
        /// </param>
        /// <param name="sequenceStep">
        /// 0 : wait HO_AVBL <br/>
        /// 1 : wait CS_0 or CS_1 <br/>
        /// 2 : wait VALID on <br/>
        /// 3 : wait TR_REQ on <br/>
        /// 4 : wait CLAMP off <br/>
        /// 5 : wait BUSY on <br/>
        /// </param>
        /// <param name="sequenceSub">
        /// 0 : initial status <br/>
        /// 1 : after L_REQ On to OHT <br/>
        /// 2 : after CS_0 or CS_1 off from OHT <br/>
        /// 3 : after UL_REQ On to OHT <br/>
        /// </param> 
        /// <param name="e84Inputs">E84 input signals (0 : off, 1: on) <br/>
        /// Bit 0 : VALID <br/>
        /// Bit 1 : CS_0 <br/>
        /// Bit 2 : CS_1 <br/>
        /// Bit 3 : AM_AVBL <br/>
        /// Bit 4 : TR_REQ <br/>
        /// Bit 5 : BUSY <br/>
        /// Bit 6 : COMPT <br/>
        /// Bit 7 : CONT <br/>
        /// Bit 8 : GO
        /// </param>
        /// <param name="e84Outputs">E84 output signals (0 : off, 1: on) <br/>
        /// Bit 0 : L_REQ <br/>
        /// Bit 1 : UL_REQ <br/>
        /// Bit 2 : VA <br/>
        /// Bit 3 : READY <br/>
        /// Bit 4 : VS_0 <br/>
        /// Bit 5 : VS_1 <br/>
        /// Bit 6 : HO_AVBL <br/>
        /// Bit 7 : ES <br/>
        /// Bit 8 : SELECT <br/>
        /// Bit 9 : MODE
        /// </param>
        /// <param name="auxInputs">Aux input signals (0 : off, 1 : on) <br/>
        /// bit 0 : aux input 0 (0 : off, 1: on) <br/>
        /// bit 1 : aux input 1 (0 : off, 1: on) <br/>
        /// bit 2 : aux input 2 (0 : off, 1: on) <br/>
        /// bit 3 : aux input 3 (0 : off, 1: on) <br/>
        /// bit 4 : aux input 4 (0 : off, 1: on) <br/>
        /// bit 5 : aux input 5 (0 : off, 1: on) 
        /// </param>
        /// <param name="auxOutputs"> aux output 0 (0 : off, 1 : on)</param>
        /// <returns>error code</returns>
        public int GetAllStatus(out int controllerStatus, out int sequenceStep, out int sequenceSub, out int e84Inputs, out int e84Outputs, out int auxInputs, out int auxOutputs)
        {
            int returnValue = (int)e84ErrorCode.E84_ERROR_SUCCESS;

            try
            {
                e84_Get_Report_All_Status(NetId, out controllerStatus, out sequenceStep, out sequenceSub, out e84Inputs, out e84Outputs, out auxInputs, out auxOutputs, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetAllStatus",
                            $"Failed to get all status of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetAllStatus",
                            $"Failed to get all status of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                controllerStatus = 0;
                sequenceStep = 0;
                sequenceSub = 0;
                e84Inputs = 0;
                e84Outputs = 0;
                auxInputs = 0;
                auxOutputs = 0;
                Debug.WriteLine("E84Device.GetAllStatus", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        /// <summary>
        /// Clear event
        /// </summary>
        /// <param name="clear">
        /// 0 : no action
        /// 1 : clear
        /// </param>
        /// <returns></returns>
        public int SetClearEvent(int clear)
        {
            int returnValue = 0;

            try
            {
                e84_Set_Clear_Event(NetId, clear, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetHeartBeatTime",
                            $"Failed to set heartbeat time of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetHeartBeatTime",
                            $"Failed to set heartbeat time of device (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetHeartBeatTime", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        } 

        public int SetCommunication(int timeOut, int retry)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Set_Communication(NetId, timeOut, retry, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.SetCommunication",
                            $"Failed to set heartbeat time of device (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.SetCommunication",
                            $"Failed to set heartbeat time of device (Network ID: {NetId.ToString()})", 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("E84Device.SetCommunication", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }

        public int GetCommunication(out int timeOut, out int retry)
        {
            int returnValue = 0;

            try
            {
                e84_Config_Get_Communication(NetId, out timeOut, out retry, out returnValue);
                if (returnValue != (int)e84ErrorCode.E84_ERROR_SUCCESS)
                {
                    if (ComType == E84ComType.SERIAL)
                        Debug.WriteLine("E84Device.GetCommunication",
                            $"Failed to get input filter time is used (Serial)", 0);
                    else
                        Debug.WriteLine("E84Device.GetCommunication",
                            $"Failed to get input filter time (Network ID: {NetId.ToString()})",
                            0);
                }
            }
            catch (Exception ex)
            {
                timeOut = 0;
                retry = 0;
                Debug.WriteLine("E84Device.GetCommunication", ex.Message, 0);
                returnValue = -1;
            }

            return returnValue;
        }
    }
}
