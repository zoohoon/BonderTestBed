using System;
using System.Collections.Generic;
using System.Linq;

namespace CameraChannelManager
{
    using ProberInterfaces;
    using ProberErrorCode;

    using LogModule;

    public class CameraChannelAdmin : ICameraChannelAdmin
    {

        public static string CameraChannelParamPath = "";
        public CameraChannelAdmin()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public bool Initialized { get; set; } = false;

        public CameraChannelParameter CameraChannelParam { get; set; }

        private IParam _DevParam;
        [ParamIgnore]
        public IParam DevParam
        {
            get { return _DevParam; }
            set
            {
                if (value != _DevParam)
                {
                    _DevParam = value;
                }
            }
        }
        //private IParam _SysParam;
        //[ParamIgnore]
        //public IParam SysParam
        //{
        //    get { return _SysParam; }
        //    set
        //    {
        //        if (value != _SysParam)
        //        {
        //            _SysParam = value;
        //        }
        //    }
        //}


        public EventCodeEnum InitCameraChannel(List<ICameraChannelControl> cameraiodevices)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                for (int i = 0; i < cameraiodevices.Count; i++)
                {
                    var cameraChannelService = CameraChannelParam.CameraChannelServiceParams[i];

                    //==> CL Port Parsing
                    MuxPortDescriptor clPortDesc = null;
                    if (cameraChannelService.CLPortDescStr.Value != null && cameraChannelService.CLPortDescStr.Value != String.Empty)
                        clPortDesc = ParsingMuxPortDescriptor(cameraChannelService.CLPortDescStr.Value);

                    //==> DataLoad Port Parsing
                    MuxPortDescriptor dataLoadPortDesc = null;
                    if (cameraChannelService.DataLoadPortDescStr.Value != null && cameraChannelService.DataLoadPortDescStr.Value != String.Empty)
                        dataLoadPortDesc = ParsingMuxPortDescriptor(cameraChannelService.DataLoadPortDescStr.Value);

                    //==> Value Bit Parsing
                    if (cameraChannelService.ValuePortDescStr.Value == null || cameraChannelService.ValuePortDescStr.Value == String.Empty)
                        throw new ArgumentNullException();

                    List<MuxPortDescriptor> valueBitDesc = new List<MuxPortDescriptor>();
                    char[] delimerterChar = new char[] { ':' };
                    List<String> valuePortMappings = cameraChannelService.ValuePortDescStr.Value.Split(delimerterChar).ToList();
                    foreach (String portMapping in valuePortMappings)
                        valueBitDesc.Add(ParsingMuxPortDescriptor(portMapping));

                    cameraChannelService.CLPortDesc = clPortDesc;
                    cameraChannelService.DataLoadPortDesc = dataLoadPortDesc;
                    cameraChannelService.ValueBitDesc = valueBitDesc;
                    foreach (CameraChannel controller in cameraChannelService.CameraChannelList)
                    {
                        if (controller.DevIndex.Value >= 0 & controller.DevIndex.Value < cameraChannelService.CameraChannelList.Count)
                        {
                            controller.CameraIODevice = cameraiodevices[controller.DevIndex.Value];
                        }
                        controller.CameraChannelDesc = cameraChannelService;
                    }
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public MuxPortDescriptor ParsingMuxPortDescriptor(String mappingStr)
        {
            int channelNum = 0;
            int portNum = 0;
            try
            {
                int cIndex = mappingStr.IndexOf('C');
                channelNum = Convert.ToInt32(mappingStr.Substring(0, cIndex));

                int pIndex = mappingStr.IndexOf('P');
                cIndex++;
                portNum = Convert.ToInt32(mappingStr.Substring(cIndex, pIndex - cIndex));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

                throw;
            }

            return new MuxPortDescriptor(channelNum, portNum);
        }

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {

                    List<ICameraChannelControl> cameraiodevices = new List<ICameraChannelControl>();
                    IIOManager iostate = this.IOManager();

                    int cnt = 0;

                    foreach (IIOBase item in iostate.IOServ.IOList)
                    {
                        ICameraChannelControl cameraChannelIO = item as ICameraChannelControl;
                        if (cnt < 2)
                        {
                            cameraiodevices.Add(cameraChannelIO);
                        }
                        cnt++;
                    }

                    InitCameraChannel(cameraiodevices);

                    Initialized = true;

                    retval = EventCodeEnum.NONE;
                }
                else
                {
                    LoggerManager.Error($"DUPLICATE_INVOCATION IN {this.GetType().Name}");

                    retval = EventCodeEnum.DUPLICATE_INVOCATION;
                }
            }
            catch (Exception err)
            {
                //LoggerManager.Error($string.Format("InitModule Error occurred. Err = {0}", e.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return retval;
        }
        public void DeInitModule()
        {
            try
            {
                LoggerManager.Debug($"DeinitModule() in {this.GetType().Name}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void SwitchCamera(int digNum, int channel)
        {
            try
            {
                CameraChannelParam.CameraChannelServiceParams[digNum].CameraChannelList[channel].SwitchCameraChannel();
            }
            catch (Exception err)
            {
                //LoggerManager.Error($String.Format("Camera Channel Out Of Range {0}", err.ToString()));
                LoggerManager.Exception(err);

            }
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                IParam tmpParam = null;
                tmpParam = new CameraChannelParameter();
                tmpParam.Genealogy = this.GetType().Name + "." + tmpParam.GetType().Name + ".";
                RetVal = this.LoadParameter(ref tmpParam, typeof(CameraChannelParameter));

                if (RetVal == EventCodeEnum.NONE)
                {
                    CameraChannelParam = tmpParam as CameraChannelParameter;

                }

                //SysParam = new IParamEmpty();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;

        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                retVal = this.SaveParameter(CameraChannelParam);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }
}
