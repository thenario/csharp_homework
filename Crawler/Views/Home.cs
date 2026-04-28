using System;
using System.Windows.Forms;
using System.Drawing;
using AntdUI;
using Crawler.Router;

namespace Crawler.Views;

public class Home : UserControl
{
    public Home()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.White;
    }

    public void InitUi()
    {
        this.Controls.Clear();

        var stackLayout = new AntdUI.StackPanel {
            Dock = DockStyle.Fill,
            Vertical = true,
            Center = true,
            Gap = 20,
            Padding = new Padding(0, 0, 0, 50) 
        };


        var lblTitle = new AntdUI.Label {
            Text = "资源爬虫系统",
            Font = new Font("Microsoft YaHei", 28, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(400, 100),
            Margin = new Padding(0, 0, 0, 30)
        };

        var btnMyResources = CreateAntdButton("我的资源", TType.Primary, TIcon.FolderOpenOutlined);
        var btnStartCrawl = CreateAntdButton("开始爬取", TType.Success, TIcon.CloudDownloadOutlined);
        var btnExit = CreateAntdButton("退出程序", TType.Error, TIcon.PoweroffOutlined);

        btnMyResources.Click += (s, e) => Router.Router.Instance.GoTo("resources");
        btnStartCrawl.Click += (s, e) => Router.Router.Instance.GoTo("crawl");
        btnExit.Click += (s, e) => Application.Exit();

        stackLayout.Controls.Add(lblTitle);
        stackLayout.Controls.Add(btnMyResources);
        stackLayout.Controls.Add(btnStartCrawl);
        stackLayout.Controls.Add(btnExit);

        this.Controls.Add(stackLayout);
    }
    private AntdUI.Button CreateAntdButton(string text, TType type, string icon)
    {
        return new AntdUI.Button
        {
            Text = text,
            Type = type,
            Icon = icon,
            Size = new Size(240, 55),
            Font = new Font("Microsoft YaHei", 12),
            Radius = 10,
            Ghost = false,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 5, 0, 5)
        };
    }
}