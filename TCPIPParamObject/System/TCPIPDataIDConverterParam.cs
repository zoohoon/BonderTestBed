using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using RequestCore.QueryPack;
using RequestCore.Query;
using RequestCore.OperationPack;
using ProberInterfaces.Enum;

namespace TCPIPParamObject.System
{
    public class TCPIPDataIDConverterParam : ISystemParameterizable, INotifyPropertyChanged
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
        public string FilePath { get; } = "TCPIP";

        [ParamIgnore]
        public string FileName { get; } = "TCPIPDataIDConverterParam.Json";
        public string Genealogy { get; set; } = "TCPIPDataIDConverterParam";
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

        [JsonIgnore]
        private const int SupportDRCount = 7;

        private Element<Dictionary<int, DRDWConnectorBase>> _DataIDDictinary = new Element<Dictionary<int, DRDWConnectorBase>>();
        public Element<Dictionary<int, DRDWConnectorBase>> DataIDDictinary
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
                //// TODO : 이미 생성되어 있을 때, 최신 데이터가 아닌 경우, SetDefaultParam()를 호출해줌으로써 삭제 안해도 되게끔....
                //if (DataIDDictinary.Value == null || DataIDDictinary.Value.Count != SupportDRCount)
                //{
                //    LoggerManager.Debug($"[TCPIPDataIDConverterParam], Init() : DataIDDictinary count = {DataIDDictinary.Value.Count}, SupportDRCount = {SupportDRCount}");

                //    MakeCATANIA();
                //}

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

        private EventCodeEnum MakeCATANIA()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DataIDDictinary.Value == null)
                {
                    DataIDDictinary.Value = new Dictionary<int, DRDWConnectorBase>();
                }
                else
                {
                    DataIDDictinary.Value = new Dictionary<int, DRDWConnectorBase>();
                }

                if (DataIDDictinary.Value.Count == 0)
                {
                    DRDWConnectorBase connectorBase = null;

                    #region SetTemp

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 1015;
                    connectorBase.WriteValidationRule = new InRange()
                    {
                        Start = new ElementLowerLimit()
                        {

                        },
                        End = new ElementUpperLimit()
                        {

                        }
                    };

                    connectorBase.WriteValueConverter = new DivisionByArgument() { OperandValue = 10 };
                    DataIDDictinary.Value.Add(020010, connectorBase);

                    #endregion

                    #region OverDrive

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 893;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.WriteValueConverter = null;

                    DataIDDictinary.Value.Add(030006, connectorBase);

                    #endregion

                    #region TCPIP BIN TYPE

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 10000161;
                    connectorBase.WriteValidationRule = new InRange()
                    {
                        Start = new ElementLowerLimit()
                        {

                        },
                        End = new ElementUpperLimit()
                        {

                        }
                    };

                    connectorBase.WriteValueConverter = new IntToEnum() { Type = typeof(BinType) };

                    DataIDDictinary.Value.Add(020048, connectorBase);

                    #endregion

                    #region CurTemp - Common

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 30000001;
                    connectorBase.IsReadOnly = true;

                    connectorBase.ReadValueConverter = new MultiplicationByArgument() { OperandValue = 10 };

                    DataIDDictinary.Value.Add(150005, connectorBase);

                    #endregion

                    #region ProbeCardID

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 10002936;
                    //connectorBase.ID = 30000022; 

                    DataIDDictinary.Value.Add(050018, connectorBase);

                    #endregion

                    #region MapDirX

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 1161;

                    connectorBase.ReadValueConverter = new EnumToInt() { Type = typeof(MapHorDirectionEnum) };

                    DataIDDictinary.Value.Add(020017, connectorBase);

                    #endregion

                    #region MapDirY

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 1162;

                    connectorBase.ReadValueConverter = new EnumToInt() { Type = typeof(MapVertDirectionEnum) };

                    DataIDDictinary.Value.Add(020018, connectorBase);

                    #endregion

                    #region RefDieX

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 20005714;

                    DataIDDictinary.Value.Add(020679, connectorBase);

                    #endregion

                    #region RefDieY

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 20005738;

                    DataIDDictinary.Value.Add(020680, connectorBase);

                    #endregion
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        private EventCodeEnum MakeCATANIADummy()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (DataIDDictinary.Value == null)
                {
                    DataIDDictinary.Value = new Dictionary<int, DRDWConnectorBase>();
                }

                if (DataIDDictinary.Value.Count == 0)
                {
                    DRDWConnectorBase connectorBase = null;

                    #region SetTemp

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 1015;
                    connectorBase.WriteValidationRule = new InRange()
                    {
                        Start = new DummyElementLowerLimit()
                        {
                            DefaultLowerlimit = -550
                        },
                        End = new DummyElementUpperLimit()
                        {
                            DefaultUpperlimit = 2000
                        }
                    };

                    connectorBase.WriteValueConverter = new DivisionByArgument() { OperandValue = 10 };
                    DataIDDictinary.Value.Add(020010, connectorBase);

                    #endregion

                    #region OverDrive

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 893;
                    connectorBase.WriteValidationRule = null;
                    connectorBase.WriteValueConverter = null;

                    DataIDDictinary.Value.Add(030006, connectorBase);

                    #endregion

                    #region TCPIP BIN TYPE

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 10000161;
                    connectorBase.WriteValidationRule = new InRange()
                    {
                        Start = new DummyElementLowerLimit()
                        {
                            DefaultLowerlimit = 0
                        },
                        End = new DummyElementUpperLimit()
                        {
                            DefaultUpperlimit = 4
                        }
                    };

                    connectorBase.WriteValueConverter = new IntToEnum() { Type = typeof(BinType) };
                    DataIDDictinary.Value.Add(020048, connectorBase);

                    #endregion

                    #region CurTemp - Common

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 30000001;
                    connectorBase.IsReadOnly = true;

                    DataIDDictinary.Value.Add(150005, connectorBase);

                    #endregion

                    #region ProbeCardID

                    connectorBase = new DRDWConnectorBase();
                    connectorBase.ID = 30000022;

                    DataIDDictinary.Value.Add(050018, connectorBase);

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
                MakeCATANIA();
                //MakeCATANIADummy();

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
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                MakeCATANIA();
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return RetVal;
        }

    }
}
