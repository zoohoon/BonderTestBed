using System;
using System.Collections.Generic;

namespace CUI
{
    using LogModule;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;

    public class TreeView : System.Windows.Controls.TreeView, INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public Guid GUID { get; set; }
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem == null)
                return;

            String curSelectedItem = SelectedItem.ToString();

            //String log = LogInterpreter.BuildLogCmdLine(
            //    CommandNameGen.Generate(typeof(ICUITreeViewCommand)),
            //    new CUITreeViewCommandParam(GUID.ToString(), curSelectedItem));

            String log = $"TreeView GUID:{GUID.ToString()} | SelectedItemChanged | {curSelectedItem}";
            LoggerManager.Debug(log);

            base.OnSelectedItemChanged(e);
        }

        public void OnSelectedItemChangedEventRaise(String text)
        {
            try
            {
                TreeViewItem selectedTreeViewItem = FindTreeViewItem(ItemContainerGenerator, Items, text);
                if (selectedTreeViewItem == null)
                    return;

                selectedTreeViewItem.IsSelected = true;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private TreeViewItem FindTreeViewItem(ItemContainerGenerator itemContainerGenerator, ItemCollection itemCollection, String text)
        {
            TreeViewItem findTreeViewItem = null;
            try
            {
                List<TreeViewItem> treeViewItems = new List<TreeViewItem>();
                foreach (var item in itemCollection)
                {
                    TreeViewItem treeViewItem = itemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    if (treeViewItem == null)
                        continue;
                    treeViewItems.Add(treeViewItem);
                }

                foreach (TreeViewItem treeViewItem in treeViewItems)
                {
                    findTreeViewItem = treeViewItem;
                    String treeViewItemHeader = treeViewItem.ToString();
                    if (treeViewItemHeader == text)
                        break;

                    findTreeViewItem = FindTreeViewItem(treeViewItem.ItemContainerGenerator, treeViewItem.Items, text);
                    if (findTreeViewItem != null)
                        break;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }

            return findTreeViewItem;
        }

    }
}
