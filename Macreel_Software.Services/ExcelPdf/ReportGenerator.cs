using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Macreel_Software.Services.ExcelPdf
{
    public class ReportGenerator
    {
      
        public async Task<byte[]> ExportToPdfAsync<T>( List<T> data,string[] headers, string title, Func<T, List<string>> mapFunc)
        {
            if (data == null || data.Count == 0)
                throw new Exception("No data available to generate PDF.");

            if (headers == null || headers.Length == 0)
                throw new Exception("Headers must be provided.");

            if (mapFunc == null)
                throw new Exception("Mapping function must be provided.");

           
            Func<IContainer, IContainer> CellStyle = container =>
                container.Padding(5)
                         .BorderBottom(1)
                         .BorderColor(Colors.Grey.Lighten3);

          
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Element(headerContainer =>
                        {
                            headerContainer
                                .Padding(10)
                                .Text(title)
                                .AlignCenter()
                                .SemiBold()
                                .FontSize(24)
                                .FontColor(Colors.Blue.Medium);
                        });

                   
                    page.Content().Border(1).Table(table =>
                    {
                       
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < headers.Length; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        columns.ConstantColumn(50);
                                        break;
                                    case 1:
                                        columns.ConstantColumn(150);
                                        break;
                                    default:
                                        columns.RelativeColumn(1);
                                        break;
                                }
                            }
                        });

                     
                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell().Border(1).Padding(5)
                                    .Element(container => container
                                        .Background(Colors.Grey.Lighten3)
                                        .Text(h)
                                        .FontSize(12)
                                        .Bold()
                                        .AlignCenter()
                                    );
                            }
                        });

                        
                        foreach (var item in data)
                        {
                            var cells = mapFunc(item);

                            if (cells == null || cells.Count != headers.Length)
                                throw new Exception("Mapping function returned invalid number of cells.");

                            for (int i = 0; i < headers.Length; i++)
                            {
                                table.Cell().Element(CellStyle).Text(cells[i] ?? "");
                            }
                        }
                    });
                });
            });

       
            try
            {
                using var stream = new MemoryStream();
                await Task.Run(() => document.GeneratePdf(stream));
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("PDF generation failed: " + ex.Message, ex);
            }
        }


        public async Task<byte[]> ExportToExcelAsync<T>(List<T> data,string[] headers,string title,Func<T, List<string>> mapFunc)
        {
            if (data == null || data.Count == 0)
                throw new Exception("No data available to generate Excel.");

            if (headers == null || headers.Length == 0)
                throw new Exception("Headers must be provided.");

            if (mapFunc == null)
                throw new Exception("Mapping function must be provided.");

            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add("Report");

                int currentRow = 1;

                
                if (!string.IsNullOrEmpty(title))
                {
                    ws.Cells[currentRow, 1].Value = title;
                    ws.Cells[currentRow, 1, currentRow, headers.Length].Merge = true;
                    ws.Cells[currentRow, 1].Style.Font.Bold = true;
                    ws.Cells[currentRow, 1].Style.Font.Size = 18;
                    ws.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    currentRow += 2;
                }

           
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[currentRow, i + 1].Value = headers[i];
                    ws.Cells[currentRow, i + 1].Style.Font.Bold = true;
                    ws.Cells[currentRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[currentRow, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    ws.Cells[currentRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[currentRow, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[currentRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                }

                currentRow++;

                foreach (var item in data)
                {
                    var cells = mapFunc(item);

                    if (cells == null || cells.Count != headers.Length)
                        throw new Exception("Mapping function returned invalid number of cells.");

                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cells[currentRow, i + 1].Value = cells[i] ?? "";
                        ws.Cells[currentRow, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        ws.Cells[currentRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[currentRow, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    currentRow++;
                }

               
                ws.Cells[ws.Dimension.Address].AutoFitColumns();

           
                using var stream = new MemoryStream();
                package.SaveAs(stream);
                return stream.ToArray();
            });
        }

    }
}
