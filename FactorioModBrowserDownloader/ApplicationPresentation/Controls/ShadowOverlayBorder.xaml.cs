using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    public partial class ShadowOverlayBorder : UserControl
    {
        public double CornerRadius
        {
            get => (double)GetValue(CornerRadiusProperty);
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
            //DataContext = this;
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(object), typeof(ShadowOverlayBorder),
            new PropertyMetadata(0d));

        public static readonly DependencyProperty ShadowDirectionProperty = DependencyProperty.Register(
            nameof(ShadowDirection), typeof(object), typeof(ShadowOverlayBorder),
            new PropertyMetadata(0d));

        public static readonly DependencyProperty ShadowDepthProperty = DependencyProperty.Register(
            nameof(ShadowDepth), typeof(object), typeof(ShadowOverlayBorder),
            new PropertyMetadata(0d));
    }
}
