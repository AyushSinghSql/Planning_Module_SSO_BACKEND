using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using WebApi.DTO;

public class ForecastPdfDocument : IDocument
{
    private readonly PlanForecastSummary _model;

    public ForecastPdfDocument(PlanForecastSummary model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Header().Text("Revenue Forecast Report").Bold().FontSize(18).AlignCenter();
            page.Content().Column(col =>
            {
                foreach (var emp in _model.EmployeeForecastSummary)
                {
                    col.Item().PaddingBottom(15).Element(c => CreateEmployeeSection(emp));
                }

                if (_model.DirectCOstForecastSummary?.Any() == true)
                {
                    col.Item().PageBreak();
                    col.Item().Text("Direct Cost Forecast Summary").Bold().FontSize(14);
                    foreach (var emp in _model.DirectCOstForecastSummary)
                    {
                        col.Item().PaddingBottom(10).Element(c => CreateEmployeeSection(emp));
                    }
                }

                col.Item().PageBreak();
                col.Item().Text("Summary Totals").Bold().FontSize(14);
                col.Item().Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });

                    void Row(string label, decimal value)
                    {
                        t.Cell().Text(label);
                        t.Cell().Text($"{value:N2}");
                    }

                    Row("Total Cost", _model.TotalCost);
                    Row("Total GNA", _model.TotalGna);
                    Row("Total Overhead", _model.TotalOverhead);
                    Row("Total Fringe", _model.TotalFringe);
                    Row("Total Burden", _model.TotalBurden);
                    Row("Total Burden Cost", _model.TotalBurdenCost);
                    //Row("TNM Revenue", _model.TNMRevenue);
                    //Row("CPFF Revenue", _model.CPFFRevenue);
                });
            });
        });
    }

    private Action<IContainer> CreateEmployeeSection(EmployeeForecastSummary emp)
    {
        return container => container
            .Border(1)
            .Padding(10)
            .Column(col =>
            {
                col.Item().Text($"Name: {emp.Name} | ID: {emp.EmplId}").Bold();
                col.Item().Text($"Org: {emp.OrgID} | Account: {emp.AccID} | PLC: {emp.PlcCode}");

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50); // Month
                        columns.ConstantColumn(50); // Year
                        columns.RelativeColumn();   // Hours
                        columns.RelativeColumn();   // Cost
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Month").Bold();
                        header.Cell().Text("Year").Bold();
                        header.Cell().Text("Hours").Bold();
                        header.Cell().Text("Cost").Bold();
                    });

                    //foreach (var entry in emp.emplSchedule?.payrollSalary ?? new List<PayrollSalary>())
                    //{
                    //    var month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(entry.month);
                    //    table.Cell().Text(month);
                    //    table.Cell().Text(entry.year.ToString());
                    //    table.Cell().Text(entry.hours.ToString("N0"));
                    //    table.Cell().Text($"${entry.cost:N2}");
                    //}
                });
            });
    }

}
