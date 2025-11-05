using FoupProcedureManagerProject;
using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Foup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace FoupModules
{
    [Serializable]
    public class FoupLoadUnloadParam : INotifyPropertyChanged, IParam, ISystemParameterizable
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public string FilePath => "Foup";
        public string FileName => "FoupLoadUnloadParam.Json";

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

        public FoupLoadUnloadParam()
        {

        }
        private List<IFoupProcedure> _FoupProcedureList = new List<IFoupProcedure>();
        public List<IFoupProcedure> FoupProcedureList
        {
            get { return _FoupProcedureList; }
            set
            {
                _FoupProcedureList = value;
                NotifyPropertyChanged(nameof(FoupProcedureList));
            }
        }

        private List<string> _LoadOrder = new List<string>();

        public List<string> LoadOrder
        {
            get { return _LoadOrder; }
            set
            {
                _LoadOrder = value;
                NotifyPropertyChanged(nameof(LoadOrder));
            }
        }

        private List<string> _UnloadOrder = new List<string>();
        public List<string> UnloadOrder
        {
            get { return _UnloadOrder; }
            set
            {
                _UnloadOrder = value;
                NotifyPropertyChanged(nameof(UnloadOrder));
            }
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum OPUS_MINI()
        {
            EventCodeEnum retval = EventCodeEnum.NONE;
            try
            {
                #region 8INCHLOADERFOUP_8INCH
                List<IFoupProcedure> FoupProcedures = null;
                FoupProcedure procedure = null;
                FoupProcedures = new List<IFoupProcedure>();

                // DP LOCK
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_6IN_8IN_PRESENCE01());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPlate_Lock();
                procedure.ReverseProcedureName = nameof(FoupDockingPlate_Unlock);
                procedure.Caption = "FOUP DOCKING PLATE LOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP UNLock
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_6IN_8IN_PRESENCE01());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPlate_Unlock();
                procedure.ReverseProcedureName = nameof(FoupDockingPlate_Lock);
                procedure.Caption = "FOUP DOCKING PLATE UNLOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Hidden;
                FoupProcedures.Add(procedure);


                // Tilt Down (FoupCassetteOpener_Unlock - 12inch Loader)
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_COVER_ON());
                //procedure.PostSafeties.Add(new I_FO_VAC_ON());
                procedure.Behavior = new FoupTilt_Down();
                procedure.ReverseProcedureName = nameof(FoupTilt_Up);
                procedure.Caption = "FOUP CASSETTE TILT DOWN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // Tilt Up (FoupCassetteOpener_Lock - 12inch Loader)
                // Opener LOCK
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_COVER_ON());
                //procedure.PreSafeties.Add(new I_FO_VAC_ON());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupTilt_Up();
                procedure.ReverseProcedureName = nameof(FoupTilt_Down);
                procedure.Caption = "FOUP CASSETTE TILT UP";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                List<string> loadOrder = new List<string>();
                #region 8INCHLOADERFOUP_8INCH LOADSEQ
                loadOrder.Add(nameof(FoupDockingPlate_Lock));
                //loadOrder.Add(nameof(FoupTilt_Down));
                #endregion

                #endregion

                List<string> unloadOrder = new List<string>();

                #region 8INCHLOADERFOUP_8INCH UNLOADSEQ
                unloadOrder.Add(nameof(FoupTilt_Up));
                unloadOrder.Add(nameof(FoupDockingPlate_Unlock));
                #endregion

                this.FoupProcedureList = FoupProcedures;
                this.LoadOrder = loadOrder;
                this.UnloadOrder = unloadOrder;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum TOP8INCH()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum OPUS_FLAT()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                #region 12INCHLOADERFOUP_12INCH
                List<IFoupProcedure> FoupProcedures = null;
                FoupProcedure procedure = null;
                FoupProcedures = new List<IFoupProcedure>();

                // Cover UP
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_CP_40_IN());
                //Loaderbusy 받아야함.
                //procedure.PreSafeties.Add(new 로더비지());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupCover_Close();
                procedure.ReverseProcedureName = nameof(FoupCover_Open);
                procedure.Caption = "FOUP COVER DOWN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // Cover DOWN
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_CP_40_IN());
                //Loaderbusy 받아야함.
                //procedure.PreSafeties.Add(new 로더비지());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupCover_Open();
                procedure.ReverseProcedureName = nameof(FoupCover_Close);
                procedure.Caption = "FOUP COVER DOWN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Hidden;
                FoupProcedures.Add(procedure);

                // DP 40OUT
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_FO_UP());
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPort40_Out();
                procedure.ReverseProcedureName = nameof(FoupDockingPort40_In);
                procedure.Caption = "INNER PLATE OUT";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP 40IN
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_FO_UP());
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPort40_In();
                procedure.ReverseProcedureName = nameof(FoupDockingPort40_Out);
                procedure.Caption = "INNER PLATE IN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // Opener LOCK
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_COVER_ON());
                //procedure.PreSafeties.Add(new I_FO_VAC_ON());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupCassetteOpener_Lock();
                procedure.ReverseProcedureName = nameof(FoupCassetteOpener_Unlock);
                procedure.Caption = "FOUP CASSETTE OPENER LOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // Opener UNLOCK
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_COVER_ON());
                //procedure.PostSafeties.Add(new I_FO_VAC_ON());
                procedure.Behavior = new FoupCassetteOpener_Unlock();
                procedure.ReverseProcedureName = nameof(FoupCassetteOpener_Lock);
                procedure.Caption = "FOUP CASSETTE OPENER UNLOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP IN
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPort_In();
                procedure.ReverseProcedureName = nameof(FoupDockingPort_Out);
                procedure.Caption = "FOUP DOCKING PORT IN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP OUT
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPort_Out();
                procedure.ReverseProcedureName = nameof(FoupDockingPort_In);
                procedure.Caption = "FOUP DOCKING PORT OUT";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP LOCK
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPlate_Lock();
                procedure.ReverseProcedureName = nameof(FoupDockingPlate_Unlock);
                procedure.Caption = "FOUP DOCKING PLATE LOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP UNLock
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPlate_Unlock();
                procedure.ReverseProcedureName = nameof(FoupDockingPlate_Lock);
                procedure.Caption = "FOUP DOCKING PLATE UNLOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Hidden;
                FoupProcedures.Add(procedure);
                #endregion

                #region 12INCHLOADERFOUP_12INCH LOAD SEQ
                List<string> loadOrder = new List<string>();
                loadOrder.Add(nameof(FoupCover_Close));
                loadOrder.Add(nameof(FoupDockingPlate_Lock));
                loadOrder.Add(nameof(FoupDockingPort_In));
                loadOrder.Add(nameof(FoupDockingPort40_In));
                loadOrder.Add(nameof(FoupCassetteOpener_Unlock));
                loadOrder.Add(nameof(FoupDockingPort40_Out));
                loadOrder.Add(nameof(FoupCover_Open));
                #endregion

                #region 12INCHLOADERFOUP_12INCH UNLOADSEQ
                List<string> unloadOrder = new List<string>();
                unloadOrder.Add(nameof(FoupCover_Close));
                unloadOrder.Add(nameof(FoupDockingPort40_In));
                unloadOrder.Add(nameof(FoupCassetteOpener_Lock));
                unloadOrder.Add(nameof(FoupDockingPort40_Out));
                unloadOrder.Add(nameof(FoupDockingPort_Out));
                unloadOrder.Add(nameof(FoupDockingPlate_Unlock));
                unloadOrder.Add(nameof(FoupCover_Open));
                #endregion

                this.FoupProcedureList = FoupProcedures;
                this.LoadOrder = loadOrder;
                this.UnloadOrder = unloadOrder;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum OPUS_TOP()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                #region 12INCHLOADERFOUP_12INCH
                List<IFoupProcedure> FoupProcedures = null;
                FoupProcedure procedure = null;
                FoupProcedures = new List<IFoupProcedure>();

                // Cover UP
                procedure = new FoupProcedure();
                ///////presafeties//////////
                //procedure.PreSafeties.Add(new I_FO_EXIST()); // SINGLE 정의 필요
                //cover up
                procedure.PreSafeties_Recovery.Add(new I_COVER_UP());
                //vacuum on
                procedure.PreSafeties_Recovery.Add(new I_FO_VAC_ON());
                //mapping sensor 확인
                procedure.PreSafeties_Recovery.Add(new I_FO_Mapping_Out());
                //wafer out sensor - false
                procedure.PreSafeties_Recovery.Add(new I_WAFER_OUT());

                ///////postsafeties////////// 
                //cover down
                procedure.PostSafeties_Recovery.Add(new I_COVER_DOWN());
                //cover open
                procedure.PostSafeties_Recovery.Add(new I_COVER_CLOSE());
                //vacuum on
                procedure.PostSafeties_Recovery.Add(new I_FO_VAC_ON());
                //mapping sensor 확인 
                procedure.PostSafeties_Recovery.Add(new I_FO_Mapping_Out());

                procedure.Behavior = new FoupCover_Close();
                procedure.ReverseProcedureName = nameof(FoupCover_Open);
                procedure.Caption = "COVER DOWN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // Cover DOWN
                procedure = new FoupProcedure();
                procedure.Behavior = new FoupCover_Open();
                procedure.ReverseProcedureName = nameof(FoupCover_Close);
                procedure.Caption = "COVER UP";
                procedure.MenuDownVisibility = System.Windows.Visibility.Hidden;
                FoupProcedures.Add(procedure);

                // DP 40OUT
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_FO_UP());
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPort40_Out();
                procedure.ReverseProcedureName = nameof(FoupDockingPort40_In);
                procedure.Caption = "INNER PLATE OUT";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP 40IN
                procedure = new FoupProcedure();
                //procedure.PreSafeties.Add(new I_FO_UP());
                //procedure.PreSafeties.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT());
                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPort40_In();
                procedure.ReverseProcedureName = nameof(FoupDockingPort40_Out);
                procedure.Caption = "INNER PLATE IN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // Opener LOCK
                procedure = new FoupProcedure();

                //procedure.PreSafeties.Add(new I_FO_EXIST()); //SINGLE 정의 필요
                procedure.PreSafeties_Recovery.Add(new I_COVER_UNLOCK());
                procedure.PreSafeties_Recovery.Add(new I_FO_VAC_ON());
                procedure.PostSafeties_Recovery.Add(new I_COVER_LOCK());
                procedure.PostSafeties_Recovery.Add(new I_FO_VAC_OFF());

                procedure.Behavior = new FoupCassetteOpener_Lock();
                procedure.ReverseProcedureName = nameof(FoupCassetteOpener_Unlock);
                procedure.Caption = "FOUP OPENER LOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // Opener UNLOCK
                procedure = new FoupProcedure();

                //procedure.PreSafeties.Add(new I_FO_EXIST()); //SINGLE 정의 필요
                procedure.PreSafeties_Recovery.Add(new I_COVER_LOCK());
                procedure.PreSafeties_Recovery.Add(new I_FO_VAC_OFF());
                procedure.PostSafeties_Recovery.Add(new I_COVER_UNLOCK());
                procedure.PostSafeties_Recovery.Add(new I_FO_VAC_ON());

                procedure.Behavior = new FoupCassetteOpener_Unlock();
                procedure.ReverseProcedureName = nameof(FoupCassetteOpener_Lock);
                procedure.Caption = "FOUP OPENER UNLOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP IN
                procedure = new FoupProcedure();

                //procedure.PreSafeties.Add(new I_FO_EXIST()); //SINGLE 정의 필요
                procedure.PreSafeties_Recovery.Add(new I_FO_VAC_OFF());
                procedure.PreSafeties_Recovery.Add(new I_DP_OUT()); //SINGLE 정의 필요

                procedure.PostSafeties_Recovery.Add(new I_FO_VAC_OFF());
                procedure.PostSafeties_Recovery.Add(new I_DP_IN()); 

                procedure.Behavior = new FoupDockingPort_In();
                procedure.ReverseProcedureName = nameof(FoupDockingPort_Out);
                procedure.Caption = "DOCKING PORT IN";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP OUT
                procedure = new FoupProcedure();

                //procedure.PreSafeties.Add(new I_FO_EXIST());  //SINGLE 정의 필요
                procedure.PreSafeties_Recovery.Add(new I_FO_VAC_OFF());
                procedure.PreSafeties_Recovery.Add(new I_DP_IN()); 
                procedure.PostSafeties_Recovery.Add(new I_FO_VAC_OFF()); 
                procedure.PostSafeties_Recovery.Add(new I_DP_OUT()); 

                procedure.Behavior = new FoupDockingPort_Out();
                procedure.ReverseProcedureName = nameof(FoupDockingPort_In);
                procedure.Caption = "DOCKING PORT OUT";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP LOCK
                procedure = new FoupProcedure();

                //procedure.PreSafeties.Add(new I_FO_EXIST()); //SINGLE 정의 필요
                procedure.PreSafeties_Recovery.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT()); 
                procedure.PreSafeties_Recovery.Add(new I_FO_UNLOCK12()); //SINGLE 정의 필요

                procedure.PostSafeties_Recovery.Add(new I_C12IN_PRESENCE1());
                //procedure.PostSafeties.Add(new I_C12IN_PLACEMENT());
                procedure.PostSafeties_Recovery.Add(new I_FO_LOCK12()); //SINGLE 정의 필요

                //procedure.PostSafeties.Add(new FoupSafety2());
                procedure.Behavior = new FoupDockingPlate_Lock();
                procedure.ReverseProcedureName = nameof(FoupDockingPlate_Unlock);
                procedure.Caption = "DOCKING PLATE LOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Visible;
                FoupProcedures.Add(procedure);

                // DP UNLock
                procedure = new FoupProcedure();

                //procedure.PreSafeties.Add(new I_FO_EXIST());  //SINGLE 정의 필요
                procedure.PreSafeties_Recovery.Add(new I_C12IN_PRESENCE1());
                //procedure.PreSafeties.Add(new I_C12IN_PLACEMENT()); 
                procedure.PreSafeties_Recovery.Add(new I_FO_LOCK12()); //SINGLE 정의 필요

                procedure.PostSafeties_Recovery.Add(new I_C12IN_PRESENCE1());
                //procedure.PostSafeties.Add(new I_C12IN_PLACEMENT()); 
                procedure.PostSafeties_Recovery.Add(new I_FO_UNLOCK12()); //SINGLE 정의 필요


                procedure.Behavior = new FoupDockingPlate_Unlock();
                procedure.ReverseProcedureName = nameof(FoupDockingPlate_Lock);
                procedure.Caption = "DOCKING PLATE UNLOCK";
                procedure.MenuDownVisibility = System.Windows.Visibility.Hidden;
                FoupProcedures.Add(procedure);
                #endregion

                #region 12INCHLOADERFOUP_12INCH LOAD SEQ
                List<string> loadOrder = new List<string>();
                if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    loadOrder.Add(nameof(FoupDockingPlate_Lock));
                    loadOrder.Add(nameof(FoupDockingPort_In));
                    loadOrder.Add(nameof(FoupCassetteOpener_Unlock));
                    //loadOrder.Add(nameof(FoupDockingPort40_Out));
                    //loadOrder.Add(nameof(FoupCover_Open));
                }
                else if(SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    loadOrder.Add(nameof(FoupDockingPlate_Lock));
                    loadOrder.Add(nameof(FoupDockingPort_In));
                    loadOrder.Add(nameof(FoupCassetteOpener_Unlock));
                    loadOrder.Add(nameof(FoupDockingPort40_Out));
                    loadOrder.Add(nameof(FoupCover_Open));
                }
                #endregion

                #region 12INCHLOADERFOUP_12INCH UNLOADSEQ
                List<string> unloadOrder = new List<string>();
                if(SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    unloadOrder.Add(nameof(FoupCover_Close));
                    //unloadOrder.Add(nameof(FoupDockingPort40_In));
                    unloadOrder.Add(nameof(FoupCassetteOpener_Lock));
                    unloadOrder.Add(nameof(FoupDockingPort_Out));
                    unloadOrder.Add(nameof(FoupDockingPlate_Unlock));
                }
                else if(SystemManager.SysteMode == SystemModeEnum.Single)
                {
                    unloadOrder.Add(nameof(FoupCover_Close));
                    unloadOrder.Add(nameof(FoupDockingPort40_In));
                    unloadOrder.Add(nameof(FoupCassetteOpener_Lock));
                    unloadOrder.Add(nameof(FoupDockingPort_Out));
                    unloadOrder.Add(nameof(FoupDockingPlate_Unlock));
                }
                #endregion

                this.FoupProcedureList = FoupProcedures;
                this.LoadOrder = loadOrder;
                this.UnloadOrder = unloadOrder;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //retval = OPUS_FLAT();
                retval = OPUS_TOP();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
            //#region ListSetting
            //procedure = new FoupProcedure();
            //procedure.PreSafeties.Add(new FoupSafety1());
            //procedure.PreSafeties.Add(new FoupSafety2());
            //procedure.PostSafeties.Add(new FoupSafety2());
            //procedure.PostSafeties.Add(new FoupSafety1());
            //procedure.Behavior = new FoupBehavior1();
            //procedure.ReverseProcedureName = nameof(FoupBehavior2);
            //FoupProcedures.Add(procedure);

            //procedure = new FoupProcedure();
            //procedure.PreSafeties.Add(new FoupSafety2());
            //procedure.PreSafeties.Add(new FoupSafety3());
            //procedure.PostSafeties.Add(new FoupSafety3());
            //procedure.PostSafeties.Add(new FoupSafety2());
            //procedure.Behavior = new FoupBehavior2();
            //procedure.ReverseProcedureName = nameof(FoupBehavior1);
            //FoupProcedures.Add(procedure);

            //procedure = new FoupProcedure();
            //procedure.PreSafeties.Add(new FoupSafety3());
            //procedure.PreSafeties.Add(new FoupSafety4());
            //procedure.PostSafeties.Add(new FoupSafety4());
            //procedure.PostSafeties.Add(new FoupSafety3());
            //procedure.Behavior = new FoupBehavior3();
            //procedure.ReverseProcedureName = nameof(FoupBehavior4);
            //FoupProcedures.Add(procedure);

            //procedure = new FoupProcedure();
            //procedure.PreSafeties.Add(new FoupSafety4());
            //procedure.PreSafeties.Add(new FoupSafety5());
            //procedure.PostSafeties.Add(new FoupSafety5());
            //procedure.PostSafeties.Add(new FoupSafety4());
            //procedure.Behavior = new FoupBehavior4();
            //procedure.ReverseProcedureName = nameof(FoupBehavior3);
            //FoupProcedures.Add(procedure);

            //procedure = new FoupProcedure();
            //procedure.PreSafeties.Add(new FoupSafety5());
            //procedure.PreSafeties.Add(new FoupSafety1());
            //procedure.PostSafeties.Add(new FoupSafety1());
            //procedure.PostSafeties.Add(new FoupSafety5());
            //procedure.Behavior = new FoupBehavior5();
            //procedure.ReverseProcedureName = null;
            //FoupProcedures.Add(procedure);
            //#endregion

            //List<string> loadOrder = new List<string>();
            //#region 8INCHLOADERFOUP_8INCH LOADSEQ
            //loadOrder.Add(nameof(FoupDockingPlate_Lock));
            //loadOrder.Add(nameof(FoupTilt_Down));
            

            #region 12INCHLOADERFOUP_12INCH LOAD SEQ
            //loadOrder.Add(nameof(FoupDockingPlate_Lock));
            //loadOrder.Add(nameof(FoupDockingPort_In));
            //loadOrder.Add(nameof(FoupCassetteOpener_Unlock));
            //loadOrder.Add(nameof(FoupDockingPort40_In));
            //loadOrder.Add(nameof(FoupCover_Open));
            #endregion

            //loadOrder.Add(nameof(FoupBehavior2));
            //loadOrder.Add(nameof(FoupBehavior3));
            //loadOrder.Add(nameof(FoupBehavior4));
            //loadOrder.Add(nameof(FoupBehavior5));

            //List<string> unloadOrder = new List<string>();

            //#region 8INCHLOADERFOUP_8INCH UNLOADSEQ
            //unloadOrder.Add(nameof(FoupTilt_Up));
            //unloadOrder.Add(nameof(FoupDockingPlate_Unlock));
            //#endregion

            //#region 12INCHLOADERFOUP_12INCH UNLOADSEQ
            //unloadOrder.Add(nameof(FoupCover_Close));
            //unloadOrder.Add(nameof(FoupDockingPort40_Out));
            //unloadOrder.Add(nameof(FoupCassetteOpener_Lock));
            //unloadOrder.Add(nameof(FoupDockingPort_Out));
            //unloadOrder.Add(nameof(FoupDockingPlate_Unlock));
            //#endregion
            //this.FoupProcedureList = FoupProcedures;
            //this.LoadOrder = loadOrder;
            //this.UnloadOrder = unloadOrder;

            //return EventCodeEnum.NONE;
        }
    }
}
