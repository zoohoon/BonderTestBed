using Autofac;
using Configurator;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET;
using ElmoMotionControl.GMAS.EASComponents.MMCLibDotNET.InternalArgs;
using ElmoMotionControlComponents.GMAS.MMCLibDotNET;
using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
////using ProberInterfaces.ThreadSync;

namespace ECATIO
{
    public class ECATIOProvider : IIOBase, ILightDeviceControl, ICameraChannelControl, INotifyPropertyChanged, IHasSysParameterizable
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private static int ByteCountPerModule = 2; // ÇÏ³ªÀÇ IO Module´ç ¸î ByteÀÇ µ¥ÀÌÅÍ¸¦ °¡Áö°í ÀÖ´ÂÁö¸¦ ÀÇ¹ÌÇÏ´Â »ó¼ö

        Thread UpdateThread;

        private bool _UseMemoryUpdate;

        ECATIODescripter EcatIODesc;
        int _ConnHndl;
        bool bStopUpdateThread;
        private short _DeviceNumber;
        public short DeviceNumber
        {
            get { return _DeviceNumber; }
            set
            {
                if (value != _DeviceNumber)
                {
                    _DeviceNumber = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<Channel> _Channels;
        public ObservableCollection<Channel> Channels
        {
            get { return _Channels; }
            set
            {
                if (value == _Channels) return;
                _Channels = value;
                RaisePropertyChanged();
            }
        }
        int DIGITAL_OUTPUT = 16;
        MMCBulkRead m_BulkRead;
        NC_BULKREAD_PRESET_5[] stReadBulkReadData;

        MMCPIBulkRead m_BulkReadPI;
        PI_BULKREAD_ENTRY BulkReadEntry;

        ushort[] nNodeList;
        public ObservableCollection<IECATIO> _MMCECATIOList;
        public ObservableCollection<IECATIO> MMCECATIOList
        {
            get { return _MMCECATIOList; }
            set
            {
                if (value == _MMCECATIOList) return;
                _MMCECATIOList = value;
                RaisePropertyChanged();
            }
        }
        private Dictionary<int, int> _AxisMapping = new Dictionary<int, int>();
        private bool _DevConnected;
        public bool DevConnected
        {
            get { return _DevConnected; }
            set { _DevConnected = value; }
        }

        public ECATIOProvider()
        {
            try
            {
                _UseMemoryUpdate = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        ~ECATIOProvider()
        {
            DeInitIO();
        }

        public int DeInitIO()
        {
            int retVal = -1;
            try
            {
                bStopUpdateThread = true;
                UpdateThread?.Join();

                DevConnected = false;

                retVal = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Error($"DeInitIO() Function error: " + err.Message);
            }
            return retVal;
        }
        public int InitIO(int devNum, ObservableCollection<Channel> channels)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            int initVal = 0;

            _ConnHndl = devNum;
            m_BulkReadPI = new MMCPIBulkRead(_ConnHndl, NC_BULKREAD_CONFIG_PI_ENUM.eBULKREAD_CONFIG_PI_2);

            try
            {
                _Channels = channels;

                retVal = LoadSysParameter();

                if (retVal == EventCodeEnum.NONE)
                {
                    initVal = InitializeController();
                }
                if (initVal == 0)
                {
                    DevConnected = true;
                }
                else
                {
                    DevConnected = false;
                    retVal = EventCodeEnum.PARAM_ERROR;
                }

                if (DevConnected)
                {
                    PrevOutputUpdate();
                    bStopUpdateThread = false;
                    UpdateThread = new Thread(new ThreadStart(UpdateIOProc));
                    UpdateThread.Start();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                initVal = -1;
            }

            return initVal;
        }
        public int InitializeController()
        {
            int nRetVal = 0;
            try
            {
                MMCNode nNodeInfo;
                MMCECATIOList = new ObservableCollection<IECATIO>();
                int cnt = 0;
                int count = 0;
                bool axisFlag = false;
                nNodeList = new ushort[EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.Where(item => item.NodeType == EnumIONodeType.Axis).Count()];
                stReadBulkReadData = new NC_BULKREAD_PRESET_5[EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.Where(item => item.NodeType == EnumIONodeType.Axis).Count()];
                foreach (ECATNodeDefinition eCatIO in EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions)
                {

                    if (eCatIO.NodeType == EnumIONodeType.IO)
                    {
                        MMCECATIOList.Add(new MMCECATIOBase(eCatIO.ID, _ConnHndl));
                        nNodeInfo = new MMCNode(eCatIO.ID, _ConnHndl);
                        BulkReadEntry.usAxisRef = nNodeInfo.AxisReference;

                        if (eCatIO.IOType == EnumIOType.INPUT || eCatIO.IOType == EnumIOType.AI)
                        {
                            BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_INPUT;
                        }
                        else if (eCatIO.IOType == EnumIOType.OUTPUT || eCatIO.IOType == EnumIOType.AO)
                        {
                            BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_OUTPUT;
                        }

                        if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.First().IOType != EnumIOType.MIXED)
                        {
                            //Beckhoff
                            if (eCatIO.IOType == EnumIOType.AI)
                            {
                                for (int i = 0; i < eCatIO.ChNum; i++)
                                {
                                    BulkReadEntry.usIndex = (ushort)i;
                                    m_BulkReadPI.AddEntry(nNodeInfo, BulkReadEntry);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < eCatIO.ChNum; i++)
                                {
                                    BulkReadEntry.usIndex = (ushort)i;
                                    m_BulkReadPI.AddEntry(nNodeInfo, BulkReadEntry);
                                }
                            }
                        }
                        else
                        {
                            //Crevis
                            BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_INPUT;
                            for (int inputVarOffset = 0; inputVarOffset < eCatIO.InputVariablesCount; inputVarOffset++)
                            {
                                BulkReadEntry.usIndex = (ushort)inputVarOffset;
                                m_BulkReadPI.AddEntry(nNodeInfo, BulkReadEntry);
                            }

                            BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_OUTPUT;
                            for (int outputVarOffset = 0; outputVarOffset < eCatIO.OutputVariablesCount; outputVarOffset++)
                            {
                                BulkReadEntry.usIndex = (ushort)outputVarOffset;
                                m_BulkReadPI.AddEntry(nNodeInfo, BulkReadEntry);
                            }
                        }

                        count++;
                    }
                    else if (eCatIO.NodeType == EnumIONodeType.Axis)
                    {
                        MMCECATIOList.Add(new MMCAxisIOBase(eCatIO.ID, _ConnHndl));
                        nNodeList[cnt] = MMCECATIOList[count].AxisReference;
                        _AxisMapping.Add(count, cnt);
                        stReadBulkReadData[cnt] = new NC_BULKREAD_PRESET_5();
                        cnt++;
                        count++;
                        axisFlag = true;
                    }


                }
                if (axisFlag)
                {
                    m_BulkRead = new MMCBulkRead(_ConnHndl);
                    m_BulkRead.Init(
                        NC_BULKREAD_PRESET_ENUM.eNC_BULKREAD_PRESET_5,
                        NC_BULKREAD_CONFIG_ENUM.eBULKREAD_CONFIG_MAX,
                        nNodeList, (ushort)nNodeList.Length);
                    m_BulkRead.Config();
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                nRetVal = -1;
                LoggerManager.Error($"ECATIO InitializeController() ERROR");
            }
            return nRetVal;
        }
        private void PrevOutputUpdate()
        {
            try
            {
                PI_VAR_UNION inputSt = new PI_VAR_UNION();
                byte[] byteData = new byte[ByteCountPerModule];

                try
                {
                    if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.First().IOType != EnumIOType.MIXED)
                    {
                        //Beckhoff
                        for (int chIndex = 0; chIndex < Channels.Count; chIndex++)
                        {
                            if (Channels[chIndex] is OutputChannel)
                            {
                                for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                {
                                    if (chIndex == 8 && portIndex == 2)
                                    {
                                        Console.WriteLine();
                                    }

                                    MMCECATIOList[chIndex].ReadPIVar((ushort)portIndex, PIVarDirection.ePI_OUTPUT, VAR_TYPE.BYTE, ref inputSt);
                                    bool byteState = (inputSt._byte == 1) ? true : false;
                                    Channels[chIndex].Port[portIndex].PortVal = byteState;
                                }
                            }
                        }
                    }
                    else
                    {
                        //Crevis
                        for (int chIndex = 0; chIndex < Channels.Count; chIndex++)
                        {
                            // OutputÀº Digital Output¸¸ Áö¿øÇÑ´Ù. Analog OutputÀº Á¶¸íÂÊ¿¡¼­ ÄÁÆ®·ÑÇÔ!
                            if (Channels[chIndex] is OutputChannel && Channels[chIndex].IOType == EnumIOType.OUTPUT)
                            {
                                for (int PIVarIndex = 0; PIVarIndex < ByteCountPerModule; PIVarIndex++)
                                {
                                    MMCECATIOList[Channels[chIndex].NodeIndex].ReadPIVar((ushort)(Channels[chIndex].VarOffset + PIVarIndex), PIVarDirection.ePI_OUTPUT, VAR_TYPE.BYTE, ref inputSt);
                                    byteData[PIVarIndex] = inputSt._byte;
                                }

                                var channelData = (int)BitConverter.ToInt16(byteData, 0); // ÀÏ´Ü 2byte¸¸ Áö¿øÇÑ´Ù.

                                for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                {
                                    bool bitValue = ((channelData >> portIndex) & 0x01) == 0x01;
                                    Channels[chIndex].Port[portIndex].PortVal = bitValue;
                                }
                            }
                        }
                    }
                }
                catch (MMCException merr)
                {
                    LoggerManager.Exception(merr);
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($err.Message);
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private byte GetReadPIVar(ushort index, uint state)
        {
            try
            {
                int totalIndex = DIGITAL_OUTPUT + index;
                if (((state >> totalIndex) >> index & 0x01) == 0x01)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void UpdateIOProc()
        {
            //byte outData;
            //bool[] doBitState = new bool[8];
            //bool updateOutput = false;
            PI_VAR_UNION writePIVarUnion = new PI_VAR_UNION();
            PI_BULKREAD_ENTRY BulkReadEntry;
            int DIGITAL_OUTPUT = 16;
            //IORet retCode = IORet.UNKNOWN;
            //   uint inputValue;
            object GetValue;
            byte[] byteData = new byte[ByteCountPerModule];

            try
            {

                while (bStopUpdateThread == false)
                {
                    // 8À» ³ÖÀº ¹è°æ : ±âÁ¸ Å¸ÀÌ¸Ó ÀÎÅÍ¹ú (4ms) + while¹®ÀÇ ÇÏ´ÜºÎ¿¡ Á¸ÀçÇß´ø Reset ÀÌº¥Æ®¿¡ ÀÇÇØ ÃÖ´ë ´ë±âÇÒ ¼ö ÀÖ´Â ½Ã°£ (4ms)
                    Thread.Sleep(8);

                    if (DevConnected == true)
                    {
                        m_BulkReadPI.Upload();
                        if (m_BulkRead != null)
                        {
                            m_BulkRead.Perform();
                            stReadBulkReadData = m_BulkRead.Preset_5;
                        }

                        if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.First().IOType != EnumIOType.MIXED)
                        {
                            //beckhoff
                            for (int chIndex = 0; chIndex < Channels.Count; chIndex++)
                            {
                                if (Channels[chIndex] is OutputChannel)
                                {
                                    if (MMCECATIOList[chIndex] is MMCECATIOBase &&
                                        EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions[chIndex].IOType
                                        == EnumIOType.OUTPUT)
                                    {
                                        BulkReadEntry.usAxisRef = MMCECATIOList[chIndex].AxisReference;
                                        BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_OUTPUT;

                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            BulkReadEntry.usIndex = (ushort)portIndex;
                                            m_BulkReadPI.GetEntry(BulkReadEntry, out GetValue);

                                            bool byteState = (bool)GetValue;
                                            if (Channels[chIndex].Port[portIndex].PortVal != byteState)
                                            {
                                                if (Channels[chIndex].Port[portIndex].PortVal)
                                                    writePIVarUnion._byte = 1;
                                                else
                                                    writePIVarUnion._byte = 0;

                                                MMCECATIOList[chIndex].WritePIVar((ushort)portIndex, writePIVarUnion, VAR_TYPE.BYTE);
                                            }
                                        }
                                    }
                                    else if (MMCECATIOList[chIndex] is MMCAxisIOBase)
                                    {
                                    }
                                }
                                if (Channels[chIndex] is InputChannel)
                                {
                                    if (MMCECATIOList[chIndex] is MMCECATIOBase)
                                    {
                                        BulkReadEntry.usAxisRef = MMCECATIOList[chIndex].AxisReference;
                                        BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_INPUT;

                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            BulkReadEntry.usIndex = (ushort)portIndex;
                                            m_BulkReadPI.GetEntry(BulkReadEntry, out GetValue);
                                            bool byteState = (bool)GetValue;

                                            Channels[chIndex].Port[portIndex].PortVal = byteState;
                                        }
                                    }
                                    else if (MMCECATIOList[chIndex] is MMCAxisIOBase)
                                    {
                                        int InputValue = (int)stReadBulkReadData[_AxisMapping[chIndex]].uiInputs;
                                        DIGITAL_OUTPUT = 16;
                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            int totalIndex = DIGITAL_OUTPUT + portIndex;
                                            if (((InputValue >> DIGITAL_OUTPUT) >> portIndex & 0x01) == 0x01)
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = true;
                                            }
                                            else
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = false;
                                            }
                                        }
                                    }
                                }
                                if (Channels[chIndex] is AnalogInputChannel)
                                {
                                    if (MMCECATIOList[chIndex] is MMCECATIOBase)
                                    {
                                        BulkReadEntry.usAxisRef = MMCECATIOList[chIndex].AxisReference;
                                        BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_INPUT;

                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            BulkReadEntry.usIndex = (ushort)(portIndex);  // Value of selected channel
                                            m_BulkReadPI.GetEntry(BulkReadEntry, out GetValue);

                                            long aiValue = Convert.ToInt64(GetValue);
                                            Channels[chIndex].Values[portIndex].PortVal = aiValue;
                                        }
                                    }
                                    else if (MMCECATIOList[chIndex] is MMCAxisIOBase)
                                    {
                                        int InputValue = (int)stReadBulkReadData[_AxisMapping[chIndex]].uiInputs;
                                        DIGITAL_OUTPUT = 16;
                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            int totalIndex = DIGITAL_OUTPUT + portIndex;

                                            if (((InputValue >> DIGITAL_OUTPUT) >> portIndex & 0x01) == 0x01)
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = true;
                                            }
                                            else
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //crevis
                            for (int chIndex = 0; chIndex < Channels.Count; chIndex++)
                            {
                                if (Channels[chIndex] is OutputChannel && Channels[chIndex].IOType == EnumIOType.OUTPUT)
                                {
                                    if (MMCECATIOList[Channels[chIndex].NodeIndex] is MMCECATIOBase)
                                    {
                                        BulkReadEntry.usAxisRef = MMCECATIOList[Channels[chIndex].NodeIndex].AxisReference;
                                        BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_OUTPUT;

                                        for (int PIVarIndex = 0; PIVarIndex < ByteCountPerModule; PIVarIndex++)
                                        {
                                            BulkReadEntry.usIndex = (ushort)(Channels[chIndex].VarOffset + PIVarIndex);
                                            m_BulkReadPI.GetEntry(BulkReadEntry, out GetValue);
                                            byteData[PIVarIndex] = (byte)GetValue;
                                        }

                                        var channelData = (int)BitConverter.ToInt16(byteData, 0); // [WLBI_TODO] 일단 2byte만 지원한다.
                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            bool bitValue = ((channelData >> portIndex) & 0x01) == 0x01;
                                            if (Channels[chIndex].Port[portIndex].PortVal != bitValue)
                                            {
                                                var PIVarIndex = portIndex / 8;
                                                if (Channels[chIndex].Port[portIndex].PortVal)
                                                {
                                                    channelData |= 0x01 << portIndex;
                                                }
                                                else
                                                {
                                                    channelData &= ~(0x01 << portIndex);
                                                }

                                                var convByte = BitConverter.GetBytes(channelData);
                                                writePIVarUnion._byte = convByte[PIVarIndex];
                                                MMCECATIOList[Channels[chIndex].NodeIndex].WritePIVar((ushort)(Channels[chIndex].VarOffset + PIVarIndex), writePIVarUnion, VAR_TYPE.BYTE);
                                            }
                                        }
                                    }
                                    else if (MMCECATIOList[chIndex] is MMCAxisIOBase)
                                    {
                                    }
                                }
                                if (Channels[chIndex] is InputChannel)
                                {
                                    if (MMCECATIOList[Channels[chIndex].NodeIndex] is MMCECATIOBase)
                                    {
                                        BulkReadEntry.usAxisRef = MMCECATIOList[Channels[chIndex].NodeIndex].AxisReference;
                                        BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_INPUT;

                                        for (int PIVarIndex = 0; PIVarIndex < ByteCountPerModule; PIVarIndex++)
                                        {
                                            BulkReadEntry.usIndex = (ushort)(Channels[chIndex].VarOffset + PIVarIndex);
                                            m_BulkReadPI.GetEntry(BulkReadEntry, out GetValue);
                                            byteData[PIVarIndex] = (byte)GetValue;
                                        }

                                        // 생성한 Data를 이용하여 channel의 port 값 설정하기
                                        var channelData = (int)BitConverter.ToInt16(byteData, 0); // [WLBI_TODO] 일단 2byte만 지원한다.
                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            bool bitValue = ((channelData >> portIndex) & 0x01) == 0x01;
                                            Channels[chIndex].Port[portIndex].PortVal = bitValue;
                                        }
                                    }
                                    else if (MMCECATIOList[Channels[chIndex].NodeIndex] is MMCAxisIOBase)
                                    {
                                        int InputValue = (int)stReadBulkReadData[_AxisMapping[chIndex]].uiInputs;
                                        DIGITAL_OUTPUT = 16;
                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            int totalIndex = DIGITAL_OUTPUT + portIndex;
                                            if (((InputValue >> DIGITAL_OUTPUT) >> portIndex & 0x01) == 0x01)
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = true;
                                            }
                                            else
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = false;
                                            }
                                        }
                                    }
                                }
                                if (Channels[chIndex] is AnalogInputChannel)
                                {
                                    if (MMCECATIOList[Channels[chIndex].NodeIndex] is MMCECATIOBase)
                                    {
                                        BulkReadEntry.usAxisRef = MMCECATIOList[Channels[chIndex].NodeIndex].AxisReference;
                                        BulkReadEntry.eDirection = (byte)PIVarDirection.ePI_INPUT;

                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            BulkReadEntry.usIndex = (ushort)(Channels[chIndex].VarOffset + portIndex);
                                            m_BulkReadPI.GetEntry(BulkReadEntry, out GetValue);

                                            long aiValue = Convert.ToInt64(GetValue);
                                            Channels[chIndex].Values[portIndex].PortVal = aiValue;
                                        }
                                    }
                                    else if (MMCECATIOList[Channels[chIndex].NodeIndex] is MMCAxisIOBase)
                                    {
                                        int InputValue = (int)stReadBulkReadData[_AxisMapping[chIndex]].uiInputs;
                                        DIGITAL_OUTPUT = 16;
                                        for (int portIndex = 0; portIndex < Channels[chIndex].Port.Count; portIndex++)
                                        {
                                            int totalIndex = DIGITAL_OUTPUT + portIndex;

                                            if (((InputValue >> DIGITAL_OUTPUT) >> portIndex & 0x01) == 0x01)
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = true;
                                            }
                                            else
                                            {
                                                Channels[chIndex].Port[portIndex].PortVal = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        public IORet ReadBit(int channel, int port, out bool value, bool reverse = false, bool isForced = false, bool ForcedValue = false)
        {
            PI_VAR_UNION inputSt = new PI_VAR_UNION();
            IORet retCode = IORet.UNKNOWN;
            value = false;

            try
            {
                if (isForced)
                {
                    value = ForcedValue;
                    retCode = IORet.NO_ERR;

                    return retCode;
                }
                else
                {
                    if (_UseMemoryUpdate == false)
                    {
                        if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.First().IOType != EnumIOType.MIXED)
                        {
                            //Beckhoff
                            if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions[channel].IOType == EnumIOType.INPUT)
                            {
                                value = Channels[channel].Port[port].PortVal;
                                retCode = IORet.NO_ERR;
                            }
                            else if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions[channel].IOType == EnumIOType.OUTPUT)
                            {
                                value = Channels[channel].Port[port].PortVal;
                                retCode = IORet.NO_ERR;
                            }
                            else
                            {
                                retCode = IORet.ERROR;
                            }
                        }
                        else
                        {
                            //Crevis
                            int PIVarIndex = port / 8;
                            if(Channels[channel].IOType == EnumIOType.INPUT)
                            {
                                value = Channels[channel].Port[port].PortVal;
                                retCode = IORet.NO_ERR;
                            }
                            else if(Channels[channel].IOType == EnumIOType.OUTPUT)
                            {
                                value = Channels[channel].Port[port].PortVal;
                                retCode = IORet.NO_ERR;
                            }
                        }
                    }
                    else
                    {
                        retCode = IORet.NO_ERR;
                    }

                    value = reverse ? !value : value;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                value = false;
                retCode = IORet.ERROR;
            }

            return retCode;
        }
        public IORet WriteBit(int channel, int port, bool value)
        {
            ushort[] recvData = new ushort[1];
            PI_VAR_UNION outputSt = new PI_VAR_UNION();
            PI_VAR_UNION inputSt = new PI_VAR_UNION();

            IORet retCode = IORet.ERROR;
            try
            {

                if (Channels[channel].IOType == EnumIOType.INPUT)
                    throw new IOException();
                else if (Channels[channel].IOType == EnumIOType.OUTPUT)
                {
                    Stopwatch stw = new Stopwatch();
                    stw.Reset();

                    if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.First().IOType != EnumIOType.MIXED)
                    {
                        //Beckhoff
                        if (value)
                            outputSt._byte = 1;
                        else
                            outputSt._byte = 0;
                        stw.Start();

                        MMCECATIOList[channel].WritePIVar((ushort)port, outputSt, VAR_TYPE.BYTE);

                        stw.Stop();

                        if (stw.ElapsedMilliseconds > 1000)
                        {
                            LoggerManager.Error($"Write timeout. Elapsed time = {stw.ElapsedMilliseconds}ms");
                        }
                        Channels[channel].Port[port].PortVal = value;
                    }
                    else
                    {
                        //Crevis
                        int PIVarIndex = port / 8;
                        int bitIndex = port % 8;

                        MMCECATIOList[Channels[channel].NodeIndex].ReadPIVar((ushort)(Channels[channel].VarOffset + PIVarIndex), PIVarDirection.ePI_OUTPUT, VAR_TYPE.BYTE, ref outputSt);
                        bool curState = ((outputSt._byte >> bitIndex) & 0x01) == 0x01;

                        if (curState == value)
                        {
                            return IORet.NO_ERR;
                        }

                        if (value)
                        {
                            outputSt._byte |= (byte)(0x01 << bitIndex);
                        }
                        else
                        {
                            outputSt._byte &= (byte)~(0x01 << bitIndex);
                        }
                        stw.Start();

                        MMCECATIOList[Channels[channel].NodeIndex].WritePIVar((ushort)(Channels[channel].VarOffset + PIVarIndex), outputSt, VAR_TYPE.BYTE);
                        
                        stw.Stop();

                        if (stw.ElapsedMilliseconds > 1000)
                        {
                            LoggerManager.Error($"Write timeout. Elapsed time = {stw.ElapsedMilliseconds}ms");
                        }
                        Channels[channel].Port[port].PortVal = value;
                    }
                }

                retCode = IORet.NO_ERR;

            }
            catch (IOException err)
            {
                LoggerManager.Exception(err);

                return retCode;
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WriteBit(channel#{0}, port#{1}, {2}): Function error occurred. Error = {3}", channel, port, value, err.Message));
                LoggerManager.Exception(err);

                retCode = IORet.ERROR;
                return retCode;
            }
            return (IORet)retCode;
        }
        public IORet ReadValue(int channel, int port, out long value, bool reverse = false)
        {
            PI_VAR_UNION inputSt = new PI_VAR_UNION();
            IORet retCode = IORet.ERROR;
            try
            {
                if (_UseMemoryUpdate == false)
                {
                    if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.First().IOType != EnumIOType.MIXED)
                    {
                        //Beckhoff
                        if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions[channel].IOType == EnumIOType.AI)
                            MMCECATIOList[channel].ReadPIVar((ushort)port, PIVarDirection.ePI_INPUT, VAR_TYPE.UINT, ref inputSt);
                        else if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions[channel].IOType == EnumIOType.AO)
                            MMCECATIOList[channel].ReadPIVar((ushort)port, PIVarDirection.ePI_OUTPUT, VAR_TYPE.UINT, ref inputSt);
                    }
                    else
                    {
                        //Crevis
                        if (Channels[channel].IOType == EnumIOType.AI)
                        {
                            MMCECATIOList[Channels[channel].NodeIndex].ReadPIVar((ushort)(Channels[channel].VarOffset + port), PIVarDirection.ePI_INPUT, VAR_TYPE.UINT, ref inputSt);
                        }
                        else if (Channels[channel].IOType == EnumIOType.AO)
                        {
                            MMCECATIOList[Channels[channel].NodeIndex].ReadPIVar((ushort)(Channels[channel].VarOffset + port), PIVarDirection.ePI_OUTPUT, VAR_TYPE.UINT, ref inputSt);
                        }
                    }
                }
                else
                {
                    retCode = IORet.NO_ERR;
                }

                value = inputSt._uint32;

            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("ReadBit(channel#{0}): Function error occurred. Error = {1}",channel, err.Message));
                LoggerManager.Exception(err);

                value = 0;
                retCode = IORet.ERROR;
            }
            return retCode;
        }
        public IORet WriteValue(int channel, int port, long value)
        {
            PI_VAR_UNION outputSt = new PI_VAR_UNION();

            IORet retCode = IORet.NO_ERR;
            try
            {
                if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions.First().IOType != EnumIOType.MIXED)
                {
                    //Beckhoff
                    if (EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions[channel].IOType == EnumIOType.AO)
                    {
                        outputSt._uint32 = (uint)value;
                        MMCECATIOList[channel].WritePIVar((ushort)port, outputSt, VAR_TYPE.UINT);
                    }
                    else
                    {
                        LoggerManager.Debug($"WriteValue(): Not proper type for write value. IO type is {EcatIODesc.ECATIODescripterParams.ECATNodeDefinitions[channel].IOType}");
                        retCode = IORet.ErrorSignatureNotMatch;
                    }
                }
                else
                {
                    //Crevis
                    if (Channels[channel].IOType == EnumIOType.AO)
                    {
                        outputSt._uint32 = (uint)value;
                        MMCECATIOList[Channels[channel].NodeIndex].WritePIVar((ushort)(Channels[channel].VarOffset + port), outputSt, VAR_TYPE.UINT);
                    }
                    else
                    {
                        LoggerManager.Debug($"WriteValue(): Not proper type for write value. IO type is {Channels[channel].IOType}");
                        retCode = IORet.ErrorSignatureNotMatch;
                    }
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WriteBit(channel#{0}, port#{1}, {2}): Function error occurred. Error = {3}", channel, port, value, err.Message));
                LoggerManager.Exception(err);

                retCode = IORet.ERROR;
                return retCode;
            }
            return (IORet)retCode;
        }
        public int WaitForIO(int channel, int port, bool level, long timeout = 0, bool isForced = false, bool forcedValue = false)
        {
            int retVal = -1;
            Stopwatch elapsedStopWatch = new Stopwatch();

            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();
            IORet ioReturn;
            try
            {
                bool runFlag = true;
                bool value;
                if (timeout == 0)
                {
                    timeout = 2000;
                }
                timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                do
                {
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Start.", elapsedStopWatch.ElapsedMilliseconds));
                    ioReturn = ReadBit(channel, port, out value, isForced: isForced, ForcedValue: forcedValue);
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Done.", elapsedStopWatch.ElapsedMilliseconds));

                    cnt++;
                    if (ioReturn == IORet.NO_ERR)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                                //throw new IOException(
                                //    string.Format("WaitForIO({0}, {1}) : wait io timeout error occurred. Timeout = {2}ms",
                                //    channel, port, timeout));
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    retVal = 0;
                                    LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                }
                                else runFlag = true;
                            }
                        }
                        else
                        {
                            if (value == level)
                            {
                                runFlag = false;
                                retVal = 0;
                                LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                            }
                            else runFlag = true;
                        }
                    }
                    else
                    {
                        runFlag = false;

                        retVal = -1;
                        LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }
            return retVal;
        }
        public void SetLight(int node, int channel, int lightPower)
        {
            try
            {
                WriteValue(node, channel, lightPower);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void WriteCameraPort(int chan, int port, bool isSet)
        {
            try
            {
                WriteBit(chan, port, isSet);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public int MonitorForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000, bool reverse = false, bool isForced = false, bool forcedValue = false, bool writelog = true, string ioKey = "")
        {
            if (timeout == 0)
                timeout = 10000;
            //#endif

            //=> Return Values
            int NO_ERROR = 0;
            int NET_IO_ERROR = -1;
            int TIME_OUT_ERROR = -2;

            int retVal = -1;
            bool matched = false;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            int cnt = 0;
            Stopwatch sustainStopWatch = new Stopwatch();
            sustainStopWatch.Reset();

            IORet ioReturn;
            List<KeyValuePair<string, long>> timeStamp;
            timeStamp = new List<KeyValuePair<string, long>>();

            try
            {
                bool runFlag = true;
                bool value;
                timeStamp.Add(new KeyValuePair<string, long>($"Entering DoWhile Loop", elapsedStopWatch.ElapsedMilliseconds));
                do
                {
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Start.", elapsedStopWatch.ElapsedMilliseconds));
                    ioReturn = ReadBit(channel, port, out value, reverse, isForced: isForced, ForcedValue: forcedValue);
                    //timeStamp.Add(new KeyValuePair<string, long>($"ReadBit Done.", elapsedStopWatch.ElapsedMilliseconds));

                    cnt++;
                    if (ioReturn == IORet.NO_ERR)
                    {
                        try
                        {
                            if (timeout != 0)
                            {
                                if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                                {
                                    if (writelog == true)
                                    {
                                        LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {timeout}ms, io:{ioKey}");
                                    }

                                    runFlag = false;
                                    retVal = TIME_OUT_ERROR;
                                    //throw new InOutException(
                                    //    string.Format("WaitForIO({0}, {1}) : wait io timeout error occurred. Timeout = {2}ms",
                                    //    channel, port, timeout));
                                }
                                else
                                {
                                    if (value == level)
                                    {
                                        if (matched == false)
                                        {
                                            sustainStopWatch.Start();
                                            matched = true;
                                            timeStamp.Add(new KeyValuePair<string, long>($"Value matched.", elapsedStopWatch.ElapsedMilliseconds));

                                        }
                                        else
                                        {
                                            if (sustainStopWatch.ElapsedMilliseconds > sustain)
                                            {
                                                runFlag = false;
                                                retVal = NO_ERROR;
                                                if (writelog == true)
                                                {
                                                    LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms, io:{ioKey}");
                                                }
                                                sustainStopWatch.Stop();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sustainStopWatch.Stop();
                                        sustainStopWatch.Reset();

                                        matched = false;
                                        runFlag = true;
                                    }
                                }
                            }
                            else
                            {
                                if (value == level)
                                {
                                    runFlag = false;
                                    retVal = NO_ERROR;
                                    if (writelog == true)
                                    {
                                        LoggerManager.Debug($"MonitorForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms, io:{ioKey}");
                                    }
                                }
                                else
                                {
                                    runFlag = true;
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            LoggerManager.Exception(err);
                            throw;
                        }
                    }
                    else
                    {
                        retVal = NET_IO_ERROR;
                        runFlag = false;
                        timeStamp.Add(new KeyValuePair<string, long>($"IO Error, Return code = {retVal}", elapsedStopWatch.ElapsedMilliseconds));

                        if (writelog == true)
                        {
                            LoggerManager.Error($"MonitorForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms, io:{ioKey}");
                        }
                        //throw new InOutException(
                        //    string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms",
                        //    channel, port, timeout));
                    }

                    Thread.Sleep(4);

                } while (runFlag == true);
            }
            catch (Exception err)
            {
                retVal = NET_IO_ERROR;
                //LoggerManager.Error($string.Format("MonitorForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }

            return retVal;
        }
        public EventCodeEnum LoadECATIODescripterParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            EcatIODesc = new ECATIODescripter();
            try
            {
                IParam tmpParam = null;
                RetVal = this.LoadParameter(ref tmpParam, typeof(ECATIODescripter.ECATIODescripterParam));
                if (RetVal == EventCodeEnum.NONE)
                {
                    EcatIODesc.ECATIODescripterParams = tmpParam as ECATIODescripter.ECATIODescripterParam;
                }
            }
            catch (Exception err)
            {
                RetVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("[ECATIOProvider] LoadSysParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

            }

            return RetVal;
        }
        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = LoadECATIODescripterParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            //this.SysParam = new IParamEmpty();

            return RetVal;
        }
        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }
        public int WaitForIO(int channel, int port, bool level, long sustain = 0, long timeout = 10000)
        {
#if DEBUG
            timeout = 60000;
#endif
            int retVal = -1;
            bool matched = false;
            Stopwatch elapsedStopWatch = new Stopwatch();
            Stopwatch sustainStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();

            sustainStopWatch.Reset();

            IORet ioReturn;
            try
            {
                bool runFlag = true;
                bool value;

                do
                {
                    Thread.Sleep(4);

                    ioReturn = ReadBit(channel, port, out value);
                    if (ioReturn == IORet.NO_ERR)
                    {
                        if (timeout != 0)
                        {
                            if (elapsedStopWatch.ElapsedMilliseconds > timeout)
                            {
                                LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {timeout}ms");

                                runFlag = false;
                                retVal = -2;
                                throw new IOException(
                                    string.Format("WaitForIO({0}, {1}) : wait io(Target Level = {level}) timeout error occurred. Timeout = {2}ms",
                                    channel, port, timeout));
                            }
                            else
                            {
                                if (value == level)
                                {
                                    if (matched == false)
                                    {
                                        sustainStopWatch.Start();
                                        matched = true;
                                    }
                                    else
                                    {
                                        if (sustainStopWatch.ElapsedMilliseconds > sustain)
                                        {
                                            runFlag = false;
                                            retVal = 0;
                                            LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                                            sustainStopWatch.Stop();
                                        }
                                    }
                                }
                                else
                                {
                                    sustainStopWatch.Stop();
                                    sustainStopWatch.Reset();
                                    matched = false;
                                    runFlag = true;
                                }
                            }
                        }
                        else
                        {
                            if (value == level)
                            {
                                runFlag = false;
                                retVal = 0;
                                LoggerManager.Debug($"WaitForIO({channel}, {port}) : IO value matched with {value}, Elapsed time = {elapsedStopWatch.ElapsedMilliseconds}ms");
                            }
                            else runFlag = true;
                        }
                    }
                    else
                    {
                        runFlag = false;

                        retVal = -1;
                        LoggerManager.Error($"WaitForIO({channel}, {port}) : wait io error occurred. Timeout = {timeout}ms");

                        throw new IOException(
                            string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms",
                            channel, port, timeout));
                    }
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("WaitForIO({0}, {1}) : wait io error occurred. Timeout = {2}ms, Err = {3}", channel, port, timeout, err.Message));
                LoggerManager.Exception(err);

            }
            finally
            {
                elapsedStopWatch?.Stop();

            }
            return retVal;
        }
    }
}

