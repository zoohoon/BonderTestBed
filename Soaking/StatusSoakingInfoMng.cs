using System;
using System.Threading;
using ProberInterfaces;
using LogModule;
using SoakingParameters;
using ProberInterfaces.Soaking;
using System.Runtime.CompilerServices;
using System.IO;
using SerializerUtil;
using Newtonsoft.Json;

namespace Soaking
{
    /// <summary>
    /// Soaking의 마지막 상태를 기억하고 기록.
    /// </summary>
    public class StatusSoakingInfoMng : IDisposable, ILastSoakingStateInfo, IFactoryModule
    {
        #region Fields
        private readonly object SoakingInfoTraceThreadLockObject = new object();
        private Thread SoakingInfoTraceThread;        
        private System.Threading.Timer SoakingInfoThreadRestartTimer;
        #endregion

        #region Property
        private bool IsStopSoakingInfoTraceThread { get; set; } = true;
        private string SoakingInfoFilePath { get; set; }
        private string SoakingInfoFileName { get; set; }
        private StatusSoakingInfo LastStatusSoakingInfo { get; set; } = new StatusSoakingInfo();
        #endregion

        #region Constructor
        public StatusSoakingInfoMng()
        {
            Init();
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            IsStopSoakingInfoTraceThread = true;

            SoakingInfoThreadRestartTimer?.Dispose();
        }
        #endregion

        #region ILastSoakingStateInfo
        public void TraceLastSoakingStateInfo(bool bStart)
        {
            Action TraceStart = () =>
            {
                IsStopSoakingInfoTraceThread = false;
                StatusSoakingInfoTraceThreadStart();
            };

            if (bStart)
            {
                if (null == SoakingInfoTraceThread)
                {
                    TraceStart();
                }
                else
                {
                    if (!SoakingInfoTraceThread.IsAlive)
                    {
                        TraceStart();
                    }
                }                
            }
            else
            {
                if ((null != SoakingInfoTraceThread) && (SoakingInfoTraceThread.IsAlive))
                {
                    IsStopSoakingInfoTraceThread = true;
                }
            }
        }

