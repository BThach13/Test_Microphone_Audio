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
    private FrequencyToNoteMapper _frequencyToNoteMapper;
    private Rectangle _innerRectangle;
    private Ellipse _circle;
    private TextBlock _note;
    private TextBlock _actualFrequency;

    public MainWindow()
    {
        InitializeComponent();
        _microphoneInputHandler = MicrophoneInputHandler.Instance;
        _frequencyToNoteMapper = FrequencyToNoteMapper.Instance;

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

        _circle = new Ellipse
        {
            Width = 100, 
            Height = 100,
            Fill = Brushes.Red,
            Stroke = Brushes.Black,

        };
        Canvas.SetTop(_circle, 150);
        Canvas.SetLeft(_circle, 250);
        canvas.Children.Add(_circle);

        // put the note in the middle of the circle
        _note = new TextBlock
        {
            Text = "X",
            FontSize = 20,
        };
        Canvas.SetTop(_note,185);
        Canvas.SetLeft(_note, 290);
        canvas.Children.Add(_note);


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

        UpdateLoop.Instance.Subscribe(SetNoteCircle);
    }

    private void SetInnerRectangleSize()
    {
        var volume = MicrophoneInputHandler.InputVolume;
        // debug output volume
        Debug.WriteLine($"Volume: {volume}");

        var newSize = Math.Round(volume * 10000);

        // map the volume the range of the inner rectangle
        newSize = Math.Max(0, newSize);
        newSize = Math.Min(300, newSize);


        _innerRectangle.Height = newSize;
    }

    private void SetActualFrequency()
    {
        var frequency = MicrophoneInputHandler.InputFrequency;
        // debug output frequency
        Debug.WriteLine($"Frequency: {frequency}");

        _actualFrequency.Text = $"Actual Frequency: {frequency}Hz";

    }

    private void SetNoteCircle()
    {
        var note = FrequencyToNoteMapper.GetClosestNote(MicrophoneInputHandler.InputFrequency);
        Debug.WriteLine($"The Note that is playing is: {note}");

        var noteName = note.Split(' ')[0];
        //regex to ignore the numbers in the note
        noteName = System.Text.RegularExpressions.Regex.Replace(noteName, @"\d", "");

        _circle.Fill = noteName switch
        {
            "C" => Brushes.Red,
            "C#/Db" => Brushes.DarkRed,
            "D" => Brushes.Orange,
            "D#/Eb" => Brushes.Yellow,
            "E" => Brushes.Green,
            "F" => Brushes.Blue,
            "F#/Gb" => Brushes.Purple,
            "G" => Brushes.Pink,
            "G#/Ab" => Brushes.Brown,
            "A" => Brushes.White,
            "A#/Bb" => Brushes.Gray,
            "B" => Brushes.LightGray,
            _ => Brushes.Black
        };
        
        _note.Text = noteName;

    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        UpdateLoop.Instance.Unsubscribe(SetInnerRectangleSize);
    }
}