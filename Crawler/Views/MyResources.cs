using System;
using System.Drawing;
using System.Windows.Forms;
using Crawler.Services.DbService;

namespace Crawler.Views
{
    public class MyResources : UserControl
    {
        private FlowLayoutPanel _list;
        
        private Button _btnPrev;
        private Button _btnNext;
        private Label _lblPageInfo;

        private int _currentPage = 1;
        private const int PageSize = 10;

        public void InitUi()
        {
            this.Controls.Clear();
            this.Padding = new Padding(0);

            var header = new Label 
            { 
                Text = "本地已归档资源", 
                Dock = DockStyle.Top, 
                Height = 60, 
                Font = new Font("微软雅黑", 16, FontStyle.Bold), 
                TextAlign = ContentAlignment.MiddleLeft, 
                Padding = new Padding(20, 0, 0, 0) 
            };

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.Transparent };
            
            _btnPrev = new Button { Text = "上一页", Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, BackColor = Color.White };
            _btnNext = new Button { Text = "下一页", Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, BackColor = Color.White };
            _lblPageInfo = new Label { Text = "第 1 页", AutoSize = true, Font = new Font("微软雅黑", 11, FontStyle.Bold) };

            _btnPrev.Click += (s, e) => {
                if (_currentPage > 1) {
                    _currentPage--;
                    LoadPageData();
                }
            };
            
            _btnNext.Click += (s, e) => {
                _currentPage++;
                LoadPageData();
            };

            bottomPanel.Controls.Add(_btnPrev);
            bottomPanel.Controls.Add(_lblPageInfo);
            bottomPanel.Controls.Add(_btnNext);

            bottomPanel.SizeChanged += (s, e) => UpdatePagerLayout(bottomPanel);

            _list = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            this.Controls.Add(_list);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(header);

            // 初始化加载第一页
            _currentPage = 1;
            LoadPageData();
        }

        // 抽取出来的居中排版方法
        private void UpdatePagerLayout(Control container)
        {
            if (container == null) return;
            int centerX = container.Width / 2;
            
            _lblPageInfo.Location = new Point(centerX - _lblPageInfo.Width / 2, 20);
            _btnPrev.Location = new Point(_lblPageInfo.Left - _btnPrev.Width - 30, 12);
            _btnNext.Location = new Point(_lblPageInfo.Right + 30, 12);
        }

        private void LoadPageData()
        {
            _list.SuspendLayout();
            
            _list.Controls.Clear(); 

            var files = new FileDbService().GetAllFiles(_currentPage, PageSize);

            foreach (var f in files)
            {
                var p = new Panel { Size = new Size(800, 70), Margin = new Padding(0, 0, 0, 10), BackColor = Color.White };
                p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);

                string typeTag = f.Type == "Video" ? "[视频]" : f.Type == "Image" ? "[图片]" : "[文本]";

                var title = new Label { Text = $"{typeTag} {f.Title}", Top = 15, Left = 20, AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
                var time = new Label { Text = f.DownloadTime.ToString("yyyy-MM-dd HH:mm") + $"  |  大小: {f.FileSize}", Top = 40, Left = 20, ForeColor = Color.Gray, AutoSize = true };

                var btn = new Button
                {
                    Text = "打开所在文件夹",
                    Size = new Size(130, 35),
                    Location = new Point(650, 18),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(240, 240, 240),
                    Cursor = Cursors.Hand
                };
                
                btn.Click += (s, e) => 
                {
                    try
                    {
                        System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + f.LocalPath + "\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("找不到文件或目录: " + ex.Message);
                    }
                };

                p.Controls.Add(title);
                p.Controls.Add(time);
                p.Controls.Add(btn);
                _list.Controls.Add(p);
            }

            _list.ResumeLayout(true);
            
            _list.AutoScrollPosition = new Point(0, 0);


            _lblPageInfo.Text = $"第 {_currentPage} 页";
            UpdatePagerLayout(_btnPrev.Parent);

            _btnPrev.Enabled = _currentPage > 1;
            _btnPrev.BackColor = _btnPrev.Enabled ? Color.White : Color.FromArgb(240, 240, 240);

            _btnNext.Enabled = files.Count == PageSize;
            _btnNext.BackColor = _btnNext.Enabled ? Color.White : Color.FromArgb(240, 240, 240);

            if (_currentPage == 1 && files.Count == 0)
            {
                _lblPageInfo.Text = "暂无归档";
                var emptyLabel = new Label { Text = "本地数据库空空如也~ 快去爬取一些资源吧！", AutoSize = true, Font = new Font("微软雅黑", 12), ForeColor = Color.Gray, Margin = new Padding(20) };
                _list.Controls.Add(emptyLabel);
                UpdatePagerLayout(_btnPrev.Parent);
            }
        }
    }
}