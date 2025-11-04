using System;

namespace TesterDriverModule
{
    //using Autofac;
    using LogModule;
    using NationalInstruments.NI4882;
    using ProberErrorCode;
    using ProberInterfaces;
    using System.Collections.ObjectModel;
    using GpibStatusFlags = ProberInterfaces.GpibStatusFlags;

    public class NI4882Driver : ITesterComDriver, IFactoryModule
    {
        public Board _Board = null;

        public ObservableCollection<CollectionComponent> CommandCollection { get; set; }

        public EventCodeEnum Connect(object connectparam)
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                int boardIndex = (int)connectparam;

                _Board = new Board(boardIndex);
                _Board.Reset();

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public EventCodeEnum DisConnect()
        {
            EventCodeEnum retval = EventCodeEnum.UNDEFINED;

            try
            {
                _Board.Reset();
                _Board.Dispose();
                _Board = null;

                retval = EventCodeEnum.NONE;
            }
            catch (Exception err)
            {
                retval = EventCodeEnum.EXCEPTION;
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public void Reset()
        {
            _Board.Reset();
        }

        public object GetState()
        {
            object retval = -1;

            try
            {
                if (_Board == null)
                {
                    return retval;
                }

                _Board.Wait(NationalInstruments.NI4882.GpibStatusFlags.None);
                retval = (int)_Board.GetCurrentStatus();
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
            return retval;
        }

        public string Read()
        {
            string tmpBuffer = null;

            try
            {
                while ((((int)GetState() & (int)GpibStatusFlags.END) != (int)GpibStatusFlags.END))
                {
                    string readData = null;
                    readData = _Board.ReadString();
                    tmpBuffer = tmpBuffer + readData;

                    if (readData.Contains("\r\n"))
                    {
                        break;
                    }
                }
            }
            catch (Exception err)
            {
                tmpBuffer = null;
                LoggerManager.Exception(err);
            }

            return tmpBuffer;
        }

        public void WriteSTB(object command)
        {
            // command => int?

            int? STB = null;

            try
            {
                STB = (int?)command;

                //_Board.SerialPollResponseByte = (byte)command;
                _Board.SerialPollResponseByte = byte.Parse(command.ToString());
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
                LoggerManager.Debug($"Try WriteString(): query_command - {query_command}", isInfo: true);
                _Board.Write(query_command);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public void SetState(int state)
        {
            throw new NotImplementedException();
        }

        //public EventCodeEnum StartReceive()
        //{
        //    throw new NotImplementedException();
        //}

        #region temp Code
        //private string GpibStaMsg(int ibsta)
        //{
        //    string result;

        //    result = "";

        //    if ((ibsta & (int)GpibStatuFlags.ERR)   == (int)GpibStatuFlags.ERR)     result = "ERR";             /* Error detected                   */
        //    if ((ibsta & (int)GpibStatuFlags.TIMO)  == (int)GpibStatuFlags.TIMO)    result = result + " TIMO";  /* Timeout                          */
        //    if ((ibsta & (int)GpibStatuFlags.END)   == (int)GpibStatuFlags.END)     result = result + " END";   /* EOI or EOS detected              */
        //    if ((ibsta & (int)GpibStatuFlags.SRQI)  == (int)GpibStatuFlags.SRQI)    result = result + " SRQI";  /* SRQ detected by CIC              */
        //    if ((ibsta & (int)GpibStatuFlags.RQS)   == (int)GpibStatuFlags.RQS)     result = result + " RQS";   /* Device needs service             */
        //    if ((ibsta & (int)GpibStatuFlags.CMPL)  == (int)GpibStatuFlags.CMPL)    result = result + " CMPL";  /* I/O completed                    */
        //    if ((ibsta & (int)GpibStatuFlags.LOK)   == (int)GpibStatuFlags.LOK)     result = result + " LOK";   /* Local lockout state              */
        //    if ((ibsta & (int)GpibStatuFlags.REM)   == (int)GpibStatuFlags.REM)     result = result + " REM";   /* Remote state                     */
        //    if ((ibsta & (int)GpibStatuFlags.CIC)   == (int)GpibStatuFlags.CIC)     result = result + " CIC";   /* Controller-in-Charge             */
        //    if ((ibsta & (int)GpibStatuFlags.ATN)   == (int)GpibStatuFlags.ATN)     result = result + " ATN";   /* Attention asserted               */
        //    if ((ibsta & (int)GpibStatuFlags.TACS)  == (int)GpibStatuFlags.TACS)    result = result + " TACS";  /* Talker active                    */
        //    if ((ibsta & (int)GpibStatuFlags.LACS)  == (int)GpibStatuFlags.LACS)    result = result + " LACS";  /* Listener active                  */
        //    if ((ibsta & (int)GpibStatuFlags.DTAS)  == (int)GpibStatuFlags.DTAS)    result = result + " DTAS";  /* Device trigger state             */
        //    if ((ibsta & (int)GpibStatuFlags.DCAS)  == (int)GpibStatuFlags.DCAS)    result = result + " DCAS";  /* Device clear state               */

        //    return result;
        //}
        #endregion
    }
}
