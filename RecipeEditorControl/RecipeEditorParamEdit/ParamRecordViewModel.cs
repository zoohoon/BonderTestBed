using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeEditorControl.RecipeEditorParamEdit
{
    using System.ComponentModel;
    using RelayCommandBase;
    using System.Windows.Input;
    using System.Windows;
    using RecipeEditorControl.RecipeEditorUC;
    using System.Windows.Media;
    using System.Runtime.CompilerServices;
    using ProberInterfaces;
    using LogModule;
    using MetroDialogInterfaces;
    using VirtualKeyboardControl;
    using MahApps.Metro.Controls.Dialogs;
    using SerializerUtil;
    using ProberInterfaces.ParamUtil;

    public class ParamRecordViewModel :  INotifyPropertyChanged, IFactoryModule
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> ElementID
        private int _ElementID;
        public int ElementID
        {
            get { return _ElementID; }
            set
            {
                if (value != _ElementID)
                {
                    _ElementID = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> CategoryID
        private String _CategoryID;
        public String CategoryID
        {
            get { return _CategoryID; }
            set
            {
                if (value != _CategoryID)
                {
                    _CategoryID = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Name
        private String _Name;
        public String Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Description
        private String _Description;
        public String Description
        {
            get { return _Description; }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MinMax
        private String _MinMax;
        public String MinMax
        {
            get { return _MinMax; }
            set
            {
                if (value != _MinMax)
                {
                    _MinMax = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion       

        #region ==> CategoryIDList
        private List<int> _CategoryIDList;
        public List<int> CategoryIDList
        {
            get { return _CategoryIDList; }
            set { _CategoryIDList = value; }
        }

        #endregion    

        #region ==> RecordVisibility
        private Visibility _RecordVisibility;
        public Visibility RecordVisibility
        {
            get { return _RecordVisibility; }
            set
            {
                if (value != _RecordVisibility)
                {
                    _RecordVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> EditorButton
        private RecipeEditorUCViewModel _EditorButton;
        public RecipeEditorUCViewModel EditorButton
        {
            get { return _EditorButton; }
            set
            {
                if (value != _EditorButton)
                {
                    _EditorButton = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> RecordColor
        private Brush _RecordColor;
        public Brush RecordColor
        {
            get { return _RecordColor; }
            set
            {
                if (value != _RecordColor)
                {
                    _RecordColor = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        private CustomDialog _AdvanceSetupView;

        public CustomDialog AdvanceSetupView
        {
            get { return _AdvanceSetupView; }
            set
            {
                _AdvanceSetupView = value;
            }
        }

        private IPnpAdvanceSetupViewModel _AdvanceSetupViewModel;

        public IPnpAdvanceSetupViewModel AdvanceSetupViewModel
        {
            get { return _AdvanceSetupViewModel; }
            set { _AdvanceSetupViewModel = value; }
        }

        private List<byte[]> _PackagableParams
            = new List<byte[]>();

        public List<byte[]> PackagableParams
        {
            get { return _PackagableParams; }
            set { _PackagableParams = value; }
        }

        #region ==> DescriptionCommand
        private AsyncCommand _DescriptionCommand;
        public ICommand DescriptionCommand
        {
            get
            {
                if (null == _DescriptionCommand) _DescriptionCommand = new AsyncCommand(DescriptionCommandFunc);
                return _DescriptionCommand;
            }
        }
        private Task DescriptionCommandFunc()
        {

            try
            {

                this.MetroDialogManager().ShowMessageDialog("Description", Description, EnumMessageStyle.Affirmative);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region ==> MinMaxAdvanceCommand
        private AsyncCommand<object> _MinMaxAdvanceCommand;
        public ICommand MinMaxAdvanceCommand
        {
            get
            {
                if (null == _MinMaxAdvanceCommand) _MinMaxAdvanceCommand = new AsyncCommand<object>(MinMaxAdvanceCommandFunc);
                return _MinMaxAdvanceCommand;
            }
        }
        private async Task MinMaxAdvanceCommandFunc(object cmdParam)
        {

            try
            {
                if(cmdParam is RecipeEditorUCViewModel)
                {
                    if (this.Elem.WriteMaskingLevel < AccountModule.AccountManager.CurrentUserInfo.UserLevel)
                        return;
                    else if (Elem.IsNumericType() == false &&  Elem.IsFloatingType() == false)
                        return;
                    


                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var editorParm = cmdParam as RecipeEditorUCViewModel;

                        AdvanceSetupView = new ElemMinMaxAdvancedSettingView();
                        AdvanceSetupViewModel = new ElemMinMaxAdvancedSettingViewModel(AdvanceSetupView, editorParm, this);
                        
                        AdvanceSetupView.DataContext = AdvanceSetupViewModel;
                        
                    });


                    if (AdvanceSetupView == null)
                        return;

                    //SetPackagableParams();

                    //AdvanceSetupViewModel.SetParameters(PackagableParams);

                    IPnpAdvanceSetupViewModel vm = AdvanceSetupViewModel as IPnpAdvanceSetupViewModel;

                    vm?.Init();

                    //await this.MetroDialogManager().ShowWindow(AdvanceSetupView, (IMetroDialogViewModel)AdvanceSetupViewModel, PackagableParams, true);
                    //await this.MetroDialogManager().ShowWindow(AdvanceSetupView);
                    await this.MetroDialogManager().ShowAdvancedDialog(AdvanceSetupView, PackagableParams, false);
                    //await this.MetroDialogManager().ShowAdvancedDialog(new ElemMinMaxAdvancedSettingViewModel(editorParm), null, false);                        
                    //});
                    


                }                
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #endregion

        public IElement Elem { get; set; }
    }
}
