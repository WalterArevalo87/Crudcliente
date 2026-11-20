# Agenda EEQ – Sistema de verificación de KPI

Aplicación web (ASP.NET Core MVC, .NET 8) que lee el Excel **AGENDA TRABAJO Z4** y muestra
los indicadores de gestión con semáforo, gráficos y el detalle de los registros de cada KPI.
No requiere base de datos.

## Requisitos
- SDK de .NET 8 o superior: https://dotnet.microsoft.com/download/dotnet/8.0

## Ejecutar
```
cd AgendaEEQ
dotnet run --launch-profile http
```
Abrir http://localhost:5189, subir el Excel en **Cargar Excel** y revisar el tablero.
El último archivo cargado se guarda en `AgendaEEQ/App_Data/` (no se sube a git).

## Hojas que se leen
SOLICITUDES, DISEÑOS, CONSTRUCCIÓN e INSPECCIONES Y REUNIONES. Las columnas se buscan por el
texto del encabezado, por lo que se pueden mover o insertar columnas sin romper el sistema.

## Indicadores
| Hoja | KPI | Meta por defecto |
|---|---|---|
| SOLICITUDES | % solicitudes atendidas | ≥ 95 % |
| | Tiempo promedio de respuesta (ingreso → primera respuesta al cliente) | ≤ 15 días |
| | % respondidas dentro del plazo | ≥ 80 % |
| | Calidad de datos (registros completos y fechas coherentes) | ≥ 90 % |
| DISEÑOS | % diseños concluidos (sin "No viable" ni "Sin trazado vial") | ≥ 70 % |
| | Avance promedio | ≥ 80 % |
| | Días desde el ingreso hasta el inicio del diseño | ≤ 30 días |
| | Presupuesto, viviendas, km MV/BV, luminarias | informativo |
| CONSTRUCCIÓN / INSPECCIONES | Obras construidas, inspecciones y reuniones | informativo |

Las metas y umbrales del semáforo se cambian en `AgendaEEQ/appsettings.json`, sección `KpiMetas`.

## Estructura
- `Services/AgendaExcelReader.cs` – lectura del Excel (ClosedXML).
- `Services/KpiCalculator.cs` – cálculo de indicadores y vistas de detalle.
- `Services/AgendaStore.cs` – guarda el último Excel cargado.
- `Controllers/KpiController.cs` – tablero, detalle y carga.
