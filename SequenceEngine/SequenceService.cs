using LogModule;
using System;
using System.ComponentModel;
using ProberInterfaces;
using System.Threading;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SequenceService
{
    public abstract class SequenceServiceBase : ISequenceEngineService, IDisposable
    {
        #region ==> PropertyChanged
        public PropertyChangedEventHandler propertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public event PropertyChangedEventHandler PropertyChanged
        //{
        //    add { this.propertyChanged += value; }
        //    remove { this.propertyChanged -= value; }
        //}
        #endregion

        private bool IsNotRunningSequence { get; set; } = false;
        Thread SequencerThread;

        private bool _PauseSeq;
        public bool PauseSeq
        {
            get
            {
                return _PauseSeq;
            }
            private set
            {
                if (_PauseSeq != value)
                {
                    _PauseSeq = value;
                    RaisePropertyChanged();
                }
            }
        }

        public abstract ModuleStateEnum SequenceRun();
        public void StopSequencer()
        {
            try
            {
                IsNotRunningSequence = true;

                if (SequencerThread?.IsAlive == true)
                    SequencerThread?.Join();


                Status = EnumEngineStatus.UNDEFINED;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void RunSequencer()
        {
            try
            {
                if (Status != EnumEngineStatus.RUNNING)
                {
                    IsNotRunningSequence = false;
                    PauseSeq = false;

                    SequencerThread = new Thread(new ThreadStart(SequenceJob));
                    SequencerThread.Name = this.ThreadName;
                    //SequencerThread.Priority = ThreadPriority.AboveNormal;
                    SequencerThread.Start();

                    Status = EnumEngineStatus.RUNNING;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void PauseSequencer(object caller)
        {
            try
            {
                PauseSeq = true;
                //LoggerManager.Debug(string.Format("PauseSequencer(): Sequencer paused. Caller = {0}, GUID = {1}", caller.ToString(), caller.GetHashCode()));

                Status = EnumEngineStatus.PAUSED;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void ResumeSequencer()
        {
            try
            {
                if (PauseSeq == true)
                {
                    PauseSeq = false;

                    //LoggerManager.Debug(string.Format("ResumeSequencer(): Resume sequencer."));
                }
                else
                {
                    LoggerManager.Error($"ResumeSequencer(): Sequencer is not in paused state.");
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void FlushQueue()
        {
            throw new NotImplementedException();
        }

        public void ForcedSuspendSequence()
        {
            //if (Service != null)
            //{
            //    Service.Pause();
            //}
        }

        public int WaitForSequencer(long timeout = 0)
        {
            Stopwatch stw = new Stopwatch();
            bool timeOuted = false;
            int retVal = -1;

            try
            {
                stw.Start();

                while (Status != EnumEngineStatus.RUNNING & timeOuted == false)
                {
                    if (timeout > 0)
                    {
                        if (stw.ElapsedMilliseconds > timeout)
                        {
                            timeOuted = true;
                        }
                    }
                }

                stw.Stop();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return retVal;
        }

        public SequenceServiceBase()
        {
        }
        ~SequenceServiceBase()
        {
            try
            {

                if (SequencerThread != null)
                {
                    if (SequencerThread.IsAlive == true)
                    {
                        StopSequencer();
                        //SequencerThread.Join();
                    }
                    SequencerThread = null;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void SequenceJob()
        {
            try
            {
                while (IsNotRunningSequence == false)
                {
                    SequenceRun();
                    //minskim// GC 호출 및 CPU 사용률 절감을 위해 기존 timer+resetevent로 thread 제어하던 로직을 제거 하고 sleep으로 대체함, sleep시간은 기존 timer interval 주기 값으로 설정함
                    Thread.Sleep(8);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void Dispose()
        {
            IsNotRunningSequence = false;
        }

        private EnumEngineStatus _Status;
        public EnumEngineStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ThreadName { get; set; }
    }
}
