using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LogModule;
using SorterPickTester.Net;
using SorterPickTester.Module;
using SorterPickTester.Data;

namespace SorterPickTester.Module
{
    public class PickerSensing
    {
        private PickerData _pickerData;

        public PickerSensing(ref PickerData pickerData)
        {
            _pickerData = pickerData;
        }
    }
}
