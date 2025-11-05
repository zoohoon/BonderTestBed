using System;
using System.Collections.Generic;

namespace VisionParams
{
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;
    using ProberInterfaces.Vision;
    using ProberErrorCode;
    using ProberInterfaces;
    using LogModule;
    using Newtonsoft.Json;

    [Serializable]
    public class VisionDigiParameters : ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);


                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }

        private ObservableCollection<DigitizerGroup> _DigitizerGroups
            = new ObservableCollection<DigitizerGroup>();
        [ParamIgnore]
        public ObservableCollection<DigitizerGroup> DigitizerGroups
        {
            get { return _DigitizerGroups; }
            set { _DigitizerGroups = value; }
        }

        private ObservableCollection<DigitizerParameter> _ParamList = new ObservableCollection<DigitizerParameter>();
        [ParamIgnore]
        public ObservableCollection<DigitizerParameter> ParamList
        {
            get { return _ParamList; }
            set { _ParamList = value; }
        }
        public string FilePath { get; } = "Vision\\";
        public string FileName { get; } = "DIgitizerParameter.Json";
        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                SetDefaultParamGige();
                //SetDefaultPramEmul();
                //SetEmulTIS();
                //SetDefaultPramOPUSVVisionMapping();

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception)
            {

            }

            return RetVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                // SetDefaultPramOPUSVVisionMapping();
                //SetDefaultPramEmul();
                //SetDefaultParamMorphis();
                // SetDefaultPramOPUSVVisionMapping();
                SetDefaultPramBSCI();
                //      SetDefaultPramGige();   // For OPUS-V Test machine
                //SetDefaultPramOPUSVVisionMapping();
                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }

            return RetVal;
        }

        private void SetDefaultParamGige()
        {
            try
            {
                _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.GIGE_EMULGRABBER, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.GIGE_EMULGRABBER, 1, 8, "gigevisionWL.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.GIGE_EMULGRABBER, 1, 8, "gigevisionPH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.GIGE_EMULGRABBER, 1, 8, "gigevisionPL.dcf"));

                //_ParamList.Add(new DigitizerParameter("21814661", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();

                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups[0].DigiGroup.Value.Add(1);
                DigitizerGroups[0].DigiGroup.Value.Add(2);
                DigitizerGroups[0].DigiGroup.Value.Add(3);


                _ParamList.Add(new DigitizerParameter("1", EnumGrabberRaft.EMULGRABBER, 7, 8, "DCF\\Morphis_LoaderDual.dcf", 1));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(4);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        private void SetDefaultPramEmul()
        {
            try
            {

                _ParamList.Add(new DigitizerParameter("0", EnumGrabberRaft.EMULGRABBER, 4, 8, "DCF\\Morphis_Dual.dcf", 0));
                _ParamList.Add(new DigitizerParameter("1", EnumGrabberRaft.EMULGRABBER, 7, 8, "DCF\\Morphis_LoaderDual.dcf", 1));

                //_ParamList.Add(new DigitizerParameter("21814661", EnumGrabberRaft.TIS, 1, 8, "", 0));
                //_ParamList.Add(new DigitizerParameter("21814663", EnumGrabberRaft.TIS, 1, 8, "", 1));

                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultPramOPUSVVisionMapping()
        {
            try
            {

                _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWL.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPL.dcf"));


                _ParamList.Add(new DigitizerParameter("17814026", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("17814023", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));


                //VIsionJig
                _ParamList.Add(new DigitizerParameter("GIGE_VM0", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM0.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_VM1", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM1.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_VM2", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM2.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_VM3", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM3.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_VM4", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM4.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_VM5", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM5.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_VM6", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM6.dcf"));


                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups[0].DigiGroup.Value.Add(1);
                DigitizerGroups[0].DigiGroup.Value.Add(2);
                DigitizerGroups[0].DigiGroup.Value.Add(3);
                DigitizerGroups[0].DigiGroup.Value.Add(4);
                DigitizerGroups[0].DigiGroup.Value.Add(5);
                DigitizerGroups[0].DigiGroup.Value.Add(6);
                DigitizerGroups[0].DigiGroup.Value.Add(7);
                DigitizerGroups[0].DigiGroup.Value.Add(8);
                DigitizerGroups[0].DigiGroup.Value.Add(9);
                DigitizerGroups[0].DigiGroup.Value.Add(10);
                DigitizerGroups[0].DigiGroup.Value.Add(11);
                DigitizerGroups[0].DigiGroup.Value.Add(12);

                //DigitizerGroups.Add(new DigitizerGroup());
                //DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                //DigitizerGroups[1].DigiGroup.Value.Add(4);
                //DigitizerGroups[1].DigiGroup.Value.Add(5);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultPram_OPUSV_Machine3()
        {
            try
            {

                //_ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWH.dcf"));
                //_ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWL.dcf"));
                //_ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPH.dcf"));
                //_ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPL.dcf"));

                //DigitizerGroups.Add(new DigitizerGroup());
                //DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                //DigitizerGroups[0].DigiGroup.Value.Add(0);
                //DigitizerGroups[0].DigiGroup.Value.Add(1);
                //DigitizerGroups[0].DigiGroup.Value.Add(2);
                //DigitizerGroups[0].DigiGroup.Value.Add(3);

                //_ParamList.Add(new DigitizerParameter("35814245", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                //_ParamList.Add(new DigitizerParameter("21814659", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                //DigitizerGroups.Add(new DigitizerGroup());
                //DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                //DigitizerGroups[1].DigiGroup.Value.Add(4);
                //DigitizerGroups[1].DigiGroup.Value.Add(5);

                _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWL.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPL.dcf"));

                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups[0].DigiGroup.Value.Add(1);
                DigitizerGroups[0].DigiGroup.Value.Add(2);
                DigitizerGroups[0].DigiGroup.Value.Add(3);

                _ParamList.Add(new DigitizerParameter("35814245", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("21814659", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(4);
                DigitizerGroups[1].DigiGroup.Value.Add(5);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultPramBSCI()
        {
            try
            {

                _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWL.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPL.dcf"));

                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups[0].DigiGroup.Value.Add(1);
                DigitizerGroups[0].DigiGroup.Value.Add(2);
                DigitizerGroups[0].DigiGroup.Value.Add(3);

                _ParamList.Add(new DigitizerParameter("17814023", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("17814026", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(4);
                DigitizerGroups[1].DigiGroup.Value.Add(5);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultParamMorphis()
        {
            try
            {
                _ParamList.Add(new DigitizerParameter("0", EnumGrabberRaft.MILMORPHIS, 4, 8, "DCF\\Morphis_Dual.dcf", 0));
                _ParamList.Add(new DigitizerParameter("1", EnumGrabberRaft.MILMORPHIS, 7, 8, "DCF\\Morphis_LoaderDual.dcf", 1));

                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(1);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultPramGige()
        {
            try
            {

                _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionWL.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionPL.dcf"));

                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups[0].DigiGroup.Value.Add(1);
                DigitizerGroups[0].DigiGroup.Value.Add(2);
                DigitizerGroups[0].DigiGroup.Value.Add(3);

                _ParamList.Add(new DigitizerParameter("21814660", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf", 1));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(4);

                _ParamList.Add(new DigitizerParameter("17814027", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf", 2));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[2].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[2].DigiGroup.Value.Add(5);


                // Digitizer number 6
                _ParamList.Add(new DigitizerParameter("GIGE_VM0", EnumGrabberRaft.MILGIGE, 1, 8, "gigevisionVM0.dcf", 3));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[3].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[3].DigiGroup.Value.Add(6);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetEmulTIS()
        {
            try
            {

                _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionWL.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionPH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionPL.dcf"));

                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups[0].DigiGroup.Value.Add(1);
                DigitizerGroups[0].DigiGroup.Value.Add(2);
                DigitizerGroups[0].DigiGroup.Value.Add(3);

                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("21814660", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(4);
                DigitizerGroups[1].DigiGroup.Value.Add(5);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SetDefaultParamTISLoader()
        {
            try
            {
                _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionWH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionWL.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionPH.dcf"));
                _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberRaft.EMULGRABBER, 1, 8, "gigevisionPL.dcf"));

                //_ParamList.Add(new DigitizerParameter("21814661", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[0].DigiGroup.Value = new ObservableCollection<int>();

                DigitizerGroups[0].DigiGroup.Value.Add(0);
                DigitizerGroups[0].DigiGroup.Value.Add(1);
                DigitizerGroups[0].DigiGroup.Value.Add(2);
                DigitizerGroups[0].DigiGroup.Value.Add(3);


                _ParamList.Add(new DigitizerParameter("21814661", EnumGrabberRaft.TIS, 1, 8, "gigevisionWH.dcf"));
                DigitizerGroups.Add(new DigitizerGroup());
                DigitizerGroups[1].DigiGroup.Value = new ObservableCollection<int>();
                DigitizerGroups[1].DigiGroup.Value.Add(4);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        //public void InitCameraDefaultParam()
        //{
        //    _ParamList.Add(new DigitizerParameter("GIGE_WH", EnumGrabberType.MILGIGE, 1, 8, "gigevisionWH.dcf"));
        //    _ParamList.Add(new DigitizerParameter("GIGE_WL", EnumGrabberType.MILGIGE, 1, 8, "gigevisionWL.dcf"));
        //    _ParamList.Add(new DigitizerParameter("GIGE_PH", EnumGrabberType.MILGIGE, 1, 8, "gigevisionPH.dcf"));
        //    _ParamList.Add(new DigitizerParameter("GIGE_PL", EnumGrabberType.MILGIGE, 1, 8, "gigevisionPL.dcf"));

        //    DigitizerGroups.Add(new DigitizerGroup());
        //    DigitizerGroups[0].DefaultSetting();
        //}
    }
}
