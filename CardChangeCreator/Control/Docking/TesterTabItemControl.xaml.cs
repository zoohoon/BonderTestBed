using LogModule;
using Microsoft.Win32;
using ProberErrorCode;
using ProberInterfaces;
using ProberInterfaces.SequenceRunner;
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
    /// TesterTabItemControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TesterTabItemControl : UserControl
    {
        ObservableCollection<string> THTypeCollection;
        Type[] allBehaviorType;
        bool isSavePossible;

        public TesterTabItemControl()
        {
            try
            {
                InitializeComponent();

                THTypeCollection = new ObservableCollection<string>();

                List<ObservableCollection<string>> tempList = GetTypeFromXML(@"C:\ProberSystem\Parameters\CardChangeParameter\CC_Equip_Type.json");

                if (tempList != null)
                {
                    if (tempList.Count == 3)
                        comboxTHType.ItemsSource = tempList[2];
                }

                GetAllCCType();

                isSavePossible = false;
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

        private void btnNewData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (comboxTHType.SelectedValue != null)
                {
                    ppSettingControl.InitSettingControl();
                    testerOrderSettingControl.InitSettingControl();
                    ppSettingControl.IsEnabled = true;
                    testerOrderSettingControl.IsEnabled = true;
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
                else if (testerOrderSettingControl.Visibility == Visibility.Visible)
                {
                    ppSettingControl.Visibility = Visibility.Visible;
                    testerOrderSettingControl.Visibility = Visibility.Hidden;
                    ret = 0;
                }
                else if (desideNextBehaviorControl.Visibility == Visibility.Visible)
                {
                    testerOrderSettingControl.Visibility = Visibility.Visible;
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

        private void btnloadData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ".thb";
                openFileDialog.Filter = "Docking documents (.thb)|*.thb|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    String path = openFileDialog.FileName;

                    comboxTHType.SelectedValue = openFileDialog.SafeFileName.Split('.')[0];

                    SequenceBehaviorStruct tempBehaviorList = SetBehaviorfromXML(path);

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

                    ObservableCollection<SequenceBehavior> behaviorInterfaceList = new ObservableCollection<SequenceBehavior>();
                    ObservableCollection<SequenceBehavior> behaviorPreList = new ObservableCollection<SequenceBehavior>();
                    ObservableCollection<SequenceBehavior> behaviorPostList = new ObservableCollection<SequenceBehavior>();

                    foreach (var v in tempBehaviorList.BehaviorOrder)
                    {
                        if (v.Flag == BehaviorFlag.TH_INTERFACE)
                            behaviorInterfaceList.Add(v);
                        else if (v.Flag == BehaviorFlag.TH_BEFORE)
                            behaviorPreList.Add(v);
                        else if (v.Flag == BehaviorFlag.TH_AFTER)
                            behaviorPostList.Add(v);
                    }
                    testerOrderSettingControl.orderLoad_InterfaceCollection = behaviorInterfaceList;
                    testerOrderSettingControl.orderLoad_PreCollection = behaviorPreList;
                    testerOrderSettingControl.orderLoad_PostCollection = behaviorPostList;

                    //behaviorInterfaceList = new ObservableCollection<SequenceBehavior>();
                    //behaviorPreList = new ObservableCollection<SequenceBehavior>();
                    //behaviorPostList = new ObservableCollection<SequenceBehavior>();

                    //foreach (var v in tempBehaviorList.CollectionUnloadBehaviorOrder)
                    //{
                    //    if (v.Flag == BehaviorFlag.TH_INTERFACE)
                    //        behaviorInterfaceList.Add(v);
                    //    else if (v.Flag == BehaviorFlag.TH_BEFORE)
                    //        behaviorPreList.Add(v);
                    //    else if (v.Flag == BehaviorFlag.TH_AFTER)
                    //        behaviorPostList.Add(v);
                    //}
                    //testerOrderSettingControl.orderUnload_InterfaceCollection = behaviorInterfaceList;
                    //testerOrderSettingControl.orderUnload_PreCollection = behaviorPreList;
                    //testerOrderSettingControl.orderUnload_PostCollection = behaviorPostList;

                    testerOrderSettingControl.lbLoadInterfaceList.ItemsSource = testerOrderSettingControl.orderLoad_InterfaceCollection;
                    testerOrderSettingControl.lbLoadPreTreatment.ItemsSource = testerOrderSettingControl.orderLoad_PreCollection;
                    testerOrderSettingControl.lbLoadPostTreatment.ItemsSource = testerOrderSettingControl.orderLoad_PostCollection;
                    //testerOrderSettingControl.lbUnloadInterfaceList.ItemsSource = testerOrderSettingControl.orderUnload_InterfaceCollection;
                    //testerOrderSettingControl.lbUnloadPreTreatment.ItemsSource = testerOrderSettingControl.orderUnload_PreCollection;
                    //testerOrderSettingControl.lbUnloadPostTreatment.ItemsSource = testerOrderSettingControl.orderUnload_PostCollection;
                }

                ppSettingControl.IsEnabled = true;
                testerOrderSettingControl.IsEnabled = true;
                desideNextBehaviorControl.IsEnabled = true;
                isSavePossible = true;
                //btnNext.IsEnabled = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public SequenceBehaviorStruct SetBehaviorfromXML(String path)
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
                //LoggerManager.Error($String.Format("LoadStageCoordsParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return ccsXML;
        }


        public int Next()
        {
            int ret = 0;
            try
            {

                if (ppSettingControl.Visibility == Visibility.Visible)
                {
                    ppSettingControl.Visibility = Visibility.Hidden;
                    testerOrderSettingControl.Visibility = Visibility.Visible;
                    ret = 1;
                }
                else if (testerOrderSettingControl.Visibility == Visibility.Visible)
                {
                    ObservableCollection<SequenceBehavior> sendBehaviorList = new ObservableCollection<SequenceBehavior>();
                    foreach (var v in testerOrderSettingControl.orderLoad_PreCollection)
                    {
                        v.Flag = BehaviorFlag.TH_BEFORE;
                        sendBehaviorList.Add(v);
                    }
                    foreach (var v in testerOrderSettingControl.orderLoad_InterfaceCollection)
                    {
                        v.Flag = BehaviorFlag.TH_INTERFACE;
                        sendBehaviorList.Add(v);
                    }
                    foreach (var v in testerOrderSettingControl.orderLoad_PostCollection)
                    {
                        v.Flag = BehaviorFlag.TH_AFTER;
                        sendBehaviorList.Add(v);
                    }

                    for (int i = 0; i < sendBehaviorList.Count; i++)
                    {
                        if (sendBehaviorList[i].NextID_Negative == null
                            || sendBehaviorList[i].NextID_Negative == "")
                        {
                            if ((sendBehaviorList.Count - 1) <= i)
                            {
                                sendBehaviorList[i].NextID_Positive = null;
                                sendBehaviorList[i].NextID_Negative = null;
                            }
                            else
                            {
                                sendBehaviorList[i].NextID_Positive = sendBehaviorList[i + 1].BehaviorID;

                                sendBehaviorList[i].NextID_Negative = sendBehaviorList[i + 1].BehaviorID;
                            }
                        }
                        else
                        {
                            bool IsExist = false;
                            foreach (var v in sendBehaviorList)
                            {
                                if (sendBehaviorList[i].NextID_Positive == v.BehaviorID)
                                {
                                    IsExist = true;
                                }
                            }

                            if (!IsExist)
                            {
                                if ((sendBehaviorList.Count - 1) <= i)
                                {
                                    sendBehaviorList[i].NextID_Positive = null;
                                }
                                else
                                {
                                    sendBehaviorList[i].NextID_Positive = sendBehaviorList[i + 1].BehaviorID;
                                }
                            }

                            IsExist = false;
                            foreach (var v in sendBehaviorList)
                            {
                                if (sendBehaviorList[i].NextID_Negative == v.BehaviorID)
                                {
                                    IsExist = true;
                                }
                            }

                            if (!IsExist)
                            {
                                if ((sendBehaviorList.Count - 1) <= i)
                                {
                                    sendBehaviorList[i].NextID_Negative = null;
                                }
                                else
                                {
                                    sendBehaviorList[i].NextID_Negative = sendBehaviorList[i + 1].BehaviorID;
                                }
                            }
                        }
                    }

                    int loadCount = sendBehaviorList.Count;

                    foreach (var v in testerOrderSettingControl.orderUnload_PreCollection)
                    {
                        v.Flag = BehaviorFlag.TH_BEFORE;
                        sendBehaviorList.Add(v);
                    }
                    foreach (var v in testerOrderSettingControl.orderUnload_InterfaceCollection)
                    {
                        v.Flag = BehaviorFlag.TH_INTERFACE;
                        sendBehaviorList.Add(v);
                    }
                    foreach (var v in testerOrderSettingControl.orderUnload_PostCollection)
                    {
                        v.Flag = BehaviorFlag.TH_AFTER;
                        sendBehaviorList.Add(v);
                    }

                    for (int i = loadCount; i < sendBehaviorList.Count; i++)
                    {
                        if (sendBehaviorList[i].NextID_Negative == null
                            || sendBehaviorList[i].NextID_Negative == "")
                        {
                            if ((sendBehaviorList.Count - 1) <= i)
                            {
                                sendBehaviorList[i].NextID_Positive = null;
                                sendBehaviorList[i].NextID_Negative = null;
                            }
                            else
                            {
                                sendBehaviorList[i].NextID_Positive = sendBehaviorList[i + 1].BehaviorID;

                                sendBehaviorList[i].NextID_Negative = sendBehaviorList[i + 1].BehaviorID;
                            }
                        }
                        else
                        {
                            bool IsExist = false;
                            foreach (var v in sendBehaviorList)
                            {
                                if (sendBehaviorList[i].NextID_Positive == v.BehaviorID)
                                {
                                    IsExist = true;
                                }
                            }

                            if (!IsExist)
                            {
                                if ((sendBehaviorList.Count - 1) <= i)
                                {
                                    sendBehaviorList[i].NextID_Positive = null;
                                }
                                else
                                {
                                    sendBehaviorList[i].NextID_Positive = sendBehaviorList[i + 1].BehaviorID;
                                }
                            }

                            IsExist = false;
                            foreach (var v in sendBehaviorList)
                            {
                                if (sendBehaviorList[i].NextID_Negative == v.BehaviorID)
                                {
                                    IsExist = true;
                                }
                            }

                            if (!IsExist)
                            {
                                if ((sendBehaviorList.Count - 1) <= i)
                                {
                                    sendBehaviorList[i].NextID_Negative = null;
                                }
                                else
                                {
                                    sendBehaviorList[i].NextID_Negative = sendBehaviorList[i + 1].BehaviorID;
                                }
                            }
                        }
                    }

                    desideNextBehaviorControl.OrderCardChangeCollection = sendBehaviorList;


                    desideNextBehaviorControl.Visibility = Visibility.Visible;
                    testerOrderSettingControl.Visibility = Visibility.Hidden;
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

                    EventCodeEnum errorCode = EventCodeEnum.NONE;
                    errorCode = Extensions_IParam.SaveDataToJson(SaveList, path);
                    if (errorCode != EventCodeEnum.NONE)
                    {
                        throw new Exception($"[{this.GetType().Name} - GetTypeFromXML] Faile SaveParameter");
                    }
                }
                object tmpParam = null;
                EventCodeEnum ret = Extensions_IParam.LoadDataFromJson(ref tmpParam, typeof(List<ObservableCollection<string>>), path);

                equiptypelist = (List<ObservableCollection<string>>)tmpParam;
            }
            catch (Exception err)
            {
                equiptypelist = null;
                //LoggerManager.Error($String.Format("GetTypeFromXML(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }

            return equiptypelist;
        }

        //private void btnNext_Click(object sender, RoutedEventArgs e)
        //{
        //    if (ppSettingControl.Visibility == Visibility.Visible)
        //    {
        //        ppSettingControl.Visibility = Visibility.Hidden;
        //        testerOrderSettingControl.Visibility = Visibility.Visible;
        //    }
        //    else if (testerOrderSettingControl.Visibility == Visibility.Visible)
        //    {
        //        ppSettingControl.Visibility = Visibility.Visible;
        //        testerOrderSettingControl.Visibility = Visibility.Hidden;

        //        SaveMachineBaseFile();
        //    }
        //}

        public void SaveMachineBaseFile(string extendFileName = "")
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
                        if (dicBehavior.ContainsKey(v.Behavior.ReverseBehavior.GetType().Name))
                            v.ReverseBehaviorGroupItem = BehaviorGroupItem.Clone(dicBehavior[v.Behavior.ReverseBehavior.GetType().Name]);
                }

                if (isSavePossible)
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    string fileName = comboxTHType.SelectedValue.ToString();

                    if (!string.IsNullOrEmpty(extendFileName))
                    {
                        extendFileName = "_" + extendFileName;
                    }

                    dlg.FileName = fileName + extendFileName;
                    dlg.DefaultExt = ".thb";
                    dlg.Filter = "Tester documents (.thb)|*.thb|All Files (*.*)|*.*";

                    Nullable<bool> result = dlg.ShowDialog();

                    if (result == true)
                    {
                        String ccsPath = dlg.FileName;

                        SequenceBehaviorStruct ccsXML = new SequenceBehaviorStruct();
                        ObservableCollection<SequenceBehavior> dockList = new ObservableCollection<SequenceBehavior>();
                        ObservableCollection<SequenceBehavior> undockList = new ObservableCollection<SequenceBehavior>();

                        ccsXML.CollectionBaseBehavior = ppSettingControl.BehaviorCollection;

                        foreach (var v in testerOrderSettingControl.orderLoad_PreCollection)
                        {
                            v.Flag = BehaviorFlag.TH_BEFORE;
                            dockList.Add(v);
                        }
                        foreach (var v in testerOrderSettingControl.orderLoad_InterfaceCollection)
                        {
                            v.Flag = BehaviorFlag.TH_INTERFACE;
                            dockList.Add(v);
                        }
                        foreach (var v in testerOrderSettingControl.orderLoad_PostCollection)
                        {
                            v.Flag = BehaviorFlag.TH_AFTER;
                            dockList.Add(v);
                        }

                        foreach (var v in testerOrderSettingControl.orderUnload_PreCollection)
                        {
                            v.Flag = BehaviorFlag.TH_BEFORE;
                            undockList.Add(v);
                        }
                        foreach (var v in testerOrderSettingControl.orderUnload_InterfaceCollection)
                        {
                            v.Flag = BehaviorFlag.TH_INTERFACE;
                            undockList.Add(v);
                        }
                        foreach (var v in testerOrderSettingControl.orderUnload_PostCollection)
                        {
                            v.Flag = BehaviorFlag.TH_AFTER;
                            undockList.Add(v);
                        }


                        ccsXML.BehaviorOrder = dockList;
                        //ccsXML.CollectionUnloadBehaviorOrder = undockList;

                        SaveToXML(ccsXML, ccsPath);
                    }
                }
                //else
                //    MessageBox.Show("Select Combobox");
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
                retVal = EventCodeEnum.UNDEFINED;
                //LoggerManager.Error($String.Format("LoadStageCoordsParam(): Error occurred while loading parameters. Err = {0}", err.Message));
                LoggerManager.Exception(err);

                throw;
            }
            return retVal;
        }
    }
}
