using ProberInterfaces.MarkAlign;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MarkAlign
{
    public class MarkAlignControlItems : IMarkAlignControlItems, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _MARK_ALIGN_MOVE_ERROR;
        public bool MARK_ALIGN_MOVE_ERROR
        {
            get { return _MARK_ALIGN_MOVE_ERROR; }
            set
            {
                if (value != _MARK_ALIGN_MOVE_ERROR)
                {
                    _MARK_ALIGN_MOVE_ERROR = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MARK_ALIGN_FOCUSING_FAILED;
        public bool MARK_ALIGN_FOCUSING_FAILED
        {
            get { return _MARK_ALIGN_FOCUSING_FAILED; }
            set
            {
                if (value != _MARK_ALIGN_FOCUSING_FAILED)
                {
                    _MARK_ALIGN_FOCUSING_FAILED = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MARK_Pattern_Failure;
        public bool MARK_Pattern_Failure
        {
            get { return _MARK_Pattern_Failure; }
            set
            {
                if (value != _MARK_Pattern_Failure)
                {
                    _MARK_Pattern_Failure = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MARK_ALGIN_PATTERN_MATCH_FAILED;
        public bool MARK_ALGIN_PATTERN_MATCH_FAILED
        {
            get { return _MARK_ALGIN_PATTERN_MATCH_FAILED; }
            set
            {
                if (value != _MARK_ALGIN_PATTERN_MATCH_FAILED)
                {
                    _MARK_ALGIN_PATTERN_MATCH_FAILED = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _MARK_ALIGN_SHIFT;
        public bool MARK_ALIGN_SHIFT
        {
            get { return _MARK_ALIGN_SHIFT; }
            set
            {
                if (value != _MARK_ALIGN_SHIFT)
                {
                    _MARK_ALIGN_SHIFT = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
