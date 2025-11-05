using LogModule;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ProberInterfaces;
using RelayCommandBase;
using SorterSystem.OpenCV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SorterSystem.VM
{
    public class VisionVM : INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public VisionVM(ISorterModuleVM vm)
        {
            _MainVM = vm;
        }


        private Dictionary<string, Mat> _ResultMat;

        private ISorterModuleVM _MainVM;

        private ObservableCollection<string> _ListMat = new ObservableCollection<string>();
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

        private bool _IsLoadFromFile = false;
        public bool IsLoadFromFile
        {
            get { return _IsLoadFromFile; }
            set
            {
                if (value != _IsLoadFromFile)
                {
                    _IsLoadFromFile = value;
                    RaisePropertyChanged();
                }
            }
        }

        private VisionOpenCV.Parameters _VisionParameter = new VisionOpenCV.Parameters();
        public VisionOpenCV.Parameters VisionParameter
        {
            get { return _VisionParameter; }
            set
            {
                if (value != _VisionParameter)
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

        private ObservableCollection<stObject> _VisionResult;
        public ObservableCollection<stObject> VisionResult
        {
            get { return _VisionResult; }
            set
            {
                if (value != _VisionResult)
                {
                    _VisionResult = value;
                    RaisePropertyChanged();
                }
            }
        }

        private ICommand _BtnSetCenterCommand;
        public ICommand BtnSetCenterCommand
        {
            get
            {
                return (this._BtnSetCenterCommand) ??
                    (this._BtnSetCenterCommand = new AsyncCommand(OnBtnSetCenter));
            }
        }
        private async Task OnBtnSetCenter()
        {
            try
            {
                _MainVM.AxisX.AxisObject.Status.Position.Ref = _MainVM.AxisX.AxisObject.Status.Position.Command;
                _MainVM.AxisY.AxisObject.Status.Position.Ref = _MainVM.AxisY.AxisObject.Status.Position.Command;
                _MainVM.AxisZ.AxisObject.Status.Position.Ref = _MainVM.AxisZ.AxisObject.Status.Position.Command;
                _MainVM.AxisT.AxisObject.Status.Position.Ref = _MainVM.AxisT.AxisObject.Status.Position.Command;
                _MainVM.AxisPZ.AxisObject.Status.Position.Ref = _MainVM.AxisPZ.AxisObject.Status.Position.Command;

                double rx = _MainVM.DisplayPort.AssignedCamera.GetRatioX();
                double ry = _MainVM.DisplayPort.AssignedCamera.GetRatioY();
                Point2d point = new Point2d();

                if (VisionResult.Count == 1)
                {
                    var obj = VisionResult.ElementAt(0);
                    point.X = (obj.posVertex.X - _ImageSource.Width * 0.5) * rx * -1.0;
                    point.Y = (obj.posVertex.Y - _ImageSource.Height * 0.5) * ry * +1.0;

                    _MainVM.AxisX.AxisObject.Status.Position.Ref = _MainVM.AxisX.AxisObject.Status.Position.Command + point.X;
                    _MainVM.AxisY.AxisObject.Status.Position.Ref = _MainVM.AxisY.AxisObject.Status.Position.Command + point.Y;
                }
                else if (VisionResult.Count > 1)
                {
                    foreach (var obj in VisionResult)
                    {
                        point.X = (obj.posVertex.X - _ImageSource.Width * 0.5) * rx * -1.0;
                        point.Y = (obj.posVertex.Y - _ImageSource.Height * 0.5) * ry * +1.0;

                        _MainVM.AxisX.AxisObject.Status.Position.Ref = _MainVM.AxisX.AxisObject.Status.Position.Command + point.X;
                        _MainVM.AxisY.AxisObject.Status.Position.Ref = _MainVM.AxisY.AxisObject.Status.Position.Command + point.Y;
                        Debug.WriteLine($"Vision Center: {point.X}, {point.Y}");
                    }
                }
                await _MainVM.AbsMove();
                await Task.Run(() => { });
            }
            catch(Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        private RelayCommand _Command1;
        public ICommand Command1
        {
            get
            {
                return (this._Command1) ??
                    (this._Command1 = new RelayCommand(OnCommand1));
            }
        }
        private void OnCommand1()
        {
            try
            {
                ClearMat();

                LoadMat(out Mat orgMat);
                if (orgMat != null)
                {
                    Mat dbgMat = orgMat.Clone();
                    VisionOpenCV.OpenCVFindEdgeDeri(orgMat, dbgMat, VisionParameter, out Dictionary<string, Mat> resultMat, out List<stObject> outResult);
                    DisplayMat(resultMat, outResult);
                }
                else
                {
                    MessageBox.Show("Mat is null.");
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _Command2;
        public ICommand Command2
        {
            get
            {
                return (this._Command2) ??
                    (this._Command2 = new RelayCommand(OnCommand2));
            }
        }
        private void OnCommand2()
        {
            try
            {
                ClearMat();

                LoadMat(out Mat orgMat);
                if (orgMat != null)
                {
                    Mat dbgMat = orgMat.Clone();
                    VisionOpenCV.OpenCVFindEdgeThres(orgMat, dbgMat, VisionParameter, out Dictionary<string, Mat> resultMat, out List<stObject> outResult);
                    DisplayMat(resultMat, outResult);
                    //SaveDbgImage();
                }
                else
                {
                    MessageBox.Show("Mat is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _Command3;
        public RelayCommand Command3
        {
            get
            {
                return (this._Command3) ??
                    (this._Command3 = new RelayCommand(OnCommand3));
            }
        }
        private void OnCommand3()
        {
            try
            {
                ClearMat();

                LoadMat(out Mat orgMat);
                if (orgMat != null)
                {
                    Mat dbgMat = orgMat.Clone();
                    VisionOpenCV.OpenCVFindEdgeThresEx(orgMat, dbgMat, VisionParameter, out Dictionary<string, Mat> resultMat, out List<stObject> outResult);
                    DisplayMat(resultMat, outResult);
                    //SaveDbgImage();
                }
                else
                {
                    MessageBox.Show("Mat is null.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _Command4;
        public RelayCommand Command4
        {
            get
            {
                return (this._Command4) ??
                    (this._Command4 = new RelayCommand(OnCommand4));
            }
        }
        private void OnCommand4()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                ImageSourceRef = openFileDialog.FileName;
                Mat _ReferenceMat = Cv2.ImRead(openFileDialog.FileName);
                if (_ReferenceMat.Channels() == 3)
                {
                    Cv2.CvtColor(_ReferenceMat, _ReferenceMat, ColorConversionCodes.BGR2GRAY);
                }
                Cv2.ImShow("Reference", _ReferenceMat);
            }
        }

        private RelayCommand _Command5;
        public RelayCommand Command5
        {
            get
            {
                return (this._Command5) ??
                    (this._Command5 = new RelayCommand(OnCommand5));
            }
        }
        private void OnCommand5()
        {
            try
            {
                ClearMat();

                LoadFromFile(ImageSourceRef, out Mat refMat);
                if (refMat == null)
                {
                    MessageBox.Show("Did not load reference image.. please check again!!!", "Error");
                    return;
                }

                LoadMat(out Mat orgMat);
                if (orgMat != null)
                {
                    Mat dbgMat = orgMat.Clone();
                    VisionOpenCV.OpenCVMatching1(orgMat, dbgMat, refMat, VisionParameterMatching, out Dictionary<string, Mat> resultMat, out stMachineResult outResult);
                    DisplayMat(resultMat, null);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand _Command6;
        public RelayCommand Command6
        {
            get
            {
                return (this._Command6) ??
                    (this._Command6 = new RelayCommand(OnCommand6));
            }
        }
        private void OnCommand6()
        {
        }

        private RelayCommand _Command7;
        public RelayCommand Command7
        {
            get
            {
                return (this._Command7) ??
                    (this._Command7 = new RelayCommand(OnCommand7));
            }
        }
        private void OnCommand7()
        {
        }

        private void LoadFromFile(string filename, out Mat mat)
        {
            mat = null;

            try
            {
                mat = Cv2.ImRead(filename);
                if (mat.Channels() == 3)
                {
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2GRAY);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                mat = null;
            }
        }

        private void LoadFromFile(out Mat mat)
        {
            mat = null;

            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == true)
                {
                    LoadFromFile(ofd.FileName, out mat);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                mat = null;
            }
        }

        private void LoadFromCam(out Mat mat)
        {
            mat = null;

            try
            {
                if (_MainVM != null && _MainVM.DisplayPort != null)
                {
                    ICamera ACam = _MainVM.DisplayPort.AssignedCamera;
                    if (ACam != null)
                    {
                        ACam.GetCurImage(out ImageBuffer imageBuffer);
                        mat = Mat.FromPixelData(imageBuffer.SizeX, imageBuffer.SizeY, MatType.CV_8U, imageBuffer.Buffer);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                mat = null;
            }
        }

        private void LoadMat(out Mat mat)
        {
            if (IsLoadFromFile)
            {
                LoadFromFile(out mat);
            }
            else
            {
                LoadFromCam(out mat);
            }
        }

        private void ClearMat()
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

        private void DisplayMat(Dictionary<string, Mat> Mats, List<stObject> Results)
        {
            _ResultMat = null;
            ListMat = null;
            MatSelectedItem = null;
            VisionResult = null;

            if (Mats != null)
            {
                _ResultMat = Mats;
                ListMat = new ObservableCollection<string>(Mats.Keys);
                MatSelectedItem = Mats.Count > 0 ? Mats.Last().Key : null;
            }

            if (Results != null)
            {
                VisionResult = new ObservableCollection<stObject>(Results);
            }
        }

        private void SaveDbgImage()
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
    }
}
