using SECS_Host.Help;
using SECS_Host.Model;
using SECS_Host.SecsMsgClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using XComPro.Library;

namespace SECS_Host.View_Model
{
    public class VM_DialogTerminalDisplay : ViewModelBase
    {
        public delegate void CloseDelegate(object sender, System.EventArgs e);

        private XComProW m_XComPro;
        public SecsMsgBase secsMsg;

        public CloseDelegate CloseWindow;
        public AddLogDelegate AddLog;

        private String _TerminalSendTxt;
        public String TerminalSendTxt
        {
            get { return _TerminalSendTxt; }
            set
            {
                if(_TerminalSendTxt != value)
                {
                    _TerminalSendTxt = value;
                    NotifyPropertyChange(nameof(TerminalSendTxt));
                }
            }
        }

        public VM_DialogTerminalDisplay(XComProW m_XComPro)
        {
            this.m_XComPro = m_XComPro;
            _TerminalSendTxt = "";
        }

        private ICommand _SendTerminalDisplayCommand;
        public ICommand SendTerminalDisplayCommand
        {
            get
            {
                if (_SendTerminalDisplayCommand == null)
                    _SendTerminalDisplayCommand = new RelayCommand(SendTerminalDisplay);
                return _SendTerminalDisplayCommand;
            }
        }
        public void SendTerminalDisplay(object obj)
        {
            String[] splitTxt = TerminalSendTxt.Split(new char[]{'\r','\n'},
                                StringSplitOptions.RemoveEmptyEntries);

            if(splitTxt.Length <= 0)
            {
                AddLog("SendTerminalDisplay : Don't Write Text in Textbox");
                return;
            }

            SECSGenericData<List<SECSData>> SendData
                = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };
            SendData.Value.Add(new SECSGenericData<byte>() { Value = 0, MsgType = SECS_DataTypeCategory.BINARY });



            if (splitTxt.Length == 1)
            {
                SendData.Value.Add(new SECSGenericData<string>() { Value = splitTxt[0], MsgType = SECS_DataTypeCategory.ASCII });
                SetSecsMsg(new SecsMsg_S10F3());
                secsMsg.SetParameter(0, 0, 10, 3, 0, 0);
            }
            else if (1 < splitTxt.Length)
            {
                SECSGenericData<List<SECSData>> elementList
                    = new SECSGenericData<List<SECSData>>() { Value = new List<SECSData>(), MsgType = SECS_DataTypeCategory.LIST };
                foreach (var v in splitTxt)
                {
                    elementList.Value.Add(new SECSGenericData<string>() { Value = v, MsgType = SECS_DataTypeCategory.ASCII });
                }
                SendData.Value.Add(elementList);

                SetSecsMsg(new SecsMsg_S10F5());
                secsMsg.SetParameter(0, 0, 10, 5, 0, 0);
            }

            secsMsg.SetSendData(SendData);
            secsMsg.Excute();
        }

        public void SetSecsMsg(SecsMsgBase _secsMsg)
        {
            secsMsg = _secsMsg;
            secsMsg.m_XComPro = m_XComPro;
            secsMsg.AddLog = AddLog;
        }

        public void Dispose()
        {
        }
    }
}
