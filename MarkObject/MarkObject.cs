using Newtonsoft.Json;
using ProberInterfaces;
using ProberInterfaces.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace MarkObjects
{
    using LogModule;
    [Serializable]
    public class MarkObject : IMarkObject, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public string Genealogy { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public object Owner { get; set; }

        [XmlIgnore, JsonIgnore, ParamIgnore]
        public List<Object> Nodes { get; set; }

        [NonSerialized]
        private Element<AlignStateEnum> _AlignState
             = new Element<AlignStateEnum>();
        [XmlIgnore, JsonIgnore]
        public Element<AlignStateEnum> AlignState
        {
            get { return _AlignState; }
            set
            {
                if (value != _AlignState)
                {
                    _AlignState = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Element<bool> _MarkSetupChangedToggle
             = new Element<bool>();

        public Element<bool> MarkSetupChangedToggle
        {
            get { return _MarkSetupChangedToggle; }
            set { _MarkSetupChangedToggle = value; }
        }



        [NonSerialized]
        private bool _DoMarkAlign = false;
        [XmlIgnore, JsonIgnore]
        public bool DoMarkAlign
        {
            get { return _DoMarkAlign; }
            set { _DoMarkAlign = value; }
        }

        public AlignStateEnum GetAlignState()
        {
            AlignStateEnum state = AlignStateEnum.IDLE;
            try
            {
                if (AlignState.DoneState == ElementStateEnum.DONE)
                    state = AlignStateEnum.DONE;
                else
                    state = AlignStateEnum.IDLE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return state;
            //return AlignState.Value;
        }

        public void SetAlignState(AlignStateEnum state)
        {
            try
            {
                if (state != AlignState.Value)
                {
                    this.LoaderController()?.UpdateLotDataInfo(StageLotDataEnum.MARKALIGNSTATE, state.ToString());

                    LoggerManager.Debug($"MarkObject.SetAlignState(): State updated to {state}. Prev. State = {AlignState.Value}");
                }

                AlignState.Value = state;

                switch (state)
                {
                    case AlignStateEnum.IDLE:
                        AlignState.DoneState = ElementStateEnum.DEFAULT; break;
                    case AlignStateEnum.DONE:
                        {
                            AlignState.DoneState = ElementStateEnum.DONE;
                            DoMarkAlign = true;
                            break;
                        }
                }

                //if (this.LoaderRemoteMediator().GetServiceCallBack() != null)
                //{
                //    //this.StageSupervisor().ServiceCallBack?.UpdateWaferAlignState(this.LoaderController().GetChuckIndex(), AlignTypeEnum.Mark, AlignState);
                //}
                //else
                //{
                //    //LoggerManager.Debug($"Can not update mark align state");
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public bool GetDoMarkAlign()
        {
            return DoMarkAlign;
        }

        public void SetElementMetaData()
        {

        }
    }
}
