using System.Globalization;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SoporteEgehid;

// -------------------------------------------------------------
// Sistema de Soporte EGEHID — backend ASP.NET Core.
// Reescritura funcional de la app FastAPI original, pensada para
// desplegarse en IIS con el ASP.NET Core Module (ya instalado por
// las otras APIs .NET). No requiere instalar módulos nuevos.
// -------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// El frontend se sirve del mismo origen, así que CORS solo se abre si se
// declaran orígenes extra por variable de entorno (igual que la versión Python).
var originsRaw = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ?? "").Trim();
var allowedOrigins = originsRaw.Length == 0
    ? Array.Empty<string>()
    : originsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
}

// JSON en snake_case para conservar exactamente el contrato de la API Python
// (id_ticket, tecnico_companero, detalle_soporte, ...).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

if (allowedOrigins.Length > 0)
    app.UseCors();

// -------------------------------------------------------------
// Base de datos SQLite (modo WAL)
// -------------------------------------------------------------
var contentRoot = app.Environment.ContentRootPath;
var dbPath = Environment.GetEnvironmentVariable("SOPORTE_DB_PATH");
if (string.IsNullOrWhiteSpace(dbPath))
    dbPath = Path.Combine(contentRoot, "soporte.db");

var connString = new SqliteConnectionStringBuilder
{
    DataSource = dbPath,
    Mode = SqliteOpenMode.ReadWriteCreate,
    Cache = SqliteCacheMode.Shared,
}.ToString();

Db.Init(connString);

// -------------------------------------------------------------
// Vistas HTML
// -------------------------------------------------------------
app.MapGet("/", () => Results.Redirect("monitoreo"));

// Los HTML se copian junto al ensamblado; AppContext.BaseDirectory funciona
// tanto en 'dotnet run' (carpeta bin) como en el publish desplegado en IIS.
var webRoot = AppContext.BaseDirectory;
app.MapGet("/formulario", () => ServeHtml(webRoot, "forms_soporte_ui.html", "Vista de formulario no encontrada"));
app.MapGet("/monitoreo", () => ServeHtml(webRoot, "monitoreo.html", "Vista de monitoreo no encontrada"));

app.MapGet("/api/health", () => Results.Json(new { status = "ok" }));

// -------------------------------------------------------------
// Catálogos maestros
// -------------------------------------------------------------
app.MapGet("/api/catalogos", () => Results.Json(new
{
    tecnicos = Catalogos.Tecnicos,
    ubicaciones = Catalogos.Ubicaciones,
    departamentos = Catalogos.Departamentos,
    equipos = Catalogos.Equipos,
    niveles = Catalogos.Niveles,
    estados = Catalogos.Estados,
}));

// -------------------------------------------------------------
// Crear ticket
// -------------------------------------------------------------
app.MapPost("/api/crear-ticket", (TicketRequest ticket) =>
{
    try
    {
        if (!Catalogos.TryValidarNivel(ticket.Nivel, out var nivel, out var err))
            return Detail(422, err);

        var ahora = DateTime.Now;
        var idTicket = "TK-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var fecha = ahora.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var horaInicio = ahora.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        var companero = string.IsNullOrWhiteSpace(ticket.TecnicoCompanero)
            ? "NO APLICA" : ticket.TecnicoCompanero!.Trim();

        using var conn = Db.Open(connString);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO tickets (
                id_ticket, tecnico, tecnico_companero, ubicacion, departamento,
                detalle_soporte, equipo, colaborador, nivel, estado,
                fecha, hora_inicio, hora_final, tiempo_total, extension
            ) VALUES ($id, $tec, $comp, $ubi, $dep, $det, $equ, $col, $niv, $est,
                      $fec, $ini, $fin, $tot, $ext)";
        cmd.AddParam("$id", idTicket);
        cmd.AddParam("$tec", ticket.Tecnico);
        cmd.AddParam("$comp", companero);
        cmd.AddParam("$ubi", ticket.Ubicacion);
        cmd.AddParam("$dep", ticket.Departamento);
        cmd.AddParam("$det", ticket.DetalleSoporte);
        cmd.AddParam("$equ", ticket.Equipo);
        cmd.AddParam("$col", ticket.Colaborador);
        cmd.AddParam("$niv", nivel);
        cmd.AddParam("$est", "PENDIENTE");
        cmd.AddParam("$fec", fecha);
        cmd.AddParam("$ini", horaInicio);
        cmd.AddParam("$fin", "--");
        cmd.AddParam("$tot", 0.0);
        cmd.AddParam("$ext", (ticket.Extension ?? "").Trim());
        cmd.ExecuteNonQuery();

        return Results.Json(new { status = "success", id_ticket = idTicket, hora_inicio = horaInicio });
    }
    catch (Exception e) { return Detail(500, e.Message); }
});

