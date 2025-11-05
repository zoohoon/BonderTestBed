using System;
using System.Linq;

namespace UIElementDropBebavior
{
    using ProberInterfaces;
    using System.Windows;
    using System.Windows.Interactivity;
    using RelayCommandBase;
    using ProberInterfaces.Vision;
    using LogModule;

    public class UIElementDropBehavior : Behavior<UIElement>, IFactoryModule
    {

        public static readonly DependencyProperty DropCommandProperty =
                                        DependencyProperty.Register("DropCommand",
                                         typeof(RelayCommand<object>),
                                         typeof(UIElementDropBehavior),
                                         new FrameworkPropertyMetadata(null));
        public RelayCommand<object> DropCommand
        {
            get { return (RelayCommand<object>)this.GetValue(UIElementDropBehavior.DropCommandProperty); }
            set
            {
                this.SetValue(UIElementDropBehavior.DropCommandProperty, value);
            }
        }

        public static readonly DependencyProperty DragOverCommandProperty =
                                DependencyProperty.Register("DragOverCommand",
                                 typeof(RelayCommand),
                                 typeof(UIElementDropBehavior),
                                 new FrameworkPropertyMetadata(null));
        public RelayCommand<object> DragOverCommand
        {
            get { return (RelayCommand<object>)this.GetValue(UIElementDropBehavior.DragOverCommandProperty); }
            set
            {
                this.SetValue(UIElementDropBehavior.DragOverCommandProperty, value);
            }
        }


        public static readonly DependencyProperty CamearaProperty =
                                DependencyProperty.Register("Cameara",
                                 typeof(ICamera),
                                 typeof(UIElementDropBehavior),
                                 new FrameworkPropertyMetadata(null));
        public ICamera Cameara
        {
            get { return (ICamera)this.GetValue(UIElementDropBehavior.CamearaProperty); }
            set
            {
                this.SetValue(UIElementDropBehavior.CamearaProperty, value);
            }
        }


        private VisionFileInfos fileinfos = new VisionFileInfos();

        public UIElementDropBehavior()
        {

        }

        protected override void OnAttached()
        {
            try
            {
                base.OnAttached();

                AssociatedObject.AllowDrop = true;
                AssociatedObject.DragOver += AssociatedObject_DragOver;
                AssociatedObject.Drop += AssociatedObject_Drop;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = { String.Empty };
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    string filepath = String.Concat(files);
                }


                fileinfos.CamType = Cameara.GetChannelType();
                fileinfos.FilePaths = files;

                if (files.Count() > 0)
                {
                    if (DropCommand != null)
                    {
                        DropCommand.Execute(fileinfos);
                    }
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (DragOverCommand != null)
                {
                    DragOverCommand.Execute(Cameara);
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
            }
        }


    }
}
