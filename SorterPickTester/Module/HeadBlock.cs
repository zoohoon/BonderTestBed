using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LogModule;
using SorterPickTester.Net;
using SorterPickTester.Module;
using SorterPickTester.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SorterPickTester.Module
{
    public class HeadBlock : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public PickerData _pickerData = new PickerData();
        public ControllerPPR _cntrPPR = null;
        public PickerAxisT _modulePickerAxisT = null;
        public PickerAxisZ _modulePickerAxisZ = null;
        public PickerSensing _modulePickerSensing = null;

        public PickerData PickerData
        {
            get { return _pickerData; }
        }

        public HeadBlock()
        {
            _cntrPPR = new ControllerPPR(ref _pickerData);
            _modulePickerAxisT = new PickerAxisT(ref _pickerData);
            _modulePickerAxisZ = new PickerAxisZ(ref _pickerData);
            _modulePickerSensing = new PickerSensing(ref _pickerData);
        }

        private bool _SetContinuane;
        public bool SetContinuane
        {
            get { return _SetContinuane; }
            set
            {
                if (value != _SetContinuane)
                {
                    _SetContinuane = value;
                    _cntrPPR.SetContinuance(value);
                    RaisePropertyChanged();
                }
            }
        }

        public void Send_PPR_Command_Once()
        {
            _cntrPPR.Send_PPR_Command_Single();
        }

        //
        //2024/09/09 : 추후에 동작 확인이 필요함. Sequence와 뭐가 다른가?
        //
        public void SetFuncSeqStart() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_SEQ_START); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncSeqStop() { _pickerData.Set_Func_Order_bit_Off(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_STOP); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncAlarmReset() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_ALARM_RES); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncHisReset() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_HIST_RST); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncServoOff() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_SERVO_OFF); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncAirStop() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_AIR_STOP); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncAirNega() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_AIR_NEGA); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncAirPosi() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_AIR_POSI); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncTrgg1() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_EXT_TRIG1); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncTrgg2() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_EXT_TRIG2); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncTrgg3() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_EXT_TRIG3); _cntrPPR.CycleFirstFlag = true; }
        public void SetFuncTrgg4() { _pickerData.Set_Func_Order_bit_On(ControllerPPR.CYC_BIT_W1_FUNC_ORDER_EXT_TRIG4); _cntrPPR.CycleFirstFlag = true; }

        //
        //2024/09/09 : Sequence Handling --> 추후에 Params 추가 필요함.
        //
        public void SetSeqNoServoOn() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_SERVO_ON); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoServoOff() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_SERVO_OFF); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoBrakeOn() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_BRAKE_ON); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoBrakeOff() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_BRAKE_OFF); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoHomeReturn() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_HOME_RETURN); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoInchingZ() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_Z_INCHING); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoInchingT() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_T_INCHING); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoUpdateUserOriginZ() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_UPDATE_USER_Z_ORIGIN); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoUpdateUserOriginT() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_UPDATE_USER_T_ORIGIN); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoClearUserOriginZ() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_CLEAR_USER_Z_ORIGIN); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoClearUserOriginT() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_CLEAR_USER_T_ORIGIN); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoUpdateSensorZero() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_UPDATE_SENSOR_ZERO_ADJUSTMENT); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoClearSensorZero() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_CLEAR_SENSOR_ZERO_ADJUSTMENT); _cntrPPR.CycleFirstFlag = true; }

        public void SetSeqNoVacuumValveOn() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_VACUUM_VALVE_ON); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoVacuumValveOff() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_VACUUM_VALVE_OFF); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoReleaseValveOn() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_VACUUM_VALVE_ON); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoReleaseValveOff() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_VACUUM_VALVE_OFF); _cntrPPR.CycleFirstFlag = true; }

        public void SetSeqNoUpdateForceControlCorrection() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_UPDATE_FORCECNTR_CORRECTION_VAL); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoSerOnZ() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_SERON_ON_Z); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoSerOnT() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_SERON_ON_T); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoSerOffZ() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_SERON_OFF_Z); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoSerOffT() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_SERON_OFF_T); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoHomeReturnZ() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_HOME_RETURN_Z); _cntrPPR.CycleFirstFlag = true; }
        public void SetSeqNoHomeReturnT() { _pickerData.Set_Sequence_Number(ControllerPPR.CYC_SEQ_NUM_HOME_RETURN_T); _cntrPPR.CycleFirstFlag = true; }
    }
}
