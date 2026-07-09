using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfReader = iTextSharp.text.pdf.PdfReader;
using Rectangle = iTextSharp.text.Rectangle;

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

            //await Task.Run(() => 
            AddTextWatermark(txtInputPath.Text, txtOutputPath.Text, text, fontSize,  opacity, rotation
                //,relativeSize , lines, fontFamily, color
                //)
                );
            
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

    /// <summary>主线程更新进度与状态文本</summary>
    private void UpdateProgress(int current, int total, string statusText)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => UpdateProgress(current, total, statusText)));
            return;
        }
        progressBar.Maximum = 100;
        progressBar.Value = (int)(current / (float)total * 100);
        lblStatus.Text = statusText;
    }

    public static void AddTextWatermark(string inputPath, string outputPath,
        string watermarkText, float fontSize = 50, float opacity = 0.3f,
        float rotation = 45, string fontPath = null)
    {
        using (PdfReader reader = new PdfReader(inputPath))
        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
        using (PdfStamper stamper = new PdfStamper(reader, fs))
        {
            BaseFont baseFont = fontPath != null ?
                BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED) :
                BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);

            PdfGState gstate = new PdfGState { FillOpacity = opacity };

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                Rectangle pageSize = reader.GetPageSize(i);
                PdfContentByte canvas = stamper.GetUnderContent(i);

                canvas.SetGState(gstate);
                canvas.BeginText();
                canvas.SetFontAndSize(baseFont, fontSize);
                canvas.SetColorFill(BaseColor.LIGHT_GRAY);

                // 平铺水印
                for (float x = 100; x < pageSize.Width; x += 300)
                {
                    for (float y = 100; y < pageSize.Height; y += 200)
                    {
                        canvas.ShowTextAligned(Element.ALIGN_CENTER,
                            watermarkText, x, y, rotation);
                    }
                }
                canvas.EndText();
            }
        }
    }


}