using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;

namespace ComponentVerificationDialog
{
    public partial class ComponentVerificationDialogUC : UserControl
    {
        ComponentVerificationDialogViewModel vm = null;
        public ComponentVerificationDialogUC()
        {
            InitializeComponent();
            vm = new ComponentVerificationDialogViewModel();
            this.DataContext = vm;

            ((INotifyCollectionChanged)LogListBox.Items).CollectionChanged += LogListBox_DataContextChanged;
            ((INotifyCollectionChanged)uiScenarioList.Items).CollectionChanged += ScenarioListBox_Changed;
            uiScenarioList.SelectionChanged += ScenarioListBox_Changed;
        }

        private void LogListBox_DataContextChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
            LogListBox.ScrollIntoView(LogListBox.SelectedItem);
        }

        private void ScenarioListBox_Changed(object sender, EventArgs e)
        {
            uiScenarioList.ScrollIntoView(uiScenarioList.SelectedItem);
        }
    }
}
