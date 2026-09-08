# Despliegue en IIS — Backend .NET (sin instalar módulos nuevos)

Este backend es una **API ASP.NET Core** (carpeta `iis-dotnet/`) que reemplaza a
la versión Python/FastAPI. Se ejecuta con el **ASP.NET Core Module
(`AspNetCoreModuleV2`)** y el **runtime ASP.NET Core** que tu IIS **ya tiene**
instalados por tus otras APIs .NET. **No requiere Python ni HttpPlatformHandler.**

Reproduce exactamente el mismo contrato de la API anterior (mismas rutas, mismo
JSON en `snake_case`, misma base de datos SQLite `soporte.db`), así que los HTML
`monitoreo.html` y `forms_soporte_ui.html` funcionan sin cambios.

## 1. Requisitos en el servidor (ya presentes si corres APIs .NET)

- **Runtime ASP.NET Core 8** (Hosting Bundle). Verifica con:
  ```powershell
  dotnet --list-runtimes
  ```
  Debe aparecer `Microsoft.AspNetCore.App 8.0.x`. (El proyecto apunta a `net8.0`;
  si prefieres otra versión instalada, cambia `<TargetFramework>` en el `.csproj`.)
- **Módulo `AspNetCoreModuleV2`** en IIS (lo instala el Hosting Bundle de .NET).

> Nada de esto se instala aparte si el servidor ya sirve APIs .NET.

## 2. Publicar la aplicación

En tu máquina de desarrollo (o en el servidor si tiene el SDK):

```powershell
cd iis-dotnet
dotnet publish -c Release -o publish
```

La carpeta `publish\` queda lista para copiarse a IIS. Ya incluye:
`SoporteEgehid.dll`, `web.config` (con `AspNetCoreModuleV2`), los HTML, la
librería nativa de SQLite (`runtimes\win-x64\native\e_sqlite3.dll`) y todas las
dependencias.

## 3. Copiar a IIS

Copia el **contenido de `publish\`** a, por ejemplo, `C:\inetpub\soporte-egehid\`.

## 4. Crear el sitio / aplicación en IIS

- **Sitio nuevo:** IIS Manager → *Add Website*. Ruta física = la carpeta.
- **Aplicación virtual** (bajo un sitio existente, p. ej. `/soporte`):
  *Add Application*, alias `soporte`, ruta física = la carpeta.
  El frontend usa rutas **relativas**, así que funciona en ambos casos.

**Application Pool:** ponlo en **"No Managed Code"** (.NET Core corre vía el
módulo, no por el CLR de IIS).

## 5. Permisos (¡importante para SQLite!)

La identidad del Application Pool (`IIS AppPool\<NombrePool>`) necesita permiso de
**Modificar** sobre la carpeta, para crear `soporte.db` y sus `.db-wal` / `.db-shm`:

```powershell
icacls "C:\inetpub\soporte-egehid" /grant "IIS AppPool\soporte-egehid:(OI)(CI)M"
```

(Reemplaza `soporte-egehid` por el nombre real de tu Application Pool.)

> Opcional: para guardar la BD en otra carpeta, define la variable de entorno
> `SOPORTE_DB_PATH` (p. ej. `C:\datos\soporte-egehid\soporte.db`). Puedes añadirla
> en `web.config` dentro de `<aspNetCore><environmentVariables>`.

## 6. Probar

- `http://<servidor>/` → redirige a `monitoreo` (dashboard).
- `http://<servidor>/formulario` → registro de tickets.
- `http://<servidor>/api/health` → `{"status":"ok"}`.

Si algo falla al arrancar, activa el log: en `web.config` pon
`stdoutLogEnabled="true"`, crea la carpeta `logs\` con permiso de escritura y
revisa `logs\stdout_*.log`. También mira el *Visor de eventos → Aplicación*.

## 7. Migrar datos existentes

La BD es el **mismo** `soporte.db` de SQLite. Si ya tenías tickets con la versión
Python, solo copia ese archivo `soporte.db` a la carpeta de la app (o apunta
`SOPORTE_DB_PATH` a su ubicación). El esquema es idéntico.

## Prueba local (opcional, antes de subir)

```powershell
cd iis-dotnet
dotnet run -c Release
# Abre http://localhost:5000/  (o el puerto que muestre la consola)
```

## Endpoints (idénticos a la versión Python)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/` | Redirige a `monitoreo` |
| GET | `/monitoreo` | Dashboard |
| GET | `/formulario` | Formulario de tickets |
| GET | `/api/health` | Estado del servicio |
| GET | `/api/catalogos` | Listas maestras (técnicos, ubicaciones, ...) |
| GET | `/api/tickets` | Lista de tickets |
| POST | `/api/crear-ticket` | Crea ticket |
| PUT | `/api/editar-ticket` | Edita ticket |
| POST | `/api/actualizar-estado` | Cambia estado (calcula cierre si RESUELTO) |
| DELETE | `/api/eliminar-ticket/{id}` | Elimina ticket |
| GET | `/api/descargar-excel` | Exporta reporte `.xlsx` |

## Notas de seguridad

- La API **no tiene autenticación**. Si queda expuesta en red, protégela con
  autenticación de IIS (Windows Auth) o restringe por IP/firewall.
- Considera **HTTPS** con certificado.
- Si consumes la API desde otro sitio web, define `ALLOWED_ORIGINS`
  (orígenes separados por coma) como variable de entorno.
