using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using ResultMapParamObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NamerMudule
{
    public enum EnumNamerCompoenent
    {
        WAFERID,
        LOTID,
        SLOTNO,
    }

    public class ResultMapNamer : INotifyPropertyChanged, IHasSysParameterizable, IFactoryModule
    {
        // Naming을 위한 Component를 관리 및 제공

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private MapHeaderObject HeaderObj { get; set; }

        public string[] filelist { get; set; }

        /// <summary>
        /// 사용할 수 있는 Components, 개발자가 미리 제공.
        /// 추후, Type 선택 및 Type에 따라 입력 가능한 데이터를 UI로....
        /// 선택 된 SET의 결과를 Display...
        /// </summary>
        private ObservableCollection<NamingComponentBase> _NamingComponents;
        public ObservableCollection<NamingComponentBase> NamingComponents
        {
            get { return _NamingComponents; }
            set
            {
                if (value != _NamingComponents)
                {
                    _NamingComponents = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ResultMapNamerParameter _resultMapNamerParameter;
        public ResultMapNamerParameter ResultMapNamerParameter
        {
            get { return _resultMapNamerParameter; }
            set
            {
                if (value != _resultMapNamerParameter)
                {
                    _resultMapNamerParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IParam ResultMapNamerSysIParam { get; private set; }

        //private List<Namer> _Namers = new List<Namer>();
        //public List<Namer> Namers
        //{
        //    get { return _Namers; }
        //    set
        //    {
        //        if (value != _Namers)
        //        {
        //            _Namers = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        public ResultMapNamer()
        {

        }

        public void SetHeaderData(MapHeaderObject _headerinfo)
        {
            HeaderObj = _headerinfo;
        }

        public ResultMapNamer(List<string> test)
        {
            try
            {
                NamingComponents = new ObservableCollection<NamingComponentBase>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public EventCodeEnum GetFilePath(string key, out string path)
        //{
        //    EventCodeEnum retval = EventCodeEnum.UNDEFINED;
        //    path = string.Empty;

        //    try
        //    {
        //        Namer namer = GetNamer(key);

        //        if(namer != null)
        //        {
        //            //if (HeaderObj != null && HeaderObj.ProberMapPropertiesDictionanry == null)
        //            //{
        //            //    HeaderObj.MakeProberMapPropertyDictionay();
        //            //}

        //            namer.SetProberMapDictionary(HeaderObj.PropertyDictionary);
        //            retval = namer.Run(out path);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public Namer GetNamer(string NamerKey)
        {
            Namer retval = null;

            try
            {
                Namer tmpNamer = ResultMapNamerParameter.Namers.FirstOrDefault(x => x.Alias.Value == NamerKey);

                if (tmpNamer != null)
                {
                    if(HeaderObj == null)
                    {
                        HeaderObj = new MapHeaderObject();
                    }

                    if(HeaderObj != null)
                    {
                        HeaderObj.MakeProberMapPropertyDictionay();

                        if (HeaderObj.PropertyDictionary != null)
                        {
                            tmpNamer.SetProberMapDictionary(HeaderObj.PropertyDictionary);

                            retval = tmpNamer;
                        }
                        else
                        {
                            LoggerManager.Debug($"[ResultMapNamer], GetNamer() : PropertyDictionary is null.");
                        }
                    }
                }
                else
                {
                    LoggerManager.Debug($"[ResultMapNamer], GetNamer() : Namer is not matched. key = {NamerKey}");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public string[] GetNamerAliaslist()
        {
            string[] retval = null;

            try
            {
                if (ResultMapNamerParameter.Namers != null && ResultMapNamerParameter.Namers.Count > 0)
                {
                    retval = new string[ResultMapNamerParameter.Namers.Count];

                    for (int i = 0; i < ResultMapNamerParameter.Namers.Count; i++)
                    {
                        retval[i] = ResultMapNamerParameter.Namers[i].Alias.Value;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum LoadSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                IParam iparam = null;
                retVal = this.LoadParameter(ref iparam, typeof(ResultMapNamerParameter));

                if (retVal == EventCodeEnum.NONE)
                {
                    ResultMapNamerSysIParam = iparam;
                    ResultMapNamerParameter = ResultMapNamerSysIParam as ResultMapNamerParameter;
                }

                SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public EventCodeEnum SaveSysParameter()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = this.SaveParameter(ResultMapNamerSysIParam);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }
    }
}
