# -*- coding: utf-8 -*-
"""
Dataset extraído de "Copia_de_SERVICIOS_DE_SOPORTE_MACRO.xlsm" (hoja "DATOS").

Esta hoja contiene los catálogos maestros (listas desplegables) usados por el
formulario de registro de soporte técnico. La hoja "MAYO 2025" es la plantilla
mensual de captura de tickets y actualmente está vacía (sin registros cargados),
por lo que no se incluye data de tickets aquí — solo los catálogos.

Estructura de un ticket (columnas del formulario "MAYO 2025"), por si luego
quieres registrar entradas:
    TECNICOS, TECN. COMPAÑERO, DIRECCION O DEPARTAMENTOS, DETALLE SOPORTE,
    EQUIPO, COLABORADOR (A), NIVEL, ESTADO, FECHA, HORA INICIO, HORA FINAL,
    EFICIENCIA
"""

# Técnicos de soporte
TECNICOS = [
    "PAUL RAFAEL SANTANA HIERRO",
    "ESMERLIN ZACARIAS CUELLO",
    "YESSICA BOURET",
    "SAMUEL DAVID MATEO RIVERA",
    "ISAAC EMMANUEL TEJADA",
    "MIGUEL ANGEL PEÑA",
    "CARLOS ANTONIO GRULLON",
    "JOAN ABAD",
]


UBICACIONES = [
    "EDIFICIO PRINCIPAL",
    "CENTRAL HIDROELÉCTRICA TAVERA",
    "CENTRAL HIDROELÉCTRICA ANGOSTURA",
    "CENTRAL HIDROELÉCTRICA MONCIÓN",
    "CENTRAL HIDROELÉCTRICA BAIGUAQUE",
    "CENTRAL HIDROELÉCTRICA BRAZO DERECHO",
    "CENTRAL HIDROELÉCTRICA INOA",
    "CENTRAL HIDROELÉCTRICA JIMENOA",
    "CENTRAL HIDROELÉCTRICA EL SALTO",
    "CENTRAL HIDROELÉCTRICA CONSTANZA",
    "CENTRAL HIDROELÉCTRICA RINCÓN",
    "CENTRAL HIDROELÉCTRICA HATILLO",
    "CENTRAL HIDROELÉCTRICA RÍO BLANCO",
    "CENTRAL HIDROELÉCTRICA JIGÜEY",
    "CENTRAL HIDROELÉCTRICA AGUACATE",
    "CENTRAL HIDROELÉCTRICA VALDESIA",
    "CENTRAL HIDROELÉCTRICA NIZAO",
    "CENTRAL HIDROELÉCTRICA NAJAYO",
    "CENTRAL HIDROELÉCTRICA PALOMINO",
    "CENTRAL HIDROELÉCTRICA SABANA YEGUA",
    "CENTRAL HIDROELÉCTRICA SABANETA",
    "CENTRAL HIDROELÉCTRICA LAS DAMAS",
    "CENTRAL HIDROELÉCTRICA LOS TOROS",
    "CENTRAL HIDROELÉCTRICA DOMINGO RODRÍGUEZ",
    "CENTRAL HIDROELÉCTRICA MAGUEYAL",
    "CENTRAL HIDROELÉCTRICA OCOA",
    "CENTRO DE TRANSPORTACIÓN QUITASUEÑO EN HAINA"
]

# Direcciones o departamentos que reciben soporte
DEPARTAMENTOS = [
    "ALMACEN",
    "ARCHIVO GENERAL",
    "CAPACITACION",
    "CCH",
    "COMUNICACIONES",
    "CONSULTORIO MEDICO",
    "CONTRALORIA",
    "COOPERATIVA",
    "DDH",
    "DIGITALIZACION",
    "DIRECCION ADMINITRATIVO",
    "DIRECCION DE RRPP",
    "DIRECCION DE TECNOLOGIA",
    "DIRECCION FINANCIERO",
    "DIRECCION GESTION HUMANA",
    "DIRECCION JURIDICA",
    "GESTION DE CALIDAD",
    "LIBRE ACCESO A LA INFOMACION",
    "MANTENIMIENTO MECANICO",
    "OBRAS CIVILES",
    "PLANIFICACION",
    "PRENSA",
    "PROTOCOLO",
    "PROYECTO ESPECIAL",
    "RECEPCION",
    "RELACIONES INTERNACIONALES",
    "SECRETARIA GENERAL",
    "SERVICIOS GENERALES",
    "SEDE QUITA SUEÑO",
    "AUDITORIA",
    "BIENESTAR Y ASISTENCIA SOCIAL",
    "SALON DE REUNIONES 5TO. PISO",
    "GERENCIA MECANICA",
    "CONTABILIDAD",
    "DIRECCION DE MANTENIMIENTO",
    "SEGURIDAD MILITAR",
    "NOMINA",
    "CENTRAL TAVERA",
    "SALON DE CONSEJO 5TO. PISO",
    "COMPRAS",
    "5TO PISO",
    "SUB ADMINISTRACION TECNICA",
    "SUB DIRECCION",
    "ENERGIA RENOVABLE",
    "COMERCIALIZACIÓN",
    "MILITAR PISO 2",
    "MILITAR PISO 3",
    "MILITAR PISO 4",
    "MILITAR PISO 5",
    "FULGON",
    "PRESIDENCIA DEL CONSEJO",
    "ADMINISTRACION",
]

# Tipos de equipo atendidos
EQUIPOS = [
    "PC",
    "PRINTER",
    "PC/PRINTER",
    "PROYECTOR",
    "RED/INTERNET",
    "PROGRAMAS/APP",
    "PANTALLA/TV",
    "OTROS",
]

# Niveles de soporte
NIVELES = [
    "NIVEL1",
    "NIVEL2",
    "NIVEL3",
]

# Estados posibles de un ticket
ESTADOS = [
    "RESUELTO",
    "NO RESUELTO",
    "PROCESO",
    "PROCESO/ RETIRO DE EQUIPO",
    "PENDIENTE",
]

# Catálogo consolidado, útil para exportar a JSON o poblar un formulario
CATALOGOS = {
    "tecnicos": TECNICOS,
    "departamentos": DEPARTAMENTOS,
    "equipos": EQUIPOS,
    "niveles": NIVELES,
    "estados": ESTADOS,
}

# Columnas del formulario de tickets (hoja "MAYO 2025"), para cuando
# empieces a registrar entradas reales
COLUMNAS_TICKET = [
    "TECNICOS",
    "TECN. COMPAÑERO",
    "DIRECCION O DEPARTAMENTOS",
    "DETALLE SOPORTE",
    "EQUIPO",
    "COLABORADOR (A)",
    "NIVEL",
    "ESTADO",
    "FECHA",
    "HORA INICIO",
    "HORA FINAL",
    "EFICIENCIA",
]

# Lista vacía de tickets — la hoja "MAYO 2025" del Excel no tenía registros
# cargados todavía. Cada ticket puede representarse como un dict con las
# claves de COLUMNAS_TICKET.
TICKETS = []


if __name__ == "__main__":
    import json
    print(json.dumps(CATALOGOS, ensure_ascii=False, indent=2))