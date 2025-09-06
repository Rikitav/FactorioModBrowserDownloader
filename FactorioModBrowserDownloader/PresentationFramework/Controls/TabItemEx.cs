using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FactorioNexus.PresentationFramework.Controls
{
    public class TabItemEx : TabItem
    {
        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            set => SetValue(IsPressedProperty, value);
        }

        public Geometry Path
        {
            get => (Geometry)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
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
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            nameof(Path), typeof(Geometry), typeof(TabItemEx),
            new FrameworkPropertyMetadata(null));
    }
}
