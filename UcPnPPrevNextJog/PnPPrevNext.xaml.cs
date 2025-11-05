using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace UcPnPPrevNextJog
{
    using LogModule;
    using System.Threading;
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PnPPrevNext : UserControl
    {
        private Task _ContinuousStepMoveEventTask;
        private bool _IsContinuousCommandEventEnable;
        private Button _SelectedJogBtn;

        public Guid GUID { get; set; }
        public bool Lockable { get; set; } = false;
        public bool InnerLockable { get; set; }
        public List<int> AvoidLockHashCodes { get; set; }

        public PnPPrevNext()
        {
            InitializeComponent();
        }

        private void StartContinuousCommandEvent()
        {
            try
            {
                if (_SelectedJogBtn == null)
                    return;

                _IsContinuousCommandEventEnable = true;
                _ContinuousStepMoveEventTask = Task.Run(() =>
                {
                    const int eventInterval = 100;
                    do
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            //_SelectedJogBtn.Command.Execute(null);
                            _SelectedJogBtn.Command.Execute(_SelectedJogBtn.CommandParameter);
                        }));
                        Thread.Sleep(eventInterval);
                    } while (_IsContinuousCommandEventEnable);
                });
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        //==> 종료! Continuous Step Move Event : 사용자가 손을 때었을때 종료 된다.
        private void ExitContinuousCommandEvent()
        {
            try
            {
                _IsContinuousCommandEventEnable = false;
                if (_ContinuousStepMoveEventTask == null)
                    return;
                _ContinuousStepMoveEventTask.Wait();
                _ContinuousStepMoveEventTask = null;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void JogBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                bool repeatEnable = false;
                if (sender == PadJogLeftUpBtn)
                {
                    _SelectedJogBtn = PadJogLeftUpControl;
                    repeatEnable = PadJogLeftUpBtn.RepeatEnable;
                }
                else if (sender == PadJogRightUpBtn)
                {
                    _SelectedJogBtn = PadJogRightUpControl;
                    repeatEnable = PadJogRightUpBtn.RepeatEnable;
                }
                else if (sender == PadJogLeftDownBtn)
                {
                    _SelectedJogBtn = PadJogLeftDownControl;
                    repeatEnable = PadJogLeftDownBtn.RepeatEnable;
                }
                else if (sender == PadJogRightDownBtn)
                {
                    _SelectedJogBtn = PadJogRightDownControl;
                    repeatEnable = PadJogRightDownBtn.RepeatEnable;
                }

                if (!_SelectedJogBtn.IsEnabled)
                    return;

                if (repeatEnable)
                {
                    ExitContinuousCommandEvent();
                    Mouse.Capture((JogBtn)sender, CaptureMode.SubTree);
                    StartContinuousCommandEvent();
                    return;
                }

                if (_SelectedJogBtn.Command != null)
                {
                    //_SelectedJogBtn.Command.Execute(null);
                    _SelectedJogBtn.Command.Execute(_SelectedJogBtn.CommandParameter);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }

        private void JogBtn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Mouse.Captured == null)
                    return;
                Mouse.Capture(null);
                ExitContinuousCommandEvent();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw err;
            }
        }
    }

}
