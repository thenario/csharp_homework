namespace assignment07;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.txtKeyword = new System.Windows.Forms.TextBox();
        this.btnSearch = new System.Windows.Forms.Button();
        this.txtBaiduRes = new System.Windows.Forms.TextBox();
        this.txtBingRes = new System.Windows.Forms.TextBox();
        this.SuspendLayout();
 
        this.txtKeyword.Location = new System.Drawing.Point(20, 20);
        this.txtKeyword.Size = new System.Drawing.Size(500, 25);
        this.txtKeyword.Name = "txtKeyword";

        this.btnSearch.Location = new System.Drawing.Point(530, 18);
        this.btnSearch.Size = new System.Drawing.Size(100, 30);
        this.btnSearch.Text = "搜索";
        this.btnSearch.Name = "btnSearch";
        
        this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);


        this.txtBaiduRes.Location = new System.Drawing.Point(20, 70);
        this.txtBaiduRes.Multiline = true;
        this.txtBaiduRes.Size = new System.Drawing.Size(350, 400);
        this.txtBaiduRes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtBaiduRes.Name = "txtBaiduRes";


        this.txtBingRes.Location = new System.Drawing.Point(390, 70);
        this.txtBingRes.Multiline = true;
        this.txtBingRes.Size = new System.Drawing.Size(350, 400);
        this.txtBingRes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtBingRes.Name = "txtBingRes";

        this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(780, 500);

        this.Controls.Add(this.txtBingRes);
        this.Controls.Add(this.txtBaiduRes);
        this.Controls.Add(this.btnSearch);
        this.Controls.Add(this.txtKeyword);
        this.Name = "Form1";
        this.Text = "双搜索引擎搜索工具";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TextBox txtKeyword;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.TextBox txtBaiduRes;
    private System.Windows.Forms.TextBox txtBingRes;
}


