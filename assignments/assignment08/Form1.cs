namespace assignment08;

public partial class Form1 : Form
{
    private readonly List<WordModel> _words;
    private int _currentIndex = 0;

    public Form1()
    {
        InitializeComponent();
        DatabaseHelper.Initialize();
        _words = DatabaseHelper.GetAll();
        txtEnglish.KeyDown += OnEnglishInputKeyDown;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_currentIndex < _words.Count)
        {
            var current = _words[_currentIndex];
            lblChinese.Text = $"请翻译：{current.Chinese}";
            txtEnglish.Clear();
            lblResult.Text = $"进度: {_currentIndex + 1}/{_words.Count}";
            // 确保每次刷新时颜色重置，否则一直是上次的红/绿
            lblResult.ForeColor = Color.Gray; 
        }
        else
        {
            lblChinese.Text = "🎉 全部练习完成！";
            txtEnglish.Enabled = false;
        }
    }

    private void OnEnglishInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true; 

            if (_currentIndex >= _words.Count) return;

            string input = txtEnglish.Text.Trim();
            string answer = _words[_currentIndex].English;

            if (input.Equals(answer, StringComparison.OrdinalIgnoreCase))
            {
                lblResult.Text = "正确！正在切换...";
                lblResult.ForeColor = Color.Green;
                
                // 增加一个小延时或者直接切（这里为了演示流畅直接切）
                _currentIndex++;
                RefreshUI();
            }
            else
            {
                lblResult.Text = $"错误！'{input}' 不正确";
                lblResult.ForeColor = Color.Red;
                txtEnglish.SelectAll();
            }
        }
    }
}