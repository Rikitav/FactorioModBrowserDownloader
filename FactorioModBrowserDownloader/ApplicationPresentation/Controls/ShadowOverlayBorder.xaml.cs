using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    /// <summary>
    /// Логика взаимодействия для ShadowOverlayBorder.xaml
    /// </summary>
    public partial class ShadowOverlayBorder : UserControl
    {
        /// <summary>
        /// Gets or sets additional content for the UserControl
        /// </summary>
        public object AdditionalContent
        {
            get { return GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }

        public ShadowOverlayBorder()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register(
            "AdditionalContent", typeof(object), typeof(ShadowOverlayBorder),
            new PropertyMetadata(null));
    }
}
