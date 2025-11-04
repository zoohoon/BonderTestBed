namespace UIElementDropBebavior
{
    using LogModule;
    using System;
    using System.Windows;
    using System.Windows.Documents;

    public class AdornerManager
    {
        private readonly AdornerLayer adornerLayer;
        private readonly Func<UIElement, Adorner> adornerFactory;

        private Adorner adorner;

        public AdornerManager(
                  AdornerLayer adornerLayer,
                  Func<UIElement, Adorner> adornerFactory)
        {
            try
            {
                this.adornerLayer = adornerLayer;
                this.adornerFactory = adornerFactory;
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Update(UIElement adornedElement)
        {
            try
            {
                if (adorner == null || !adorner.AdornedElement.Equals(adornedElement))
                {
                    Remove();
                    adorner = adornerFactory(adornedElement);
                    adornerLayer.Add(adorner);
                    adornerLayer.Update(adornedElement);
                    adorner.Visibility = Visibility.Visible;
                }
            }
            catch (Exception err)
            {
                LoggerManager.Exception(err);
                throw;
            }
        }

        public void Remove()
        {
            try
            {
                if (adorner != null)
                {
                    adorner.Visibility = Visibility.Collapsed;
                    adornerLayer.Remove(adorner);
                    adorner = null;
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
