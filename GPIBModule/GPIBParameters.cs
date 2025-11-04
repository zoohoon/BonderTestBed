using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.Command;
using ProberInterfaces.Command.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using LogModule;
using Newtonsoft.Json;
using RequestInterface;
using RequestCore.Query;
using ProberInterfaces.Enum;
using RequestCore.QueryPack.GPIB;
using RequestCore.QueryPack;
using RequestCore.ActionPack.Internal;
using RequestCore.ActionPack.GPIB;
using RequestCore.OperationPack;

namespace GPIBModule
{

    [Serializable]
    public class GPIBSysParam : ISystemParameterizable, INotifyPropertyChanged, IGPIBSysParam, IParam, IParamNode
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
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
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
        public string FileName { get; } = "GPIBSystemParam.Json";
        public string Genealogy { get; set; } = "GPIBSysParam";
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

        public GPIBSysParam()
        {
        }

        private Element<EnumGpibEnable> _EnumGPIBEnable = new Element<EnumGpibEnable>();
        public Element<EnumGpibEnable> EnumGPIBEnable
        {
            get { return _EnumGPIBEnable; }
            set
            {
                if (value != _EnumGPIBEnable)
                {
                    _EnumGPIBEnable = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumGpibCommType> _EnumGpibComType = new Element<EnumGpibCommType>();
        public Element<EnumGpibCommType> EnumGpibComType
        {
            get { return _EnumGpibComType; }
            set
            {
                if (value != _EnumGpibComType)
                {
                    _EnumGpibComType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<EnumGpibProbingMode> _EnumGpibProbingMode
            = new Element<EnumGpibProbingMode>();
        public Element<EnumGpibProbingMode> EnumGpibProbingMode
        {
            get
            {
                Element<EnumGpibProbingMode> retMode = null;
                retMode = _EnumGpibProbingMode ?? new Element<EnumGpibProbingMode>() { Value = ProberInterfaces.EnumGpibProbingMode.INTERNAL };
                return retMode;
            }
            set
            {
                if (value != _EnumGpibProbingMode)
                {
                    _EnumGpibProbingMode = value;
                    RaisePropertyChanged();
                }
            }
        }


        private Element<int> _BoardIndex = new Element<int>();
        public Element<int> BoardIndex
        {
            get { return _BoardIndex; }
            set
            {
                if (value != _BoardIndex)
                {
                    _BoardIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<BinType> _EnumBinType = new Element<BinType>();
        public Element<BinType> EnumBinType
        {
            get { return _EnumBinType; }
            set
            {
                if (value != _EnumBinType)
                {
                    _EnumBinType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _ExistPrefixInRetVal
            = new Element<bool>() { Value = false };
        public Element<bool> ExistPrefixInRetVal
        {
            get { return _ExistPrefixInRetVal; }
            set
            {
                if (value != _ExistPrefixInRetVal)
                {
                    _ExistPrefixInRetVal = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _UseAutoLocationNum
            = new Element<bool>() { Value = false };
        public Element<bool> UseAutoLocationNum
        {
            get { return _UseAutoLocationNum; }
            set
            {
                if (value != _UseAutoLocationNum)
                {
                    _UseAutoLocationNum = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _SpecialGCMDMultiDutMode
            = new Element<bool>() { Value = false };
        public Element<bool> SpecialGCMDMultiDutMode
        {
            get { return _SpecialGCMDMultiDutMode; }
            set
            {
                if (value != _SpecialGCMDMultiDutMode)
                {
                    _SpecialGCMDMultiDutMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _TelGCMDForcedOver8MultiDutMode
            = new Element<bool>() { Value = false };
        public Element<bool> TelGCMDForcedOver8MultiDutMode
        {
            get { return _TelGCMDForcedOver8MultiDutMode; }
            set
            {
                if (value != _TelGCMDForcedOver8MultiDutMode)
                {
                    _TelGCMDForcedOver8MultiDutMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _TelGCMDMultiTestYesNoFM
            = new Element<bool>() { Value = false };
        public Element<bool> TelGCMDMultiTestYesNoFM
        {
            get { return _TelGCMDMultiTestYesNoFM; }
            set
            {
                if (value != _TelGCMDMultiTestYesNoFM)
                {
                    _TelGCMDMultiTestYesNoFM = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _TelSendSlotNumberForSmallNCommand
            = new Element<int>() { Value = 0 };
        public Element<int> TelSendSlotNumberForSmallNCommand
        {
            get { return _TelSendSlotNumberForSmallNCommand; }
            set
            {
                if (value != _TelSendSlotNumberForSmallNCommand)
                {
                    _TelSendSlotNumberForSmallNCommand = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _EdgeCorrection
            = new Element<int>() { Value = 50 };
        public Element<int> EdgeCorrection
        {
            get { return _EdgeCorrection; }
            set
            {
                if (value != _EdgeCorrection)
                {
                    _EdgeCorrection = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ConsecutiveFailMode
            = new Element<int>() { Value = 1 };
        public Element<int> ConsecutiveFailMode
        {
            get { return _ConsecutiveFailMode; }
            set
            {
                if (value != _ConsecutiveFailMode)
                {
                    _ConsecutiveFailMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _ConsecutiveFailSkipLine
            = new Element<int>() { Value = 0 };
        public Element<int> ConsecutiveFailSkipLine
        {
            get { return _ConsecutiveFailSkipLine; }
            set
            {
                if (value != _ConsecutiveFailSkipLine)
                {
                    _ConsecutiveFailSkipLine = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _MultiTestLocFor2Chan
            = new Element<int>() { Value = 0 };
        public Element<int> MultiTestLocFor2Chan
        {
            get { return _MultiTestLocFor2Chan; }
            set
            {
                if (value != _MultiTestLocFor2Chan)
                {
                    _MultiTestLocFor2Chan = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<string> _TelMultiTestYesNo
            = new Element<string>() { Value = "NONE" };
        public Element<string> TelMultiTestYesNo
        {
            get { return _TelMultiTestYesNo; }
            set
            {
                if (value != _TelMultiTestYesNo)
                {
                    _TelMultiTestYesNo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<int> _AddSignForLargeA = new Element<int>() { Value = 1 };
        public Element<int> AddSignForLargeA
        {
            get { return _AddSignForLargeA; }
            set
            {
                if (value != _AddSignForLargeA)
                {
                    _AddSignForLargeA = value;
                    RaisePropertyChanged();
                }
            }
        }

        //device param에 있던 파라미터.
        private Element<bool> _IsTestCompleteIncluded = new Element<bool>() { Value = false };
        public Element<bool> IsTestCompleteIncluded
        {
            get
            {
                return _IsTestCompleteIncluded;
            }
            set
            {
                if (value != _IsTestCompleteIncluded)
                {
                    _IsTestCompleteIncluded = value;
                    RaisePropertyChanged();
                }
            }
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                EnumBinType.Value = BinType.BIN_PASSFAIL;
                EnumGpibComType.Value = EnumGpibCommType.TSK_EMUL;
                BoardIndex.Value = 0;
                ExistPrefixInRetVal.Value = true;
                EnumGpibProbingMode.Value = ProberInterfaces.EnumGpibProbingMode.INTERNAL;
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception)
            {
                throw new Exception("Error during Setting Default Param From GpibSysParam.");
            }

            return RetVal;
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {
                EnumGPIBEnable.Value = EnumGpibEnable.DISABLE;

                EnumBinType.Value = BinType.BIN_PASSFAIL;
                EnumGpibComType.Value = EnumGpibCommType.TSK;
                BoardIndex.Value = 0;
                ExistPrefixInRetVal.Value = false;
                EnumGpibProbingMode.Value = ProberInterfaces.EnumGpibProbingMode.INTERNAL;
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From GpibSysParam. {err.Message}");
            }

            return RetVal;
        }
    }

    [Serializable]
    public class GpibRequestParam : ISystemParameterizable, INotifyPropertyChanged
    {
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<object> Nodes { get; set; }
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

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Debug($"[GpibCmdNFuncConnectParam] [Method = Init] [Error = {err}]");
                retval = EventCodeEnum.PARAM_ERROR;
            }

            return retval;
        }
        public void SetElementMetaData()
        {

        }
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
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
        public string FileName { get; } = "GpibRequestParam.json";

        [XmlIgnore, JsonIgnore]
        public EnumGpibCommType GpibCommType { get; set; } = EnumGpibCommType.TSK;
        [XmlIgnore, JsonIgnore]
        public EnumGpibProbingMode GpibProbingMode { get; set; } = EnumGpibProbingMode.INTERNAL;

        private List<CommunicationRequestSet> _GpibRequestSetList;
        public List<CommunicationRequestSet> GpibRequestSetList
        {
            get { return _GpibRequestSetList; }
            set
            {
                if (value != _GpibRequestSetList)
                {
                    _GpibRequestSetList = value;
                    RaisePropertyChanged(nameof(GpibRequestSetList));
                }
            }
        }

        public GpibRequestParam()
        {
            GpibRequestSetList = new List<CommunicationRequestSet>();
        }

        public string Genealogy { get; set; } = "GpibRequestParam";

        public EventCodeEnum SetDefaultParam(EnumGpibCommType commType)
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;

            try
            {
                GpibCommType = commType;

                if (GpibCommType == EnumGpibCommType.TEL
                    || GpibCommType == EnumGpibCommType.TEL_EMUL)
                {
                    GpibRequestSetList.AddRange(SetDefault_TEL());
                }
                else if (GpibCommType == EnumGpibCommType.TSK
                    || GpibCommType == EnumGpibCommType.TSK_EMUL)
                {

                    GpibRequestSetList.AddRange(SetDefault_TSK());
                    GpibRequestSetList.AddRange(SetDefault_UR());

                }
                else if (GpibCommType == EnumGpibCommType.TSK_SPECIAL
                    || GpibCommType == EnumGpibCommType.TSK_SPECIAL_EMUL)
                {
                    GpibRequestSetList.AddRange(SetDefault_TSK_Special());
                    GpibRequestSetList.AddRange(SetDefault_UR());

                }
                RetVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From GpibRequestParam. {err.Message}");
            }
            return RetVal;
        }


        public EventCodeEnum SetDefaultParam()
        { 
            return EventCodeEnum.NONE;
        }

        public List<CommunicationRequestSet> SetDefault_TSK()
        {           
            List<CommunicationRequestSet> tmpGpibRequestSet = new List<CommunicationRequestSet>();

            try
            {
                //<< Action >>
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "D", Request = new ZDown()});// = "Z Axis Down" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "J", Request = new MoveToNextPosition()}); //= "Chuck Move Next Die Position" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "U", Request = new UnloadWafer()}); //= "Wafer Unload" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "U0", Request = new UnloadAllWaferAction()}); //= "All Wafer Unload" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "Z", Request = new ZUp()}); //= "Z Axis Zup" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "L", Request = new UnloadWafer()}); //= "Wafer Unload" });

                //<< Query >>
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "Q",
                    Request = new RequestCore.QueryPack.QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                         {
                            new QueryHasAffix(){
                                Data = new QueryFill(){ Data = new ProbingUserYIndex(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "3" },
                                              Vector = true
                                              },
                                prefix = "Y"
                            },

                            new QueryHasAffix(){
                                Data = new QueryFill(){ Data = new ProbingUserXIndex(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "3" },
                                              Vector = true
                                              },
                                prefix = "X"
                            },
                        }
                    },
                    ////= "Probing User Index Y/X"
                });// Q
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "O", Request = new GetOnWaferInformation()}); //= "On Wafer Information" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "B", Request = new ProberID()}); //= "Prober Name" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "f",
                    Request = new RequestCore.QueryPack.QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                         {
                             new QueryFill(){ Data = new ToInt(){ Argument =  new Calculator("*") { x = new FixData(){ ResultData = "10" }, y = new GetCurrentChuckTemp() } },
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "4" },
                                              Vector = true
                                              },
                              new QueryFill(){ Data = new ToInt(){ Argument =  new Calculator("*") { x = new FixData(){ ResultData = "10" }, y = new GetSettingChuckTemp() } },
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "4" },
                                              Vector = true
                                              },
                        }
                    },
                    //= "Current Chuck Temperature + Setting Chuck Temperature"

                });// f
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "f1", Request = new TSK_SettingTemperature()}); //= "Chuck Setting Temperature" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "w", Request = new GetCassetteStatus() { linkchar = "." }});// = "Cassette Status" });
                //tmpGpibRequestSet.Add(new CommunicationRequestSet()
                //{
                //    Name = "b",
                //    Request = new QueryFill()
                //    {
                //        Data = new LotName(),
                //        RightPaddingData = new WaferID(),
                //        Length = new FixData() { ResultData = "19" },
                //    },
                //    //= "wafer id"
                //});// b
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "b",
                    Request = new QueryFill()
                    {
                        Data = new WaferID(),
                        RightPaddingData = new FixData() { ResultData = " " },
                        Length = new FixData() { ResultData = "12" },
                    },
                    //= "wafer id"
                });// b
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "d", Request = new GetCassetteStatus() { linkchar = "." }});// = "Cassette Staus" });
                //tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "i", Request = new TSK_DeviceInfoReq() }); 
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "i00",
                    Request = new QueryFill()
                    {
                        Data = new WaferID(),
                        PaddingData = new FixData() { ResultData = " " },
                        Length = new FixData() { ResultData = "19" },
                    },
                    //= "Wafer Name"

                });// i00
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "i01",
                    Request = new QueryHasAffix()
                    {
                        Data = new QueryDownUnit() { Data = new WaferSize(), Argument = 1000 },
                        DataFormat = "{0,3:000}",
                        postfix = "m",
                    },
                    //= "Wafer Size"

                });// i01
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "i20",
                    Request = new QueryFill()
                    {
                        Data = new CardID(),
                        PaddingData = new FixData() { ResultData = " " },
                        Length = new FixData() { ResultData = "5" },
                    },
                    //= "Card Name"
                });// i20
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "i22",
                    Request = new QueryHasAffix()
                    {
                        Data = new QueryNumToAscii() { Data = new WaferThickness() },
                        DataFormat = "{0,4:0000}",
                    },
                    //= "Wafer Thickness"
                });// i22
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "i24",
                    Request = new QueryHasAffix()
                    {
                        Data = new QueryNumToAscii() { Data = new OverDrive() },
                        DataFormat = "{0,5:00000}"
                    },
                    //= "Probing Overdrive"
                });// i24
                   //tmpGpibRequestSet.Add(new CommunicationRequestSet()
                   //{
                   //    Name = "V",
                   //    Request = new QueryFill()
                   //    {
                   //        Data = new LotName(),
                   //        RightPaddingData = new FixData() { ResultData = " " },
                   //        Length = new FixData() { ResultData = "16" },
                   //    },
                   //    //= "Lot Name"
                   //});// V
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "V",
                    Request = new QueryFill()
                    {
                        Data = new LotName(),
                        RightPaddingData = new FixData() { ResultData = " " },
                        Length = new FixData() { ResultData = "8" },
                    },
                    //= "Lot Name"
                });// V

                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "TSC", Request = new GetTestSampleInform()}); //= "Test Sample Infomation" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "DR", Request = new GetDataUsingDRID()}); //= "DR" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "DW", Request = new SetDataUsingDWID()}); //= "DW" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "ku",
                    Request = new QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                        {
                             ////-- WaferName
                             //new QueryFill(){ Data =  new WaferID(),
                             //                 PaddingData = new FixData() { ResultData = " " },
                             //                 Length = new FixData() { ResultData = "12" },
                             //                 },
                              //-- DeviceName
                             new QueryFill(){ Data =  new DeviceName(),
                                              PaddingData = new FixData() { ResultData = " " },
                                              //Length = new FixData() { ResultData = "12" },
                                              },

                              //-- WaferSizeInch
                             new QueryFill(){ Data =  new WaferSizeInch(),
                                              Length = new FixData() { ResultData = "12" },
                                              },

                             //-- FlatOrientation
                             new QueryFill(){ Data =  new FlatOrientation(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "3" },
                                              },

                              //-- DeviceSizeWidth
                             new QueryFill(){ Data =  new DeviceSizeWidth(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "5" },
                                              },

                             //-- DeviceSizeHeight 
                              new QueryFill(){ Data =  new DeviceSizeHeight(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "5" },
                                              },

                              //-- Fixed0 
                              new QueryFill(){ Data =  new DeviceSizeHeight(),
                                              PaddingData = new FixData() { ResultData = "000000000000000000XN11" },
                                              Length = new FixData() { ResultData = "22" },
                                              },

                             //-- WaferRadius 
                              new QueryFill(){ Data =  new WaferRadius100um(),
                                              Length = new FixData()  { ResultData = "4" },
                                              },

                               //-- TargetSense 
                              new QueryFill(){ PaddingData = new FixData() { ResultData = "0" },
                                                Length = new FixData() { ResultData = "1" }
                                                },

                              
                              //-- Fixed1
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "000000000000" },
                                              Length = new FixData() { ResultData = "12" },
                                              },

                              //-- StdChipX
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "+00" },
                                              Length = new FixData() { ResultData = "3" },
                                              },

                              //-- StdChipY
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "+00" },
                                              Length = new FixData() { ResultData = "3" },
                                              },

                               //-- Fixed2
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "N000000N00" },
                                              Length = new FixData() { ResultData = "12" },
                                              },

                              //-- ProbeAreaSelect
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "Y" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              //-- EdgeCorrect
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "100" },
                                              Length = new FixData() { ResultData = "3" },
                                              },

                              
                              //-- SampleProbe
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                               //-- SampleFixed
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "+00+00+00+00+00+00+00+00+00+00+00+00+00+00+00+00" },
                                              Length = new FixData() { ResultData = "48" },
                                              },

