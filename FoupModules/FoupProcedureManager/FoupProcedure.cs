using ProberErrorCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using ProberInterfaces.Foup;
using System.Windows;
using Newtonsoft.Json;
using LogModule;

namespace FoupProcedureManagerProject
{
    [Serializable]
    public class FoupProcedure : INotifyPropertyChanged, IFoupProcedure
    {
        public FoupProcedure()
        {

        }
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private string _Caption;
        public string Caption
        {
            get { return _Caption; }
            set
            {
                _Caption = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _MenuDownVisibility;
        public Visibility MenuDownVisibility
        {
            get { return _MenuDownVisibility; }
            set
            {
                if (value != _MenuDownVisibility)
                {
                    _MenuDownVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        private FoupBehavior _Behavior;
        public FoupBehavior Behavior
        {
            get
            { return _Behavior; }
            set
            {
                _Behavior = value;
                RaisePropertyChanged();
            }
        }

        private FoupSafeties _PreSafeties_Recovery = new FoupSafeties();
        public FoupSafeties PreSafeties_Recovery
        {
            get { return _PreSafeties_Recovery; }
            set
            {
                _PreSafeties_Recovery = value;
                RaisePropertyChanged();
            }
        }

        private FoupSafeties _PostSafeties_Recovery = new FoupSafeties();
        public FoupSafeties PostSafeties_Recovery
        {
            get { return _PostSafeties_Recovery; }
            set
            {
                _PostSafeties_Recovery = value;
                RaisePropertyChanged();
            }
        }

        private string _ReverseProcedureName = null;
        public string ReverseProcedureName
        {
            get { return _ReverseProcedureName; }
            set
            {
                _ReverseProcedureName = value;
                RaisePropertyChanged();
            }
        }

        private FoupProcedure _ReverseProcedure;
        [XmlIgnore, JsonIgnore]
        public FoupProcedure ReverseProcedure
        {
            get { return _ReverseProcedure; }
            set
            {
                _ReverseProcedure = value;
                RaisePropertyChanged();
            }
        }

        public EventCodeEnum BehaviorRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = Behavior.Run();

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }


        public EventCodeEnum PreSafetiesRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {
                retVal = SafetiesRun(PreSafeties_Recovery);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public EventCodeEnum PostSafetiesRun()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                retVal = SafetiesRun(PostSafeties_Recovery);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        private EventCodeEnum SafetiesRun(FoupSafeties safeties)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                if (safeties.Count > 0)
                {
                    foreach (var safety in safeties)
                    {
                        retVal = safety.Run();
                        if(retVal != EventCodeEnum.FOUP_CHECK_IO_DONE)
                        {
                            LoggerManager.RecoveryLog($"Conditions Safeties, {safety} State = {safety.State}, Error Code = {retVal}", true);
                            return retVal;
                        }
                    }

                    // 세이프티의 각각의 enum이 Done이면 EventCodeEnum은 ->Done
                    List<FoupSafety> Item = safeties.FindAll((x => x.State != FoupSafetyStateEnum.DONE));
                    if (Item.Count() == 0)
                    {
                        retVal = EventCodeEnum.NONE;
                    }
                    else
                    {
                        retVal = EventCodeEnum.FOUP_ERROR;
                    }
                }

                

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
