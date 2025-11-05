namespace RecipeEditorControl.RecipeEditorUC
{
    using Autofac;
    using LoaderBase.Communication;
    using LogModule;
    using MahApps.Metro.Controls.Dialogs;
    using ProberInterfaces;
    using ProberInterfaces.Proxies;
    using RecipeEditorControl.RecipeEditorParamEdit;
    using RelayCommandBase;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using VirtualKeyboardControl;

    public class RecipeEditorCmdButtonViewModel : RecipeEditorUCViewModel
    {
        #region ==> BtnCommand
        public ICommand BtnCommand { get; set; }
        #endregion

        public RecipeEditorCmdButtonViewModel(IElement elem, ICommand btnCommand) 
            : base(elem)
        {
            BtnCommand = btnCommand;
        }
    }


    public class ElemMinMaxAdvancedSettingViewModel : INotifyPropertyChanged, IPnpAdvanceSetupViewModel, IFactoryModule, IElemMinMaxAdvanceSetupViewModel
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion



        private IElement _Elem;

        public IElement Elem
        {
            get { return _Elem; }
            set
            {
                _Elem = value;
                RaisePropertyChanged();
            }
        }


        public CustomDialog View { get; set; }

        public RecipeEditorUCViewModel editorButton { get; set; }
        public ParamRecordViewModel Record { get; set; }

        public ElemMinMaxAdvancedSettingViewModel(CustomDialog view, RecipeEditorUCViewModel editor, ParamRecordViewModel record)
        {
            editorButton = editor;
            Elem = editor.Elem;
            View = view;
            Min = Elem.LowerLimit.ToString();
            Max = Elem.UpperLimit.ToString();
            Record = record;
        }

        #region ==> Min
        private string _Min;
        public string Min
        {
            get { return _Min; }
            set
            {
                if (value != _Min)
                {
                    _Min = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> Max
        private string _Max;
        public string Max
        {
            get { return _Max; }
            set
            {
                if (value != _Max)
                {
                    _Max = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion
        private RelayCommand<Object> _MinTextBoxClickCommand;
        public ICommand MinTextBoxClickCommand
        {
            get
            {
                if (null == _MinTextBoxClickCommand) _MinTextBoxClickCommand = new RelayCommand<Object>(MinTextBoxClickCommandFunc);
                return _MinTextBoxClickCommand;
            }
        }


        private void MinTextBoxClickCommandFunc(Object param) //v22_merge TODO: 이부분 UserLevel 필요.
        {
            try
            {

                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                editorButton.SetElemLimitValueBuffer(tb.Text);
                if (editorButton.FlushMinValueBuffer())
                {
                    //TODO:현재 위치가 로더여서 셀쪽으로 접근해야함. 현재 RecipeEditor는 Cell만 지원하고 있으므로 셀로 접근.

                    Task task = new Task(() =>
                    {
                        if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                        {
                            var proxy = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>().GetProxy<IParamManagerProxy>();
                            proxy.UpdateLowerLimit(Elem.PropertyPath, Elem.LowerLimit.ToString());

                        }
                        else
                        {
                            this.ParamManager().UpdateLowerLimit(Elem.PropertyPath, Elem.LowerLimit.ToString());
                        }
                        Min = Elem.LowerLimit.ToString();
                    });
                    task.Start();
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private RelayCommand<Object> _MaxTextBoxClickCommand;
        public ICommand MaxTextBoxClickCommand
        {
            get
            {
                if (null == _MaxTextBoxClickCommand) _MaxTextBoxClickCommand = new RelayCommand<Object>(MaxTextBoxClickCommandFunc);
                return _MaxTextBoxClickCommand;
            }
        }


        private void MaxTextBoxClickCommandFunc(Object param)
        {
            try
            {

                System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                editorButton.SetElemLimitValueBuffer(tb.Text);
                if (editorButton.FlushMaxValueBuffer())
                {
                    Task task = new Task(() =>
                    {
                        if (AppDomain.CurrentDomain.FriendlyName == "LoaderSystem.exe")
                        {
                            var proxy = this.GetLoaderContainer().Resolve<ILoaderCommunicationManager>().GetProxy<IParamManagerProxy>();
                            proxy.UpdateUpperLimit(Elem.PropertyPath, Elem.UpperLimit.ToString());

                        }
                        else
                        {
                            this.ParamManager().UpdateUpperLimit(Elem.PropertyPath, Elem.UpperLimit.ToString());
                        }
                        Min = Elem.LowerLimit.ToString();
                    });
                    task.Start();
                }
            
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region ==> ExitCommand
        private AsyncCommand _ExitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (null == _ExitCommand) _ExitCommand = new AsyncCommand(ExitCommandFunc);
                return _ExitCommand;
            }
        }
        private async Task ExitCommandFunc()
        {           
            try
            {
                Record.MinMax = $"{Elem.LowerLimit}/{Elem.UpperLimit}";
                await this.MetroDialogManager().CloseWindow(View, View.GetType().Name);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public List<byte[]> GetParameters()
        {
            List<byte[]> parameters = new List<byte[]>();
            try
            {
                //if (editorButton != null)
                //    parameters.Add(SerializeManager.SerializeToByte(EditorButton));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return parameters;
        }

        public void SetParameters(List<byte[]> datas = null)
        {
            try
            {
                //if (datas != null)
                //{
                //    foreach (var param in datas)
                //    {
                //        object target;
                //        SerializeManager.DeserializeFromByte(param, out target, typeof(RecipeEditorUCViewModel));
                //        if (target != null)
                //        {
                //            EditorButton = target as IElement;
                //            break;
                //        }
                //    }
                //}
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Init()
        {
            return;
        }
        #endregion
    }

}
