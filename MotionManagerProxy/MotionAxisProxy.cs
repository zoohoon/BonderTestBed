using System;
using System.Threading.Tasks;
using System.ServiceModel.Description;
using System.ServiceModel;
namespace MotionManagerProxy
{
    using LogModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class MotionAxisProxy : ClientBase<IMotionManager>, IMotionAxisProxy, INotifyPropertyChanged
    {
        public MotionAxisProxy(string ip, int port, InstanceContext callback) :
                base(callback, new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IMotionManager)),
                    new NetTcpBinding()
                    {
                        SendTimeout = new TimeSpan(0, 5, 0),
                        ReceiveTimeout = TimeSpan.MaxValue,
                        Security = new NetTcpSecurity() { Mode = SecurityMode.None },
                        ReliableSession = new OptionalReliableSession() { InactivityTimeout = TimeSpan.FromMinutes(1), Enabled = true }
                    },
                    new EndpointAddress($"net.tcp://{ip}:{port}/POS/{ServiceAddress.MotionManagerService}")))
        {
            LoggerManager.Debug($"End point address: {this.Endpoint.Address.Uri.AbsoluteUri}");
        }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private ObservableCollection<ProbeAxisObject> _Axes
            = new ObservableCollection<ProbeAxisObject>();
        public ObservableCollection<ProbeAxisObject> Axes
        {
            get { return _Axes; }
            set
            {
                if (value != _Axes)
                {
                    _Axes = value;
                    RaisePropertyChanged();
                }
            }
        }
        private object chnLockObj = new object();
        public bool IsServiceAvailable()
        {
            bool retVal = false;
            try
            {
                lock (chnLockObj)
                {
                    if (IsOpened())
                    {
                        var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                        try
                        {
                            (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                            retVal = Channel.IsServiceAvailable();
                        }
                        catch (Exception)
                        {
                            LoggerManager.Error($"MotionAxisProxy IsServiceAvailable timeout error");
                        }
                        finally
                        {
                            (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                        }
                    }
                    else
                    {
                        LoggerManager.Error($"FileManager Service service error.");
                        retVal = false;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public ProbeAxisObject GetAxis(EnumAxisConstants axis)
        {
            ProbeAxisObject retval = null;

            try
            {
                if (IsOpened())
                {
                    var originOperationTimeout = (Channel as IContextChannel).OperationTimeout;
                    try
                    {
                        (Channel as IContextChannel).OperationTimeout = new TimeSpan(0, 0, 15);
                        retval = Channel.GetAxis(axis);
                    }
                    catch (Exception)
                    {
                        LoggerManager.Error($"MotionAxisProxy GetAxis timeout error");
                    }
                    finally
                    {
                        (Channel as IContextChannel).OperationTimeout = originOperationTimeout;
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void InitAxes()
        {
            try
            {
                Axes.Clear();
                for (int i = 0; i < (int)EnumAxisConstants.MAX_STAGE_AXIS; i++)
                {
                    var axis = GetAxis((EnumAxisConstants)i);
                    if (axis != null) Axes.Add(axis);

                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }
        public int DisableAxes()
        {
            try
            {
                if (IsOpened())
                    return Channel.DisableAxes();
                else
                    return -1;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                return -1;
            }
        }

        public async Task<EventCodeEnum> HomingAsync(ProbeAxisObject axis)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = await Channel.HomingAsync(axis);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public EventCodeEnum LoaderSystemInit()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.LoaderSystemInit();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }
        public async Task<EventCodeEnum> StageSystemInitAsync()
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = await Channel.StageSystemInitAsync();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum RelMove(EnumAxisConstants axis, double pos)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.RelMove(axis, pos);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum RelMove(ProbeAxisObject axis, double pos, double vel, double acc)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.RelMove(axis, pos, vel, acc);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum VMove(EnumAxisConstants axis, double vel, EnumTrjType trjtype)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.VMove(axis, vel, trjtype);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum WaitForAxisMotionDone(EnumAxisConstants axis, long timeout = 0)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.WaitForAxisMotionDone(axis, timeout);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum IsThreeLegUp(EnumAxisConstants axis, ref bool isthreelegup)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.IsThreeLegUp(axis, ref isthreelegup);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum IsThreeLegDown(EnumAxisConstants axis, ref bool isthreelegdn)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.IsThreeLegDown(axis, ref isthreelegdn);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum IsRls(EnumAxisConstants axis, ref bool isrls)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.IsRls(axis, ref isrls);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public EventCodeEnum IsFls(EnumAxisConstants axis, ref bool isfls)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.IsFls(axis, ref isfls);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public void InitService()
        {
            try
            {
                if (IsOpened())
                {
                    Channel.InitHostService();
                }
                IsServiceAvailable();
                InitAxes();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


        public EventCodeEnum CheckSWLimit(EnumAxisConstants axistype, double position)
        {
            EventCodeEnum retVal = EventCodeEnum.UNDEFINED;
            try
            {
                if (IsOpened())
                    retVal = Channel.CheckSWLimit(axistype, position);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retVal;
        }

        public bool IsOpened()
        {
            if (State == CommunicationState.Opened | State == CommunicationState.Created)
                return true;
            else
                return false;
        }
        public CommunicationState GetCommunicationState()
        {
            return this.State;
        }

        public void DeInitService()
        {
            //Dispose
        }

       
    }
}
