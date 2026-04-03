using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace assignment05;
public partial class MainWindow : Window
{

    private string _expression = "";
    public MainWindow()
    {
        InitializeComponent();
    }
    private void Number_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        if (Display.Text == "0") Display.Text = "";
        Display.Text += btn.Content.ToString();
        _expression += btn.Content.ToString();
    }

    private void Operator_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        _expression += btn.Content.ToString();
        Display.Text += btn.Content.ToString();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _expression = "";
        Display.Text = "0";
    }

    private void Equal_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DataTable dt = new DataTable();
            var result = dt.Compute(_expression, "");

            Display.Text = result.ToString();
            _expression = result.ToString();
        }
        catch
        {
            Display.Text = "错误";
            _expression = "";
        }
    }
}