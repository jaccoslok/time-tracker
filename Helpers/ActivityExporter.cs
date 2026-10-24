using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using TimeTracker.ViewModels;

namespace TimeTracker.Helpers;

// Writes the activities shown in the main window to CSV or Excel
public static class ActivityExporter
{
    private static readonly string[] Headers =
        ["Date", "Start", "End", "Hours", "Project Code", "Customer", "Project", "Task",
         "Remarks", "Billable", "Rate", "Amount", "AI Time Saved"];

    public static void WriteCsv(Stream stream, IEnumerable<ActivityRow> rows)
    {
        // UTF-8 with a byte-order mark, so Excel shows € and accented letters correctly.
        // leaveOpen: the caller owns the stream and disposes it.
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
                                            bufferSize: -1, leaveOpen: true);
        writer.WriteLine(string.Join(",", Headers));

        // InvariantCulture: always "1.50", never "1,50", which would break the comma-separated columns
        var invariant = CultureInfo.InvariantCulture;
        foreach (var row in rows)
        {
            string[] fields =
            [
                row.StartedAtLocal.ToString("yyyy-MM-dd"),
                row.StartText,
                row.EndText,
                row.Hours.ToString("0.00", invariant),
                Quote(row.Project.Code),
                Quote(row.Project.Customer),
                Quote(row.Project.Description),
                Quote(row.TaskName),
                Quote(row.Remarks),
                row.IsBillable ? "Yes" : "No",
                row.HourlyRate.ToString("0.00", invariant),
                row.Amount.ToString("0.00", invariant),
                row.AiTimeSavedHours.ToString("0.0#", invariant)
            ];
            writer.WriteLine(string.Join(",", fields));
        }
    }

    // CSV text fields go between quotes; a quote inside the text is doubled
    private static string Quote(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

    public static void WriteExcel(Stream stream, IReadOnlyList<ActivityRow> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Activities");

        for (var column = 0; column < Headers.Length; column++)
            sheet.Cell(1, column + 1).Value = Headers[column];
        sheet.Row(1).Style.Font.Bold = true;

        var rowIndex = 2;
        foreach (var row in rows)
        {
            // Real dates and numbers (not text), so the sheet can sum and filter them
            sheet.Cell(rowIndex, 1).Value  = row.StartedAtLocal.Date;
            sheet.Cell(rowIndex, 2).Value  = row.StartText;
            sheet.Cell(rowIndex, 3).Value  = row.EndText;
            sheet.Cell(rowIndex, 4).Value  = row.Hours;
            sheet.Cell(rowIndex, 5).Value  = row.Project.Code;
            sheet.Cell(rowIndex, 6).Value  = row.Project.Customer;
            sheet.Cell(rowIndex, 7).Value  = row.Project.Description;
            sheet.Cell(rowIndex, 8).Value  = row.TaskName;
            sheet.Cell(rowIndex, 9).Value  = row.Remarks;
            sheet.Cell(rowIndex, 10).Value = row.IsBillable ? "Yes" : "No";
            sheet.Cell(rowIndex, 11).Value = row.HourlyRate;
            sheet.Cell(rowIndex, 12).Value = row.Amount;
            sheet.Cell(rowIndex, 13).Value = row.AiTimeSavedHours;
            rowIndex++;
        }

        // Totals line
        var lastDataRow = rowIndex - 1;
        sheet.Cell(rowIndex + 1, 1).Value = "Total";
        sheet.Cell(rowIndex + 1, 4).Value = rows.Sum(row => row.Hours);
        sheet.Cell(rowIndex + 1, 12).Value = rows.Sum(row => row.Amount);
        sheet.Cell(rowIndex + 1, 13).Value = rows.Sum(row => row.AiTimeSavedHours);
        sheet.Row(rowIndex + 1).Style.Font.Bold = true;

        sheet.Range(2, 1, lastDataRow + 2, 1).Style.DateFormat.Format = "yyyy-mm-dd";
        sheet.Column(4).Style.NumberFormat.Format = "0.00";
        sheet.Range(2, 11, lastDataRow + 2, 12).Style.NumberFormat.Format = "#,##0.00";
        sheet.Columns().AdjustToContents();

        workbook.SaveAs(stream);
    }
}
