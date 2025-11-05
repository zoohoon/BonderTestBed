using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ProberSystem.UserControls
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            try
            {
            MenuBtnVisible_0 = true;
            MenuBtnVisible_1 = true;
            MenuBtnVisible_2 = false;
            MenuBtnVisible_3 = false;
            MenuBtnVisible_4 = false;
            MenuBtnVisible_5 = false;

            InitializeComponent();
           

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                this.lblCurTime.Content = DateTime.Now.ToString(@"yyyy\/MM\/dd HH:mm:ss");
            }, this.Dispatcher);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
        }

        #region DependencyProperties
        // Button Command
        public static DependencyProperty MenuBtnCmd0Property = DependencyProperty.Register("MenuBtnCommand_0", typeof(ICommand), typeof(MenuBar));
        public ICommand MenuBtnCommand_0
        {
            get { return (ICommand)GetValue(MenuBtnCmd0Property); }
            set { SetValue(MenuBtnCmd0Property, value);          }
        }

        public static DependencyProperty MenuBtnCmd1Property = DependencyProperty.Register("MenuBarExBtnCommand_1", typeof(ICommand), typeof(MenuBar));
        public ICommand MenuBtnCommand_1
        {
            get { return (ICommand)GetValue(MenuBtnCmd1Property); }
            set { SetValue(MenuBtnCmd1Property, value); }
        }

        public static DependencyProperty MenuBtnCmd2Property = DependencyProperty.Register("MenuBarExBtnCommand_2", typeof(ICommand), typeof(MenuBar));
        public ICommand MenuBtnCommand_2
        {
            get { return (ICommand)GetValue(MenuBtnCmd2Property); }
            set { SetValue(MenuBtnCmd2Property, value); }
        }

        public static DependencyProperty MenuBtnCmd3Property = DependencyProperty.Register("MenuBarExBtnCommand_3", typeof(ICommand), typeof(MenuBar));
        public ICommand MenuBtnCommand_3
        {
            get { return (ICommand)GetValue(MenuBtnCmd3Property); }
            set { SetValue(MenuBtnCmd3Property, value); }
        }

        public static DependencyProperty MenuBtnCmd4Property = DependencyProperty.Register("MenuBarExBtnCommand_4", typeof(ICommand), typeof(MenuBar));
        public ICommand MenuBtnCommand_4
        {
            get { return (ICommand)GetValue(MenuBtnCmd4Property); }
            set { SetValue(MenuBtnCmd4Property, value); }
        }
        public static DependencyProperty MenuBtnCmd5Property = DependencyProperty.Register("MenuBarExBtnCommand_5", typeof(ICommand), typeof(MenuBar));
        public ICommand MenuBtnCommand_5
        {
            get { return (ICommand)GetValue(MenuBtnCmd5Property); }
            set { SetValue(MenuBtnCmd5Property, value); }
        }

        // Button Name
        public static DependencyProperty MenuBtnName0Property = DependencyProperty.Register("MenuBtnName_0", typeof(string), typeof(MenuBar));
        public string MenuBtnName_0
        {
            get { return (string)GetValue(MenuBtnName0Property); }
            set { SetValue(MenuBtnName0Property, value); }
        }
        public static DependencyProperty MenuBtnName1Property = DependencyProperty.Register("MenuBtnName_1", typeof(string), typeof(MenuBar));
        public string MenuBtnName_1
        {
            get { return (string)GetValue(MenuBtnName1Property); }
            set { SetValue(MenuBtnName1Property, value); }
        }
        public static DependencyProperty MenuBtnName2Property = DependencyProperty.Register("MenuBtnName_2", typeof(string), typeof(MenuBar));
        public string MenuBtnName_2
        {
            get { return (string)GetValue(MenuBtnName2Property); }
            set { SetValue(MenuBtnName2Property, value); }
        }
        public static DependencyProperty MenuBtnName3Property = DependencyProperty.Register("MenuBtnName_3", typeof(string), typeof(MenuBar));
        public string MenuBtnName_3
        {
            get { return (string)GetValue(MenuBtnName3Property); }
            set { SetValue(MenuBtnName3Property, value); }
        }
        public static DependencyProperty MenuBtnName4Property = DependencyProperty.Register("MenuBtnName_4", typeof(string), typeof(MenuBar));
        public string MenuBtnName_4
        {
            get { return (string)GetValue(MenuBtnName4Property); }
            set { SetValue(MenuBtnName4Property, value); }
        }
        public static DependencyProperty MenuBtnName5Property = DependencyProperty.Register("MenuBtnName_5", typeof(string), typeof(MenuBar));
        public string MenuBtnName_5
        {
            get { return (string)GetValue(MenuBtnName5Property); }
            set { SetValue(MenuBtnName5Property, value); }
        }

        // Button Visibility
        public static DependencyProperty MenuBtnVisible0Property = DependencyProperty.Register("MenuBtnVisible_0", typeof(bool), typeof(MenuBar));
        public bool MenuBtnVisible_0
        {
            get { return (bool)GetValue(MenuBtnVisible0Property); }
            set { SetValue(MenuBtnVisible0Property, value); }
        }
        public static DependencyProperty MenuBtnVisible1Property = DependencyProperty.Register("MenuBtnVisible_1", typeof(bool), typeof(MenuBar));
        public bool MenuBtnVisible_1
        {
            get { return (bool)GetValue(MenuBtnVisible1Property); }
            set { SetValue(MenuBtnVisible1Property, value); }
        }
        public static DependencyProperty MenuBtnVisible2Property = DependencyProperty.Register("MenuBtnVisible_2", typeof(bool), typeof(MenuBar));
        public bool MenuBtnVisible_2
        {
            get { return (bool)GetValue(MenuBtnVisible2Property); }
            set { SetValue(MenuBtnVisible2Property, value); }
        }
        public static DependencyProperty MenuBtnVisible3Property = DependencyProperty.Register("MenuBtnVisible_3", typeof(bool), typeof(MenuBar));
        public bool MenuBtnVisible_3
        {
            get { return (bool)GetValue(MenuBtnVisible3Property); }
            set { SetValue(MenuBtnVisible3Property, value); }
        }
        public static DependencyProperty MenuBtnVisible4Property = DependencyProperty.Register("MenuBtnVisible_4", typeof(bool), typeof(MenuBar));
        public bool MenuBtnVisible_4
        {
            get { return (bool)GetValue(MenuBtnVisible4Property); }
            set { SetValue(MenuBtnVisible4Property, value); }
        }
        public static DependencyProperty MenuBtnVisible5Property = DependencyProperty.Register("MenuBtnVisible_5", typeof(bool), typeof(MenuBar));
        public bool MenuBtnVisible_5
        {
            get { return (bool)GetValue(MenuBtnVisible5Property); }
            set { SetValue(MenuBtnVisible5Property, value); }
        }

        #endregion


    }
}
