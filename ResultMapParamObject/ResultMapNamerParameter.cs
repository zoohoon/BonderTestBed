using LogModule;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.ResultMap;
using RequestCore.Formatter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ResultMapParamObject
{
    public class ResultMapNamerParameter : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> IParam
        public string FilePath { get; set; } = "ResultMap";
        public string FileName { get; set; } = nameof(ResultMapNamerParameter) + ".json";
        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; } = new List<object>();
        #endregion

        private List<Namer> _Namers = new List<Namer>();
        public List<Namer> Namers
        {
            get { return _Namers; }
            set
            {
                if (value != _Namers)
                {
                    _Namers = value;
                    RaisePropertyChanged();
                }
            }
        }
        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                retVal = SetEmulParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return retVal;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;

            try
            {
                MakeDefaultNamerSet();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retVal;
        }

        public void SetElementMetaData()
        {
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public EventCodeEnum MakeDefaultNamerSet()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Namer newNamer = new Namer();

                // Case 0) LOT + "." + "0" + WaferNumber
                newNamer.Alias.Value = "STIF";
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.LOTID));
                newNamer.ComponentSet.Add(new NamingSeperatorComponent("."));
                newNamer.ComponentSet.Add(new NamingConstantComponent("0"));

                NamingProberPropertyComponent PropertyComponent = new NamingProberPropertyComponent(EnumProberMapProperty.WAFERID);
                PropertyComponent.Formatter = new DigitFormatter("00", 8, 2);
                newNamer.ComponentSet.Add(PropertyComponent);

                //NamingProberPropertyComponent PropertyComponent = new NamingProberPropertyComponent(EnumProberMapProperty.SLOTNO);
                //PropertyComponent.Formatter = new NumericFormatter("00");
                //newNamer.ComponentSet.Add(PropertyComponent);

                Namers.Add(newNamer);

                // Case 1) WaferID
                newNamer = new Namer();
                newNamer.Alias.Value = "OnlyWaferID";
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.WAFERID));
                Namers.Add(newNamer);

                // Case 2) SlotNum + "." + WaferID
                newNamer = new Namer();
                newNamer.Alias.Value = "SlotPlusWaferID";
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.SLOTNO));
                newNamer.ComponentSet.Add(new NamingSeperatorComponent("."));
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.WAFERID));
                Namers.Add(newNamer);

                // Case 3) WaferID + "A", WaferID + "Z", Suffix
                newNamer = new Namer();
                newNamer.Alias.Value = "WaferIDPlusSuffix";
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.WAFERID));
                newNamer.ComponentSet.Add(new NamingSuffixComponent(NamingSuffixType.UPPERALPHABETICAL, "A-Z"));
                Namers.Add(newNamer);

                // Case 4) WaderID + SlotNum + CPCount
                newNamer = new Namer();
                newNamer.Alias.Value = "WaferIDPlusSlotNumPlusCPCount";
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.WAFERID));
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.SLOTNO));
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.CPCOUNT));
                Namers.Add(newNamer);

                // Case 5) Constant
                newNamer = new Namer();
                newNamer.Alias.Value = "Constant";
                newNamer.ComponentSet.Add(new NamingProberPropertyComponent(EnumProberMapProperty.WAFERID));
                Namers.Add(newNamer);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
