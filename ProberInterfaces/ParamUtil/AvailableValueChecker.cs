using System;
using System.Collections.Generic;

namespace ProberInterfaces.ParamUtil
{
    public class AvailableValueChecker
    {
        private static Dictionary<String, ValueCheckEngine> _ValueCheckEngineList = new Dictionary<String, ValueCheckEngine>();

        public ValueCheckEngine GetValueCheckerEngine(String valueCheckSyntax)
        {
            if (_ValueCheckEngineList.ContainsKey(valueCheckSyntax))
                return _ValueCheckEngineList[valueCheckSyntax];

            ValueCheckEngine checkEngine = null;
            const char tildChar = '~';
            const char commaChar = ',';
            const char slashChar = '/';
            int tildeIdx = valueCheckSyntax.IndexOf(tildChar);
            int commaIdx = valueCheckSyntax.IndexOf(commaChar);
            int slashIdx = valueCheckSyntax.IndexOf(slashChar);

            if (tildeIdx != -1)
                checkEngine = new RangeValueCheckEngine(valueCheckSyntax, tildChar);
            else if (commaIdx != -1)
                checkEngine = new CommaValueCheckEngine(valueCheckSyntax, commaChar);
            else if (slashIdx != -1)
                checkEngine = new BoolValueCheckEngine(valueCheckSyntax, slashChar);
            else
                checkEngine = new LengthValueCheckEngine(valueCheckSyntax);

            if (checkEngine.Parsing() == false)
                return null;

            _ValueCheckEngineList.Add(valueCheckSyntax, checkEngine);


            return checkEngine;
        }
    }
}
