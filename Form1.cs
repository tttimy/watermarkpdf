using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Font;
using iText.IO.Font;

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

        PdfReader reader = new PdfReader(inputPath);
        PdfWriter writer = new PdfWriter(outputPath, new iText.Kernel.Properties().SetPdfVersion(iText.Kernel.Properties.PdfVersion.PDF_2_0));
        PdfDocument pdfDoc = new PdfDocument(reader, writer);

        int numberOfPages = pdfDoc.GetNumberOfPages();
        
        iText.Kernel.Geom.Rectangle pageSize = pdfDoc.GetFirstPage().GetPageSize();
        float pageWidth = pageSize.GetWidth();
        float pageHeight = pageSize.GetHeight();

        float scaledFontSize = fontSize * relativeSize * (pageWidth / 1000f);

        for (int i = 1; i <= numberOfPages; i++)
        {
            iText.Kernel.Pdf.Page page = pdfDoc.GetPage(i);
            pageSize = page.GetPageSize();
            pageWidth = pageSize.GetWidth();
            pageHeight = pageSize.GetHeight();

            PdfCanvas canvas = new PdfCanvas(page);
            PdfFormXObject watermarkLayer = new PdfFormXObject(pageSize);
            iText.Kernel.Pdf.Canvas.PdfCanvas layerCanvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(watermarkLayer, pdfDoc);

            using MemoryStream ms = new MemoryStream();
            using var bmp = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bmp);
            int emSize = (int)scaledFontSize;
            using var font = new Font(fontFamily, emSize, FontStyle.Regular, GraphicsUnit.Pixel);
            SizeF textSize = g.MeasureString(text, font, 10000);
            
            float textWidth = (float)textSize.Width;
            float textHeight = (float)textSize.Height;
            
            if (relativeSize != 0)
            {
                float targetWidth = pageWidth * 0.3f * relativeSize;
                if (textWidth > targetWidth && targetWidth > 0)
                {
                    textWidth = targetWidth;
                    textHeight = textHeight * (targetWidth / (float)textSize.Width);
                }
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            int textAlpha = (int)(opacity / 100f * 255);
            float stepY = pageHeight / (lines + 1);
            float stepX = pageWidth / 2;

            layerCanvas.SaveState();
            layerCanvas.BeginText();
            layerCanvas.SetFontAndSize(iText.IO.Font.Constants.StandardFonts.HELVETICA, scaledFontSize * 0.7f);
            layerCanvas.SetFillColor(new DeviceCmyk(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));
            
            float angle = (float)(rotation * Math.PI / 180);
            layerCanvas.ConcatMatrix((float)Math.Cos(angle), (float)Math.Sin(angle), (float)-Math.Sin(angle), (float)Math.Cos(angle), 0, 0);

            for (int line = 0; line < lines; line++)
            {
                float yPos = stepY * (line + 1);
                float xPos = stepX;
                
                layerCanvas.ShowTextAligned(text, xPos, yPos, iText.Kernel.Geom.Constants.TextAlignment.CENTER, 
                    iText.Kernel.Geom.VerticalAlignment.BOTTOM, angle);
            }

            layerCanvas.EndText();
            layerCanvas.RestoreState();

            canvas.AddXObject(watermarkLayer);
            canvas.Release();

            this.Invoke((Action)(() =>
            {
                progressBar.Value = (int)((float)i / numberOfPages * 100);
                lblStatus.Text = $"正在处理... 第 {i}/{numberOfPages} 页";
                Application.DoEvents();
            }));
        }

        pdfDoc.Close();
    }
}