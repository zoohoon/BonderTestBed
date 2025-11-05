using System.Collections.Generic;

namespace TesterDriverModule
{
    using BinDataMaker;
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.Communication.Scenario;
    using ProberInterfaces.Enum;
    using RequestCore.QueryPack.GPIB;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TesterDriverModule.NI4882;

    public class CommandAndReceived
    {
        public string Command { get; set; }
        public bool Received { get; set; }
        public bool Sended { get; set; }
    }

    public class NI4882EmulDriver : ITesterComDriver, IFactoryModule, IHasTesterScenarioModule
    {
        public ITesterScenarioModule ScenarioModule { get; set; }

        Random random = new Random(DateTime.Now.Millisecond);
        public ObservableCollection<CollectionComponent> CommandCollection { get; set; }

        private string lastReceiveSTB = string.Empty;
        public Queue<string> TesterResponseQueue = new Queue<string>();
        public CommandAndReceived CurrentQueryData = null;

        public NI4882EmulDriver()
        {
            if (ScenarioModule == null)
            {
                ScenarioModule = new NI4882TesterScenario();
                ScenarioModule.InitModule();
            }
        }

        public EventCodeEnum Connect(object connectparam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurrentQueryData = null;
                TesterResponseQueue.Clear();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public EventCodeEnum DisConnect()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                CurrentQueryData = null;
                TesterResponseQueue.Clear();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
        public object GetState()
        {
            try
            {
                if (this.LotOPModule().ModuleState.State == ModuleStateEnum.RUNNING)
                {
                    if (TesterResponseQueue.Count > 0)
                    {
                        return (int)GpibStatusFlags.LACS | (int)GpibStatusFlags.REM;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return 0;
        }
        public string Read()
        {
            string retval = string.Empty;

            try
            {
                if (TesterResponseQueue != null && TesterResponseQueue.Count > 0)
                {
                    retval = TesterResponseQueue.Dequeue();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void WriteSTB(object command)
        {
            try
            {
                lastReceiveSTB = command.ToString();

                EnumGPIBCommandType cmdType = EnumGPIBCommandType.ACTION;

                VerifyCommandOrder(lastReceiveSTB, cmdType);

                LoggerManager.Debug($"[{this.GetType().Name}], WriteSTB() : STB = {lastReceiveSTB}", isInfo: true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);

            }
        }
        public void WriteString(string query_command)
        {
            try
            {
                LoggerManager.Debug($"[{this.GetType().Name}], WriteString() : QUERY = {query_command.Replace("\r", "").Replace("\n", "")}", isInfo: true);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void VerifyCommandOrder(string InputCommand, EnumGPIBCommandType type)
        {
            try
            {
                if (!string.IsNullOrEmpty(InputCommand))
                {
                    ScenarioCommand scenariocommand = ScenarioModule.SelectedScenario.Commands.FirstOrDefault(x => x.Name.Contains(InputCommand));

                    if (scenariocommand != null)
                    {
                        // 시나리오에 정의되어 있는 데이터를 이용하여 그 다음을 구성.

                        if (scenariocommand.RequestSet != null && scenariocommand.RequestSet.Count > 0)
                        {
                            GetReqeustcommandSet(scenariocommand.RequestSet);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
            }
        }
        private void GetReqeustcommandSet(List<RequestSet> requestset)
        {
            try
            {
                TesterCommand tmp = null;
                ScenarioTree tree = null;

                string requestname = string.Empty;

                foreach (var request in requestset)
                {
                    // 분기에 따른 Request를 얻어와야하는 경우 처리
                    if (request.getRequestNameFunc != null)
                    {
                        tree = request.getRequestNameFunc(lastReceiveSTB);

                        if (tree.RequestSet != null)
                        {
                            GetReqeustcommandSet(tree.RequestSet);
                        }
                    }
                    else
                    {
                        requestname = request.RequestName;
                    }

                    if (!string.IsNullOrEmpty(requestname))
                    {
                        tmp = ScenarioModule.CommandRecipe.Commands.FirstOrDefault(x => x.Name.Contains(requestname));

                        if (tmp != null)
                        {
                            string response = string.Empty;

                            bool IsMcommand = false;

                            // TODO : M 확인

                            if (IsMcommand == true)
                            {
                                response = MakeM();
                            }
                            else
                            {
                                response = tmp.Name;
                            }

                            TesterResponseQueue.Enqueue(response);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private string MakeM()
        {
            string retval = string.Empty;

            try
            {
                int dutCount = this.StageSupervisor().ProbeCardInfo.ProbeCardDevObjectRef.DutList.Count;

                var bintype = this.GPIB().GetBinCalulateType();

                if (bintype == BinType.BIN_PASSFAIL)
                {
                    // TODO : 

                    var bindatamaker = new BinDataMaker();

                    bindatamaker.SetDutCount(dutCount);
                    bindatamaker.SetBinType(bintype);
                    bindatamaker.SetPrefix("M");

                    retval = bindatamaker.MakeBinInfo();

                    //for (int i = 0; i < dutCount; i++)
                    //{
                    //    int randData = random.Next(1, 100);
                    //    randData = (randData < 85) ? 0 : 1;
                    //    paddingStr += (char)((int)'@' + randData);
                    //}
                }
                else if (bintype == BinType.BIN_6BIT)
                {
                    string tmpStr = string.Empty;
                    int passPercent = 35; // 패스 확률

                    for (int i = 0; i < dutCount; i++)
                    {
                        int randData = random.Next(0, 100); // 여기서 100과 밑의 80은 확률을 표현한것.
                        if (i % 4 == 0)
                        {
                            tmpStr = string.Empty;
                        }

                        randData = randData < passPercent ? 0 : (random.Next(1, 5));

                        tmpStr += (char)((int)'@' + randData);

                        if (dutCount % 4 == 3 || i == (dutCount - 1))
                        {
                            char prePix = '@';

                            for (int j = 0; j < (tmpStr?.Length ?? 0); j++)
                            {
                                if (tmpStr[j] != '@')
                                {
                                    prePix = (char)(((int)prePix) | (1 << j));
                                }
                            }

                            retval = $"M{retval}{prePix}{tmpStr}";
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void Reset()
        {
            // Nothing
        }
    }
}
