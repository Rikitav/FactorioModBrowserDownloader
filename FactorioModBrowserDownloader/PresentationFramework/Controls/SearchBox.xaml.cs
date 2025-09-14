using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FactorioNexus.PresentationFramework.Controls
{
    public partial class SearchBox : UserControl
    {
        private readonly DispatcherTimer textChangeTimer;

        public TimeSpan TextChangeInterval
        {
            get => (TimeSpan)GetValue(TextChangeIntervalProperty);
            set => SetValue(TextChangeIntervalProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string IntervalledText
        {
            get => (string)GetValue(IntervalledTextProperty);
            set => SetValue(IntervalledTextProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public bool IsTextEmpty
        {
            get => (bool)GetValue(IsTextEmptyProperty);
            private set => SetValue(IsTextEmptyProperty, value);
        }

        public Brush PlaceholderForeground
        {
            get => (Brush)GetValue(PlaceholderForegroundProperty);
            set => SetValue(PlaceholderForegroundProperty, value);
        }

        public Thickness PlaceholderMargin
        {
            get => (Thickness)GetValue(PlaceholderMarginProperty);
            set => SetValue(PlaceholderMarginProperty, value);
        }

        public double PlaceholderFontSize
        {
            get => (double)GetValue(PlaceholderFontSizeProperty);
            set => SetValue(PlaceholderFontSizeProperty, value);
        }

        public ICommand? TextChangedCommand
        {
            get => (ICommand?)GetValue(TextChangedCommandProperty);
            set => SetValue(TextChangedCommandProperty, value);
        }

        public event TextChangedEventHandler TextChanged
        {
            add => AddHandler(TextChangedEvent, value);
            remove => RemoveHandler(TextChangedEvent, value);
        }

        public SearchBox()
        {
            InitializeComponent();
            
            textChangeTimer = new DispatcherTimer(DispatcherPriority.Background);
            textChangeTimer.Tick += TextChange_Tick;
        }

        public void FocusInput()
        {
            Keyboard.Focus(textBox);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = textBox.Text;
        }

        private void TextChange_Tick(object? sender, EventArgs e)
        {
            textChangeTimer.Stop();
            IntervalledText = Text;
            RaiseEvent(new TextChangedEventArgs(TextChangedEvent, UndoAction.None));
            TextChangedCommand?.Execute(Text);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.Property.Name)
            {
                case nameof(Text):
                    {
                        IsTextEmpty = string.IsNullOrEmpty(Text);
                        textChangeTimer.Stop();
                        textChangeTimer.Start();
                        break;
                    }

                case nameof(TextChangeInterval):
                    {
                        textChangeTimer.Interval = TextChangeInterval;
                        break;
                    }
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(SearchBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IntervalledTextProperty = DependencyProperty.Register(
            nameof(IntervalledText), typeof(string), typeof(SearchBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(SearchBox),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsTextEmptyProperty = DependencyProperty.Register(
            nameof(IsTextEmpty), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(true));

        public static readonly DependencyProperty PlaceholderForegroundProperty = DependencyProperty.Register(
            nameof(PlaceholderForeground), typeof(Brush), typeof(SearchBox),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PlaceholderMarginProperty = DependencyProperty.Register(
            nameof(PlaceholderMargin), typeof(Thickness), typeof(SearchBox),
            new PropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty PlaceholderFontSizeProperty = DependencyProperty.Register(
            nameof(PlaceholderFontSize), typeof(double), typeof(SearchBox),
            new PropertyMetadata(15d));

        public static readonly DependencyProperty TextChangeIntervalProperty = DependencyProperty.Register(
            nameof(TextChangeInterval), typeof(TimeSpan), typeof(SearchBox),
            new PropertyMetadata(TimeSpan.Zero));

        public static readonly DependencyProperty TextChangedCommandProperty = DependencyProperty.Register(
            nameof(TextChangedCommand), typeof(ICommand), typeof(SearchBox),
            new PropertyMetadata(null));

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(TextChanged), RoutingStrategy.Bubble,
            typeof(TextChangedEventHandler), typeof(SearchBox));
    }
}
