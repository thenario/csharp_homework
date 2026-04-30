using System.Drawing;
using System.Windows.Forms;
using Crawler.Router;
using AntdUI;
using Crawler.Services.DbService;

namespace Crawler.Views;

public class MainWindow : Form
{
    // 明确使用 AntdUI 的 Panel
    private AntdUI.Panel contentPanel;

    public MainWindow()
    {
        this.Text = "爬虫工具";
        this.Size = new Size(900, 650);
        this.StartPosition = FormStartPosition.CenterScreen;

        // 初始化数据库与表结构
        new FileDbService().InitialFileDbService();

        // 实例化 AntdUI.Panel
        contentPanel = new AntdUI.Panel { Dock = DockStyle.Fill };
        this.Controls.Add(contentPanel);

        Router.Router.Instance.Register(this);
        Router.Router.Instance.GoTo("home");
    }

    public void SwitchView(UserControl view)
    {
        contentPanel.Controls.Clear();
        view.Dock = DockStyle.Fill;
        contentPanel.Controls.Add(view);
    }
}
