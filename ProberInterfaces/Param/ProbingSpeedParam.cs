using System.Collections.Generic;

namespace ProberInterfaces.Param
{
    public class ProbingSpeedRateList : List<ProbingFeedRate> { }

    public class ProbingFeedRate
    {
        private double _SectionRate; // 0 ~ 100 까지.
        public double SectionRate
        {
            get { return _SectionRate; }
            set
            {
                if(value != _SectionRate)
                {
                    if (100 < value)
                    {
                        _SectionRate = 100;
                    }
                    else if (value < 0)
                    {
                        _SectionRate = 0;
                    }
                    else
                    {
                        _SectionRate = value;
                    }
                }
            }
        }

        private double _FeedRate; // 0 ~ 1 까지.
        public double FeedRate
        {
            get { return _FeedRate; }
            set
            {
                if(value != _FeedRate)
                {
                    if(1 < value)
                    {
                        _FeedRate = 1;
                    }
                    else if(value < 0)
                    {
                        _FeedRate = 0;
                    }
                    else
                    {
                        _FeedRate = value;
                    }
                }
            }
        }
    }
}
