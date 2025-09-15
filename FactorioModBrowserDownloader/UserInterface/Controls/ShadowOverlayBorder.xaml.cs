using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.UserInterface.Controls
{
    public partial class ShadowOverlayBorder : UserControl
    {
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public double ShadowDirection
        {
            get => (double)GetValue(ShadowDirectionProperty);
            set => SetValue(ShadowDirectionProperty, value);
        }

        public double ShadowDepth
        {
            get => (double)GetValue(ShadowDepthProperty);
            set => SetValue(ShadowDepthProperty, value);
        }

        public ShadowOverlayBorder()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(ShadowOverlayBorder),
            new PropertyMetadata(default(CornerRadius)));

        public static readonly DependencyProperty ShadowDirectionProperty = DependencyProperty.Register(
            nameof(ShadowDirection), typeof(double), typeof(ShadowOverlayBorder),
            new PropertyMetadata(0d));

        public static readonly DependencyProperty ShadowDepthProperty = DependencyProperty.Register(
            nameof(ShadowDepth), typeof(double), typeof(ShadowOverlayBorder),
            new PropertyMetadata(0d));
    }
}
