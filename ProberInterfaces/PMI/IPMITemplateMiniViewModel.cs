using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ProberInterfaces.PMI
{
    public interface IHasPMITemplateMiniViewModel
    {
        PMITemplateMiniViewModel PMITemplateMiniViewModel { get; set; }
        void UpdatePMITemplateMiniViewModel();
    }

    public interface IPMITemplateMiniViewModel
    {
        int SelectedPadTemplateIndex { get; set; }
        PadTemplate SelectedPadTemplate { get; set; }
        PMIDrawingGroup DrawingGroup { get; set; }
        int PadTemplateInfoCount { get; set; }

        void UpdatePMITemplateMiniViewModel();
    }

    [DataContract, Serializable]
    public class NormalPMIMapTemplateInfo
    {
        private int _SelectedNormalPMIMapTemplateIndex;
        [DataMember]
        public int SelectedNormalPMIMapTemplateIndex
        {
            get { return _SelectedNormalPMIMapTemplateIndex; }
            set
            {
                if (value != _SelectedNormalPMIMapTemplateIndex)
                {
                    _SelectedNormalPMIMapTemplateIndex = value;
                }
            }
        }

        private DieMapTemplate _SelectedNormalPMIMapTemplate;
        [DataMember]
        public DieMapTemplate SelectedNormalPMIMapTemplate
        {
            get { return _SelectedNormalPMIMapTemplate; }
            set
            {
                if (value != _SelectedNormalPMIMapTemplate)
                {
                    _SelectedNormalPMIMapTemplate = value;
                }
            }
        }
    }

    [DataContract, Serializable]
    public class PMITemplateMiniViewModel : INotifyPropertyChanged, IPMITemplateMiniViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdatePMITemplateMiniViewModel()
        {
            return;
        }
        #endregion

        private int _SelectedPadTemplateIndex;
        [DataMember]
        public int SelectedPadTemplateIndex
        {
            get { return _SelectedPadTemplateIndex; }
            set
            {
                if (value != _SelectedPadTemplateIndex)
                {
                    _SelectedPadTemplateIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _PadTemplateInfoCount;
        [DataMember]
        public int PadTemplateInfoCount
        {
            get { return _PadTemplateInfoCount; }
            set
            {
                if (value != _PadTemplateInfoCount)
                {
                    _PadTemplateInfoCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PadTemplate _SelectedPadTemplate;
        [DataMember]
        public PadTemplate SelectedPadTemplate
        {
            get { return _SelectedPadTemplate; }
            set
            {
                if (value != _SelectedPadTemplate)
                {
                    _SelectedPadTemplate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PMIDrawingGroup _DrawingGroup;
        [DataMember]
        public PMIDrawingGroup DrawingGroup
        {
            get { return _DrawingGroup; }
            set
            {
                if (value != _DrawingGroup)
                {
                    _DrawingGroup = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
