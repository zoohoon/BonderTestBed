using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using RequestInterface;
using RequestCore.QueryPack;
using RequestCore.OperationPack;

namespace GPIBParamObject.System
{
    public class GPIBDataIDConverterParam : ISystemParameterizable, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [ParamIgnore]
        public string FilePath { get; } = "GPIB";

        [ParamIgnore]
        public string FileName { get; } = "GPIBDataIDConverterParam.Json";
        public string Genealogy { get; set; } = "GPIBDataIDConverterParam";
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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }


        private Element<Dictionary<int, URUWConnectorBase>> _DataIDDictinary = new Element<Dictionary<int, URUWConnectorBase>>();
        public Element<Dictionary<int, URUWConnectorBase>> DataIDDictinary
        {
            get { return _DataIDDictinary; }
            set
            {
                if (value != _DataIDDictinary)
                {
                    _DataIDDictinary = value;
                    RaisePropertyChanged();
                }
            }
        }

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

        private EventCodeEnum MakeURDictionary()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DataIDDictinary.Value == null)
                {
                    DataIDDictinary.Value = new Dictionary<int, URUWConnectorBase>();
                }

                if (DataIDDictinary.Value.Count == 0)
                {
                    URUWConnectorBase connectorBase = null;

                    #region SetTemp ( ElementID: 1015, MappingID: 0033 )

                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 30001025;//1015;//#Hynix_Merge: 검토 필요, Hynix에서는 1015로 쓰고 있었음. V20에서는 30001025
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new HasAffixArg()
                    {
                        postfix = "deg"
                    };
                    DataIDDictinary.Value.Add(0033, connectorBase);

                    #endregion

                    #region SamplePinCount = PinAlignSettignParam[0] = WaferInterval ( ElementID: 595, MappingID: 0137 )

                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 594;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new HasAffixArg()
                    {
                        DataFormat = "{00}"
                    };
                    DataIDDictinary.Value.Add(0137, connectorBase);

                    #endregion

                    #region OverDrive  ( ElementID: 893, MappingID: 0204 )

                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 893;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new FillArg()
                    {
                        Length = new FixData() { ResultData = "3" },
                        PaddingData = new FixData() { ResultData = "0" }
                    };

                    DataIDDictinary.Value.Add(0204, connectorBase);

                    #endregion

                    #region PMIEnable  ( ElementID: 818, MappingID: 0281 )

                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 818;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new ToInt();                   
                    DataIDDictinary.Value.Add(0281, connectorBase);

                    #endregion

                    #region MapDirX  ( ElementID: 1161, MappingID: 1224 )

                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 1161;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new UserToTskMapDir();
                    DataIDDictinary.Value.Add(1224, connectorBase);

                    #endregion

                    #region MapDirY  ( ElementID: 1162, MappingID: 1225 )

                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 1162;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new UserToTskMapDir();
                    DataIDDictinary.Value.Add(1225, connectorBase);

                    #endregion

                    #region PreHeatingTime = PROBECARDCHANGE_SOAK ( ElementID: 20000748, MappingID: 1227 )

                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20000748;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new HasAffixArg()
                    {
                        OptimizeFormat = (RequestBase)new DivisionByArgument { OperandValue = 60 },
                        postfix = "min",
                        DataFormat = "{00}"
                    };

                    DataIDDictinary.Value.Add(1227, connectorBase);

                    #endregion

                    #region Device Name ( ElementID: 30002005, MappingID: 1 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 30002005;//30002005;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new DeviceName();
                    DataIDDictinary.Value.Add(1, connectorBase);
                    #endregion

                    #region Continous Fail Enable ( ElementID: 20005643, MappingID: 221 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20005644;//20005643;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new ToInt();
                    DataIDDictinary.Value.Add(221, connectorBase);
                    #endregion

                    #region BinAnyCountLimit Param ( ElementID: 20005657, MappingID: 223 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20005666;//20005657;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new ToInt();
                    DataIDDictinary.Value.Add(223, connectorBase);
                    #endregion

                    #region Polish Wafer Thickness ( ElementID: 20005827, MappingID: 1334 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20005672;//20005827; // PolishWaferIntervalParameter[0].PolishWaferCleaningParameter[0].Thickness
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new FillArg() // new ToInt()
                    {
                        Length = new FixData() { ResultData = "4" },
                        PaddingData = new FixData() { ResultData = "0" }
                    };
                    DataIDDictinary.Value.Add(1334, connectorBase);
                    #endregion

                    #region NC Interval ( ElementID: 20000629, MappingID: 1337 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20000629;//20000629;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new FillArg() // new ToInt()
                    {
                        Length = new FixData() { ResultData = "2" },
                        PaddingData = new FixData() { ResultData = "0" }
                    };
                    DataIDDictinary.Value.Add(1337, connectorBase);
                    #endregion

                    #region NC Overdrive ( ElementID: 20005865, MappingID: 1338 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20005712;//20005865; // PolishWaferIntervalParameter[0].PolishWaferCleaningParameter[0].OverdriveValue
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new FillArg() // new ToInt()
                    {
                        Length = new FixData() { ResultData = "3" },
                        PaddingData = new FixData() { ResultData = "0" }
                    };
                    DataIDDictinary.Value.Add(1338, connectorBase);
                    #endregion

                    #region Polish Wafer Contact Count ( ElementID: 20003513, MappingID: 1339 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20005684;//20003513;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new FillArg() // new ToInt()
                    {
                        Length = new FixData() { ResultData = "2" },
                        PaddingData = new FixData() { ResultData = "0" }
                    };
                    DataIDDictinary.Value.Add(1339, connectorBase);
                    #endregion

                    #region Double Contact Count ( ElementID: 20005693, MappingID: 3801 )
                    connectorBase = new URUWConnectorBase();
                    connectorBase.ID = 20005719;//20005693;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.ReadValueConverter = new ToInt();
                    DataIDDictinary.Value.Add(3801, connectorBase);
                    #endregion
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {

                //MakeCATANIADummy();

                retval = SetDefaultParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                RetVal = MakeURDictionary();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

    }
}
