using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FactorioNexus.UserInterface.Controls
{
    public class ClippingBorder : Border
    {
        private readonly RectangleGeometry _clipRect = new RectangleGeometry();
        private object? _oldClip;

        protected override void OnRender(DrawingContext dc)
        {
            OnApplyChildClip();
            base.OnRender(dc);
        }

        public override UIElement Child
        {
            get => base.Child;

            set
            {
                if (Child != value)
                {
                    Child?.SetValue(ClipProperty, _oldClip);
                    _oldClip = value != null ? ReadLocalValue(ClipProperty) : null;
                    base.Child = value;
                }
            }
        }

        protected virtual void OnApplyChildClip()
        {
            UIElement child = Child;
            if (child != null)
            {
                _clipRect.RadiusX = _clipRect.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - BorderThickness.Left * 0.5);
                _clipRect.Rect = new Rect(Child.RenderSize);
                child.Clip = _clipRect;
            }
        }
    }
}
