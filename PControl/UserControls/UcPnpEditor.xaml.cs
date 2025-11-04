
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PnPControl.UserControls
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows.Controls;
    using System.Xml.Serialization;
    using Autofac;
    using NLog;
    using ParamHelper;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.PnpSetup;

    /// <summary>
    /// Interaction logic for UcPnpEditor.xaml
    /// </summary>
    public partial class UcPnpEditor : UserControl, IMainScreenView
    {
        readonly Guid _PageGUID = new Guid("71EEBCF8-3E48-5F67-9AE2-12E134A62A1D");
        public Guid ViewGUID { get { return _PageGUID; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private static Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public UcPnpEditor()
        {
            InitializeComponent();
        }

        private PNPCommandButtonDescriptorList _Buttons = new PNPCommandButtonDescriptorList();
        public PNPCommandButtonDescriptorList Buttons
        {
            get { return _Buttons; }
            set
            {
                if (value != _Buttons)
                {
                    _Buttons = value;
                    NotifyPropertyChanged("Buttons");
                }
            }
        }

        //private ObservableCollection<PNPCommandButtonDescriptor> _Buttons = new ObservableCollection<PNPCommandButtonDescriptor>();
        //public ObservableCollection<PNPCommandButtonDescriptor> Buttons
        //{
        //    get { return _Buttons; }
        //    set
        //    {
        //        if (value != _Buttons)
        //        {
        //            _Buttons = value;
        //            NotifyPropertyChanged("Buttons");
        //        }
        //    }
        //}

        private PNPCommandButtonDescriptor _Button = new PNPCommandButtonDescriptor();
        public PNPCommandButtonDescriptor Button
        {
            get { return _Button; }
            set
            {
                if (value != _Button)
                {
                    _Button = value;
                    NotifyPropertyChanged("Button");
                }
            }
        }


        private ObservableCollection<IPnpSetupScreen> _PnpSetupScreens = new ObservableCollection<IPnpSetupScreen>();
        [XmlIgnore]
        public ObservableCollection<IPnpSetupScreen> PnpSetupScreens
        {
            get { return _PnpSetupScreens; }
            set
            {
                if (value != _PnpSetupScreens)
                {
                    _PnpSetupScreens = value;
                    NotifyPropertyChanged("PnpSetupScreens");
                }
            }
        }

        private ObservableCollection<string> _AssemblyName = new ObservableCollection<string>();
        [XmlIgnore]
        public ObservableCollection<string> AssemblyName
        {
            get { return _AssemblyName; }
            set
            {
                if (value != _AssemblyName)
                {
                    _AssemblyName = value;
                    NotifyPropertyChanged("AssemblyName");
                }
            }
        }


        private PnpStepInfo _SetupStepInfo = new PnpStepInfo();
        public PnpStepInfo SetupStepInfo
        {
            get { return _SetupStepInfo; }
            set
            {
                if (value != _SetupStepInfo)
                {
                    _SetupStepInfo = value;
                    NotifyPropertyChanged("SetupStepInfo");
                }
            }
        }

        public string ViewModelType => throw new NotImplementedException();

        private IPnpSetupScreen CurrSetupScreen;


        public ErrorCodeEnum InitModule()
        {
            //this.DataContext = this;
            return ErrorCodeEnum.NONE;
        }
        public void DeInitModule()
        {

        }
        public ErrorCodeEnum InitPage(object parameter = null)
        {
            return ErrorCodeEnum.NONE;
        }

        public ErrorCodeEnum LoadPnpEditor()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                foreach (var item in this.ProberStation().Plist)
                {
                    var value = item.GetValue(this.ProberStation(), null);
                    if (value != null)
                    {
                        if (value is IPnpSetupScreen)
                        {
                            PnpSetupScreens.Add((IPnpSetupScreen)value);

                        }
                    }
                }

            }
            catch (Exception err)
            {
                throw err;
            }
            return retVal;
        }


        //..Add as Step3 form Step2(Setup)
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        //..Remove form Step3(Setup)
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        //..Add as Step2 form Step1(Recovery)
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            int seletedindex = lbox_AssemblyList.SelectedIndex;
        }

        //..Remove form Step2(Recovery)
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            int seletedindex = lbox_AssemblyList.SelectedIndex;
        }

        //..Add as Step3 form Step2(Recovery)
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {

        }

        //..Remove form Step3(Recovery)
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {

        }

        //Save
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            SavePnpParam();
        }


        //..PnpSetupsList_SelectionChanged(
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (Mouse.LeftButton != MouseButtonState.Pressed)
                //{
                //    int seletedindex = PnpSetupsList.SelectedIndex;
                //    CurrSetupScreen = PnpSetupScreens[seletedindex];
                //    AssemblyName.Clear();
                //    foreach (var pnpsetup in CurrSetupScreen.PnpSteps)
                //    {
                //        AssemblyName.Add(pnpsetup.GetType().ToString());
                //    }

                //    LoadPnpParam();
                //}
            }
            catch (Exception err)
            {
                Log.Error(err + "PnpSetupsList_SelectionChanged() : Error occured");
            }
        }

        //private void PnpSetupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if(Mouse.LeftButton != MouseButtonState.Pressed)
        //        {
        //            int seletedindex = PnpSetupsList.SelectedIndex;
        //            CurrSetupScreen = PnpSetupScreens[seletedindex];
        //            AssemblyName.Clear();
        //            foreach (var pnpsetup in CurrSetupScreen.PnpSteps)
        //            {
        //                AssemblyName.Add(pnpsetup.GetType().ToString());
        //            }

        //            LoadPnpParam();
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        Log.Error(err + "PnpSetupsList_SelectionChanged() : Error occured");
        //    }

        //}


        private ErrorCodeEnum SavePnpParam()
        {
            ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
            try
            {
                object deserializedObj;



                SetupStepInfo.AssemblyName = CurrSetupScreen.ToString();


                retVal = ParamServices.Serialize(CurrSetupScreen.ParamPath, SetupStepInfo, new Type[] { });

                deserializedObj = ParamServices.Deserialize(CurrSetupScreen.ParamPath, typeof(PnpStepInfo));

            }
            catch (Exception err)
            {
                Log.Error(err + "SavePnpParam() : Error occured");
            }
            return retVal;
        }

        //private ErrorCodeEnum LoadPnpParam()
        //{
        //    ErrorCodeEnum retVal = ErrorCodeEnum.UNDEFINED;
        //    try
        //    {
        //        object deserializedObj;

        //        if (File.Exists(CurrSetupScreen.ParamPath) == false)
        //        {
        //            CurrSetupScreen.DefaultPNPParamSetting();
        //            retVal = ParamServices.Serialize(CurrSetupScreen.ParamPath, SetupStepInfo, new Type[] { });
        //        }

        //        deserializedObj = ParamServices.Deserialize(CurrSetupScreen.ParamPath, typeof(PnpStepInfo));
        //    }
        //    catch (Exception err)
        //    {
        //        Log.Error(err + "LoadPnpParam() : Error occured");
        //    }
        //    return retVal;
        //}

        private void PNPEditor_Loaded(object sender, RoutedEventArgs e)
        {
            object deserializedObj;

            //string filepath;

            //filepath = "C:\\ProberSystem\\Parameters\\RecipeSetup\\SetupButtons.xml";
            PNPCommandButtonDescriptorList pnpbtnDescList = new PNPCommandButtonDescriptorList();
            string FullPath;

            string RootPath = this.FileManager().FileManagerParam.SystemParamRootDirectory;
            string FilePath = pnpbtnDescList.FilePath;
            string FileName = pnpbtnDescList.FileName;

            if (FilePath != "")
            {
                FullPath = RootPath + "\\" + FilePath + "\\" + FileName;
            }
            else
            {
                FullPath = RootPath + "\\" + FileName;
            }

            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(FullPath)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FullPath));
                }

                if (File.Exists(FullPath) == false)
                {
                    ParamServices.Serialize(FullPath, this.Buttons, new Type[] { typeof(PNPCommandButtonDescriptor), typeof(PNPCommandButtonDescriptorList) });
                }
                deserializedObj = ParamServices.Deserialize(
                     FullPath, typeof(PNPCommandButtonDescriptorList),
                     new Type[] { typeof(ObservableCollection<PNPCommandButtonDescriptor>) });

                Buttons = (PNPCommandButtonDescriptorList)deserializedObj;
            }
            catch (Exception err)
            {
                Log.Error(err + "LoadButtonList() : Error occured.");
                throw err;
            }
        }

        //.. Add Button to Recipe Editor 
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            Button = new PNPCommandButtonDescriptor();
        }


        private void PnpEditor_treeview_Expanded(object sender, RoutedEventArgs e)
        {

        }

        ListBox dragSource = null;
        private void PnpSetupsList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            dragSource = parent;
            object data = GetDataFromListBox(dragSource, e.GetPosition(parent));

            if (data != null)
            {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }

            //e.Effects = DragDropEffects.Move;
        }

        #region GetDataFromListBox(ListBox,Point)
        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }

        #endregion



        private void lbox_AssemblyList_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void lbox_AssemblyList_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }


        Point _lastMouseDown;
        TreeViewItem draggedItem, _target;
        private void PnpEditor_treeview_DragOver(object sender, DragEventArgs e)
        {
            try
            {

                //Point currentPosition = e.GetPosition(PnpEditor_treeview);


                //if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                //    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                //{
                //    // Verify that this is a valid drop and then store the drop target
                //    TreeViewItem item = GetNearestContainer(e.OriginalSource as UIElement);
                //    if (CheckDropTarget(draggedItem, item))
                //    {
                //        e.Effects = DragDropEffects.Move;
                //    }
                //    else
                //    {
                //        e.Effects = DragDropEffects.None;
                //    }
                //}
                e.Handled = true;
            }
            catch (Exception err)
            {
                Log.Error(err + "PnpEditor_treeview_DragOver() : Error occured.");
            }
        }

        private void PnpEditor_treeview_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                TreeViewItem TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
            }
            catch (Exception err)
            {
                Log.Error(err + "PnpEditor_treeview_Drop() : Error occured.");
            }
        }

        private void PnpEditor_treeview_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                //if (e.LeftButton == MouseButtonState.Pressed)
                //{
                //    Point currentPosition = e.GetPosition(PnpEditor_treeview);


                //    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                //        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                //    {
                //        draggedItem = (TreeViewItem)PnpEditor_treeview.SelectedItem;
                //        if (draggedItem != null)
                //        {
                //            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(PnpEditor_treeview, PnpEditor_treeview.SelectedValue,
                //                DragDropEffects.Move);
                //            //Checking target is not null and item is dragging(moving)
                //            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
                //            {
                //                // A Move drop was accepted
                //                if (!draggedItem.Header.ToString().Equals(_target.Header.ToString()))
                //                {
                //                    CopyItem(draggedItem, _target);
                //                    _target = null;
                //                    draggedItem = null;
                //                }

                //            }
                //        }
                //    }
                //}
            }
            catch (Exception err)
            {
                Log.Error(err + "PnpEditor_treeview_MouseMove() : Error occured.");
            }
        }

        private void PnpEditor_treeview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ChangedButton == MouseButton.Left)
            //{
            //    _lastMouseDown = e.GetPosition(PnpEditor_treeview);
            //}
        }
        private bool CheckDropTarget(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            //Check whether the target item is meeting your condition
            bool _isEqual = false;
            if (!_sourceItem.Header.ToString().Equals(_targetItem.Header.ToString()))
            {
                _isEqual = true;
            }
            return _isEqual;

        }
        private void CopyItem(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {

            //Asking user wether he want to drop the dragged TreeViewItem here or not
            if (MessageBox.Show("Would you like to drop " + _sourceItem.Header.ToString() + " into " + _targetItem.Header.ToString() + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    //adding dragged TreeViewItem in target TreeViewItem
                    addChild(_sourceItem, _targetItem);

                    //finding Parent TreeViewItem of dragged TreeViewItem 
                    TreeViewItem ParentItem = FindVisualParent<TreeViewItem>(_sourceItem);
                    // if parent is null then remove from TreeView else remove from Parent TreeViewItem
                    if (ParentItem == null)
                    {
                        //PnpEditor_treeview.Items.Remove(_sourceItem);
                    }
                    else
                    {
                        ParentItem.Items.Remove(_sourceItem);
                    }
                }
                catch
                {

                }
            }

        }

        public void addChild(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            // add item in target TreeViewItem 
            TreeViewItem item1 = new TreeViewItem();
            item1.Header = _sourceItem.Header;
            _targetItem.Items.Add(item1);
            foreach (TreeViewItem item in _sourceItem.Items)
            {
                addChild(item, item1);
            }
        }
        static TObject FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                TObject found = parent as TObject;
                if (found != null)
                {
                    return found;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }

            return null;
        }

        private void PnpSetupsList_DragLeave(object sender, DragEventArgs e)
        {

        }

        private TreeViewItem GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem container = element as TreeViewItem;
            try
            {
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return container;
        }

    }



    [Serializable]
    public class PnpStepInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private string _PnpScreenName;
        public string PnpScreenName
        {
            get { return _PnpScreenName; }
            set
            {
                if (value != _PnpScreenName)    
                {
                    _PnpScreenName = value;
                    NotifyPropertyChanged("PnpScreenName");
                }
            }
        }

        private string _ParamPath;
        public string ParamPath
        {
            get { return _ParamPath; }
            set
            {
                if (value != _ParamPath)
                {
                    _ParamPath = value;
                    NotifyPropertyChanged("ParamPath");
                }
            }
        }



        private string _AssemblyName;
        public string AssemblyName
        {
            get { return _AssemblyName; }
            set
            {
                if (value != _AssemblyName)
                {
                    _AssemblyName = value;
                    NotifyPropertyChanged("AssemblyName");
                }
            }
        }


        public ObservableCollection<PnpStepInfo> SetupStepSetps { get; set; }
        public ObservableCollection<PnpStepInfo> RecoverySteps { get; set; }

        public PnpStepInfo()
        {

        }

        public PnpStepInfo(string name)
        {
            this.AssemblyName = name;
        }

        public PnpStepInfo(string name, ObservableCollection<PnpStepInfo> setupstepsetps, ObservableCollection<PnpStepInfo> recoverysetps)
        {
            try
            {
            this.AssemblyName = name;
            this.RecoverySteps = recoverysetps;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }
        public ErrorCodeEnum InitPage(object parameter = null)
        {
            return ErrorCodeEnum.NONE;
        }
    }


}
