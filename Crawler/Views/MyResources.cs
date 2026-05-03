using System;
using System.Drawing;
using System.Windows.Forms;
using Crawler.Services.DbService;

namespace Crawler.Views
{
    public class MyResources : UserControl
    {
        private FlowLayoutPanel _list;
        
        // 分页UI控件
        private Button _btnPrev;
        private Button _btnNext;
        private Label _lblPageInfo;
        
        // 分页控制变量
        private int _currentPage = 1;
        private const int PageSize = 10;

        public void InitUi()
        {
            this.Controls.Clear();
            this.Padding = new Padding(0);

            // --- 1. 顶部标题 ---
            var header = new Label 
            { 
                Text = "本地已归档资源", 
                Dock = DockStyle.Top, 
                Height = 60, 
                Font = new Font("微软雅黑", 16, FontStyle.Bold), 
                TextAlign = ContentAlignment.MiddleLeft, 
                Padding = new Padding(20, 0, 0, 0) 
            };

            // --- 2. 底部操作区 (分页控制台) ---
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.Transparent };
            
            _btnPrev = new Button { Text = "上一页", Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, BackColor = Color.White };
            _btnNext = new Button { Text = "下一页", Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, BackColor = Color.White };
            _lblPageInfo = new Label { Text = "第 1 页", AutoSize = true, Font = new Font("微软雅黑", 11, FontStyle.Bold) };

            // 翻页事件绑定
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

            // 动态居中算法：无论窗口怎么拉伸，分页按钮始终居中
            bottomPanel.SizeChanged += (s, e) => UpdatePagerLayout(bottomPanel);

            // --- 3. 中间列表区 ---
            _list = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            // 注意添加顺序
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
            
            // 页码居中
            _lblPageInfo.Location = new Point(centerX - _lblPageInfo.Width / 2, 20);
            // 上一页在左侧
            _btnPrev.Location = new Point(_lblPageInfo.Left - _btnPrev.Width - 30, 12);
            // 下一页在右侧
            _btnNext.Location = new Point(_lblPageInfo.Right + 30, 12);
        }

        private void LoadPageData()
        {
            // 冻结渲染，避免清空和添加时的闪烁
            _list.SuspendLayout();
            
            // 核心修改：切换页面时，先清空掉上一页的所有旧数据
            _list.Controls.Clear(); 

            // 获取当前页的数据
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

            // 恢复渲染
            _list.ResumeLayout(true);
            
            // 翻页后，自动让列表滚动回最顶部！
            _list.AutoScrollPosition = new Point(0, 0);

            // --- 4. 更新底部分页控制台的状态 ---
            
            // 更新页码文字并重新居中计算
            _lblPageInfo.Text = $"第 {_currentPage} 页";
            UpdatePagerLayout(_btnPrev.Parent);

            // 如果是第一页，禁用“上一页”按钮
            _btnPrev.Enabled = _currentPage > 1;
            _btnPrev.BackColor = _btnPrev.Enabled ? Color.White : Color.FromArgb(240, 240, 240);

            // 如果本页获取到的数据不够一页(10条)，说明已经到最后一页了，禁用“下一页”
            _btnNext.Enabled = files.Count == PageSize;
            _btnNext.BackColor = _btnNext.Enabled ? Color.White : Color.FromArgb(240, 240, 240);

            // 体验优化：如果数据库彻底是空的
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