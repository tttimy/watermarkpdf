using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Extgstate;

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

    /// <summary>
    /// PDF批量添加斜向铺满水印（iText7）
    /// </summary>
    /// <param name="text">水印文字</param>
    /// <param name="fontSize">基础字号</param>
    /// <param name="rotation">旋转角度(°)</param>
    /// <param name="opacity">透明度 0~100</param>
    /// <param name="relativeSize">页面相对缩放系数</param>
    /// <param name="lines">纵向重复行数</param>
    /// <param name="fontFamily">字体名称，如微软雅黑 Microsoft YaHei</param>
    /// <param name="color">水印颜色</param>
    private async void AddWatermark(string text, float fontSize, float rotation, int opacity, float relativeSize, int lines, string fontFamily, System.Drawing.Color color)
    {
        // 1. 基础参数校验
        string inputPath = txtInputPath.Text.Trim();
        string outputPath = txtOutputPath.Text.Trim();
        if (!File.Exists(inputPath))
        {
            MessageBox.Show("源PDF文件不存在！");
            return;
        }
        if (string.IsNullOrEmpty(outputPath))
        {
            MessageBox.Show("请选择输出保存路径！");
            return;
        }
        if (opacity < 0) opacity = 0;
        if (opacity > 100) opacity = 100;

        // UI前置锁定
        progressBar.Value = 0;

        try
        {
            // 后台执行PDF操作，不阻塞UI
            await Task.Run(() =>
            {
                // 加载字体：优先自定义字体，失败回退默认黑体
                PdfFont font;
                try
                {
                    font = PdfFontFactory.CreateFont(fontFamily, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                }
                catch
                {
                    font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    // 回退到程序自带的 simsun.ttc
                    //string fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "simsun.ttc");
                    //if (File.Exists(fallbackPath))
                    //    font = PdfFontFactory.CreateFont(fallbackPath, 0, PdfEncodings.IDENTITY_H);
                    //else
                    //    // 最后尝试系统路径
                    //    font = PdfFontFactory.CreateFont(@"C:\Windows\Fonts\simsun.ttc", 0, PdfEncodings.IDENTITY_H);
                }

                using var reader = new PdfReader(inputPath);
                using var writer = new PdfWriter(outputPath);
                using var pdfDoc = new PdfDocument(reader, writer);

                int pageCount = pdfDoc.GetNumberOfPages();
                double rad = rotation * Math.PI / 180;
                float alpha = opacity / 100f;

                for (int pageIdx = 1; pageIdx <= pageCount; pageIdx++)
                {
                    var page = pdfDoc.GetPage(pageIdx);
                    var pageRect = page.GetPageSize();
                    float pw = pageRect.GetWidth();
                    float ph = pageRect.GetHeight();

                    // 自适应字号，宽高共同参与缩放
                    float baseScale = Math.Min(pw, ph) / 1000f;

                    if (relativeSize <= 0) relativeSize = 1.0f;
                    float realFontSize = fontSize * relativeSize * (Math.Min(pw, ph) / 1000f);
                    if (realFontSize < 6) realFontSize = 12; // 可读下限
                    // 新建后置画布（水印在底层内容下方，不遮挡文字）
                    PdfCanvas canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);
                    // 透明度扩展图形状态
                    PdfExtGState transparentGs = new PdfExtGState()
                        .SetFillOpacity(alpha)
                        .SetStrokeOpacity(alpha);

                    canvas.SaveState();
                    canvas.SetExtGState(transparentGs);
                    canvas.SetFillColor(new DeviceRgb(color.R / 255f, color.G / 255f, color.B / 255f));

                    float stepY = ph / (lines + 1);
                    float centerX = pw / 2;

                    for (int line = 0; line < lines; line++)
                    {
                        float centerY = stepY * (line + 1);

                        canvas.SaveState();
                        // 旋转矩阵
                        canvas.ConcatMatrix(
                            (float)Math.Cos(rad), (float)Math.Sin(rad),
                            -(float)Math.Sin(rad), (float)Math.Cos(rad),
                            centerX, centerY
                        );

                        canvas.BeginText();
                        canvas.SetFontAndSize(font, realFontSize);
                        float textW = font.GetWidth(text, realFontSize);
                        canvas.MoveText(-textW / 2, 0); // 文字居中
                        canvas.ShowText(text);
                        canvas.EndText();

                        canvas.RestoreState();
                    }

                    canvas.RestoreState();
                    canvas.Release();

                    // 更新UI进度（主线程同步）
                    int currentPage = pageIdx;
                    UpdateProgress(pageIdx, pageCount, $"处理中 {pageIdx}/{pageCount} 页");
                }

                pdfDoc.Close();
                writer.Close();
                reader.Close();
            });

            MessageBox.Show($"处理完毕，文件已保存至：\n{outputPath}");
        }
        catch (IOException ex)
        {
            MessageBox.Show($"文件读写异常：{ex.Message}\n请关闭占用PDF的程序后重试");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"处理失败：{ex.Message}");
        }
        finally
        {
        }
    }


}