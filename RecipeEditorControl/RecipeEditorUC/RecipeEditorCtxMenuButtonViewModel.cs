using System;

namespace RecipeEditorControl.RecipeEditorUC
{
    using ProberInterfaces;

    using RelayCommandBase;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using System.Runtime.CompilerServices;
    using AccountModule;
    using LogModule;

    public class RecipeEditorCtxMenuButtonViewModel : RecipeEditorUCViewModel
    {
        #region ==> MenuListSource
        public ObservableCollection<ContextMenuItemViewModel> MenuListSource { get; set; }
        #endregion

        public delegate void SaveCommandDel(IElement elem);

        public SaveCommandDel SaveCommand { get; set; }

        public RecipeEditorCtxMenuButtonViewModel(IElement elem, String[] popupItemMenuLabel, SaveCommandDel saveCommand)
            : base(elem)
        {
            try
            {
                MenuListSource = new ObservableCollection<ContextMenuItemViewModel>();
                foreach (String popupMenuItemLabel in popupItemMenuLabel)
                {
                    ContextMenuItemViewModel menuItem = new ContextMenuItemViewModel(popupMenuItemLabel);
                    menuItem.CtxMenu = this;
                    MenuListSource.Add(menuItem);
                }

                SaveCommand = saveCommand;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }

    public class ContextMenuItemViewModel : INotifyPropertyChanged
    {
        #region ==> PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ==> MenuItemHeader
        private String _MenuItemHeader;
        public String MenuItemHeader
        {
            get { return _MenuItemHeader; }
            set
            {
                if (value != _MenuItemHeader)
                {
                    _MenuItemHeader = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region ==> MenuItemCommand
        private RelayCommand<Object> _MenuItemCommand;
        public ICommand MenuItemCommand
        {
            get
            {
                if (null == _MenuItemCommand) _MenuItemCommand = new RelayCommand<Object>(MenuItemCommandFunc);
                return _MenuItemCommand;
            }
        }
        private void MenuItemCommandFunc(Object senderMenu)
        {
            try
            {
                if (senderMenu == null)
                    return;

                ContextMenuItemViewModel menu = senderMenu as ContextMenuItemViewModel;
                if (menu == null)
                    return;

                if (CtxMenu.Elem.WriteMaskingLevel < AccountManager.CurrentUserInfo.UserLevel)
                    return;

                CtxMenu.SetElemValueBuffer(menu.MenuItemHeader);
                if (CtxMenu.FlushValueBuffer())
                    CtxMenu.SaveCommand(CtxMenu.Elem);
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
        #endregion

        public RecipeEditorCtxMenuButtonViewModel CtxMenu { get; set; }
        public ContextMenuItemViewModel(String menuItemHeader)
        {
            try
            {
                _MenuItemHeader = menuItemHeader;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }
    }
}
