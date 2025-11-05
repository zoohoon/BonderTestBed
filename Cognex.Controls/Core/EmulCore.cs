using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cognex.Controls.Core
{
    using Cognex.Command;
    using LoaderBase.AttachModules.ModuleInterfaces;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Windows.Media.Imaging;

    /*
     * Emul, 실제적인 구현은 CognexCore에 존재한다.
     */
    public class EmulCore : ICognexProcessManager
    {
        public CognexCommandManager CognexCommandManager { get; set; }
        public CognexProcessSysParameter CognexProcSysParam { get; set; }
        public CognexProcessDevParameter CognexProcDevParam { get; set; }
        public List<string> Ocr { get; set; }
        public List<string> LastOcrResult { get; set; }
        public List<double> OcrScore { get; set; }
        public List<EnumCognexModuleState> HostRunning { get; set; }

        public void Async_SetConfigLightPower(string host, string lightPower, Func<bool> func)
        {
            func();
            return;
        }

        public Task<bool> ConnectDisplay(string host)
        {
            return Task.FromResult<bool>(true);
        }

        public Task<bool> ConnectNative(string host)
        {
            return Task.FromResult<bool>(true);
        }

        public void DisconnectDisplay()
        {
        }

        public void DisconnectNative()
        {
        }

        public void Dispose()
        {
        }

        public bool DO_AcquireConfig(string host)
        {
            return true;
        }

        public bool DO_GetConfigEx(string host)
        {
            return true;
        }

        public bool DO_GetConfigTune(string host)
        {
            return true;
        }

        public bool DO_GetFilterOperationList(string host)
        {
            return true;
        }

        public bool DO_InsertFilterOperation(string host, params string[] args)
        {
            return true;
        }

        public bool DO_ReadConfig(string host, string flag)
        {
            return true;
        }

        public bool DO_RemoveFilterOperationAll(string host)
        {
            return true;
        }
        private bool[] ManualOCRState = new bool[3];
        public void SetManualOCRState(int hostIdx,bool isSuccess)
        {
            ManualOCRState[hostIdx] = isSuccess;
        }
        public bool GetManualOCRState(int hostIdx)
        {
            return ManualOCRState[hostIdx];
        }

        public bool DO_RI(string host)
        {
            return true;
            /*
            if (CognexCommandManager.CognexRICommand.Status == "1")
                return true;
            try
            {

                BitmapImage bitmapImage = new BitmapImage(new Uri("pack://application:,,,/ImageResourcePack;component/Images/OcrImage.bmp"));
                Bitmap bitmap = null;
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bm = new System.Drawing.Bitmap(outStream);
                    bitmap = new Bitmap(bm);
                }

                Image img = bitmap;
                ImageConverter converter = new ImageConverter();
                byte[] buffer = (byte[])converter.ConvertTo(img, typeof(byte[]));

                StringBuilder hex = new StringBuilder(buffer.Length * 2);
                foreach (byte b in buffer)
                    hex.AppendFormat("{0:x2}", b);

                CognexCommandManager.CognexRICommand.Data = hex.ToString();
                CognexCommandManager.CognexRICommand.Size = buffer.Length.ToString();
                CognexCommandManager.CognexRICommand.ByteData = buffer;
                CognexCommandManager.CognexRICommand.Status = "1";
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return true;
            */
        }

        public bool DO_SetConfigCharSize(string host, string height, string width)
        {
            return true;
        }

        public bool DO_SetConfigChecksum(string host, string checksum)
        {
            return true;
        }
        public bool DO_SetConfigRetry(string host, string mode)
        {
            return true;
        }
        public bool DO_SetConfigFieldString(string host, string fieldString)
        {
            return true;
        }

        public bool DO_SetConfigLightMode(string host, string mode)
        {
            return true;
        }

        public bool DO_SetConfigLightPower(string host, string lightPower)
        {
            return true;
        }

        public bool DO_SetConfigMark(string host, string mark)
        {
            return true;
        }

        public bool DO_SetConfigOrientation(string host, string orientation)
        {
            return true;
        }

        public bool DO_SetConfigRegion(string host, string y, string x, string height, string width, string theta, string phi)
        {
            return true;
        }

        public bool DO_SetCustomFilterName(string host, string customFilterName)
        {
            return true;
        }

        public bool DO_TuneConfigEx(string host, string flag, bool filter, bool light, bool size)
        {
            return true;
        }

        public String GetOCRString(string host, bool adjust, out String score)
        {
            score = "100";
            return "EMUL_OCR_STRING";
        }

        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore)
        {
            ocr = "EMUL_OCR_STRING";
            ocrScore = 100;
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore,OCRDevParameter ocrDev)
        {
            ocr = "EMUL_OCR_STRING";
            ocrScore = 100;
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum GetOCRString(int hostIndex, bool adjust, out string ocr, out double ocrScore, OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo)
        {
            ocr = "EMUL_OCR_STRING";
            ocrScore = 100;
            return EventCodeEnum.NONE;
        }

        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust)
        {
            return EventCodeEnum.NONE;
        }
        public Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust,OCRDevParameter ocrDev)
        {
            return EventCodeEnum.NONE;
        }
        public EventCodeEnum DoOCRStringCatch(int hostIndex, bool adjust, OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo)
        {
            if (hostIndex < 0)
            {
                return EventCodeEnum.COGNEX_OUT_OF_MODULE_INDEX;
            }
            if (HostRunning[hostIndex] == EnumCognexModuleState.RUNNING)
            {
                return EventCodeEnum.NONE;
            }

            //==> !!! OCR Read 읽음 완료 상태를 외부에서 바꿔 주어야 한다.
            HostRunning[hostIndex] = EnumCognexModuleState.RUNNING;
            EventCodeEnum result = EventCodeEnum.UNDEFINED;
            try
            {
                String ocr;
                double ocrScore;
                //result = GetOCRString(hostIndex, adjust, out ocr, out ocrScore, ocrDev, activeLotInfo);
                Thread.Sleep(3000);

                ocr = activeLotInfo.LotID + $"-{DateTime.Now.ToString("yyyyMMdd-hhmmss")}";
                ocrScore = 390;

                result = EventCodeEnum.NONE;
                //==> OCR 결과 저장
                Ocr[hostIndex] = ocr;
                OcrScore[hostIndex] = ocrScore;
                LoggerManager.Debug($"[DoOCR String Catch] hostIndex: {hostIndex}, adjust: {adjust} , OCR String: {ocr} , OCR Score:{ocrScore}");
            }
            catch (Exception ex)
            {
                LoggerManager.Error($"[COGNEX OCR] : {ex}");
            }
            finally
            {
                //HostRunning[hostIndex] = EnumCognexModuleState.READ_OCR;
            }

            return result;
        }

        public Task<EventCodeEnum> DoOCRStringCatchAsync(int hostIndex, bool adjust, OCRDevParameter ocrDev)
        {
            return Task.FromResult<EventCodeEnum>(EventCodeEnum.NONE);
        }

        public EventCodeEnum WaitForOCR(int hostIndex, int timeOut)
        {
            //==> OCR을 읽을 때 까지 대기 한다.
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            Stopwatch elapsedStopWatch = new Stopwatch();
            elapsedStopWatch.Reset();
            elapsedStopWatch.Start();
            try
            {
                bool runFlag = true;
                System.Threading.Thread.Sleep(100);
                do
                {
                    if (HostRunning[hostIndex] == EnumCognexModuleState.READ_OCR)
                    {
                        //==> OCR을 읽었다.
                        retVal = EventCodeEnum.NONE;
                        runFlag = false;
                    }
                    else if (HostRunning[hostIndex] == EnumCognexModuleState.FAIL)
                    {
                        //==> OCR을 못 읽었다.
                        retVal = EventCodeEnum.UNDEFINED;
                        runFlag = false;
                    }
                    else if (HostRunning[hostIndex] == EnumCognexModuleState.ABORT)
                    {

                        retVal = EventCodeEnum.LOADER_FIND_NOTCH_FAIL;
                        runFlag = false;
                    }
                    else
                    {
                        if (elapsedStopWatch.ElapsedMilliseconds > timeOut)
                        {
                            retVal = EventCodeEnum.UNDEFINED;
                            runFlag = false;
                        }
                        else
                        {
                            runFlag = true;
                        }
                    }
                    System.Threading.Thread.Sleep(10);
                } while (runFlag == true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            finally
            {
                //==> OCR을 다 읽었으면 finally 가 실행 된다, OCR 상태를 IDLE로 만들어 다시 OCR을 읽을 수 있도록 상태를 초기화 한다.
                HostRunning[hostIndex] = EnumCognexModuleState.IDLE;
                elapsedStopWatch?.Stop();
            }
         

            return retVal;
        }
        public bool InitWindow(IntPtr clientWindowHandle)
        {
            return true;
        }

        public bool IsInit()
        {
            return true;
        }

        public void OnSizeChanged(EventArgs e, int clientWindowWidth, int clientWindowHeight)
        {
            return;
        }

        public bool SaveOCRImage(string host)
        {
            return true;
            /*
            try
            {
                DO_RI(host);
                BitmapImage image = CognexCommandManager.CognexRICommand.GetBitmapImage();
                return LoggerManager.CognexLoggerCtl.SaveImage(image);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            */
        }

        public void SwitchGraphics(bool flag)
        {
            return;
        }

        public void SwitchLiveMode()
        {
            return;
        }

        public void SwitchOnlineMode()
        {
            return;
        }

        public void SwitchSpreadSwitch()
        {
            return;
        }
        public void WndProc(int msg, IntPtr lParam)
        {
            return;
        }
        public void SaveConfig()
        {
            return;
        }
        public void LoadConfig()
        {
            return;
        }

        public bool SaveOCRImage(int hostIndex)
        {
            return false;
        }

        public string CalOcrChecksum(string ocrString)
        {
            return "";
        }

        public int GetCheckDotMat()
        {
            return -1;
        }
        public bool SEMIChecksum(String ocr)
        {
            if (ocr.Length <= 0)
            {
                return true;
            }
            char[] inputCheckSum = new char[2];
            bool result = false;
            try
            {
                if (ocr.Length > 2)
                {

                    inputCheckSum[0] = ocr[ocr.Length - 2];
                    inputCheckSum[1] = ocr[ocr.Length - 1];

                    StringBuilder stb = new StringBuilder(ocr);
                    stb[ocr.Length - 2] = 'A';
                    stb[ocr.Length - 1] = '0';
                    String strImsiOcrBuf = stb.ToString();

                    int[] chrtmp = new int[18];

                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        chrtmp[i] = strImsiOcrBuf[i];
                    }

                    int seed = 0;

                    for (int i = 0; i < strImsiOcrBuf.Length; i++)
                    {
                        int j = (seed * 8) % 59;

                        j = j + chrtmp[i] - 32;

                        seed = j % 59;
                    }

                    char[] calcCheckSum = new char[2];

                    if (seed == 0)
                    {
                        calcCheckSum[0] = 'A';
                        calcCheckSum[1] = '0';
                    }
                    else
                    {
                        seed = 59 - seed;

                        int j = (seed / 8) & 0x7;
                        int i = seed & 0x7;

                        calcCheckSum[0] = (char)(j + 33 + 32);
                        calcCheckSum[1] = (char)(i + 16 + 32);
                    }

                    String checksum = $"{calcCheckSum[0]}{calcCheckSum[1]}";
                    result = inputCheckSum[0] == calcCheckSum[0] && inputCheckSum[1] == calcCheckSum[1];
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
        public bool CheckIntegrity(OCRDevParameter ocrDev, LoaderBase.ActiveLotInfo activeLotInfo, String ocr)
        {
            // 다른 기타 OCR Integrity
            bool result = false;

            try
            {
                bool LotIntegrity = ocrDev.lotIntegrity.LotIntegrityEnable;

                LoggerManager.Debug($"[OCR] CheckIntegrity LotIntegrity: {LotIntegrity}");

                if (LotIntegrity == true)
                {
                    if (activeLotInfo == null || activeLotInfo.State == LotStateEnum.Idle)
                    {
                        //lot 중이 아님. lotIntegrity 확인할 필요 없음. 
                        result = true;
                        LoggerManager.Debug($"[OCR] CheckIntegrity TRUE activeLotInfo is nuill or Idle(cur: {activeLotInfo?.State}) State. Reading OCR : {ocr}");
                    }
                    else
                    {
                        //lot 중. 
                        if (string.IsNullOrEmpty(activeLotInfo.LotID) == false)
                        {
                            string Lotname = activeLotInfo.LotID.Substring(ocrDev.lotIntegrity.LotnameDigit, ocrDev.lotIntegrity.Lotnamelength);

                            if (ocr.Substring(ocrDev.lotIntegrity.LotnameDigit, ocrDev.lotIntegrity.Lotnamelength) == Lotname)
                            {
                                result = true;
                                LoggerManager.Debug($"[OCR] CheckIntegrity TRUE Lot ID: {activeLotInfo.LotID} ({Lotname}),  Reading OCR : {ocr}");
                            }
                            else
                            {
                                result = false;
                                LoggerManager.Debug($"[OCR] CheckIntegrity FALSE Lot ID: {activeLotInfo.LotID} ({Lotname}), Reading OCR : {ocr}");
                            }
                        }
                        else
                        {
                            result = false;
                            LoggerManager.Debug($"[OCR] CheckIntegrity FALSE LotID is null or empty, Reading OCR : {ocr}");
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return result;
        }
    }
}
