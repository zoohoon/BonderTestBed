using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace UcCircularShapedJog
{
    using LogModule;
    using System.Diagnostics;
    using System.Threading;
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CircularJog : UserControl
    {
        private Task _ContinuousStepMoveEventTask;
        private bool _IsContinuousCommandEventEnable;
        private Button _SelectedJogBtn;


        public Guid GUID { get; set; }
        public bool Lockable { get; set; } = false;
        public bool InnerLockable { get; set; }
        public List<int> AvoidLockHashCodes { get; set; }

        public CircularJog()
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
                    const int eventInterval = 150;

                    do
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if(_SelectedJogBtn != null)
                            {
                                _SelectedJogBtn.Command.Execute(_SelectedJogBtn.CommandParameter);
                            }
                        }));

                        Stopwatch stopwatch = Stopwatch.StartNew();

                        while (true)
                        {
                            //some other processing to do STILL POSSIBLE
                            if (stopwatch.ElapsedMilliseconds >= eventInterval)
                            {
                                break;
                            }

                            Thread.Sleep(1); //so processor can rest for a while
                        }

                        //Thread.Sleep(eventInterval);

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

                if (sender == PadJogLeftBtn)
                {
                    _SelectedJogBtn = PadJogLeftControl;
                }
                else if (sender == PadJogRightBtn)
                {
                    _SelectedJogBtn = PadJogRightControl;
                }
                else if (sender == PadJogUpBtn)
                {
                    _SelectedJogBtn = PadJogUpControl;
                }
                else if (sender == PadJogDownBtn)
                {
                    _SelectedJogBtn = PadJogDownControl;
                }
                else if (sender == PadJogSelectBtn)
                {
                    _SelectedJogBtn = PadJogSelectControl;
                }

                repeatEnable = ((JogBtn)sender).RepeatEnable;

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
