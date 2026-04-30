using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using Crawler.Entities;
using Crawler.Services.DbService;

namespace Crawler.Views;

public class MyResources : UserControl 
{
    private readonly FileDbService _dbService = new();
    
    private AntdUI.Panel _listContainer = null!;
    private AntdUI.Panel _emptyView = null!;
    private AntdUI.Button _btnLoadMore = null!;
    
    private int _currentPage = 1;
    private const int PageSize = 10;

    public MyResources()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.White;
    }

    public void InitUi()
    {
        this.Controls.Clear();

        var header = new AntdUI.Panel {
            Dock = DockStyle.Top,
            Height = 70,
            Padding = new Padding(20, 10, 20, 10)
        };
        
        var lblTitle = new AntdUI.Label {
            Text = "我的资源列表 - 管理已下载的音视频与文本资源",
            Font = new Font("Microsoft YaHei", 12, FontStyle.Bold),
            Dock = DockStyle.Left,
            AutoSize = true
        };

        var btnBack = new AntdUI.Button {
            Text = "返回",
            Type = AntdUI.TTypeMini.Default,
            Dock = DockStyle.Right,
            Size = new Size(80, 40)
        };
        btnBack.Click += (s, e) => Router.Router.Instance.GoTo("home");
        
        header.Controls.Add(lblTitle);
        header.Controls.Add(btnBack);

        _btnLoadMore = new AntdUI.Button {
            Text = "加载更多",
            Dock = DockStyle.Bottom,
            Height = 50,
            Type = AntdUI.TTypeMini.Default,
            Ghost = true,
            Visible = false
        };
        _btnLoadMore.Click += (s, e) => LoadData();

        _emptyView = new AntdUI.Panel {
            Dock = DockStyle.Fill,
            Visible = false
        };
        var lblEmpty = new AntdUI.Label {
            Text = "暂无数据记录\n去爬取页面看看吧！",
            Font = new Font("微软雅黑", 12),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        _emptyView.Controls.Add(lblEmpty);

        _listContainer = new AntdUI.Panel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20)
        };

        this.Controls.Add(_listContainer);
        this.Controls.Add(_emptyView);
        this.Controls.Add(header);
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
                _emptyView.Visible = true;
                _listContainer.Visible = false;
                _btnLoadMore.Visible = false;
                return;
            }

            _emptyView.Visible = false;
            _listContainer.Visible = true;

            foreach (var file in files)
            {
                var item = CreateFileItem(file);
                _listContainer.Controls.Add(item);
                item.BringToFront(); 
            }

            _btnLoadMore.Visible = (files.Count == PageSize);
            _currentPage++;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载失败: {ex.Message}");
        }
    }

    private Control CreateFileItem(FileEntity file)
    {
        var card = new AntdUI.Panel {
            Size = new Size(0, 100),
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 12),
            Radius = 8, 
            BorderWidth = 1f
        };

        var lblTitle = new AntdUI.Label {
            Text = file.Title ?? "未知文件名",
            Font = new Font("微软雅黑", 12, FontStyle.Bold),
            Location = new Point(15, 15),
            AutoSize = true
        };

        var tagType = new AntdUI.Button {
            Text = file.Type,
            Type = file.Type == "Video" ? AntdUI.TTypeMini.Primary : AntdUI.TTypeMini.Success,
            Location = new Point(15, 45),
            Size = new Size(60, 24),
            Radius = 4,
            Ghost = true
        };

        var lblInfo = new AntdUI.Label {
            Text = $"大小: {file.FileSize}  |  时间: {file.DownloadTime:yyyy-MM-dd}",
            ForeColor = Color.Gray,
            Font = new Font("微软雅黑", 9),
            Location = new Point(90, 48),
            AutoSize = true
        };

        var btnOpen = new AntdUI.Button {
            Text = "打开位置",
            Type = AntdUI.TTypeMini.Primary,
            Ghost = true,
            Location = new Point(card.Width - 110, 30),
            Anchor = AnchorStyles.Right,
            Size = new Size(90, 32)
        };

        btnOpen.Click += (s, e) => {
            if (System.IO.File.Exists(file.LocalPath) || System.IO.Directory.Exists(file.LocalPath)) {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{file.LocalPath}\"");
            } else {
                MessageBox.Show("文件或目录已被移动或删除");
            }
        };

        card.Controls.Add(lblTitle);
        card.Controls.Add(tagType);
        card.Controls.Add(lblInfo);
        card.Controls.Add(btnOpen);

        return card;
    }
}