using System;
using System.Drawing;
using System.Windows.Forms;
using Crawler.Services.DbService;

namespace Crawler.Views
{
    public class MyResources : UserControl
    {
        private FlowLayoutPanel _list;

        public void InitUi()
        {
            this.Controls.Clear();
            var header = new Label { Text = "本地已归档资源", Dock = DockStyle.Top, Height = 60, Font = new Font("微软雅黑", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0) };

            _list = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            this.Controls.Add(_list);
            this.Controls.Add(header);

            LoadData();
        }

        private void LoadData()
        {
            var files = new FileDbService().GetAllFiles(1, 10);
            foreach (var f in files)
            {
                var p = new Panel { Size = new Size(800, 70), Margin = new Padding(0, 0, 0, 10), BackColor = Color.White };
                p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);

                var title = new Label { Text = f.Title, Top = 15, Left = 20, AutoSize = true, Font = new Font("微软雅黑", 10, FontStyle.Bold) };
                var time = new Label { Text = f.DownloadTime.ToString("yyyy-MM-dd"), Top = 40, Left = 20, ForeColor = Color.Gray };

                var btn = new Button
                {
                    Text = "定位文件",
                    Size = new Size(100, 35),
                    Location = new Point(680, 18),
                    FlatStyle = FlatStyle.Flat,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                btn.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", "/select," + f.LocalPath);

                p.Controls.Add(title);
                p.Controls.Add(time);
                p.Controls.Add(btn);
                _list.Controls.Add(p);
            }
        }
    }
}