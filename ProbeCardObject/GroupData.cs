using ProberInterfaces;
using ProberInterfaces.PinAlign.ProbeCardData;
using System;
using System.Collections.Generic;
using System.Linq;
using LogModule;

namespace ProbeCardObject
{
    [Serializable]
    public class GroupData : IGroupData, IFactoryModule
    {
        public PINGROUPALIGNRESULT GroupResult { get; set; }
        public List<int> PinNumList { get; set; }
        public int GroupNum { get; set; }
        public GroupData()
        {
            try
            {
                PinNumList = new List<int>();
                GroupResult = PINGROUPALIGNRESULT.CONTINUE;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public GroupData(GroupData data)
        {
            try
            {
                foreach (int pinNum in data.PinNumList)
                {
                    PinNumList.Add(pinNum);
                }

                GroupResult = data.GroupResult;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        public List<IPinData> GetPinList()
        {
            List<IPinData> pinList = new List<IPinData>();

            try
            {
                List<IPinData> alignPassList = new List<IPinData>();
                List<IPinData> alignNotPerformedList = new List<IPinData>();
                List<IPinData> alignSkipList = new List<IPinData>();
                List<IPinData> alignFailList = new List<IPinData>();

                IEnumerable<IPinData> groupPinList =
                    this.StageSupervisor().ProbeCardInfo.GetPinList().
                    Where(pin => PinNumList.Contains(pin.PinNum.Value));



                foreach (IPinData pinData in groupPinList)
                {
                    if (pinData.Result.Value == PINALIGNRESULT.PIN_PASSED ||
                        pinData.Result.Value == PINALIGNRESULT.PIN_FORCED_PASS)
                    {
                        alignPassList.Add(pinData);
                    }
                    else if (pinData.Result.Value == PINALIGNRESULT.PIN_NOT_PERFORMED)
                    {
                        alignNotPerformedList.Add(pinData);
                    }
                    else if (pinData.Result.Value == PINALIGNRESULT.PIN_SKIP)
                    {
                        alignSkipList.Add(pinData);
                    }
                    else
                    {
                        alignFailList.Add(pinData);
                    }
                }

                pinList.AddRange(alignPassList);
                pinList.AddRange(alignNotPerformedList);
                pinList.AddRange(alignSkipList);
                pinList.AddRange(alignFailList);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return pinList;
        }
    }
}
