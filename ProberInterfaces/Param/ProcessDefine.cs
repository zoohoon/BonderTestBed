using System;

namespace ProberInterfaces.Param
{
    using System.ComponentModel;
    using ProberInterfaces.Enum;
    using System.Collections.ObjectModel;
    using ProberInterfaces.WaferAlign;
    using ProberErrorCode;
    using ProberInterfaces.Vision;
    using Newtonsoft.Json;
    using LogModule;

    [Serializable]
    public class WaferProcParametricTable : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #region // Properties



        private CatCoordinates _Position;
        public CatCoordinates Position
        {
            get { return _Position; }
            set
            {
                if (value != _Position)
                {
                    _Position = value;
                    NotifyPropertyChanged("Position");
                }
            }
        }

        private IndexCoord _Index;
        public IndexCoord Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    NotifyPropertyChanged("Index");
                }
            }
        }

        private WaferAlignProcTypeEnum _ProcType;
        public WaferAlignProcTypeEnum ProcType
        {
            get { return _ProcType; }
            set
            {
                if (value != _ProcType)
                {
                    _ProcType = value;
                    NotifyPropertyChanged("ProcType");
                }
            }
        }
        private ObservableCollection<WaferAlignProcResource> _Resouces;
        public ObservableCollection<WaferAlignProcResource> Resouces
        {
            get { return _Resouces; }
            set
            {
                if (value != _Resouces)
                {
                    _Resouces = value;
                    NotifyPropertyChanged("Resouces");
                }
            }
        }

        public WaferProcParametricTable()
        {
            try
            {
                Position = new CatCoordinates();
                Resouces = new ObservableCollection<WaferAlignProcResource>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion
    }

    public class WaferProcResult : INotifyPropertyChanged, IComparable
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public int CompareTo(object obj)
        {
            try
            {
                WaferProcResult compareTarget;
                if (obj is WaferProcResult)
                {
                    compareTarget = (WaferProcResult)obj;

                    if (ResultPos.X.Value > compareTarget.ResultPos.X.Value)
                    {
                        return 1;
                    }
                    else if (ResultPos.X.Value < compareTarget.ResultPos.X.Value)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return 0;
        }


        #region // Properties
        private WaferCoordinate _ResultPos = new WaferCoordinate();
        public WaferCoordinate ResultPos
        {
            get { return _ResultPos; }
            set
            {
                if (value != _ResultPos)
                {
                    _ResultPos = value;
                    NotifyPropertyChanged("ResultPos");
                }
            }
        }

        private WaferCoordinate _VerifyPos;
        public WaferCoordinate VerifyPos
        {
            get { return _VerifyPos; }
            set
            {
                if (value != _VerifyPos)
                {
                    _VerifyPos = value;
                    NotifyPropertyChanged("VerifyPos");
                }
            }
        }


        private IndexCoord _Index;
        public IndexCoord Index
        {
            get { return _Index; }
            set
            {
                if (value != _Index)
                {
                    _Index = value;
                    NotifyPropertyChanged("Index");
                }
            }
        }


        private HeightMappingStateBase _HeightMapping;
        public HeightMappingStateBase HeightMapping
        {
            get { return _HeightMapping; }
            set
            {
                if (value != _HeightMapping)
                {
                    _HeightMapping = value;
                    NotifyPropertyChanged("HeightMapping");
                }
            }
        }

        private PatternInfomation _PatternInfo;
        public PatternInfomation PatternInfo
        {
            get { return _PatternInfo; }
            set
            {
                if (value != _PatternInfo)
                {
                    _PatternInfo = value;
                    NotifyPropertyChanged("PatternInfo");
                }
            }
        }

        private RectSize _AlignIndexSize;
        public RectSize AlignIndexSize
        {
            get { return _AlignIndexSize; }
            set
            {
                if (value != _AlignIndexSize)
                {
                    _AlignIndexSize = value;
                    NotifyPropertyChanged("AlignIndexSize");
                }
            }
        }


        private EventCodeEnum _ErrorCodeType;
        public EventCodeEnum ErrorCodeType
        {
            get { return _ErrorCodeType; }
            set
            {
                if (value != _ErrorCodeType)
                {
                    _ErrorCodeType = value;
                    NotifyPropertyChanged("ErrorCodeType");
                }
            }
        }

        private PMResult _PmResult;
        public PMResult PmResult
        {
            get { return _PmResult; }
            set
            {
                if (value != _PmResult)
                {
                    _PmResult = value;
                    NotifyPropertyChanged("PmResult");
                }
            }
        }


        public WaferProcResult()
        {

        }
        public WaferProcResult(EventCodeEnum errortype)
        {
            ErrorCodeType = errortype;
        }
        public WaferProcResult(WaferCoordinate coord)
        {
            ResultPos = coord;
        }

        public WaferProcResult(WaferCoordinate coord, IndexCoord index)
        {
            try
            {
                ResultPos = coord;
                Index = index;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public WaferProcResult(WaferCoordinate coord, IndexCoord index, PatternInfomation ptinfo)
        {
            try
            {
                ResultPos = coord;
                Index = index;
                PatternInfo = ptinfo;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferProcResult(WaferCoordinate coord, IndexCoord index, PatternInfomation ptinfo, RectSize indexsize)
        {
            try
            {
                ResultPos = coord;
                Index = index;
                PatternInfo = ptinfo;
                AlignIndexSize = indexsize;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferProcResult(WaferCoordinate coord, IndexCoord index, PatternInfomation ptinfo, RectSize indexsize, PMResult result)
        {
            try
            {
                ResultPos = coord;
                Index = index;
                PatternInfo = ptinfo;
                AlignIndexSize = indexsize;
                PmResult = result;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferProcResult(WaferCoordinate coord, MachineIndex index)
        {
            try
            {
                ResultPos = coord;
                Index = index;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferProcResult(WaferCoordinate coord, EventCodeEnum errortype)
        {
            try
            {
                ResultPos = coord;
                ErrorCodeType = errortype;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferProcResult(WaferCoordinate coord, IndexCoord index, EventCodeEnum errortype)
        {
            try
            {
                ResultPos = coord;
                Index = index;
                ErrorCodeType = errortype;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferProcResult(WaferCoordinate coord, long xindex, long yindex, EventCodeEnum errortype)
        {
            try
            {
                ResultPos = coord;
                Index = new MachineIndex(xindex, yindex);
                ErrorCodeType = errortype;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferProcResult(WaferCoordinate coord, IndexCoord index, HeightMappingStateBase heightmapping)
        {
            try
            {
                ResultPos = coord;
                Index = index;
                HeightMapping = heightmapping;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        #endregion


        public void CopyTo(WaferProcResult target)
        {
            try
            {
                ResultPos.CopyTo(target.ResultPos);
                VerifyPos.CopyTo(target.VerifyPos);
                Index.CopyTo(target.Index);
                target.HeightMapping = this.HeightMapping;
                PatternInfo.CopyTo(target.PatternInfo);
                target.AlignIndexSize.Width = this.AlignIndexSize.Width;
                target.AlignIndexSize.Height = this.AlignIndexSize.Height;
                target.ErrorCodeType = this.ErrorCodeType;
                target.PmResult.CopyTo(target.PmResult);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
    public class WaferAlignProcResource : INotifyPropertyChanged
    {
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #region // Properties

        private EnumProberCam _Cam;
        public EnumProberCam Cam
        {
            get { return _Cam; }
            set
            {
                if (value != _Cam)
                {
                    _Cam = value;
                    NotifyPropertyChanged("Cam");
                }
            }
        }

        private ObservableCollection<LightValueParam> _LightsValue = new ObservableCollection<LightValueParam>();
        public ObservableCollection<LightValueParam> LightsValue
        {
            get { return _LightsValue; }
            set
            {
                if (value != _LightsValue)
                {
                    _LightsValue = value;
                    NotifyPropertyChanged("LightsValue");
                }
            }
        }


        private PatternInfomation _PatternInfo;
        public PatternInfomation PatternInfo
        {
            get { return _PatternInfo; }
            set
            {
                if (value != _PatternInfo)
                {
                    _PatternInfo = value;
                    NotifyPropertyChanged("PatternInfo");
                }
            }
        }

        private BlobParameter _BlobParam;
        public BlobParameter BlobParam
        {
            get { return _BlobParam; }
            set
            {
                if (value != _BlobParam)
                {
                    _BlobParam = value;
                    NotifyPropertyChanged("BlobParam");
                }
            }
        }
        #endregion

        public WaferAlignProcResource()
        {

        }
        public WaferAlignProcResource(EnumProberCam cam, PatternInfomation patterninfo)
        {
            try
            {
                Cam = cam;
                PatternInfo = patterninfo;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferAlignProcResource(EnumProberCam cam, PatternInfomation patterninfo, ObservableCollection<LightValueParam> lightparam)
        {
            try
            {
                Cam = cam;
                PatternInfo = patterninfo;
                LightsValue = lightparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferAlignProcResource(EnumProberCam cam, BlobParameter blobparam)
        {
            try
            {
                Cam = cam;
                BlobParam = blobparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public WaferAlignProcResource(EnumProberCam cam, PatternInfomation patterninfo, BlobParameter blobparam)
        {
            try
            {
                Cam = cam;
                PatternInfo = patterninfo;
                BlobParam = blobparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void DefaultSettingBlobParam()
        {
            try
            {
                BlobParameter mblobparam = new BlobParameter();

                mblobparam.BlobMinRadius.Value = 3;
                mblobparam.BlobThreshHold.Value = 120;
                mblobparam.MinBlobArea.Value = 50;

                BlobParam = mblobparam;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        //public void DefaultSettingPmParam()
        //{
        //    PMParameter mpmparam = new PMParameter();
        //    mpmparam.PMAcceptance.Value = 80;
        //    mpmparam.PMCertainty.Value = 95;
        //    mpmparam.PMOccurrence.Value = 1;
        //    PMParam = mpmparam;
        //}
    }


}
