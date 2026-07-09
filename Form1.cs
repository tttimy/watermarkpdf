using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;

namespace PdfWatermark;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void BtnBrowseInput_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog() { Filter = "PDF files (*.pdf)|*.pdf" };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            txtInputPath.Text = ofd.FileName;
            if (string.IsNullOrEmpty(txtOutputPath.Text))
            {
                txtOutputPath.Text = System.IO. Path.Combine(System.IO.Path.GetDirectoryName(ofd.FileName) ?? "",
                    System.IO.Path.GetFileNameWithoutExtension(ofd.FileName) + "_watermarked.pdf");
            }
        }
    }

    private void BtnBrowseOutput_Click(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog() { Filter = "PDF files (*.pdf)|*.pdf" };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            txtOutputPath.Text = sfd.FileName;
        }
    }

    private void BtnColor_Click(object sender, EventArgs e)
    {
        using var cd = new ColorDialog() { Color = watermarkColor };
        if (cd.ShowDialog() == DialogResult.OK)
        {
            watermarkColor = cd.Color;
            panelColorPreview.BackColor = System.Drawing.Color.FromArgb((int)numOpacity.Value, cd.Color);
        }
    }

    private void UpdatePreview()
    {
        if (watermarkColor.A == (int)numOpacity.Value && panelColorPreview.BackColor.A == (int)numOpacity.Value)
        {
            return;
        }
        watermarkColor = System.Drawing.Color.FromArgb((int)numOpacity.Value, watermarkColor.R, watermarkColor.G, watermarkColor.B);
        panelColorPreview.BackColor = System.Drawing.Color.FromArgb((int)numOpacity.Value, watermarkColor.R, watermarkColor.G, watermarkColor.B);
    }

    private async void BtnAddWatermark_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtInputPath.Text) || string.IsNullOrWhiteSpace(txtOutputPath.Text))
        {
            MessageBox.Show("请选择输入和输出PDF文件路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!File.Exists(txtInputPath.Text))
        {
            MessageBox.Show("输入文件不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        btnAddWatermark.Enabled = false;
        progressBar.Value = 0;
        lblStatus.Text = "正在处理...";

        try
        {
            string text = txtWatermarkText.Text;
            float fontSize = (float)numFontSize.Value;
            float rotation = (float)numRotation.Value;
            int opacity = (int)numOpacity.Value;
            float relativeSize = (float)numRelativeSize.Value / 100f;
            int lines = (int)numLines.Value;
            string fontFamily = cmbFont.SelectedItem?.ToString() ?? "Arial";
            System.Drawing.Color color = watermarkColor;

            await Task.Run(() => AddWatermark(text, fontSize, rotation, opacity, relativeSize, lines, fontFamily, color));
            
            lblStatus.Text = "完成!";
            MessageBox.Show("水印添加成功!", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            lblStatus.Text = "错误: " + ex.Message;
            MessageBox.Show("处理失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnAddWatermark.Enabled = true;
            progressBar.Value = 0;
        }
    }
    private void AddWatermark(string text, float fontSize, float rotation, int opacity, float relativeSize, int lines, string fontFamily, System.Drawing.Color color)
    {
        string inputPath = txtInputPath.Text;
        string outputPath = txtOutputPath.Text;

        using var reader = new PdfReader(inputPath);
        using var writer = new PdfWriter(outputPath);
        using var pdfDoc = new PdfDocument(reader, writer);

        int numberOfPages = pdfDoc.GetNumberOfPages();

        // 加载字体
        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        for (int i = 1; i <= numberOfPages; i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageSize = page.GetPageSize();
            float pageWidth = pageSize.GetWidth();
            float pageHeight = pageSize.GetHeight();

            float scaledFontSize = fontSize * relativeSize * (pageWidth / 1000f);

            var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

            // 设置透明度
            var gs1 = new iText.Kernel.Pdf.Extgstate.PdfExtGState().SetFillOpacity(opacity / 100f);
            canvas.SaveState();
            canvas.SetExtGState(gs1);

            // 设置颜色
            canvas.SetFillColor(new DeviceRgb(color.R, color.G, color.B));

            // 计算水印位置
            float stepY = pageHeight / (lines + 1);
            float xPos = pageWidth / 2;

            for (int line = 0; line < lines; line++)
            {
                float yPos = stepY * (line + 1);

                // 旋转
                canvas.SaveState();
                canvas.ConcatMatrix((float)Math.Cos(rotation * Math.PI / 180), (float)Math.Sin(rotation * Math.PI / 180),
                                    (float)-Math.Sin(rotation * Math.PI / 180), (float)Math.Cos(rotation * Math.PI / 180),
                                    xPos, yPos);

                canvas.BeginText();
                canvas.SetFontAndSize(font, scaledFontSize);
                // 居中对齐
                float textWidth = font.GetWidth(text, scaledFontSize);
                canvas.MoveText(-textWidth / 2, 0);
                canvas.ShowText(text);
                canvas.EndText();

                canvas.RestoreState();
            }

            canvas.RestoreState();

            // 进度条
            this.Invoke((Action)(() =>
            {
                progressBar.Value = (int)((float)i / numberOfPages * 100);
                lblStatus.Text = $"正在处理... 第 {i}/{numberOfPages} 页";
                Application.DoEvents();
            }));
        }
    }


}