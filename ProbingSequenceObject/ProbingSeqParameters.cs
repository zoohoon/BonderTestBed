using System;
using System.Collections.Generic;
using ProberInterfaces;
using System.ComponentModel;
using System.Xml.Serialization;
using ProberErrorCode;
using LogModule;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace ProbingSequenceObject
{
    public enum ProbingTypeEnum
    {
        UNDEFINED = 0,
        CP1,
        REPROBE,
        SECONDREPROBE,
        CP2
    }

    [Serializable]
    public class ProbingSequenceSet : INotifyPropertyChanged, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Element<List<MachineIndex>> _ProbingSeq = new Element<List<MachineIndex>>();
        public Element<List<MachineIndex>> ProbingSeq
        {
            get { return _ProbingSeq; }
            set
            {
                if (value != _ProbingSeq)
                {
                    _ProbingSeq = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<ProbingTypeEnum> _ProbingType = new Element<ProbingTypeEnum>();
        public Element<ProbingTypeEnum> ProbingType
        {
            get { return _ProbingType; }
            set
            {
                if (value != _ProbingType)
                {
                    _ProbingType = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ProbingSequenceSet(ProbingTypeEnum type)
        {
            this.ProbingType.Value = type;
        }
    }


    [Serializable]
    public class ProbingSeqParameters : INotifyPropertyChanged, IProbingSequenceParameter, IParamNode
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }
        public List<object> Nodes { get; set; }
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

        private void MakeProbingSeqSets()
        {
            if ((ProbingSequenceSets == null) || (ProbingSequenceSets.Count < 4))
            {
                ProbingSequenceSets = new List<ProbingSequenceSet>();
                ProbingSequenceSets.Add(new ProbingSequenceSet(ProbingTypeEnum.CP1));
                ProbingSequenceSets.Add(new ProbingSequenceSet(ProbingTypeEnum.REPROBE));
                ProbingSequenceSets.Add(new ProbingSequenceSet(ProbingTypeEnum.SECONDREPROBE));
                ProbingSequenceSets.Add(new ProbingSequenceSet(ProbingTypeEnum.CP2));
            }
        }

        public EventCodeEnum Init()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                MakeProbingSeqSets();

                ProbingSequenceSet tmp = ProbingSequenceSets.Find(x => x.ProbingType.Value == ProbingTypeEnum.CP1);

                if((tmp?.ProbingSeq?.Value?.Count ?? -1) == 0)
                {
                    tmp.ProbingSeq.Value = ProbingSeq.Value;
                }

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

        [ParamIgnore]
        public string FilePath { get; } = "";

        [ParamIgnore]
        public string FileName { get; } = "ProbingSequence.Json";

        private Element<List<MachineIndex>> _ProbingSeq = new Element<List<MachineIndex>>();
        public Element<List<MachineIndex>> ProbingSeq
        {
            get { return _ProbingSeq; }
            set
            {
                if (value != _ProbingSeq)
                {
                    _ProbingSeq = value;
                    RaisePropertyChanged();
                }
            }
        }
        private List<ProbingSequenceSet> _ProbingSequenceSets;
        public List<ProbingSequenceSet> ProbingSequenceSets
        {
            get { return _ProbingSequenceSets; }
            set
            {
                if (value != _ProbingSequenceSets)
                {
                    _ProbingSequenceSets = value;
                    RaisePropertyChanged();
                }
            }
        }


        public ProbingSeqParameters()
        {
            try
            {
                ProbingSeq = new Element<List<MachineIndex>>();
                ProbingSeq.Value = new List<MachineIndex>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }
        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                ProbingSeq = new Element<List<MachineIndex>>();
                ProbingSeq.Value = new List<MachineIndex>();

                MakeProbingSeqSets();

                retVal = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                throw new Exception($"Error during Setting Default Param From {this.GetType().Name}. {err.Message}");
            }
            return retVal;
        }
    }
}
