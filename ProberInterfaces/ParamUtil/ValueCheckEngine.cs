using LogModule;

using System;
using System.Collections.Generic;

namespace ProberInterfaces.ParamUtil
{
    [Serializable]
    public abstract class ValueCheckEngine
    {
        protected String _ValueCheckSyntax;
        private ValueCheckEngine()
        { }
        public ValueCheckEngine(String valueCheckSyntax)
        {
            _ValueCheckSyntax = valueCheckSyntax;
        }

        public abstract bool Parsing();
        public abstract bool CheckAvailableValue(double value);
    }
    public class RangeValueCheckEngine : ValueCheckEngine
    {
        private readonly char _TildeChar;
        private double _LowerLimit;
        private double _UpperLimit;

        public RangeValueCheckEngine(string valueCheckSyntax, char tildeChar)
            : base(valueCheckSyntax)
        {
            _LowerLimit = double.MinValue;
            _UpperLimit = double.MaxValue;
            _TildeChar = tildeChar;
        }

        public override bool CheckAvailableValue(double value)
        {
            if (_LowerLimit <= value && value <= _UpperLimit)
                return true;
            else
                return false;
        }

        public override bool Parsing()
        {
            String[] split = _ValueCheckSyntax.Split(_TildeChar);
            if (split.Length != 2)
                return false;
            if (double.TryParse(split[0], out _LowerLimit) == false)
                return false;
            if (double.TryParse(split[1], out _UpperLimit) == false)
                return false;

            return true;
        }
    }
    public class CommaValueCheckEngine : ValueCheckEngine
    {
        private readonly char _CommaChar;
        private HashSet<double> _AvailValueSet;

        public CommaValueCheckEngine(string valueCheckSyntax, char commaChar)
            : base(valueCheckSyntax)
        {
            _AvailValueSet = new HashSet<double>();
            _CommaChar = commaChar;
        }

        public override bool CheckAvailableValue(double value)
        {
            if (_AvailValueSet.Count < 1)
                return false;

            if (_AvailValueSet.Contains(value) == false)
                return false;

            return true;
        }

        public override bool Parsing()
        {
            String[] split = _ValueCheckSyntax.Split(_CommaChar);
            double dblValue = 0;

            bool commaParsingResult = true;
            try
            {
            foreach (String item in split)
            {
                if (double.TryParse(item, out dblValue) == false)
                {
                    commaParsingResult = false;
                    break;
                }
                _AvailValueSet.Add(dblValue);
            }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                 throw;
            }
            return commaParsingResult;
        }
    }
    public class LengthValueCheckEngine : ValueCheckEngine
    {
        private int _LengthLimit;

        public LengthValueCheckEngine(string valueCheckSyntax)
            : base(valueCheckSyntax)
        {
            _LengthLimit = int.MaxValue;
        }

        public override bool CheckAvailableValue(double value)
        {
            return value <= _LengthLimit;
        }

        public override bool Parsing()
        {
            int lengthValue = 0;
            if (int.TryParse(_ValueCheckSyntax, out lengthValue) == false)
                return false;

            _LengthLimit = lengthValue;
            return true;
        }
    }
    public class BoolValueCheckEngine : ValueCheckEngine
    {
        private readonly char _SlashChar;
        public BoolValueCheckEngine(string valueCheckSyntax, char slashChar)
            : base(valueCheckSyntax)
        {
            _SlashChar = slashChar;
        }

        public override bool CheckAvailableValue(double value)
        {
            return true;
        }

        public override bool Parsing()
        {
            _ValueCheckSyntax = _ValueCheckSyntax.Trim();
            String truefalseString = true.ToString() + _SlashChar + false.ToString();

            return _ValueCheckSyntax == truefalseString;
        }
    }

}