                               //-- PassNet
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "65535" },
                                              Length = new FixData() { ResultData = "5" },
                                              },

                              //-- Fixed3
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "000" },
                                              Length = new FixData() { ResultData = "3" },
                                              },
                               //-- AlignAxis
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "X" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                               //-- AutoFocus
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "1" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              
                               //-- AutoFocus
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "9" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              
                               //-- AlignSizeX
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "9" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              
                               //-- AlignSizeY
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "9" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              
                               //-- MonitorChipX
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "9" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              
                               //-- MonitorChipY
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "9" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                             //-- Fixed4
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "00000000" },
                                              Length = new FixData() { ResultData = "8" },
                                              },

                             //-- MultiChip
                              new QueryFill(){
                                              Data = new MultiChip(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                             //-- MultiLocation
                              new QueryFill(){
                                              Data = new MultiSiteLocation(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              },

                               //-- UnloadStop
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "N" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                                //-- StopCounter
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              },

                              //-- TestRejectCst
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" }, //"0":LOAD CST, 1:REJECT CST....?  --- Fixed
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              //-- AlignRejectCst
                              new QueryFill(){
                                              Data = new MultiSiteLocation(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                             //-- ContFailProcess1
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              },
                               //-- ContFailProcess2
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              //-- ContFailLimit
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "002" },
                                              Length = new FixData() { ResultData = "3" },
                                              },

                               //-- SkipChipRow
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              },

                               //-- CheckBackCount
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                              //-- PolishAfterChkBack
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "N" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                                 //-- NC_Enable
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              },

                                 //-- NC_ZCount
                              new QueryFill(){
                                              Data = new NCContactCount(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              },

                                 //-- NC_ChipCount
                              new QueryFill(){
                                              Data = new NCDieInterval(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "4" },
                                              },

                                 //-- NC_WaferCount
                              new QueryFill(){
                                              Data = new NCwaferInterval(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              },

                                 //-- NC_Overdrive
                              new QueryFill(){
                                              Data = new NCOverdrive(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "3" },
                                              },

                                //-- NC_TotalCount
                              new QueryFill(){
                                              Data = new NCContactCounter(),
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "4" },
                                              },

                               //-- HotChuckTemp
                              new QueryFill(){
                                              Data = new QueryFloor(){ Data = new SetTemp()} ,
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "3" },
                                              },

                              //-- PresetAddrX
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              },

                                //-- PresetAddrY
                              new QueryFill(){
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              },
                        }

                    },
                    //= " Up-load a device parameter group"

                });// ku
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "H",
                    Request = new QueryFill()
                    {
                        Data = new MultiSiteLocation(),
                        PaddingData = new FixData() { ResultData = "0" },
                        Length = new FixData() { ResultData = "2" },
                    },
                    //= "Multi Size Location"
                });// H
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "X",
                    Request = new QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                        {
                            new QueryFill(){
                                Data = new CurrentSlotNumber(),
                                PaddingData = new FixData() { ResultData = "0" },
                                Length = new FixData() { ResultData = "2" },
                            },
                            new QueryFill(){
                                Data = new CassetteNum(),
                                PaddingData = new FixData() { ResultData = "0" },
                                Length = new FixData() { ResultData = "1" },
                            },
                        }
                    },
                    //= "SlotNumber + Origin CassetteNumber"
                });  // X
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "x",
                    Request = new QueryMerge()
                    {

                        //x : [Query]  Request cassette status (aa + bb + cc) ** PROC_SMALL_x
                        //              aa: slot number of loaded wafer on chuck (decimal)
                        //              bb: number of not finished wafers in the origin cassette (decimal)
                        //              cc: Cassette No.1 Status ID + Cassette No.2 Status ID
                        //                  (ID = Cassette Status 
                        //                          0: No Cassette
                        //                          1: Ready to test
                        //                          2: Testing under way
                        //                          3: Testing finished 
                        //                          4: Rejected wafer cassette )
                        //              ** semics12 내용 <-- 해당 내용 반영
                        //                   format; "x543210[CR][LF]",
                        //                   54:slot number of wafer on chuck
                        //                   32:number of not finished wafer in the cassette
                        //                   1:wafer status (ref. w command)
                        //                   0:cassette No. (always "1" for SWIRI)

                        Querys = new List<RequestBase>()
                         {
                             new QueryFill(){ Data = new CurrentSlotNumber() ,
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              Vector = true
                                              },
                              new QueryFill(){ Data = new LeftWaferCount() ,
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "2" },
                                              Vector = true
                                              },
                               new QueryFill(){ Data = new WaferStatus() ,
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              Vector = true
                                              },
                                new QueryFill(){ Data = new CassetteNum() ,
                                              PaddingData = new FixData() { ResultData = "0" },
                                              Length = new FixData() { ResultData = "1" },
                                              Vector = true
                                              },

                        },
                    },
                    //= "Cassette Status"

                });// x

                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "M",
                    Request = new RequestCore.QueryPack.QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                        {
                            new GetCalcualtePassFailYield(),
                            //new MoveToNextPosition(), // MPT에서는 J커맨드에서 MoveNextDieState로 보냄
                                                      //new CheckRemainSequence()
                        }
                    },
                    //= "Calculate Bin Data + Chuck Move Next Position"
                });// M
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "ms",
                    Request = new RequestCore.Controller.Branch()
                    {
                        ////1. Waiting for the lot start 'I'
                        ////2. Card replacement going on 'C'
                        ////3. Lot process going on 'R'
                        ////4. Waiting for the operator's help in an error 'E'

                        Condition = new IsLotIdleState(),
                        Positive = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsCardChangeGoingOn(),
                            Positive = new FixData() { ResultData = "C" },
                            Negative = new FixData() { ResultData = "I" },
                        },
                        Negative = new RequestCore.Controller.Branch()
                        {
                            Condition = new IsLotRunningState(),
                            Positive = new FixData() { ResultData = "R" },
                            Negative = new RequestCore.Controller.Branch()
                            {
                                Condition = new IsLotPausedState(),
                                Positive = new FixData() { ResultData = "E" },
                                Negative = new GpibException() { Argument = "Exception from ms Command." }
                            }
                        }
                    },
                    //= "Prober status request"

                });// ms






            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return tmpGpibRequestSet;
        }

        public List<CommunicationRequestSet> SetDefault_TSK_Special()
        {
            List<CommunicationRequestSet> tmpGpibRequestSet = new List<CommunicationRequestSet>();
            try
            {

                //<< Action >>
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "D", Request = new ZDown()}); //= "Z Axis Down" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "Z", Request = new ZUp()}); //= "Z Axis Up" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "U", Request = new UnloadWafer()}); //= "Wafe Unload" });

                //<< Query >>
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "G", Request = new TSK_WaferParameter()}); //= "Wafer Information" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "f1", Request = new TSK_SettingTemperature()}); //= "Probing Setting Temperature" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "C", Request = new GetCalcualtePassFailYield()}); //= "Calcualte Bin Pass/Fail/Yield" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "n", Request = new GetLoadedWaferId()}); //= "Wafer Name" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "d", Request = new GetCassetteStatus()}); //= "Cassette Status" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "Q", Request = new MoveToNextPosition()}); //= "Chuck Move Next Die Position" });


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return tmpGpibRequestSet;
        }


        public List<CommunicationRequestSet> SetDefault_TEL()
        {
            List<CommunicationRequestSet> tmpGpibRequestSet = new List<CommunicationRequestSet>();
            try
            {
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "A", Request = new GetTelXYCoordinate()}); //= "XY Coordinates Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "B", Request = new MoveToFirstDie()}); //= "Transfer to Reference Die" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "C",
                    Request = new RequestCore.QueryPack.QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                    {
                        new GetCalcualtePassFailYield(),
                        new MoveToNextPosition(),
                        //new CheckRemainSequence()
                    }
                    }
                    ,
                    //= "Pass, Fail, BIN Input"
                });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "CD", Request = new GetCalcualtePassFailYield()}); //= "Calcualte Bin Pass/Fail/Yield" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "CO", Request = new GetCenterDieOffset()}); //= "CenterDie Offset" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "D", Request = new ZDown()}); //= "Z Axis Down" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "DD", Request = new GetDieSize()}); //= "Die Size" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "E", Request = new GetPassFailCount()}); //= "Pass, Fail Count Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "F", Request = new GetMachineNumber()}); //= "Machine Number Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "Ft", Request = new GetProberType()}); //= "Prober Name" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "F1", Request = new GetProberId()}); //= "Chuck Probing Temperature Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "G", Request = new GetLotInfomation()}); //= "Wafer Parameter Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "G#003", Request = new GetMultipleChannelParam()}); //= " Multiple Channel Parameters Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "N", Request = new SetDeviceName()}); //= "Wafer Name" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "O",
                    Request = new RequestCore.QueryPack.QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                    {
                        new RequestCore.QueryPack.FixData(){ResultData = "O"},
                        new GetOnWaferInformation(),
                    }
                    ,

                    },
                    //= "On Wafer Request"
                });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "OD", Request = new SetOverDrive()}); //= "Probing OverDrive" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "RC",
                    Request = new RequestCore.QueryPack.QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                         {
                                    new RequestCore.QueryPack.FixData(){ResultData = "X"},
                                    new RequestCore.QueryPack.RefUIndexX(),
                                    new RequestCore.QueryPack.FixData(){ResultData = "Y"},
                                    new RequestCore.QueryPack.RefUIndexY(),
                        }


                    },
                    //= "Ref User Index"

                });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "S", Request = new GetCassetteStatus() { linkchar = "." }}); //= "Cassette Status Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "T", Request = new TestStart()}); //= "Test Start" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "U", Request = new UnloadWafer()}); //= "Unload Wafer" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "V", Request = new GetTotalTestDieCount()}); //= "Total TestDie Count" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "WD", Request = new GetProbingOverdrive()}); //= "Probing Overdrive" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "WF", Request = new GetWaferOrientation()}); //= "Wafer Orientation" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "WS", Request = new GetCurrentWaferSize()}); //= "WaferSize" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "WU", Request = new GetUnitsOfWaferAndDieDimensions()}); //= "Units Of Wafer And Die Demension" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "Z+", Request = new ZIncrement()}); //= "Z Axis Drive" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "Z-", Request = new ZDecrement()}); //= "Z Axis Drive" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "Z", Request = new ZUp()}); //= "Z Axis Up" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet()
                {
                    Name = "a",
                    Request = new RequestCore.QueryPack.QueryMerge()
                    {
                        Querys = new List<RequestBase>()
                    {
                                    new RequestCore.QueryPack.FixData(){ResultData = "X"},
                                    new RequestCore.QueryPack.OrgUIndexX(),
                                    new RequestCore.QueryPack.FixData(){ResultData = "Y"},
                                    new RequestCore.QueryPack.OrgUIndexY(),
                    }
                    }
                    ,
                    //= "Reference Die Coordinates Request"
                });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "b", Request = new PosDieToDesiredPos()}); //= "Designated Coordinate Indexing" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "d", Request = new GetCassetteMapInfo()}); //= "Cassette Map Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "e", Request = new BuzzerOnAndStopProber()}); //= "Buzzer ON, Stop" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "f1", Request = new GetCurrentChuckTemp()}); //= "Chuck Probing Temperature Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "f2", Request = new GetSettingChuckTemp()}); //= "Chuck Setting Temperature Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "g", Request = new GetLotParameters()}); //= "Lot Parameter Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "h", Request = new SetHotCuckTemp()}); //= "Chuck Temperature Setting" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "n", Request = new GetLoadedWaferId()}); //= "Wafer ID Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "p", Request = new NeedleCleaning()}); //= "Needle Polish" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "pt", Request = new GetProbeCardName()}); //= "ProbeCard Name" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ptc", Request = new GetProbeCardName()}); //= "ProbeCard Name" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "r", Request = new IdxControlForFailInput()}); //= "Index (Fail input)" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "s", Request = new GetErrorCode()}); //= "Error, Assist Code Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "w", Request = new GetWaferNumber()}); //= "Wafer Number Request" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "?aAD003", Request = new GetProbingOverdrive()}); //= "Probing Overdrive" });


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return tmpGpibRequestSet;
        }

        public List<CommunicationRequestSet> SetDefault_UR()
        {
            List<CommunicationRequestSet> tmpGpibRequestSet = new List<CommunicationRequestSet>();
            try
            {
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur0033", Request = new QueryHasAffix() { Data = new GetElement("TempController.TempControllerDevParam.SetTemp"), postfix = "deg" }}); //= "Setting Temperature" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur0137", Request = new QueryHasAffix() { Data = new GetElement("PinAligner.PinAlignDevParameters.PinAlignSettignParameter[0].SamplePinAlignmentPinCount"), DataFormat = "{00}" }}); //= "PinAlign Sample Pin Count" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur0204", Request = new QueryFill() { Data = new GetElement("Probing.ProbingModuleDevParam.OverDrive"), Length = new FixData() { ResultData = "3" }, PaddingData = new FixData() { ResultData = "0" }, Vector = true }}); //= "OverDrive" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur0281", Request = new ToInt() { Argument = new GetElement("PMIModule.PMIModuleDevParam.NormalPMI") }}); //= "PMI Enable" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1224", Request = new UserToTskMapDir() { WaferMapDir = new GetElement("WaferObject.WaferDevObject.PhysInfo.MapDirX") }}); //= "Wafer Orientaion Direction X" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1225", Request = new UserToTskMapDir() { WaferMapDir = new GetElement("WaferObject.WaferDevObject.PhysInfo.MapDirY") }}); //= "Wafer Orientaion Direction Y" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1227", Request = new QueryHasAffix() { Data = new Calculator("/") { x = new GetElement("SoakingModule.SoakingDeviceFile.ProbeCardChangeEventSoaking[2].SoakingTimeInSeconds"), y = new FixData() { ResultData = "60" } }, postfix = "min", DataFormat = "{00}" }}); //= "PreHeating Time" });

                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1", Request = new GetElement("WaferObject.WaferDevObject.Info.DeviceName")}); //= "Device Name" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur221", Request = new GetElement("Probing.ProbingModuleDevParam.BinFuncParam.ContinuousFailEnable")}); //= "Bin Continuous Fail" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur223", Request = new GetElement("Probing.ProbingModuleDevParam.BinFuncParam.BinAnyCountLimit")}); //= "Bin Any Count Limit" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1334", Request = new GetElement("PolishWaferModule.PolishWaferParameter.PolishWaferIntervalParameter[0].PolishWaferCleaningParameter[0].Thickness")}); //= "Polish Wafer Cleaning Thickness" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1337", Request = new GetElement("PolishWaferModule.PolishWaferParameter.PolishWaferIntervalParameter[0].IntervalCount")}); //= "Polish Wafer Cleaning Interval Count" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1338", Request = new GetElement("PolishWaferModule.PolishWaferParameter.PolishWaferIntervalParameter[0].PolishWaferCleaningParameter[0].OverdriveValue")}); //= "Polish Wafer Cleaning Overdrive" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur1339", Request = new GetElement("PolishWaferModule.PolishWaferParameter.PolishWaferIntervalParameter[0].TouchdownCount")}); //= "Polish Wafer Cleaning TouchDown Count" });
                tmpGpibRequestSet.Add(new CommunicationRequestSet() { Name = "ur3801", Request = new GetElement("Probing.ProbingModuleDevParam.MultipleContactCount")}); //= "Multiple Contact Count" });

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return tmpGpibRequestSet;
        }

        public EventCodeEnum SetEmulParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                retVal = this.SetDefaultParam();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }


    [Serializable]
    public class GPIBCommandRecipe : CommandRecipe
    {


        public override EventCodeEnum Init()
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

        public GPIBCommandRecipe() { }

        [XmlIgnore, JsonIgnore]
        public override string FilePath { get; } = "GPIB";
        [XmlIgnore, JsonIgnore]
        public override string FileName { get; } = "Command_RECIPE_GPIB.Json";

        public override EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal;
            try
            {

                CommandDescriptor desc;

                desc = CommandDescriptor.Create<ISRQ_RESPONSE>();
                AddRecipe(desc);

                desc = CommandDescriptor.Create<IGpibAbort>();
                AddRecipe(desc);

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override EventCodeEnum SetEmulParam()
        {
            return this.SetDefaultParam();
        }
    }

    [Serializable]
    public class GPIBDataMappingTable : ISystemParameterizable
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region ==> IParam Implement
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; } = "GPIBDataMappingTable";
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


        private Element<Dictionary<int, int>> _GPIB_DRDW_Table = new Element<Dictionary<int, int>>();
        public Element<Dictionary<int, int>> GPIB_DRDW_Table
        {
            get { return _GPIB_DRDW_Table; }
            set
            {
                _GPIB_DRDW_Table = value;
            }
        }

        #endregion
        public string FilePath { get; } = "GPIB";
        public string FileName { get; } = "GPIBDataMappingTable.Json";

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        [XmlIgnore, JsonIgnore]
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

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                // 150004                         Setting Temperatrue
                // 150005                         Current Temperatrue

                GPIB_DRDW_Table.Value.Add(150004, 150004);
                GPIB_DRDW_Table.Value.Add(150005, 150004);
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
    }


}
