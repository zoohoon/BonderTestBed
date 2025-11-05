using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SorterSystem.OpenCV
{
    public partial class VisionOpenCV
    {
        public class Parameters : INotifyPropertyChanged
        {
            #region ==> PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            private int _ThresholdMin = 50;
            public int ThresholdMin
            {
                get { return _ThresholdMin; }
                set
                {
                    if (value != _ThresholdMin)
                    {
                        _ThresholdMin = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private int _ThresholdMax = 255;
            public int ThresholdMax
            {
                get { return _ThresholdMax; }
                set
                {
                    if (value != _ThresholdMax)
                    {
                        _ThresholdMax = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private int _SobelKSize = 3;
            public int SobelKSize
            {
                get { return _SobelKSize; }
                set
                {
                    if (value != _SobelKSize)
                    {
                        _SobelKSize = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private int _SobelScale = 1;
            public int SobelScale
            {
                get { return _SobelScale; }
                set
                {
                    if (value != _SobelScale)
                    {
                        _SobelScale = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private int _MorphologyOpen = 51;
            public int MorphologyOpen
            {
                get { return _MorphologyOpen; }
                set
                {
                    if (value != _MorphologyOpen)
                    {
                        _MorphologyOpen = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private int _MorphologyClose = 5;
            public int MorphologyClose
            {
                get { return _MorphologyClose; }
                set
                {
                    if (value != _MorphologyClose)
                    {
                        _MorphologyClose = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private int _BlobMinArea = 100;
            public int BlobMinArea
            {
                get { return _BlobMinArea; }
                set
                {
                    if (value != _BlobMinArea)
                    {
                        _BlobMinArea = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private int _ProfileStep = 15;
            public int ProfileStep
            {
                get { return _ProfileStep; }
                set
                {
                    if (value != _ProfileStep)
                    {
                        _ProfileStep = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private bool _ImageInvert = false;
            public bool ImageInvert
            {
                get { return _ImageInvert; }
                set
                {
                    if (value != _ImageInvert)
                    {
                        _ImageInvert = value;
                        RaisePropertyChanged();
                    }

                }
            }

            private bool _LineDetection = true;
            public bool LineDetection
            {
                get { return _LineDetection; }
                set
                {
                    if (value != _LineDetection)
                    {
                        _LineDetection = value;
                        RaisePropertyChanged();
                    }

                }
            }
        }

        public class ParamsMatching : INotifyPropertyChanged
        {
            #region ==> PropertyChanged
            public event PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            private int _MinScore = 20;
            public int MinScore
            {
                get { return _MinScore; }
                set
                {
                    if (value != _MinScore)
                    {
                        _MinScore = value;
                        RaisePropertyChanged();
                    }
                }
            }

            private int _MaxAngle = 50;
            public int MaxAngle
            {
                get { return _MaxAngle; }
                set
                {
                    if (value != _MaxAngle)
                    {
                        _MaxAngle = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }
    }
}
