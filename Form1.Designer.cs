namespace PdfWatermark;

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

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        Text = "PDF加水印工具";

        // 水印文字
        var lblText = new Label() { Text = "水印文字:", Location = new Point(20, 20), Width = 80 };
        txtWatermarkText = new TextBox() { Location = new Point(120, 20), Width = 400 };

        // 字号
        var lblFontSize = new Label() { Text = "字号:", Location = new Point(20, 60), Width = 80 };
        numFontSize = new NumericUpDown() { Location = new Point(120, 60), Width = 100, Minimum = 8, Maximum = 200, Value = 36 };

        // 旋转角度
        var lblRotation = new Label() { Text = "旋转角度:", Location = new Point(20, 100), Width = 80 };
        numRotation = new NumericUpDown() { Location = new Point(120, 100), Width = 100, Minimum = -90, Maximum = 90, Value = 45 };

        // 不透明度
        var lblOpacity = new Label() { Text = "不透明度(0-100):", Location = new Point(20, 140), Width = 100 };
        numOpacity = new NumericUpDown() { Location = new Point(120, 140), Width = 100, Minimum = 1, Maximum = 100, Value = 30 };

        // 相对尺寸
        var lblRelativeSize = new Label() { Text = "相对尺寸(%):", Location = new Point(20, 180), Width = 100 };
        numRelativeSize = new NumericUpDown() { Location = new Point(120, 180), Width = 100, Minimum = 10, Maximum = 500, Value = 100 };

        // 行数
        var lblLines = new Label() { Text = "每页行数:", Location = new Point(20, 220), Width = 100 };
        numLines = new NumericUpDown() { Location = new Point(120, 220), Width = 100, Minimum = 1, Maximum = 10, Value = 2 };

        // 输入PDF
        var lblInput = new Label() { Text = "输入PDF:", Location = new Point(20, 270), Width = 80 };
        txtInputPath = new TextBox() { Location = new Point(120, 270), Width = 400, ReadOnly = true };
        btnBrowseInput = new Button() { Text = "浏览...", Location = new Point(530, 270), Width = 80 };

        // 输出PDF
        var lblOutput = new Label() { Text = "输出PDF:", Location = new Point(20, 310), Width = 80 };
        txtOutputPath = new TextBox() { Location = new Point(120, 310), Width = 400, ReadOnly = true };
        btnBrowseOutput = new Button() { Text = "浏览...", Location = new Point(530, 310), Width = 80 };

        // 开始加水印按钮
        btnAddWatermark = new Button() { Text = "开始加水印", Location = new Point(120, 360), Width = 120, Height = 35 };

        // 进度条
        progressBar = new ProgressBar() { Location = new Point(120, 410), Width = 400 };

        // 日志
        lblStatus = new Label() { Text = "就绪", Location = new Point(20, 480), Width = 500 };

        // 颜色选择
        var lblColor = new Label() { Text = "颜色:", Location = new Point(20, 450), Width = 80 };
        btnColor = new Button() { Text = "选择颜色", Location = new Point(120, 450), Width = 100 };
        panelColorPreview = new Panel() { Location = new Point(230, 450), Size = new Size(30, 30), BackColor = Color.FromArgb(128, Color.Black), BorderStyle = BorderStyle.FixedSingle };
        watermarkColor = Color.FromArgb(128, Color.Black);

        // 字体选择
        var lblFont = new Label() { Text = "字体:", Location = new Point(20, 490), Width = 80 };
        cmbFont = new ComboBox() { Location = new Point(120, 490), Width = 200 };
        var fonts = System.Drawing.FontFamily.Families.Select(f => f.Name).ToArray();
        cmbFont.Items.AddRange(fonts);
        cmbFont.SelectedItem = "Arial";

        // 事件
        btnBrowseInput.Click += BtnBrowseInput_Click;
        btnBrowseOutput.Click += BtnBrowseOutput_Click;
        btnAddWatermark.Click += BtnAddWatermark_Click;
        numFontSize.ValueChanged += (s, e) => { UpdatePreview(); };
        numRotation.ValueChanged += (s, e) => { UpdatePreview(); };
        numOpacity.ValueChanged += (s, e) => { UpdatePreview(); };
        numRelativeSize.ValueChanged += (s, e) => { UpdatePreview(); };
        numLines.ValueChanged += (s, e) => { UpdatePreview(); };
        txtWatermarkText.TextChanged += (s, e) => { UpdatePreview(); };
        btnColor.Click += BtnColor_Click;

        // 添加到表单
        Controls.AddRange(new Control[] {
            lblText, txtWatermarkText,
            lblFontSize, numFontSize,
            lblRotation, numRotation,
            lblOpacity, numOpacity,
            lblRelativeSize, numRelativeSize,
            lblLines, numLines,
            lblInput, txtInputPath, btnBrowseInput,
            lblOutput, txtOutputPath, btnBrowseOutput,
            btnAddWatermark, progressBar,
            lblStatus, lblColor, btnColor, panelColorPreview,
            lblFont, cmbFont
        });
    }

    #endregion

    private TextBox txtWatermarkText;
    private NumericUpDown numFontSize;
    private NumericUpDown numRotation;
    private NumericUpDown numOpacity;
    private NumericUpDown numRelativeSize;
    private NumericUpDown numLines;
    private TextBox txtInputPath;
    private Button btnBrowseInput;
    private TextBox txtOutputPath;
    private Button btnBrowseOutput;
    private Button btnAddWatermark;
    private ProgressBar progressBar;
    private Label lblStatus;
    private Button btnColor;
    private Panel panelColorPreview;
    private Color watermarkColor;
    private ComboBox cmbFont;
}