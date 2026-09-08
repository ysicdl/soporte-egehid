using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;

namespace SoporteEgehid;

// -------------------------------------------------------------
// DTOs de entrada. Con la política snake_case del serializador,
// TecnicoCompanero <-> tecnico_companero, DetalleSoporte <-> detalle_soporte, etc.
// -------------------------------------------------------------
public record TicketRequest
{
    public string Tecnico { get; init; } = "";
    public string Ubicacion { get; init; } = "";
    public string Departamento { get; init; } = "";
    public string DetalleSoporte { get; init; } = "";
    public string Equipo { get; init; } = "";
    public string Colaborador { get; init; } = "";
    public string Nivel { get; init; } = "";
    public string Extension { get; init; } = "";
    public string? TecnicoCompanero { get; init; } = "NO APLICA";
}

public record TicketEditarRequest
{
    public string IdTicket { get; init; } = "";
    public string Tecnico { get; init; } = "";
    public string? TecnicoCompanero { get; init; } = "NO APLICA";
    public string Colaborador { get; init; } = "";
    public string Extension { get; init; } = "";
    public string Ubicacion { get; init; } = "";
    public string Departamento { get; init; } = "";
    public string Equipo { get; init; } = "";
    public string Nivel { get; init; } = "";
    public string DetalleSoporte { get; init; } = "";
    public string Estado { get; init; } = "PENDIENTE";
}

public record ActualizarEstadoRequest
{
    public string IdTicket { get; init; } = "";
    public string NuevoEstado { get; init; } = "";
}

// -------------------------------------------------------------
// Modelo de salida (serializa en snake_case).
// -------------------------------------------------------------
public record Ticket
{
    public string IdTicket { get; init; } = "";
    public string Tecnico { get; init; } = "";
    public string TecnicoCompanero { get; init; } = "NO APLICA";
    public string Ubicacion { get; init; } = "";
    public string Departamento { get; init; } = "";
    public string DetalleSoporte { get; init; } = "";
    public string Equipo { get; init; } = "";
    public string Colaborador { get; init; } = "";
    public string Nivel { get; init; } = "";
    public string Estado { get; init; } = "PENDIENTE";
    public string Fecha { get; init; } = "";
    public string HoraInicio { get; init; } = "";
    public string HoraFinal { get; init; } = "--";
    public double TiempoTotal { get; init; }
    public string Extension { get; init; } = "--";

    // Orden de columnas idéntico a Db.SelectAll.
    public static Ticket From(SqliteDataReader r) => new()
    {
        IdTicket = r.GetString(0),
        Tecnico = r.IsDBNull(1) ? "" : r.GetString(1),
        TecnicoCompanero = r.IsDBNull(2) ? "NO APLICA" : r.GetString(2),
        Ubicacion = r.IsDBNull(3) ? "" : r.GetString(3),
        Departamento = r.IsDBNull(4) ? "" : r.GetString(4),
        DetalleSoporte = r.IsDBNull(5) ? "" : r.GetString(5),
        Equipo = r.IsDBNull(6) ? "" : r.GetString(6),
        Colaborador = r.IsDBNull(7) ? "" : r.GetString(7),
        Nivel = r.IsDBNull(8) ? "" : r.GetString(8),
        Estado = r.IsDBNull(9) ? "PENDIENTE" : r.GetString(9),
        Fecha = r.IsDBNull(10) ? "" : r.GetString(10),
        HoraInicio = r.IsDBNull(11) ? "" : r.GetString(11),
        HoraFinal = r.IsDBNull(12) ? "--" : r.GetString(12),
        TiempoTotal = r.IsDBNull(13) ? 0.0 : r.GetDouble(13),
        Extension = r.IsDBNull(14) ? "--" : r.GetString(14),
    };
}
