using LogModule;
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
using System.Windows.Forms;

namespace CardChangeMaker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, ObservableCollection<string>> dicCCTypeName;
        ObservableCollection<string> dicTesterTypeName;
        string directoryPath;
        private Type[] allBehaviorType;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                dicCCTypeName = new Dictionary<string, ObservableCollection<string>>();
                dicTesterTypeName = new ObservableCollection<string>();
                GetAllCCType();
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

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.ShowDialog();
                directoryPath = dialog.SelectedPath;

                cbxSequenceMainType.SelectedIndex = -1;
                cbxEquipmentType.SelectedIndex = -1;
                cbxTesterDockType.SelectedIndex = -1;
                cbxTesterUndockType.SelectedIndex = -1;

                foreach (var v in dicCCTypeName)
                    v.Value.Clear();
                dicCCTypeName.Clear();
                dicTesterTypeName.Clear();

                if (System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(directoryPath);

                    foreach (var item in di.GetFiles())
                    {
                        if (item.Extension.ToString().Equals(".ccb"))
                        {
                            if (!dicCCTypeName.ContainsKey(item.Name.Split('_')[0]))
                                dicCCTypeName.Add(item.Name.Split('_')[0], new ObservableCollection<string>() { item.Name.Split('_')[1].Split('.')[0] });
                            else
                                dicCCTypeName[item.Name.Split('_')[0]].Add(item.Name.Split('_')[1].Split('.')[0]);
                        }
                        else if (item.Extension.ToString().Equals(".thb"))
                        {
                            dicTesterTypeName.Add(item.Name.Split('.')[0]);
                        }
                    }

                    cbxSequenceMainType.ItemsSource = dicCCTypeName.Keys;
                    cbxTesterDockType.ItemsSource = dicTesterTypeName;
                    cbxTesterUndockType.ItemsSource = dicTesterTypeName;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void cbxSequenceMainType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                cbxEquipmentType.SelectedIndex = -1;

                if (cbxSequenceMainType.SelectedIndex != -1)
                    cbxEquipmentType.ItemsSource = dicCCTypeName[(string)cbxSequenceMainType.SelectedValue];
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void MakeCCSButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = 0;

                SequenceBehaviorStruct machineStructForXML;
                SequenceBehaviorStruct testerStructForXML;

                ObservableCollection<BehaviorGroupItem> mainSequenceBehaviorGroup = new ObservableCollection<BehaviorGroupItem>();
                ObservableCollection<BehaviorGroupItem> testerDockBehaviorGroup = new ObservableCollection<BehaviorGroupItem>();
                ObservableCollection<BehaviorGroupItem> testerUndockBehaviorGroup = new ObservableCollection<BehaviorGroupItem>();

                //Dictionary<string, BehaviorGroupItem> BehaviorDictionary = new Dictionary<string, BehaviorGroupItem>();

                if (cbxSequenceMainType.SelectedItem != null && cbxEquipmentType.SelectedItem != null)
                {
                    string loadMachinePath = directoryPath + "\\" + cbxSequenceMainType.SelectedValue.ToString() + "_" + cbxEquipmentType.SelectedValue.ToString() + ".ccb";

                    machineStructForXML = GetBehaviorfromXML(loadMachinePath);
                    foreach (var v in machineStructForXML.BehaviorOrder)
                    {
                        if (machineStructForXML.CollectionBaseBehavior.Count(i => i.Behavior.GetType() == v.GetType()) != 0)
                        {
                            BehaviorGroupItem tmpObj = machineStructForXML.CollectionBaseBehavior.First(i => i.Behavior.GetType() == v.GetType());
                            BehaviorGroupItem copyObj = CloneBehaviorInDic(tmpObj, v);
                            mainSequenceBehaviorGroup.Add(copyObj);
                        }
                    }
                }

                testerStructForXML = null;
                if (cbxTesterDockType.SelectedItem != null)
                {
                    string loadTesterPath = directoryPath + "\\" + cbxTesterDockType.SelectedValue.ToString() + ".thb";
                    testerStructForXML = GetBehaviorfromXML(loadTesterPath);

                    foreach (var v in testerStructForXML.BehaviorOrder)
                    {
                        if (testerStructForXML.CollectionBaseBehavior.Count(i => i.Behavior.GetType() == v.GetType()) != 0)
                        {
                            BehaviorGroupItem tmpObj = testerStructForXML.CollectionBaseBehavior.First(i => i.Behavior.GetType() == v.GetType());
                            BehaviorGroupItem copyObj = CloneBehaviorInDic(tmpObj, v);
                            testerDockBehaviorGroup.Add(copyObj);
                        }
                    }
                }

                testerStructForXML = null;
                if (cbxTesterDockType.SelectedItem != null)
                {
                    string loadTesterPath = directoryPath + "\\" + cbxTesterUndockType.SelectedValue.ToString() + ".thb";
                    testerStructForXML = GetBehaviorfromXML(loadTesterPath);

                    foreach (var v in testerStructForXML.BehaviorOrder)
                    {
                        if (testerStructForXML.CollectionBaseBehavior.Count(i => i.Behavior.GetType() == v.GetType()) != 0)
                        {
                            BehaviorGroupItem tmpObj = testerStructForXML.CollectionBaseBehavior.First(i => i.Behavior.GetType() == v.GetType());
                            BehaviorGroupItem copyObj = CloneBehaviorInDic(tmpObj, v);
                            testerUndockBehaviorGroup.Add(copyObj);
                        }
                    }
                }

                idx = 0;
                while (idx < mainSequenceBehaviorGroup.Count)
                {
                    if (mainSequenceBehaviorGroup[idx].Behavior.Flag == BehaviorFlag.DOCK)
                    {
                        string negId = null;
                        string posId = null;

                        if (string.IsNullOrEmpty(negId))
                        {
                            negId = mainSequenceBehaviorGroup[idx].Behavior.NextID_Negative;
                        }
                        if (string.IsNullOrEmpty(posId))
                        {
                            posId = mainSequenceBehaviorGroup[idx].Behavior.NextID_Positive;
                        }

                        mainSequenceBehaviorGroup.RemoveAt(idx);

                        foreach (var v in testerDockBehaviorGroup)
                        {
                            if (v.Behavior.Flag == BehaviorFlag.TH_INTERFACE)
                            {
                                BehaviorGroupItem copyObj = CloneBehaviorInDic(v, v.Behavior);
                                mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Positive = copyObj.BehaviorID;
                                mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Negative = copyObj.BehaviorID;
                                mainSequenceBehaviorGroup.Insert(idx++, copyObj);
                            }
                        }

                        if (0 < mainSequenceBehaviorGroup.Count)
                        {
                            mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Positive = posId;
                            mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Negative = negId;
                        }

                    }
                    else if (mainSequenceBehaviorGroup[idx].Behavior.Flag == BehaviorFlag.UNDOCK)
                    {
                        string negId = null;
                        string posId = null;

                        if (string.IsNullOrEmpty(negId))
                        {
                            negId = mainSequenceBehaviorGroup[idx].Behavior.NextID_Negative;
                        }
                        if (string.IsNullOrEmpty(posId))
                        {
                            posId = mainSequenceBehaviorGroup[idx].Behavior.NextID_Positive;
                        }

                        mainSequenceBehaviorGroup.RemoveAt(idx);

                        foreach (var v in testerUndockBehaviorGroup)
                        {
                            if (v.Behavior.Flag == BehaviorFlag.TH_INTERFACE)
                            {
                                BehaviorGroupItem copyObj = CloneBehaviorInDic(v, v.Behavior);
                                mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Positive = copyObj.BehaviorID;
                                mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Negative = copyObj.BehaviorID;
                                mainSequenceBehaviorGroup.Insert(idx++, copyObj);
                            }
                        }

                        if (0 < mainSequenceBehaviorGroup.Count)
                        {
                            mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Positive = posId;
                            mainSequenceBehaviorGroup[idx - 1].Behavior.NextID_Negative = negId;
                        }

                    }

                    idx++;
                }

                SaveMachineBaseFile(mainSequenceBehaviorGroup, testerDockBehaviorGroup, testerUndockBehaviorGroup);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private static BehaviorGroupItem CloneBehaviorInDic(BehaviorGroupItem BehaviorGroup, SequenceBehavior v)
        {
            BehaviorGroupItem tempItem = BehaviorGroupItem.Clone(BehaviorGroup);
            try
            {
                tempItem.Behavior.Flag = v.Flag;
                tempItem.Behavior.BehaviorID = v.BehaviorID;
                tempItem.Behavior.NextID_Negative = v.NextID_Negative;
                tempItem.Behavior.NextID_Positive = v.NextID_Positive;

            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return tempItem;
        }

        private void SaveMachineBaseFile(ObservableCollection<BehaviorGroupItem> SequenceList,
                                            ObservableCollection<BehaviorGroupItem> DockingSequenceList,
                                            ObservableCollection<BehaviorGroupItem> UndockingSequenceList)
        {
            try
            {
                SequenceBehaviors ccsStruct = new SequenceBehaviors();
                string fileName = null;
                Nullable<bool> result = null;

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                if (SequenceList != null &&
                    SequenceList.Count != 0)
                {
                    fileName = "CardChangeSequence.json";
                    dlg.FileName = fileName;
                    dlg.DefaultExt = ".json";
                    dlg.Filter = "JSON documents (.json)|*.json|All Files (*.*)|*.*";

                    result = dlg.ShowDialog();
                    if (result == true)
                    {
                        String xmlFullPath = dlg.FileName;

                        ccsStruct.SequenceBehaviorCollection = SequenceList;

                        if (SaveToXML(ccsStruct, xmlFullPath) < 0)
                        {
                            System.Windows.MessageBox.Show("Sequence Save Fail");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Sequence Save Success");
                        }
                    }
                }

                if (DockingSequenceList != null &&
                    DockingSequenceList.Count != 0)
                {
                    fileName = "THDockSequence.json";
                    dlg.FileName = fileName;
                    dlg.DefaultExt = ".json";
                    dlg.Filter = "JSON documents (.json)|*.json|All Files (*.*)|*.*";

                    result = dlg.ShowDialog();
                    if (result == true)
                    {
                        String xmlFullPath = dlg.FileName;

                        ccsStruct.SequenceBehaviorCollection = DockingSequenceList;

                        if (SaveToXML(ccsStruct, xmlFullPath) < 0)
                        {
                            System.Windows.MessageBox.Show("DockingSequenceList Save Fail");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("DockingSequenceList Save Success");
                        }
                    }
                }

                if (UndockingSequenceList != null &&
                    UndockingSequenceList.Count != 0)
                {
                    fileName = "THUndockSequence.json";
                    dlg.FileName = fileName;
                    dlg.DefaultExt = ".json";
                    dlg.Filter = "JSON documents (.json)|*.json|All Files (*.*)|*.*";

                    result = dlg.ShowDialog();
                    if (result == true)
                    {
                        String xmlFullPath = dlg.FileName;

                        ccsStruct.SequenceBehaviorCollection = UndockingSequenceList;

                        if (SaveToXML(ccsStruct, xmlFullPath) < 0)
                        {
                            System.Windows.MessageBox.Show("UndockingSequenceList Save Fail");
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("UndockingSequenceList Save Success");
                        }
                    }
                }



            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private EventCodeEnum SaveToXML(SequenceBehaviors _ccsXML, string path)
        {
            EventCodeEnum retVal = EventCodeEnum.NONE;
            try
            {

                try
                {
                    retVal = Extensions_IParam.SaveParameter(null, _ccsXML, null, path);
                    if (retVal != EventCodeEnum.NONE)
                    {
                        throw new Exception($"[{this.GetType().Name} - SaveToXML] Faile SaveParameter");
                    }


                }
                catch (Exception err)
                {
                    retVal = EventCodeEnum.PARAM_ERROR;
                    //LoggerManager.Error($String.Format("SaveToXML(): Error occurred while loading parameters. Err = {0}", err.Message));
                    LoggerManager.Exception(err);

                    throw;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
            return retVal;
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
    }
}
