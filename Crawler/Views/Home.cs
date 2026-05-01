using System;
using System.Drawing;
using System.Windows.Forms;

namespace Crawler.Views {
    public class Home : UserControl {
        public void InitUi() {
            this.Controls.Clear();
            var container = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                Padding = new Padding(40),
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            container.Controls.Add(CreateCard("开始爬取资源", "输入 URL 自动分析并提取多媒体文件。", Color.FromArgb(52, 152, 219), (s, e) => {
                (this.ParentForm as MainWindow)?.SwitchView(new Crawl());
            }));

            container.Controls.Add(CreateCard("本地资源库", "查看已下载的文件、路径及历史记录。", Color.FromArgb(155, 89, 182), (s, e) => {
                (this.ParentForm as MainWindow)?.SwitchView(new MyResources());
            }));

            container.Controls.Add(CreateCard("退出系统", "安全关闭爬虫管理后台。", Color.FromArgb(231, 76, 60), (s, e) => {
                if (MessageBox.Show("确定要退出程序吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    Application.Exit();
                }
            }));

            this.Controls.Add(container);
        }

        private Panel CreateCard(string title, string desc, Color theme, EventHandler click) {
            var card = new Panel { 
                Size = new Size(300, 200),
                BackColor = Color.White, 
                Margin = new Padding(15),
                Cursor = Cursors.Hand
            };

            card.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
            };

            var lblTitle = new Label { 
                Text = title, 
                Font = new Font("微软雅黑", 14, FontStyle.Bold), 
                ForeColor = theme, 
                Top = 25,
                Left = 20, 
                Width = 260,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblDesc = new Label { 
                Text = desc, 
                Top = 70,
                Left = 20, 
                Width = 250, 
                Height = 60, 
                ForeColor = Color.DimGray,
                Font = new Font("微软雅黑", 10)
            };
            
            var btn = new Button {
                Text = "立即进入",
                Dock = DockStyle.Bottom,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = theme,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += click;

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            card.Controls.Add(btn);

            card.Click += click; 
            
            return card;
        }
    }
}