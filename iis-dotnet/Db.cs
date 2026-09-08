using Microsoft.Data.Sqlite;

namespace SoporteEgehid;

// Acceso a datos SQLite. Réplica del esquema y las consultas de la app Python.
public static class Db
{
    public const string SelectAll =
        "SELECT id_ticket, tecnico, tecnico_companero, ubicacion, departamento, " +
        "detalle_soporte, equipo, colaborador, nivel, estado, fecha, hora_inicio, " +
        "hora_final, tiempo_total, extension FROM tickets " +
        "ORDER BY fecha DESC, hora_inicio DESC";

    public static SqliteConnection Open(string connString)
    {
        var conn = new SqliteConnection(connString);
        conn.Open();
        return conn;
    }

    public static void Init(string connString)
    {
        using var conn = Open(connString);
        using (var pragma = conn.CreateCommand())
        {
            pragma.CommandText = "PRAGMA journal_mode=WAL;";
            pragma.ExecuteNonQuery();
        }
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS tickets (
                id_ticket TEXT PRIMARY KEY,
                tecnico TEXT,
                tecnico_companero TEXT,
                ubicacion TEXT,
                departamento TEXT,
                detalle_soporte TEXT,
                equipo TEXT,
                colaborador TEXT,
                nivel TEXT,
                estado TEXT,
                fecha TEXT,
                hora_inicio TEXT,
                hora_final TEXT,
                tiempo_total REAL,
                extension TEXT
            )";
        cmd.ExecuteNonQuery();
    }

    // Lee (fecha, hora_inicio) de un ticket; fecha == null si no existe.
    public static (string?, string?) LeerFechaHora(SqliteConnection conn, string targetIdUpper)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT fecha, hora_inicio FROM tickets WHERE UPPER(id_ticket)=$id";
        cmd.AddParam("$id", targetIdUpper);
        using var r = cmd.ExecuteReader();
        if (!r.Read())
            return (null, null);
        return (r.IsDBNull(0) ? "" : r.GetString(0), r.IsDBNull(1) ? "" : r.GetString(1));
    }
}

public static class SqliteExtensions
{
    public static void AddParam(this SqliteCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}
