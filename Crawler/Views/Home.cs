using System;
using System.Windows.Forms;
using System.Drawing;
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

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

        var lblTitle = new Label
        {
            Text = "爬虫系统",
            Font = new Font("Microsoft YaHei", 24, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
        };

        var btnMyResources = CreateButton("我的资源");
        var btnStartCrawl = CreateButton("开始爬取");
        var btnExit = CreateButton("退出程序");

        btnMyResources.Click += (s, e) => Router.Router.Instance.GoTo("resources");
        btnStartCrawl.Click += (s, e) => Router.Router.Instance.GoTo("crawl");
        btnExit.Click += (s, e) => Application.Exit();

        layout.Controls.Add(lblTitle, 0, 0);
        layout.Controls.Add(btnMyResources, 0, 1);
        layout.Controls.Add(btnStartCrawl, 0, 2);
        layout.Controls.Add(btnExit, 0, 3);

        this.Controls.Add(layout);
    }

    private Button CreateButton(string text)
    {
        return new Button
        {
            Text = text,
            Size = new Size(220, 50),
            Anchor = AnchorStyles.None,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei", 12),
            Cursor = Cursors.Hand
        };
    }
}