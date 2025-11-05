using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autofac;
using LoaderBase;
using ProberErrorCode;
using ProberInterfaces;
using LoaderParameters;
using System.Runtime.CompilerServices;
using LogModule;
using ProberInterfaces.Enum;

namespace LoaderCore.ReadSemicsOCRStates
{
    public abstract class ReadSemicsOCRState : LoaderProcStateBase
    {
        public ReadSemicsOCR Module { get; set; }

        public ReadSemicsOCRState(ReadSemicsOCR module)
        {
            this.Module = module;
        }
        protected void StateTransition(ReadSemicsOCRState stateObj)
        {
            try
            {
                Module.StateObj = stateObj;
                LoggerManager.Debug($"[LOADER] {Module.GetType().Name} state tranition : {State}");
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        #region => OCR Remote Methods
        protected EventCodeEnum RaiseInvalidState([CallerMemberName]string memberName = "")
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                //Log.Warn($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");
                LoggerManager.Debug($"{Module.GetType().Name}.{GetType().Name}.{memberName}() : Invalid state error occurred.");

                retval = EventCodeEnum.LOADER_STATE_INVALID;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            imgBuf = new byte[0];
            w = 0;
            h = 0;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum ChangeLight(int channelMapIdx, ushort intensity)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public virtual EventCodeEnum SetOcrID(string ocrID)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum OCRRemoteEnd()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum GetOCRDoneState()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum OCRRetry()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public virtual EventCodeEnum OCRFail()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                retval = RaiseInvalidState();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        #endregion

        protected ILoaderModule Loader => Module.Container.Resolve<ILoaderModule>();

        protected ISemicsOCRModule OCR => Module.Param.UseOCR as ISemicsOCRModule;

        protected IARMModule UseARM => Module.Param.UseARM;

        protected bool IsManualMode()
        {
            bool retval = false;

            try
            {
                retval = Module.Param.DestPos == OCR;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        protected ReadOCRProcessingParam ConvertToOCRProcessingParam(SemicsOCRDevice device, int index)
        {
            ReadOCRProcessingParam param = new ReadOCRProcessingParam();

            try
            {
                param.OcrCalibrateMaxX = device.OcrCalibrateMaxX.Value;
                param.OcrCalibrateMaxX = device.OcrCalibrateMaxX.Value;
                param.OcrReadRegionPosX = device.OcrReadRegionPosX.Value;
                param.OcrReadRegionPosY = device.OcrReadRegionPosY.Value;
                param.OcrReadRegionWidth = device.OcrReadRegionWidth.Value;
                param.OcrReadRegionHeight = device.OcrReadRegionHeight.Value;
                param.OcrCharSizeX = device.OcrCharSizeX.Value;
                param.OcrCharSizeY = device.OcrCharSizeY.Value;
                param.OcrCharSpacing = device.OcrCharSpacing.Value;
                param.OcrMaxStringLength = device.OcrMaxStringLength.Value;
                param.OcrCalibrateMinX = device.OcrCalibrateMinX.Value;
                param.OcrCalibrateMaxX = device.OcrCalibrateMaxX.Value;
                param.OcrCalibrateStepX = device.OcrCalibrateStepX.Value;
                param.OcrCalibrateMinY = device.OcrCalibrateMinY.Value;
                param.OcrCalibrateMaxY = device.OcrCalibrateMaxY.Value;
                param.OcrCalibrateStepY = device.OcrCalibrateStepY.Value;

                param.OcrSampleString = device.OCRParamTables[index].OcrSampleString.Value;
                param.OcrConstraint = device.OCRParamTables[index].OcrConstraint.Value;
                param.OcrStrAcceptance = device.OCRParamTables[index].OcrStrAcceptance.Value;
                param.OcrCharAcceptance = device.OCRParamTables[index].OcrCharAcceptance.Value;
                param.UserOcrLightType = device.OCRParamTables[index].UserOcrLightType.Value;
                param.UserOcrLight1_Offset = device.OCRParamTables[index].UserOcrLight1_Offset.Value;
                param.UserOcrLight2_Offset = device.OCRParamTables[index].UserOcrLight2_Offset.Value;
                param.UserOcrLight3_Offset = device.OCRParamTables[index].UserOcrLight3_Offset.Value;
                param.OcrCalibrationType = device.OCRParamTables[index].OcrCalibrationType.Value;
                param.OcrMasterFilter = device.OCRParamTables[index].OcrMasterFilter.Value;
                param.OcrMasterFilterGain = device.OCRParamTables[index].OcrMasterFilterGain.Value;
                param.OcrSlaveFilter = device.OCRParamTables[index].OcrSlaveFilter.Value;
                param.OcrSlaveFilterGain = device.OCRParamTables[index].OcrSlaveFilterGain.Value;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return param;
        }

        protected bool CalOcrCheckSum()
        {
            //TBD Function
            return true;
            /*
            bool retval = false;
            try
            {
                int i, j;
                int intLength;
                int[] chrtmp = new int[19];
                int seedVal;
                string strOrgCS;
                string strNewCS;
                string strImsiOcrBuf;

                //Function CalOcrChecksum(ByRef strtmp As String, ByRef strChkSum As String) As Boolean
                //    On Error GoTo SysError
                //    Dim i As Integer
                //    Dim j As Integer
                //    Dim intLength As Integer
                //    Dim chrtmp(0 To 18) As Integer
                //    Dim seedval As Integer
                //    Dim strOrgCS(0 To 1) As String
                //    Dim strNewCS(0 To 1) As String
                //    Dim strImsiOcrBuf As String


                //    strImsiOcrBuf = strtmp
                //    intLength = c_Int(Len(strImsiOcrBuf))
                //    If Not intLength > 2 Then GoTo FAILED

                //    'modified on 2002/11/07 10:55:19 cwpark
                //    'save original checksum
                //    strOrgCS(0) = Mid$(strImsiOcrBuf, intLength - 1, 1)
                //    strOrgCS(1) = Mid$(strImsiOcrBuf, intLength - 0, 1)
                //    'force set default checksum


                //    '' <<< add by SangYun 081009
                //    If InStr(strtmp, "*") Then
                //        strChkSum = "**"
                //        CalOcrChecksum = False
                //        Exit Function
                //    End If
                //    '' >>>

                //    Mid$(strImsiOcrBuf, intLength - 1, 1) = "A"
                //    Mid$(strImsiOcrBuf, intLength - 0, 1) = "0"

                //    For i = 1 To intLength
                //        chrtmp(i) = Asc(Mid$(strImsiOcrBuf, i, 1))
                //    Next i

                //    seedval = 0
                //    For i = 1 To intLength '- 2
                //        j = (seedval * 8) Mod 59
                //        j = j + (chrtmp(i) - 32)
                //        seedval = j Mod 59
                //    Next

                //    If seedval = 0 Then
                //        strNewCS(0) = "A"
                //        strNewCS(1) = "0"
                //    Else
                //        seedval = 59 - seedval
                //        j = (seedval \ 8) And & H7
                //        i = seedval And & H7
                //        strNewCS(0) = Chr$(j + 33 + 32)
                //        strNewCS(1) = Chr$(i + 16 + 32)
                //    End If

                //    strChkSum = strNewCS(0) + strNewCS(1) 'return by arg.
                //    If strOrgCS(0) <> strNewCS(0) Or strOrgCS(1) <> strNewCS(1) Then
                //FAILED:
                //        CalOcrChecksum = False
                //    Else
                //        CalOcrChecksum = True
                //    End If
                //    Exit Function

                //SysError:
                //            CalOcrChecksum = False
                //    LogSysErr Err.description, Err.Number, "[modLoader] CalOcrChecksum()", False
                //    Debug.Assert 0
                //End Function

                retval = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
            */
        }

        protected int GetMaxValueIndex(double[] arr)
        {
            int retval = 0;

            try
            {
                double MaxVal = arr[0];

                for (int i = 1; i < arr.Length; i++)
                {
                    if (MaxVal < arr[i])
                    {
                        retval = i;
                        MaxVal = arr[i];
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        protected void SetOcrLight(int paramTableIndex = 0)
        {
            try
            {
                // Light
                if (Module.Data.EnableOcrLightRemoteMode == false)
                {
                    if (Module.Data.UseOCRDevice.UserLightEnable.Value == false)
                    {
                        Loader.Light.SetLight(OCR.Definition.LightChannel1.Value, OCR.Definition.OcrLight1_Offset.Value);
                        Loader.Light.SetLight(OCR.Definition.LightChannel2.Value, OCR.Definition.OcrLight2_Offset.Value);
                        Loader.Light.SetLight(OCR.Definition.LightChannel3.Value, OCR.Definition.OcrLight3_Offset.Value);
                    }
                    else
                    {
                        var param = Module.Data.UseOCRDevice.OCRParamTables[paramTableIndex];

                        Loader.Light.SetLight(OCR.Definition.LightChannel1.Value, param.UserOcrLight1_Offset.Value);
                        Loader.Light.SetLight(OCR.Definition.LightChannel2.Value, param.UserOcrLight2_Offset.Value);
                        Loader.Light.SetLight(OCR.Definition.LightChannel3.Value, param.UserOcrLight3_Offset.Value);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        protected void ClearOcrLight()
        {
            try
            {
                // Light
                Loader.Light.SetLight(OCR.Definition.LightChannel1.Value, 0);
                Loader.Light.SetLight(OCR.Definition.LightChannel2.Value, 0);
                Loader.Light.SetLight(OCR.Definition.LightChannel3.Value, 0);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class IdleState : ReadSemicsOCRState
    {
        public IdleState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.IDLE;

        public override void Execute()
        {
            try
            {
                /*
                ////=>테스트 코드 start
                Loader.VisionManager.SingleGrab(OCR.Definition.OCRCam.Value);

                Loader.ServiceCallback.UI_ShowLoaderCam();

                System.Threading.Thread.Sleep(1000);

                Loader.ServiceCallback.UI_HideLoaderCam();

                Module.Param.TransferObject.SetOCRState("TESTLOT-01-C1", 100, OCRReadStateEnum.DONE);
                if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                {
                    Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, "TESTLOT-01-C1");
                }
                Loader.BroadcastLoaderInfo();
                StateTransition(new DoneState(Module));

                return;
                //<= 테스트 코드 end
                */
                if (IsManualMode() == false)
                {
                    Loader.ServiceCallback.UI_ShowLoaderCam();
                }

                Module.Data = new ReadSemicsOCRData();
                Module.Data.UseOCRDevice = GetUseOCRDevice();

                Module.Data.ReadOCRResults = new List<ReadOCRResult>();

                StateTransition(new ReadingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private SemicsOCRDevice GetUseOCRDevice()
        {
            SemicsOCRDevice device = null;

            try
            {
                if (Module.Param.TransferObject.OverrideOCRDeviceOption.IsEnable.Value)
                {
                    device = Module.Param.TransferObject.OverrideOCRDeviceOption.OCRDeviceBase as SemicsOCRDevice;
                }
                else
                {
                    device = OCR.Device;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return device;
        }

        private List<ReadOCRProcessingParam> GetReadingParams(SemicsOCRDevice device)
        {
            List<ReadOCRProcessingParam> list = new List<ReadOCRProcessingParam>();

            try
            {
                int readProcCount = device.OCRParamTables.Count;

                for (int i = 0; i < readProcCount; i++)
                {
                    var readProcParam = ConvertToOCRProcessingParam(device, i);
                    list.Add(readProcParam);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            
            return list;
        }
    }

    public class ReadingState : ReadSemicsOCRState
    {
        public ReadingState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                SemicsOCRDevice device = Module.Data.UseOCRDevice;

                // Clean OCR State
                Module.Param.TransferObject.CleanOCRState();

                // Set ocr processing param
                // ReadOCRProcessingParam procParam = module.ProcVar.ReadingParams.FirstOrDefault();

                bool isReadSuceed = false;

                try
                {
                    // Light
                    SetOcrLight(0);

                    // Grab
                    ImageBuffer ib = Loader.VisionManager.SingleGrab(OCR.Definition.OCRCam.Value);

                    // OCR vision processing    
                    var procParam = ConvertToOCRProcessingParam(device, 0);
                    string font_get_path = this.FileManager().GetDeviceParamFullPath(device.OCRFontFilePath.Value, device.OCRFontFileName.Value); ;
                    var ocrRel = Loader.VisionManager.ReadOCRProcessing(ib, procParam, font_get_path, true);

                    Module.Data.ReadOCRResults.Add(ocrRel);
                    isReadSuceed = true;
                }
                catch (Exception err)
                {
                    isReadSuceed = false;
                    //LoggerManager.Error($ex.Message);
                    LoggerManager.Exception(err);

                }

                if (isReadSuceed == false)
                {
                    Module.Param.TransferObject.SetOCRState("", 0, OCRReadStateEnum.FAILED);
                    if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, "");
                    }

                    if (IsManualMode())
                    {
                        StateTransition(new DoneState(Module)); //processing error
                        return;
                    }
                    else
                    {
                        StateTransition(new WaitForOCRRemoteState(Module));
                        return;
                    }
                }

                StateTransition(new AnalyzeState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class AnalyzeState : ReadSemicsOCRState
    {
        public AnalyzeState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                string readStr = Module.Data.ReadOCRResults[0].OCRResultStr;
                double score = Module.Data.ReadOCRResults[0].OCRResultScore;

                //=> Check Ocr Confirm
                bool isOCRConfirm = true;
                if (Module.Data.UseOCRDevice.OcrConfirmEnable.Value)
                {
                    //=> Check Score
                    if (Module.Data.ReadOCRResults[0].OCRResultScore < Module.Data.UseOCRDevice.OCRParamTables[0].OcrStrAcceptance.Value)
                    {
                        isOCRConfirm = false;
                    }

                    //=> Check Slot Number 
                    if (isOCRConfirm && Module.Data.UseOCRDevice.SlotIntegrityEnable.Value)
                    {
                        string readSlotNum = "";

                        // Get slot number
                        for (int i = 0; i < Module.Data.UseOCRDevice.OcrMaxStringLength.Value - 2; i++)
                        {
                            if (
                                (Module.Data.UseOCRDevice.OCRParamTables[0].OcrConstraint.Value[i] == 'S') &&
                                (Module.Data.UseOCRDevice.OCRParamTables[0].OcrConstraint.Value[i + 1] == 'S')
                                )
                            {
                                readSlotNum = Module.Data.ReadOCRResults[0].OCRResultStr.Substring(i, 2);
                                break;
                            }
                        }

                        int checkSlotNum;
                        if (string.IsNullOrEmpty(readSlotNum) || readSlotNum.Contains("*"))
                        {
                            isOCRConfirm = false;
                        }
                        else if (int.TryParse(readSlotNum, out checkSlotNum))
                        {
                            int originSlotNum = Module.Param.TransferObject.OriginHolder.Index;
                            if (checkSlotNum != originSlotNum)
                            {
                                isOCRConfirm = false;
                            }
                        }
                    }

                    //=> Check Sum
                    if (isOCRConfirm && Module.Data.UseOCRDevice.OcrCheckSumEnable.Value)
                    {
                        string[] ocrCheckSum = new string[2];

                        // Get Checksum number
                        for (int i = 0; i < Module.Data.UseOCRDevice.OcrMaxStringLength.Value - 2; i++)
                        {
                            if ((Module.Data.UseOCRDevice.OCRParamTables[0].OcrConstraint.Value[i] == 'C') &&
                                (Module.Data.UseOCRDevice.OCRParamTables[0].OcrConstraint.Value[i + 1] == 'C'))
                            {
                                ocrCheckSum[0] = Module.Data.ReadOCRResults[0].OCRResultStr.Substring(i, 1);
                                ocrCheckSum[1] = Module.Data.ReadOCRResults[0].OCRResultStr.Substring(i + 1, 1);
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(ocrCheckSum[0]) ||
                            string.IsNullOrEmpty(ocrCheckSum[1]) ||
                            ocrCheckSum[0].Contains("*") ||
                            ocrCheckSum[1].Contains("*"))
                        {
                            isOCRConfirm = false;
                        }
                        else if (CalOcrCheckSum() == false)
                        {
                            isOCRConfirm = false;
                        }
                    }
                }

                if (isOCRConfirm == false)
                {
                    if (Module.Data.UseOCRDevice.OcrAdvancedReadEnable.Value)
                    {
                        StateTransition(new AdvanceReadingState(Module));
                        return;
                    }
                    else if (IsManualMode())
                    {
                        Module.Param.TransferObject.SetOCRState(readStr, score, OCRReadStateEnum.FAILED);
                        if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, readStr);
                        }
                        Loader.BroadcastLoaderInfo();

                        StateTransition(new DoneState(Module)); // processing error
                        return;
                    }
                    else
                    {
                        Module.Param.TransferObject.SetOCRState(readStr, score, OCRReadStateEnum.FAILED);
                        if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, readStr);
                        }
                        Loader.BroadcastLoaderInfo();

                        StateTransition(new WaitForOCRRemoteState(Module));
                        return;
                    }
                }

                if (IsManualMode() == false)
                {
                    Loader.ServiceCallback.UI_HideLoaderCam();
                }

                ClearOcrLight();

                Module.Param.TransferObject.SetOCRState(readStr, score, OCRReadStateEnum.DONE);
                if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                {
                    Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, readStr);
                }
                Loader.BroadcastLoaderInfo();

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class AdvanceReadingState : ReadSemicsOCRState
    {
        public AdvanceReadingState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                bool isReadSuceed = false;
                for (int i = 1; i < Module.Data.UseOCRDevice.OCRParamTables.Count; i++)
                {
                    try
                    {
                        // Light
                        SetOcrLight(i);

                        // Grab
                        ImageBuffer ib = Loader.VisionManager.WaitGrab(OCR.Definition.OCRCam.Value);

                        // Set ocr processing param
                        var procParam = ConvertToOCRProcessingParam(Module.Data.UseOCRDevice, i);
                        string font_get_path = this.FileManager().GetDeviceParamFullPath(Module.Data.UseOCRDevice.OCRFontFilePath.Value, Module.Data.UseOCRDevice.OCRFontFileName.Value);
                        // OCR vision processing    
                        var ocrRel = Loader.VisionManager.ReadOCRProcessing(ib, procParam, font_get_path, true);

                        Module.Data.ReadOCRResults.Add(ocrRel);

                        isReadSuceed = true;
                    }
                    catch (Exception err)
                    {
                        isReadSuceed = false;
                        //LoggerManager.Error($ex.Message);
                        //LoggerManager.Exception(err);
                        LoggerManager.Exception(err);

                    }
                }

                if (isReadSuceed == false)
                {
                    Module.Param.TransferObject.SetOCRState("", 0, OCRReadStateEnum.FAILED);
                    if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, "");
                    }

                    if (IsManualMode())
                    {
                        StateTransition(new DoneState(Module));
                        return;
                    }
                    else
                    {
                        StateTransition(new WaitForOCRRemoteState(Module));
                        return;
                    }
                }

                StateTransition(new AdvanceAnalyzeState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class AdvanceAnalyzeState : ReadSemicsOCRState
    {
        public AdvanceAnalyzeState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override void Execute()
        {
            try
            {
                SemicsOCRDevice device = Module.Data.UseOCRDevice;

                string[] ocrCheckSum = new string[2];
                double[] tmpScore = new double[device.OCRParamTables.Count];
                int index = 0;
                StringBuilder sb = new StringBuilder();

                ReadOCRResult AdvancedResult = new ReadOCRResult();

                for (int i = 0; i < device.OcrMaxStringLength.Value; i++)
                {
                    for (int j = 0; j < device.OCRParamTables.Count; j++)
                    {
                        tmpScore[j] = Module.Data.ReadOCRResults[j].OCRCharInfo.CharScore[i];
                    }

                    index = GetMaxValueIndex(tmpScore);
                    AdvancedResult.OCRCharInfo.CharScore[i] = Module.Data.ReadOCRResults[index].OCRCharInfo.CharScore[i];
                    sb.Append((Module.Data.ReadOCRResults[index].OCRResultStr).Substring(i, 1));
                }

                AdvancedResult.OCRResultStr = sb.ToString();
                AdvancedResult.OCRResultScore = AdvancedResult.OCRCharInfo.CharScore.Average();

                bool isOCRConfirm = true;
                // Score check
                if (AdvancedResult.OCRResultScore < device.OCRParamTables[0].OcrStrAcceptance.Value)
                {
                    isOCRConfirm = false;
                }

                if (isOCRConfirm == true)
                {
                    for (int i = 0; i < device.OcrMaxStringLength.Value; i++)
                    {
                        if (AdvancedResult.OCRCharInfo.CharScore[i] < device.OCRParamTables[0].OcrCharAcceptance.Value)
                        {
                            isOCRConfirm = false;
                            break;
                        }
                    }
                }

                // Slot number check
                if (isOCRConfirm && device.SlotIntegrityEnable.Value)
                {
                    string readSlotNum = "";

                    // Get slot number
                    for (int i = 0; i < device.OcrMaxStringLength.Value - 2; i++)
                    {
                        if ((device.OCRParamTables[0].OcrConstraint.Value[i] == 'S') && (device.OCRParamTables[0].OcrConstraint.Value[i + 1] == 'S'))
                        {
                            readSlotNum = AdvancedResult.OCRResultStr.Substring(i, 2);
                            break;
                        }
                    }

                    int checkSlotNum;
                    if (string.IsNullOrEmpty(readSlotNum) || readSlotNum.Contains("*"))
                    {
                        isOCRConfirm = false;
                    }
                    else if (int.TryParse(readSlotNum, out checkSlotNum))
                    {
                        int originSlotNum = Module.Param.TransferObject.OriginHolder.Index;
                        if (checkSlotNum != originSlotNum)
                        {
                            isOCRConfirm = false;
                        }
                    }
                }

                // CheckSum
                if (isOCRConfirm && device.OcrCheckSumEnable.Value)
                {
                    // Get Checksum number
                    for (int i = 0; i < device.OcrMaxStringLength.Value - 2; i++)
                    {
                        if ((device.OCRParamTables[0].OcrConstraint.Value[i] == 'C') && (device.OCRParamTables[0].OcrConstraint.Value[i + 1] == 'C'))
                        {
                            ocrCheckSum[0] = AdvancedResult.OCRResultStr.Substring(i, 1);
                            ocrCheckSum[1] = AdvancedResult.OCRResultStr.Substring(i + 1, 1);
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(ocrCheckSum[0]) ||
                        string.IsNullOrEmpty(ocrCheckSum[1]) ||
                        ocrCheckSum[0].Contains("*") ||
                        ocrCheckSum[1].Contains("*"))
                    {
                        isOCRConfirm = false;
                    }
                    else if (CalOcrCheckSum() == false)
                    {
                        isOCRConfirm = false;
                    }

                }

                string readStr = AdvancedResult.OCRResultStr;
                double score = AdvancedResult.OCRResultScore;

                if (isOCRConfirm == false)
                {
                    if (device.OcrAdvancedReadEnable.Value)
                    {
                        StateTransition(new AdvanceReadingState(Module));
                        return;
                    }
                    else if (IsManualMode())
                    {
                        Module.Param.TransferObject.SetOCRState(readStr, score, OCRReadStateEnum.FAILED);
                        if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, readStr);
                        }
                        Loader.BroadcastLoaderInfo();

                        StateTransition(new DoneState(Module));
                        return;
                    }
                    else
                    {
                        Module.Param.TransferObject.SetOCRState(readStr, score, OCRReadStateEnum.FAILED);
                        if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                        {
                            Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, readStr);
                        }
                        Loader.BroadcastLoaderInfo();

                        StateTransition(new WaitForOCRRemoteState(Module));
                        return;
                    }
                }

                if (IsManualMode() == false)
                {
                    Loader.ServiceCallback.UI_HideLoaderCam();
                }

                ClearOcrLight();
                Module.Param.TransferObject.SetOCRState(readStr, score, OCRReadStateEnum.DONE);
                if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                {
                    Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, readStr);
                }
                Loader.BroadcastLoaderInfo();

                StateTransition(new DoneState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class WaitForOCRRemoteState : ReadSemicsOCRState
    {
        public WaitForOCRRemoteState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SUSPENDED;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.OCR_FAILED;

        public override void Execute() { /*NoWORKS*/ }

        public override void Resume()
        {
            try
            {
                StateTransition(new RemotingState(Module));
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
    }

    public class RemotingState : ReadSemicsOCRState
    {
        public RemotingState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.RUNNING;

        public override ReasonOfSuspendedEnum ReasonOfSuspended => ReasonOfSuspendedEnum.NONE;

        public override void Execute() { /*NoWORKS*/ }

        public override EventCodeEnum GetOCRImage(out byte[] imgBuf, out int w, out int h)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            ImageBuffer ib = Loader.VisionManager.WaitGrab(OCR.Definition.OCRCam.Value);

            imgBuf = ib.Buffer;
            w = ib.SizeX;
            h = ib.SizeY;

            try
            {
                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum ChangeLight(int lightChannel, ushort intensity)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.Data.EnableOcrLightRemoteMode = true;

                Loader.Light.SetLight(lightChannel, intensity);

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public override EventCodeEnum SetOcrID(string ocrID)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                Module.Data.OcrTextByUserInput = ocrID;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum OCRRemoteEnd()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                if (string.IsNullOrEmpty(Module.Data.OcrTextByUserInput))
                {
                    StateTransition(new ReadingState(Module));
                }
                else
                {
                    if (IsManualMode() == false)
                    {
                        Loader.ServiceCallback.UI_HideLoaderCam();
                    }

                    ClearOcrLight();

                    Module.Param.TransferObject.SetOCRState(Module.Data.OcrTextByUserInput, 100, OCRReadStateEnum.DONE);
                    if (Module.Param.TransferObject.OriginHolder.ModuleType == ModuleTypeEnum.SLOT)
                    {
                        Loader.ServiceCallback.WaferIDChanged(Module.Param.TransferObject.OriginHolder.Index, Module.Data.OcrTextByUserInput);
                    }
                    Loader.BroadcastLoaderInfo();

                    StateTransition(new DoneState(Module));
                }

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public override EventCodeEnum OCRFail()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                StateTransition(new SystemErrorState(Module));

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
    
    public class DoneState : ReadSemicsOCRState
    {
        public DoneState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.DONE;

        public override void Execute() { /*NoWORKS*/ }
    }
   
    public class SystemErrorState : ReadSemicsOCRState
    {
        public SystemErrorState(ReadSemicsOCR module) : base(module) { }

        public override LoaderProcStateEnum State => LoaderProcStateEnum.SYSTEM_ERROR;

        public override void Execute() { /*NoWORKS*/ }

        public override void SelfRecovery() { /*NoWORKS*/ }
    }
}
