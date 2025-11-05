using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SorterOpenCV.OpenCV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SorterOpenCV
{
    class MainVM : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        private Mat _CurrentMat = null;
        private Mat _ReferenceMat = null;
        private Mat _DebugMat = null;


        private Dictionary<string, Mat> _ResultMat;

        private ObservableCollection<string> _ListMat;
        public ObservableCollection<string> ListMat
        {
            get { return _ListMat; }
            set
            {
                if (value != _ListMat)
                {
                    _ListMat = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _MatSelectedItem;
        public string MatSelectedItem
        {
            get { return _MatSelectedItem; }
            set
            {
                if (value != _MatSelectedItem)
                {
                    _MatSelectedItem = value;
                    RaisePropertyChanged();

                    if (value != null)
                    {
                        if (_ResultMat.TryGetValue(value, out Mat mat))
                        {
                            ImageSource = mat.ToBitmapSource();
                        }
                        else
                        {
                            ImageSource = null;
                        }
                    }
                }
            }
        }

        private string _ImageSourceOrg;
        public string ImageSourceOrg
        {
            get { return _ImageSourceOrg; }
            set
            {
                if (value != _ImageSourceOrg)
                {
                    _ImageSourceOrg = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _ImageSourceRef;
        public string ImageSourceRef
        {
            get { return _ImageSourceRef; }
            set
            {
                if (value != _ImageSourceRef)
                {
                    _ImageSourceRef = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BitmapSource _ImageSource;
        public BitmapSource ImageSource
        {
            get { return _ImageSource; }
            set
            {
                if (value != _ImageSource)
                {
                    _ImageSource = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICommand _CommandLoad;
        public ICommand CommandLoad
        {
            get
            {
                return (this._CommandLoad) ??
                    (this._CommandLoad = new DelegateCommand(OnCommandLoad));
            }
        }
        private void OnCommandLoad()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ImageSourceOrg = openFileDialog.FileName;
                _CurrentMat = Cv2.ImRead(openFileDialog.FileName);
                if (_CurrentMat.Channels() == 3)
                {
                    Cv2.CvtColor(_CurrentMat, _CurrentMat, ColorConversionCodes.BGR2GRAY);
                }
                if (_DebugMat != null)
                    _DebugMat.Release();
            }
        }

        private ICommand _CommandLoadRef;
        public ICommand CommandLoadRef
        {
            get
            {
                return (this._CommandLoadRef) ??
                    (this._CommandLoadRef = new DelegateCommand(OnCommandLoadRef));
            }
        }
        private void OnCommandLoadRef()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ImageSourceRef = openFileDialog.FileName;
                _ReferenceMat = Cv2.ImRead(openFileDialog.FileName);
                if (_ReferenceMat.Channels() == 3)
                {
                    Cv2.CvtColor(_ReferenceMat, _ReferenceMat, ColorConversionCodes.BGR2GRAY);
                }
                Cv2.ImShow("Reference", _ReferenceMat);
            }
        }

        private ICommand _Command1;
        public ICommand Command1
        {
            get
            {
                return (this._Command1) ??
                    (this._Command1 = new DelegateCommand(OnCommand1));
            }
        }

         private void OnCommand1()
        {
            if (_CurrentMat != null)
            {
                ClearMat();
                if (_DebugMat != null)
                    _DebugMat.Release();
                _DebugMat = _CurrentMat.Clone();

                List<stObject> outObject;
                VisionOpenCV.OpenCVFindEdgeDeri(_CurrentMat, _DebugMat, VisionParameter, out Dictionary<string, Mat> resultMat, out outObject);
                DisplayMat(resultMat);

                SaveDbgImage();

                foreach (stObject _res in outObject)
                {
                    Console.WriteLine(_res.eDiePos + ", posCenter=>" + _res.posCenter.ToString() + ", posVertex=>" + _res.posVertex.ToString());
                }
            }
        }

        private ICommand _Command2;
        public ICommand Command2
        {
            get
            {
                return (this._Command2) ??
                    (this._Command2 = new DelegateCommand(OnCommand2));
            }
        }
        private void OnCommand2()
        {
            if (_CurrentMat != null)
            {
                ClearMat();
                if (_DebugMat != null)
                    _DebugMat.Release();
                _DebugMat = _CurrentMat.Clone();

                List<stObject> outObject;
                VisionOpenCV.OpenCVFindEdgeDeriEx(_CurrentMat, _DebugMat, VisionParameter, out Dictionary<string, Mat> resultMat, out outObject);
                DisplayMat(resultMat);

                SaveDbgImage();

                foreach (stObject _res in outObject)
                {
                    Console.WriteLine(_res.eDiePos + ", posCenter=>" + _res.posCenter.ToString() + ", posVertex=>" + _res.posVertex.ToString());
                }
            }
        }

        private ICommand _Command3;
        public ICommand Command3
        {
            get
            {
                return (this._Command3) ??
                    (this._Command3 = new DelegateCommand(OnCommand3));
            }
        }
        private void OnCommand3()
        {
            if (_CurrentMat != null)
            {
                ClearMat();
                if (_DebugMat != null)
                    _DebugMat.Release();
                _DebugMat = _CurrentMat.Clone();

                List<stObject> outResult;
                VisionOpenCV.OpenCVFindEdgeThres(_CurrentMat, _DebugMat, VisionParameter, out Dictionary<string, Mat> resultMat, out outResult);
                DisplayMat(resultMat);

                SaveDbgImage();

                foreach (stObject _res in outResult)
                {
                    Console.WriteLine(_res.eDiePos + ", posCenter=>" + _res.posCenter.ToString() + ", posVertex=>" + _res.posVertex.ToString());
                }
            }
        }

        private ICommand _Command4;
        public ICommand Command4
        {
            get
            {
                return (this._Command4) ??
                    (this._Command4 = new DelegateCommand(OnCommand4));
            }
        }
        private void OnCommand4()
        {
            if (_CurrentMat != null)
            {
                ClearMat();
                if (_DebugMat != null)
                    _DebugMat.Release();
                _DebugMat = _CurrentMat.Clone();

                List<stObject> outResult;
                VisionOpenCV.OpenCVFindEdgeThresEx(_CurrentMat, _DebugMat, VisionParameter, out Dictionary<string, Mat> resultMat, out outResult);
                DisplayMat(resultMat);

                SaveDbgImage();

                foreach (stObject _res in outResult)
                {
                    Console.WriteLine(_res.eDiePos + ", posCenter=>" + _res.posCenter.ToString() + ", posVertex=>" + _res.posVertex.ToString());
                }
            }
        }

        private ICommand _Command5;
        public ICommand Command5
        {
            get
            {
                return (this._Command5) ??
                    (this._Command5 = new DelegateCommand(OnCommand5));
            }
        }
        private void OnCommand5()
        {
        }

        private ICommand _Command6;
        public ICommand Command6
        {
            get
            {
                return (this._Command6) ??
                    (this._Command6 = new DelegateCommand(OnCommand6));
            }
        }
        private void OnCommand6()
        {
        }

        private ICommand _Command7;
        public ICommand Command7
        {
            get
            {
                return (this._Command7) ??
                    (this._Command7 = new DelegateCommand(OnCommand7));
            }
        }
        private void OnCommand7()
        {
        }

        private ICommand _Command8;
        public ICommand Command8
        {
            get
            {
                return (this._Command8) ??
                    (this._Command8 = new DelegateCommand(OnCommand8));
            }
        }
        private void OnCommand8()
        {
        }

        private ICommand _CmdMatch1;
        public ICommand CmdMatch1
        {
            get
            {
                return (this._CmdMatch1) ??
                    (this._CmdMatch1 = new DelegateCommand(OnCmdMatch1));
            }
        }
        private void OnCmdMatch1()
        {
            if (_ReferenceMat == null)
            {
                MessageBox.Show("Did not load reference image.. please check again!!!", "Error");
            }
            else if (_CurrentMat != null)
            {
                ClearMat();
                if (_DebugMat != null)
                    _DebugMat.Release();
                _DebugMat = _CurrentMat.Clone();

                stMachineResult outResult;
                VisionOpenCV.OpenCVMatching1(_CurrentMat, _DebugMat, _ReferenceMat, VisionParameterMatching, out Dictionary<string, Mat> resultMat, out outResult);
                DisplayMat(resultMat);

                SaveDbgImage();
                Console.WriteLine("posCenter=> " + outResult.ToString() + ", Score=> " + outResult.dScore.ToString() + ", Angle=> " + outResult.dAngle.ToString());
            }
        }

        private ICommand _CmdMatch2;
        public ICommand CmdMatch2
        {
            get
            {
                return (this._CmdMatch2) ??
                    (this._CmdMatch2 = new DelegateCommand(OnCmdMatch2));
            }
        }
        private void OnCmdMatch2()
        {
        }

        private ICommand _CmdMatch3;
        public ICommand CmdMatch3
        {
            get
            {
                return (this._CmdMatch3) ??
                    (this._CmdMatch3 = new DelegateCommand(OnCmdMatch3));
            }
        }
        private void OnCmdMatch3()
        {
        }

        private ICommand _CmdMatch4;
        public ICommand CmdMatch4
        {
            get
            {
                return (this._CmdMatch4) ??
                    (this._CmdMatch4 = new DelegateCommand(OnCmdMatch4));
            }
        }
        private void OnCmdMatch4()
        {
        }

        private VisionOpenCV.Parameters _VisionParameter = new VisionOpenCV.Parameters();
        public VisionOpenCV.Parameters VisionParameter
        {
            get { return _VisionParameter; }
            set
            {
                if (_VisionParameter != null)
                {
                    _VisionParameter = value;
                    RaisePropertyChanged();
                }
            }
        }

        private VisionOpenCV.ParamsMatching _VisionParameterMatching = new VisionOpenCV.ParamsMatching();
        public VisionOpenCV.ParamsMatching VisionParameterMatching
        {
            get { return _VisionParameterMatching; }
            set
            {
                if (_VisionParameterMatching != null)
                {
                    _VisionParameterMatching = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void ClearMat()
        {
            MatSelectedItem = null;
            ListMat = null;

            if (_ResultMat != null)
            {
                foreach (var mat in _ResultMat)
                {
                    mat.Value.Release();
                }

                _ResultMat.Clear();
            }
        }

        public void SaveDbgImage()
        {
            DateTime nowDate = DateTime.Now;
            string strNowDate;
            strNowDate = "[ " + DateTime.Now.ToString("yyyyMMdd hhmmss") + "] ";

            if (_ResultMat != null)
            {
                int index = 0;
                string _filename = "";
                foreach (var mat in _ResultMat)
                {
                    string SelectedItem = mat.Key;
                    _filename = "c:\\Test\\" + strNowDate + index + "_" + SelectedItem + "_" + ".bmp";
                    Cv2.ImWrite(_filename, mat.Value);

                    index++;
                }
            }
        }

        private void DisplayMat(Dictionary<string, Mat> Mats)
        {
            ListMat = new ObservableCollection<string>(Mats.Keys);
            _ResultMat = Mats;
            MatSelectedItem = Mats.Last().Key;
        }

        public void OpenCVDbgViewer(Mat _mat, string _strTitle = "Test", double _ZoomLevel = 1.0)
        {
            ImageSource = _mat.ToBitmapSource();
            //Cv2.ImShow(_strTitle.ToString(), _mat);
            //Mat matResize = new Mat();
            //Cv2.Resize(_mat, matResize, new OpenCvSharp.Size(_mat.Rows * _ZoomLevel, _mat.Cols * _ZoomLevel));
            //Cv2.ImShow(_strTitle.ToString(), matResize);
            //matResize.Release();
            //Cv2.WaitKey(500);
        }
    }
}