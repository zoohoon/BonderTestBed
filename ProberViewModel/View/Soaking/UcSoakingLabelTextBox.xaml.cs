using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace SoakingSettingView
{
    /// <summary>
    /// 마크업 XAML에서 숫자 범위 제한 설정을 한번에 할 수 있도록 노출
    /// </summary>
    public class RangeLimitExtension : MarkupExtension
    {
        public RangeLimitExtension() { RangeType = RangeLimitType.Unlimited; }

        public RangeLimitType RangeType { set; get; }
        public double Min { set; get; }
        public double Max { set; get; }


        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new RangeOption() { RangeType = this.RangeType, Min = this.Min, Max = this.Max };
        }
    }

    /// <summary>
    /// 제한 방식
    /// </summary>
    public enum RangeLimitType
    {
        /// <summary>
        /// 제한없음
        /// </summary>
        Unlimited,
        /// <summary>
        /// 자연수
        /// </summary>
        RangeInPositive,
        /// <summary>
        /// 두 숫자 범위 사이
        /// </summary>
        RangeInValue,
        /// <summary>
        /// 1~100 사이
        /// </summary>
        RangeInPercentage,
    }

    /// <summary>
    /// 입력 숫자 제약 조건
    /// </summary>
    public class RangeOption
    {
        public RangeLimitType RangeType { get; set; } = RangeLimitType.Unlimited;
        public double Min { get; set; } = 0f;
        public double Max { get; set; } = 0f;



        public bool IsRangaeValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (RangeType == RangeLimitType.Unlimited)
                return true;

            if (RangeType == RangeLimitType.RangeInPositive && value.StartsWith("-"))
                return false;

            if (RangeType == RangeLimitType.RangeInValue)
            {
                if (double.TryParse(value, out double d))
                {
                    return (d >= Min) && (d <= Max);
                }
            }

            if (RangeType == RangeLimitType.RangeInPercentage) 
            {
                if (int.TryParse(value, out int d)) 
                {
                    return (d >= 0) && (d <= 100);
                }
            }

            return true;
        }

        public string InfoMessage()
        {
            if (RangeType == RangeLimitType.Unlimited)
                return null;

            if (RangeType == RangeLimitType.RangeInPositive)
                return "Greater than or equal to 0";

            if (RangeType == RangeLimitType.RangeInValue)
                return $"Between {Min} and {Max}";

            if (RangeType == RangeLimitType.RangeInPercentage)
                return $"Between 0 and 100";

            return null;
        }

    }

    /// <summary>
    /// UcSoakingLabelTextBox.xaml에 대한 상호 작용 논리
    /// </summary>    
    public partial class UcSoakingLabelTextBox : UserControl
    {
        public string Tag
        {
            get => (string)GetValue(TagProperty);
            set => SetValue(TagProperty, value);
        }

        public static readonly DependencyProperty TagProperty =
           DependencyProperty.Register(
               "Tag",
               typeof(string),
               typeof(UcSoakingLabelTextBox));



        public string EndTag
        {
            get => (string)GetValue(EndTagProperty);
            set => SetValue(EndTagProperty, value);
        }

        public static readonly DependencyProperty EndTagProperty =
           DependencyProperty.Register(
               "EndTag",
               typeof(string),
               typeof(UcSoakingLabelTextBox));



        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
           DependencyProperty.Register(
               "Text",
               typeof(string),
               typeof(UcSoakingLabelTextBox), new PropertyMetadata(null, OnTextPropertyPropertyChangedCallback));

        private static void OnTextPropertyPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UcSoakingLabelTextBox t)
            {
                string old = (string)e.OldValue;
                string newValue = (string)e.NewValue;

                if (!string.IsNullOrWhiteSpace(old))
                {
                    if (!t.Range.IsRangaeValid(newValue))
                    {
                        t.SetValue(e.Property, e.OldValue);
                    }
                }
            }
        }


        private string ToolTipMessage
        {
            get => (string)GetValue(ToolTipMessageProperty);
            set => SetValue(ToolTipMessageProperty, value);
        }

        private static readonly DependencyProperty ToolTipMessageProperty =
           DependencyProperty.Register(
               "ToolTipMessage",
               typeof(string),
               typeof(UcSoakingLabelTextBox));







        public static readonly DependencyProperty RangeMinProperty =
           DependencyProperty.Register(
               "RangeMin",
               typeof(int),
               typeof(UcSoakingLabelTextBox), new PropertyMetadata(0, OnRangeMinPropertyChangedCallback));

        private static void OnRangeMinPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UcSoakingLabelTextBox t)
            {
                t.Range.Min = (int)e.NewValue;
                t.TooltipUpdate();
            }
        }

        public int RangeMin
        {
            get => (int)GetValue(RangeMinProperty);
            set
            {
                SetValue(RangeMinProperty, value);
                Range.Min = value;
            }
        }








        public static readonly DependencyProperty RangeMaxProperty =
           DependencyProperty.Register(
               "RangeMax",
               typeof(int),
               typeof(UcSoakingLabelTextBox), new PropertyMetadata(0, OnRangeMaxPropertyChangedCallback));

        private static void OnRangeMaxPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UcSoakingLabelTextBox t)
            {
                t.Range.Max = (int)e.NewValue;
                t.TooltipUpdate();
            }
        }

        public int RangeMax
        {
            get => (int)GetValue(RangeMaxProperty);
            set
            {
                SetValue(RangeMaxProperty, value);
            }
        }







        private RangeOption _range = new RangeOption();

        public RangeOption Range
        {
            get => _range;
            set
            {
                _range = value;
                TooltipUpdate();
            }
        }

        public void TooltipUpdate()
        {
            ToolTipMessage = Range.InfoMessage();
        }







        public int TextWidth
        {
            get => (int)GetValue(TextWidthProperty);
            set => SetValue(TextWidthProperty, value);
        }

        public static readonly DependencyProperty TextWidthProperty =
           DependencyProperty.Register(
               "TextWidth",
               typeof(int),
               typeof(UcSoakingLabelTextBox));

        public UcSoakingLabelTextBox()
        {
            InitializeComponent();
        }
    }
}
