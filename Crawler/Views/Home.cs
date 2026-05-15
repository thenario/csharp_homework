using System;
using System.Drawing;//图形与图像处理的包，设置颜色，尺寸，坐标
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;//各种桌面应用程序组件的包

namespace Crawler.Views 
{
    public class Home : UserControl //winforms.usercontorl，因为该view是作为mainwindow的组件，所以用usercontrol
    {
        public void InitUi() 
        {
            this.Controls.Clear();//先清空组件内容
            this.BackColor = Color.FromArgb(245, 247, 250);//修改背景颜色
            this.Padding = new Padding(40);//设置内边距
            //创建一个头部panel
            //top属性设置panel停靠在顶部
            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 130 };
            
            var lblWelcome = new Label {
                Text = "欢迎使用 全能媒体引擎",//label的文本内容
                Font = new Font("微软雅黑", 26, FontStyle.Bold),//字体，大小，样式
                ForeColor = Color.FromArgb(44, 62, 80),//前景颜色，即文字的颜色
                AutoSize = true,//按照内容自动撑开
                Location = new Point(0, 0)//设置坐标，左上角
            };

            var lblDesc = new Label {//依旧label
                Text = "这是一款基于原生 WinForms 打造的现代化爬虫管理系统。\n底层融合了无头浏览器、AI 正文智能识别及防盗链穿透技术，为您提供所见即所得的极客级抓取体验。\n\n👈 请从左侧导航菜单选择您需要的功能模块开始操作。",
                Font = new Font("微软雅黑", 11),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(5, 55)
            };
            //将组件添加到创建的panel里去
            headerPanel.Controls.Add(lblWelcome);
            headerPanel.Controls.Add(lblDesc);

            var cardContainer = new FlowLayoutPanel {//流式排布，自动布局
                Dock = DockStyle.Fill,//填充剩余空间
                AutoScroll = true,//当内容超出父组件范围时，显示滚轮以滑动
                Padding = new Padding(0, 20, 0, 0)//内边距，左上右下的顺时针
            };

            cardContainer.Controls.Add(CreateInfoCard(
                "📄 沉浸式文本嗅探", 
                "无视网页广告与防爬遮罩，智能提取文章纯净正文。支持所见即所得的文本预览、编辑与一键保存，告别二次请求的漫长等待。", 
                Color.FromArgb(52, 152, 219)
            ));

            cardContainer.Controls.Add(CreateInfoCard(
                "🖼️ 高清图片画廊", 
                "自动滚动深层隐藏页面，突破防盗链与懒加载限制提取原图。内置沉浸式画廊模式，支持分页无缝预览，确保所见即所下。", 
                Color.FromArgb(46, 204, 113)
            ));

            cardContainer.Controls.Add(CreateInfoCard(
                "🎬 强力音视频解析", 
                "智能嗅探网页深层隐藏播放器，无缝调度 yt-dlp 底层核心引擎。支持一键调用本地播放器预览，支持视音频分离纯净提取。", 
                Color.FromArgb(155, 89, 182)
            ));

            cardContainer.Controls.Add(CreateInfoCard(
                "📚 模块化本地图库", 
                "文本、图片、音视频全自动分类入库，支持 SQLite 本地化持久存储。提供自适应网格排版，支持一键打开物理路径与冗余资源清理。", 
                Color.FromArgb(241, 196, 15)
            ));

            //先Fill 再Top，引擎会从底层开始渲染
            this.Controls.Add(cardContainer);
            this.Controls.Add(headerPanel);
        }

        //生成说明卡片的方法
        private Panel CreateInfoCard(string title, string desc, Color themeColor) 
        {
            var card = new Panel { 
                Size = new Size(450, 160),//大小,先宽再高
                BackColor = Color.White,//背景颜色
                Margin = new Padding(0, 0, 30, 30)//右侧和下侧留白
            };

            //绘制卡片淡淡的灰色边框
            card.Paint += (s, e) => {//当card重绘时触发
                //e.Graphics表示画布引擎
                //card.ClientRectangle绘制区域，即以组件左上角为顶点的大小和组件一样的矩形
                //Color.FromArgb(230, 230, 230)边框颜色
                //ButtonBorderStyle.Solid边框线条样式，即实线样式
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
            };

            // 现代 UI 点缀：卡片左侧的高亮颜色条
            var leftBar = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = themeColor };
            card.Controls.Add(leftBar);

            var lblTitle = new Label { 
                Text = title, 
                Font = new Font("微软雅黑", 14, FontStyle.Bold), 
                ForeColor = Color.FromArgb(44, 62, 80), 
                Location = new Point(30,25),
                AutoSize = true
            };

            var lblDesc = new Label { 
                Text = desc, 
                Location = new Point(30,70),
                Size = new Size(390,80),
                ForeColor = Color.DimGray,
                Font = new Font("微软雅黑", 10)
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            
            return card;
        }
    }
}