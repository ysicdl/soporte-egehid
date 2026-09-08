namespace SoporteEgehid;

// Dataset maestro EGEHID y validaciones. Copiado 1:1 de la app Python.
public static class Catalogos
{
    public static readonly string[] Tecnicos =
    {
        "PAUL RAFAEL SANTANA HIERRO",
        "ESMERLIN ZACARIAS CUELLO",
        "MIGUEL ANGEL PEÑA",
        "CARLOS ANTONIO GRULLON",
        "OTRO",
    };

    public static readonly string[] Ubicaciones =
    {
        "NO APLICA", "EDIFICIO PRINCIPAL", "CENTRAL HIDROELÉCTRICA TAVERA",
        "CENTRAL HIDROELÉCTRICA ANGOSTURA", "CENTRAL HIDROELÉCTRICA MONCIÓN",
        "CENTRAL HIDROELÉCTRICA BAIGUAQUE", "CENTRAL HIDROELÉCTRICA BRAZO DERECHO",
        "CENTRAL HIDROELÉCTRICA INOA", "CENTRAL HIDROELÉCTRICA JIMENOA",
        "CENTRAL HIDROELÉCTRICA EL SALTO", "CENTRAL HIDROELÉCTRICA CONSTANZA",
        "CENTRAL HIDROELÉCTRICA RINCÓN", "CENTRAL HIDROELÉCTRICA HATILLO",
        "CENTRAL HIDROELÉCTRICA RÍO BLANCO", "CENTRAL HIDROELÉCTRICA JIGÜEY",
        "CENTRAL HIDROELÉCTRICA AGUACATE", "CENTRAL HIDROELÉCTRICA VALDESIA",
        "CENTRAL HIDROELÉCTRICA NIZAO", "CENTRAL HIDROELÉCTRICA NAJAYO",
        "CENTRAL HIDROELÉCTRICA PALOMINO", "CENTRAL HIDROELÉCTRICA SABANA YEGUA",
        "CENTRAL HIDROELÉCTRICA SABANETA", "CENTRAL HIDROELÉCTRICA LAS DAMAS",
        "CENTRAL HIDROELÉCTRICA LOS TOROS", "CENTRAL HIDROELÉCTRICA DOMINGO RODRÍGUEZ",
        "CENTRAL HIDROELÉCTRICA MAGUEYAL", "CENTRAL HIDROELÉCTRICA OCOA",
        "CENTRO DE TRANSPORTACIÓN QUITASUEÑO EN HAINA",
    };

    public static readonly string[] Departamentos =
    {
        "NO APLICA", "ALMACEN", "ARCHIVO GENERAL", "CAPACITACION", "CCH", "COMUNICACIONES",
        "CONSULTORIO MEDICO", "CONTRALORIA", "COOPERATIVA", "DDH", "DIGITALIZACION",
        "DIRECCION ADMINITRATIVO", "DIRECCION DE RRPP", "DIRECCION DE TECNOLOGIA",
        "DIRECCION FINANCIERO", "DIRECCION GESTION HUMANA", "DIRECCION JURIDICA",
        "GESTION DE CALIDAD", "LIBRE ACCESO A LA INFOMACION", "MANTENIMIENTO MECANICO",
        "OBRAS CIVILES", "PLANIFICACION", "PRENSA", "PROTOCOLO", "PROYECTO ESPECIAL",
        "RECEPCION", "RELACIONES INTERNACIONALES", "SECRETARIA GENERAL",
        "SERVICIOS GENERALES", "SEDE QUITA SUEÑO", "AUDITORIA", "BIENESTAR Y ASISTENCIA SOCIAL",
        "SALON DE REUNIONES 5TO. PISO", "GERENCIA MECANICA", "CONTABILIDAD",
        "DIRECCION DE MANTENIMIENTO", "SEGURIDAD MILITAR", "NOMINA", "CENTRAL TAVERA",
        "SALON DE CONSEJO 5TO. PISO", "COMPRAS", "5TO PISO", "SUB ADMINISTRACION TECNICA",
        "SUB DIRECCION", "ENERGIA RENOVABLE", "COMERCIALIZACIÓN", "PRESIDENCIA DEL CONSEJO", "ADMINISTRACION",
    };

    public static readonly string[] Equipos =
    {
        "OTRO", "PC", "PRINTER", "PC/PRINTER", "PROYECTOR", "RED/INTERNET",
        "PROGRAMAS/APP", "PANTALLA/TV", "OTROS",
    };

    public static readonly string[] Niveles = { "NIVEL1", "NIVEL2", "NIVEL3" };

    public static readonly string[] Estados =
    {
        "RESUELTO", "NO RESUELTO", "PROCESO", "PROCESO/ RETIRO DE EQUIPO", "PENDIENTE", "ANULADO",
    };

    private static readonly HashSet<string> NivelesSet = new(Niveles.Select(n => n.ToUpperInvariant()));
    private static readonly HashSet<string> EstadosSet = new(Estados.Select(e => e.ToUpperInvariant()));

    public static bool TryValidarNivel(string? nivel, out string normalizado, out string error)
    {
        normalizado = (nivel ?? "").Trim().ToUpperInvariant();
        if (!NivelesSet.Contains(normalizado))
        {
            error = $"Nivel inválido: {nivel}";
            return false;
        }
        error = "";
        return true;
    }

    public static bool TryValidarEstado(string? estado, out string normalizado, out string error)
    {
        normalizado = (estado ?? "").Trim().ToUpperInvariant();
        if (!EstadosSet.Contains(normalizado))
        {
            error = $"Estado inválido: {estado}";
            return false;
        }
        error = "";
        return true;
    }
}
