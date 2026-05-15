using System;
using System.Drawing;
using System.Windows.Forms;

namespace Crawler.Views
{
    public partial class MainWindow : Form
    {
        private Panel sideBar;//侧边栏
        private Panel contentArea;//右边的内容区域
        private TableLayoutPanel navMenu;//导航栏内容区域
        private Color primaryColor = Color.FromArgb(44, 62, 80);//背景颜色
        private Color accentColor = Color.FromArgb(52, 152, 219);//

        public MainWindow()
        {
            this.Text = "全能媒体引擎 - 沉浸版";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;//设置窗体初始出现在屏幕上的位置，中心
            this.Font = new Font("Microsoft YaHei", 10);
            this.BackColor = Color.White;

            InitLayout();
            SwitchView(new Home());
        }

        private void InitLayout()
        {
            sideBar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = primaryColor };
            var lblLogo = new Label { Text = "CRAWLER", ForeColor = Color.White,Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Top, Height = 80, TextAlign = ContentAlignment.MiddleCenter };
            //设置导航栏区域
            //tablelayoutpanel可以设置行列，默认按照容器大小平均分
            navMenu = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8 };
            for(int i=0; i<9; i++) navMenu.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));//手动设置平均分

            AddNavButton("🏠 首页", 0, (s, e) => SwitchView(new Home()));
            //爬虫分类
            AddNavButton("📄 文本嗅探", 1, (s, e) => SwitchView(new TextCrawl()));
            AddNavButton("🖼️ 图片嗅探", 2, (s, e) => SwitchView(new ImageCrawl()));
            AddNavButton("🎬 音视频嗅探", 3, (s, e) => SwitchView(new MediaCrawl()));
            //媒体库分类
            AddNavButton("📚 文本库", 4, (s, e) => SwitchView(new ResourceLibrary("Text", "文本库")));
            AddNavButton("📸 图片库", 5, (s, e) => SwitchView(new ResourceLibrary("Image", "图片库")));
            AddNavButton("🎞️ 媒体库", 6, (s, e) => SwitchView(new ResourceLibrary("Video", "媒体库")));
            //系统
            AddNavButton("🚪 退出系统", 7, (s, e) => Application.Exit(), Color.FromArgb(231, 76, 60));

            sideBar.Controls.Add(navMenu);
            sideBar.Controls.Add(lblLogo);

            contentArea = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250) };
            this.Controls.Add(contentArea);
            this.Controls.Add(sideBar);
        }

        private void AddNavButton(string text, int row, EventHandler clickEvent, Color? overrideHover = null)
        {   
            //anchor代表锚定，即合谋一条边保持固定的相对距离
            //margin表示外边距
            //flat表示除去原本的样式，没有原本的3d感
            //cursor表示hover时的样式，即一只手
            //textalign表示文本的对齐方式，此处即正左
            var btn = new Button { Text = text, Height = 55, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(15, 0, 15, 0), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20,0,0,0), Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;//除去flat之后剩余边框
            btn.FlatAppearance.MouseOverBackColor = overrideHover ?? accentColor;//hover时的颜色
            btn.Click += clickEvent;//绑定点击事件
            navMenu.Controls.Add(btn, 0, row);//添加到0列，row行
        }

        public void SwitchView(UserControl view)
        {
            foreach (Control control in contentArea.Controls) control.Dispose();//先销毁内容区域里的所有组件
            contentArea.Controls.Clear();//清空页面
            view.Dock = DockStyle.Fill;
            contentArea.Controls.Add(view);

            view.GetType().GetMethod("InitUi")?.Invoke(view, null);//通过反射运行view的初始化
        }
    }
}