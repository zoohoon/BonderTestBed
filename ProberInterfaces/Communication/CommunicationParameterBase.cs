using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProberInterfaces.Communication
{
    public class CommunicationParameterBase
    {
		private Element<int> _ModuleIndex = new Element<int>();
		public Element<int> ModuleIndex
		{
			get { return _ModuleIndex; }
			set { _ModuleIndex = value; }
		}

		private Element<bool> _ModuleAttached = new Element<bool>();
		public Element<bool> ModuleAttached
		{
			get { return _ModuleAttached; }
			set { _ModuleAttached = value; }
		}

		private Element<EnumCommmunicationType> _ModuleCommType = new Element<EnumCommmunicationType>();
		public Element<EnumCommmunicationType> ModuleCommType
		{
			get { return _ModuleCommType; }
			set { _ModuleCommType = value; }
		}

		private Element<string> _IP = new Element<string>();
		public Element<string> IP
		{
			get { return _IP; }
			set { _IP = value; }
		}

		private Element<int> _Port = new Element<int>();
		public Element<int> Port
		{
			get { return _Port; }
			set { _Port = value; }
		}

		private Element<int> _BaudRate = new Element<int>();
		public Element<int> BaudRate
		{
			get { return _BaudRate; }
			set { _BaudRate = value; }
		}

		private Element<int> _DataBits = new Element<int>();
		public Element<int> DataBits
		{
			get { return _DataBits; }
			set { _DataBits = value; }
		}

		private Element<StopBits> _StopBits = new Element<StopBits>();
		public Element<StopBits> StopBits
		{
			get { return _StopBits; }
			set { _StopBits = value; }
		}
	
		private Element<string> _PortName = new Element<string>();
		public Element<string> PortName
		{
			get { return _PortName; }
			set { _PortName = value; }
		}

		private Element<string> _Hub = new Element<string>();
		public Element<string> Hub
		{
			get { return _Hub; }
			set { _Hub = value; }
		}

		private Element<int> _IntervalTime = new Element<int>();
		/// <summary>
		/// Milliseconds 
		/// 센서 데이터 업데이트 주기
		/// </summary>
		public Element<int> IntervalTime
        {
            get { return _IntervalTime; }
            set { _IntervalTime = value; }
        }
	}
}
