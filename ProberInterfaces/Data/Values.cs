namespace ProberInterfaces
{
    public class ValueDescription
    {
        public object Value;
        public string Description;
    }
    public class Values
    {
        private double _XVlaue;

        public double XValue
        {
            get { return _XVlaue; }
            set { _XVlaue = value; }
        }

        private double _YValue;

        public double YValue
        {
            get { return _YValue; }
            set { _YValue = value; }
        }

        public Values()
        {

        }
        public Values(double xvalue , double yvalue)
        {
            XValue = xvalue;
            YValue = yvalue;
        }

    }
}
