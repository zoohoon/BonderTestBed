using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SECS_Host.Help
{
    public class BoolList : List<string>
    {
        public BoolList()
        {
            this.Add(bool.TrueString);
            this.Add(bool.FalseString);
        }
    }
}