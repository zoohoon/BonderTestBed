using System;
using System.Windows.Controls;

namespace WizardEditorView.EditorUserControls
{
    using RelayCommandBase;
    using System.ComponentModel;
    using System.Collections.ObjectModel;
    /// <summary>
    /// Interaction logic for WizardCategoryEditor.xaml
    /// </summary>
    public partial class WizardCategoryEditor : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public WizardCategoryEditor()
        {
            InitializeComponent();
        }

        #region //..Properties

        private string _NewCategoryName;
        public string NewCategoryName
        {
            get { return _NewCategoryName; }
            set
            {
                if (value != _NewCategoryName) 
                {
                    _NewCategoryName = value;
                    NotifyPropertyChanged("NewCategoryName");
                }
            }
        }

        private ObservableCollection<string> _CateogySteps;
        public ObservableCollection<string> CateogySteps
        {
            get { return _CateogySteps; }
            set
            {
                if (value != _CateogySteps)
                {
                    _CateogySteps = value;
                    NotifyPropertyChanged("CateogySteps");
                }
            }
        }

        private ObservableCollection<string> _CateogyStepAllItems;
        public ObservableCollection<string> CateogyStepAllItems
        {
            get { return _CateogyStepAllItems; }
            set
            {
                if (value != _CateogyStepAllItems)
                {
                    _CateogyStepAllItems = value;
                    NotifyPropertyChanged("CateogyStepAllItems");
                }
            }
        }




        #endregion

        #region //..Commands
        private RelayCommand<object> _CategoryChangedCommand;
        public RelayCommand<object> CategoryChangedCommand
        {
            get
            {
                if (null == _CategoryChangedCommand) _CategoryChangedCommand = new RelayCommand<object>(CategoryChanged);
                return _CategoryChangedCommand;
            }
        }
        private void CategoryChanged(object param)
        {

        }

        private RelayCommand _CategoryDeleteCommand;
        public RelayCommand CategoryDeleteCommand
        {
            get
            {
                if (null == _CategoryDeleteCommand) _CategoryDeleteCommand = new RelayCommand(CategoryDelete);
                return _CategoryDeleteCommand;
            }
        }
        private void CategoryDelete()
        {

        }


        private RelayCommand _CategoryAddCommand;
        public RelayCommand CategoryAddCommand
        {
            get
            {
                if (null == _CategoryAddCommand) _CategoryAddCommand = new RelayCommand(CategoryAdd);
                return _CategoryAddCommand;
            }
        }
        private void CategoryAdd()
        {

        }

        private RelayCommand _CategoryOrderUpCommand;
        public RelayCommand CategoryOrderUpCommand
        {
            get
            {
                if (null == _CategoryOrderUpCommand) _CategoryOrderUpCommand = new RelayCommand(CategoryOrderUp);
                return _CategoryOrderUpCommand;
            }
        }
        private void CategoryOrderUp()
        {

        }

        private RelayCommand _CategoryOrderDownCommand;
        public RelayCommand CategoryOrderDownCommand
        {
            get
            {
                if (null == _CategoryOrderDownCommand) _CategoryOrderDownCommand = new RelayCommand(CategoryOrderDown);
                return _CategoryOrderDownCommand;
            }
        }
        private void CategoryOrderDown()
        {

        }

        private RelayCommand _CategoryStepAddCommand;
        public RelayCommand CategoryStepAddCommand
        {
            get
            {
                if (null == _CategoryStepAddCommand) _CategoryStepAddCommand = new RelayCommand(CategoryStepAdd);
                return _CategoryStepAddCommand;
            }
        }
        private void CategoryStepAdd()
        {

        }

        private RelayCommand _CategoryStepDeleteCommand;
        public RelayCommand CategoryStepDeleteCommand
        {
            get
            {
                if (null == _CategoryStepDeleteCommand) _CategoryStepDeleteCommand = new RelayCommand(CategoryStepDelete);
                return _CategoryStepDeleteCommand;
            }
        }
        private void CategoryStepDelete()
        {

        }

        private RelayCommand _CategoryStepOrderUpCommand;
        public RelayCommand CategoryStepOrderUpCommand
        {
            get
            {
                if (null == _CategoryStepOrderUpCommand) _CategoryStepOrderUpCommand = new RelayCommand(CategoryStepOrderUp);
                return _CategoryStepOrderUpCommand;
            }
        }
        private void CategoryStepOrderUp()
        {

        }

        private RelayCommand _CategoryStepOrderDownCommand;
        public RelayCommand CategoryStepOrderDownCommand
        {
            get
            {
                if (null == _CategoryStepOrderDownCommand) _CategoryStepOrderDownCommand = new RelayCommand(CategoryStepOrderDown);
                return _CategoryStepOrderDownCommand;
            }
        }
        private void CategoryStepOrderDown()
        {

        }


        #endregion
    }
}
