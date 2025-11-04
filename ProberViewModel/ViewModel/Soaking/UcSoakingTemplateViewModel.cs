using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Autofac;
using ProberInterfaces;
using RelayCommandBase;
using SoakingParameters;
using VirtualKeyboardControl;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Windows.Controls;

namespace ProberViewModel
{
    /// <summary>
    /// ViewModel의 기본적인 PropertyChanged 구현
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IFactoryModule
    {
        /// <summary>
        /// 속성 알림 이벤트
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 변경 발생시 호출
        /// </summary>
        /// <param name="propertyName">리스너에게 알릴 속성이름(입력하지 않을경우 컴파일러에서 호출될때 자동으로 입력됨)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// 조건부 이벤트 발생
        /// </summary>
        /// <typeparam name="T">유형</typeparam>
        /// <param name="storage">기존값</param>
        /// <param name="value">변경할 값</param>
        /// <param name="propertyName">리스너에게 알릴 속성의 이름 (입력하지 않을경우 컴파일러에서 호출될때 자동으로 입력됨)</param>
        /// <returns></returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Soakingsteptable
    /// Chillingtimetable
    /// 항목에 대한 뷰모델 
    /// </summary>
    /// <typeparam name="T">Soakingsteptable , Chillingtimetable  두개의 클래스만 사용</typeparam>
    public class UcSoakingTemplateViewModel<T> : MetroCustomDialogViewModel
    {
        #region Template and Seleted Item

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private ObservableCollection<TemplateItem<T>> _templateList = new ObservableCollection<TemplateItem<T>>();
        /// <summary>
        ///  템플릿 목록
        /// </summary>
        public ObservableCollection<TemplateItem<T>> TemplateList
        {
            get => _templateList;
            set
            {
                SetProperty(ref _templateList, value);
            }
        }

        private TemplateItem<T> _selectedTemplate;
        /// <summary>
        /// 선택된 템플릿 아이템
        /// </summary>
        public TemplateItem<T> SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                SetProperty(ref _selectedTemplate, value);

                OnPropertyChanged(nameof(HasSelectedTemplate));
                if(value != null)
                {
                    NewTemplateName = value.Name;
                }
            }
        }

        public bool HasSelectedTemplate
        {
            get => _selectedTemplate != null;
        }


        

        #endregion


        #region ApplyResult
        private bool? _applyResult = null;
        /// <summary>
        /// 값이 변경될때 화면 표시 for DataTrigger
        /// </summary>
        public bool? ApplyResult
        {
            get => _applyResult;
            set => SetProperty(ref _applyResult, value);
        }
        #endregion

        #region for NewItemSave

        private bool _enableOverwrite = false;
        /// <summary>
        /// 새로운 템플릿이 저장 가능한지 여부 / 기존 템플릿 덮어쓰기 경우 사용자가 한번더 확인해야함.
        /// </summary>
        public bool EnableOverwrite
        {
            get => _enableOverwrite;
            set
            {
                SetProperty(ref _enableOverwrite, value);
            }
        }

        private bool _isOverwrite = false;
        /// <summary>
        /// 기존 템플릿 덮어쓰기 인지 확인
        /// </summary>
        public bool IsOverwrite
        {
            get => _isOverwrite;
            set
            {
                SetProperty(ref _isOverwrite, value);
            }
        }

        

        private string _newTemplateName;
        public string NewTemplateName
        {
            get => _newTemplateName;
            set
            {
                SetProperty(ref _newTemplateName, value);
                var nameInList = TemplateList.Where(x => string.Compare(value, x.Name) == 0);
                if (nameInList == null || !nameInList.Any())
                    IsOverwrite = false;
                else
                    IsOverwrite = true;

                EnableOverwrite = false;
            }
        }

        private TemplateItem<T> _newTemplate;
        /// <summary>
        /// 선택된 템플릿 아이템
        /// </summary>
        public TemplateItem<T> NewTemplate
        {
            get => _newTemplate;
            set
            {
                SetProperty(ref _newTemplate, value);
            }
        }



        #endregion


        #region Event
        /// <summary>
        /// 템플릿 적용 요청
        /// </summary>
        public event ObjectEventHandler<TemplateItem<T>> ApplyRequest;

       

        /// <summary>
        /// 창 닫기 요청
        /// </summary>
        //public event EventHandler CloseRequest;
        #endregion

