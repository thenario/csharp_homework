using System.Drawing;
using System.Windows.Forms;
using Crawler.Router;
using AntdUI;

namespace Crawler.Views;

public class MainWindow : Form
{
    private Panel contentPanel;

    public MainWindow()
    {
        this.Text = "爬虫工具";
        this.Size = new Size(900, 650);
        this.StartPosition = FormStartPosition.CenterScreen;

        contentPanel = new Panel { Dock = DockStyle.Fill };
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