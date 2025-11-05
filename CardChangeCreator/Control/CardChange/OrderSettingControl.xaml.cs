using SequenceRunner;
using Serial;
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
    /// OrderSettingControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OrderSettingControl : UserControl, INotifyPropertyChanged
    {
        private Type[] behaviorTypes;

        private ObservableCollection<SequenceBehavior> behaviorCollection;
        public ObservableCollection<SequenceBehavior> BehaviorCollection
        { get { return behaviorCollection; } set { behaviorCollection = value; NotifyPropertyChange(nameof(BehaviorCollection)); } }

        private ObservableCollection<SequenceBehavior> _OrderCollection;
        public ObservableCollection<SequenceBehavior> OrderCollection
        { get { return _OrderCollection; } set { _OrderCollection = value; NotifyPropertyChange(nameof(OrderCollection)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChange(String propName)
        {
            if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public OrderSettingControl()
        {
            try
            {
                InitializeComponent();

                behaviorCollection = new ObservableCollection<SequenceBehavior>();
                _OrderCollection = new ObservableCollection<SequenceBehavior>();

                var behaviorSubclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                         from type in assembly.GetTypes()
                                         where type.IsSubclassOf(typeof(SequenceBehavior))
                                         select type;

                behaviorTypes = behaviorSubclasses.ToArray();

                foreach (var v in behaviorTypes)
                {
                    behaviorCollection.Add((SequenceBehavior)Activator.CreateInstance(v));
                }

                this.DataContext = this;
                //lbBehaviorList.ItemsSource = behaviorCollection;

                InitSettingControl();
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLineIf(LoggerManager.GPTraceSwitch.TraceError, err);
                throw;
            }
        }

        public void InitSettingControl()
        {
            OrderCollection.Clear();
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
                    DragDropSendData tmpSendData = new DragDropSendData() { SendData = (SequenceBehavior)data };
                    DragDrop.DoDragDrop(parent, tmpSendData, DragDropEffects.Copy);
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
                ListBox parent = (ListBox)sender;
                DragDropSendData data;
                SequenceBehavior cloneBehaviorObject = null;

                data = (DragDropSendData)e.Data.GetData(typeof(DragDropSendData));
                cloneBehaviorObject = SequenceBehavior.Clone((SequenceBehavior)data.SendData);
                cloneBehaviorObject.BehaviorID = SerialMaker.ByteToSerial(SerialMaker.MakeSerialByte(DateTime.Now.Ticks.ToString()));
                if (cloneBehaviorObject != null)
                {
                    if (parent.Tag.ToString().Equals("Unload"))
                    {
                        OrderCollection.Add(cloneBehaviorObject);
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

                if (lbTemp.Equals(lbUnloadOrderList))
                {
                    OrderCollection.RemoveAt(idx);
                }
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

                    //if (lbTemp.Equals(lbUnloadOrderList))
                    //{
                    //    OrderCardChangeCollection.RemoveAt(idx);
                    //}

                    if (-1 < idx && lbTemp.Equals(lbUnloadOrderList))
                    {
                        if (btn?.Tag.ToString() == "FirstOfOrder")
                        {
                            SequenceBehavior tmp = OrderCollection[idx];
                            OrderCollection.RemoveAt(idx);
                            OrderCollection.Insert(0, tmp);

                            lbTemp.SelectedIndex = 0;
                        }
                        else if (btn?.Tag.ToString() == "OnceUP")
                        {
                            SequenceBehavior tmp = OrderCollection[idx];
                            OrderCollection.RemoveAt(idx);

                            idx--;

                            if (idx < 1)
                                idx = 0;

                            OrderCollection.Insert(idx, tmp);
                            lbTemp.SelectedIndex = idx;
                        }
                        else if (btn?.Tag.ToString() == "OnceDown")
                        {
                            SequenceBehavior tmp = OrderCollection[idx];
                            OrderCollection.RemoveAt(idx);

                            idx++;

                            if (OrderCollection.Count < idx)
                                idx = OrderCollection.Count;

                            OrderCollection.Insert(idx, tmp);
                            lbTemp.SelectedIndex = idx;
                        }
                        else if (btn?.Tag.ToString() == "EndOfOrder")
                        {
                            SequenceBehavior tmp = OrderCollection[idx];
                            OrderCollection.RemoveAt(idx);
                            OrderCollection.Add(tmp);
                            lbTemp.SelectedIndex = OrderCollection.Count - 1;
                        }

                        OrderCollection[OrderCollection.Count - 1].NextID_Negative = null;
                        OrderCollection[OrderCollection.Count - 1].NextID_Positive = null;
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