// -------------------------------------------------------------
// Editar ticket
// -------------------------------------------------------------
app.MapPut("/api/editar-ticket", (TicketEditarRequest ticket) =>
{
    try
    {
        if (!Catalogos.TryValidarNivel(ticket.Nivel, out var nivel, out var errN))
            return Detail(422, errN);
        if (!Catalogos.TryValidarEstado(ticket.Estado, out var nuevoEst, out var errE))
            return Detail(422, errE);

        var targetId = (ticket.IdTicket ?? "").Trim().ToUpperInvariant();

        using var conn = Db.Open(connString);
        var (fecha, horaIni) = Db.LeerFechaHora(conn, targetId);
        if (fecha is null)
            return Detail(404, $"Ticket {ticket.IdTicket} no encontrado");

        var horaFinal = "--";
        var tiempoTotal = 0.0;
        if (nuevoEst == "RESUELTO")
            (horaFinal, tiempoTotal) = CalcularCierre(fecha, horaIni);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE tickets
            SET tecnico=$tec, tecnico_companero=$comp, colaborador=$col, extension=$ext,
                ubicacion=$ubi, departamento=$dep, equipo=$equ, nivel=$niv,
                detalle_soporte=$det, estado=$est, hora_final=$fin, tiempo_total=$tot
            WHERE UPPER(id_ticket)=$id";
        cmd.AddParam("$tec", ticket.Tecnico);
        cmd.AddParam("$comp", ticket.TecnicoCompanero ?? "NO APLICA");
        cmd.AddParam("$col", ticket.Colaborador);
        cmd.AddParam("$ext", ticket.Extension);
        cmd.AddParam("$ubi", ticket.Ubicacion);
        cmd.AddParam("$dep", ticket.Departamento);
        cmd.AddParam("$equ", ticket.Equipo);
        cmd.AddParam("$niv", nivel);
        cmd.AddParam("$det", ticket.DetalleSoporte);
        cmd.AddParam("$est", nuevoEst);
        cmd.AddParam("$fin", horaFinal);
        cmd.AddParam("$tot", tiempoTotal);
        cmd.AddParam("$id", targetId);
        cmd.ExecuteNonQuery();

        return Results.Json(new { status = "success", message = $"Ticket {targetId} actualizado exitosamente." });
    }
    catch (Exception e) { return Detail(500, e.Message); }
});

// -------------------------------------------------------------
// Eliminar ticket
// -------------------------------------------------------------
app.MapDelete("/api/eliminar-ticket/{id_ticket}", (string id_ticket) =>
{
    try
    {
        var targetId = (id_ticket ?? "").Trim().ToUpperInvariant();
        using var conn = Db.Open(connString);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM tickets WHERE UPPER(id_ticket)=$id";
        cmd.AddParam("$id", targetId);
        var filas = cmd.ExecuteNonQuery();

        if (filas == 0)
            return Detail(404, $"Ticket {id_ticket} no encontrado");

        return Results.Json(new { status = "success", message = $"Ticket {targetId} eliminado exitosamente." });
    }
    catch (Exception e) { return Detail(500, e.Message); }
});

