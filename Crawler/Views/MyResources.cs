using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Crawler.Entities;
using Crawler.Services.DbService;
using Crawler.Router;

namespace Crawler.Views;

public class MyResources : UserControl
{
    private readonly FileDbService _dbService = new();
    private FlowLayoutPanel _listContainer;
    private Label _lblEmpty;
    private Button _btnLoadMore;
    
    private int _currentPage = 1;
    private const int PageSize = 10;

    public MyResources()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.FromArgb(245, 245, 245);
    }

    public void InitUi()
    {
        this.Controls.Clear();

        var navPanel = new Panel {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var btnBack = new Button {
            Text = "← 返回首页",
            Location = new Point(15, 15),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("微软雅黑", 9),
            Cursor = Cursors.Hand
        };
        btnBack.Click += (s, e) => Router.Router.Instance.GoTo("home");

        var lblTitle = new Label {
            Text = "我的资源列表",
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            Location = new Point(130, 16),
            AutoSize = true
        };

        navPanel.Controls.Add(btnBack);
        navPanel.Controls.Add(lblTitle);

        _btnLoadMore = new Button {
            Text = "点击加载更多...",
            Dock = DockStyle.Bottom,
            Height = 50,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            Font = new Font("微软雅黑", 10),
            Visible = false
        };
        _btnLoadMore.Click += (s, e) => LoadData();

        _lblEmpty = new Label {
            Text = "📁 暂无数据记录\n去爬取页面看看吧！",
            Font = new Font("微软雅黑", 12),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            Visible = false
        };
        _listContainer = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(20, 10, 20, 10)
        };

        this.Controls.Add(_listContainer);
        this.Controls.Add(_lblEmpty);
        this.Controls.Add(navPanel);
        this.Controls.Add(_btnLoadMore);

        LoadData();
    }

    private void LoadData()
    {
        try 
        {
            var files = _dbService.GetAllFiles(_currentPage, PageSize);

            if (_currentPage == 1 && (files == null || files.Count == 0))
            {
                _lblEmpty.Visible = true;
                _listContainer.Visible = false;
                _btnLoadMore.Visible = false;
                return;
            }

            _lblEmpty.Visible = false;
            _listContainer.Visible = true;

            foreach (var file in files)
            {
                _listContainer.Controls.Add(CreateFileItem(file));
            }

            _btnLoadMore.Visible = (files.Count == PageSize);
            _currentPage++;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载数据失败: {ex.Message}");
        }
    }

    private Control CreateFileItem(FileEntity file)
    {
        var itemPanel = new Panel {
            Size = new Size(_listContainer.Width - 60, 90),
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(15)
        };

        var lblName = new Label {
            Text = file.Title ?? "未知文件名",
            Font = new Font("微软雅黑", 11, FontStyle.Bold),
            Location = new Point(15, 15),
            Width = 500,
            AutoEllipsis = true
        };

        var lblDetail = new Label {
            Text = $"类型: {file.Type}  |  大小: {file.FileSize}  |  时间: {file.DownloadTime:yyyy-MM-dd HH:mm}",
            ForeColor = Color.Gray,
            Font = new Font("微软雅黑", 9),
            Location = new Point(15, 45),
            AutoSize = true
        };

        var btnOpen = new Button {
            Text = "打开位置",
            Size = new Size(90, 35),
            Location = new Point(itemPanel.Width - 110, 25),
            Anchor = AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        
        btnOpen.Click += (s, e) => {
            if (System.IO.File.Exists(file.LocalPath)) {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{file.LocalPath}\"");
            } else {
                MessageBox.Show("文件路径不存在！");
            }
        };

        itemPanel.Controls.Add(lblName);
        itemPanel.Controls.Add(lblDetail);
        itemPanel.Controls.Add(btnOpen);

        return itemPanel;
    }
}