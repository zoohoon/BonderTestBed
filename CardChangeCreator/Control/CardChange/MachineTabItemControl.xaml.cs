using LogModule;
using Microsoft.Win32;
using ProberErrorCode;
using ProberInterfaces;
using SequenceRunner;
using SerializerUtil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CardChangeCreator
{
    /// <summary>
    /// MachineTabItemControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MachineTabItemControl : UserControl
    {
        ObservableCollection<string> CCTypeCollection;
        ObservableCollection<string> EquipmentTypeCollection;
        Type[] allBehaviorType;
        bool isSavePossible;

        public MachineTabItemControl()
        {
            try
            {
                InitializeComponent();

                CCTypeCollection = new ObservableCollection<string>();
                EquipmentTypeCollection = new ObservableCollection<string>();
                GetAllCCType();

                isSavePossible = false;

                List<ObservableCollection<string>> tempList = GetTypeFromXML(@"C:\ProberSystem\Parameters\CardChangeParameter\CC_Equip_Type.json");

                if (tempList != null)
                {
                    if (0 < tempList.Count)
                        comboxCCType.ItemsSource = tempList[0];
                    if (1 < tempList.Count)
                        comboxEquipmentType.ItemsSource = tempList[1];
                }

                //CCTypeCollection.Add("CCType1");
                //CCTypeCollection.Add("CCType2");
                //CCTypeCollection.Add("CCType3");

                //EquipmentTypeCollection.Add("EquipmentType1");
                //EquipmentTypeCollection.Add("EquipmentType2");
                //EquipmentTypeCollection.Add("EquipmentType3");

                //comboxCCType.ItemsSource = CCTypeCollection;
                //comboxEquipmentType.ItemsSource = EquipmentTypeCollection;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void GetAllCCType()
        {
            try
            {
                if (allBehaviorType == null)
                {

                    var safetySubclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                           from type in assembly.GetTypes()
                                           where type.IsSubclassOf(typeof(SequenceSafety))
                                           select type;

                    var behaviorSubclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                             from type in assembly.GetTypes()
                                             where type.IsSubclassOf(typeof(SequenceBehavior))
                                             select type;

                    List<Type> allType = new List<Type>() {typeof(SequenceBehaviorStruct),  typeof(BehaviorGroupItem),
                                                      typeof(BehaviorGroupItem),
                                                      typeof(SequenceBehaviors) };
                    foreach (var v in safetySubclasses)
                    {
                        allType.Add(v);
                    }

                    foreach (var v in behaviorSubclasses)
                    {
                        allType.Add(v);
                    }

                    allBehaviorType = allType.ToArray();
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public int Next()
        {
            int ret = 0;
            try
            {

                if (ppSettingControl.Visibility == Visibility.Visible)
                {
                    ppSettingControl.Visibility = Visibility.Hidden;
                    orderSettingControl.Visibility = Visibility.Visible;
                    ret = 1;
                }
                else if (orderSettingControl.Visibility == Visibility.Visible)
                {
                    for (int i = 0; i < orderSettingControl.OrderCollection.Count; i++)
                    {
                        if (orderSettingControl.OrderCollection[i].NextID_Negative == null
                            || orderSettingControl.OrderCollection[i].NextID_Negative == "")
                        {
                            if ((orderSettingControl.OrderCollection.Count - 1) <= i)
                            {
                                orderSettingControl.OrderCollection[i].NextID_Positive = null;
                                orderSettingControl.OrderCollection[i].NextID_Negative = null;
                            }
                            else
                            {
                                orderSettingControl.OrderCollection[i].NextID_Positive = orderSettingControl.OrderCollection[i + 1].BehaviorID;

                                orderSettingControl.OrderCollection[i].NextID_Negative = orderSettingControl.OrderCollection[i + 1].BehaviorID;
                            }
                        }
                        else
                        {
                            bool IsExist = false;
                            foreach (var v in orderSettingControl.OrderCollection)
                            {
                                if (orderSettingControl.OrderCollection[i].NextID_Positive == v.BehaviorID)
                                {
                                    IsExist = true;
                                }
                            }

                            if (!IsExist)
                            {
                                if ((orderSettingControl.OrderCollection.Count - 1) <= i)
                                {
                                    orderSettingControl.OrderCollection[i].NextID_Positive = null;
                                }
                                else
                                {
                                    orderSettingControl.OrderCollection[i].NextID_Positive = orderSettingControl.OrderCollection[i + 1].BehaviorID;
                                }
                            }

                            IsExist = false;
                            foreach (var v in orderSettingControl.OrderCollection)
                            {
                                if (orderSettingControl.OrderCollection[i].NextID_Negative == v.BehaviorID)
                                {
                                    IsExist = true;
                                }
                            }

                            if (!IsExist)
                            {
                                if ((orderSettingControl.OrderCollection.Count - 1) <= i)
                                {
                                    orderSettingControl.OrderCollection[i].NextID_Negative = null;
                                }
                                else
                                {
                                    orderSettingControl.OrderCollection[i].NextID_Negative = orderSettingControl.OrderCollection[i + 1].BehaviorID;
                                }
                            }
                        }
                    }

                    desideNextBehaviorControl.OrderCardChangeCollection = orderSettingControl.OrderCollection;

                    desideNextBehaviorControl.Visibility = Visibility.Visible;
                    orderSettingControl.Visibility = Visibility.Hidden;
                    ret = 2;
                }
                else if (desideNextBehaviorControl.Visibility == Visibility.Visible)
                {
                    ppSettingControl.Visibility = Visibility.Visible;
                    desideNextBehaviorControl.Visibility = Visibility.Hidden;
                    ret = 0;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        public int Prev()
        {
            int ret = 0;
            try
            {

                if (ppSettingControl.Visibility == Visibility.Visible)
                {
                    ppSettingControl.Visibility = Visibility.Hidden;
                    desideNextBehaviorControl.Visibility = Visibility.Visible;
                    ret = 2;
                }
                else if (orderSettingControl.Visibility == Visibility.Visible)
                {
                    ppSettingControl.Visibility = Visibility.Visible;
                    orderSettingControl.Visibility = Visibility.Hidden;
                    ret = 0;
                }
                else if (desideNextBehaviorControl.Visibility == Visibility.Visible)
                {
                    orderSettingControl.Visibility = Visibility.Visible;
                    desideNextBehaviorControl.Visibility = Visibility.Hidden;
                    ret = 1;
                }

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return ret;
        }

        //private void btnNext_Click(object sender, RoutedEventArgs e)
        //{
        //    if (ppSettingControl.Visibility == Visibility.Visible)
        //    {
        //        ppSettingControl.Visibility = Visibility.Hidden;
        //        orderSettingControl.Visibility = Visibility.Visible;
        //        orderSettingControl.VisibleButton();
        //    }
        //    else if (orderSettingControl.Visibility == Visibility.Visible)
        //    {
        //        ppSettingControl.Visibility = Visibility.Visible;
        //        orderSettingControl.Visibility = Visibility.Hidden;
        //        SaveMachineBaseFile();
        //    }
        //}

        public void SaveMachineBaseFile()
        {
            try
            {
                Dictionary<string, BehaviorGroupItem> dicBehavior = new Dictionary<string, BehaviorGroupItem>();

                foreach (var v in ppSettingControl.BehaviorCollection)
                {
                    v.Behavior.SetReverseBehavior();
                    dicBehavior.Add(v.Behavior.GetType().Name, BehaviorGroupItem.Clone(v));
                }

                foreach (var v in ppSettingControl.BehaviorCollection)
                {
                    if (v.Behavior.ReverseBehavior != null)
                    {
                        if (dicBehavior.ContainsKey(v.Behavior.ReverseBehavior.GetType().Name))
                            v.ReverseBehaviorGroupItem = BehaviorGroupItem.Clone(dicBehavior[v.Behavior.ReverseBehavior.GetType().Name]);
                    }
                }

                if (isSavePossible)
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    string fileName = comboxCCType.SelectedValue.ToString() + '_' + comboxEquipmentType.SelectedValue.ToString();
                    dlg.FileName = fileName;
                    dlg.DefaultExt = ".ccb";
                    dlg.Filter = "Card Chage Machine documents (.ccb)|*.ccb|All Files (*.*)|*.*";

                    Nullable<bool> result = dlg.ShowDialog();

                    if (result == true)
                    {
                        String ccsPath = dlg.FileName;

                        SequenceBehaviorStruct ccsXML = new SequenceBehaviorStruct();
                        ccsXML.CollectionBaseBehavior = ppSettingControl.BehaviorCollection;
                        ccsXML.BehaviorOrder = orderSettingControl.OrderCollection; ;

                        SaveToXML(ccsXML, ccsPath);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private EventCodeEnum SaveToXML(SequenceBehaviorStruct _ccsXML, string path)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;

            try
            {
                bool serializeVal = SerializeManager.Serialize(path, _ccsXML);
            }
            catch (Exception err)
            {
                retVal = EventCodeEnum.PARAM_ERROR;
                //LoggerManager.Error($String.Format("LoadStageCoordsParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return retVal;
        }

        private void btnloadData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ".ccb";
                openFileDialog.Filter = "Card Chage Machine documents (.ccb)|*.ccb|All Files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    String path = openFileDialog.FileName;
                    string fileName = openFileDialog.SafeFileName;
                    fileName = fileName.Split('.')[0];
                    comboxCCType.SelectedValue = fileName.Split('_')[0];
                    comboxEquipmentType.SelectedValue = fileName.Split('_')[1];

                    SequenceBehaviorStruct tempBehaviorList = GetBehaviorfromXML(path);

                    ObservableCollection<BehaviorGroupItem> tempControlList = (ObservableCollection<BehaviorGroupItem>)ppSettingControl.lbBehaviorList.ItemsSource;

                    foreach (var v in tempControlList)
                    {
                        bool _bRun = false;
                        int i = 0;

                        while (!_bRun && i < tempBehaviorList.CollectionBaseBehavior.Count)
                        {
                            if (v.ToString().Equals(tempBehaviorList.CollectionBaseBehavior[i].ToString()))
                            {
                                v.PreSafetyList = tempBehaviorList.CollectionBaseBehavior[i].PreSafetyList;
                                v.PostSafetyList = tempBehaviorList.CollectionBaseBehavior[i].PostSafetyList;
                                _bRun = true;
                            }
                            i++;
                        }
                    }
                    ppSettingControl.lbBehaviorList.SelectedIndex = -1;

                    orderSettingControl.OrderCollection = tempBehaviorList.BehaviorOrder;

                    ppSettingControl.IsEnabled = true;
                    orderSettingControl.IsEnabled = true;
                    desideNextBehaviorControl.IsEnabled = true;
                    isSavePossible = true;
                    //btnNext.IsEnabled = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public SequenceBehaviorStruct GetBehaviorfromXML(String path)
        {
            object deserializedObj = null;
            SequenceBehaviorStruct ccsXML = null;

            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }
                if (File.Exists(path) == true)
                {
                    //IParam tmpParam = null;
                    bool retVal = SerializeManager.Deserialize(path, out deserializedObj, deserializeObjType: typeof(SequenceBehaviorStruct));
                    //EventCodeEnum loadRet = Extensions_IParam.LoadParameter(null, ref tmpParam, typeof(SequenceBehaviorStruct), null, path);

                    ccsXML = deserializedObj as SequenceBehaviorStruct;
                }
            }
            catch (Exception err)
            {
                ccsXML = null;
                //LoggerManager.Error($String.Format("GetBehaviorfromXML(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return ccsXML;
        }

        public List<ObservableCollection<string>> GetTypeFromXML(String path)
        {
            List<ObservableCollection<string>> equiptypelist = null;

            try
            {
                if (Directory.Exists(System.IO.Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }
                if (File.Exists(path) == false)
                {
                    List<ObservableCollection<string>> SaveList = new List<ObservableCollection<string>>();
                    ObservableCollection<string> CCTypeColl = new ObservableCollection<string>();
                    CCTypeColl.Add("CCType1");
                    CCTypeColl.Add("CCType2");
                    CCTypeColl.Add("CCType3");

                    ObservableCollection<string> EquipmentTypeColl = new ObservableCollection<string>();
                    EquipmentTypeColl.Add("EquipmentType1");
                    EquipmentTypeColl.Add("EquipmentType2");
                    EquipmentTypeColl.Add("EquipmentType3");

                    ObservableCollection<string> THTypeColl = new ObservableCollection<string>();
                    THTypeColl.Add("THType1");
                    THTypeColl.Add("THType2");
                    THTypeColl.Add("THType3");

                    SaveList.Add(CCTypeColl);
                    SaveList.Add(EquipmentTypeColl);
                    SaveList.Add(THTypeColl);

                    EventCodeEnum errorCode = Extensions_IParam.SaveDataToJson(SaveList, path);
                }

                object tmpParam = null;
                EventCodeEnum ret = Extensions_IParam.LoadDataFromJson(ref tmpParam, typeof(List<ObservableCollection<string>>), path);

                equiptypelist = (List<ObservableCollection<string>>)tmpParam;
            }
            catch (Exception err)
            {
                equiptypelist = null;
                LoggerManager.Exception(err);

                throw;
            }

            return equiptypelist;
        }

        private void btnNewData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboxCCType.SelectedValue != null && comboxEquipmentType.SelectedValue != null)
                {
                    ppSettingControl.InitSettingControl();
                    orderSettingControl.InitSettingControl();
                    ppSettingControl.IsEnabled = true;
                    orderSettingControl.IsEnabled = true;
                    desideNextBehaviorControl.IsEnabled = true;
                    isSavePossible = true;
                    //btnNext.IsEnabled = true;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
