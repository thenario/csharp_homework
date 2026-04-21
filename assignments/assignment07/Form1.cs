using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace assignment07;

public partial class Form1 : Form
{
    private static readonly HttpClient client = new HttpClient();

    public Form1()
    {
        InitializeComponent();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    }

    private async void btnSearch_Click(object sender, EventArgs e)
    {
        string keyword = txtKeyword.Text.Trim();
        if (string.IsNullOrEmpty(keyword)) return;

        btnSearch.Enabled = false;
        txtBaiduRes.Text = "正在搜索百度...";
        txtBingRes.Text = "正在搜索Bing...";

        try
        {
            Task<string> baiduTask = GetSearchSnippetAsync($"https://www.baidu.com/s?wd={keyword}");
            Task<string> bingTask = GetSearchSnippetAsync($"https://www.bing.com/search?q={keyword}");

            await Task.WhenAll(baiduTask, bingTask);

            txtBaiduRes.Text = await baiduTask;
            txtBingRes.Text = await bingTask;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"搜索出错: {ex.Message}");
        }
        finally
        {
            btnSearch.Enabled = true;
        }
    }

    private async Task<string> GetSearchSnippetAsync(string url)
    {
        try
        {
            string html = await client.GetStringAsync(url);

            html = Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", RegexOptions.IgnoreCase);

            string plainText = Regex.Replace(html, "<[^>]+>", "").Replace("&nbsp;", " ");

            plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

            return plainText.Length > 200 ? plainText.Substring(0, 200) : plainText;
        }
        catch (Exception)
        {
            return "抓取失败，请检查网络或搜索引擎接口。";
        }
    }
}