// -------------------------------------------------------------
// Actualizar estado
// -------------------------------------------------------------
app.MapPost("/api/actualizar-estado", (ActualizarEstadoRequest req) =>
{
    try
    {
        if (!Catalogos.TryValidarEstado(req.NuevoEstado, out var nuevoEst, out var errE))
            return Detail(422, errE);

        var targetId = (req.IdTicket ?? "").Trim().ToUpperInvariant();

        using var conn = Db.Open(connString);
        var (fecha, horaIni) = Db.LeerFechaHora(conn, targetId);
        if (fecha is null)
            return Detail(404, $"Ticket {req.IdTicket} no encontrado");

        var horaFinal = "--";
        var tiempoTotal = 0.0;
        if (nuevoEst == "RESUELTO")
            (horaFinal, tiempoTotal) = CalcularCierre(fecha, horaIni);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE tickets SET estado=$est, hora_final=$fin, tiempo_total=$tot
                            WHERE UPPER(id_ticket)=$id";
        cmd.AddParam("$est", nuevoEst);
        cmd.AddParam("$fin", horaFinal);
        cmd.AddParam("$tot", tiempoTotal);
        cmd.AddParam("$id", targetId);
        cmd.ExecuteNonQuery();

        return Results.Json(new { status = "success", message = $"Estado de {req.IdTicket} actualizado a {nuevoEst}" });
    }
    catch (Exception e) { return Detail(500, e.Message); }
});

// -------------------------------------------------------------
// Listar tickets
// -------------------------------------------------------------
app.MapGet("/api/tickets", () =>
{
    try
    {
        var tickets = new List<Ticket>();
        using var conn = Db.Open(connString);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = Db.SelectAll;
        using var r = cmd.ExecuteReader();
        while (r.Read())
            tickets.Add(Ticket.From(r));

        return Results.Json(new { tickets });
    }
    catch (Exception e) { return Detail(500, e.Message); }
});

// -------------------------------------------------------------
// Descargar Excel
// -------------------------------------------------------------
app.MapGet("/api/descargar-excel", () =>
{
    try
    {
        var filas = new List<Ticket>();
        using (var conn = Db.Open(connString))
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = Db.SelectAll;
            using var r = cmd.ExecuteReader();
            while (r.Read())
                filas.Add(Ticket.From(r));
        }

        var bytes = ExcelReporte.Generar(filas);
        var nombre = $"Reporte_Soportes_EGEHID_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombre);
    }
    catch (Exception e) { return Detail(500, e.Message); }
});

app.Run();

// =============================================================
// Helpers de nivel superior
// =============================================================
static IResult ServeHtml(string root, string file, string notFoundMsg)
{
    var path = Path.Combine(root, file);
    if (!File.Exists(path))
        return Results.NotFound(new { detail = notFoundMsg });
    return Results.File(path, "text/html; charset=utf-8");
}

// Devuelve error con el mismo formato que FastAPI: {"detail": "..."}
static IResult Detail(int status, string msg) => Results.Json(new { detail = msg }, statusCode: status);

// Réplica de _calcular_cierre: (hora_final, tiempo_total_min) al pasar a RESUELTO.
static (string, double) CalcularCierre(string? fechaStr, string? horaInicioStr)
{
    try
    {
        var inicio = DateTime.ParseExact($"{fechaStr} {horaInicioStr}", "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture);
        var ahora = DateTime.Now;
        var segundos = (ahora - inicio).TotalSeconds;
        var minutos = Math.Round(Math.Max(segundos, 0) / 60.0, 2);
        return (ahora.ToString("HH:mm:ss", CultureInfo.InvariantCulture), minutos);
    }
    catch
    {
        return (DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture), 1.0);
    }
}
