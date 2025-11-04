using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Serialization;
using ProberInterfaces;
using ProberInterfaces.CardChange;
using ProberErrorCode;
using Newtonsoft.Json;
using LogModule;
using ProberInterfaces.SequenceRunner;
using MetroDialogInterfaces;

namespace SequenceRunner
{
    [Serializable]
    public abstract class SequenceRun : ISequenceBehaviorRun
    {
        public virtual Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            return Task.FromResult<IBehaviorResult>(retVal);
        }

    }

    [Serializable]
    
    public abstract class SequenceSafety : SequenceRun, ISequenceBehaviorSafety
    {
        public SequenceSafety() { }

        private List<IOPortDescripter<bool>> _InputPorts;
        [XmlIgnore, JsonIgnore]
        public List<IOPortDescripter<bool>> InputPorts
        {
            get { return _InputPorts; }
            set { _InputPorts = value; }
        }

        private List<IOPortDescripter<bool>> _OutputPorts;
        [XmlIgnore, JsonIgnore]
        public List<IOPortDescripter<bool>> OutputPorts
        {
            get { return _OutputPorts; }
            set { _OutputPorts = value; }
        }

        public string ParamLabel { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }


        public virtual int InitModule()
        {
            int retVal = 0;
            try
            {
                _InputPorts = new List<IOPortDescripter<bool>>();
                _OutputPorts = new List<IOPortDescripter<bool>>();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            return Task.FromResult<IBehaviorResult>(retVal);
        }

        #region << Clone & ToString >>
        public override string ToString()
        {
            return this.GetType().Name.ToString();
        }

        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
        #endregion

        protected ICardChangeSysParam GetSysParam()
        {
            ICardChangeSysParam ccParam = this.CardChangeModule().CcSysParams_IParam as ICardChangeSysParam;
            return ccParam;
        }
        protected ICardChangeDevParam GetDevParam()
        {
            ICardChangeDevParam ccParam = this.CardChangeModule().CcDevParams_IParam as ICardChangeDevParam;
            return ccParam;
        }

        protected Task<bool> IsFrontDoorOpenEx(bool bshowMsg = true)
        {
            bool retVal = false;

            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIFRONTDOORCLOSE");
                IOPortDescripter<bool> DIFRONTDOORCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                _propertyInfo = _type.GetProperty("DIFRONTDOOROPEN");
                IOPortDescripter<bool> DIFRONTDOOROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);

                bool bDIFRONTDOORCLOSE = false, bDIFRONTDOOROPEN = false;

                this.IOManager().IOServ.ReadBit(DIFRONTDOORCLOSE, out bDIFRONTDOORCLOSE);

                if (bDIFRONTDOORCLOSE == false)
                {
                    if (ccDevParam.FrontDoorOpenSensorAttached.Value == true)
                    {
                        this.IOManager().IOServ.ReadBit(DIFRONTDOOROPEN, out bDIFRONTDOOROPEN);
                        if (bDIFRONTDOOROPEN == true)
                        {
                            retVal = true;
                        }
                        else
                        {
                            retVal = false;
                        }
                    }
                    else
                    {
                        retVal = true;
                    }
                }
                else
                {
                    retVal = false;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<bool>(retVal);
        }
    }

    [Serializable]
    public class PlateMoveOutBeforeSwingDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDMOVEINDONE");
                IOPortDescripter<bool> DICARDMOVEINDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEINDONE);

                _propertyInfo = _type.GetProperty("DICARDMOVEOUTDONE");
                IOPortDescripter<bool> DICARDMOVEOUTDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEOUTDONE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DICARDMOVEINDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEINDONE"));
                IOPortDescripter<bool> DICARDMOVEOUTDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEOUTDONE"));
                bool bDICARDMOVEOUTDONE = false, bDICARDMOVEINDONE = false;

                this.IOManager().IOServ.ReadBit(DICARDMOVEINDONE, out bDICARDMOVEINDONE);
                this.IOManager().IOServ.ReadBit(DICARDMOVEOUTDONE, out bDICARDMOVEOUTDONE);
                if (bDICARDMOVEINDONE == true || bDICARDMOVEOUTDONE == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Swing Down Failed!\r\n");
                    sb.Append("Must be plate move out before swing down!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;
                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class PlateEmptyBeforeSwingDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICardOnPlateL");
                IOPortDescripter<bool> DICardOnPlateL = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICardOnPlateL);

                _propertyInfo = _type.GetProperty("DICardOnPlateR");
                IOPortDescripter<bool> DICardOnPlateR = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICardOnPlateR);

                _propertyInfo = _type.GetProperty("DICardOnPlateM");
                IOPortDescripter<bool> DICardOnPlateM = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICardOnPlateM);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                IOPortDescripter<bool> DICardOnPlateL = InputPorts.Find(io => io.Key.Value.Equals("DICardOnPlateL"));
                IOPortDescripter<bool> DICardOnPlateR = InputPorts.Find(io => io.Key.Value.Equals("DICardOnPlateR"));
                IOPortDescripter<bool> DICardOnPlateM = InputPorts.Find(io => io.Key.Value.Equals("DICardOnPlateM"));
                bool bDICardOnPlateL = false, bDICardOnPlateR = false, bDICardOnPlateM = false;

                this.IOManager().IOServ.ReadBit(DICardOnPlateL, out bDICardOnPlateL);
                this.IOManager().IOServ.ReadBit(DICardOnPlateR, out bDICardOnPlateR);

                if (bDICardOnPlateL == true || bDICardOnPlateR == true ||
                    (ccDevParam.AddedPCDetectEXSensorOnPlate.Value == true && bDICardOnPlateM == true))
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Swing Down Failed!\r\n");
                    sb.Append("Must be plate empty before swing down!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CarrierDownBeforeSwingDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);
                if (bDICARDHOLDERCLOSE == true || bDICARDHOLDEROPEN == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Swing Down Failed!\r\n");
                    sb.Append("Must be Carrier Down before swing down!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckCarrierupBeforeSwingdown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);

                if (bDICARDHOLDERCLOSE == false || bDICARDHOLDEROPEN == true)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier is not up!\r\n");
                    sb.Append("Must be Carrier up!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class PlateMoveOutOnSwingUpDN : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDMOVEINDONE");
                IOPortDescripter<bool> DICARDMOVEINDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEINDONE);

                _propertyInfo = _type.GetProperty("DICARDMOVEOUTDONE");
                IOPortDescripter<bool> DICARDMOVEOUTDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEOUTDONE);

                _type = this.IOManager().IO.Outputs.GetType();
                _propertyInfo = _type.GetProperty("DOCCMoveInMotorEnable");
                IOPortDescripter<bool> DOCCMoveInMotorEnable = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);
                OutputPorts.Add(DOCCMoveInMotorEnable);

                _propertyInfo = _type.GetProperty("DOCCMoveOutMotorEnable");
                IOPortDescripter<bool> DOCCMoveOutMotorEnable = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Outputs);
                OutputPorts.Add(DOCCMoveOutMotorEnable);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDMOVEINDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEINDONE"));
                IOPortDescripter<bool> DICARDMOVEOUTDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEOUTDONE"));
                IOPortDescripter<bool> DOCCMoveInMotorEnable = OutputPorts.Find(io => io.Key.Value.Equals("DOCCMoveInMotorEnable"));
                IOPortDescripter<bool> DOCCMoveOutMotorEnable = OutputPorts.Find(io => io.Key.Value.Equals("DOCCMoveOutMotorEnable"));
                bool bDICARDMOVEINDONE = false, bDICARDMOVEOUTDONE = false;

                this.IOManager().IOServ.ReadBit(DICARDMOVEINDONE, out bDICARDMOVEINDONE);
                this.IOManager().IOServ.ReadBit(DICARDMOVEOUTDONE, out bDICARDMOVEOUTDONE);

                if (bDICARDMOVEINDONE != false || bDICARDMOVEOUTDONE != true)
                {
                    this.IOManager().IOServ.WriteBit(DOCCMoveInMotorEnable, false);
                    this.IOManager().IOServ.WriteBit(DOCCMoveOutMotorEnable, true);
                }

                retVal.ErrorCode = EventCodeEnum.NONE;


            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class FrontDoorOpenBeforeSwingUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                if (await IsFrontDoorOpenEx() == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Swing Up Failed!\r\n");
                    sb.Append("Must be front door open before swing up!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class InnerCoverOpenBeforeSwingUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIDD_FRONT_INNER_COVER_OPEN");
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIDD_FRONT_INNER_COVER_OPEN);

                _propertyInfo = _type.GetProperty("DIDD_FRONT_INNER_COVER_CLOSE");
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIDD_FRONT_INNER_COVER_CLOSE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DIDD_FRONT_INNER_COVER_OPEN"));
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DIDD_FRONT_INNER_COVER_CLOSE"));
                bool bDIDD_FRONT_INNER_COVER_OPEN = false, bDIDD_FRONT_INNER_COVER_CLOSE = false;

                this.IOManager().IOServ.ReadBit(DIDD_FRONT_INNER_COVER_OPEN, out bDIDD_FRONT_INNER_COVER_OPEN);
                this.IOManager().IOServ.ReadBit(DIDD_FRONT_INNER_COVER_CLOSE, out bDIDD_FRONT_INNER_COVER_CLOSE);

                if (bDIDD_FRONT_INNER_COVER_OPEN == false || bDIDD_FRONT_INNER_COVER_CLOSE == true)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Swing Up Failed!\r\n");
                    sb.Append("Must be inner cover open before swing up!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckCarrierUpBeforeSwingUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);

                if (bDICARDHOLDERCLOSE == false || bDICARDHOLDEROPEN == true)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier is not up!\r\n");
                    sb.Append("Must be Carrier up!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckCleanUnitDownBeforeSwingUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICLEANUNITUP_0");
                IOPortDescripter<bool> DICLEANUNITUP_0 = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICLEANUNITUP_0);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICLEANUNITUP_0 = InputPorts.Find(io => io.Key.Value.Equals("DICLEANUNITUP_0"));
                bool bDICLEANUNITUP_0 = false;

                this.IOManager().IOServ.ReadBit(DICLEANUNITUP_0, out bDICLEANUNITUP_0);
                if (bDICLEANUNITUP_0 == false) /*|| gProberIO_IsCleanUnitUp == true*/
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("CleanUnit is not down!\r\n");
                    sb.Append("Must be CleanUnit down!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CarrierDownBeforeSwingUP : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);

                if (bDICARDHOLDERCLOSE == true || bDICARDHOLDEROPEN == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Swing Up Failed!\r\n");
                    sb.Append("Must be Carrier Down before swing up!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }


    [Serializable]
    public class RotateUnlockBeforeCarrierDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_ROT_OPEN");
                IOPortDescripter<bool> DICH_ROT_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_OPEN);

                _propertyInfo = _type.GetProperty("DICH_ROT_CLOSE");
                IOPortDescripter<bool> DICH_ROT_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_CLOSE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICH_ROT_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_OPEN"));
                IOPortDescripter<bool> DICH_ROT_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_CLOSE"));
                bool bDICH_ROT_OPEN = false, bDICH_ROT_CLOSE = false;

                this.IOManager().IOServ.ReadBit(DICH_ROT_OPEN, out bDICH_ROT_OPEN);
                this.IOManager().IOServ.ReadBit(DICH_ROT_CLOSE, out bDICH_ROT_CLOSE);

                if (bDICH_ROT_CLOSE == true || bDICH_ROT_OPEN == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier Down Failed!\r\n");
                    sb.Append("Must be rotate unlock before carrier down!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckHeadUnlockBeforeCarrierUpDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIZIF_UNLOCK);

                _propertyInfo = _type.GetProperty("DICLP_LOCK");
                IOPortDescripter<bool> DICLP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICLP_LOCK);

                _propertyInfo = _type.GetProperty("DICLP_UNLOCK");
                IOPortDescripter<bool> DICLP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICLP_UNLOCK);

                _propertyInfo = _type.GetProperty("DITH_LOCK");
                IOPortDescripter<bool> DITH_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_LOCK);

                _propertyInfo = _type.GetProperty("DITH_UNLOCK");
                IOPortDescripter<bool> DITH_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_UNLOCK);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DIZIF_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DIZIF_UNLOCK"));
                IOPortDescripter<bool> DICLP_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DICLP_LOCK"));
                IOPortDescripter<bool> DICLP_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DICLP_UNLOCK"));
                IOPortDescripter<bool> DITH_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_LOCK"));
                IOPortDescripter<bool> DITH_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_UNLOCK"));
                bool bDIZIF_UNLOCK = false, bDICLP_LOCK = false, bDICLP_UNLOCK = false, bDITH_LOCK = false, bDITH_UNLOCK = false;

                this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out bDIZIF_UNLOCK);
                this.IOManager().IOServ.ReadBit(DICLP_LOCK, out bDICLP_LOCK);
                this.IOManager().IOServ.ReadBit(DICLP_UNLOCK, out bDICLP_UNLOCK);
                this.IOManager().IOServ.ReadBit(DITH_LOCK, out bDITH_LOCK);
                this.IOManager().IOServ.ReadBit(DITH_UNLOCK, out bDITH_UNLOCK);

                if (bDIZIF_UNLOCK == false || bDICLP_LOCK == false || bDICLP_UNLOCK == false || bDITH_LOCK == false || bDITH_UNLOCK == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier Down Failed!\r\n");
                    sb.Append("Probe card is locked on tester head!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class RotateUnlockBeforeCarrierUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_ROT_CLOSE");
                IOPortDescripter<bool> DICH_ROT_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_CLOSE);

                _propertyInfo = _type.GetProperty("DICH_ROT_OPEN");
                IOPortDescripter<bool> DICH_ROT_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_OPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DICH_ROT_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_CLOSE"));
                IOPortDescripter<bool> DICH_ROT_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_OPEN"));
                bool bDICH_ROT_CLOSE = false, bDICH_ROT_OPEN = false;

                this.IOManager().IOServ.ReadBit(DICH_ROT_CLOSE, out bDICH_ROT_CLOSE);
                this.IOManager().IOServ.ReadBit(DICH_ROT_OPEN, out bDICH_ROT_OPEN);

                if (bDICH_ROT_CLOSE == true || bDICH_ROT_OPEN == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier Up Failed!\r\n");
                    sb.Append("Must be rotate unlock before carrier up!");
 
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class SwingUpBeforeCarrierUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIPlateSwingUpPOS");
                IOPortDescripter<bool> DIPlateSwingUpPOS = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIPlateSwingUpPOS);

                _propertyInfo = _type.GetProperty("DIPlateSwingDownPOS");
                IOPortDescripter<bool> DIPlateSwingDownPOS = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIPlateSwingDownPOS);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DIPlateSwingUpPOS = InputPorts.Find(io => io.Key.Value.Equals("DIPlateSwingUpPOS"));
                IOPortDescripter<bool> DIPlateSwingDownPOS = InputPorts.Find(io => io.Key.Value.Equals("DIPlateSwingDownPOS"));
                bool bDIPlateSwingUpPOS = false, bDIPlateSwingDownPOS = false;

                this.IOManager().IOServ.ReadBit(DIPlateSwingUpPOS, out bDIPlateSwingUpPOS);
                this.IOManager().IOServ.ReadBit(DIPlateSwingDownPOS, out bDIPlateSwingDownPOS);

                if (bDIPlateSwingUpPOS == false || bDIPlateSwingDownPOS == true)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier Up Failed!\r\n");
                    sb.Append("Must be swing up before carrier up!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class PlateMoveInBeforeCarrierUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDMOVEINDONE");
                IOPortDescripter<bool> DICARDMOVEINDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEINDONE);

                _propertyInfo = _type.GetProperty("DICARDMOVEOUTDONE");
                IOPortDescripter<bool> DICARDMOVEOUTDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEOUTDONE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDMOVEINDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEINDONE"));
                IOPortDescripter<bool> DICARDMOVEOUTDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEOUTDONE"));
                bool bDICARDMOVEINDONE = false, bDICARDMOVEOUTDONE = false;

                this.IOManager().IOServ.ReadBit(DICARDMOVEINDONE, out bDICARDMOVEINDONE);
                this.IOManager().IOServ.ReadBit(DICARDMOVEOUTDONE, out bDICARDMOVEOUTDONE);

                if (bDICARDMOVEINDONE == false || bDICARDMOVEOUTDONE == true)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier Up Failed!\r\n");
                    sb.Append("Must be plate move in before carrier up!");
 
                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }


    [Serializable]
    public class TubDownBeforeCarrierUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_TUB_OPEN");
                IOPortDescripter<bool> DICH_TUB_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_TUB_OPEN);

                _propertyInfo = _type.GetProperty("DICH_TUB_CLOSE");
                IOPortDescripter<bool> DICH_TUB_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_TUB_CLOSE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICH_TUB_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DICH_TUB_OPEN"));
                IOPortDescripter<bool> DICH_TUB_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICH_TUB_CLOSE"));
                bool bDICH_TUB_OPEN = false, bDICH_TUB_CLOSE = false;

                this.IOManager().IOServ.ReadBit(DICH_TUB_OPEN, out bDICH_TUB_OPEN);
                this.IOManager().IOServ.ReadBit(DICH_TUB_CLOSE, out bDICH_TUB_CLOSE);

                if (bDICH_TUB_CLOSE == true || bDICH_TUB_OPEN == false)
                {
                    retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                    StringBuilder sb = new StringBuilder();
                    sb.Append("Carrier Up Failed!\r\n");
                    sb.Append("Tub Up : ");
                    sb.Append(bDICH_TUB_CLOSE == true ? "Yes\r\n" : "No\r\n");
                    sb.Append("Tub Down : ");
                    sb.Append(bDICH_TUB_OPEN == true ? "Yes\r\n" : "No\r\n");
                    sb.Append("Must be Tub down before carrier up !");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class TubDownBeforeCarrierUpCardHolderOnPlate : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICardOnPlateL");
                IOPortDescripter<bool> DICardOnPlateL = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICardOnPlateL);

                _propertyInfo = _type.GetProperty("DICardOnPlateR");
                IOPortDescripter<bool> DICardOnPlateR = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICardOnPlateR);

                _type = this.IOManager().IO.Inputs.GetType();
                _propertyInfo = _type.GetProperty("DICH_TUB_OPEN");
                IOPortDescripter<bool> DICH_TUB_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_TUB_OPEN);

                _propertyInfo = _type.GetProperty("DICH_TUB_CLOSE");
                IOPortDescripter<bool> DICH_TUB_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_TUB_CLOSE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICardOnPlateL = InputPorts.Find(io => io.Key.Value.Equals("DICardOnPlateL"));
                IOPortDescripter<bool> DICardOnPlateR = InputPorts.Find(io => io.Key.Value.Equals("DICardOnPlateR"));
                IOPortDescripter<bool> DICH_TUB_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DICH_TUB_OPEN"));
                IOPortDescripter<bool> DICH_TUB_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICH_TUB_CLOSE"));
                bool bDICardOnPlateL = false, bDICardOnPlateR = false;
                bool bDICH_TUB_OPEN = false, bDICH_TUB_CLOSE = false;

                this.IOManager().IOServ.ReadBit(DICardOnPlateL, out bDICardOnPlateL);
                this.IOManager().IOServ.ReadBit(DICardOnPlateR, out bDICardOnPlateR);

                if (bDICardOnPlateL == true || bDICardOnPlateR == false)
                {
                    this.IOManager().IOServ.ReadBit(DICH_TUB_OPEN, out bDICH_TUB_OPEN);
                    this.IOManager().IOServ.ReadBit(DICH_TUB_CLOSE, out bDICH_TUB_CLOSE);
                    if (bDICH_TUB_CLOSE == true || bDICH_TUB_OPEN == false)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Carrier Up Failed!\r\n");
                        sb.Append("Tub Up : ");
                        sb.Append(bDICH_TUB_CLOSE == true ? "Yes\r\n" : "No\r\n");
                        sb.Append("Tub Down : ");
                        sb.Append(bDICH_TUB_OPEN == true ? "Yes\r\n" : "No\r\n");
                        sb.Append("Must be Tub down before carrier up when Card holder exists on plate !");

                        EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                        switch (result)
                        {
                            case EnumMessageDialogResult.AFFIRMATIVE:
                                retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                                break;
                        }
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CarrierUpBeforeStageMove : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);


                if (bDICARDHOLDERCLOSE == false || bDICARDHOLDEROPEN == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Stage Can not Move!\r\n");
                    sb.Append("Must be carrier up before stage moving!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CarrierDownBeforeStageMove : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);

                if (bDICARDHOLDERCLOSE == true || bDICARDHOLDEROPEN == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Stage Can not Move!\r\n");
                    sb.Append("Must be carrier Down before stage moving!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");
                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckCardTrayUnLockBeforeCarrierMoveIn : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDTRAY_LOCK");
                IOPortDescripter<bool> DICARDTRAY_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDTRAY_LOCK);

                _propertyInfo = _type.GetProperty("DICARDTRAY_UNLOCK");
                IOPortDescripter<bool> DICARDTRAY_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDTRAY_UNLOCK);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDTRAY_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DICARDTRAY_LOCK"));
                IOPortDescripter<bool> DICARDTRAY_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DICARDTRAY_UNLOCK"));
                bool bDICARDTRAY_LOCK = false, bDICARDTRAY_UNLOCK = false;

                this.IOManager().IOServ.ReadBit(DICARDTRAY_LOCK, out bDICARDTRAY_LOCK);
                this.IOManager().IOServ.ReadBit(DICARDTRAY_UNLOCK, out bDICARDTRAY_UNLOCK);

                if (bDICARDTRAY_LOCK == true || bDICARDTRAY_UNLOCK == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Card tray is Lock!\r\n");
                    sb.Append("Must be Card tray UnLock!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class InnerCoverCloseBeforeStageMove : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIDD_FRONT_INNER_COVER_OPEN");
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIDD_FRONT_INNER_COVER_OPEN);

                _propertyInfo = _type.GetProperty("DIDD_FRONT_INNER_COVER_CLOSE");
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIDD_FRONT_INNER_COVER_CLOSE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DIDD_FRONT_INNER_COVER_OPEN"));
                IOPortDescripter<bool> DIDD_FRONT_INNER_COVER_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DIDD_FRONT_INNER_COVER_CLOSE"));
                bool bDIDD_FRONT_INNER_COVER_OPEN = false, bDIDD_FRONT_INNER_COVER_CLOSE = false;

                this.IOManager().IOServ.ReadBit(DIDD_FRONT_INNER_COVER_OPEN, out bDIDD_FRONT_INNER_COVER_OPEN);
                this.IOManager().IOServ.ReadBit(DIDD_FRONT_INNER_COVER_CLOSE, out bDIDD_FRONT_INNER_COVER_CLOSE);

                if (bDIDD_FRONT_INNER_COVER_OPEN == true || bDIDD_FRONT_INNER_COVER_CLOSE == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Stage Can not Move!\r\n");
                    sb.Append("Must be inner cover close before stage moving!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class PlateMoveOutBeforeStageMove : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDMOVEINDONE");
                IOPortDescripter<bool> DICARDMOVEINDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEINDONE);

                _propertyInfo = _type.GetProperty("DICARDMOVEOUTDONE");
                IOPortDescripter<bool> DICARDMOVEOUTDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEOUTDONE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDMOVEINDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEINDONE"));
                IOPortDescripter<bool> DICARDMOVEOUTDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEOUTDONE"));
                bool bDICARDMOVEINDONE = false, bDICARDMOVEOUTDONE = false;

                this.IOManager().IOServ.ReadBit(DICARDMOVEINDONE, out bDICARDMOVEINDONE);
                this.IOManager().IOServ.ReadBit(DICARDMOVEOUTDONE, out bDICARDMOVEOUTDONE);

                if (bDICARDMOVEINDONE == true || bDICARDMOVEOUTDONE == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Stage Can not Move!\r\n");
                    sb.Append("Must be plate move out before stage moving!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class RotateLockBeforeStageMove : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_ROT_CLOSE");
                IOPortDescripter<bool> DICH_ROT_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_CLOSE);

                _propertyInfo = _type.GetProperty("DICH_ROT_OPEN");
                IOPortDescripter<bool> DICH_ROT_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_OPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DICH_ROT_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_CLOSE"));
                IOPortDescripter<bool> DICH_ROT_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_OPEN"));
                bool bDICH_ROT_CLOSE = false, bDICH_ROT_OPEN = false;

                this.IOManager().IOServ.ReadBit(DICH_ROT_CLOSE, out bDICH_ROT_CLOSE);
                this.IOManager().IOServ.ReadBit(DICH_ROT_OPEN, out bDICH_ROT_OPEN);


                if (bDICH_ROT_CLOSE == false || bDICH_ROT_OPEN == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Stage Can not Move!\r\n");
                    sb.Append("Must be rotate lock before stage moving!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class TubUpBeforeStageMove : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_TUB_OPEN");
                IOPortDescripter<bool> DICH_TUB_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_TUB_OPEN);

                _propertyInfo = _type.GetProperty("DICH_TUB_CLOSE");
                IOPortDescripter<bool> DICH_TUB_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_TUB_CLOSE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICH_TUB_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DICH_TUB_OPEN"));
                IOPortDescripter<bool> DICH_TUB_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICH_TUB_CLOSE"));
                bool bDICH_TUB_OPEN = false, bDICH_TUB_CLOSE = false;

                this.IOManager().IOServ.ReadBit(DICH_TUB_OPEN, out bDICH_TUB_OPEN);
                this.IOManager().IOServ.ReadBit(DICH_TUB_CLOSE, out bDICH_TUB_CLOSE);

                if (bDICH_TUB_CLOSE == false || bDICH_TUB_OPEN == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Stage Can not Move!\r\n");
                    sb.Append("Must be tub up before stage moving!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class StageMoveBackBeforePlateMoveIN : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                ICardChangeSysParam ccSysParam = GetSysParam();
                ICardChangeDevParam ccDevParam = GetDevParam();

                //SACCOffSetY = c_Lng(GetIniItem(PATH_PARAMETER + "Machine Settings" + ".cfg.ini", "DATA2", "SACCOffSetY", "0", True))

                ProbeAxisObject axisY = this.MotionManager().GetAxis(EnumAxisConstants.Y);
                double yActualPos = 0;
                double yRefPos = 0;

                this.MotionManager().GetActualPos(EnumAxisConstants.Y, ref yActualPos);
                this.MotionManager().GetRefPos(EnumAxisConstants.Y, ref yRefPos);

                if (yActualPos + ccDevParam.SACCOffSetY.Value > yRefPos + 500)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Plate Move In Failed!\r\n");
                    sb.Append("Must be stage place to backside before plate move in!!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class SwingUpBeforePlateMoveIn : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIPlateSwingUpPOS");
                IOPortDescripter<bool> DIPlateSwingUpPOS = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIPlateSwingUpPOS);

                _propertyInfo = _type.GetProperty("DIPlateSwingDownPOS");
                IOPortDescripter<bool> DIPlateSwingDownPOS = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIPlateSwingDownPOS);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DIPlateSwingUpPOS = InputPorts.Find(io => io.Key.Value.Equals("DIPlateSwingUpPOS"));
                IOPortDescripter<bool> DIPlateSwingDownPOS = InputPorts.Find(io => io.Key.Value.Equals("DIPlateSwingDownPOS"));
                bool bDIPlateSwingUpPOS = false, bDIPlateSwingDownPOS = false;

                this.IOManager().IOServ.ReadBit(DIPlateSwingUpPOS, out bDIPlateSwingUpPOS);
                this.IOManager().IOServ.ReadBit(DIPlateSwingDownPOS, out bDIPlateSwingDownPOS);

                if (bDIPlateSwingUpPOS == false || bDIPlateSwingDownPOS == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Plate Move In Failed!\r\n");
                    sb.Append("Must be swing up before plate move in!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CarrierDownBeforeMoveIn : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);

                if (bDICARDHOLDERCLOSE == true || bDICARDHOLDEROPEN == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Plate Move In Failed!\r\n");
                    sb.Append("Must be Carrier Down before plate move in!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CarrierDownBeforeMoveOut : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);

                if (bDICARDHOLDERCLOSE == true || bDICARDHOLDEROPEN == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Plate Move Out Failed!\r\n");
                    sb.Append("Must be carrier down before plate move out!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class FrontDoorOpenBeforePlateMoveOut : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                if (await IsFrontDoorOpenEx() == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Plate Move Out Failed!\r\n");
                    sb.Append("Must be front door open before plate move out!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class PlateMoveInBeforeTubDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDMOVEINDONE");
                IOPortDescripter<bool> DICARDMOVEINDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEINDONE);

                _propertyInfo = _type.GetProperty("DICARDMOVEOUTDONE");
                IOPortDescripter<bool> DICARDMOVEOUTDONE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDMOVEOUTDONE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DICARDMOVEINDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEINDONE"));
                IOPortDescripter<bool> DICARDMOVEOUTDONE = InputPorts.Find(io => io.Key.Value.Equals("DICARDMOVEOUTDONE"));
                bool bDICARDMOVEOUTDONE = false, bDICARDMOVEINDONE = false;

                this.IOManager().IOServ.ReadBit(DICARDMOVEINDONE, out bDICARDMOVEINDONE);
                this.IOManager().IOServ.ReadBit(DICARDMOVEOUTDONE, out bDICARDMOVEOUTDONE);

                if (bDICARDMOVEINDONE == false || bDICARDMOVEOUTDONE == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Tub Down Failed!\r\n");
                    sb.Append("Plate In : ");
                    sb.Append(bDICARDMOVEINDONE == true ? "Yes\r\n" : "No\r\n");
                    sb.Append("Plate Out : ");
                    sb.Append(bDICARDMOVEOUTDONE == true ? "Yes\r\n" : "No\r\n");
                    sb.Append("Must be plate move in before tub down!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class ConfirmCardHolderBeforeTubDownInCarrierDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICARDHOLDERCLOSE");
                IOPortDescripter<bool> DICARDHOLDERCLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDERCLOSE);

                _propertyInfo = _type.GetProperty("DICARDHOLDEROPEN");
                IOPortDescripter<bool> DICARDHOLDEROPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICARDHOLDEROPEN);

                _propertyInfo = _type.GetProperty("DICardOnPlateL");
                IOPortDescripter<bool> DICardOnPlateL = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICardOnPlateL);

                _propertyInfo = _type.GetProperty("DICardOnPlateR");
                IOPortDescripter<bool> DICardOnPlateR = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICardOnPlateR);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICARDHOLDERCLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDERCLOSE"));
                IOPortDescripter<bool> DICARDHOLDEROPEN = InputPorts.Find(io => io.Key.Value.Equals("DICARDHOLDEROPEN"));
                IOPortDescripter<bool> DICardOnPlateL = InputPorts.Find(io => io.Key.Value.Equals("DICardOnPlateL"));
                IOPortDescripter<bool> DICardOnPlateR = InputPorts.Find(io => io.Key.Value.Equals("DICardOnPlateR"));
                bool bDICARDHOLDERCLOSE = false, bDICARDHOLDEROPEN = false;
                bool bDICardOnPlateL = false, bDICardOnPlateR = false;

                this.IOManager().IOServ.ReadBit(DICARDHOLDERCLOSE, out bDICARDHOLDERCLOSE);
                this.IOManager().IOServ.ReadBit(DICARDHOLDEROPEN, out bDICARDHOLDEROPEN);
                this.IOManager().IOServ.ReadBit(DICardOnPlateL, out bDICardOnPlateL);
                this.IOManager().IOServ.ReadBit(DICardOnPlateR, out bDICardOnPlateR);

                if (bDICARDHOLDERCLOSE == false || bDICARDHOLDEROPEN == true)
                {
                    if (bDICardOnPlateL == false || bDICardOnPlateR == false)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Tub Down Failed!\r\n");
                        sb.Append("Carrier Up  : ");
                        sb.Append(bDICARDHOLDERCLOSE == true ? "Yes\r\n" : "No\r\n");
                        sb.Append("Carrier Down  : ");
                        sb.Append(bDICARDHOLDEROPEN == true ? "Yes\r\n" : "No\r\n");
                        sb.Append("Plate Left  : ");
                        sb.Append(bDICardOnPlateL == true ? "Yes\r\n" : "No\r\n");
                        sb.Append("Plate Right  : ");
                        sb.Append(bDICardOnPlateR == true ? "Yes\r\n" : "No\r\n");
                        sb.Append("Must be carrier up before tub down!");

                        EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                        switch (result)
                        {
                            case EnumMessageDialogResult.AFFIRMATIVE:
                                retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                                break;
                        }
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckHeadUnlockBeforeTubUpDown : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIZIF_UNLOCK);

                _propertyInfo = _type.GetProperty("DICLP_LOCK");
                IOPortDescripter<bool> DICLP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICLP_LOCK);

                _propertyInfo = _type.GetProperty("DICLP_UNLOCK");
                IOPortDescripter<bool> DICLP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICLP_UNLOCK);

                _propertyInfo = _type.GetProperty("DITH_LOCK");
                IOPortDescripter<bool> DITH_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_LOCK);

                _propertyInfo = _type.GetProperty("DITH_UNLOCK");
                IOPortDescripter<bool> DITH_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_UNLOCK);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DIZIF_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DIZIF_UNLOCK"));
                IOPortDescripter<bool> DICLP_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DICLP_LOCK"));
                IOPortDescripter<bool> DICLP_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DICLP_UNLOCK"));
                IOPortDescripter<bool> DITH_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_LOCK"));
                IOPortDescripter<bool> DITH_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_UNLOCK"));
                bool bDIZIF_UNLOCK = false, bDICLP_LOCK = false;
                bool bDICLP_UNLOCK = false, bDITH_LOCK = false, bDITH_UNLOCK = false;

                this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out bDIZIF_UNLOCK);
                this.IOManager().IOServ.ReadBit(DICLP_LOCK, out bDICLP_LOCK);
                this.IOManager().IOServ.ReadBit(DICLP_UNLOCK, out bDICLP_UNLOCK);
                this.IOManager().IOServ.ReadBit(DITH_LOCK, out bDITH_LOCK);
                this.IOManager().IOServ.ReadBit(DITH_UNLOCK, out bDITH_UNLOCK);

                if (bDIZIF_UNLOCK == false || bDICLP_LOCK == true || bDICLP_UNLOCK == false || bDITH_LOCK == true || bDITH_UNLOCK == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Card Holder Tub Down Failed!\r\n");
                    sb.Append("Probe card is locked on tester head!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class RotateLockBeforeTubUp : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICH_ROT_OPEN");
                IOPortDescripter<bool> DICH_ROT_OPEN = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_OPEN);

                _propertyInfo = _type.GetProperty("DICH_ROT_CLOSE");
                IOPortDescripter<bool> DICH_ROT_CLOSE = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICH_ROT_CLOSE);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DICH_ROT_OPEN = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_OPEN"));
                IOPortDescripter<bool> DICH_ROT_CLOSE = InputPorts.Find(io => io.Key.Value.Equals("DICH_ROT_CLOSE"));
                bool bDICH_ROT_OPEN = false, bDICH_ROT_CLOSE = false;

                this.IOManager().IOServ.ReadBit(DICH_ROT_OPEN, out bDICH_ROT_OPEN);
                this.IOManager().IOServ.ReadBit(DICH_ROT_CLOSE, out bDICH_ROT_CLOSE);

                if (bDICH_ROT_CLOSE == false || bDICH_ROT_OPEN == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Card holder Tub Up Failed!\r\n");
                    sb.Append("Must be rotate lock before tub up!");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckZifLockBeforeClampLock : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DIZIF_UNLOCK");
                IOPortDescripter<bool> DIZIF_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DIZIF_UNLOCK);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {
                IOPortDescripter<bool> DIZIF_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DIZIF_UNLOCK"));
                bool bDIZIF_UNLOCK = false;

                this.IOManager().IOServ.ReadBit(DIZIF_UNLOCK, out bDIZIF_UNLOCK);

                if (bDIZIF_UNLOCK == true)
                {
                    StringBuilder sb = new StringBuilder();
                    //VBTRACE "Operation skipped : ZIF Unlock"

                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckTesterHeadLockBeforeClampLock : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DITH_LOCK");
                IOPortDescripter<bool> DITH_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_LOCK);

                _propertyInfo = _type.GetProperty("DITH_UNLOCK");
                IOPortDescripter<bool> DITH_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_UNLOCK);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DITH_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_LOCK"));
                IOPortDescripter<bool> DITH_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_UNLOCK"));
                bool bDIZIF_LOCK = false, bDITH_UNLOCK = false;

                this.IOManager().IOServ.ReadBit(DITH_LOCK, out bDIZIF_LOCK);
                this.IOManager().IOServ.ReadBit(DITH_UNLOCK, out bDITH_UNLOCK);

                if (bDIZIF_LOCK == false || bDITH_UNLOCK == true)
                {
                    StringBuilder sb = new StringBuilder();
                    //VBTRACE "Operation skipped : Tester Head Unlock"

                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return Task.FromResult<IBehaviorResult>(retVal);
        }
    }

    [Serializable]
    public class CheckTesterHeadUnlockBeforeZifUnlock : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DITH_LOCK");
                IOPortDescripter<bool> DITH_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_LOCK);

                _propertyInfo = _type.GetProperty("DITH_UNLOCK");
                IOPortDescripter<bool> DITH_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DITH_UNLOCK);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DITH_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_LOCK"));
                IOPortDescripter<bool> DITH_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DITH_UNLOCK"));
                bool bDIZIF_LOCK = false, bDITH_UNLOCK = false;

                this.IOManager().IOServ.ReadBit(DITH_LOCK, out bDIZIF_LOCK);
                this.IOManager().IOServ.ReadBit(DITH_UNLOCK, out bDITH_UNLOCK);

                if (bDIZIF_LOCK == true || bDITH_UNLOCK == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("ZIF_Unlock is failed because Tester Head is locked!\r\n");
                    sb.Append("Head_Lock State : ");
                    sb.Append(bDIZIF_LOCK == true ? "Yes\r\n" : "No\r\n");
                    sb.Append("Head_Unlock State : ");
                    sb.Append(bDITH_UNLOCK == true ? "Yes\r\n" : "No\r\n");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

    [Serializable]
    public class CheckClampUnlockBeforeZifUnlock : SequenceSafety
    {
        public override int InitModule()
        {
            int retVal = base.InitModule();
            try
            {

                Type _type = this.IOManager().IO.Inputs.GetType();
                PropertyInfo _propertyInfo = _type.GetProperty("DICLP_LOCK");
                IOPortDescripter<bool> DICLP_LOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICLP_LOCK);

                _propertyInfo = _type.GetProperty("DICLP_UNLOCK");
                IOPortDescripter<bool> DICLP_UNLOCK = (IOPortDescripter<bool>)_propertyInfo.GetValue(this.IOManager().IO.Inputs);
                InputPorts.Add(DICLP_UNLOCK);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }

        public override async Task<IBehaviorResult> Run()
        {
            IBehaviorResult retVal = new BehaviorResult();
            try
            {

                IOPortDescripter<bool> DICLP_LOCK = InputPorts.Find(io => io.Key.Value.Equals("DICLP_LOCK"));
                IOPortDescripter<bool> DICLP_UNLOCK = InputPorts.Find(io => io.Key.Value.Equals("DICLP_UNLOCK"));
                bool bDICLP_LOCK = false, bDICLP_UNLOCK = false;

                this.IOManager().IOServ.ReadBit(DICLP_LOCK, out bDICLP_LOCK);
                this.IOManager().IOServ.ReadBit(DICLP_UNLOCK, out bDICLP_UNLOCK);

                if (bDICLP_LOCK == true || bDICLP_UNLOCK == false)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("ZIF_Unlock is failed because Clamp is locked!\r\n");
                    sb.Append("Clamp_Lock State : ");
                    sb.Append(bDICLP_LOCK == true ? "Yes\r\n" : "No\r\n");
                    sb.Append("Clamp_Unlock State : ");
                    sb.Append(bDICLP_UNLOCK == true ? "Yes\r\n" : "No\r\n");

                    EnumMessageDialogResult result = await this.MetroDialogManager().ShowMessageDialog("Safety Message", sb.ToString(), EnumMessageStyle.Affirmative, "OK", "");

                    switch (result)
                    {
                        case EnumMessageDialogResult.AFFIRMATIVE:
                            retVal.ErrorCode = EventCodeEnum.UNDEFINED;

                            break;
                    }
                }
                else
                {
                    retVal.ErrorCode = EventCodeEnum.NONE;

                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
        }
    }

}
