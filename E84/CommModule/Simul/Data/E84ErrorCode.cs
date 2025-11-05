using LogModule;
using ProberInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace E84
{
    public class E84ErrorCode : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private int _CodeNumber;
        public int CodeNumber
        {
            get { return _CodeNumber; }
            set
            {
                _CodeNumber = value;
                RaisePropertyChanged();
            }
        }

        private string _CodeName;
        public string CodeName
        {
            get { return _CodeName; }
            set
            {
                _CodeName = value;
                RaisePropertyChanged();
            }
        }


        private string _Description;
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                RaisePropertyChanged();
            }
        }

        private E84EventCode _EventCode;
        public E84EventCode EventCode
        {
            get { return _EventCode; }
            set
            {
                _EventCode = value;
                RaisePropertyChanged();
            }
        }

        public E84ErrorCode()
        {

        }
        public E84ErrorCode(int CodeNumber)
        {
            this.CodeNumber = CodeNumber;
        }
        public E84ErrorCode(int CodeNumber, string CodeName, string Description, E84EventCode EventCode)
        {
            try
            {
                this.CodeNumber = CodeNumber;
                this.CodeName = CodeName;
                this.Description = Description;
                this.EventCode = EventCode;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        public E84ErrorCode Copy()
        {
            return new E84ErrorCode
            {
                CodeNumber = this.CodeNumber,
                CodeName = this.CodeName,
                Description = this.Description,
                EventCode = this.EventCode
            };
        }
    }
}
