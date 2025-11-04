using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NavPanelControl
{
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows.Media.Animation;
    using ProberInterfaces.PnpSetup;
    using ProberErrorCode;
    using ProberInterfaces.State;
    using System.IO;
    using LogModule;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using CUIServices;

    /// <summary>
    /// Interaction logic for UserControlNavPanel.xaml
    /// </summary>
    //public partial class NaviControl : UserControl
    //{
    //    public NaviControl()
    //    {
    //        InitializeComponent();
    //    }
    //}

    public partial class NaviControl : UserControl, IFactoryModule, ICUIControl, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public Guid GUID { get; set; }
        private int _MaskingLevel;
        public int MaskingLevel
        {
            get
            {
                _MaskingLevel = CUIService.GetMaskingLevel(this.GUID);
                return _MaskingLevel;
            }
            set
            {
                if (value != _MaskingLevel)
                {
                    _MaskingLevel = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _IsReleaseMode;
        public bool IsReleaseMode
        {
            get { return _IsReleaseMode; }
            set
            {
                if (value != _IsReleaseMode)
                {
                    _IsReleaseMode = value;
                    RaisePropertyChanged();
                }
            }
        }

        private BindingBase _IsEnableBindingBase;
        public BindingBase IsEnableBindingBase
        {
            get { return _IsEnableBindingBase; }
            set
            {
                if (value != _IsEnableBindingBase)
                {
                    _IsEnableBindingBase = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool Lockable { get; set; } = true;
        public bool InnerLockable { get; set; } = false;
        public List<int> AvoidLockHashCodes { get; set; }

        private int _tier;
        public int tier
        {
            get { return _tier; }
            set
            {
                if (value != _tier)
                {
                    _tier = value;
                    RaisePropertyChanged();
                }
            }
        }


        //int tier;
        //private IPnpManager PnpManager
        //{ get { return this.PnpManager(); } }
        //ViewModel vm;
        private static void DatasPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }



        public static readonly DependencyProperty DatasProperty =
                                DependencyProperty.Register("Datas",
                                                            typeof(ObservableCollection<ObservableCollection<ICategoryNodeItem>>),
                                                            typeof(NaviControl),
                                                            new FrameworkPropertyMetadata(new ObservableCollection<ObservableCollection<ICategoryNodeItem>>()
                                                                , new PropertyChangedCallback(DatasPropertyChanged)));
        public ObservableCollection<ObservableCollection<ICategoryNodeItem>> Datas
        {
            get { return (ObservableCollection<ObservableCollection<ICategoryNodeItem>>)this.GetValue(DatasProperty); }
            set { this.SetValue(DatasProperty, value); }
        }

        public static readonly DependencyProperty PnpManagerProperty =
                        DependencyProperty.Register("PnpManager",
                                                    typeof(IPnpManager),
                                                    typeof(NaviControl), null);
        public IPnpManager PnpManager
        {
            get { return (IPnpManager)this.GetValue(PnpManagerProperty); }
            set { this.SetValue(PnpManagerProperty, value); }
        }

        private static async void SelectedItemPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            NaviControl navi = null;
            navi = (NaviControl)sender;
            if (sender != null)
            {

                if (navi.SelectedItem != (ICategoryNodeItem)e.NewValue)
                    await navi.PnpManager.SetViewModel(e.NewValue);

                //navi.SelectedItem =(ICategoryNodeItem)e.NewValue;

            }
        }


        public static readonly DependencyProperty SelectedItemProperty =
                               DependencyProperty.Register("SelectedItem",
                                                           typeof(ICategoryNodeItem),
                                                           typeof(NaviControl), new FrameworkPropertyMetadata(
                                                               null, new PropertyChangedCallback(SelectedItemPropertyChanged)));
        public ICategoryNodeItem SelectedItem
        {
            get { return (ICategoryNodeItem)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }


        private ICategoryNodeItem ListBoxSelectedItem { get; set; }
        private ICategoryNodeItem PreSelectedItem { get; set; }
        private ICategoryNodeItem CategoryFormItem { get; set; }
        private ICategoryNodeItem PreCategoryForm { get; set; }
        private bool Iscategoryback { get; set; } = false;
        /// <summary>
        /// 첫번째 step 인지 알기위해.
        /// </summary>
        private bool Isfirstdefaultstep { get; set; } = true;
        private ListBox CurListBox { get; set; }
        //private ICategoryNodeItem SeleteItem;


        public NaviControl()
        {
            InitializeComponent();
        }

        private async void ItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ListBox listBox = sender as ListBox;

                if (listBox != null && SelectedItem != null)
                {
                    CurListBox = listBox;

                    if (PreSelectedItem != null)
                    {
                        //await this.MetroDialogManager().ShowWaitCancelDialog(this.GetHashCode().ToString(), "Step Switching");
                    }

                    //System.Threading.Thread.Sleep(5);
                    ListBoxSelectedItem = (ICategoryNodeItem)CurListBox.SelectedItem;
                    await UpdateItem(sender);
                    //System.Threading.Thread.Sleep(5);

                    if (Iscategoryback & SelectedItem is IPnpCategoryForm)
                    {
                        Iscategoryback = false;
                    }


                    if (PreSelectedItem != null)
                    {
                        if ((!(SelectedItem is IPnpCategoryForm)) & (PreSelectedItem?.Header != SelectedItem?.Header))
                        {
                            //await this.PnPManager().SetViewModel(SelectedItem, null);
                        }

                    }
                }
            }
            catch (Exception err)
            {

                LoggerManager.Exception(err);
            }
            finally
            {
                if (PreSelectedItem != null)
                {
                    //await this.WaitCancelDialogService().CloseDialog();
                    //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
                }
            }
        }

        private Tuple<ICategoryNodeItem, int> FindRoot(ICategoryNodeItem node, int currindex)
        {
            try
            {
                if (node.Parent != null)
                {
                    currindex++;
                    return FindRoot(node.Parent, currindex);
                }
                else
                {
                    return new Tuple<ICategoryNodeItem, int>(node.Parent, currindex);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }


        private async void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //await node.Cleanup(node);
                ListBox listBox = sender as ListBox;

                try
                {
                    if (sender != null)
                    {
                        Point pt = e.GetPosition(listBox);

                        var dpobj = NavItemsControl.ItemContainerGenerator.ContainerFromIndex(tier);
                        var scrollViwer = GetScrollViewer(dpobj) as ScrollViewer;
                        if (scrollViwer != null)
                        {
                            if (listBox.SelectedItem != null)
                            {
                                for (int index = 0; index < listBox.Items.Count; index++)
                                {
                                    if (listBox.Items[index] == listBox.SelectedItem)
                                    {
                                        double maxy = 47 * (index + 1 + scrollViwer.VerticalOffset);
                                        double miny = maxy - 47;

                                        if (pt.Y > miny && pt.Y < maxy)
                                        {
                                            if (listBox.SelectedItem is IPnpCategoryForm)
                                            {
                                                await EnterCategoryFolder(listBox, (ICategoryNodeItem)listBox.SelectedItem);
                                            }
                                            else
                                            {
                                                await PageSwitched();
                                            }
                                        }


                                        break;
                                    }
                                }
                            }
                        }


                        if (listBox.SelectedItem is ICategoryNodeItem)
                        {
                            //if (!(listBox.SelectedItem is IPnpCategoryForm))
                            //{
                            PreSelectedItem = listBox.SelectedItem as ICategoryNodeItem;
                            //}
                        }
                    }

                }
                catch (Exception err)
                {
                    LoggerManager.Exception(err);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

        }

        /// <summary>
        /// 선택된 Moduld(SelectedItem) 의 PageSwitched 함수 호출
        /// </summary>
        /// <returns></returns>
        private async Task PageSwitched()
        {
            //await Task.Factory.StartNew(async () =>
            //{
            //    await SelectedItem.PageSwitched();
            //});

            //await SelectedItem.PageSwitched();
            await ListBoxSelectedItem.PageSwitched();
        }

        /// <summary>
        /// Folder(CategorForm) 선택되었을때 하위 항목들을 보여준다.    
        /// </summary>
        private async Task EnterCategoryFolder(ListBox listBox, ICategoryNodeItem node, bool isback = false)
        {
            try
            {
                double left;
                int index = 0;
                var result = FindRoot(node, index);
                index = result.Item2;
                tier = index;

                DependencyObject ic = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(listBox));
                if (node.Categories.Count > 0)
                {
                    SelectedItem = node;
                    await this.PnPManager().SetViewModel(SelectedItem, null);
                    PreCategoryForm = node;
                    tier++;
                }
                else
                {
                    if ((node is IPnpCategoryForm))
                        PreCategoryForm = null;
                }

                if (node.Parent != null)
                {
                    CategoryFormItem = node;
                }
                else
                {
                    if (!(node is IPnpCategoryForm))
                        PreCategoryForm = null;
                }

                var currCanvasLeft = Canvas.GetLeft(NavItemsControl);
                left = (listBox.ActualWidth) * -tier;
                if (isback == true)
                {
                    tier--;
                    if (tier < 0) tier = 0;
                    left = (listBox.ActualWidth) * -tier;
                }

                if (double.IsNaN(currCanvasLeft))
                {
                    Canvas.SetLeft(NavItemsControl, 16);
                }

                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation(left, TimeSpan.FromMilliseconds(300));
                da.EasingFunction = new ExponentialEase();
                Storyboard.SetTargetProperty(da, new PropertyPath("(Canvas.Left)"));
                sb.Children.Add(da);
                NavItemsControl.BeginStoryboard(sb);

                ObservableCollection<ICategoryNodeItem> categories = new ObservableCollection<ICategoryNodeItem>();

                if (isback == false)
                {


                    foreach (var category in node.Categories)
                    {
                        categories.Add(category as ICategoryNodeItem);
                    }

                    if (Datas.Count() > tier + 1)
                    {
                        if (node.Categories.Count > 0)
                        {
                            Datas[tier] = categories;
                        }
                    }
                    else
                    {
                        if (Datas.Count() <= tier)
                        {
                            Datas.Add(categories);
                        }
                        else
                        {
                            if (node.Categories.Count > 0)
                            {
                                Datas[tier] = categories;
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        private async Task UpdateItem(object sender = null, bool pre = false)
        {
            try
            {

                if (sender != null)
                {
                    ListBox listBox = sender as ListBox;
                    if (listBox != null)
                    {
                        ICategoryNodeItem node;

                        if (listBox.SelectedItem is ICategoryNodeItem)
                        {
                            node = (ICategoryNodeItem)listBox.SelectedItem;

                            if (PreSelectedItem != null)
                            {
                                EventCodeEnum retval = EventCodeEnum.UNDEFINED;

                                if (PreSelectedItem != ListBoxSelectedItem)
                                {
                                    if (PreSelectedItem != node)
                                    {
                                        bool prestep = false;
                                        int seletedindex = 0;
                                        int preindex = 0;


                                        for (int lbindex = 0; lbindex < listBox.Items.Count; lbindex++)
                                        {


                                            if (listBox.Items[lbindex] == ListBoxSelectedItem)
                                            {
                                                seletedindex = lbindex;
                                            }

                                            if (listBox.Items[lbindex] == PreSelectedItem)
                                            {
                                                preindex = lbindex;
                                            }
                                        }



                                        if (seletedindex <= preindex)
                                        {
                                            prestep = true;
                                            //await Task.Run(async () =>
                                            //{
                                            //    await (PreSelectedItem as IMainScreenViewModel).Cleanup(EventCodeEnum.NONE);
                                            //});
                                            await Application.Current.Dispatcher.Invoke<Task>(() =>
                                            {
                                                return (PreSelectedItem as IMainScreenViewModel).Cleanup(EventCodeEnum.NODATA);
                                            });
                                            retval = EventCodeEnum.NONE;
                                            //retval = await (PreSelectedItem as IMainScreenViewModel).Cleanup(EventCodeEnum.NONE);
                                        }

                                        if (!prestep && !Iscategoryback)
                                        {
                                            if (!(ListBoxSelectedItem as ICategoryNodeItem).NoneCleanUp)
                                            {
                                                //await Task.Run(async () =>
                                                //{
                                                //    retval = await (PreSelectedItem as IMainScreenViewModel).Cleanup(node);
                                                //});
                                                retval = await Application.Current.Dispatcher.Invoke<Task<EventCodeEnum>>(() =>
                                                {
                                                    return (PreSelectedItem as IMainScreenViewModel).Cleanup(node);
                                                });
                                                //retval = await (PreSelectedItem as IMainScreenViewModel).Cleanup(node);
                                            }
                                            else
                                                retval = EventCodeEnum.NONE;
                                        }

                                        if (retval != EventCodeEnum.NONE && !Iscategoryback)
                                        {
                                            if (PreCategoryForm != null)
                                            {
                                                SelectedItem = PreCategoryForm;
                                                listBox.UnselectAll();
                                                listBox.SelectedItem = PreCategoryForm;
                                                await EnterCategoryFolder(listBox, PreCategoryForm, true);
                                                //listBox.SelectedItem = null;
                                            }
                                            else
                                            {
                                                SelectedItem = PreSelectedItem;
                                                listBox.SelectedItem = PreSelectedItem;
                                            }

                                            return;

                                        }
                                        else
                                        {
                                            SelectedItem = ListBoxSelectedItem;
                                        }
                                    }
                                }
                            }

                            if (!pre)
                            {
                                //첫번째 step 이 folder 면 자동으로 내부로 들어가는것을 막기위해.
                                if (!Iscategoryback & !Isfirstdefaultstep)
                                {
                                    await EnterCategoryFolder(listBox, node);
                                }

                                if (Isfirstdefaultstep)
                                    Isfirstdefaultstep = false;

                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            try
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
                {
                    var child = VisualTreeHelper.GetChild(o, i);

                    var result = GetScrollViewer(child);
                    if (result == null)
                    {
                        continue;
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }
        private void OnScrollUp(object sender, RoutedEventArgs e)
        {
            try
            {
                var dpobj = NavItemsControl.ItemContainerGenerator.ContainerFromIndex(tier);
                var scrollViwer = GetScrollViewer(dpobj) as ScrollViewer;

                if (scrollViwer != null)
                {
                    // Logical Scrolling by Item
                    // scrollViwer.LineUp();
                    // Physical Scrolling by Offset
                    scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset - 3);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void OnScrollDown(object sender, RoutedEventArgs e)
        {
            try
            {
                var dpobj = NavItemsControl.ItemContainerGenerator.ContainerFromIndex(tier);

                var scrollViwer = GetScrollViewer(dpobj) as ScrollViewer;

                if (scrollViwer != null)
                {
                    // Logical Scrolling by Item
                    // scrollViwer.LineDown();
                    // Physical Scrolling by Offset
                    scrollViwer.ScrollToVerticalOffset(scrollViwer.VerticalOffset + 3);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private async void PrevTierButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (CurListBox.SelectedItem != null)
                    CurListBox.SelectedItem = null;

                if (CategoryFormItem != null)
                {
                    //await Task.Run(async () =>
                    //{
                    //    await (CategoryFormItem as IMainScreenViewModel).Cleanup(EventCodeEnum.NONE);
                    //});
                    await Application.Current.Dispatcher.Invoke<Task>(() =>
                    {
                        return (CategoryFormItem as IMainScreenViewModel).Cleanup(EventCodeEnum.NODATA);
                    });
                }

                double left;
                DependencyObject ic = VisualTreeHelper.GetParent(NavItemsControl);
                var currCanvasLeft = Canvas.GetLeft(NavItemsControl);
                tier--;
                if (tier < 0) tier = 0;
                left = -(ucwindow.ActualWidth * tier);
                //Canvas.SetLeft(NavItemsControl, left);
                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation(left, TimeSpan.FromMilliseconds(300));
                da.EasingFunction = new ExponentialEase();
                Storyboard.SetTargetProperty(da, new PropertyPath("(Canvas.Left)"));
                sb.Children.Add(da);
                NavItemsControl.BeginStoryboard(sb);

                Iscategoryback = true;
                SelectedItem = PreCategoryForm;
                await this.PnPManager().SetViewModel(SelectedItem, null);
                Iscategoryback = false;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //await this.MetroDialogManager().CloseWaitCancelDialaog(this.GetHashCode().ToString());
            }
        }

        private void Ucwindow_Unloaded(object sender, RoutedEventArgs e)
        {
            NaviControl navi = null;
            navi = (NaviControl)sender;
            if (sender != null)
            {
                if (navi.tier != 0)
                {
                    var currCanvasLeft = Canvas.GetLeft(NavItemsControl);
                    tier--;
                    if (tier < 0) tier = 0;
                    double left = -(ucwindow.ActualWidth * tier);
                    Storyboard sb = new Storyboard();
                    DoubleAnimation da = new DoubleAnimation(left, TimeSpan.FromMilliseconds(300));
                    da.EasingFunction = new ExponentialEase();
                    Storyboard.SetTargetProperty(da, new PropertyPath("(Canvas.Left)"));
                    sb.Children.Add(da);
                    NavItemsControl.BeginStoryboard(sb);
                }

                //await this.PnPManager().PnpCleanup();

                navi.PreCategoryForm = null;
                navi.PreSelectedItem = null;
                navi.Iscategoryback = false;
                navi.Isfirstdefaultstep = true;
            }
        }
    }



    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                if (parameter is bool)
                {
                    bool reverse = (bool)parameter;
                    if (value == null)
                    {
                        return reverse ? Visibility.Visible : Visibility.Hidden;
                    }
                    else
                    {
                        return reverse ? Visibility.Hidden : Visibility.Visible;
                    }
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            else
            {
                if (value == null)
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ParentNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                if (parameter is bool)
                {
                    bool reverse = (bool)parameter;
                    if (value == null)
                    {
                        return reverse ? Visibility.Hidden : Visibility.Visible;
                    }
                    else
                    {
                        return reverse ? Visibility.Visible : Visibility.Hidden;
                    }
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
            else
            {
                if (value == null)
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SeletedParentNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                if (parameter is bool)
                {
                    bool reverse = (bool)parameter;
                    if (value == null)
                    {
                        return reverse ? Visibility.Visible : Visibility.Hidden;
                    }
                    else
                    {
                        return reverse ? Visibility.Hidden : Visibility.Visible;
                    }
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            else
            {
                if (value == null)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Visible;
                }
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(string), typeof(SolidColorBrush))]
    public class HeaderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SetupStateBase)
            {
                SetupStateBase setupstate = value as SetupStateBase;
                switch (setupstate.GetState())
                {
                    case EnumMoudleSetupState.COMPLETE:
                        //return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a0c1a7"));
                        return new SolidColorBrush(Colors.Green);
                    case EnumMoudleSetupState.NOTCOMPLETED:
                        //return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DADCDE"));
                        return new SolidColorBrush(Colors.Red);
                    case EnumMoudleSetupState.VERIFY:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2bca7"));
                    case EnumMoudleSetupState.UNDEFINED:
                        return (Color)ColorConverter.ConvertFromString("#DADCDE");
                }
            }
            return (Color)ColorConverter.ConvertFromString("#DADCDE");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }

    public class PathDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ObservableCollection<ICategoryNodeItem>)
            {
                int count = (value as ObservableCollection<ICategoryNodeItem>).Count;
                if (count != 0)
                {
                    return "M20,18H4V8H20M20,6H12L10,4H4C2.89,4 2,4.89 2,6V18A2,2 0 0,0 4,20H20A2,2 0 0,0 22,18V8C22,6.89 21.1,6 20,6Z";
                }
                else
                {
                    return "F1 M 40.12,5.49L 21.255,40.8L 20.4525,40.8L 0,23.2875L 5.775,15.7875L 19.2525,27.3L 37.695,-1.90735e-006L 45.12,5.49 Z";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }

    public class IconConverter : IMultiValueConverter
    {

        static ImageSource Folder_Complete_backup = ResourceAccessor.Get(Properties.Resources.Folder_Complete_backup);
        static ImageSource Folder_None = ResourceAccessor.Get(Properties.Resources.Folder_None);
        static ImageSource Folder_NotComplete_backup = ResourceAccessor.Get(Properties.Resources.Folder_NotComplete_backup);
        static ImageSource Module_Complete_backup = ResourceAccessor.Get(Properties.Resources.Module_Complete_backup);
        static ImageSource Module_NotComplete_backup = ResourceAccessor.Get(Properties.Resources.Module_NotComplete_backup);
        static ImageSource Module_Notyet_backup = ResourceAccessor.Get(Properties.Resources.Module_Notyet_backup);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int categoriescount = 0;
            SetupStateBase setupstate = null;

            try
            {
                if (values != null)
                {
                    if (values[0] is SetupStateBase)
                    {
                        setupstate = values[0] as SetupStateBase;
                    }

                    if (values[1] is ObservableCollection<ITemplateModule>)
                    {
                        categoriescount = (values[1] as ObservableCollection<ITemplateModule>).Count;
                    }
                }

                if (setupstate != null)
                {
                    if (categoriescount != 0)
                    {
                        //하위 모듈이 있는경우
                        if (setupstate.GetState() == EnumMoudleSetupState.COMPLETE)
                        {
                            return Folder_Complete_backup;
                        }
                        else if (setupstate.GetState() == EnumMoudleSetupState.NONE)
                        {
                            return Folder_None;
                        }
                        else
                        {
                            return Folder_NotComplete_backup;
                        }
                    }
                    else
                    {
                        //하위 모듈이 없는경우
                        if (setupstate.GetState() == EnumMoudleSetupState.COMPLETE)
                        {
                            return Module_Complete_backup;
                        }
                        else if (setupstate.GetState() == EnumMoudleSetupState.NONE)
                        {
                            return null;
                        }
                        else if (setupstate.GetState() == EnumMoudleSetupState.VERIFY)
                        {
                            return Module_Notyet_backup;
                        }
                        else
                        {
                            return Module_NotComplete_backup;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return null;
        }



        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PreButtonVisibilityConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values[0] == null)
                    return Visibility.Hidden;
                if (values[0] is ICategoryNodeItem)
                {
                    ICategoryNodeItem item = values[0] as ICategoryNodeItem;
                    if (values[0] is IPnpCategoryForm)
                    {

                        if ((int)values[1] > 0)
                        {
                            return Visibility.Visible;
                        }
                        else
                            return Visibility.Hidden;
                    }

                    else if (item.Categories.Count != 0)
                        return Visibility.Visible;
                    else if (item.Parent == null)
                        return Visibility.Hidden;
                    else
                        return Visibility.Visible;

                }

            }
            catch (Exception err)
            {
                //LoggerManager.Debug(err);
                LoggerManager.Exception(err);
                LoggerManager.Debug("PreButtonVisibilityConverter.Convert()");
            }

            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    public class BackGroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return (Color)ColorConverter.ConvertFromString("#3C454F");
                else
                    return (Color)ColorConverter.ConvertFromString("#ce5c1c");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return (Color)ColorConverter.ConvertFromString("#3C454F");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    internal static class ResourceAccessor
    {
        public static ImageSource Get(System.Drawing.Bitmap bitmap)
        {
            BitmapImage image = new BitmapImage();

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    (bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    image.BeginInit();
                    ms.Seek(0, SeekOrigin.Begin);
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.StreamSource = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return image;
        }
    }

}
