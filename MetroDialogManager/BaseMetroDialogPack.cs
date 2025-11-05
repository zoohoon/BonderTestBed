using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroDialogModule
{
    public class BaseMetroDialogPack
    {
        private BaseMetroDialog _dialog;
        public BaseMetroDialog dialog
        {
            get { return _dialog; }
            set
            {
                if (value != _dialog)
                {
                    _dialog = value;
                }
            }
        }

        private string _HashCode;
        public string HashCode
        {
            get { return _HashCode; }
            set
            {
                if (value != _HashCode)
                {
                    _HashCode = value;
                }
            }
        }
    }
}
