using LogModule;
using Newtonsoft.Json;
using ProberInterfaces.AlignEX;
using ProberInterfaces.WaferAlign;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProberInterfaces.WaferAlignEX
{
    [Serializable]
    public class WaferAlignAcqStep : WaferAlignAcqStepBase, INotifyPropertyChanged
    {
        private IWaferAligner _WaferAligner;
        [XmlIgnore, JsonIgnore]
        public IWaferAligner WaferAligner
        {
            get { return _WaferAligner; }
            set { _WaferAligner = value; }
        }
        private ObservableCollection<AlignProcessAcqBase> _Acquisitions
            = new ObservableCollection<AlignProcessAcqBase>();

        public override ObservableCollection<AlignProcessAcqBase> Acquisitions
        {
            get { return _Acquisitions; }
            set
            {
                if (value != _Acquisitions)
                {
                    _Acquisitions = value;
                    RaisePropertyChanged();
                }
            }
        }

        public WaferAlignAcqStep()
        {

        }
        public WaferAlignAcqStep(IWaferAligner waferaligner)
        {
            WaferAligner = waferaligner;
        }
        public WaferAlignAcqStep(ObservableCollection<AlignProcessAcqBase> acqs)
        {
            Acquisitions = acqs;
        }

        public WaferAlignAcqStep(AlignProcessAcqBase acq)
        {
            Acquisitions.Add(acq);
        }
        public WaferAlignAcqStep(WaferAlignProcAcqEnum[] acqtypes)
        {
            try
            {
            Acquisitions = new ObservableCollection<AlignProcessAcqBase>();

            foreach (WaferAlignProcAcqEnum acqtype in acqtypes)
            {
                switch (acqtype)
                {
                    case WaferAlignProcAcqEnum.UNDEFINED:
                        break;
                    case WaferAlignProcAcqEnum.WAFER_CENTER:
                        Acquisitions.Add(new WaferCenter());
                        break;
                    case WaferAlignProcAcqEnum.LOW_MAG_ALIGN:
                        Acquisitions.Add(new LowMagAlign());
                        break;
                    case WaferAlignProcAcqEnum.HI_MAG_ALIGN:
                        Acquisitions.Add(new HighMagAlign());
                        break;
                    case WaferAlignProcAcqEnum.DIE_SIZE_X:
                        Acquisitions.Add(new DieSizeX());
                        break;
                    case WaferAlignProcAcqEnum.DIE_SIZE_Y:
                        Acquisitions.Add(new DieSizeY());
                        break;
                    case WaferAlignProcAcqEnum.ANGLE:
                        Acquisitions.Add(new WaferAngle());
                        break;
                    case WaferAlignProcAcqEnum.HD_ANGLE:
                        Acquisitions.Add(new HDWaferAngle());
                        break;
                    case WaferAlignProcAcqEnum.SQUARENESS:
                        Acquisitions.Add(new WaferSquareness());
                        break;
                    case WaferAlignProcAcqEnum.HEIGHT_PROFILE:
                        Acquisitions.Add(new HeightProfile());
                        break;
                    case WaferAlignProcAcqEnum.LOWER_LEFT_EDGE:
                        Acquisitions.Add(new LowerLeftCorner());
                        break;
                    case WaferAlignProcAcqEnum.COORD_ORIGIN:
                        Acquisitions.Add(new CoordinateOrigin());
                        break;
                    case WaferAlignProcAcqEnum.SINGULATION_OFFSET:
                        Acquisitions.Add(new SingulationOffset());
                        break;
                    default:
                        break;
                }
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public WaferAlignAcqStep(WaferAlignProcAcqEnum acqtype)
        {
            try
            {
            Acquisitions = new ObservableCollection<AlignProcessAcqBase>();

            switch (acqtype)
            {
                case WaferAlignProcAcqEnum.UNDEFINED:
                    break;
                case WaferAlignProcAcqEnum.WAFER_CENTER:
                    Acquisitions.Add(new WaferCenter());
                    break;
                case WaferAlignProcAcqEnum.LOW_MAG_ALIGN:
                    Acquisitions.Add(new LowMagAlign());
                    break;
                case WaferAlignProcAcqEnum.HI_MAG_ALIGN:
                    Acquisitions.Add(new HighMagAlign());
                    break;
                case WaferAlignProcAcqEnum.DIE_SIZE_X:
                    Acquisitions.Add(new DieSizeX());
                    break;
                case WaferAlignProcAcqEnum.DIE_SIZE_Y:
                    Acquisitions.Add(new DieSizeY());
                    break;
                case WaferAlignProcAcqEnum.ANGLE:
                    Acquisitions.Add(new WaferAngle());
                    break;
                case WaferAlignProcAcqEnum.HD_ANGLE:
                    Acquisitions.Add(new HDWaferAngle());
                    break;
                case WaferAlignProcAcqEnum.SQUARENESS:
                    Acquisitions.Add(new WaferSquareness());
                    break;
                case WaferAlignProcAcqEnum.HEIGHT_PROFILE:
                    Acquisitions.Add(new HeightProfile());
                    break;
                case WaferAlignProcAcqEnum.LOWER_LEFT_EDGE:
                    Acquisitions.Add(new LowerLeftCorner());
                    break;
                case WaferAlignProcAcqEnum.COORD_ORIGIN:
                    Acquisitions.Add(new CoordinateOrigin());
                    break;
                case WaferAlignProcAcqEnum.SINGULATION_OFFSET:
                    Acquisitions.Add(new SingulationOffset());
                    break;
                default:
                    break;
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

    }
}
