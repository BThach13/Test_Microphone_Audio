using System.Diagnostics;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Test_Microphone_Audio;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MicrophoneInputHandler _microphoneInputHandler;
    private Rectangle _innerRectangle;
    private Ellipse _circle;
    private TextBlock _actualFrequency;
    private bool _grow = true;
    public MainWindow()
    {
        InitializeComponent();
        _microphoneInputHandler = MicrophoneInputHandler.Instance;

        var canvas = new Canvas();
        Content = canvas;

        var title = new TextBlock
        {
            Text = "Hieronder staat een cirkel die van kleur moet veranderen met toonhoogte en \n een balk die meer of minder gevuld wordt aan de hand van volume",
            FontSize = 20,
            Margin = new Thickness(10, 10, 10, 10)
        };
        canvas.Children.Add(title);

        _actualFrequency = new TextBlock
        {
            Text = "Actual Frequency: 0Hz",
            FontSize = 20,
            Margin = new Thickness(10, 50, 10, 10)
        };
        Canvas.SetBottom(_actualFrequency, 10);
        Canvas.SetLeft(_actualFrequency, 10);
        canvas.Children.Add(_actualFrequency);

        _circle = new Ellipse { Width = 100, Height = 100, Fill = Brushes.Red };
        Canvas.SetTop(_circle, 150);
        Canvas.SetLeft(_circle, 250);
        canvas.Children.Add(_circle);

        var outerRectangle = new Rectangle
        {
            Width = 30,
            Height = 300,
            Fill = Brushes.Transparent,
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        Canvas.SetRight(outerRectangle, 20);
        Canvas.SetBottom(outerRectangle, 50);
        canvas.Children.Add(outerRectangle);

        _innerRectangle = new Rectangle
        {
            Width = 30,
            Height = 100,
            Fill = Brushes.Blue
        };
        Canvas.SetRight(_innerRectangle, 20);
        Canvas.SetBottom(_innerRectangle, 50); // Positioning it on top of the outerRectangle
        canvas.Children.Add(_innerRectangle);

        UpdateLoop.Instance.Subscribe(SetInnerRectangleSize);

        UpdateLoop.Instance.Subscribe(SetActualFrequency);
    }

    private void SetInnerRectangleSize()
    {
        // debug output volume
        Debug.WriteLine($"Volume: {MicrophoneInputHandler.InputVolume}");

        var newSize = Math.Round(MicrophoneInputHandler.InputVolume * 10000);

        // map the volume the range of the inner rectangle
        newSize = Math.Max(0, newSize);
        newSize = Math.Min(300, newSize);


        _innerRectangle.Height = newSize;
    }

    private void SetActualFrequency()
    {
        // debug output frequency
        Debug.WriteLine($"Frequency: {MicrophoneInputHandler.InputFrequency}");

        _actualFrequency.Text = $"Actual Frequency: {MicrophoneInputHandler.InputFrequency}Hz";

    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        UpdateLoop.Instance.Unsubscribe(SetInnerRectangleSize);
    }
}