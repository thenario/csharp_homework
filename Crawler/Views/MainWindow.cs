using System;
using System.Drawing;
using System.Windows.Forms;

namespace Crawler.Views
{
    public partial class MainWindow : Form
    {
        private Panel sideBar;
        private Panel contentArea;
        private TableLayoutPanel navMenu; 
        private Color primaryColor = Color.FromArgb(44, 62, 80); 
        private Color accentColor = Color.FromArgb(52, 152, 219);

        public MainWindow()
        {
            this.Text = "全能媒体引擎 - 沉浸版";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei", 10);
            this.BackColor = Color.White;

            InitLayout();
            SwitchView(new Home());
        }

        private void InitLayout()
        {
            sideBar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = primaryColor };

            var lblLogo = new Label { Text = "CRAWLER", ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Top, Height = 80, TextAlign = ContentAlignment.MiddleCenter };
            sideBar.Controls.Add(lblLogo);

            navMenu = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 9 };
            for(int i=0; i<9; i++) navMenu.RowStyles.Add(new RowStyle(SizeType.Percent, 11.1f));

            AddNavButton("🏠 首页", 0, (s, e) => SwitchView(new Home()));
            // 爬虫分类
            AddNavButton("📄 文本嗅探", 1, (s, e) => SwitchView(new TextCrawl()));
            AddNavButton("🖼️ 图片嗅探", 2, (s, e) => SwitchView(new ImageCrawl()));
            AddNavButton("🎬 音视频嗅探", 3, (s, e) => SwitchView(new MediaCrawl()));
            // 媒体库分类
            AddNavButton("📚 文本库", 5, (s, e) => SwitchView(new ResourceLibrary("Text", "文本库")));
            AddNavButton("📸 图片库", 6, (s, e) => SwitchView(new ResourceLibrary("Image", "图片库")));
            AddNavButton("🎞️ 媒体库", 7, (s, e) => SwitchView(new ResourceLibrary("Video", "媒体库")));
            // 系统
            AddNavButton("🚪 退出系统", 8, (s, e) => Application.Exit(), Color.FromArgb(231, 76, 60));

            sideBar.Controls.Add(navMenu);

            contentArea = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250) };
            this.Controls.Add(contentArea);
            this.Controls.Add(sideBar);
        }

        private void AddNavButton(string text, int row, EventHandler clickEvent, Color? overrideHover = null)
        {
            var btn = new Button { Text = text, Height = 55, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(15, 0, 15, 0), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20,0,0,0), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = overrideHover ?? accentColor;
            btn.Click += clickEvent;
            navMenu.Controls.Add(btn, 0, row);
        }

        public void SwitchView(UserControl view)
        {
            foreach (Control control in contentArea.Controls) control.Dispose();
            contentArea.Controls.Clear();
            view.Dock = DockStyle.Fill;
            contentArea.Controls.Add(view);

            view.GetType().GetMethod("InitUi")?.Invoke(view, null);
        }
    }
}