        /// <summary>
        /// Set Default Value
        /// </summary>
        public void ResetLastSoakingStateInfo()
        {
            LastStatusSoakingInfo.SoakingState = SoakingStateEnum.PREPARE;
            LastStatusSoakingInfo.ChillingTime = 0;
            LastStatusSoakingInfo.InfoUpdateTime = default;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveLastSoakingStateInfo(SoakingStateEnum state = SoakingStateEnum.UNDEFINED)
        {
            lock (SoakingInfoTraceThreadLockObject)
            {
                UpdateLastSoakingStateInfo(state);
                                
                string fullPath = SoakingInfoFilePath + "\\" + SoakingInfoFileName;

                if (!DirectoryCheck(SoakingInfoFilePath))
                {
                    return;
                }

                SerializeManager.Serialize(fullPath, LastStatusSoakingInfo, serializerType: SerializerType.JSON);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public object LoadLastSoakingStateInfo()
        {
            lock (SoakingInfoTraceThreadLockObject)
            {
                string fullPath = SoakingInfoFilePath + "\\" + SoakingInfoFileName;

                if (!DirectoryCheck(SoakingInfoFilePath))
                {
                    return null;
                }
                
                if (File.Exists(fullPath))
                {
                    try
                    {
                        SerializeManager.Deserialize(fullPath, out var param, deserializerType: SerializerType.JSON);
                        LastStatusSoakingInfo = JsonConvert.DeserializeObject<StatusSoakingInfo>(param.ToString());
                    }
                    catch (Exception err)
                    {
                        LoggerManager.SoakingErrLog($"{err.Message}");
                        return null;
                    }
                }
                else
                {
                    // Set Default Value
                    ResetLastSoakingStateInfo();
                }
            }

            return LastStatusSoakingInfo;
        }
        #endregion

        #region Method
        private void Init()
        {
            // Set Path
            string cellNo = $"C{this.LoaderController().GetChuckIndex():D2}";
            SoakingInfoFilePath = this.FileManager().FileManagerParam.LogRootDirectory + $@"\Backup\{cellNo}";
            SoakingInfoFileName = @"LastStatusSoakingInfo.json";

            SoakingInfoThreadRestartTimer = new Timer(new TimerCallback(StatusSoakingInfoTraceThreadRestart), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        private bool DirectoryCheck(string path)
        {
            bool ret = true;

            try
            {
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception err)
            {
                LoggerManager.SoakingErrLog($"{err.Message}");
                ret = false;
            }

            return ret;
        }        

        private void StatusSoakingInfoTraceThreadRestart(object obj)
        {
            SoakingInfoThreadRestartTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            StatusSoakingInfoTraceThreadStart();
        }
        
        /// <summary>
        /// Chilling Time을 관리하기 위한 Thread
        /// </summary>
        private void StatusSoakingInfoTraceThreadStart()
        {
            SoakingInfoTraceThread = new Thread(StatusSoakingInfoTraceThreadFunc);
            SoakingInfoTraceThread.Start();
        }

        private void StatusSoakingInfoTraceThreadFunc()
        {
            LoggerManager.SoakingLog($"Soaking status monitoring thread start.");
            try
            {
                Func<bool> CheckReady = () =>
                {
                    if (!this.SoakingModule().MonitoringManager().IsMachineInitDone)
                    {
                        return false;
                    }

                    return true;
                };

                DateTime lastUpdateTime = DateTime.Now;
                while (false == IsStopSoakingInfoTraceThread)
                {
                    if (!CheckReady())
                    {
                        Thread.Sleep(3000);
                        continue;
                    }

                    // 매 5초 마다 Status Soaking Info를 File에 저장한다.
                    DateTime currDateTime = DateTime.Now;
                    if ((currDateTime - lastUpdateTime).TotalSeconds < 5)
                    {
                        Thread.Sleep(1000);
                        continue;
                        
                    }

                    lastUpdateTime = currDateTime;
                    SaveLastSoakingStateInfo();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                if (false == IsStopSoakingInfoTraceThread) /// 정상적인 종료가 아닌것으로 판단하여 시간을 두고 재실행 해준다.
                {
                    LoggerManager.SoakingErrLog($"thread abnormal termination.");
                    SoakingInfoThreadRestartTimer.Change(5000, System.Threading.Timeout.Infinite);
                }
                else
                {
                    ResetLastSoakingStateInfo();
                    LoggerManager.SoakingLog($"thread termination.");
                }
            }
        }

        private void UpdateLastSoakingStateInfo(StatusSoakingInfo info = null)
        {
            if (null == info)
            {
                var soakingModule = this.SoakingModule();

                LastStatusSoakingInfo.SoakingState = soakingModule.GetStatusSoakingState();
                LastStatusSoakingInfo.ChillingTime = soakingModule.ChillingTimeMngObj.GetChillingTime() / 1000;
                LastStatusSoakingInfo.InfoUpdateTime = DateTime.Now;
            }
            else
            {
                LastStatusSoakingInfo = info;
            }
        }

        private void UpdateLastSoakingStateInfo(SoakingStateEnum state)
        {
            if ((state == SoakingStateEnum.PREPARE) ||
                (state == SoakingStateEnum.RECOVERY) ||
                (state == SoakingStateEnum.MAINTAIN))
            {
                var soakingModule = this.SoakingModule();

                if (state == SoakingStateEnum.UNDEFINED)
                {
                    LastStatusSoakingInfo.SoakingState = soakingModule.GetStatusSoakingState();
                }
                else
                {
                    LastStatusSoakingInfo.SoakingState = state;
                }

                LastStatusSoakingInfo.ChillingTime = soakingModule.ChillingTimeMngObj.GetChillingTime() / 1000;
                LastStatusSoakingInfo.InfoUpdateTime = DateTime.Now;
            }
            else
            {
                UpdateLastSoakingStateInfo();
            }
        }
        #endregion
    }
}
