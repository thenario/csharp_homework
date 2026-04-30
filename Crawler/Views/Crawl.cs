using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI;
using Crawler.Core;
using Crawler.Engines;
using Crawler.Entities;
using Crawler.Services.DbService;

namespace Crawler.Views
{
    public class Crawl : UserControl
    {
        private AntdUI.Input _txtUrl = null!;
        private DataGridView _tableResults = null!;
        private AntdUI.Progress _totalProgress = null!;
        private AntdUI.Label _lblStatus = null!;
        private AntdUI.Input _txtSavePath = null!;
        
        private CancellationTokenSource? _cts;
        private List<ScanResult> _currentData = new();

        public void InitUi()
        {
            this.Controls.Clear();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            // 1. 顶部操作面板
            var topPanel = new AntdUI.Panel { Dock = DockStyle.Top, Height = 60 };
            
            var btnBack = new AntdUI.Button { Text = "返回", Type = AntdUI.TTypeMini.Default, Location = new Point(0, 10), Size = new Size(80, 38) };
            btnBack.Click += (s, e) => Router.Router.Instance.GoTo("home");

            _txtUrl = new AntdUI.Input { PlaceholderText = "输入目标 URL...", Location = new Point(90, 10), Size = new Size(400, 38), Radius = 6 };
            
            var btnScan = new AntdUI.Button { Text = "扫描资源", Type = AntdUI.TTypeMini.Primary, Location = new Point(500, 10), Size = new Size(100, 38) };
            btnScan.Click += async (s, e) => await HandleScan();

            topPanel.Controls.Add(btnBack);
            topPanel.Controls.Add(_txtUrl);
            topPanel.Controls.Add(btnScan);

            // 2. 底部控制面板
            var bottomPanel = new AntdUI.Panel { Dock = DockStyle.Bottom, Height = 130, Padding = new Padding(0, 10, 0, 0) };
            
            var lblPath = new AntdUI.Label { Text = "保存路径:", Location = new Point(0, 18), AutoSize = true };
            
            _txtSavePath = new AntdUI.Input { Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads"), Location = new Point(70, 10), Size = new Size(300, 38), Radius = 6 };
            
            var btnBrowse = new AntdUI.Button { Text = "选择路径", Type = AntdUI.TTypeMini.Default, Location = new Point(380, 10), Size = new Size(80, 38) };
            btnBrowse.Click += (s, e) => {
                using var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "选择保存路径", SelectedPath = _txtSavePath.Text };
                if (dialog.ShowDialog() == DialogResult.OK) {
                    _txtSavePath.Text = dialog.SelectedPath;
                }
            };

            var btnRename = new AntdUI.Button { Text = "修改选中名称", Type = AntdUI.TTypeMini.Default, Location = new Point(470, 10), Size = new Size(110, 38) };
            btnRename.Click += HandleRename;

            var btnDownload = new AntdUI.Button { Text = "开始下载", Type = AntdUI.TTypeMini.Success, Location = new Point(0, 70), Size = new Size(100, 40) };
            var btnCancel = new AntdUI.Button { Text = "取消下载", Type = AntdUI.TTypeMini.Error, Location = new Point(110, 70), Size = new Size(80, 40), Ghost = true };
            
            // 修复：补全 Size 属性，赋予固定宽度和高度，否则高度默认 0 无法显示
            _totalProgress = new AntdUI.Progress { Location = new Point(200, 78), Size = new Size(400, 24) };
            
            _lblStatus = new AntdUI.Label { Text = "等待操作...", Location = new Point(620, 78), AutoSize = true, ForeColor = Color.Gray };

            btnDownload.Click += async (s, e) => await HandleDownload();
            btnCancel.Click += (s, e) => {
                if (_cts != null && !_cts.IsCancellationRequested) {
                    _cts.Cancel();
                    _lblStatus.Text = "正在取消下载任务...";
                }
            };

            bottomPanel.Controls.Add(lblPath);
            bottomPanel.Controls.Add(_txtSavePath);
            bottomPanel.Controls.Add(btnBrowse);
            bottomPanel.Controls.Add(btnRename);
            bottomPanel.Controls.Add(btnDownload);
            bottomPanel.Controls.Add(btnCancel);
            bottomPanel.Controls.Add(_totalProgress);
            bottomPanel.Controls.Add(_lblStatus);

            // 3. 中间表格
            _tableResults = new DataGridView {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White
            };

            this.Controls.Add(_tableResults);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private void HandleRename(object? sender, EventArgs e)
        {
            if (_tableResults.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先勾选需要修改名称的资源！");
                return;
            }
            if (_tableResults.SelectedRows.Count > 1)
            {
                MessageBox.Show("一次只能修改一个资源的名称！");
                return;
            }

            var rowIndex = _tableResults.SelectedRows[0].Index;
            var data = _currentData[rowIndex];

            using var form = new Form { 
                Width = 350, Height = 160, 
                Text = "修改资源名称", 
                StartPosition = FormStartPosition.CenterParent, 
                FormBorderStyle = FormBorderStyle.FixedDialog, 
                MaximizeBox = false, MinimizeBox = false 
            };
            
            var txt = new System.Windows.Forms.TextBox { Left = 20, Top = 20, Width = 290, Height = 40, Text = data.Title };
            var btnOk = new System.Windows.Forms.Button { Text = "确定", Left = 140, Top = 70, Width = 80, Height = 35 };
            var btnCancel = new System.Windows.Forms.Button { Text = "取消", Left = 230, Top = 70, Width = 80, Height = 35 };
            
            btnOk.Click += (s, args) => { form.DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, args) => { form.DialogResult = DialogResult.Cancel; };

            form.Controls.Add(txt);
            form.Controls.Add(btnOk);
            form.Controls.Add(btnCancel);
            
            if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
            {
                data.Title = txt.Text.Trim();
                _tableResults.DataSource = null; 
                _tableResults.DataSource = _currentData;
            }
        }

        private async Task HandleScan()
        {
            string url = _txtUrl.Text;
            if (string.IsNullOrEmpty(url)) 
            {
                MessageBox.Show("请输入需要扫描的 URL！");
                return;
            }

            try
            {
                _lblStatus.Text = "正在扫描页面...";
                await Task.Delay(1000); 

                _currentData = new List<ScanResult>
                {
                    new ScanResult { Title = "解析到的主视频", Type = "Video", Url = url, Extension = ".mp4" },
                    new ScanResult { Title = "网页配图集合", Type = "Image", Url = url, Extension = "folder" },
                    new ScanResult { Title = "网页正文内容", Type = "Text", Url = url, Extension = ".md" }
                };

                _tableResults.DataSource = _currentData;
                _lblStatus.Text = $"扫描完成，发现 {_currentData.Count} 个可用资源。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"扫描过程发生错误: {ex.Message}");
                _lblStatus.Text = "扫描失败";
            }
        }

        private async Task HandleDownload()
        {
            if (_tableResults.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先在列表中勾选需要下载的资源！");
                return;
            }

            string saveDir = _txtSavePath.Text;
            if (string.IsNullOrWhiteSpace(saveDir))
            {
                MessageBox.Show("请指定文件保存路径！");
                return;
            }

            try 
            {
                if (!Directory.Exists(saveDir)) 
                {
                    Directory.CreateDirectory(saveDir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法创建保存目录: {ex.Message}");
                return;
            }

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _cts = new CancellationTokenSource(); 
            
            // 修复进度条赋值：AntdUI 的 Progress.Value 接收 0.0f - 1.0f 的浮点数
            var progressHandler = new Progress<ProgressInfo>(info => {
                _totalProgress.Value = info.Percent / 100f;
                _lblStatus.Text = info.Message;
            });

            try
            {
                int total = _tableResults.SelectedRows.Count;
                int current = 0;

                foreach (DataGridViewRow row in _tableResults.SelectedRows)
                {
                    var data = _currentData[row.Index];
                    current++;
                    _lblStatus.Text = $"准备下载 ({current}/{total}): {data.Title}";
                    _totalProgress.Value = 0f;

                    try
                    {
                        ICrawlEngine engine = data.Type switch {
                            "Video" => new MediaEngine(),
                            "Image" => new ImageEngine(),
                            "Text" => new TextEngine(),
                            _ => throw new Exception("无匹配的爬虫引擎")
                        };

                        string fullPath = data.Type == "Image" ? Path.Combine(saveDir, data.Title) : Path.Combine(saveDir, data.Title + data.Extension);

                        if (data.Type == "Image" && !Directory.Exists(fullPath))
                        {
                            Directory.CreateDirectory(fullPath);
                        }

                        await engine.StartAsync(data.Url, fullPath, progressHandler, _cts.Token);

                        try 
                        {
                            var dbService = new FileDbService();
                            dbService.SaveFile(new FileEntity {
                                Title = data.Title,
                                Url = data.Url,
                                LocalPath = fullPath,
                                Type = data.Type,
                                OriginalName = data.Title + data.Extension,
                                FileSize = "未知",
                                DownloadTime = DateTime.Now
                            });
                        } 
                        catch { }
                    }
                    catch (OperationCanceledException)
                    {
                        throw; 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"[{data.Title}] 下载出错: {ex.Message}");
                    }
                }
                
                _totalProgress.Value = 1f; // 100%
                MessageBox.Show("所有选中的任务均已处理完成！");
                _lblStatus.Text = "下载完毕";
            }
            catch (OperationCanceledException)
            {
                _totalProgress.Value = 0f;
                _lblStatus.Text = "下载已被用户强行取消！";
                MessageBox.Show("任务已取消");
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "下载发生严重错误";
                MessageBox.Show($"发生错误: {ex.Message}");
            }
        }
    }
}