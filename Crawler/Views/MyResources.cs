using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Crawler.Services.DbService;

namespace Crawler.Views
{
    public class ResourceLibrary : UserControl
    {
        private FlowLayoutPanel _list;//列表容器
        private string _typeFilter;//文件类型
        private string _pageTitle;//页标题

        public ResourceLibrary(string typeFilter, string pageTitle)
        {
            _typeFilter = typeFilter;
            _pageTitle = pageTitle;
        }

        public void InitUi()
        {
            this.Controls.Clear();//清空控件
            this.Padding = new Padding(0);//内边距为零

            var header = new Label { Text = _pageTitle, Dock = DockStyle.Top, Height = 60, Font = new Font("微软雅黑", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0) };

            _list = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
            this.Controls.Add(_list);
            this.Controls.Add(header);

            LoadData();
        }

        private void LoadData()
        {
            _list.SuspendLayout();//先暂停他的渲染
            _list.Controls.Clear();//清空组件

            //查询该类的所有资源
            var files = new FileDbService().GetFilesByType(_typeFilter, 1, 15);

            foreach (var f in files)//对每一个文件创建panel并add到list里面
            {
                var p = new Panel { Size = new Size(900, 90), Margin = new Padding(0, 0, 0, 10), BackColor = Color.White };
                p.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);

                var title = new Label { 
                    Text = f.Title, 
                    Top = 15, Left = 20, 
                    AutoSize = true, 
                    MaximumSize = new Size(600, 0), //限定最大宽度
                    Font = new Font("微软雅黑", 11, FontStyle.Bold) 
                };
                
                var time = new Label { Text = f.DownloadTime.ToString("yyyy-MM-dd HH:mm") + $"  |  大小: {f.FileSize}", Top = 60, Left = 20, ForeColor = Color.Gray, AutoSize = true };

                var btnOpen = new Button { Text = "打开文件", Size = new Size(100, 35), Location = new Point(650, 25), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240), Cursor = Cursors.Hand };
                //打开资源管理器并定位文件所在
                // /select表示选中
                btnOpen.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + f.LocalPath + "\"");

                //删除按钮
                var btnDel = new Button { Text = "删除", Size = new Size(80, 35), Location = new Point(770, 25), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, Cursor = Cursors.Hand };
                btnDel.Click += (s, e) => {
                    if (MessageBox.Show("确定要从数据库和硬盘彻底删除该文件吗？"/*正文*/, "警告"/*窗口标题*/, MessageBoxButtons.YesNo/*提供是否两个选项*/, MessageBoxIcon.Warning/*黄色的感叹号警告标志*/) == DialogResult.Yes)
                    {
                        new FileDbService().DeleteFile(f.Id);
                        if (File.Exists(f.LocalPath)) File.Delete(f.LocalPath); // 同时删除物理硬盘文件
                        LoadData(); // 重新加载界面
                    }
                };

                p.Controls.Add(title);
                p.Controls.Add(time);
                p.Controls.Add(btnOpen);
                p.Controls.Add(btnDel);
                _list.Controls.Add(p);
            }

            if (files.Count == 0) _list.Controls.Add(new Label { Text = "该库空空如也~", AutoSize = true, Font = new Font("微软雅黑", 12), ForeColor = Color.Gray });

            _list.ResumeLayout(true);//恢复渲染
        }
    }
}