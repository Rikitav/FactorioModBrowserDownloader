using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FactorioNexus.PresentationFramework.Controls
{
    public class TabItemEx : TabItem
    {
        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            set => SetValue(IsPressedProperty, value);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            IsPressed = false;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            IsPressed = true;
        }

        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(
            nameof(IsPressed), typeof(bool), typeof(TabItemEx),
            new PropertyMetadata(false));
    }
}
