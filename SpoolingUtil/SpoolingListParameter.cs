using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpoolingUtil
{
    using ProberErrorCode;
    using ProberInterfaces;

    /// <summary>
    /// spooling list item parameter
    /// </summary>
    public class SpoolingListParameter : IParam
    {
        public string FilePath { get; set; } = "";

        public string FileName { get; set; } = "";

        public bool IsParamChanged { get; set; }
        public string Genealogy { get; set; }
        public object Owner { get; set; }
        public List<object> Nodes { get; set; }

        public ProberErrorCode.EventCodeEnum Init()
        {
            return EventCodeEnum.NONE;
        }

        public ProberErrorCode.EventCodeEnum SetDefaultParam()
        {
            return EventCodeEnum.NONE;
        }

        public void SetElementMetaData()
        {
            return;
        }

        public ProberErrorCode.EventCodeEnum SetEmulParam()
        {
            return EventCodeEnum.NONE;
        }

        private ObservableCollection<SpoolingListItem> _SpoolingListParam = new ObservableCollection<SpoolingListItem>();
        public ObservableCollection<SpoolingListItem> SpoolingListParam
        {
            get { return _SpoolingListParam; }
            set
            {
                if (value != _SpoolingListParam)
                {
                    _SpoolingListParam = value;                    
                }
            }
        }
    }

    /// <summary>
    /// spooling item information
    /// </summary>
    public class SpoolingListItem
    {
        public SpoolingListItem(string targetItemPath, string uploadSubPath, string key, string dateTime, int cell_idx, bool useBinary)
        {
            TargetItemPath = targetItemPath;
            UploadSubPath = uploadSubPath;
            Key = key;
            Date = dateTime;
            Cell = cell_idx;
            UseBinary = useBinary;
        }

        private string _TargetItemPath;
        public string TargetItemPath
        {
            get { return _TargetItemPath; }
            set
            {
                if (value != _TargetItemPath)                
                    _TargetItemPath = value;                                    
            }
        }

        private string _UploadSubPath;
        public string UploadSubPath
        {
            get { return _UploadSubPath; }
            set
            {
                if (value != _UploadSubPath)
                    _UploadSubPath = value;
            }
        }

        private string _Key;
        public string Key
        {
            get { return _Key; }
            set
            {
                if (value != _Key)
                    _Key = value;
            }
        }

        private string _Date;
        public string Date
        {
            get { return _Date; }
            set
            {
                if (value != _Date)
                    _Date = value;
            }
        }

        private int _Cell;
        public int Cell
        {
            get { return _Cell; }
            set
            {
                if (value != _Cell)
                    _Cell = value;
            }
        }

        private bool _UseBinary;
        public bool UseBinary
        {
            get { return _UseBinary; }
            set
            {
                if (value != _UseBinary)
                    _UseBinary = value;
            }
        }

    }

}