        /// <summary>
        /// 선택된 템플릿을 반영
        /// </summary>
        public ICommand CmdApply{ get; set; }        
        /// <summary>
        /// 선택된 템플릿 복제
        /// </summary>
        public ICommand CmdCopy { get; set; }
        /// <summary>
        /// 선택된 템플릿 삭제
        /// </summary>
        public ICommand CmdDelete { get; set; }
      
        public ICommand CmdSave { get; set; }

        public ICommand CmdOverwriteAgree { get; set; }
        public ICommand TextBoxClickCommand { get; set; }

        private readonly Dispatcher dispatcher;
        public UcSoakingTemplateViewModel(ContentControl window, string name) : base(window, name)
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            SetCommand();
        }


        private static int GEN_NAME_MAX = 70; //이름이 너무 긴 경우 제한
        private static int GEN_NAME_LIMIT_PICK_LEN = 10;//기존 이름에서 가져올 이름 길이 (처움부터)
        /// <summary>
        /// 중복되지 않은 이름 생성
        /// </summary>
        /// <param name="existItems">기존 이름 목록</param>
        /// <param name="newName">새로 생성할 이름</param>
        /// <returns></returns>
        private static string GenerateNewName(IEnumerable<string> existItems, string newName)
        {
            Regex fileAutoGenNumber = new Regex(@"([\S ]+)\((\d+)\)$");

            string retName = newName;

            //이름이 너무 길면 자름
            if(newName.Length> GEN_NAME_MAX) 
            {
                retName = newName.Substring(0, GEN_NAME_LIMIT_PICK_LEN) + "~1";
            }

            //if(fileAutoGenNumber.IsMatch(newName))
            //{
            //    var match = fileAutoGenNumber.Match(newName);
            //    retName = match.Groups[1].Value.Trim();
            //}

            //중복된 이름이 갖는 인덱스 목록
            List<int> duplicatedNameNumber = new List<int>();

            foreach(string name in existItems)
            {
                if(fileAutoGenNumber.IsMatch(name))
                {

                    var match = fileAutoGenNumber.Match(name);

                    if(string.Compare(retName, match.Groups[1].Value.Trim(),true)==0)
                    {
                        if(int.TryParse(match.Groups[2].Value, out int autoNumber))
                        {
                            duplicatedNameNumber.Add(autoNumber);
                        }
                    }
                }
                else if(string.Compare(retName, name.Trim(), true)==0)
                {
                    duplicatedNameNumber.Add(-1);
                }
            }

            if(duplicatedNameNumber.Any())
            {
                for(int i=1; i<1000; ++i)
                {
                    if(!duplicatedNameNumber.Contains(i))
                    {
                        return $"{retName} ({i})";
                    }
                }
            }
            else
            {
                return retName;
            }

            ///연변 1000번까지 부여했으나 초과하는경우 생성 실패.
            return $"[E]{retName}";
        }
        
        public ObjectValidator<object> Validator;
        public bool ValidateCheckFunc(object obj, out string resultMessage)
        {
            resultMessage = "";
            if (Validator == null)
                return true;
            
            resultMessage = "OK";
            return  Validator(obj, out resultMessage);
        }


        /// <summary>
        /// 커맨드의 동작을 설정
        /// </summary>
        public void SetCommand()
        {
            //선택된 템플릿 적용을 위해 이벤트 발생
            CmdApply = new AsyncCommand(
                async () =>
                {
                    ApplyResult = null;
                    ApplyResult = ApplyRequest?.Invoke(this, new ObjectEventArgs<TemplateItem<T>>(SelectedTemplate));
                });

            //선택된 아이템 다음 항목으로 복사된 항목을 추가.
            CmdCopy = new AsyncCommand(
                async () => {
                    System.Diagnostics.Debug.WriteLine("CmdCopy: ");
                    if (null == TemplateList)
                    {
                        return;
                    }
                    int index = TemplateList.IndexOf(SelectedTemplate);
                    if (index < 0)
                    {
                        return;
                    }

                    string newName = GenerateNewName(TemplateList.Select(x => x.Name), SelectedTemplate.Name + "_Copy");

                    dispatcher.Invoke(() =>
                    {
                        var cloneItem = SelectedTemplate.Clone();
                        cloneItem.SaveRequest += TemplateItem_SaveRequest;
                        cloneItem.PropertyChanged += TemplateItem_PropertyChanged;
                        cloneItem.Validator = ValidateCheckFunc;
                        cloneItem.Name = newName;
                        cloneItem.EditName = newName;
                        TemplateList.Insert(index + 1, cloneItem);
                        SelectedTemplate = TemplateList[index + 1];

                        Save();
                    });
                },
                () => {
                    return null != SelectedTemplate;
                });
            
            //항목 삭제
            CmdDelete = new AsyncCommand(
                async () => {
                    if (null == TemplateList)
                    {
                        return;
                    }
                    
                    int index = TemplateList.IndexOf(SelectedTemplate);
                    SelectedTemplate.SaveRequest -= TemplateItem_SaveRequest;
                    SelectedTemplate.PropertyChanged -= TemplateItem_PropertyChanged;
                    if (index < 0)
                    {
                        return;
                    }
                                        
                    
                    dispatcher.Invoke(() =>
                    {
                        TemplateList.RemoveAt(index);

                        if (TemplateList.Count < index)
                        {
                            index -= 1;
                        }

                        if (index >= 0 && TemplateList.Count>index)
                        {
                            SelectedTemplate = TemplateList[index];
                        }
                        else
                        {
                            SelectedTemplate = null;
                        }

                        Save();
                    });
                },
                () => {
                    return null != SelectedTemplate;
                } );
            /////다이얼로그 닫기
            //CmdClose = new AsyncCommand(
            //    async () => {

            //        bool? HasNotSavedItem = TemplateList?.Where(x => x.Modified).Any();
            //        if(HasNotSavedItem == true)
            //        {
            //            if (MessageBox.Show($"Are you sure you want to quit without saving?", "Template Editor", MessageBoxButton.YesNo) == MessageBoxResult.No)
            //            {
            //                return;
            //            }
            //        }

            //        CloseRequest?.Invoke(this, null);
            //    });
            //텍스트 박스 입력기 표시
            TextBoxClickCommand = new RelayCommand<object>(
                (param) =>
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL|KB_TYPE.ALPHABET|KB_TYPE.SPECIAL, 0, 100);
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                });
            ///기존 항목 덮어쓰기 동의
            CmdOverwriteAgree = new RelayCommand(() =>
            {
                EnableOverwrite = true;
            });

            CmdSave = new AsyncCommand(
                 async () => {
                     System.Diagnostics.Debug.WriteLine("CmdSave: ");
                     
                     dispatcher.Invoke(() =>
                     {
                         var saveTemplate = TemplateList.Where(x => string.Compare(NewTemplateName, x.Name) == 0).FirstOrDefault();

                         if (null == saveTemplate)
                         {
                             NewTemplate.Name = NewTemplateName;
                             TemplateList.Add(NewTemplate);
                         }
                         else
                         {
                             saveTemplate.Steps = NewTemplate.Steps;
                             saveTemplate.Time = NewTemplate.Time;
                         }

                         
                         Save();

                         IDOK.Execute(this);
                         //저장후 닫기
                         //CloseRequest?.Invoke(this, null);
                     });
                 });
        }

        private void TemplateItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(TemplateItem<int>.EditName)) ///Edit 이름이 현재 목록에서 동일한 이름이 있는지 확인.
            {
                if(sender is TemplateItem<Chillingtimetable> c)
                {

                    bool duplicate = TemplateList.Where(x => string.Compare(x.Name, c.EditName, true) == 0).Any();

                    //이름 바꾸지 않았으면 중복 아님, 셀프 중복 항목 예외
                    if (duplicate && string.Compare(c.Name, c.EditName, true) == 0)
                    {
                        duplicate = false;
                    }

                    c.EditNameIsDuplicate = duplicate;

                }
                else if(sender is TemplateItem<Soakingsteptable> s)
                {
                    bool duplicate = TemplateList.Where(x => string.Compare(x.Name, s.EditName, true) == 0).Any();

                    //이름 바꾸지 않았으면 중복 아님, 셀프 중복 항목 예외
                    if (duplicate && string.Compare(s.Name, s.EditName, true) == 0)
                    {
                        duplicate = false;
                    }

                    s.EditNameIsDuplicate = duplicate;
                }
            }
        }

        private void TemplateItem_SaveRequest(object sender, EventArgs e)
        {
            Save();
        }
         
        private string TemplateFilePath()
        {
            string paramRoot =  this.FileManager().FileManagerParam.SystemParamRootDirectory;
            string path = System.IO.Path.Combine(paramRoot, @"Soaking\SoakingTemplate.json");//System.IO.Path.Combine(this.FileManager().FileManagerParam.SystemParamRootDirectory, @"Soaking\SoakingTemplate.json");
            return path;
        }

        private TemplateData LoadTemplate()
        {
            TemplateData templateData = null;

            string templatePath = TemplateFilePath();
            if (System.IO.File.Exists(templatePath))
                templateData = JsonConvert.DeserializeObject<TemplateData>(System.IO.File.ReadAllText(templatePath));

            if (templateData == null)
                templateData = new TemplateData();

            return templateData;
        }

        private void SaveTemplate(TemplateData templateData)
        {
            string resultJson = JsonConvert.SerializeObject(templateData);
            
            string templatePath = TemplateFilePath();
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(templatePath)))//경로 생성.
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(templatePath));
            }

            System.IO.File.WriteAllText(TemplateFilePath(), resultJson);
        }

        private void ClearTemplateList()
        {
            foreach (var template in TemplateList.OrEmptyIfNull())
            {
                template.SaveRequest -= TemplateItem_SaveRequest;
                template.PropertyChanged -= TemplateItem_PropertyChanged;
            }
            TemplateList?.Clear();
        }

        private void AddTemplateEvent(ref TemplateItem<T> template)
        {
            template.SaveRequest += TemplateItem_SaveRequest;
            template.PropertyChanged += TemplateItem_PropertyChanged;
            template.Validator = ValidateCheckFunc;
            template.EditName = template.Name;
            template.EditSteps = template.Steps.DeepClone();
        }
        public void Load()
        {   
            var templateData = LoadTemplate();

            ClearTemplateList();

            if (typeof(T) == typeof(Soakingsteptable))
            {
                var sample = GetSoakingSample();
                foreach (var template in templateData.SoakingTemplates.OrEmptyIfNull())
                {
                    var tempateT = template as TemplateItem<T>;
                    AddTemplateEvent(ref tempateT);
                    TemplateList.Add(tempateT);
                    if(string.Compare(tempateT.Name, sample?.Name)==0)
                    {
                        sample = null;
                    }
                }
                if (sample != null)
                {
                    AddTemplateEvent(ref sample);
                    TemplateList.Insert(0, sample);
                }
            }
            else if (typeof(T) == typeof(Chillingtimetable))
            {
                var sample = GetChillingSample();
                foreach (var template in templateData.ChillingTemplate.OrEmptyIfNull())
                {
                    var tempateT = template as TemplateItem<T>;
                    AddTemplateEvent(ref tempateT);
                    TemplateList.Add(tempateT);
                    if (string.Compare(tempateT.Name, sample?.Name) == 0)
                    {
                        sample = null;
                    }
                }
                if (sample != null)
                {
                    AddTemplateEvent(ref sample);
                    TemplateList.Insert(0, sample);
                }
            }
        }

        public void Save()
        {

            var templateData = LoadTemplate();

            if (typeof(T) == typeof(Soakingsteptable))
            {
                if(templateData.SoakingTemplates!= null)
                    templateData.SoakingTemplates.Clear();
                else
                    templateData.SoakingTemplates = new List<TemplateItem<Soakingsteptable>>();
                foreach (var template in TemplateList)
                {
                    templateData.SoakingTemplates .Add(template as TemplateItem<Soakingsteptable>);
                }
            }
            else if (typeof(T) == typeof(Chillingtimetable))
            {
                if (templateData.ChillingTemplate != null)
                    templateData.ChillingTemplate.Clear();
                else
                    templateData.ChillingTemplate = new List<TemplateItem<Chillingtimetable>>();
                foreach (var template in TemplateList)
                {
                    templateData.ChillingTemplate.Add(template as TemplateItem<Chillingtimetable>);
                }
            }
            SaveTemplate(templateData);
        }

        private TemplateItem<T> GetSoakingSample()
        {
            var sample = new TemplateItem<Soakingsteptable>()
            {
                Name = "Soaking Sample",
                Time = DateTime.Now,
                Steps = new ObservableCollection<Soakingsteptable>()
                {
                    new Soakingsteptable() { StepIdx = new Element<int>(1), TimeSec = new Element<int>(600), OD_Value = new Element<double>(-80) },
                    new Soakingsteptable() { StepIdx = new Element<int>(2), TimeSec = new Element<int>(1400), OD_Value = new Element<double>(-80) },
                    new Soakingsteptable() { StepIdx = new Element<int>(3), TimeSec = new Element<int>(2400), OD_Value = new Element<double>(-80) },
                    new Soakingsteptable() { StepIdx = new Element<int>(4), TimeSec = new Element<int>(3900), OD_Value = new Element<double>(-80) },
                    new Soakingsteptable() { StepIdx = new Element<int>(5), TimeSec = new Element<int>(6300), OD_Value = new Element<double>(-80) },
                    new Soakingsteptable() { StepIdx = new Element<int>(6), TimeSec = new Element<int>(int.MaxValue), OD_Value = new Element<double>(-80) }
                }
            };
            return sample as TemplateItem<T>;
        }
        private TemplateItem<T> GetChillingSample()
        {
            var sample = new TemplateItem<Chillingtimetable>()
            {
                Name = "Chilling Sample",
                Time = DateTime.Now,
                Steps = new ObservableCollection<Chillingtimetable>(){
                         new Chillingtimetable(){StepIdx = new Element<int>(1), SoakingTimeSec = new Element<int>( 360), ChillingTimeSec = new Element<int>( 180)},
                         new Chillingtimetable(){StepIdx = new Element<int>(2), SoakingTimeSec = new Element<int>(1200), ChillingTimeSec = new Element<int>( 600)},
                         new Chillingtimetable(){StepIdx = new Element<int>(3), SoakingTimeSec = new Element<int>(2400), ChillingTimeSec = new Element<int>(1200)},
                         new Chillingtimetable(){StepIdx = new Element<int>(4), SoakingTimeSec = new Element<int>(3600), ChillingTimeSec = new Element<int>(1800)},
                         new Chillingtimetable(){StepIdx = new Element<int>(5), SoakingTimeSec = new Element<int>(4800), ChillingTimeSec = new Element<int>(2400)},
                         new Chillingtimetable(){StepIdx = new Element<int>(6), SoakingTimeSec = new Element<int>(6000), ChillingTimeSec = new Element<int>(3000)},
                         new Chillingtimetable(){StepIdx = new Element<int>(7), SoakingTimeSec = new Element<int>(7200), ChillingTimeSec = new Element<int>(3600)}
                    }
            };
            return sample as TemplateItem<T>;
        }

        //private void SetDemoData()
        //{
        //    SoakingTemplateList = new ObservableCollection<TemplateItem<Soakingsteptable>>()
        //    {
        //        new TemplateItem<Soakingsteptable>(){Name="Soaking Template 1", Time=DateTime.Now, Steps= new ObservableCollection<Soakingsteptable>(){ 
        //             new Soakingsteptable(){ StepIdx= new Element<int>(1), TimeSec= new Element<int>(100), OD_Value=new Element<double>(1.0)  },
        //             new Soakingsteptable(){ StepIdx= new Element<int>(2), TimeSec= new Element<int>(200), OD_Value=new Element<double>(2.0)  }
        //        } },
        //        new TemplateItem<Soakingsteptable>(){Name="Soaking Template 2", Time=DateTime.Now, Steps= new ObservableCollection<Soakingsteptable>(){
        //             new Soakingsteptable(){ StepIdx= new Element<int>(1), TimeSec= new Element<int>(300), OD_Value=new Element<double>(1.0)  },
        //             new Soakingsteptable(){ StepIdx= new Element<int>(2), TimeSec= new Element<int>(400), OD_Value=new Element<double>(2.0)  }
        //        } }
        //    };

        //    ChillingTemplateList = new ObservableCollection<TemplateItem<Chillingtimetable>>()
        //    {
        //        new TemplateItem<Chillingtimetable>(){Name="Soaking Template 1", Time=DateTime.Now, Steps= new ObservableCollection<Chillingtimetable>(){
        //             new Chillingtimetable(){ StepIdx= new Element<int>(1),  SoakingTimeSec = new Element<int>(100),  ChillingTimeSec=new Element<int>(110)  },
        //             new Chillingtimetable(){ StepIdx= new Element<int>(2), SoakingTimeSec= new Element<int>(200), ChillingTimeSec=new Element<int>(210)  }
        //        } },
        //        new TemplateItem<Chillingtimetable>(){Name="Soaking Template 2", Time=DateTime.Now, Steps= new ObservableCollection<Chillingtimetable>(){
        //             new Chillingtimetable(){ StepIdx= new Element<int>(1), SoakingTimeSec= new Element<int>(300), ChillingTimeSec=new Element<int>(110)  },
        //             new Chillingtimetable(){ StepIdx= new Element<int>(2), SoakingTimeSec= new Element<int>(400), ChillingTimeSec=new Element<int>(210)  }
        //        } }
        //    };
        //}
    }

    public class TemplateData
    {
        public string Info { get; set; } = "Soaking Template List";
        public List<TemplateItem<Soakingsteptable>> SoakingTemplates { get; set; }
        public List<TemplateItem<Chillingtimetable>> ChillingTemplate { get; set; }
    }

    
    /// <summary>
    /// 템플릿 하나에 대한 내용
    /// 템플릿의 이름 및 절차 정보관리
    /// </summary>
    /// <typeparam name="T">Soakingsteptable , Chillingtimetable  두개의 클래스만 사용</typeparam>
    [Serializable]
    public class TemplateItem<T> : ViewModelBase
    {

        private string _name;
        private string _editName;
        private DateTime _time;

        /// <summary>
        /// 템플릿 이름
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        /// <summary>
        /// 변경할 이름. (사용자가 SAVE를 누르기 전까지)
        /// </summary>
        [JsonIgnore]
        public string EditName {
            get => _editName;
            set => SetProperty(ref _editName, value);
        }
        [JsonIgnore]

        private bool _editNameIsDuplicate = false;
        public bool EditNameIsDuplicate
        {
            get => _editNameIsDuplicate;
            set => SetProperty(ref _editNameIsDuplicate, value);
        }
        /// <summary>
        /// 마지막 수정(생성) 된 시간
        /// </summary>
        public DateTime Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private bool _modified = false;
        /// <summary>
        /// 변경 사항 있는지 확인
        /// </summary>
        [JsonIgnore]
        public bool Modified
        {
            get => _modified;
            set => SetProperty(ref _modified, value);
        }

        private bool _equalToCurrent = false;
        /// <summary>
        /// 지금 설정된 항목과 동일한 내용인지 확인
        /// </summary>
        [JsonIgnore]
        public bool EqualToCurrent
        {
            get => _equalToCurrent;
            set => SetProperty(ref _equalToCurrent, value);
        }

        private ObservableCollection<T> _steps;
        /// <summary>
        ///  설정된 절차.
        /// </summary>
        public ObservableCollection<T> Steps
        {
            get => _steps;
            set => SetProperty(ref _steps, value);

        }

        private ObservableCollection<T> _editSteps;
        /// <summary>
        /// 편집용 절차 사본
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<T> EditSteps
        {
            get => _editSteps;
            set
            {
                if (_editSteps?.Count != value?.Count)
                {
                    EditStepCount = null == value ? 0 : value.Count;
                }
                if(null!= value)
                {
                    IndexRebuild(value);
                }
                SetProperty(ref _editSteps, value);
            }
        }

        private int _editStepCount = 0;
        [JsonIgnore]
        public int EditStepCount
        {
            get => _editStepCount;
            set => SetProperty(ref _editStepCount, value);
        }

        /// <summary>
        /// 현재 설정 비교용
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<T> ReferenceSteps;

        private T _selectedStep;
        /// <summary>
        /// 선택된 항목 아이템
        /// </summary>
        [JsonIgnore]
        public T SelectedStep
        {
            get => _selectedStep;
            set => SetProperty(ref _selectedStep, value);
        }

        /// <summary>
        /// 선택된 항목을 삭제
        /// </summary>
        [JsonIgnore]
        public ICommand CmdDelete { get; set; }
        /// <summary>
        /// 선택된 항목 아래 새로운 항목 추가
        /// </summary>
        [JsonIgnore]
        public ICommand CmdAdd { get; set; }
        /// <summary>
        /// 전체 항목 삭제
        /// </summary>
        [JsonIgnore]
        public ICommand CmdClear { get; set; }
        /// <summary>
        /// 변경된 내용을 저장
        /// </summary>
        [JsonIgnore]
        public ICommand CmdSave { get; set; }
        /// <summary>
        /// 변경사항 취소
        /// </summary>
        [JsonIgnore]
        public ICommand CmdRestore { get; set; }
        /// <summary>
        /// xprtmxm
        /// </summary>
        [JsonIgnore]
        public ICommand TextBoxClickCommand { get; set; }
        /// <summary>
        /// 수행 절차 목록을 관리
        /// </summary>

        /// <summary>
        /// 수정 사항 저장 요청
        /// </summary>
        public event EventHandler SaveRequest;
        [JsonIgnore]
        public ValidateValueCallback callback;
        [JsonIgnore]
        public ObjectValidator<object> Validator;

        public TemplateItem()
        {
            SetCommand();
        }

        private void IndexRebuild( ObservableCollection<T> list)
        {
            for(int i=0;i<list.Count; ++i)
            {
                if (list[i] is Soakingsteptable s)
                {
                    int index = (i == list.Count - 1) ? Int32.MaxValue : i + 1;
                    s.StepIdx.Value = index;
                }
                else if (list[i] is Chillingtimetable c)
                {

                    c.StepIdx.Value = i + 1;
                }
            }
        }


        private bool? _isValid = null;
        [JsonIgnore]
        public bool? IsValid
        {
            get
            {
                if (null == _isValid)
                {
                    ValidateCheck(Steps);
                }
                return _isValid;
            }
            set => SetProperty(ref _isValid, value);
        }

        private string _validateResult;
        [JsonIgnore]
        public string ValidateResult
        {
            get => _validateResult;
            set => SetProperty(ref _validateResult, value);
        }

        private void ValidateCheck(IEnumerable<T> list)
        {
            bool? result = null;
            string message = "";

            if (Validator == null) // 유효성 검증 하지 않음. 
            {
                result = true;

            }
            else if (list == null || !list.Any()) //유효성 검증 대상 없음. 
            {
                result = null;
            }
            else
            {
                result = Validator.Invoke(list, out message);
            }

            IsValid = result;
            ValidateResult = message;
        }

        /// <summary>
        /// 커맨드의 동작을 설정
        /// </summary>
        private void SetCommand()
        {
            //삭제
            CmdDelete = new RelayCommand(
                () => {
                    if (null == EditSteps)
                    {
                        return;
                    }
                    int index = EditSteps.IndexOf(SelectedStep);
                    if (index < 0)
                    {
                        return;
                    }

                    EditSteps.RemoveAt(index);
                    IndexRebuild(EditSteps);
                    if (EditSteps.Count < index)
                    {
                        index -= 1;
                    }

                    if (index >= 0)
                    {
                        SelectedStep = EditSteps[index];
                    }
                    else
                    {
                        SelectedStep = default(T);
                    }
                    EditStepCount--;
                },
                () => {
                    return null != SelectedStep;                
                });
            //전체 삭제
            CmdClear = new RelayCommand(
                () =>                {
                    EditSteps?.Clear();
                    SelectedStep = default(T);
                    EditStepCount = 0;
                },
                () =>
                {
                    return EditSteps == null ? false : EditSteps.Any();
                });
            //항목 추가
            CmdAdd = new RelayCommand(
                () => {
                    if (EditSteps == null)
                    {
                        EditSteps = new ObservableCollection<T>();
                    }

                    int index = EditSteps.IndexOf(SelectedStep);                    
                    if (index < 0)
                    {
                        //get last index
                        index = EditSteps.Count - 1;
                    }
                    T obj = (T)Activator.CreateInstance(typeof(T));
                    EditSteps.Insert(index + 1, obj);
                    IndexRebuild(EditSteps);
                    SelectedStep = obj;
                    EditStepCount++;
                },
                () => {
                    return true;
                });
            //변경사항 저장
            CmdSave = new RelayCommand(
                () =>
                {
                    Name = EditName;
                    Steps = EditSteps.DeepClone();
                    ValidateCheck(Steps);
                    SaveRequest?.Invoke(this, null);
                },
                () =>
                {   
                    Modified = IsModified();
                    EqualToCurrent = IsEqualToCurrent();



                    return Modified && !EditNameIsDuplicate && EditName.Length > 0;
                }
                );
            //변경 사항 취소
            CmdRestore = new RelayCommand(
                () =>
                {
                    EditName = Name;
                    EditSteps = Steps.DeepClone();

                    IndexRebuild(EditSteps);

                },
                () =>
                {
                    Modified = IsModified();
                    EqualToCurrent = IsEqualToCurrent();

                    return Modified;
                }
                );

            TextBoxClickCommand = new RelayCommand<object>(
                (param) =>
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)param;
                    tb.Text = VirtualKeyboard.Show(tb.Text, KB_TYPE.DECIMAL, 0, 100);
                    tb.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                });

        }

        /// <summary>
        /// 저장되지 않은 변경 사항이 있는지 확인
        /// </summary>
        /// <returns>true: 수정됨 , false: 변경사항 없음</returns>
        public bool IsModified()
        {
            //이름 바뀜
            if (string.Compare(EditName, Name) != 0)
            {
                return true;
            }

            //항목 없음. 
            if (null == Steps || null == EditSteps)
            {
                return false;
            }

            //항목 갯수 다름.
            if (Steps?.Count() != EditSteps?.Count())
            {
                return true;
            }

            //항목 개별 항목 값 다름.
            for (int i = Steps.Count() - 1; i >= 0; --i)
            {
                //T == Soakingsteptable
                if (Steps[i] is Soakingsteptable sBase && EditSteps[i] is Soakingsteptable sComp)
                {
                    if (sBase.TimeSec.Value != sComp.TimeSec.Value || sBase.OD_Value.Value != sComp.OD_Value.Value)
                    {
                        return true;
                    }
                }
                //T == Chillingtimetable
                else if (Steps[i] is Chillingtimetable cBase && EditSteps[i] is Chillingtimetable cComp)
                {
                    if (cBase.ChillingTimeSec.Value != cComp.ChillingTimeSec.Value || cBase.SoakingTimeSec.Value != cComp.SoakingTimeSec.Value)
                    {
                        return true;
                    }
                }
                // T == ?
                // TODO 새로운 T 아이템이 추가되면 위와 같이 새로운 비교 방법을 추가 합니다. 
                else if (!Steps[i].Equals(EditSteps[i]))
                {
                    return true;
                }
            }

            return false;
        }
        public bool IsEqualToCurrent()
        {
            //항목 없음. 
            if (null == Steps || null == ReferenceSteps)
            {
                return false;
            }

            //항목 갯수 다름.
            if (Steps?.Count() != ReferenceSteps?.Count())
            {
                return false;
            }

            //항목 개별 항목 값 다름.
            for (int i = Steps.Count() - 1; i >= 0; --i)
            {
                //T == Soakingsteptable
                if (Steps[i] is Soakingsteptable sBase && ReferenceSteps[i] is Soakingsteptable sComp)
                {
                    if (sBase.TimeSec.Value != sComp.TimeSec.Value || sBase.OD_Value.Value != sComp.OD_Value.Value)
                    {
                        return false;
                    }
                }
                //T == Chillingtimetable
                else if (Steps[i] is Chillingtimetable cBase && ReferenceSteps[i] is Chillingtimetable cComp)
                {
                    if (cBase.ChillingTimeSec.Value != cComp.ChillingTimeSec.Value || cBase.SoakingTimeSec.Value != cComp.SoakingTimeSec.Value)
                    {
                        return false;
                    }
                }
                // T == ?
                // TODO 새로운 T 아이템이 추가되면 위와 같이 새로운 비교 방법을 추가 합니다. 
                else if (!Steps[i].Equals(EditSteps[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 복제 
        /// </summary>
        /// <returns></returns>
        public TemplateItem<T> Clone()
        {
            var ret = new TemplateItem<T>();
            ret.Name = this.Name;
            ret.EditName = this.Name;
            ret.Time = DateTime.Now;
            ret.ReferenceSteps = this.ReferenceSteps;
            ret.Steps = this.Steps.DeepClone();
            ret.EditSteps = this.Steps.DeepClone();
            
            return ret;
        }
    }

    #region ObjectEventHandler
    /// <summary>
    /// Object 배열을 포함하는 이벤트 정의
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ObjectArrayEventHandler<T>(object sender, ObjectArrayEventArgs<T> e);
    /// <summary>
    /// 이전값과 변경된 값을 포함하는 이벤트 정의
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ObjectChangeEventHandler<T>(object sender, ObjectChangedEventArg<T> e);

    /// <summary>
    /// 변수 하나를 포함하는 제네릭 이벤트 정의
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate bool ObjectEventHandler<T>(object sender, ObjectEventArgs<T> e);

    public delegate bool ObjectValidator<T>(T obj, out string message);
    /// <summary>
    /// Object Array를 포함하는 이벤트 전달 변수
    /// </summary>
    public class ObjectArrayEventArgs<T> : EventArgs
    {
        private readonly T[] value;
        public T[] Value { get => value; }
        public ObjectArrayEventArgs(T[] value)
        {
            this.value = value;
        }
    }
    /// <summary>
    /// 이전값과 변경된값을 전달 하는 변수
    /// </summary>
    public class ObjectChangedEventArg<T> : EventArgs
    {
        private readonly T oldValue;
        public T OldValue { get => oldValue; }

        private readonly T newValue;
        public T NewValue { get => newValue; }

        public ObjectChangedEventArg(T oldvalue, T newvalue)
        {
            this.oldValue = oldvalue;
            this.newValue = newvalue;
        }
    }
    /// <summary>
    /// Object를 포함하는 이벤트 전달 변수
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectEventArgs<T> : EventArgs
    {
        private readonly T value;
        public T Value { get => value; }
        public ObjectEventArgs(T value)
        {
            this.value = value;
        }
    }
    #endregion

    #region Extension
    /// <summary>
    /// 확장 메서드
    /// </summary>
    public static partial class Extension
    {
        /// <summary>
        /// 객체를 완전 복사. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
        /// <summary>
        /// 배열이 null 인경우 Empty로 변환 foreach에서 null 체크 대신 활용.
        /// </summary>
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
    #endregion
}
