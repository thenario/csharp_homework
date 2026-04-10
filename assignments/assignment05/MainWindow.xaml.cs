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
/*

Algorithm CoinChange(coins, n, y):
    // coins: 硬币面值数组 [v1, v2, ..., vn]
    // n: 硬币种类数
    // y: 目标总价值
    
    // 创建并初始化 DP 数组
    Create array dp[y + 1]
    dp[0] = 0
    For i From 1 To y Do:
        dp[i] = Infinity // 初始化为无穷大
    // 动态规划计算
    For i From 1 To y Do:// 遍历每种目标金额
        For j From 0 To n-1 Do:// 遍历每种硬币面值
            If i >= coins[j] Then:
                // 如果当前硬币能用，更新最小值
                dp[i] = Min(dp[i], dp[i - coins[j]] + 1)
    // 3. 返回结果
    Return dp[y]

时间复杂度:O(n \times y)
空间复杂度:O(y)
*/