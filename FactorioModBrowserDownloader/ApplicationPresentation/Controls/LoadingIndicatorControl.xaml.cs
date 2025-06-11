using System.Windows;
using System.Windows.Controls;

namespace FactorioNexus.ApplicationPresentation.Controls
{
    /// <summary>
    /// Логика взаимодействия для LoadingIndicatorControl.xaml
    /// </summary>
    public partial class LoadingIndicatorControl : UserControl
    {
        public double CenterX
        {
            get => (double)GetValue(CenterXProperty);
            set => SetValue(CenterXProperty, value);
        }

        public double CenterY
        {
            get => (double)GetValue(CenterYProperty);
            set => SetValue(CenterYProperty, value);
        }

        public LoadingIndicatorControl()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.Property.Name)
            {
                case nameof(Height):
                    CenterX = Height / 2;
                    break;

                case nameof(Width):
                    CenterY = Width / 2;
                    break;
            }
        }

        public static readonly DependencyProperty CenterXProperty = DependencyProperty.Register(
            nameof(CenterX), typeof(double), typeof(LoadingIndicatorControl),
            new PropertyMetadata((double)0));

        public static readonly DependencyProperty CenterYProperty = DependencyProperty.Register(
            nameof(CenterY), typeof(double), typeof(LoadingIndicatorControl),
            new PropertyMetadata((double)0));
    }
}
