using LoaderParameters;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using StageStateEnum = LoaderBase.Communication.StageStateEnum;

namespace LoaderMapView
{
    public class LoaderMapViewModel: INotifyPropertyChanged
    {
        private static LoaderMapViewModel instance = null;


        private string _StartTime;
        public string StartTime
        {
            get { return _StartTime; }
            set
            {
                if (value != _StartTime)
                {
                    _StartTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private string _EndTime;
        public string EndTime 
        {
            get { return _EndTime; }
            set
            {
                if (value != _EndTime)
                {
                    _EndTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private long _ElapsedTime = new long();
        public long ElapsedTime
        {
            get { return _ElapsedTime; }
            set
            {
                if (value != _ElapsedTime)
                {
                    _ElapsedTime = value;
                    RaisePropertyChanged();
                }
            }
        }
        private LoaderMapViewModel()
        {
            for (int i = 0; i < stagecnt; i++)
            {
                Cells.Add(new StageObject(i));
            }
            for (int i = 0; i < paCount; i++)
            {
                PAs.Add(new PAObject(i));
            }
            for (int i = 0; i < armcnt; i++)
            {
                Arms.Add(new ArmObject(i));
            }

            for (int i = 0; i < buffercnt; i++)
            {
                Buffers.Add(new BufferObject(i));
            }
            //for (int i = 0; i < foupCount; i++)
            //{
            //    Foups.Add(new FoupObject(i));
            //    Foups[i].WaferCount = 25;
            //}
        }

        public static LoaderMapViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LoaderMapViewModel();

                }
                return instance;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<StageObject> _Cells = new ObservableCollection<StageObject>();
        public ObservableCollection<StageObject> Cells
        {
            get { return _Cells; }
            set
            {
                if (value != _Cells)
                {
                    _Cells = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<PAObject> _PAs = new ObservableCollection<PAObject>();
        public ObservableCollection<PAObject> PAs
        {
            get { return _PAs; }
            set
            {
                if (value != _PAs)
                {
                    _PAs = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<FixedTrayInfoObject> _FTs = new ObservableCollection<FixedTrayInfoObject>();
        public ObservableCollection<FixedTrayInfoObject> FTs
        {
            get { return _FTs; }
            set
            {
                if (value != _FTs)
                {
                    _FTs = value;
                    RaisePropertyChanged();
                }
            }
        }
        private ObservableCollection<ArmObject> _Arms = new ObservableCollection<ArmObject>();
        public ObservableCollection<ArmObject> Arms
        {
            get { return _Arms; }
            set
            {
                if (value != _Arms)
                {
                    _Arms = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ObservableCollection<BufferObject> _Buffers = new ObservableCollection<BufferObject>();
        public ObservableCollection<BufferObject> Buffers
        {
            get { return _Buffers; }
            set
            {
                if (value != _Buffers)
                {
                    _Buffers = value;
                    RaisePropertyChanged();
                }
            }
        }
        //private ObservableCollection<FoupObject> _Foups = new ObservableCollection<FoupObject>();
        //public ObservableCollection<FoupObject> Foups
        //{
        //    get { return _Foups; }
        //    set
        //    {
        //        if (value != _Foups)
        //        {
        //            _Foups = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}
        private LoaderMap _LoaderMap = new LoaderMap();
        public LoaderMap LoaderMap
        {
            get { return _LoaderMap; }
            set
            {
                if (value != _LoaderMap)
                {
                    _LoaderMap = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void LoaderMapConvert(LoaderMap map)
        {
            Cells.Clear();
            //Foups.Clear();
            PAs.Clear();
            Buffers.Clear();
            Arms.Clear();
            FTs.Clear();
            for (int i = 0; i < map.ChuckModules.Count(); i++)
            {
                var cell = new StageObject(i);
                //cell.Name = map.ChuckModules[i].ID.ToString();
                if (map.ChuckModules[i].Substrate != null)
                {
                    cell.State = StageStateEnum.Requested;
                    cell.TargetName = map.ChuckModules[i].Substrate.PrevPos.ToString();
                    cell.WaferStatus = map.ChuckModules[i].WaferStatus;
                    cell.Progress = map.ChuckModules[i].Substrate.OriginHolder.Index;
                }
                else
                {
                    cell.State = StageStateEnum.Not_Request;
                    cell.WaferStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
                }
                Cells.Add(cell);
            }

            for (int i = 0; i < map.PreAlignModules.Count(); i++)
            {
                var pa = new PAObject(i);
                if(map.PreAlignModules[i].Substrate!=null)
                {
                    pa.WaferStatus = map.PreAlignModules[i].WaferStatus;
                }
                else
                {
                    pa.WaferStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
                }

                PAs.Add(pa);

            }

            //FTs
            for (int i = 0; i < map.FixedTrayModules.Count(); i++)
            {
                var ft = new FixedTrayInfoObject(i);
                if (map.FixedTrayModules[i].Substrate != null)
                {
                    ft.WaferStatus = map.FixedTrayModules[i].WaferStatus;
                }
                else
                {
                    ft.WaferStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
                }

                FTs.Add(ft);

            }


            for (int i = 0; i < map.ARMModules.Count(); i++)
            {
                var arm = new ArmObject(i);
                if (map.ARMModules[i].Substrate != null)
                {
                    arm.WaferStatus = map.ARMModules[i].WaferStatus;
                }
                else
                {
                    arm.WaferStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
                }

                Arms.Add(arm);

            }

            for (int i = 0; i < map.BufferModules.Count(); i++)
            {
                var buffer = new BufferObject(i);
                if (map.BufferModules[i].Substrate != null)
                {
                    buffer.WaferStatus = map.BufferModules[i].WaferStatus;
                }
                else
                {
                    buffer.WaferStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
                }

                Buffers.Add(buffer);

            }
            //for (int i = 0; i < map.CassetteModules.Count(); i++)
            //{
            //    var foup = new FoupObject(i);
            //    foup.Slots.Clear();
            //    for (int j=0;  j< map.CassetteModules[i].SlotModules.Count();j++)
            //    {
            //        var slot = new SlotObject(map.CassetteModules[i].SlotModules.Count()-j);
            //        if (map.CassetteModules[i].SlotModules[j].Substrate != null)
            //        {
            //            slot.WaferStatus = map.CassetteModules[i].SlotModules[j].WaferStatus;
            //            slot.WaferState= map.CassetteModules[i].SlotModules[j].Substrate.WaferState;
            //        }
            //        else
            //        {
            //            slot.WaferStatus = ProberInterfaces.EnumSubsStatus.NOT_EXIST;
            //        }
            //        foup.Slots.Add(slot);
            //    }
            //    Foups.Add(foup);
            //}
        }
        /*
        DateTime StartTimeDate;
        public void SetStartTime(DateTime startTime)
        {
            StartTimeDate = startTime;
            StartTime = startTime.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
        }
        public void SetEndTime (DateTime endTime)
        {
            var elaspsedTime = endTime - StartTimeDate;
            EndTime = endTime.ToString(@"yyyy/MM/dd hh:mm:ss tt", new CultureInfo("en-US"));
            ElapsedTime =(long) elaspsedTime.TotalMilliseconds;
        }
        public void SetElapsedTime(TimeSpan time)
        {
            try
            {
                EndTime = time.ToString(@"hh\:mm\:ss\.fff");
                //EndTime = string.Format("{0:mm\\:ss.fff} days", time);
            }
            catch (Exception err)
            {

            }
        }
        */
        int stagecnt = 12;
        int armcnt = 2;
        int buffercnt = 5;
        int paCount = 3;
        //int foupCount = 3;
    }

}
