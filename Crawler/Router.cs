using System;
using System.Windows.Forms;
using Crawler.Views;

namespace Crawler.Router;

public class Router
{
    public static Router Instance { get; } = new Router();
    private MainWindow? _mainWindow;

    private Router() { }

    public void Register(MainWindow window)
    {
        _mainWindow = window;
    }

    public void GoTo(string pageName)
    {
        if (_mainWindow == null) return;

        UserControl? targetView = pageName.ToLower() switch
        {
            "home" => new Home(),
            "crawl" => new Crawl(),
            "resources" => new MyResources(),
            _ => null
        };

        if (targetView != null)
        {
            if (targetView is Home h) h.InitUi();
            else if (targetView is Crawl c) c.InitUi();
            else if (targetView is MyResources r) r.InitUi();

            _mainWindow.SwitchView(targetView);
        }
    }
}