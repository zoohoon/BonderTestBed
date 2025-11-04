using LogModule;
using ProberErrorCode;
using System;

namespace ProberInterfaces.ResultMap
{
    public interface IMapDataConverter
    {
        // TODO : 
        // Convert : Base Date를 이용 | Formatter 적용 가능 필요
        // ConvertBack : Parsing 데이터 이용 | Formatter 적용 가능 필요

        /// <summary>
        /// BASE DATA TO MAP
        /// </summary>
        object Convert(object coupler);
        /// <summary>
        /// MAP TO BASE DATA
        /// </summary>
        object ConvertBack(object coupler, object value);
    }

    public class MapDataConverter : IMapDataConverter
    {
        //public double Evaluate(String input)
        //{
        //    String expr = "(" + input + ")";
        //    Stack<String> ops = new Stack<String>();
        //    Stack<Double> vals = new Stack<Double>();

        //    try
        //    {
        //        for (int i = 0; i < expr.Length; i++)
        //        {
        //            String s = expr.Substring(i, 1);
        //            if (s.Equals("(")) { }
        //            else if (s.Equals("+")) ops.Push(s);
        //            else if (s.Equals("-")) ops.Push(s);
        //            else if (s.Equals("*")) ops.Push(s);
        //            else if (s.Equals("/")) ops.Push(s);
        //            else if (s.Equals("sqrt")) ops.Push(s);
        //            else if (s.Equals(")"))
        //            {
        //                int count = ops.Count;
        //                while (count > 0)
        //                {
        //                    String op = ops.Pop();
        //                    double v = vals.Pop();

        //                    if (op.Equals("+")) v = vals.Pop() + v;
        //                    else if (op.Equals("-")) v = vals.Pop() - v;
        //                    else if (op.Equals("*")) v = vals.Pop() * v;
        //                    else if (op.Equals("/")) v = vals.Pop() / v;
        //                    else if (op.Equals("sqrt")) v = Math.Sqrt(v);
        //                    vals.Push(v);

        //                    count--;
        //                }
        //            }
        //            else vals.Push(Double.Parse(s));
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return vals.Pop();
        //}
        //public List<Calculator> GetCalculators(string expression)
        //{
        //    List<Calculator> retval = new List<Calculator>();

        //    try
        //    {
        //        for (int i = 0; i < expression.Length; i++)
        //        {
        //            String s = expression.Substring(i, 1);

        //            if (s.Equals("+"))
        //            {
        //                retval.Add(new Calculator("+"));
        //            }
        //            else if (s.Equals("-"))
        //            {
        //                retval.Add(new Calculator("-"));
        //            }
        //            else if (s.Equals("*"))
        //            {
        //                retval.Add(new Calculator("*"));
        //            }
        //            else if (s.Equals("/"))
        //            {
        //                retval.Add(new Calculator("/"));
        //            }
        //            else if (s.Equals("sqrt"))
        //            {
        //                retval.Add(new Calculator("srqt"));
        //            }
        //        }
        //    }
        //    catch (Exception err)
        //    {
        //        LoggerManager.Exception(err);
        //    }

        //    return retval;
        //}

        public object Convert(object coupler)
        {
            object retval = null;

            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                MapPropertyConnector input = coupler as MapPropertyConnector;

                if (input != null)
                {
                    //List<ComponentNameAndValueBase> datas = input.BaseData;
                    ConnectMethodBase method = input.ConenectMethod;

                    if (method != null)
                    {
                        #region Backup

                        //if (datas.Count == 1)
                        //{
                        //    tmpval = datas.First().PropertyValue;
                        //}
                        //else
                        //{
                        //    //// TODO : 사용 안됨 ?
                        //    //if (!string.IsNullOrEmpty(input.Expression))
                        //    //{
                        //    //    List<double> numbers = new List<double>();

                        //    //    foreach (var item in datas)
                        //    //    {
                        //    //        double number;

                        //    //        if (double.TryParse(item.PropertyValue.ToString(), out number))
                        //    //        {
                        //    //            numbers.Add(number);
                        //    //        }
                        //    //        else
                        //    //        {
                        //    //            throw new Exception("Invalid value.");
                        //    //        }
                        //    //    }

                        //    //    List<Calculator> calculators = GetCalculators(input.Expression);

                        //    //    if (calculators.Count == numbers.Count - 1)
                        //    //    {
                        //    //        int numberIndex = 0;

                        //    //        double nextvalue = numbers[numberIndex];

                        //    //        foreach (var calculator in calculators)
                        //    //        {
                        //    //            calculator.x = nextvalue;
                        //    //            calculator.y = numbers[numberIndex + 1];

                        //    //            ret = calculator.Run();

                        //    //            if (ret == EventCodeEnum.NONE)
                        //    //            {
                        //    //                nextvalue = System.Convert.ToDouble(calculator.Result);
                        //    //                numberIndex++;

                        //    //                tmpval = nextvalue;
                        //    //            }
                        //    //        }
                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        // TODO : Invalid
                        //    //    }
                        //    //}
                        //}
                        #endregion

                        retval = method.PropertyValue;

                        if (input.InverseFormatter != null)
                        {
                            input.InverseFormatter.Argument = retval;
                            ret = input.InverseFormatter.Run();

                            if (ret == EventCodeEnum.NONE)
                            {
                                if (input.InverseFormatter.Result != null)
                                {
                                    retval = input.InverseFormatter.Result.ToString();
                                }
                                else
                                {
                                    retval = string.Empty;
                                }
                            }
                            else
                            {
                                // ???
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }

        public object ConvertBack(object coupler, object value)
        {
            object retval = null;
            EventCodeEnum ret = EventCodeEnum.UNDEFINED;

            try
            {
                MapPropertyConnector input = coupler as MapPropertyConnector;

                if (input != null)
                {
                    if (input.ReverseFormatter != null)
                    {
                        input.ReverseFormatter.Argument = value;
                        ret = input.ReverseFormatter.Run();

                        if (ret == EventCodeEnum.NONE)
                        {
                            if (input.ReverseFormatter.Result != null)
                            {
                                retval = input.ReverseFormatter.Result.ToString();
                            }
                            else
                            {
                                retval = string.Empty;
                            }
                        }
                        else
                        {
                            // ???
                        }
                    }
                    else
                    {
                        retval = value.ToString();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }

            return retval;
        }
    }
}
