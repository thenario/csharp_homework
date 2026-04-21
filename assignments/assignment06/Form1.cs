using System.Net.Http;
using System.Text.RegularExpressions;

namespace assignment06;

public partial class Form1 : Form
{
    private TextBox txtUrl = new();
    private Button btnFetch = new();
    private TextBox txtResult = new();
    private static readonly HttpClient client = new HttpClient();

    public Form1()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        this.Text = "网页信息提取器";
        this.Size = new Size(600, 450);

        txtUrl.Location = new Point(20, 20);
        txtUrl.Size = new Size(400, 25);
        txtUrl.PlaceholderText = "请输入URL";

        btnFetch.Text = "提取";
        btnFetch.Location = new Point(430, 18);
        btnFetch.Click += async (s, e) => await FetchData();

        txtResult.Location = new Point(20, 60);
        txtResult.Size = new Size(540, 330);
        txtResult.Multiline = true;
        txtResult.ScrollBars = ScrollBars.Vertical;
        txtResult.ReadOnly = true;

        this.Controls.Add(txtUrl);
        this.Controls.Add(btnFetch);
        this.Controls.Add(txtResult);
    }

    private async Task FetchData()
    {
        string url = txtUrl.Text.Trim();
        if (string.IsNullOrEmpty(url)) return;

        try
        {
            txtResult.Text = "正在获取数据...";
            string html = await client.GetStringAsync(url);

            string phonePattern = @"1[3-9]\d{9}";
            string emailPattern = @"[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+";

            var phones = Regex.Matches(html, phonePattern).Select(m => m.Value);
            var emails = Regex.Matches(html, emailPattern).Select(m => m.Value);

            txtResult.Text = $"手机号\r\n{string.Join("\r\n", phones.Distinct())}\r\n\r\n" +
                             $"邮箱\r\n{string.Join("\r\n", emails.Distinct())}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("错误: " + ex.Message);
        }
    }
}
