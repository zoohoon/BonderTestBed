using LogModule;
using LogModule.LoggerController;
using SecsGemServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProberDevelopPackWindow.Tab.Run.Gem
{
    public class RunGemRemoteMessage
    {
        public RunGemRemoteMessage() { }

        public static string GetDefaultMessage(string command)
        {
            string retval = string.Empty;

            try
            {
                // EnumRemoteCommand에 대한 시도
                if (Enum.TryParse(command, out EnumRemoteCommand remoteCommand))
                {
                    // 문자열이 EnumRemoteCommand에 성공적으로 매핑되면 해당 오버로드 호출
                    retval = GetDefaultMessage(remoteCommand);
                }
                // EnumCarrierAction에 대한 시도
                else if (Enum.TryParse(command, out EnumCarrierAction carrierAction))
                {
                    // 문자열이 EnumCarrierAction에 성공적으로 매핑되면 해당 오버로드 호출
                    retval = GetDefaultMessage(carrierAction);
                }
                else
                {
                    // command가 어떤 Enum에도 매핑되지 않는 경우
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }


        public static string GetDefaultMessage(EnumRemoteCommand? command)
        {
            string retval = string.Empty;

            try
            {
                switch (command)
                {
                    case EnumRemoteCommand.UNDEFINE:
                        break;
                    case EnumRemoteCommand.ABORT:
                        break;
                    case EnumRemoteCommand.CC_START:
                        break;
                    case EnumRemoteCommand.DLRECIPE:
                        retval = @"
                                S2F41W  /* HOST COMMAND SEND */
                                <L   
                                   <A ""DLRECIPE"">  /* RCMD */
                                   <L      
                                      <L         
                                         <A ""RECIPE"">     /* CPNAME */
                                         <A ""3PVC-758VA-H087S-ORIGIN-180NOTCH-HOT"">  /* CPVAL */
                                      >
                                      <L         
                                         <A ""STAGE_ID"">   /* CPNAME */
                                         <A ""1"">   /* CPVAL */
                                      >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.JOB_CANCEL:
                        break;
                    case EnumRemoteCommand.JOB_CREATE:
                        break;
                    case EnumRemoteCommand.ONLINE_LOCAL:
                        break;
                    case EnumRemoteCommand.ONLINE_REMOTE:
                        break;
                    case EnumRemoteCommand.ONLINEPP_SELECT:
                        break;
                    case EnumRemoteCommand.PAUSE:
                        break;
                    case EnumRemoteCommand.PP_SELECT:
                        break;
                    case EnumRemoteCommand.PSTART:
                        retval = @"
                                S2F49W  /*RCMD PSTART*/
                                <L   
                                   <U4 1>  
                                   <A """"> 
                                   <A ""PSTART"">   /* RCMD */
                                   <L   
                                       <L
                                          <A ""LOT_ID"">     /* CPNAME */
                                          <A ""Lot222"">     /* CPVALUE */
                                        >     
                                       <L
                                          <A ""PORT_ID"">     /* CPNAME */
                                          <U4 2>     /* CPVALUE */
                                        >  
                                       <L
                                          <A ""CARRIER_ID"">     /* CPNAME */
                                          <A ""PCF5155"">     /* CPVALUE */
                                        >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.PW_REQUEST:
                        break;
                    case EnumRemoteCommand.RESTART:
                        break;
                    case EnumRemoteCommand.RESUME:
                        retval = @"
                                S2F49W  /*RCMD RESUME*/
                                <L   
                                   <U4 1>  
                                   <A """"> 
                                   <A ""RESUME"">   /* RCMD */
                                   <L    
                                       <L
                                          <A ""STAGE_ID"">     /* CPNAME */
                                          <A ""3"">     /* CPVALUE */
                                        > 
                                   >
                                > 
                                ";
                        break;
                    case EnumRemoteCommand.SCAN_CASSETTE:
                        break;
                    case EnumRemoteCommand.SIGNAL_TOWER:
                        break;
                    case EnumRemoteCommand.START:
                        break;
                    case EnumRemoteCommand.STOP:
                        break;
                    case EnumRemoteCommand.UNDOCK:
                        retval = @"
                                S2F49W  /*RCMD UNDOCK*/
                                <L   
                                   <U4 1>
                                   <A  """">
                                   <A ""UNDOCK"">     /* RCMD */
                                   <L      
                                      <L         
                                         <A ""PORT_ID"">  /* CPNAME */
                                         <U4 2>     /* CPVALUE */
                                      >
                                      <L         
                                         <A ""CARRIER_ID"">   /* CPNAME */
                                         <A ""PCF5155"">  /* CPVALUE */
                                      >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.WFCLN:
                        break;
                    case EnumRemoteCommand.WFIDCONFPROC:
                        retval = @"
                                S2F49W  /*RCMD WFIDCONFPROC*/
                                <L   
                                   <U4 1>  
                                   <A """"> 
                                   <A ""WFIDCONFPROC"">   /* RCMD */
                                   <L    
                                       <L
                                          <A ""LOTID"">     /* CPNAME */
                                          <A ""LOTID"">     /* CPVALUE */
                                        >  
                                       <L
                                          <A ""PORT_ID"">     /* CPNAME */
                                          <U4 2>     /* CPVALUE */
                                        >
		                                <L
                                          <A ""SLOT_NUM"">     /* CPNAME */
                                          <U4 1>     /* CPVALUE */
                                        >  		
                                        <L
                                          <A ""WAFER_ID"">     /* CPNAME */
                                          <A ""CE2GT171SEB26"">     /* CPVALUE */
                                        >
		                                <L
                                          <A ""OCRREAD"">     /* CPNAME */
                                          <A ""1"">     /* CPVALUE */
                                        >
                                   >
                                > 
                                ";
                        break;
                    case EnumRemoteCommand.ZIF_REQUEST:
                        break;
                    case EnumRemoteCommand.ACTIVATE_PROCESS:
                        break;
                    case EnumRemoteCommand.DOWNLOAD_STAGE_RECIPE:
                        break;
                    case EnumRemoteCommand.SET_PARAMETERS:
                        break;
                    case EnumRemoteCommand.DOCK_FOUP:
                        break;
                    case EnumRemoteCommand.SELECT_SLOTS:
                        break;
                    case EnumRemoteCommand.START_LOT:
                        break;
                    case EnumRemoteCommand.Z_UP:
                        break;
                    case EnumRemoteCommand.END_TEST:
                        break;
                    case EnumRemoteCommand.CANCEL_CARRIER:
                        break;
                    case EnumRemoteCommand.CARRIER_SUSPEND:
                        break;
                    case EnumRemoteCommand.ERROR_END:
                        break;
                    case EnumRemoteCommand.START_STAGE:
                        break;
                    case EnumRemoteCommand.CANCELCARRIER:
                        break;
                    case EnumRemoteCommand.CHECKPARMETER:
                        break;
                    case EnumRemoteCommand.ZUP:
                        retval = @"
                              S2F41W  /*ZUP 임의로 지정*/
                                <L   
                                   <A ""ZUP"">   /* RCMD */
                                   <L    
                                       <L
                                          <A ""STAGEID"">     /* CPNAME */
                                          <A ""3"">     /* CPVALUE */
                                        >  
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.TESTEND:
                        retval = @"
                              S2F41W  /*RCMD TEST_END  임의로 지정*/
                                <L   
                                   <A ""TESTEND"">   /* RCMD */
                                   <L    
                                       <L
                                          <A ""STAGEID"">     /* CPNAME */
                                          <A ""3"">      /* CPVALUE */
                                        >  
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.WAFERUNLOAD:
                        break;
                    case EnumRemoteCommand.WAFERID_LIST:
                        break;
                    case EnumRemoteCommand.UNDOCK_FOUP:
                        break;
                    case EnumRemoteCommand.STAGE_SLOT:
                        retval = @"
                                S2F49W  /*RCMD STAGE_SLOT*/
                                <L   
                                   <U4 1>  
                                   <A """"> 
                                   <A ""STAGE_SLOT"">   /* RCMD */
                                   <L    
                                       <L
                                          <A ""PORT_ID"">     /* CPNAME */
                                          <U4 2>     /* CPVALUE */
                                        >  
                                        <L
                                          <A ""CARRIER_ID"">     /* CPNAME */
                                          <A ""PCF5155"">     /* CPVALUE */
                                        >
                                        <L
                                          <A ""STAGE"">     /* CPNAME */
                                          <A ""1000000000000000000000000"">     /* CPVALUE */
                                        >
		                                <L
                                          <A ""SLOT"">     /* CPNAME */
                                          <A ""1100000000000000000000000"">     /* CPVALUE */
                                        >
                                   >
                                >  
                                ";
                        break;
                    case EnumRemoteCommand.SELECT_SLOTS_STAGES:
                        break;
                    case EnumRemoteCommand.END_TEST_LP:
                        break;
                    case EnumRemoteCommand.ERROR_END_LP:
                        break;
                    case EnumRemoteCommand.CHANGE_LP_MODE_STATE:
                        break;
                    case EnumRemoteCommand.PABORT:
                        retval = @"
                                S2F41W /*RCMD PABORT*/
                                <L   
                                   <A ""PABORT"">   /* RCMD */
                                   <L    
                                       <L
                                          <A ""STAGEID"">     /* CPNAME */
                                          <A ""3"">     /* CPVALUE */
                                        >  
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.START_CARD_CHANGE:
                        retval = @"
                                S2F41W  /*RCMD START_CARD_CHANGE*/
                                <L   
                                   <A ""START_CARD_CHANGE"">  /* RCMD */
                                   <L      
                                      <L         
                                         <A ""STAGE_NUM"">    /* CPNAME */
                                         <A ""1"">    /* CPVALUE */
                                      >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.MOVEIN_CARD_CLOSE_COVER:
                        retval = @"
                                S2F41W  /*RCMD MOVEIN_CARD_CLOSE_COVER*/
                                <L   
                                   <A ""MOVEIN_CARD_CLOSE_COVER"">    /* RCMD */
                                   <L      
                                      <L         
                                         <A ""STAGE_NUM"">    /* CPNAME */
                                         <A ""1"">    /* CPVALUE */
                                      >
                                      <L         
                                         <A ""ENDSEQ"">   /* CPNAME */
                                         <U1 1>     /* CPVALUE */
                                      >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.PROCEED_CARD_CHANGE:
                        retval = @"
                                S2F41W  /*RCMD PROCEED_CARD_CHANGE*/
                                <L   
                                   <A ""PROCEED_CARD_CHANGE"">    /* RCMD */
                                   <L      
                                      <L         
                                         <A ""STAGE_NUM"">    /* CPNAME */
                                         <A ""1"">    /* CPVALUE */
                                      >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.SKIP_CARD_ATTACH:
                        retval = @"
                               S2F41W  /*RCMD SKIP_CARD_ATTACH*/
                                <L   
                                   <A ""SKIP_CARD_ATTACH"">   /* RCMD */
                                   <L      
                                      <L         
                                         <A ""STAGE_NUM"">    /* CPNAME */
                                         <A ""1"">    /* CPVALUE */
                                      >
                                      <L         
                                         <A ""ENDSEQ"">   /* CPNAME */
                                         <U1 1>     /* CPVALUE */
                                      >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.CARD_SEQ_ABORT:
                        break;
                    case EnumRemoteCommand.DEVICE_CHANGE:
                        break;
                    case EnumRemoteCommand.LOTMODE_CHANGE:
                        break;
                    case EnumRemoteCommand.SELECT_SLOTS_STAGE:
                        break;
                    case EnumRemoteCommand.CHANGE_LOADPORT_MODE:
                        break;
                    case EnumRemoteCommand.TC_START:
                        retval = @"
                                S2F41W  /*RCMD TC_START*/
                                <L   
                                   <A ""TC_START"">   /* RCMD */
                                   <L   
                                       <L
                                          <A ""LOT_ID"">     /* CPNAME */
                                          <A ""LOT111"">     /* CPVALUE */
                                        >     
                                       <L
                                          <A ""PORT_ID"">     /* CPNAME */
                                          <U4 1>     /* CPVALUE */
                                        >  
                                       <L
                                          <A ""CARRIER_ID"">     /* CPNAME */
                                          <A ""PCF51551"">     /* CPVALUE */
                                        >
                                   >
                                > 
                                ";
                        break;
                    case EnumRemoteCommand.TC_END:
                        retval = @"
                                S2F41W  /*TC_END  */
                                <L   
                                   <A ""TC_END"">     /* RCMD */
                                   <L      
                                      <L         
                                         <A ""STAGEID"">  /* CPNAME */
                                         <A ""1"">    /* CPVALUE */
                                      >
                                   >
                                >
                                ";
                        break;
                    case EnumRemoteCommand.WAFER_CHANGE:
                        retval = @"
                            S2F49W  /*WAFERCHANGE*/
                            <L  
                                <U4 1>
                                <A """">
                                <A ""WAFER_CHANGE"">  /* RCMD */
                                <L
                                    <L       
                                        <A ""OCRRead"">
                                        <U4 0>
                                    >
                                    <L
                                        <L
                                            <L
                                                <A ""WAFERID"">
                                                <A ""WaferID"">
                                            >
                                            <L
                                                <A ""LOC1_LP"">
                                                <A ""LP1"">
                                            >
                                            <L
                                                <A ""LOC1_ATOM_IDX"">
                                                <A ""S13"">
                                            >
                                            <L
                                                <A ""LOC2_LP"">
                                                <A """">
                                            >
                                            <L
                                                <A ""LOC2_ATOM_IDX"">
                                                <A ""f2"">
                                            >
                                        >
                                        <L
                                            <L
                                                <A ""WAFERID"">
                                                <A ""WaferID"">
                                            >
                                            <L
                                                <A ""LOC1_LP"">
                                                <A ""LP1"">
                                            >
                                            <L
                                                <A ""LOC1_ATOM_IDX"">
                                                <A ""S12"">
                                            >
                                            <L
                                                <A ""LOC2_LP"">
                                                <A """">
                                            >
                                            <L
                                                <A ""LOC2_ATOM_IDX"">
                                                <A ""f1"">
                                            >
                                        >
                                    >
                                >
                            >
                            ";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public static string GetDefaultMessage(EnumCarrierAction? command)
        {
            string retval = string.Empty;

            try
            {
                switch (command)
                {
                    case EnumCarrierAction.PROCEEDWITHCARRIER:
                        retval = @"
                            S3F17W  /*CRCMD PROCEED_WITH_CARRIER*/
                            <L   
                               <U4 0>  
                               <A ""PROCEEDWITHCARRIER""> /* RCMD */
                               <A ""PCF5155"">   /* CARRIER ID */
                               <U1 1>   /* PIN  - LoadPort Number */
                               <L    
                                   <L
                                      <A ""LOTID"">     /* CPNAME */
                                      <A ""Lot222"">     /* CPVALUE */
                                   >  
                               >
                            >   
                            ";
                        break;
                    case EnumCarrierAction.PROCEEDWITHSLOT:
                        retval = @"
                            S3F17W  /*CRCMD PROCEED_WITH_SLOT*/
                            <L   
                               <U4 0>  
                               <A ""ProceedWithSlot""> /* RCMD */
                               <A ""PCF5155"">   /* CARRIER ID */
                               <U1 2>   /* PIN  - LoadPort Number */
                               <L    
                                   <L
                                      <A ""SlotMap"">     /* CPNAME */
		                              <L
			                            <A ""1"">     /* CPVALUE 1 : Use , 3 : Not Use*/
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""3"">     /* CPVALUE */
			                            <A ""3"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
			                            <A ""1"">     /* CPVALUE */
                                      >
                                    >
		                            <L
                                      <A ""Usage"">     /* CPNAME */
		                              <A ""PW"">     /* CPVALUE */
                                    >
		                            <L
                                      <A ""SlotMap"">     /* CPNAME */
		                              <L
			                            <A ""CE2GT171SEB26"">     /* CPVALUE */
			                            <A ""CE2GT171SEB27"">     /* CPVALUE */
			                            <A ""CE2GT171SEB28"">     /* CPVALUE */
			                            <A ""CE2GT171SEB29"">     /* CPVALUE */
			                            <A ""CE2GT171SEB30"">     /* CPVALUE */
			                            <A ""CE2GT171SEB31"">     /* CPVALUE */
			                            <A ""CE2GT171SEB32"">     /* CPVALUE */
			                            <A ""CE2GT171SEB33"">     /* CPVALUE */
			                            <A ""CE2GT171SEB34"">     /* CPVALUE */
			                            <A ""CE2GT171SEB35"">     /* CPVALUE */
			                            <A ""CE2GT171SEB36"">     /* CPVALUE */
			                            <A ""CE2GT171SEB37"">     /* CPVALUE */
			                            <A ""CE2GT171SEB38"">     /* CPVALUE */
			                            <A ""CE2GT171SEB39"">     /* CPVALUE */
			                            <A ""CE2GT171SEB40"">     /* CPVALUE */
			                            <A ""CE2GT171SEB41"">     /* CPVALUE */
			                            <A ""CE2GT171SEB42"">     /* CPVALUE */
			                            <A ""CE2GT171SEB43"">     /* CPVALUE */
			                            <A ""CE2GT171SEB44"">     /* CPVALUE */
			                            <A ""CE2GT171SEB45"">     /* CPVALUE */
			                            <A ""CE2GT171SEB46"">     /* CPVALUE */
			                            <A ""CE2GT171SEB47"">     /* CPVALUE */
			                            <A ""CE2GT171SEB48"">     /* CPVALUE */
			                            <A ""CE2GT171SEB49"">     /* CPVALUE */
			                            <A ""CE2GT171SEB50"">     /* CPVALUE */
                                      >
                                    >  		
                               >
                            >
                            ";
                        break;
                    case EnumCarrierAction.CANCELCARRIER:
                        retval = @"
                            S3F17W /*RCMD CANCELCARRIER*/
                            <L   
                               <U4 0>
                               <A ""CANCELCARRIER"">     /* RCMD */
                               <A ""PCF5155"">    /* CARRIER ID */
                               <U1 2>   /* PIN  - LOADPORT NUMBER */
                               <L      
                                  <L         
                                     <A ""LOTID"">    /* CPNAME */
                                     <A ""LOT222"">   /* CPVALUE */
                                  >
                               >
                            >
                            ";
                        break;
                    case EnumCarrierAction.RELEASECARRIER:
                        break;
                    case EnumCarrierAction.PROCESSEDWITHCELLSLOT:
                        break;
                    case EnumCarrierAction.CHANGEACCESSMODE:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
