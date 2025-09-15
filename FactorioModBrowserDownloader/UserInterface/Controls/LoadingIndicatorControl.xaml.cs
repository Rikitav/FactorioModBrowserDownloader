using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FactorioNexus.UserInterface.Controls
{
    public partial class LoadingIndicatorControl : UserControl
    {
        public Brush IndicatorBackground
        {
            get => (Brush)GetValue(IndicatorBackgroundProperty);
            set => SetValue(IndicatorBackgroundProperty, value);
        }

        public Brush IndicatorForeground
        {
            get => (Brush)GetValue(IndicatorForegroundProperty);
            set => SetValue(IndicatorForegroundProperty, value);
        }

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
                case nameof(ActualHeight):
                    CenterX = ActualHeight / 2;
                    break;

                case nameof(ActualWidth):
                    CenterY = ActualWidth / 2;
                    break;
            }
        }

        public static readonly DependencyProperty IndicatorBackgroundProperty = DependencyProperty.Register(
            nameof(IndicatorBackground), typeof(Brush), typeof(LoadingIndicatorControl),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty IndicatorForegroundProperty = DependencyProperty.Register(
            nameof(IndicatorForeground), typeof(Brush), typeof(LoadingIndicatorControl),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CenterXProperty = DependencyProperty.Register(
            nameof(CenterX), typeof(double), typeof(LoadingIndicatorControl),
            new FrameworkPropertyMetadata((double)0));

        public static readonly DependencyProperty CenterYProperty = DependencyProperty.Register(
            nameof(CenterY), typeof(double), typeof(LoadingIndicatorControl),
            new FrameworkPropertyMetadata((double)0));
    }
}
