# Despliegue en IIS — Sistema de Soporte EGEHID

Aplicación **FastAPI** servida por **IIS** mediante `HttpPlatformHandler`
(IIS lanza un proceso `uvicorn` y actúa como proxy inverso hacia él).

## 1. Requisitos en el servidor

1. **Python 3.11+** instalado (marca "Add to PATH"). Anota la ruta real de
   `python.exe`, p. ej. `C:\Python312\python.exe`.
2. **IIS** con el rol de servidor web habilitado.
3. **HttpPlatformHandler** para IIS. Descárgalo del instalador oficial de
   Microsoft ("HttpPlatformHandler 1.2") e instálalo. Verifica que aparezca en
   *IIS Manager → Módulos* (`httpPlatformHandler`).

## 2. Copiar la aplicación

Copia el contenido de esta carpeta a, por ejemplo, `C:\inetpub\soporte-egehid\`.
Archivos necesarios en producción:

```
main.py
web.config
monitoreo.html
forms_soporte_ui.html
requirements.txt
*.ico  (opcional)
```

> No copies `soporte.db`, `venv/`, `__pycache__/` ni `dataset_soporte.py`
> (este último es material de referencia, no lo usa la app).

## 3. Instalar dependencias

Recomendado usar un entorno virtual dentro de la carpeta:

```powershell
cd C:\inetpub\soporte-egehid
C:\Python312\python.exe -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
```

Si usas venv, ajusta `processPath` en `web.config` a
`C:\inetpub\soporte-egehid\.venv\Scripts\python.exe`.

## 4. Ajustar `web.config`

Edita `processPath` para que apunte a tu `python.exe` (o al del venv).
Opcionalmente descomenta:

- `SOPORTE_DB_PATH` — para guardar la BD en una carpeta de datos aparte.
- `ALLOWED_ORIGINS` — solo si consumes la API desde **otro** sitio web.

## 5. Crear el sitio / aplicación en IIS

- **Como sitio nuevo:** IIS Manager → *Sites → Add Website*. Ruta física =
  la carpeta de la app. Elige puerto/host.
- **Como aplicación virtual** (bajo un sitio existente, p. ej. `/soporte`):
  *Add Application*, alias `soporte`, ruta física = la carpeta.
  El frontend ya usa rutas **relativas**, así que funciona en ambos casos.

## 6. Permisos (¡importante para SQLite!)

La identidad del **Application Pool** (por defecto `IIS AppPool\<NombrePool>`)
necesita permiso de **Modificar** sobre:

- La carpeta de la app (para crear `soporte.db`, `.db-wal`, `.db-shm`), **o**
- La carpeta indicada en `SOPORTE_DB_PATH`.

Además crea la carpeta `logs\` y dale escritura (la usa `stdoutLogFile`):

```powershell
icacls "C:\inetpub\soporte-egehid" /grant "IIS AppPool\soporte-egehid:(OI)(CI)M"
```

(Reemplaza `soporte-egehid` por el nombre real de tu Application Pool.)

## 7. Probar

- `http://<servidor>/` → redirige a `/monitoreo` (dashboard).
- `http://<servidor>/formulario` → registro de tickets.
- `http://<servidor>/api/health` → `{"status":"ok"}`.

Si algo falla, revisa `logs\stdout*.log`.

## Comprobación local (opcional, antes de subir)

```powershell
python -m pip install -r requirements.txt
python main.py
# Abre http://127.0.0.1:8000/
```

## Notas de seguridad

- La API **no tiene autenticación**. Si el sitio queda expuesto en red,
  protégelo con autenticación de IIS (Windows Auth) o restringe por IP/firewall.
- Considera activar **HTTPS** en IIS con un certificado.
- Se corrigió un XSS en el dashboard (los datos ahora se escapan al renderizar)
  y se validan `nivel` y `estado` en el backend.
