using SequenceRunner;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LogModule;

namespace CardChangeCreator
{
    /// <summary>
    /// Pre_Post_ProcessingSettingControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pre_Post_ProcessingSettingControl : UserControl, INotifyPropertyChanged
    {
        private void NotifyPropertyChange(String propName)
        {
            if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        private ObservableCollection<BehaviorGroupItem> behaviorCollection;
        public ObservableCollection<BehaviorGroupItem> BehaviorCollection
        {
            get { return behaviorCollection; }
            set
            {
                behaviorCollection = value;
                NotifyPropertyChange(nameof(BehaviorCollection));
            }
        }

        private ObservableCollection<SequenceSafety> safetyCollection;
        public ObservableCollection<SequenceSafety> SafetyCollection
        {
            get { return safetyCollection; }
            set
            {
                safetyCollection = value;
                NotifyPropertyChange(nameof(SafetyCollection));
            }
        }

        private ObservableCollection<SequenceSafety> preTreatmentCollection;
        public ObservableCollection<SequenceSafety> PreTreatmentCollection
        {
            get { return preTreatmentCollection; }
            set
            {
                preTreatmentCollection = value;
                NotifyPropertyChange(nameof(PreTreatmentCollection));
            }
        }

        private ObservableCollection<SequenceSafety> postTreatmentCollection;
        public ObservableCollection<SequenceSafety> PostTreatmentCollection
        {
            get { return postTreatmentCollection; }
            set
            {
                postTreatmentCollection = value;
                NotifyPropertyChange(nameof(PostTreatmentCollection));
            }
        }



        private Type[] safetyTypes;
        private Type[] behaviorTypes;

        public Pre_Post_ProcessingSettingControl()
        {
            try
            {
                InitializeComponent();

                behaviorCollection = new ObservableCollection<BehaviorGroupItem>();
                safetyCollection = new ObservableCollection<SequenceSafety>();

                var safetySubclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                       from type in assembly.GetTypes()
                                       where type.IsSubclassOf(typeof(SequenceSafety))
                                       select type;

                safetyTypes = safetySubclasses.ToArray();

                var behaviorSubclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                         from type in assembly.GetTypes()
                                         where type.IsSubclassOf(typeof(SequenceBehavior))
                                         select type;

                behaviorTypes = behaviorSubclasses.ToArray();

                foreach (var v in safetyTypes)
                {
                    SafetyCollection.Add((SequenceSafety)Activator.CreateInstance(v));
                }

                InitSettingControl();

                this.DataContext = this;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void InitSettingControl()
        {
            try
            {
                behaviorCollection.Clear();

                foreach (var v in behaviorTypes)
                {
                    BehaviorCollection.Add(new BehaviorGroupItem((SequenceBehavior)Activator.CreateInstance(v)));
                }

                lbBehaviorList.SelectedIndex = -1;
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        #region GetDataFromListBox(ListBox,Point)
        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
                try
                {
                    {
                        object data = DependencyProperty.UnsetValue;
                        while (data == DependencyProperty.UnsetValue)
                        {
                            data = source.ItemContainerGenerator.ItemFromContainer(element);
                            if (data == DependencyProperty.UnsetValue)
                            {
                                element = VisualTreeHelper.GetParent(element) as UIElement;
                            }
                            if (element == source)
                            {
                                return null;
                            }
                        }
                        if (data != DependencyProperty.UnsetValue)
                        {
                            return data;
                        }
                    }
                }
                catch (Exception err)
                {
                    System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                    throw;
                }
            return null;
        }

        #endregion

        private ListBox dragSource = null;
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListBox parent = (ListBox)sender;
                dragSource = parent;
                object data = GetDataFromListBox(dragSource, e.GetPosition(parent));
                DragDropSendData SendData = new DragDropSendData() { SendData = data };
                if (data != null)
                {
                    DragDrop.DoDragDrop(parent, SendData, DragDropEffects.Copy);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            try
            {
                int idx = lbBehaviorList.SelectedIndex;
                bool hasSelected = idx >= 0 ? true : false;

                if (hasSelected)
                {
                    ListBox parent = (ListBox)sender;
                    DragDropSendData data;
                    SequenceSafety cloneBehaviorObject = null;

                    data = (DragDropSendData)e.Data.GetData(typeof(DragDropSendData)); //여기가 문제

                    cloneBehaviorObject = SequenceSafety.Clone((SequenceSafety)data.SendData);

                    if (cloneBehaviorObject != null)
                    {
                        if (parent.Name.ToString().Equals("lbPreTreatment"))
                            behaviorCollection[idx].PreSafetyList.Add(cloneBehaviorObject);
                        else if (parent.Name.ToString().Equals("lbPostTreatment"))
                            behaviorCollection[idx].PostSafetyList.Add(cloneBehaviorObject);
                    }
                    //PreTreatmentCollection = behaviorCollection[idx].PreSafetyList;
                    //PostTreatmentCollection = behaviorCollection[idx].PostSafetyList;
                }
            } 
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private void lbBehaviorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int idx = lbBehaviorList.SelectedIndex;
                if (0 <= idx)
                {
                    //PreTreatmentCollection = behaviorCollection[idx].PreSafetyList;
                    //PostTreatmentCollection = behaviorCollection[idx].PostSafetyList;
                }
                else
                {
                    PreTreatmentCollection = null;
                    PostTreatmentCollection = null;
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private ListBox tbTemp;

        public event PropertyChangedEventHandler PropertyChanged;

        private void lb_GotFocus(object sender, RoutedEventArgs e)
        {
            tbTemp = (ListBox)sender;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int idx = tbTemp.SelectedIndex;

                if (tbTemp.Equals(lbPreTreatment))
                {
                    behaviorCollection[lbBehaviorList.SelectedIndex].PreSafetyList.RemoveAt(idx);
                }
                else if (tbTemp.Equals(lbPostTreatment))
                {
                    behaviorCollection[lbBehaviorList.SelectedIndex].PostSafetyList.RemoveAt(idx);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }
    }
}
