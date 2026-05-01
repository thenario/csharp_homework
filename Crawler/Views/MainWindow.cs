using System;
using System.Drawing;
using System.Windows.Forms;

namespace Crawler.Views
{
    public partial class MainWindow : Form
    {
        private Panel sideBar;
        private Panel contentArea;
        private Color primaryColor = Color.FromArgb(44, 62, 80); 
        private Color accentColor = Color.FromArgb(52, 152, 219);

        public MainWindow()
        {
            this.Text = "资源爬虫管理系统 - Native版";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei", 10);
            this.BackColor = Color.White;

            InitLayout();
            SwitchView(new Home());
        }

        private void InitLayout()
        {
            sideBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = primaryColor
            };

            var lblLogo = new Label
            {
                Text = "CRAWLER",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter
            };
            sideBar.Controls.Add(lblLogo);

            AddNavButton("首页", 80, (s, e) => SwitchView(new Home()));
            AddNavButton("资源爬取", 130, (s, e) => SwitchView(new Crawl()));
            AddNavButton("本地资源", 180, (s, e) => SwitchView(new MyResources()));

            contentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            this.Controls.Add(contentArea);
            this.Controls.Add(sideBar);
        }

        private void AddNavButton(string text, int top, EventHandler clickEvent)
        {
            var btn = new Button
            {
                Text = text,
                Top = top,
                Left = 0,
                Width = 200,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = accentColor;
            btn.Click += clickEvent;
            sideBar.Controls.Add(btn);
        }

        public void SwitchView(UserControl view)
        {
            foreach (Control control in contentArea.Controls)
            {
                control.Dispose();
            }
            contentArea.Controls.Clear();
            view.Dock = DockStyle.Fill;
            contentArea.Controls.Add(view);
            if (view is Home h) h.InitUi();
            if (view is Crawl c) c.InitUi();
            if (view is MyResources r) r.InitUi();
        }
    }
}