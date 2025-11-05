namespace Temperature.Temp
{
    public class E5EN_HCommand
    {
        #region CompoWay/F
        enum tagMODBUSSTATUS
        {
            WSTS_OK = 0,
            WSTS_ILLEGAL_CMD,
            WSTS_ILLEGAL_ADDR,
            WSTS_ILLEGAL_DATA,
            WSTS_CRC_ERROR,
            WSTS_ECHO_ERROR,
            WSTS_TIMEOUT,
            WSTS_SYSTEM,
            WSTS_MON_RUN
        }

        protected int OMRD = 100;
        protected int OMWR = 200;
        protected int OMSP = 300;

        protected int WT_STX = 2;
        protected int WT_ETX = 3;

        protected string WT_Node_Num = "01";
        protected string WT_Sub_ADD_N_SID = "000";

        protected string WT_Read_Variable_Area = "0101";
        protected string WT_Write_Variable_Area = "0102";
        protected string WT_Composite_Read_Variable_Area = "0104";
        protected string WT_Composite_Write_Variable_Area = "0113";
        protected string WT_Read_Controller_Attribute = "0503";
        protected string WT_Read_Controller_Status = "0601";
        protected string WT_Echoback_Test = "0801";
        protected string WT_Operation_Command = "3005";

        protected string WT_Data_Var_type_80 = "80";
        protected string WT_Data_Var_type_81 = "81";
        protected string WT_Data_Var_type_82 = "82";
        protected string WT_Data_Var_type_83 = "83";
        protected string WT_Data_Bit_Pos = "00";
        protected string WT_Data_Num_Element = "0001";

        protected string WT_PV_Add = "0000";
        protected string WT_LSTATUS_Add = "0001";
        protected string WT_HSTATUS_Add = "0012";
        protected string WT_SP_Add = "0003";
        protected string WT_PID_ON_Add = "0007";
        protected string WT_Temp_Offset = "0012";
        protected string WT_Set_PID_PB = "0015";
        protected string WT_Set_PID_IT = "0016";
        protected string WT_Set_PID_DT = "0017";

        protected string WT_OP_CMD_Code_Com_Wrt = "00";
        protected string WT_OP_CMD_Code_Run_Stop = "01";
        protected string WT_OP_CMD_Code_SWRESET = "06";
        protected string WT_OP_CMD_Code_MoveSetArea1 = "07";
        protected string WT_Set_OP_RUN = "00";
        protected string WT_Set_OP_STOP = "01";
        protected string WT_Set_OP_Com_Wrt_OFF = "00";
        protected string WT_Set_OP_Com_Wrt_On = "01";
        protected string WT_Set_OP_SWRESET = "00";
        protected string WT_Set_OP_MoveSetArea1 = "00";
        #endregion

    }
}
