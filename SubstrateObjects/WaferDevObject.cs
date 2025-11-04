
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubstrateObjects
{
    using ProberInterfaces;
    //using MapObject;
    using System.ComponentModel;
    using ProberInterfaces.Param;
    using System.Xml.Serialization;
    using Autofac;
    using ProberInterfaces.Enum;
    using System.Diagnostics;
    using System.Windows;
    using ProberErrorCode;
    using LogModule;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    [Serializable]
    public class WaferDevObject : IWaferDevObject, INotifyPropertyChanged
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

        [JsonIgnore, ParamIgnore]
        public bool IsParamChanged { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public List<object> Nodes { get; set; }

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
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
        [ParamIgnore]
        public string FilePath { get; } = "";

        [XmlIgnore, JsonIgnore]
        [ParamIgnore]
        public string FileName { get; } = "WaferMapFile.Json";

        private Element<long> _ValidDieCount = new Element<long>();
        public virtual Element<long> ValidDieCount
        {
            get
            {
                return _ValidDieCount;
            }
            set
            {
                if (_ValidDieCount != value)
                {
                    _ValidDieCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        private PhysicalInfo _PhysInfo = new PhysicalInfo();
        public PhysicalInfo PhysInfo
        {
            get { return _PhysInfo; }
            set
            {
                if (value != _PhysInfo)
                {
                    _PhysInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        private SubstrateInfo _Info = new SubstrateInfo();
        public SubstrateInfo Info
        {
            get { return _Info; }
            set
            {
                if (value != _Info)
                {
                    _Info = value;
                    RaisePropertyChanged();
                }
            }
        }
        #region Method

        public void InitMap()
        {
            long maxX, minX, maxY, minY;
            long xNum = 0, yNum = 0;

            Stopwatch stw = new Stopwatch();

            IDeviceObject[,] dies = new IDeviceObject[xNum, yNum];

            try
            {
                if (Info.Devices.Count != 0)
                {
                    maxX = Info.Devices.Max(d => d.DieIndexM.XIndex);
                    minX = Info.Devices.Min(d => d.DieIndexM.XIndex);
                    maxY = Info.Devices.Max(d => d.DieIndexM.YIndex);
                    minY = Info.Devices.Min(d => d.DieIndexM.YIndex);

                    xNum = maxX - minX + 1;
                    yNum = maxY - minY + 1;

                    dies = new IDeviceObject[xNum, yNum];

                    stw.Reset();
                    stw.Start();

                    Info.MarkDieCount.Value = 0;
                    Info.TestDieCount.Value = 0;

                    ConcurrentBag<DeviceObject> devices = new ConcurrentBag<DeviceObject>(Info.Devices);

                    Parallel.ForEach(devices, device =>
                    {
                        var exist = devices.TryTake(out var d);

                        if (exist)
                        {
                            dies[d.DieIndexM.XIndex, d.DieIndexM.YIndex] = d;

                            var dietype = dies[d.DieIndexM.XIndex, d.DieIndexM.YIndex].DieType.Value;
                            var diestate = dies[d.DieIndexM.XIndex, d.DieIndexM.YIndex].State;

                            if (dietype == DieTypeEnum.MARK_DIE ||
                                dietype == DieTypeEnum.CONFIRM_MARK_DIE)
                            {
                                diestate.Value = DieStateEnum.MARK;

                                Info.MarkDieCount.Value++;
                            }
                            else if (dietype == DieTypeEnum.SKIP_DIE)
                            {
                                diestate.Value = DieStateEnum.SKIPPED;
                            }
                            else if (dietype != DieTypeEnum.NOT_EXIST)
                            {
                                diestate.Value = DieStateEnum.NORMAL;

                                Info.TestDieCount.Value++;
                            }
                            else
                            {
                                diestate.Value = DieStateEnum.NOT_EXIST;
                            }
                        }
                    });
                }

                stw.Stop();

                LoggerManager.Debug($"[{this.GetType().Name}] InitMap() : {stw.ElapsedMilliseconds} ms");

                Info.DIEs = dies;

                Info.UpdatePadsToDevices();
                Info.AveWaferThick = PhysInfo.Thickness.Value;
                Info.PMIInfo.SetWaferInfo((int)xNum, (int)yNum);
            }
            catch (Exception err)
            {
                //LoggerManager.Error($err, "Error occurred while init dies map.");
                LoggerManager.Exception(err);

            }
        }

        public EventCodeEnum SetEmulParam()
        {
            return SetDefaultParam();
        }

        public EventCodeEnum SetDefaultParam()
        {
            EventCodeEnum RetVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (PhysInfo == null)
                {
                    PhysInfo = new PhysicalInfo();
                }

                if (PhysInfo.WaferSize_um.Value == 0)
                    PhysInfo.WaferSize_um.Value = 200000; // 12INCHVisionMapping
                if (PhysInfo.DieSizeX.Value == 0)
                    PhysInfo.DieSizeX.Value = 6000;
                if (PhysInfo.DieSizeY.Value == 0)
                    PhysInfo.DieSizeY.Value = 6000;
                if (PhysInfo.Thickness.Value == 0)
                    PhysInfo.Thickness.Value = 500;
                if (PhysInfo.ThicknessTolerance.Value == 0)
                    PhysInfo.ThicknessTolerance.Value = 200;

                //PhysInfo.WaferSize_um.Value = 300000; // 12INCH
                //PhysInfo.DieSizeX.Value = 7177;
                //PhysInfo.DieSizeY.Value = 8460;
                //PhysInfo.Thickness.Value = 405;

                    //PhysInfo.WaferSize_um.Value = 200000; // 8INCH TestDeivce
                    //PhysInfo.DieSizeX.Value = 5420.7;
                    //PhysInfo.DieSizeY.Value = 5370.9;
                    //PhysInfo.Thickness.Value = 586;

                    //PhysInfo.WaferSize_um.Value = 200000; // 8INCH BSCI
                    //PhysInfo.DieSizeX.Value = 5102;
                    //PhysInfo.DieSizeY.Value = 8500;
                    //PhysInfo.Thickness.Value = 790;


                    //PhysInfo.WaferSize_um.Value = 200000; // 8INCH MultiDevice
                    //PhysInfo.DieSizeX.Value = 20330.4;
                    //PhysInfo.DieSizeY.Value = 21139.3;
                    //PhysInfo.Thickness.Value = 586;

                    //PhysInfo.WaferSize_um.Value = 200000; // 8INCH MultiDevice
                    //PhysInfo.DieSizeX.Value = 10467.6;
                    //PhysInfo.DieSizeY.Value = 21901.1;
                    //PhysInfo.Thickness.Value = 586;

                if (PhysInfo.NotchType.Value == null | PhysInfo.NotchType.Value == WaferNotchTypeEnum.UNKNOWN.ToString())
                    PhysInfo.NotchType.Value = WaferNotchTypeEnum.NOTCH.ToString();

                if (PhysInfo.WaferSize_um.Value == 0)
                    PhysInfo.WaferMargin_um.Value = 5000;
                PhysInfo.WaferSize_Offset_um.Value = 0;
                PhysInfo.FramedNotchAngle.Value = 0;

                PhysInfo.LowLeftCorner = new WaferCoordinate();
                Info.RefDieLeftCorner = new WaferCoordinate();

                //PhysInfo.CassetteID.Value = "";
                //PhysInfo.WaferID.Value = "";

                Info.WaferID.Value = string.Empty;
                Info.CassetteID = "";

                Info.ActualDieSize.Width.Value = PhysInfo.DieSizeX.Value;
                Info.ActualDieSize.Height.Value = PhysInfo.DieSizeY.Value;
                Info.ActualDeviceSize.Width.Value = PhysInfo.DieSizeX.Value;
                Info.ActualDeviceSize.Height.Value = PhysInfo.DieSizeY.Value;

                PhysInfo.MapDirX.Value = MapHorDirectionEnum.RIGHT;
                PhysInfo.MapDirY.Value = MapVertDirectionEnum.UP;
                PhysInfo.PadType.Value = EnumPadType.NORMAL;

                SubstrateInfoInit();

                Info.Pads.Flag = 0;

                Info.Pads.SetPadObject.PadSizeX.Value = 80;
                Info.Pads.SetPadObject.PadSizeY.Value = 80;
                Info.Pads.SetPadObject.PadShape.Value = EnumPadShapeType.SQUARE;
                Info.Pads.SetPadObject.PadColor.Value = EnumPadColorType.WHITE;


                Info.Pads.DutPadInfos.Clear();
                Info.Pads.PMIPadInfos.Clear();

                CalcWaferCenterIndex();

                PhysInfo.CenU.XIndex.Value = PhysInfo.CenM.XIndex.Value;
                PhysInfo.CenU.YIndex.Value = PhysInfo.CenM.YIndex.Value;

                PhysInfo.OrgM.XIndex.Value = 0;
                PhysInfo.OrgM.YIndex.Value = 0;

                PhysInfo.OrgU.XIndex.Value = 0;
                PhysInfo.OrgU.YIndex.Value = 0;
                UpdateWaferObject(AutoCalWaferMap(true));

                RetVal = EventCodeEnum.NONE;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return RetVal;
        }

        public EventCodeEnum UpdateWaferObject(byte[,] map, bool isSave = true)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                bool hasChild = false;

                try
                {
                    Info.Devices = new List<DeviceObject>();

                    DeviceObject dev;
                    Info.MarkDieCount.Value = 0;
                    Info.TestDieCount.Value = 0;

                    for (int i = 0; i < map.GetUpperBound(0) + 1; i++)
                    {
                        for (int j = 0; j < map.GetUpperBound(1) + 1; j++)
                        {
                            dev = new DeviceObject();

                            dev.DieIndexM.XIndex = i;
                            dev.DieIndexM.YIndex = j;
                            dev.DieIndex.XIndex = i;
                            dev.DieIndex.YIndex = j;

                            dev.DieType.Value = (DieTypeEnum)map[i, j];
                            dev.WaferSubstrateType.Value = PhysInfo.WaferSubstrateType.Value;
                            if (dev.DieType.Value == DieTypeEnum.MARK_DIE
                                || dev.DieType.Value == DieTypeEnum.CONFIRM_MARK_DIE)
                            {
                                dev.State.Value = DieStateEnum.MARK;
                                Info.MarkDieCount.Value++;
                            }
                            //else if (dev.DieType.Value == DieTypeEnum.CHANGEMARK_DIE)
                            //{
                            //    dev.State.Value = DieStateEnum.CHANGEDMARK;
                            //    MarkDieCount.Value++;
                            //}
                            else if (dev.DieType.Value == DieTypeEnum.SKIP_DIE)
                            {
                                dev.State.Value = DieStateEnum.SKIPPED;
                            }
                            //else if (dev.DieType.Value == DieTypeEnum.MODIFY_DIE)
                            //{
                            //    dev.State.Value = DieStateEnum.MODIFY_DIE;
                            //}
                            else if (dev.DieType.Value != DieTypeEnum.NOT_EXIST)
                            {
                                dev.State.Value = DieStateEnum.NORMAL;
                                Info.TestDieCount.Value++;
                            }
                            else
                            {
                                dev.State.Value = DieStateEnum.NOT_EXIST;
                            }

                            ValidDieCount.Value++;

                            #region // Add sub device
                            hasChild = false;
                            dev.SubDevice = new DeviceGroup();
                            //dev.Groups.Add(new DeviceGroup());
                            //dev.Groups.Add(new DeviceGroup());
                            //dev.Groups.Add(new DeviceGroup());

                            if (dev.State.Value != DieStateEnum.NOT_EXIST & hasChild == true)
                            {
                                SubDieObject child;
                                child = new SubDieObject();

                                double childXSize, childYSize;

                                childXSize = PhysInfo.DieSizeX.Value / 3;
                                childYSize = PhysInfo.DieSizeY.Value / 3;

                                for (int yindex = 0; yindex < 3; yindex++)
                                {
                                    for (int xindex = 0; xindex < 3; xindex++)
                                    {
                                        child = new SubDieObject();
                                        child.Position.X.Value = childXSize * xindex;
                                        child.Position.Y.Value = childYSize * yindex;
                                        child.Position.Z.Value = 0.0;
                                        if (xindex == 2)
                                        {
                                            child.Position.Y.Value = PhysInfo.DieSizeY.Value / 2 * yindex;
                                            child.Size.Width.Value = childXSize / 2;
                                            child.Size.Height.Value = PhysInfo.DieSizeY.Value / 2;
                                        }
                                        else
                                        {
                                            child.Position.Y.Value = childYSize * yindex;
                                            child.Size.Width.Value = childXSize;
                                            child.Size.Height.Value = childYSize;
                                        }

                                        child.DieIndexM.XIndex = xindex;
                                        child.DieIndexM.YIndex = yindex;
                                        child.DieIndex.XIndex = xindex;
                                        child.DieIndex.YIndex = yindex;

                                        child.State = dev.State;
                                        child.DieType = dev.DieType;
                                        child.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                                        child.WaferSubstrateType.Value = dev.WaferSubstrateType.Value;

                                        if (xindex == 2)
                                        {
                                            if (yindex < 2)
                                            {
                                                dev.SubDevice.Children.Add(child);
                                            }
                                        }
                                        else
                                        {
                                            if (yindex < 2) dev.SubDevice.Children.Add(child);
                                        }
                                    }
                                }

                                child = new SubDieObject();
                                child.Position.X.Value = childXSize * 0.3;
                                child.Position.Y.Value = childYSize * 2.3;
                                child.Position.T.Value = 45;
                                //child.Position.T.Value = 0;
                                child.Size.Width.Value = childXSize / 2;
                                child.Size.Height.Value = childYSize / 2;
                                child.DieIndexM.XIndex = 0;
                                child.DieIndexM.YIndex = 2;
                                child.DieIndex.XIndex = 0;
                                child.DieIndex.YIndex = 2;
                                child.State = dev.State;
                                child.DieType = dev.DieType;
                                child.SubstrateType.Value = SubstrateTypeEnum.Wafer;
                                child.WaferSubstrateType.Value = WaferSubstrateTypeEnum.Normal;

                                dev.SubDevice.Children.Add(child);
                            }
                            //dev.SubDevice = dev.Groups[0];
                            #endregion

                            Info.Devices.Add(dev);
                        }
                    }

                    PhysInfo.MapCountX.Value = map.GetUpperBound(0) + 1;
                    PhysInfo.MapCountY.Value = map.GetUpperBound(1) + 1;


                    
                    //if (isSave != false)
                    //{
                    //    //SaveCurrentWaferMap((WaferObject)Wafer);
                    //    retVal = Extensions_IParam.SaveParameter(Wafer);
                    //}

                    //Wafer.InitMap();

                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {

                    throw err;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public byte[,] AutoCalWaferMap(bool mark_die_on)
        {
            double rectstartx;
            double rectstarty;
            double rectcenterx;
            double rectcentery;
            double wafercenterx;
            double wafercentery;
            double waferradius;
            double testradius;

            double flatdepth = 0.0;

            int i, j;
            double dtmp0;
            double dtmp1;
            double dtmp2;
            double dtmp3;

            double wSize = PhysInfo.WaferSize_um.Value;

            EnumWaferSize wafer_size = PhysInfo.WaferSizeEnum;
            switch (wSize)
            {
                case 150000:
                    wafer_size = EnumWaferSize.INCH6;
                    break;
                case 200000:
                    wafer_size = EnumWaferSize.INCH8;
                    break;
                case 300000:
                    wafer_size = EnumWaferSize.INCH12;
                    break;
            }
            byte[,] bmap;
            try
            {

                WaferNotchTypeEnum flat_notch = (WaferNotchTypeEnum)Enum.Parse(typeof(WaferNotchTypeEnum), PhysInfo.NotchType.Value.ToString(), true);
                double flat_notch_dir = 0;
                double die_num_x = 0.0;
                double die_num_y = 0.0;

                //if (PhysInfo.MapCountY.Value != 0)
                //    die_num_y = PhysInfo.MapCountY.Value;
                //else
                //    die_num_y = Convert.ToInt32(Math.Ceiling((double)(wSize / (Info.ActualDieSize.Width.Value))));

                //if (PhysInfo.MapCountX.Value != 0)
                //    die_num_x = PhysInfo.MapCountX.Value;
                //else
                //    die_num_x = Convert.ToInt32(Math.Ceiling((double)(wSize / (Info.ActualDieSize.Height.Value))));
                CalcWaferCenterIndex();

                die_num_x = PhysInfo.MapCountX.Value;
                die_num_y = PhysInfo.MapCountY.Value;

                //double die_size_x = Info.ActualDeviceSize.Width.Value;
                //double die_size_y = Info.ActualDeviceSize.Height.Value;
                double die_size_x = Info.ActualDieSize.Width.Value;
                double die_size_y = Info.ActualDieSize.Height.Value;

                //CalcRefDieOffset();
                CalcMapOffset();

                double offset_x = PhysInfo.CenDieOffset.GetX();
                double offset_y = PhysInfo.CenDieOffset.GetY();

                //offset_x = 0;
                //offset_y = 0;

                double wafer_margin = PhysInfo.WaferMargin_um.Value;         //EdgeOffset

                bmap = new byte[(int)die_num_x, (int)die_num_y];

                waferradius = wSize / 2d;
                testradius = waferradius - wafer_margin;

                wafercenterx = wSize / 2d;
                wafercentery = wSize / 2d;

                rectcenterx = wafercenterx + offset_x;
                rectstartx = rectcenterx - (die_size_x * die_num_x) / 2d;

                rectcentery = wafercentery + offset_y;
                rectstarty = rectcentery - (die_size_y * die_num_y) / 2d;

                if (flat_notch == WaferNotchTypeEnum.FLAT)     //Flat(4" ,5" , 6" ,8")
                {
                    if (wSize == 100000)        //4"
                    {
                        flatdepth = 2714d;
                    }
                    else if (wSize == 125000)       //5"
                    {
                        flatdepth = 3723d;
                    }
                    else if (wSize == 150000)       //6"
                    {
                        flatdepth = 5729d;
                    }
                    else if (wSize == 200000)       //8"
                    {
                        flatdepth = 4500d;
                    }
                    else if (wSize == 300000)       //12"
                    {

                    }

                    if (wSize == 100000 || wSize == 125000 || wSize == 150000 || wSize == 200000 || wSize == 300000)
                    {
                        if (flat_notch_dir == 0)
                        {
                            wafercenterx = wafercenterx + (flatdepth / 2d);
                        }
                        else if (flat_notch_dir == 90)
                        {
                            wafercentery = wafercentery + (flatdepth / 2d);
                        }
                        else if (flat_notch_dir == 180)
                        {
                            wafercenterx = wafercenterx - (flatdepth / 2d);
                        }
                        else
                        {
                            wafercentery = wafercentery - (flatdepth / 2d);
                        }
                    }
                }
                else //Notch(6", 8", 12")
                {
                    flatdepth = 0d;
                }

                for (i = 0; i < die_num_x; i++)
                {
                    for (j = 0; j < die_num_y; j++)
                    {
                        dtmp0 = Distance2D(rectstartx + i * die_size_x, rectstarty + j * die_size_y, wafercenterx, wafercentery);
                        dtmp1 = Distance2D(rectstartx + (i + 1) * die_size_x, rectstarty + j * die_size_y, wafercenterx, wafercentery);
                        dtmp2 = Distance2D(rectstartx + i * die_size_x, rectstarty + (j + 1) * die_size_y, wafercenterx, wafercentery);
                        dtmp3 = Distance2D(rectstartx + (i + 1) * die_size_x, rectstarty + (j + 1) * die_size_y, wafercenterx, wafercentery);

                        if (dtmp0 <= testradius && dtmp1 <= testradius && dtmp2 <= testradius && dtmp3 <= testradius)
                        {
                            if (flat_notch == WaferNotchTypeEnum.NOTCH)
                            {
                                if (wafer_size == EnumWaferSize.INCH6 || wafer_size == EnumWaferSize.INCH8)    //gCMI_Get_mWaferSizeum(0) : 6" ,gCMI_Get_mWaferSizeum(1) = 8"
                                {
                                    if (flat_notch_dir == 0)
                                    {
                                        if ((rectstartx + ((i + 1) * die_size_x)) <= (wafercenterx + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstartx + (i * die_size_x)) <= (wafercenterx + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 90)
                                    {
                                        if ((rectstarty + ((j + 1) * die_size_y)) <= (wafercentery + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstarty + (j * die_size_y)) <= (wafercentery + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 180)
                                    {
                                        if ((rectstartx + (i * die_size_x)) >= (wafercenterx - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstartx + ((i + 1) * die_size_x)) >= (wafercenterx - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 270)
                                    {
                                        if ((rectstarty + (j * die_size_y)) >= (wafercentery - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstarty + ((j + 1) * die_size_y)) >= (wafercentery - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    bmap[i, j] = 1;
                                }
                            }
                            else
                            {
                                bmap[i, j] = 1;
                            }
                        }
                        else if ((wafer_margin < 0d) && (dtmp0 <= testradius || dtmp1 <= testradius || dtmp2 <= testradius || dtmp3 <= testradius))
                        {
                            if (flat_notch == WaferNotchTypeEnum.NOTCH)
                            {
                                if (wafer_size == EnumWaferSize.INCH6 || wafer_size == EnumWaferSize.INCH8)
                                {
                                    if (flat_notch_dir == 0)
                                    {
                                        if ((rectstartx + ((i + 1) * die_size_x)) <= (wafercenterx + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstartx + (i * die_size_x)) <= (wafercenterx + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 90)
                                    {
                                        if ((rectstarty + ((j + 1) * die_size_y)) <= (wafercentery + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstarty + (j * die_size_y)) <= (wafercentery + (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 180)
                                    {
                                        if ((rectstartx + (i * die_size_x)) >= (wafercenterx - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstartx + ((i + 1) * die_size_x)) >= (wafercenterx - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 270)
                                    {
                                        if ((rectstarty + (j * die_size_y)) >= (wafercentery - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            bmap[i, j] = 1;
                                        }
                                        else if ((rectstarty + ((j + 1) * die_size_y)) >= (wafercentery - (waferradius - wafer_margin - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    bmap[i, j] = 1;
                                }
                            }
                            else
                            {
                                bmap[i, j] = 1;
                            }
                        }
                        else if (dtmp0 <= waferradius || dtmp1 <= waferradius || dtmp2 <= waferradius || dtmp3 <= waferradius)
                        {
                            if (flat_notch == WaferNotchTypeEnum.NOTCH)
                            {
                                if (wafer_size == EnumWaferSize.INCH6 || wafer_size == EnumWaferSize.INCH8)
                                {
                                    if (flat_notch_dir == 0)
                                    {
                                        if ((rectstartx + (i * die_size_x)) <= (wafercenterx + (waferradius - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 90)
                                    {
                                        if ((rectstarty + (j * die_size_y)) <= (wafercentery + (waferradius - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 180)
                                    {
                                        if ((rectstartx + ((i + 1) * die_size_x)) >= (wafercenterx - (waferradius - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }

                                    else if (flat_notch_dir == 270)
                                    {
                                        if ((rectstarty + ((j + 1) * die_size_y)) >= (wafercentery - (waferradius - flatdepth)))
                                        {
                                            if (mark_die_on)
                                            {
                                                bmap[i, j] = 3;
                                            }
                                            else
                                            {
                                                bmap[i, j] = 1;
                                            }
                                        }
                                        else
                                        {
                                            bmap[i, j] = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    if (mark_die_on)
                                    {
                                        bmap[i, j] = 3;
                                    }
                                }
                            }
                            else
                            {
                                if (mark_die_on)
                                {
                                    bmap[i, j] = 3;
                                }
                                else
                                {
                                    bmap[i, j] = 1;
                                }
                            }
                        }
                        else
                        {
                            bmap[i, j] = 0;
                        }
                    }
                }

                try
                {
                    //string filepath;
                    //StringBuilder stb = new StringBuilder();

                    //FileManager.GetXMLFilePath(typeof(WaferObject), out filepath);
                    //filepath = "C:\\Logs\\ProberSystem\\WaferMap";

                    //if (Directory.Exists(Path.GetDirectoryName(filepath)) == false)
                    //{
                    //    Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                    //}

                    //using (StreamWriter fileStream = new StreamWriter(filepath + ".dbg"))
                    //{
                    //    stb.Clear();
                    //    stb.Append(string.Format("Y \\ X  "));
                    //    for (int x = 0; x < die_num_x; x++)
                    //    {
                    //        stb.Append(string.Format("[{0:0000}]", x));
                    //    }
                    //    fileStream.WriteLine(stb);
                    //    for (int y = 0; y < die_num_y; y++)
                    //    {
                    //        stb.Clear();
                    //        stb.Append(string.Format("[{0:0000}] ", y));
                    //        for (int x = 0; x < die_num_x; x++)
                    //        {
                    //            stb.Append(string.Format("    {0:00}", bmap[x, y]));
                    //        }
                    //        //writeBuffer = Encoding.ASCII.GetBytes(stb.ToString());
                    //        //fileStream.WriteLine(writeBuffer);
                    //        fileStream.WriteLine(stb);
                    //    }
                    //}
                }
                catch (Exception err)
                {
                    //LoggerManager.Error($err, "AutoCalWaferMap() : Error occurred.");
                    LoggerManager.Exception(err);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return bmap;
        }

        private double Distance2D(double x1, double y1, double x2, double y2)
        {
            double distance = 0.0;
            try
            {
                distance = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return distance;
        }

        public void SubstrateInfoInit()
        {
            try
            {
                if (Info == null)
                {
                    Info = new SubstrateInfo();
                }

                Info.Init();

                Info.ActualDieSize.Width.Value = PhysInfo.DieSizeX.Value;
                Info.ActualDieSize.Height.Value = PhysInfo.DieSizeY.Value;

                Info.ActualDeviceSize.Width.Value = (Info.ActualDieSize.Width.Value - PhysInfo.DieXClearance.Value);
                Info.ActualDeviceSize.Height.Value = (Info.ActualDieSize.Height.Value - PhysInfo.DieYClearance.Value);

                Info.ActualThickness = PhysInfo.Thickness.Value;

                if (this.GetContainer().IsRegistered<IStageSupervisor>())
                {
                    Info.PMIInfo.Init();
                }

                var waferObj = (this.Owner as WaferObject);
                if (waferObj != null)
                {
                    waferObj.WaferDevObject = this;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public void UpdateWaferBoundary()
        {
            try
            {
                if (!this.GetContainer().IsRegistered<IWaferAligner>())
                    return;

                Point centerdierefcorner = this.WaferAligner().GetLeftCornerPosition(Info.WaferCenter);

                Point gDWWaferAlignLLC = new Point(
                    Info.WaferCenter.GetX() + PhysInfo.LowLeftCorner.GetX(),
                    Info.WaferCenter.GetY() + PhysInfo.LowLeftCorner.GetY());

                if (Math.Abs(Info.WaferCenter.GetX() - gDWWaferAlignLLC.X) > Info.ActualDieSize.Width.Value)
                    centerdierefcorner.X = gDWWaferAlignLLC.X + (Info.ActualDieSize.Width.Value * Math.Truncate((Info.WaferCenter.GetX() - gDWWaferAlignLLC.X) / Info.ActualDieSize.Width.Value));
                else
                    centerdierefcorner.X = gDWWaferAlignLLC.X;

                if (centerdierefcorner.X > Info.WaferCenter.GetX())
                    centerdierefcorner.X = centerdierefcorner.X - Info.ActualDieSize.Width.Value;

                if (Math.Abs(Info.WaferCenter.GetY() - gDWWaferAlignLLC.Y) > Info.ActualDieSize.Height.Value)
                    centerdierefcorner.Y = gDWWaferAlignLLC.Y + (Info.ActualDieSize.Height.Value * Math.Truncate((Info.WaferCenter.GetY() - gDWWaferAlignLLC.Y) / Info.ActualDieSize.Height.Value));
                else
                    centerdierefcorner.Y = gDWWaferAlignLLC.Y;

                if (centerdierefcorner.Y > Info.WaferCenter.GetY())
                    centerdierefcorner.Y = centerdierefcorner.Y - Info.ActualDieSize.Height.Value;

                Info.RefDieLeftCorner.X.Value = centerdierefcorner.X;
                Info.RefDieLeftCorner.Y.Value = centerdierefcorner.Y;
                LoggerManager.Debug($"RefDieLeftCorner = ({Info.RefDieLeftCorner.X.Value:0.00},{Info.RefDieLeftCorner.Y.Value:0.00})");
                //PhysInfo.CenU = new ElemUserIndex(PhysInfo.CenM.XIndex.Value, PhysInfo.CenM.YIndex.Value);

                var userIndex = this.CoordinateManager().WMIndexConvertWUIndex(PhysInfo.CenM.XIndex.Value, PhysInfo.CenM.YIndex.Value);
                PhysInfo.CenU = new ElemUserIndex(userIndex.XIndex, userIndex.YIndex);

                PhysInfo.OrgM = new ElemMachineIndex(0, 0);
                PhysInfo.OrgU = new ElemUserIndex(0, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public void CalcWaferCenterIndex()
        {
            try
            {
                if (!this.GetContainer().IsRegistered<IWaferAligner>())
                    return;

                Point centerdierefcorner = this.WaferAligner().GetLeftCornerPosition(Info.WaferCenter);
                //PhysInfo.LowLeftCorner.X.Value = centerdierefcorner.X;
                //PhysInfo.LowLeftCorner.Y.Value = centerdierefcorner.Y;

                Point gDWWaferAlignLLC = new Point(
                    Info.WaferCenter.GetX() + PhysInfo.LowLeftCorner.GetX(),
                    Info.WaferCenter.GetY() + PhysInfo.LowLeftCorner.GetY());

                if (Math.Abs(Info.WaferCenter.GetX() - gDWWaferAlignLLC.X) > Info.ActualDieSize.Width.Value)
                    centerdierefcorner.X = gDWWaferAlignLLC.X + (Info.ActualDieSize.Width.Value * Math.Truncate((Info.WaferCenter.GetX() - gDWWaferAlignLLC.X) / Info.ActualDieSize.Width.Value));
                else
                    centerdierefcorner.X = gDWWaferAlignLLC.X;

                if (centerdierefcorner.X > Info.WaferCenter.GetX())
                    centerdierefcorner.X = centerdierefcorner.X - Info.ActualDieSize.Width.Value;

                if (Math.Abs(Info.WaferCenter.GetY() - gDWWaferAlignLLC.Y) > Info.ActualDieSize.Height.Value)
                    centerdierefcorner.Y = gDWWaferAlignLLC.Y + (Info.ActualDieSize.Height.Value * Math.Truncate((Info.WaferCenter.GetY() - gDWWaferAlignLLC.Y) / Info.ActualDieSize.Height.Value));
                else
                    centerdierefcorner.Y = gDWWaferAlignLLC.Y;

                if (centerdierefcorner.Y > Info.WaferCenter.GetY())
                    centerdierefcorner.Y = centerdierefcorner.Y - Info.ActualDieSize.Height.Value;

                var avaWaferSize = PhysInfo.WaferSize_um.Value - PhysInfo.WaferMargin_um.Value * 2.0;

                //double ddummyrightcount = ((avaWaferSize / 2) + (Info.WaferCenter.GetX() - centerdierefcorner.X)) / Info.ActualDieSize.Width.Value;
                //rightcount : Center die 기준으로 오른쪽에 다이가 몇 개 있나
                int rightcount = Convert.ToInt32(Math.Round(((avaWaferSize / 2) + (Info.WaferCenter.GetX() - centerdierefcorner.X)) / Info.ActualDieSize.Width.Value)) - 1;
                //if ((ddummyrightcount % Info.ActualDieSize.Width.Value) != 0)
                //    rightcount = rightcount + 1;

                //double ddummyleftcount = ((avaWaferSize / 2) - (Info.WaferCenter.GetX() - centerdierefcorner.X)) / Info.ActualDieSize.Width.Value;
                //leftcount : Center die 포함해서 왼쪽으로 다이가 몇 개 있나 (즉, 이 값이 센터 다이의 머신 좌표 X) 
                int leftcount = Convert.ToInt32(Math.Round(((avaWaferSize / 2) - (Info.WaferCenter.GetX() - centerdierefcorner.X)) / Info.ActualDieSize.Width.Value));

                //if ((ddummyleftcount % Info.ActualDieSize.Width.Value) != 0)
                //{
                //    leftcount = leftcount + 1;
                //}

                PhysInfo.MapCountX.Value = rightcount + leftcount;
                if (PhysInfo.CenM == null)
                    PhysInfo.CenM = new ElemMachineIndex();

                if ((PhysInfo.MapCountX.Value % 2) == 0)
                {
                    PhysInfo.CenM.XIndex.Value = PhysInfo.MapCountX.Value / 2;
                }
                else
                {
                    PhysInfo.CenM.XIndex.Value = leftcount - 1;
                }

                double ddummyuppercount = ((avaWaferSize / 2) + (Info.WaferCenter.GetY() - centerdierefcorner.Y)) / Info.ActualDieSize.Height.Value;
                //uppercount : Center die �������� ���ʿ� ���̰� �� �� �ֳ�
                int uppercount = Convert.ToInt32(Math.Round(((avaWaferSize / 2) + (Info.WaferCenter.GetY() - centerdierefcorner.Y)) / Info.ActualDieSize.Height.Value)) - 1;

                //if ((ddummyuppercount % Info.ActualDieSize.Height.Value) != 0)
                //{
                //    uppercount = uppercount + 1;
                //}

                double ddummylowercount = ((avaWaferSize / 2) - (Info.WaferCenter.GetY() - centerdierefcorner.Y)) / Info.ActualDieSize.Height.Value;
                //lowercount : Center die �����ؼ� �Ʒ������� ���̰� �� �� �ֳ�
                int lowercount = Convert.ToInt32(Math.Round(((avaWaferSize / 2) - (Info.WaferCenter.GetY() - centerdierefcorner.Y)) / Info.ActualDieSize.Height.Value));

                //if ((ddummylowercount % Info.ActualDieSize.Height.Value) != 0)
                //{
                //    lowercount = lowercount + 1;
                //}

                PhysInfo.MapCountY.Value = uppercount + lowercount;

                if ((PhysInfo.MapCountY.Value % 2) == 0)
                {
                    PhysInfo.CenM.YIndex.Value = PhysInfo.MapCountY.Value / 2;
                }
                else
                {
                    PhysInfo.CenM.YIndex.Value = lowercount - 1;
                }

                Info.RefDieLeftCorner.X.Value = centerdierefcorner.X;
                Info.RefDieLeftCorner.Y.Value = centerdierefcorner.Y;

                PhysInfo.CenU = new ElemUserIndex(PhysInfo.CenM.XIndex.Value, PhysInfo.CenM.YIndex.Value);
                PhysInfo.OrgM = new ElemMachineIndex(0, 0);
                PhysInfo.OrgU = new ElemUserIndex(0, 0);
                PhysInfo.RefU = new ElemUserIndex(0, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void CalcMapOffset()
        {
            try
            {
                if ((PhysInfo.MapCountX.Value % 2) != 0)
                    PhysInfo.CenDieOffset.X.Value = -((Info.WaferCenter.GetX() - Info.RefDieLeftCorner.X.Value) - Info.ActualDieSize.Width.Value / 2);
                else
                    PhysInfo.CenDieOffset.X.Value = -(Info.ActualDieSize.Width.Value - (Info.WaferCenter.GetX() - Info.RefDieLeftCorner.X.Value));

                if ((PhysInfo.MapCountY.Value % 2) != 0)
                    PhysInfo.CenDieOffset.Y.Value = -((Info.WaferCenter.GetY() - Info.RefDieLeftCorner.Y.Value) - Info.ActualDieSize.Height.Value / 2);
                else
                    PhysInfo.CenDieOffset.Y.Value = -(Info.ActualDieSize.Height.Value - (Info.WaferCenter.GetY() - Info.RefDieLeftCorner.Y.Value));

                //PhysInfo.CenDieOffset.X.Value = 0;
                //PhysInfo.CenDieOffset.Y.Value = 0;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        //public EventCodeEnum CalcRefDieOffset()
        //{
        //    EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
        //    try
        //    {
        //        double centeroffsetx = 0.0;
        //        double centeroffsety = 0.0;

        //        double indexWidth = Info.ActualDieSize.Width.Value;
        //        double indexHeight = Info.ActualDieSize.Height.Value;
        //        double wafercenterX = Info.WaferCenter.GetX();
        //        double wafercenterY = Info.WaferCenter.GetY();
        //        double wSize = PhysInfo.WaferSize_um.Value;

        //        try
        //        {
        //            long hordiecount = PhysInfo.MapCountX.Value;
        //            long vertiecount = PhysInfo.MapCountY.Value;

        //            if (hordiecount % 2 != 0)
        //            {
        //                //Horizantal Odd
        //                //centeroffsetx = -((indexWidth - PhysInfo.DieXClearance.Value) / 2.0);
        //                centeroffsetx = (Info.RefDieLeftCorner.GetX() + (indexWidth - PhysInfo.DieXClearance.Value) / 2.0) - Info.WaferCenter.GetX();
        //            }
        //            else
        //            {
        //                //Horizantal Even
        //                //centeroffsetx = Info.RefDieLeftCorner.GetX() - Info.WaferCenter.GetX();
        //                centeroffsetx = Info.WaferCenter.GetX();
        //            }

        //            if (vertiecount % 2 != 0)
        //            {
        //                //Vertical Odd
        //                //centeroffsety = -((indexHeight - PhysInfo.DieYClearance.Value) / 2.0);
        //                centeroffsety = (Info.RefDieLeftCorner.GetY() + (indexHeight - PhysInfo.DieYClearance.Value) / 2.0) - Info.WaferCenter.GetY();
        //            }
        //            else
        //            {
        //                //Vertical Even
        //                //centeroffsety = Info.RefDieLeftCorner.GetY() - Info.WaferCenter.GetY();
        //                centeroffsety = Info.WaferCenter.GetY();
        //            }


        //            PhysInfo.CenDieOffset.X.Value = centeroffsetx;
        //            PhysInfo.CenDieOffset.Y.Value = centeroffsety;

        //            LoggerManager.Debug($"[CalcRefDieOffset] WaferCenterX : {Info.WaferCenter.GetX()}, WaferCetnerY : {Info.WaferCenter.GetY()}");
        //            LoggerManager.Debug($"RefDieLeftCornerX : {Info.RefDieLeftCorner.GetX()}, RefDieLeftCornerY : {Info.RefDieLeftCorner.GetY()}");
        //            LoggerManager.Debug($"RefDieOffsetX : {PhysInfo.CenDieOffset.X.Value}, RefDieOffsetY : {PhysInfo.CenDieOffset.Y.Value }");


        //            retVal = EventCodeEnum.NONE;
        //        }

        //        catch (Exception err)
        //        {
        //            LoggerManager.Exception(err);
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //    return retVal;
        //}

        public EventCodeEnum Init()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {

                if (PhysInfo.OCRMode == null)
                {
                    PhysInfo.OCRMode = new Element<OCRModeEnum>();
                    PhysInfo.OCRMode.Value = OCRModeEnum.NONE;
                }

                if (PhysInfo.OCRType == null)
                {
                    PhysInfo.OCRType = new Element<OCRTypeEnum>();
                    PhysInfo.OCRType.Value = OCRTypeEnum.UNDEFINED;
                }

                if (PhysInfo.OCRDirection == null)
                {
                    PhysInfo.OCRDirection = new Element<OCRDirectionEnum>();
                    PhysInfo.OCRDirection.Value = OCRDirectionEnum.UNDEFINED;
                }

                try
                {
                    SubstrateInfoInit();

                    if (PhysInfo.WaferSize_um.Value != 0)
                    {
                        switch (PhysInfo.WaferSize_um.Value)
                        {
                            case 150000:
                                PhysInfo.WaferSizeEnum = EnumWaferSize.INCH6;
                                break;
                            case 200000:
                                PhysInfo.WaferSizeEnum = EnumWaferSize.INCH8;
                                break;
                            case 300000:
                                PhysInfo.WaferSizeEnum = EnumWaferSize.INCH12;
                                break;
                        }
                    }

                    InitMap();

                    //Test Code
                    //SetLotDeaultParam();
                    retVal = EventCodeEnum.NONE;
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    //LoggerManager.Debug($"[WaferObject] [Method = Init] [Error = {err}]");
                    retVal = EventCodeEnum.PARAM_ERROR;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        //public void SetLotDeaultParam()
        //{
        //    try
        //    {
        //        Info.DeviceName.Value = "ASFDEFDBFBDFGDBVFDGRGDFGBFDGFDGFDGFG";
        //        Info.OperatorName.Value = "SADWRSDF";
        //        //PhysInfo.CassetteID.Value = "82148738578435";
        //        PhysInfo.WaferID.Value = "SAFDWERSDGSDFGSDF-01";
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //        throw;
        //    }
        //}

        public WaferDevObject()
        {
        }

        #endregion

        public void SetElementMetaData()
        {
            PhysInfo.OCRMode.ElementName = "OCRMode";
            PhysInfo.OCRMode.CategoryID = "10013201";

            PhysInfo.OCRType.ElementName = "OCRType";
            PhysInfo.OCRType.CategoryID = "10013201";

            PhysInfo.OCRDirection.ElementName = "OCRDirection";
            PhysInfo.OCRDirection.CategoryID = "10013201";
        }
    }
}
