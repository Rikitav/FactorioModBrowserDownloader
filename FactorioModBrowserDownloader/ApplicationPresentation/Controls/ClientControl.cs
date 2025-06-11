using FactorioNexus.ModPortal;
using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    public abstract class ClientControl : UserControl
    {
        public FactorioClient Client
        {
            get => (FactorioClient)GetValue(ClientProperty);
            set => SetValue(ClientProperty, value);
        }

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register(
            nameof(Client), typeof(FactorioClient), typeof(ClientControl),
            new PropertyMetadata(null));
    }
}
