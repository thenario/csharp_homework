namespace assignment08;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    // 定义控件变量（解决 CS0103 错误）
    private System.Windows.Forms.Label lblChinese;
    private System.Windows.Forms.TextBox txtEnglish;
    private System.Windows.Forms.Label lblResult;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblChinese = new System.Windows.Forms.Label();
        this.txtEnglish = new System.Windows.Forms.TextBox();
        this.lblResult = new System.Windows.Forms.Label();
        this.SuspendLayout();

        // lblChinese
        this.lblChinese.Location = new System.Drawing.Point(50, 40);
        this.lblChinese.Size = new System.Drawing.Size(300, 30);
        this.lblChinese.Font = new System.Drawing.Font("Microsoft YaHei", 12F);

        // txtEnglish
        this.txtEnglish.Location = new System.Drawing.Point(50, 80);
        this.txtEnglish.Size = new System.Drawing.Size(200, 30);

        // lblResult
        this.lblResult.Location = new System.Drawing.Point(50, 130);
        this.lblResult.Size = new System.Drawing.Size(300, 30);
        this.lblResult.ForeColor = System.Drawing.Color.Gray;

        // Form1
        this.ClientSize = new System.Drawing.Size(400, 250);
        this.Controls.Add(this.lblChinese);
        this.Controls.Add(this.txtEnglish);
        this.Controls.Add(this.lblResult);
        this.Text = "单词练习助手";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}