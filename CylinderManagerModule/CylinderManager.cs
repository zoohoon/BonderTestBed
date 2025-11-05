using System;
using System.Collections.Generic;
using ProberErrorCode;
using ProberInterfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;

namespace CylinderManagerModule
{

    public class CylinderManager : ICylinderManager, INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public bool Initialized { get; set; } = false;

        private ICylinderType _Cylinders;
        public ICylinderType Cylinders
        {
            get { return _Cylinders; }
            set
            {
                if (value != _Cylinders)
                {
                    _Cylinders = value;
                    RaisePropertyChanged();
                }
            }
        }
        private CylinderParams _Params;
        public CylinderParams Params
        {
            get
            {
                return _Params;
            }
            set
            {
                if (value != _Params)
                {
                    _Params = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        public List<object> Nodes { get; set; }

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

        public EventCodeEnum InitModule()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (Initialized == false)
                {
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
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadParameter(string FullPath, List<IOPortDescripter<bool>> IOMap, CylinderParams DefaultParam = null)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                IParam loadParam = null;
                if (DefaultParam != null)
                {
                    IParam saveParam = DefaultParam;
                    saveParam.Genealogy = this.GetType().Name + "." + saveParam.GetType().Name + ".";
                    RetVal = Extensions_IParam.SaveParameter(null, saveParam, null, FullPath);

                    if (RetVal == EventCodeEnum.NONE)
                    {
                        Params = saveParam as CylinderParams;
                    }
                }

                RetVal = this.LoadParameter(ref loadParam, typeof(CylinderParams), null, FullPath);
                Params = loadParam as CylinderParams;

                #region Param Mapping

                foreach (var cyl in Cylinders.Members)
                {
                    string CylinderName = cyl.GetDisplayName();

                    var Mappingdata = Params.CylinderMappingParameterList.Find(x => x.CylinderName == CylinderName);

                    if (Mappingdata != null)
                    {
                        bool Valid = true;

                        var CylinderObject = cyl;

                        IOPortDescripter<bool> ex_out;
                        IOPortDescripter<bool> re_out;

                        List<IOPortDescripter<bool>> ex_in = new List<IOPortDescripter<bool>>();
                        List<IOPortDescripter<bool>> re_in = new List<IOPortDescripter<bool>>();
                        List<IOPortDescripter<bool>> intlck_in = new List<IOPortDescripter<bool>>();

                        ex_out = IOMap.Find(x => x.Key.Value == Mappingdata.Extend_Output_Key);

                        if (ex_out == null)
                        {
                            // Error
                            Valid = false;
                        }

                        re_out = IOMap.Find(x => x.Key.Value == Mappingdata.Retract_OutPut_Key);

                        if (re_out == null)
                        {
                            // Error
                            Valid = false;
                        }

                        foreach (var key in Mappingdata.Extend_Input_key_list)
                        {
                            IOPortDescripter<bool> tmp;

                            tmp = IOMap.Find(x => x.Key.Value == key);

                            if (tmp != null)
                            {
                                ex_in.Add(tmp);
                            }
                            else
                            {
                                Valid = false;
                                break;
                            }

                        }

                        foreach (var key in Mappingdata.Retract_Input_key_list)
                        {
                            IOPortDescripter<bool> tmp;

                            tmp = IOMap.Find(x => x.Key.Value == key);

                            if (tmp != null)
                            {
                                re_in.Add(tmp);
                            }
                            else
                            {
                                Valid = false;
                                break;
                            }
                        }

                        if(Mappingdata.Interlock_Input_key_list != null)
                        {
                            foreach (var key in Mappingdata.Interlock_Input_key_list)
                            {
                                IOPortDescripter<bool> tmp;

                                tmp = IOMap.Find(x => x.Key.Value == key);

                                if (tmp != null)
                                {
                                    intlck_in.Add(tmp);
                                }
                                else
                                {
                                    Valid = false;
                                    break;
                                }

                            }
                        }

                        if (Valid == true)
                        {
                            CylinderObject.SetExtend_OutPut(ex_out);
                            CylinderObject.SetRetract_OutPut(re_out);
                            CylinderObject.SetExtend_Input(ex_in);
                            CylinderObject.SetRetract_Input(re_in);
                            CylinderObject.SetInterlock_Input(intlck_in);



                            CylinderObject.Init();
                        }
                        else
                        {
                            // ERROR
                        }

                    }
                }
                #endregion

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum SaveParameter(string FullPath)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                RetVal = Extensions_IParam.SaveParameter(null, Params, null, FullPath);

                if (RetVal == EventCodeEnum.PARAM_ERROR)
                {
                    LoggerManager.Error($"[CylinderManager] SaveSysParam(): Serialize Error");
                    return RetVal;
                }

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        //public void Extend(ICylinderType cylinder)
        //{
        //    cylinder.Extend();
        //}
        //public void Retract(ICylinderType cylinder)
        //{
        //    cylinder.Retract();
        //}

        //public int sinqCylinderMove(IOPortDescripter<bool> Ex_Output,
        //                            bool Ex_Outlevel,
        //                            IOPortDescripter<bool> Re_Output,
        //                            bool Re_Outlevel,
        //                            IOPortDescripter<bool> Ex_Input,
        //                            bool Ex_level,
        //                            IOPortDescripter<bool> Re_Input,
        //                            bool Re_level,
        //                            long maintainTime = 400,
        //                            long timeout = 60000)
        //{
        //    int retVal = -1;
        //    //IOManager.IOServ.WriteBit(Ex_Output, Ex_Outlevel);
        //    //IOManager.IOServ.WriteBit(Re_Output, Re_Outlevel);
        //    //IOManager.IOServ.MonitorForIO(Re_Input, false);
        //    //IOManager.IOServ.MonitorForIO(Ex_Input, true , maintainTime , timeout);

        //    return retVal;
        //}

        //#region =>>Stage Command
        //private void Extend_CardHolder()
        //{
        //    this.Extend(StageCylinderType.CardHolder);
        //}
        //private void Retract_CardHolder()
        //{
        //    this.Retract(StageCylinderType.CardHolder);
        //}
        //private void Extend_CHTube()
        //{
        //    this.Extend(StageCylinderType.CHTube);
        //}
        //private void Retract_CHTubet()
        //{
        //    this.Retract(StageCylinderType.CHTube);
        //}
        //private void Extend_CHRotate()
        //{
        //    this.Extend(StageCylinderType.CHRotate);
        //}
        //private void Retract_CHRotate()
        //{
        //    this.Retract(StageCylinderType.CHRotate);
        //}
        //private void Extend_SwingPlate()
        //{
        //    this.Extend(StageCylinderType.SwingPlate);
        //}
        //private void Retract_SwingPlate()
        //{
        //    this.Retract(StageCylinderType.SwingPlate);
        //}
        //private void Extend_ThreeLEG()
        //{
        //    this.Extend(StageCylinderType.ThreeLEG);
        //}
        //private void Retract_ThreeLEG()
        //{
        //    this.Retract(StageCylinderType.ThreeLEG);
        //}
        //private void Extend_CleanUnit()
        //{
        //    this.Extend(StageCylinderType.CleanUnit);
        //}
        //private void Retract_CleanUnit()
        //{
        //    this.Retract(StageCylinderType.CleanUnit);
        //}
        //private void Extend_CH_ROT()
        //{
        //    this.Extend(StageCylinderType.CH_ROT);
        //}
        //private void Retract_CH_ROT()
        //{
        //    this.Retract(StageCylinderType.CH_ROT);
        //}
        //private void Extend_CH_TUB()
        //{
        //    this.Extend(StageCylinderType.CH_TUB);
        //}
        //private void Retract_CH_TUB()
        //{
        //    this.Retract(StageCylinderType.CH_TUB);
        //}
        //private void Extend_ZIF_REQ()
        //{
        //    this.Extend(StageCylinderType.ZIF_REQ);
        //}
        //private void Retract_ZIF_REQ()
        //{
        //    this.Retract(StageCylinderType.ZIF_REQ);
        //}
        //private void Extend_NeedleBrush()
        //{
        //    this.Extend(StageCylinderType.NeedleBrush);
        //}
        //private void Retract_NeedleBrush()
        //{
        //    this.Retract(StageCylinderType.NeedleBrush);
        //}
        //private void Extend_Front_Inner_Cover()
        //{
        //    this.Extend(StageCylinderType.Front_Inner_Cover);
        //}
        //private void Retract_Front_Inner_Cover()
        //{
        //    this.Retract(StageCylinderType.Front_Inner_Cover);
        //}
        //private void Extend_T2K_Cylinder()
        //{
        //    this.Extend(StageCylinderType.T2K_Cylinder);
        //}
        //private void Retract_T2K_Cylinder()
        //{
        //    this.Retract(StageCylinderType.T2K_Cylinder);
        //}
        //private void Extend_Tester_Head()
        //{
        //    this.Extend(StageCylinderType.Tester_Head);
        //}
        //private void Retract_Tester_Head()
        //{
        //    this.Retract(StageCylinderType.Tester_Head);
        //}
        //#endregion
        //#region =>>Loader Command
        //private void Extend_ArmExtender()
        //{
        //    this.Extend(LoaderCylinderType.ArmExtender);
        //}
        //private void Retract_ArmExtender()
        //{
        //    this.Retract(LoaderCylinderType.ArmExtender);
        //}
        //private void Extend_FramedPre()
        //{
        //    this.Extend(LoaderCylinderType.FramedPre);
        //}
        //private void Retract_FramedPre()
        //{
        //    this.Retract(LoaderCylinderType.FramedPre);
        //}
        //private void Extend_TesterHead()
        //{
        //    this.Extend(LoaderCylinderType.TesterHead);
        //}
        //private void Retract_TesterHead()
        //{
        //    this.Retract(LoaderCylinderType.TesterHead);
        //}
        //private void Extend_CardTray()
        //{
        //    this.Extend(LoaderCylinderType.CardTray);
        //}
        //private void Retract_CardTray()
        //{
        //    this.Retract(LoaderCylinderType.CardTray);
        //}
        //private void Extend_InspectionCover()
        //{
        //    this.Extend(LoaderCylinderType.InspectionCover);
        //}
        //private void Retract_InspectionCover()
        //{
        //    this.Retract(LoaderCylinderType.InspectionCover);
        //}
        //private void Extend_PreIN()
        //{
        //    this.Extend(LoaderCylinderType.PreIN);
        //}
        //private void Retract_PreIN()
        //{
        //    this.Retract(LoaderCylinderType.PreIN);
        //}
        //#endregion
        //#region =>>Foup Command
        //private void Extend_FoupDockingPlate6()
        //{
        //    this.Extend(FoupCylinderType.FoupDockingPlate6);
        //}
        //private void Retract_FoupDockingPlate6()
        //{
        //    this.Retract(FoupCylinderType.FoupDockingPlate6);
        //}
        //private void Extend_FoupDockingPlate8()
        //{
        //    this.Extend(FoupCylinderType.FoupDockingPlate8);
        //}
        //private void Retract_FoupDockingPlate8()
        //{
        //    this.Retract(FoupCylinderType.FoupDockingPlate8);
        //}
        //private void Extend_FoupDockingPlate12()
        //{
        //    this.Extend(FoupCylinderType.FoupDockingPlate12);
        //}
        //private void Retract_FoupDockingPlate12()
        //{
        //    this.Retract(FoupCylinderType.FoupDockingPlate12);
        //}
        //private void Extend_FoupDockingPort()
        //{
        //    this.Extend(FoupCylinderType.FoupDockingPort);
        //}
        //private void Retract_FoupDockingPort()
        //{
        //    this.Retract(FoupCylinderType.FoupDockingPort);
        //}
        //private void Extend_FoupDockingPort40()
        //{
        //    this.Extend(FoupCylinderType.FoupDockingPort40);
        //}
        //private void Retract_FoupDockingPort40()
        //{
        //    this.Retract(FoupCylinderType.FoupDockingPort40);
        //}
        //private void Extend_FoupOpener()
        //{
        //    this.Extend(FoupCylinderType.FoupOpener);
        //}
        //private void Retract_FoupOpener()
        //{
        //    this.Retract(FoupCylinderType.FoupOpener);
        //}
        //private void Extend_FoupCover()
        //{
        //    this.Extend(FoupCylinderType.FoupCover);
        //}
        //private void Retract_FoupCover()
        //{
        //    this.Retract(FoupCylinderType.FoupCover);
        //}
        //#endregion



        //public EventCodeEnum InitModule(Autofac.IContainer container)
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    RetVal = EventCodeEnum.NONE;

        //    //StageCylinders = new StageCylinderType();
        //    //LoaderCylinders = new LoaderCylinderType();
        //    //FoupCylinders = new FoupCylinderType();

        //    //#region Assign Foup's parameters 

        //    ////List<ICylinderType> StageCylinderCollection = StageCylinders.Members.ToList();
        //    ////List<ICylinderType> LoaderCylinderCollection = LoaderCylinders.Members.ToList();
        //    //List<ICylinderType> FoupCylinderCollection = FoupCylinders.Members.ToList();

        //    //foreach (var cyl in FoupCylinderCollection)
        //    //{
        //    //    var CylinderObject = cyl;
        //    //    string CylinderName = cyl.GetDisplayName();


        //    //    var param = Params.CylinderParamList.Find(x => x.Address.DevAddresses == CylinderName);
        //    //    //var param = Params.CylinderaramList.Find(x => x. == CylinderName);

        //    //    if (param != null)
        //    //    {
        //    //        CylinderObject.CylinderParam = param;
        //    //    }
        //    //}
        //    //#endregion

        //    return RetVal;
        //}

        //public EventCodeEnum SetContainer(Autofac.IContainer container)
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    this.Container = container;

        //    RetVal = EventCodeEnum.NONE;

        //    return RetVal;
        //}

        //public EventCodeEnum SaveSysParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    return RetVal;
        //}

        //public EventCodeEnum LoadParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    RetVal = LoadSysParameter();

        //    return RetVal;
        //}

        //public EventCodeEnum SaveParameter()
        //{
        //    EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

        //    return RetVal;
        //}

    }
}
