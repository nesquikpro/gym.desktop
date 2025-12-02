using GymClient.Interfaces;
using GymClient.Models;
using OfficeOpenXml;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Windows;

public class ExportService : IExportService
{
    public void ExportToPdf(IEnumerable<VisitDto> visits)
    {
        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "GymVisitsPDF",
                Filter = "PDF File|*.pdf",
                Title = "Сохранить как PDF"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            string filePath = saveFileDialog.FileName;

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            double margin = 40;
            double y = margin;

            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            var rowFont = new XFont("Arial", 12, XFontStyle.Regular);

            // Заголовок
            gfx.DrawString("Список посещений", titleFont, XBrushes.Black, margin, y);
            y += 30;
            gfx.DrawLine(XPens.Black, margin, y, page.Width - margin, y);
            y += 20;

            double usableWidth = page.Width - 2 * margin;
            double col1Width = usableWidth * 0.5;
            double col2Width = usableWidth * 0.2;
            double col3Width = usableWidth * 0.3;

            double col1 = margin;
            double col2 = col1 + col1Width;
            double col3 = col2 + col2Width;
            double rowHeight = 20;

            gfx.DrawRectangle(XBrushes.LightGray, col1 - 2, y - 15, usableWidth, rowHeight);
            gfx.DrawString("ФИО", headerFont, XBrushes.Black, col1, y);
            gfx.DrawString("Чип", headerFont, XBrushes.Black, col2, y);
            gfx.DrawString("Дата и время", headerFont, XBrushes.Black, col3, y);
            y += rowHeight;

            bool alternate = false;

            foreach (var visit in visits)
            {
                var localTime = visit.VisitDateTime.Kind == DateTimeKind.Utc
                    ? visit.VisitDateTime.ToLocalTime()
                    : DateTime.SpecifyKind(visit.VisitDateTime, DateTimeKind.Utc).ToLocalTime();

                if (alternate)
                    gfx.DrawRectangle(XBrushes.LightGray, col1 - 2, y - 15, usableWidth, rowHeight);
                alternate = !alternate;

                gfx.DrawString(visit.MemberFullName, rowFont, XBrushes.Black, new XRect(col1, y - rowHeight + 5, col1Width, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(visit.ChipNumber, rowFont, XBrushes.Black, new XRect(col2, y - rowHeight + 5, col2Width, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(localTime.ToString("dd.MM.yyyy HH:mm"), rowFont, XBrushes.Black, new XRect(col3, y - rowHeight + 5, col3Width, rowHeight), XStringFormats.TopLeft);

                y += rowHeight;

                if (y > page.Height - margin)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }
            }

            gfx.DrawString($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm}", new XFont("Arial", 10), XBrushes.Gray, margin, page.Height - 30);

            document.Save(filePath);
        }
        catch (IOException ioEx)
        {
            System.Windows.MessageBox.Show($"Ошибка при сохранении PDF: {ioEx.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    public void ExportToExcel(IEnumerable<VisitDto> visits)
    {
        try
        {
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "GymVisitsExcel",
                Filter = "Excel File|*.xlsx",
                Title = "Сохранить как Excel"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            string filePath = saveFileDialog.FileName;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Посещения");

                worksheet.Cells[1, 1].Value = "ФИО";
                worksheet.Cells[1, 2].Value = "Чип";
                worksheet.Cells[1, 3].Value = "Дата и время";

                int row = 2;
                foreach (var visit in visits)
                {
                    var localTime = visit.VisitDateTime.Kind == DateTimeKind.Utc
                        ? visit.VisitDateTime.ToLocalTime()
                        : DateTime.SpecifyKind(visit.VisitDateTime, DateTimeKind.Utc).ToLocalTime();

                    worksheet.Cells[row, 1].Value = visit.MemberFullName;
                    worksheet.Cells[row, 2].Value = visit.ChipNumber;
                    worksheet.Cells[row, 3].Value = localTime.ToString("dd.MM.yyyy HH:mm");
                    row++;
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }
        catch (IOException ioEx)
        {
            System.Windows.MessageBox.Show($"Ошибка при сохранении Excel: {ioEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

}
