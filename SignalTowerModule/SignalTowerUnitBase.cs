using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalTowerModule
{
    using Autofac;
    using LoaderBase;
    using LogModule;
    using Newtonsoft.Json;
    using NotifyEventModule;
    using ProberErrorCode;
    using ProberInterfaces;
    using ProberInterfaces.SignalTower;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using TwinCatHelper;

    public class SignalTowerUnitBase : IParamNode, INotifyPropertyChanged
    {
        [XmlIgnore, JsonIgnore]
        public string Genealogy { get; set; }
        [NonSerialized]
        private Object _Owner;
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public Object Owner
        {
            get { return _Owner; }
            set
            {
                if (_Owner != value)
                {
                    _Owner = value;
                }
            }
        }
        [XmlIgnore, JsonIgnore]
        public List<object> Nodes { get; set; }

        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 지정된 속성이 변경되었음을 발생시킵니다.
        /// </summary>
        /// <param name="propertyName">속성 이름</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion 

        private string _Label;
        public string Label
        {
            get { return _Label; }
            set { _Label = value; }
        }
        private string _Type;
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        private Element<string> _IoPropertyName = new Element<string>();
        /// <summary>
        /// DOREDLAMP의 Output Description을 가져오거나 설정합니다.
        /// </summary>
        [DataMember]
        public Element<string> IoPropertyName
        {
            get { return _IoPropertyName; }
            set { _IoPropertyName = value; }
        }

        private Color _Color;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
        //private bool _Blinkbuzzer;
        //public bool Blinkbuzzer
        //{
        //    get { return _Blinkbuzzer; }
        //    set{ _Blinkbuzzer = value; }
        //}
       
        private ObservableCollection<SignalTowerEventParam> _OnQueue = new ObservableCollection<SignalTowerEventParam>();
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public ObservableCollection<SignalTowerEventParam> OnQueue
        {
            get { return _OnQueue; }            
            set
            {
                if (_OnQueue != value)
                {
                    _OnQueue = value;
                    RaisePropertyChanged();
                }
            }
        }

        // foup number or cell number + foup raising event or foup state or cell state 
        private ObservableCollection<SignalTowerEventParam> _BlinkQueue = new ObservableCollection<SignalTowerEventParam>();
        [XmlIgnore, JsonIgnore, ParamIgnore]
        public ObservableCollection<SignalTowerEventParam> BlinkQueue
        {
            get { return _BlinkQueue; }
            set
            {
                if (_BlinkQueue != value)
                {
                    _BlinkQueue = value;
                    RaisePropertyChanged();
                }
            }
        }        

        private SignalTowerManager Module { get; set; }

        private IOPortDescripter<bool> IoProperty { get; set; }
        private ILoaderModule Loader { get; set; }       

        public SignalTowerUnitBase()
        { }
        public SignalTowerUnitBase(string label,
            string type,
            string iopropertyname,
            Color color,
            bool blinkbuzzer)
        {
            Label = label;
            Type = type;
            IoPropertyName.Value = iopropertyname;
            Color = color;                      
        }

        public void InitModule(SignalTowerManager module)
        {
            try
            {
                Module = module;
                
                Loader = Module.GetLoaderContainer().Resolve<ILoaderModule>();
                //실제 하드웨어 출력을 위한 연결
                IoProperty = Loader.IOManager.GetIOPortDescripter(_IoPropertyName.Value);

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            
        }        

        // 실제 시그널을 끄고 키는 함수
        public void SetPulse()
        {            
            try
            {
                if (Loader != null)
                {

                    //PLC 연결
                    //1.GPLoader에서 set해준다.
                    //2.PLC module 직접 호출.
                    //
                    if (Module.SignalTowerUnits != null)
                    {                        
                        foreach (var item in Module.SignalTowerUnits)
                        {
                            if (item.BlinkQueue.Count == 0 && item.OnQueue.Count == 0)       // on, blink queue에 데이터가 없는 경우
                            {
                                Module.GetGPLoader().SetOnSignalTowerState(item.Label, false);
                                if (item.Label == "RED")
                                    Module.GetGPLoader().StopRedBlink();

                                else if (item.Label == "GREEN")
                                    Module.GetGPLoader().StopGreenBlink();

                                else if (item.Label == "YELLOW")
                                    Module.GetGPLoader().StopYellowBlink();

                            }
                            else if (item.BlinkQueue.Count > 0)      // on, blink queue에 데이터가 있는 경우 -> on queue가 씹힌다.  Red on 은 예외
                            {
                                if (item.Label == "RED")
                                {
                                    if(item.OnQueue.Count > 0)
                                        Module.GetGPLoader().SetOnSignalTowerState(item.Label, true);
                                    else
                                        Module.GetGPLoader().RunRedBlink();
                                }                                                                   
                                else if (item.Label == "GREEN")
                                    Module.GetGPLoader().RunGreenBlink();

                                else if (item.Label == "YELLOW")
                                    Module.GetGPLoader().RunYellowBlink();
                            }
                            else if (item.OnQueue.Count > 0)
                            {
                                Module.GetGPLoader().SetOnSignalTowerState(item.Label, true);
                                if(item.Label == "BUZZER")
                                    Module.GetGPLoader().LoaderBuzzer(true);
                            }                      
                        }                                                                                                                             

                        //IO 연결 //bool type을 여기서 넘겨 받고 넘겨 주는게 맞는가?
                        //Loader.IOManager.WriteIO(IoProperty,onoff);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }            
        }
    }   
}
