using System;
using System.Collections.Generic;
using System.Linq;

namespace ProberDevelopPackWindow.Tab
{
    using LogModule;
    using ProberInterfaces;
    using ProberInterfaces.GEM;
    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class GemVidsEditorViewModel : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private ObservableCollection<VIDInfo> _ECIDs
            = new ObservableCollection<VIDInfo>();
        public ObservableCollection<VIDInfo> ECIDs
        {
            get { return _ECIDs; }
            set
            {
                if (value != _ECIDs)
                {
                    _ECIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<VIDInfo> _DVIDs
            = new ObservableCollection<VIDInfo>();
        public ObservableCollection<VIDInfo> DVIDs
        {
            get { return _DVIDs; }
            set
            {
                if (value != _DVIDs)
                {
                    _DVIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<VIDInfo> _SVIDs
            = new ObservableCollection<VIDInfo>();
        public ObservableCollection<VIDInfo> SVIDs
        {
            get { return _SVIDs; }
            set
            {
                if (value != _SVIDs)
                {
                    _SVIDs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private VIDInfo _SelectedSvid;
        public VIDInfo SelectedSvid
        {
            get { return _SelectedSvid; }
            set
            {
                if (value != _SelectedSvid)
                {
                    _SelectedSvid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private VIDInfo _SelectedDvid;
        public VIDInfo SelectedDvid
        {
            get { return _SelectedDvid; }
            set
            {
                if (value != _SelectedDvid)
                {
                    _SelectedDvid = value;
                    RaisePropertyChanged();
                }
            }
        }

        private VIDInfo _SelectedEcid;
        public VIDInfo SelectedEcid
        {
            get { return _SelectedEcid; }
            set
            {
                if (value != _SelectedEcid)
                {
                    _SelectedEcid = value;
                    RaisePropertyChanged();
                }
            }
        }



        public GemVidsEditorViewModel()
        {
            InitViewModel();
        }

        public void InitViewModel()
        {
            try
            {
                //Add, Delete, Save, Clear

                var dicECID = this.GEMModule().DicECID;
                var dicDVID = this.GEMModule().DicDVID;
                var dicSVID = this.GEMModule().DicSVID;
                ECIDs.Clear();
                DVIDs.Clear();
                SVIDs.Clear();
                SelectedSvid = null;
                SelectedEcid = null;
                SelectedDvid = null;
                foreach (var ecid in dicECID.DicProberGemID.Value)
                {
                    ECIDs.Add(new VIDInfo(ecid.Key, ecid.Value.VID));
                }

                foreach (var dvid in dicDVID.DicProberGemID.Value)
                {
                    DVIDs.Add(new VIDInfo(dvid.Key, dvid.Value.VID));
                }

                foreach (var svid in dicSVID.DicProberGemID.Value)
                {
                    SVIDs.Add(new VIDInfo(svid.Key, svid.Value.VID));
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        #region Command

        private RelayCommand<object> _AddVidCommand;
        public ICommand AddVidCommand
        {
            get
            {
                if (null == _AddVidCommand) _AddVidCommand = new RelayCommand<object>(AddVidCommandFunc);
                return _AddVidCommand;
            }
        }
        private void AddVidCommandFunc(object obj)
        {
            try
            {
                if(obj is EnumVidType)
                {
                    EnumVidType type = (EnumVidType)obj;
                    switch (type)
                    {
                        case EnumVidType.NONE:
                            break;
                        case EnumVidType.SVID:
                            if(SVIDs.SingleOrDefault(property => property.PropertyPath == "") == null)
                            {
                                SVIDs.Add(new VIDInfo("", -1));
                            }
                            break;
                        case EnumVidType.DVID:
                            if (DVIDs.SingleOrDefault(property => property.PropertyPath == "") == null)
                            {
                                DVIDs.Add(new VIDInfo("", - 1));
                            }
                            break;
                        case EnumVidType.ECID:
                            if (ECIDs.SingleOrDefault(property => property.PropertyPath == "") == null)
                            {
                                ECIDs.Add(new VIDInfo("", -1));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _DeleteVidCommand;
        public ICommand DeleteVidCommand
        {
            get
            {
                if (null == _DeleteVidCommand) _DeleteVidCommand = new RelayCommand<object>(DeleteVidCommandFunc);
                return _DeleteVidCommand;
            }
        }
        private void DeleteVidCommandFunc(object obj)
        {
            try
            {
                if (obj is EnumVidType)
                {
                    EnumVidType type = (EnumVidType)obj;
                    switch (type)
                    {
                        case EnumVidType.NONE:
                            break;
                        case EnumVidType.SVID:
                            if(SelectedSvid != null)
                            {
                                SVIDs.Remove(SelectedSvid);
                            }
                            break;
                        case EnumVidType.DVID:
                            if (SelectedDvid != null)
                            {
                                DVIDs.Remove(SelectedDvid);
                            }
                            break;
                        case EnumVidType.ECID:
                            if (SelectedEcid != null)
                            {
                                ECIDs.Remove(SelectedEcid);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<object> _ClearVidCommand;
        public ICommand ClearVidCommand
        {
            get
            {
                if (null == _ClearVidCommand) _ClearVidCommand = new RelayCommand<object>(ClearVidCommandFunc);
                return _ClearVidCommand;
            }
        }
        private void ClearVidCommandFunc(object obj)
        {
            try
            {
                if (obj is EnumVidType)
                {
                    EnumVidType type = (EnumVidType)obj;
                    switch (type)
                    {
                        case EnumVidType.NONE:
                            break;
                        case EnumVidType.SVID:
                            SVIDs.Clear();
                            break;
                        case EnumVidType.DVID:
                            DVIDs.Clear();
                            break;
                        case EnumVidType.ECID:
                            ECIDs.Clear();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region < Save Parameter Command >

        private RelayCommand _SaveParameterCommand;
        public ICommand SaveParameterCommand
        {
            get
            {
                if (null == _SaveParameterCommand) _SaveParameterCommand = new RelayCommand(SaveParameterCommandFunc);
                return _SaveParameterCommand;
            }
        }

        public void SaveParameterCommandFunc()
        {
            try
            {
                var dicECID = this.GEMModule().DicECID;
                var dicDVID = this.GEMModule().DicDVID;
                var dicSVID = this.GEMModule().DicSVID;

                ////[ECID]
                //Add & Modify
                foreach (var ecid in ECIDs)
                {
                    if(dicECID.DicProberGemID.Value.ContainsKey(ecid.PropertyPath) == true)
                    {
                        GemVidInfo vidInfo = null;
                        dicECID.DicProberGemID.Value.TryGetValue(ecid.PropertyPath, out vidInfo);
                        if(vidInfo != null)
                        {
                            if(vidInfo.VID != ecid.VID)
                            {
                                vidInfo.VID = ecid.VID;
                            }
                        }
                    }
                    else
                    {
                        dicECID.DicProberGemID.Value.Add(ecid.PropertyPath, new GemVidInfo(ecid.VID, null));
                    }
                }

                //Delete
                if(ECIDs.Count != dicECID.DicProberGemID.Value.Count)
                {
                    if(ECIDs.Count ==0)
                    {
                        dicECID.DicProberGemID.Value.Clear();
                    }
                    else
                    {
                        List<string> deletevids = new List<string>();
                        foreach (var vid in dicECID.DicProberGemID.Value)
                        {
                            if(ECIDs.SingleOrDefault(ecid => ecid.PropertyPath == vid.Key) == null)
                            {
                                deletevids.Add(vid.Key);
                            }
                        }
                        foreach (var dvid in deletevids)
                        {
                            dicECID.DicProberGemID.Value.Remove(dvid);
                        }
                    }
                }

                ////[DVID]
                //Add & Modify
                foreach (var dvid in DVIDs)
                {
                    if (dicDVID.DicProberGemID.Value.ContainsKey(dvid.PropertyPath) == true)
                    {
                        GemVidInfo vidInfo = null;
                        dicDVID.DicProberGemID.Value.TryGetValue(dvid.PropertyPath, out vidInfo);
                        if (vidInfo != null)
                        {
                            if (vidInfo.VID != dvid.VID)
                            {
                                vidInfo.VID = dvid.VID;
                            }
                        }
                    }
                    else
                    {
                        dicDVID.DicProberGemID.Value.Add(dvid.PropertyPath, new GemVidInfo(dvid.VID, null));
                    }
                }

                //Delete
                if (DVIDs.Count != dicDVID.DicProberGemID.Value.Count)
                {
                    if (DVIDs.Count == 0)
                    {
                        dicDVID.DicProberGemID.Value.Clear();
                    }
                    else
                    {
                        List<string> deletevids = new List<string>();
                        foreach (var vid in dicDVID.DicProberGemID.Value)
                        {
                            if (DVIDs.SingleOrDefault(ecid => ecid.PropertyPath == vid.Key) == null)
                            {
                                deletevids.Add(vid.Key);
                            }
                        }
                        foreach (var dvid in deletevids)
                        {
                            dicDVID.DicProberGemID.Value.Remove(dvid);
                        }
                    }
                }

                ////[SVID]
                //Add & Modify
                foreach (var svid in SVIDs)
                {
                    if (dicSVID.DicProberGemID.Value.ContainsKey(svid.PropertyPath) == true)
                    {
                        GemVidInfo vidInfo = null;
                        dicSVID.DicProberGemID.Value.TryGetValue(svid.PropertyPath, out vidInfo);
                        if (vidInfo != null)
                        {
                            if (vidInfo.VID != svid.VID)
                            {
                                vidInfo.VID = svid.VID;
                            }
                        }
                    }
                    else
                    {
                        dicSVID.DicProberGemID.Value.Add(svid.PropertyPath, new GemVidInfo(svid.VID, null));
                    }
                }

                //Delete
                if (SVIDs.Count != dicSVID.DicProberGemID.Value.Count)
                {
                    if (SVIDs.Count == 0)
                    {
                        dicSVID.DicProberGemID.Value.Clear();
                    }
                    else
                    {
                        List<string> deletevids = new List<string>();
                        foreach (var vid in dicSVID.DicProberGemID.Value)
                        {
                            if (SVIDs.SingleOrDefault(ecid => ecid.PropertyPath == vid.Key) == null)
                            {
                                deletevids.Add(vid.Key);
                            }
                        }
                        foreach (var dvid in deletevids)
                        {
                            dicSVID.DicProberGemID.Value.Remove(dvid);
                        }
                    }
                }

                this.GEMModule().SaveSysParameter();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #region < Load Parameter Command >

        private RelayCommand _LoadParameterCommand;
        public ICommand LoadParameterCommand
        {
            get
            {
                if (null == _LoadParameterCommand) _LoadParameterCommand = new RelayCommand(LoadParameterCommandFunc);
                return _LoadParameterCommand;
            }
        }

        public void LoadParameterCommandFunc()
        {
            try
            {
                InitViewModel();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        #endregion
    }

    public class VIDInfo : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        #endregion

        private string _PropertyPath;
        public string PropertyPath
        {
            get { return _PropertyPath; }
            set
            {
                if (value != _PropertyPath)
                {
                    _PropertyPath = value;
                    RaisePropertyChanged();
                }
            }
        }


        private int _VID;
        public int VID
        {
            get { return _VID; }
            set
            {
                if (value != _VID)
                {
                    _VID = value;
                    RaisePropertyChanged();
                }
            }
        }

        public VIDInfo(string propertyPath, int vid)
        {
            PropertyPath = propertyPath;
            VID = vid;
        }

    }
}
