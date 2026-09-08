using ClosedXML.Excel;

namespace SoporteEgehid;

// Genera el reporte .xlsx con el mismo formato que la versión openpyxl.
public static class ExcelReporte
{
    private static readonly string[] Headers =
    {
        "ID Ticket", "Técnico", "Técnico Compañero", "Ubicación", "Departamento",
        "Detalle Soporte", "Equipo", "Colaborador(a)", "Nivel", "Estado",
        "Fecha", "Hora Inicio", "Hora Final", "Tiempo Total (min)", "Extensión",
    };

    private static readonly double[] ColumnWidths =
    {
        14, 25, 25, 28, 25, 35, 15, 22, 12, 18, 14, 14, 14, 22, 12,
    };

    public static byte[] Generar(IReadOnlyList<Ticket> filas)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Reportes");

        // Encabezados
        for (var i = 0; i < Headers.Length; i++)
            ws.Cell(1, i + 1).Value = Headers[i];

        // Filas de datos (mismo orden que Db.SelectAll)
        var row = 2;
        foreach (var t in filas)
        {
            ws.Cell(row, 1).Value = t.IdTicket;
            ws.Cell(row, 2).Value = t.Tecnico;
            ws.Cell(row, 3).Value = t.TecnicoCompanero;
            ws.Cell(row, 4).Value = t.Ubicacion;
            ws.Cell(row, 5).Value = t.Departamento;
            ws.Cell(row, 6).Value = t.DetalleSoporte;
            ws.Cell(row, 7).Value = t.Equipo;
            ws.Cell(row, 8).Value = t.Colaborador;
            ws.Cell(row, 9).Value = t.Nivel;
            ws.Cell(row, 10).Value = t.Estado;
            ws.Cell(row, 11).Value = t.Fecha;
            ws.Cell(row, 12).Value = t.HoraInicio;
            ws.Cell(row, 13).Value = t.HoraFinal;
            ws.Cell(row, 14).Value = t.TiempoTotal;
            ws.Cell(row, 15).Value = t.Extension;
            row++;
        }

        // Estilo del encabezado (azul EGEHID, texto blanco en negrita, centrado, bordes)
        var headerRange = ws.Range(1, 1, 1, Headers.Length);
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0056D2");
        headerRange.Style.Font.FontName = "Segoe UI";
        headerRange.Style.Font.FontSize = 11;
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D9D9D9");
        headerRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#D9D9D9");

        // Anchos de columna
        for (var i = 0; i < ColumnWidths.Length; i++)
            ws.Column(i + 1).Width = ColumnWidths[i];

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
