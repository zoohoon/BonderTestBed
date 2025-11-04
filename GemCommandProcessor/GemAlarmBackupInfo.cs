using LogModule;
using Newtonsoft.Json;
using ProberErrorCode;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XGemCommandProcessor
{
    public class GemAlarmBackupInfo : INotifyPropertyChanged, IParamNode, IParam, ISystemParameterizable
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #endregion

        #region => IParamNode
        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
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

        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }
        #endregion

        #region => IParam

        public string FilePath { get; set; } = "";

        public string FileName { get; set; } = "";

        public EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum SetDefaultParam()
        {
            try
            {
                Initialize();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return EventCodeEnum.NONE;
        }
        public void SetElementMetaData()
        {

        }

        #endregion

        [JsonIgnore]
        public string FileFullPath { get; set; } = "";

        private List<GemAlarmStateInfo> _GemAlarmStateInfos
             = new List<GemAlarmStateInfo>();

        public List<GemAlarmStateInfo> GemAlarmStateInfos
        {
            get { return _GemAlarmStateInfos; }
            set { _GemAlarmStateInfos = value; }
        }





        public void Initialize()
        {
            try
            {
                GemAlarmStateInfos.Clear();
                GemAlarmStateInfos.Add(new GemAlarmStateInfo());
                if (SystemManager.SysteMode == SystemModeEnum.Multiple)
                {
                    for (int i = 1; i <= SystemModuleCount.ModuleCnt.StageCount; i++)
                    {
                        GemAlarmStateInfos.Add(new GemAlarmStateInfo(i));
                    }
                }
                
             
                LoggerManager.Debug($"[GemAlarmBackupInfo] Initialize.");                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }


    }

    public class GemAlarmStateInfo
    {
        //private GemProcessorType _ProcessorType;

        //public GemProcessorType ProcessorType
        //{
        //    get { return _ProcessorType; }
        //    set { _ProcessorType = value; }
        //}

        private int _CellIndex;
        /// <summary>
        /// 만약 GemProcessorType이 Commander이나 Single일 경우 0, Cell일 경우는 StageNumber
        /// </summary>
        public int CellIndex
        {
            get { return _CellIndex; }
            set { _CellIndex = value; }
        }

        private List<long> _ALIDs = new List<long>();

        public List<long> ALIDs
        {
            get { return _ALIDs; }
            set { _ALIDs = value; }
        }

      
        public GemAlarmStateInfo(int cellIdx = 0)
        {
            //ProcessorType = processorType;
            CellIndex = cellIdx;
        }
    }
}
