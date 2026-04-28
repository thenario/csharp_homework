using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using Crawler.Entities;
using Crawler.Services.DbService;

namespace Crawler.Views;

public class MyResources : UserContrl
{
    private readonly FileDbService _dbService = new();
    private AntdUI.In_List _listContainer;
    private AntdUI.Empty _emptyView;
    private AntdUI.Button _btnLoadMore;
    
    private int _currentPage = 1;
    private const int PageSize = 10;

    public MyResources()
    {
        this.Dock = DockStyle.Fill;
    }

    public void InitUi()
    {
        this.Controls.Clear();

        var header = new AntdUI.PageHeader {
            Text = "我的资源列表",
            SubText = "管理已下载的音视频与文本资源",
            Dock = DockStyle.Top,
            Height = 70,
            Padding = new Padding(20, 10, 20, 10),
            Divider = true
        };
        var btnBack = new AntdUI.Button {
            Text = "返回",
            Type = TType.Default,
            Icon = TIcon.ArrowLeftOutlined,
            Size = TSize.Small
        };
        btnBack.Click += (s, e) => Router.Router.Instance.GoTo("home");
        header.Extra = btnBack;

        _btnLoadMore = new AntdUI.Button {
            Text = "加载更多",
            Dock = DockStyle.Bottom,
            Height = 50,
            Type = TType.Ghost,
            Visible = false
        };
        _btnLoadMore.Click += (s, e) => LoadData();

        _emptyView = new AntdUI.Empty {
            Text = "暂无数据记录",
            Description = "去爬取页面看看吧！",
            Dock = DockStyle.Fill,
            Visible = false
        };

        _listContainer = new AntdUI.In_List {
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
                _listContainer.Controls.Add(CreateFileItem(file));
            }

            _btnLoadMore.Visible = (files.Count == PageSize);
            _currentPage++;
        }
        catch (Exception ex)
        {
            AntdUI.Message.error(this, $"加载失败: {ex.Message}");
        }
    }

    private Control CreateFileItem(FileEntity file)
    {
        var card = new AntdUI.Card {
            Size = new Size(0, 100),
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 12),
            Radius = 8, // 圆角
            Text = file.Title ?? "未知文件名",
        };

        var tagType = new AntdUI.Tag {
            Text = file.Type,
            Type = file.Type == "Video" ? TType.Primary : TType.Success,
            Location = new Point(15, 45)
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
            Type = TType.Primary,
            Ghost = true,
            Location = new Point(card.Width - 110, 30),
            Anchor = AnchorStyles.Right,
            Size = new Size(90, 32)
        };

        btnOpen.Click += (s, e) => {
            if (System.IO.File.Exists(file.LocalPath)) {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{file.LocalPath}\"");
            } else {
                AntdUI.Message.warn(this, "文件已被移动或删除");
            }
        };

        card.Controls.Add(tagType);
        card.Controls.Add(lblInfo);
        card.Controls.Add(btnOpen);

        return card;
    }
}