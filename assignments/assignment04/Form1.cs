using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace assignment04;

public partial class Form1 : Form
{
    private string filePath1 = "";
    private string filePath2 = "";

    public Form1()
    {
        InitializeComponent();
        this.Text = "assignment04 merge two files";
        this.Size = new Size(400, 300);
        Button fileOneBtn = new Button
        {
            Text = "Select File 1",
            Top = 20,
            Left = 20,
            Width = 100
        };
        Label fileOneLbl = new Label
        {
            Text = "click to select",
            Top = 25,
            Left = 130,
            Width = 200
        };
        fileOneBtn.Click += (s, e) =>
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件 (*.txt)|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filePath1 = ofd.FileName;
                fileOneLbl.Text = Path.GetFileName(filePath1);
            }
        };

        Button fileTwoBtn = new Button
        {
            Text = "Select File 2",
            Top = 60,
            Left = 20,
            Width = 100
        };
        Label fileTwoLbl = new Label
        {
            Text = "click to select",
            Top = 65,
            Left = 130,
            Width = 200
        };
        fileTwoBtn.Click += (s, e) =>
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件 (*.txt)|*.txt";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filePath2 = ofd.FileName;
                fileTwoLbl.Text = Path.GetFileName(filePath2);
            }
        };

        Button mergeBtn = new Button
        {
            Text = "Merge Files",
            Top = 110,
            Left = 20,
            Width = 100,
            BackColor = Color.LightBlue
        };
        mergeBtn.Click += (s, e) =>
        {
            if (string.IsNullOrEmpty(filePath1) || string.IsNullOrEmpty(filePath2))
            {
                MessageBox.Show("请先选择两个文件");
                return;
            }
            try
            {
                string content1 = File.ReadAllText(filePath1);
                string content2 = File.ReadAllText(filePath2);
                string dataDir = Path.Combine(Application.StartupPath, "Data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);             
                string desPath = Path.Combine(dataDir, "MergedResult.txt");                
                File.WriteAllText(desPath, content1 + Environment.NewLine + content2);
                MessageBox.Show($"合并成功！保存至：{desPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"合并出错: {ex.Message}");
            }
        };

        this.Controls.Add(fileOneBtn);
        this.Controls.Add(fileOneLbl);
        this.Controls.Add(fileTwoBtn);
        this.Controls.Add(fileTwoLbl);
        this.Controls.Add(mergeBtn);
    }
}