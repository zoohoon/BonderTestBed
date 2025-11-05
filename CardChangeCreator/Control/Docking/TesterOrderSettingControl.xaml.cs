using SequenceRunner;
using Serial;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LogModule;

namespace CardChangeCreator
{
    /// <summary>
    /// OrderSettingControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TesterOrderSettingControl : UserControl
    {
        private Type[] behaviorTypes;

        public ObservableCollection<SequenceBehavior> orderLoad_InterfaceCollection;
        public ObservableCollection<SequenceBehavior> orderLoad_PreCollection;
        public ObservableCollection<SequenceBehavior> orderLoad_PostCollection;

        public ObservableCollection<SequenceBehavior> orderUnload_InterfaceCollection;
        public ObservableCollection<SequenceBehavior> orderUnload_PreCollection;
        public ObservableCollection<SequenceBehavior> orderUnload_PostCollection;

        private ObservableCollection<SequenceBehavior> behaviorCollection;


        public TesterOrderSettingControl()
        {
            try
            {
                InitializeComponent();


                orderLoad_InterfaceCollection = new ObservableCollection<SequenceBehavior>();
                orderLoad_PreCollection = new ObservableCollection<SequenceBehavior>();
                orderLoad_PostCollection = new ObservableCollection<SequenceBehavior>();
                orderUnload_InterfaceCollection = new ObservableCollection<SequenceBehavior>();
                orderUnload_PreCollection = new ObservableCollection<SequenceBehavior>();
                orderUnload_PostCollection = new ObservableCollection<SequenceBehavior>();

                behaviorCollection = new ObservableCollection<SequenceBehavior>();

                var behaviorSubclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                         from type in assembly.GetTypes()
                                         where type.IsSubclassOf(typeof(SequenceBehavior))
                                         select type;

                behaviorTypes = behaviorSubclasses.ToArray();

                foreach (var v in behaviorTypes)
                {
                    behaviorCollection.Add((SequenceBehavior)Activator.CreateInstance(v));
                }

                InitSettingControl();
                lbBehaviorList.ItemsSource = behaviorCollection;
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
                orderLoad_InterfaceCollection.Clear();
                orderLoad_PreCollection.Clear();
                orderLoad_PostCollection.Clear();
                orderUnload_InterfaceCollection.Clear();
                orderUnload_PreCollection.Clear();
                orderUnload_PostCollection.Clear();

                lbLoadInterfaceList.ItemsSource = orderLoad_InterfaceCollection;
                lbLoadPostTreatment.ItemsSource = orderLoad_PostCollection;
                lbLoadPreTreatment.ItemsSource = orderLoad_PreCollection;
                lbUnloadInterfaceList.ItemsSource = orderUnload_InterfaceCollection;
                lbUnloadPostTreatment.ItemsSource = orderUnload_PostCollection;
                lbUnloadPreTreatment.ItemsSource = orderUnload_PreCollection;
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

                if (data != null)
                {
                    DragDrop.DoDragDrop(parent, data, DragDropEffects.Copy);
                }
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private void lbOrderList_Drop(object sender, DragEventArgs e)
        {
            try
            {
                int i = 0;
                ListBox parent = (ListBox)sender;
                SequenceBehavior data = null;
                SequenceBehavior cloneBehaviorObject = null;

                //type 검색
                while (i < behaviorTypes.Length && data == null)
                {
                    data = (SequenceBehavior)e.Data.GetData(behaviorTypes[i].FullName); //여기가 문제
                    if (data == null)
                        i++;
                }

                cloneBehaviorObject = SequenceBehavior.Clone(data);
                cloneBehaviorObject.BehaviorID = SerialMaker.ByteToSerial(SerialMaker.MakeSerialByte(DateTime.Now.Ticks.ToString()));

                if (cloneBehaviorObject != null)
                {
                    if (parent.Tag.ToString().Equals("LoadInterface"))
                    {
                        orderLoad_InterfaceCollection.Add(cloneBehaviorObject);
                        lbLoadInterfaceList.ItemsSource = orderLoad_InterfaceCollection;
                    }
                    else if (parent.Tag.ToString().Equals("LoadPre"))
                    {
                        orderLoad_PreCollection.Add(cloneBehaviorObject);
                        lbLoadPreTreatment.ItemsSource = orderLoad_PreCollection;
                    }
                    else if (parent.Tag.ToString().Equals("LoadPost"))
                    {
                        orderLoad_PostCollection.Add(cloneBehaviorObject);
                        lbLoadPostTreatment.ItemsSource = orderLoad_PostCollection;
                    }
                    else if (parent.Tag.ToString().Equals("UnloadInterface"))
                    {
                        orderUnload_InterfaceCollection.Add(cloneBehaviorObject);
                        lbUnloadInterfaceList.ItemsSource = orderUnload_InterfaceCollection;
                    }
                    else if (parent.Tag.ToString().Equals("UnloadPre"))
                    {
                        orderUnload_PreCollection.Add(cloneBehaviorObject);
                        lbUnloadPreTreatment.ItemsSource = orderUnload_PreCollection;
                    }
                    else if (parent.Tag.ToString().Equals("UnloadPost"))
                    {
                        orderUnload_PostCollection.Add(cloneBehaviorObject);
                        lbUnloadPostTreatment.ItemsSource = orderUnload_PostCollection;
                    }
                }

            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private ListBox lbTemp;
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int idx = lbTemp.SelectedIndex;

                if (lbTemp.Equals(lbLoadInterfaceList))
                    orderLoad_InterfaceCollection.RemoveAt(idx);
                else if (lbTemp.Equals(lbLoadPreTreatment))
                    orderLoad_PreCollection.RemoveAt(idx);
                else if (lbTemp.Equals(lbLoadPostTreatment))
                    orderLoad_PostCollection.RemoveAt(idx);
                else if (lbTemp.Equals(lbUnloadInterfaceList))
                    orderUnload_InterfaceCollection.RemoveAt(idx);
                else if (lbTemp.Equals(lbUnloadPreTreatment))
                    orderUnload_PreCollection.RemoveAt(idx);
                else if (lbTemp.Equals(lbUnloadPostTreatment))
                    orderUnload_PostCollection.RemoveAt(idx);
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        private void lb_GotFocus(object sender, RoutedEventArgs e)
        {
            lbTemp = (ListBox)sender;
        }

        private void OrderChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;

                if (lbTemp != null)
                {
                    int idx = lbTemp.SelectedIndex;
                    ObservableCollection<SequenceBehavior> OrderCardChangeCollection = null;

                    if (lbTemp.Equals(lbLoadInterfaceList))
                        OrderCardChangeCollection = orderLoad_InterfaceCollection;
                    else if (lbTemp.Equals(lbLoadPreTreatment))
                        OrderCardChangeCollection = orderLoad_PreCollection;
                    else if (lbTemp.Equals(lbLoadPostTreatment))
                        OrderCardChangeCollection = orderLoad_PostCollection;
                    else if (lbTemp.Equals(lbUnloadInterfaceList))
                        OrderCardChangeCollection = orderUnload_InterfaceCollection;
                    else if (lbTemp.Equals(lbUnloadPreTreatment))
                        OrderCardChangeCollection = orderUnload_PreCollection;
                    else if (lbTemp.Equals(lbUnloadPostTreatment))
                        OrderCardChangeCollection = orderUnload_PostCollection;


                    if (-1 < idx && OrderCardChangeCollection != null)
                    {
                        if (btn?.Tag.ToString() == "FirstOfOrder")
                        {
                            SequenceBehavior tmp = OrderCardChangeCollection[idx];
                            OrderCardChangeCollection.RemoveAt(idx);
                            OrderCardChangeCollection.Insert(0, tmp);

                            lbTemp.SelectedIndex = 0;
                        }
                        else if (btn?.Tag.ToString() == "OnceUP")
                        {
                            SequenceBehavior tmp = OrderCardChangeCollection[idx];
                            OrderCardChangeCollection.RemoveAt(idx);

                            idx--;

                            if (idx < 1)
                                idx = 0;

                            OrderCardChangeCollection.Insert(idx, tmp);
                            lbTemp.SelectedIndex = idx;
                        }
                        else if (btn?.Tag.ToString() == "OnceDown")
                        {
                            SequenceBehavior tmp = OrderCardChangeCollection[idx];
                            OrderCardChangeCollection.RemoveAt(idx);

                            idx++;

                            if (OrderCardChangeCollection.Count < idx)
                                idx = OrderCardChangeCollection.Count;

                            OrderCardChangeCollection.Insert(idx, tmp);
                            lbTemp.SelectedIndex = idx;
                        }
                        else if (btn?.Tag.ToString() == "EndOfOrder")
                        {
                            SequenceBehavior tmp = OrderCardChangeCollection[idx];
                            OrderCardChangeCollection.RemoveAt(idx);
                            OrderCardChangeCollection.Add(tmp);
                            lbTemp.SelectedIndex = OrderCardChangeCollection.Count - 1;
                        }

                        OrderCardChangeCollection[OrderCardChangeCollection.Count - 1].NextID_Negative = null;
                        OrderCardChangeCollection[OrderCardChangeCollection.Count - 1].NextID_Positive = null;
                    }
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
