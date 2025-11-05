using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProberInterfaces.Param
{
    public interface IProberCardListParameter : INotifyPropertyChanged
    {
        List<PinBaseFiducialMarkParameter> FiducialMarInfos { get; set; }

        string CardID { get; set; }
    }

    [Serializable]
    public class ProberCardListParameter : IProberCardListParameter
    {
        #region ==> PropertyChanged
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private List<PinBaseFiducialMarkParameter> _FiducialMarInfos;
        public List<PinBaseFiducialMarkParameter> FiducialMarInfos
        {
            get { return _FiducialMarInfos; }
            set
            {
                if (value != _FiducialMarInfos)
                {
                    _FiducialMarInfos = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _CardID;
        public string CardID
        {
            get { return _CardID; }
            set
            {
                if (value != _CardID)
                {
                    _CardID = value;
                    RaisePropertyChanged();
                }
            }
        }

    }
}
