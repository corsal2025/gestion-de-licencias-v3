using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using GestionLicencias.Core.Domain;
using GestionLicencias.Core.Domain.Entities;
using GestionLicencias.Core.Domain.Interfaces;
using GestionLicencias.Infrastructure.Services;
using GestionLicencias.Web.Services;

namespace GestionLicencias.Web.Components.Pages.Modulos.Licencias;

public partial class LicenciasList
{
    [Inject] TramitesBroadcast Broadcast { get; set; } = default!;
    private List<TramiteLicencia> Registros = new();
    private HashSet<string> RutsDuplicados = new();
    private Dictionary<string, int> Estadisticas = new();
    private bool Cargando = false;
    private string FiltroTermino = "";
    private string FiltroOtorgamiento = "";
    private string FiltroEstadoCarpeta = "";
    private int PaginaActual = 1;
    private int TotalResultados = 0;
    private int TotalPaginas = 0;
    private int PageSize = 50;
    private string Mensaje = "";
    private bool EsError = false;
    private bool MostrarPegado = false;
    private string? NombreArchivoExcel;
    private int RegistrosImportados;
    private string LugarAtencionImport = "";
    private bool MostrarPdf = false;
    private string PdfOpcion = "";
    private bool GenerandoPdf = false;
    private string PdfLugar = "PLACILLA";
    private string PdfFuncionaria = "SANDRA";
    private string TextoPegado = "";
    private string TipoListado = "Todos";
    private bool MostrarDominios = false;
    private List<string> Dominios = new();
    private string NuevoDominio = "";
    private bool MostrarToastDomicilio = false;
    private int? CiudadDropdownAbierto = null;
    private string FiltroCiudadRapida = "";
    private HashSet<int> Seleccionados = new();
    private bool TodosSeleccionados => Registros.Count > 0 && Registros.All(r => Seleccionados.Contains(r.Id));
    private readonly SemaphoreSlim _guardandoSem = new(1, 1);

    private string UsuarioActual => Sesion.Actual?.NombreUsuario ?? "SISTEMA";
    private bool PuedeGestionarJuzgado => Sesion.Actual?.Rol is Roles.Administrador or Roles.Jefatura;
    private bool NoEdita(string seccion) => !Sesion.PuedeEditar(seccion);
    private bool IdoneidadDeshabilitadaFila(TramiteLicencia r) =>
        NoEdita(Roles.IdoneidadMoral) || (r.ProcesoBloqueado && !PuedeGestionarJuzgado);

    private static readonly string[] DominiosBase =
        ["gmail.com", "hotmail.com", "outlook.com", "yahoo.com", "icloud.com", "munivalpo.cl"];

    private static readonly Dictionary<string, string> FuncionariaPorLugar = new()
    {
        ["PLACILLA"] = "SANDRA",
        ["AV. ARGENTINA"] = "",
        ["MERCADO PUERTO"] = ""
    };

    private static readonly string[] FormatosFecha =
        ["d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "dd-MM-yyyy", "yyyy-MM-dd"];

    protected override async Task OnInitializedAsync() => await BuscarAsync();

    private async Task BuscarAsync()
    {
        Cargando = true;
        StateHasChanged();
        try
        {
            var (res, tot) = await ModuloService.BuscarAsync(new BusquedaFilter
            {
                Termino = FiltroTermino,
                Otorgamiento = FiltroOtorgamiento,
                EstadoCarpeta = FiltroEstadoCarpeta,
                Pagina = PaginaActual,
                PageSize = PageSize
            });
            Registros = res.ToList();
            foreach (var r in Registros)
            {
                r.RUT = BaseTramite.FormatearRut(r.RUT);
                if (r.CambioDomicilioActivo && !string.IsNullOrEmpty(r.CiudadCambioDomicilio) && string.IsNullOrEmpty(r.LugarOrigenCambioDomicilio))
                    r.LugarOrigenCambioDomicilio = r.CiudadCambioDomicilio;
            }
            TotalResultados = tot;
            TotalPaginas = Math.Max(1, (int)Math.Ceiling((double)tot / PageSize));
            ActualizarDuplicados();
            Estadisticas = await StatsService.ObtenerEstadisticasOtorgamientoAsync();
            var statsConaset = await StatsService.ObtenerEstadisticasConasetAsync();
            var statsCarpetaPedida = await StatsService.ObtenerEstadisticasCarpetaPedidaAsync();
            foreach (var kv in statsConaset) Estadisticas[kv.Key] = kv.Value;
            foreach (var kv in statsCarpetaPedida) Estadisticas[kv.Key] = kv.Value;
        }
        catch (Exception ex)
        {
            Mensaje = $"Error: {ex.Message}";
            EsError = true;
        }
        finally
        {
            Cargando = false;
        }
    }

    private void ActualizarDuplicados() =>
        RutsDuplicados = Registros.GroupBy(r => r.RUT)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

    private static string NormalizarTelefono(string? telefono)
    {
        var t = (telefono ?? "").Trim();
        if (t.Length == 0) return t;
        var digits = System.Text.RegularExpressions.Regex.Replace(t, @"\D", "");
        if (digits.StartsWith("56") && digits.Length > 9)
            digits = digits[2..];
        return $"+56 - {digits}";
    }

    private async Task GuardarFila(TramiteLicencia r)
    {
        r.RUT = BaseTramite.FormatearRut(r.RUT);
        r.Telefono = NormalizarTelefono(r.Telefono);

        var (ok, msg) = ModuloService.ValidarTramite(r);
        if (!ok)
        {
            Mensaje = $"No guardado: {msg}";
            EsError = true;
            return;
        }

        try
        {
            r.UsuarioId = UsuarioActual;
            await ModuloService.ActualizarAsync(r);
            _ = Broadcast.NotificarAsync();
            ActualizarDuplicados();
            Estadisticas = await StatsService.ObtenerEstadisticasOtorgamientoAsync();
            var statsConaset = await StatsService.ObtenerEstadisticasConasetAsync();
            var statsCarpetaPedida = await StatsService.ObtenerEstadisticasCarpetaPedidaAsync();
            foreach (var kv in statsConaset) Estadisticas[kv.Key] = kv.Value;
            foreach (var kv in statsCarpetaPedida) Estadisticas[kv.Key] = kv.Value;
            var quien = string.IsNullOrWhiteSpace(r.NombreCompleto) ? r.RUT : r.NombreCompleto;
            Mensaje = $"Guardado: {quien} — {DateTime.Now:HH:mm:ss}";
            EsError = false;
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al guardar: {ex.Message}";
            EsError = true;
        }
    }

    private async Task OnAsisteChanged(TramiteLicencia r)
    {
        if (r.ProcesoBloqueado)
            r.Estado = "BLOQUEADO - NO ASISTIÓ";
        else if (r.Estado == "BLOQUEADO - NO ASISTIÓ")
            r.Estado = "En Proceso";

        await GuardarFila(r);

        if (r.ProcesoBloqueado)
        {
            Mensaje = $"Proceso bloqueado: {(string.IsNullOrWhiteSpace(r.NombreCompleto) ? r.RUT : r.NombreCompleto)} no asistió. Debe realizar el trámite nuevamente desde cero.";
            EsError = true;
        }
    }

    private async Task OnLugarAtencionChanged(TramiteLicencia r)
    {
        if (r.LugarAtencion == "PLACILLA")
            r.ImpresaPor = "SANDRA";
        else
            r.ImpresaPor = null;
        await GuardarFila(r);
    }

    private async Task OnEstadoCarpetaChanged(TramiteLicencia r)
    {
        if (r.EstadoCarpeta == "SUBIDA A CONASET")
            r.FechaSubidaConaset = DateTime.Today;
        else
            r.FechaSubidaConaset = null;

        if (!r.CambioDomicilioActivo)
        {
            r.CambioDomicilioPedido = null;
            r.CiudadCambioDomicilio = null;
            r.LugarOrigenCambioDomicilio = null;
        }
        else
        {
            r.LugarOrigenCambioDomicilio = r.CiudadCambioDomicilio;
        }
        await GuardarFila(r);
    }

    private async Task OnCambioDomicilioPedidoChanged(TramiteLicencia r)
    {
        if (r.CambioDomicilioPedido == "SI")
        {
            r.LugarOrigenCambioDomicilio = r.CiudadCambioDomicilio;
        }
        else
        {
            r.LugarOrigenCambioDomicilio = null;
        }

        await GuardarFila(r);

        if (r.CambioDomicilioPedido == "NO")
        {
            MostrarToastDomicilio = true;
            StateHasChanged();
            await Task.Delay(4000);
            MostrarToastDomicilio = false;
            StateHasChanged();
        }
    }

    private static string[] FiltrarCiudades(string filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro))
            return Catalogos.CiudadesChile.Take(20).ToArray();
        return Catalogos.CiudadesChile
            .Where(c => c.Contains(filtro.ToUpperInvariant()))
            .Take(20)
            .ToArray();
    }

    private void AbrirCiudadDropdown(TramiteLicencia r)
    {
        CiudadDropdownAbierto = r.Id;
        FiltroCiudadRapida = r.CiudadCambioDomicilio ?? "";
    }

    private async Task FiltrarCiudadRapida(TramiteLicencia r, ChangeEventArgs e)
    {
        FiltroCiudadRapida = e.Value?.ToString() ?? "";
        CiudadDropdownAbierto = r.Id;

        var valor = string.IsNullOrWhiteSpace(FiltroCiudadRapida) ? null : FiltroCiudadRapida.ToUpperInvariant();
        r.CiudadCambioDomicilio = valor;
        if (r.CambioDomicilioActivo)
        {
            r.LugarOrigenCambioDomicilio = valor;
        }
        else
        {
            r.LugarOrigenCambioDomicilio = null;
        }

        if (string.IsNullOrWhiteSpace(FiltroCiudadRapida))
        {
            await GuardarFila(r);
        }
        await Task.CompletedTask;
    }

    private async Task SeleccionarCiudadRapida(TramiteLicencia r, string ciudad)
    {
        r.CiudadCambioDomicilio = ciudad;
        if (r.CambioDomicilioActivo)
        {
            r.LugarOrigenCambioDomicilio = ciudad;
        }
        else
        {
            r.LugarOrigenCambioDomicilio = null;
        }
        CiudadDropdownAbierto = null;
        FiltroCiudadRapida = "";
        await GuardarFila(r);
    }

    private async Task OnEstadoF8Changed(TramiteLicencia r, ChangeEventArgs e)
    {
        r.EstadoF8 = e.Value?.ToString() ?? "";

        r.EstadoCarpeta = r.EstadoF8 switch
        {
            "SUBIDA A CONASET"    => "SUBIDA CON F8",
            "CREAR OFICIO"        => "CREAR OFICIO",
            "CAMBIO DE DOMICILIO" => "CAMBIO DE DOMICILIO",
            "PRIMERA LICENCIA"    => "1° LICENCIA",
            _                     => r.EstadoCarpeta
        };

        r.FechaSubidaConaset = r.EstadoF8 == "SUBIDA A CONASET" ? DateTime.Today : null;

        await GuardarFila(r);
    }

    private async Task OnIdoneidadMoralChanged(TramiteLicencia r)
    {
        if (r.IdoneidadMoral == "REPROBADA" || r.IdoneidadMoral == "JUZGADO")
        {
            r.DesbloqueoAdministrativo = false;
            r.DesbloqueadoPor = null;
            r.FechaDesbloqueo = null;
        }
        await GuardarFila(r);
        if (r.IdoneidadMoral == "JUZGADO")
        {
            var nombre = r.NombreCompleto ?? $"{r.Nombre} {r.Apellido}";
            var msg = $"ATENCIÓN: El postulante {nombre} (RUT: {r.RUT}) ha sido derivado a JUZGADO. Su proceso quedará bloqueado.";
            await JS.InvokeVoidAsync("alert", msg);
        }
    }

    private static bool EsBloqueoPorJuzgado(TramiteLicencia r) =>
        r.IdoneidadMoral == "JUZGADO" && !r.DesbloqueoAdministrativo;

    private static bool EstaBloqueadoPorRechazoOJuzgado(TramiteLicencia r) =>
        !r.DesbloqueoAdministrativo && (r.IdoneidadMoral == "REPROBADA" || r.IdoneidadMoral == "JUZGADO");

    private async Task BloquearJuzgado(TramiteLicencia r)
    {
        if (!PuedeGestionarJuzgado) return;

        r.IdoneidadMoral = "JUZGADO";
        r.DesbloqueoAdministrativo = false;
        r.DesbloqueadoPor = null;
        r.FechaDesbloqueo = null;

        await GuardarFila(r);
        var nombre = string.IsNullOrWhiteSpace(r.NombreCompleto) ? r.RUT : r.NombreCompleto;
        await JS.InvokeVoidAsync("alert", $"ATENCIÓN: El postulante {nombre} (RUT: {r.RUT}) ha sido derivado a JUZGADO. Su proceso quedó bloqueado.");
    }

    private async Task DesbloquearJuzgado(TramiteLicencia r)
    {
        if (!PuedeGestionarJuzgado) return;

        r.DesbloqueoAdministrativo = true;
        r.DesbloqueadoPor = UsuarioActual;
        r.FechaDesbloqueo = DateTime.UtcNow;

        await GuardarFila(r);
        Mensaje = $"Bloqueo por Juzgado levantado: {(string.IsNullOrWhiteSpace(r.NombreCompleto) ? r.RUT : r.NombreCompleto)}";
        EsError = false;
    }

    private void EnviarCorreoSolicitudCarpeta(TramiteLicencia r)
    {
        var ciudad = r.CiudadCambioDomicilio ?? "";
        var nombre = r.NombreCompleto ?? $"{r.Nombre} {r.Apellido}";
        var rut = r.RUT ?? "";

        var asunto = Uri.EscapeDataString($"Solicitud de Carpeta — {nombre} ({rut})");
        var cuerpo = Uri.EscapeDataString(
$@"Estimados,

Reciban un cordial saludo de la Municipalidad de Valparaíso.

Por medio del presente, solicito a ustedes la entrega de la última carpeta disponible en su municipio correspondiente al siguiente contribuyente:

Nombre: {nombre}
RUT: {rut}
Ciudad de origen: {ciudad}

Agradeciendo de antemano su atención, quedo atento(a) a sus noticias.

Saludos cordiales,
Municipalidad de Valparaíso
Departamento de Licencias de Conducir");

        var mailto = $"mailto:?subject={asunto}&body={cuerpo}";
        JS.InvokeVoidAsync("eval", $"window.open('{mailto}', '_blank')");
    }

    private async Task EnviarCorreoBloqueoContribuyente(TramiteLicencia r)
    {
        if (!EstaBloqueadoPorRechazoOJuzgado(r))
            return;

        if (string.IsNullOrWhiteSpace(r.Email))
        {
            await JS.InvokeVoidAsync("alert", "El contribuyente no tiene correo registrado.");
            return;
        }

        var nombre = string.IsNullOrWhiteSpace(r.NombreCompleto) ? $"{r.Nombre} {r.Apellido}".Trim() : r.NombreCompleto;
        var rut = r.RUT ?? "";
        var motivo = r.IdoneidadMoral == "JUZGADO"
            ? "derivación a Juzgado"
            : "rechazo de idoneidad moral";

        var asunto = Uri.EscapeDataString($"Estado de su trámite de licencia — {rut}");
        var cuerpo = Uri.EscapeDataString(
$@"Estimado(a) {nombre},

Le informamos que su trámite de licencia de conducir se encuentra temporalmente bloqueado por {motivo}.

RUT: {rut}
Estado actual: {r.IdoneidadMoral}

Para más información, por favor comuníquese con la Municipalidad de Valparaíso, Departamento de Licencias de Conducir.

Saludos cordiales,
Municipalidad de Valparaíso
Departamento de Licencias de Conducir");

        var destinatario = Uri.EscapeDataString(r.Email.Trim());
        var mailto = $"mailto:{destinatario}?subject={asunto}&body={cuerpo}";
        await JS.InvokeVoidAsync("eval", $"window.open('{mailto}', '_blank')");
    }

    private async Task ActualizarEstadoCarpeta(TramiteLicencia r)
    {
        r.EstadoCarpeta = r.FechaUltimaCarpeta.HasValue
            && r.FechaUltimaCarpeta.Value >= new DateTime(2023, 7, 1)
                ? "SE ENCUENTRA EN OF.43"
                : "SE ENCUENTRA EN ARCHIVOS";

        if (!r.CambioDomicilioActivo)
        {
            r.CambioDomicilioPedido = null;
            r.CiudadCambioDomicilio = null;
            r.LugarOrigenCambioDomicilio = null;
        }
        await GuardarFila(r);
    }

    private async Task Desbloquear(TramiteLicencia r)
    {
        r.DesbloqueoAdministrativo = true;
        r.DesbloqueadoPor = UsuarioActual;
        r.FechaDesbloqueo = DateTime.UtcNow;
        if (r.Estado == "BLOQUEADO - NO ASISTIÓ") r.Estado = "En Proceso";
        await GuardarFila(r);
    }

    private async Task BuscarClick()
    {
        PaginaActual = 1;
        await BuscarAsync();
    }

    private async Task OnKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await BuscarClick();
    }

    private async Task Ir(int p)
    {
        PaginaActual = Math.Clamp(p, 1, TotalPaginas);
        await BuscarAsync();
    }

    private async Task OnPageSizeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var newSize))
        {
            PageSize = newSize;
            PaginaActual = 1;
            await BuscarAsync();
        }
    }

    private async Task Limpiar()
    {
        FiltroTermino = FiltroOtorgamiento = FiltroEstadoCarpeta = "";
        PaginaActual = 1;
        Mensaje = "";
        await BuscarAsync();
    }

    private IEnumerable<int> PaginasVisibles()
    {
        var desde = Math.Max(1, PaginaActual - 2);
        var hasta = Math.Min(TotalPaginas, PaginaActual + 2);
        for (var i = desde; i <= hasta; i++) yield return i;
    }

    private void Nuevo() => Nav.NavigateTo("/modulos/licencias/crear");
    private void Editar(int id) => Nav.NavigateTo($"/modulos/licencias/editar/{id}");
    private void IrEstadisticas() => Nav.NavigateTo("/modulos/licencias/estadisticas");
    private void IrSeguimiento() => Nav.NavigateTo("/modulos/licencias/seguimiento");

    private async Task Eliminar(int id)
    {
        var ok = await JS.InvokeAsync<bool>("confirm", "¿Eliminar este registro?");
        if (!ok) return;
        await ModuloService.EliminarAsync(id);
        Mensaje = "Registro eliminado.";
        EsError = false;
        await BuscarAsync();
    }

    private void ToggleSeleccion(int id)
    {
        if (!Seleccionados.Remove(id))
            Seleccionados.Add(id);
    }

    private void ToggleSeleccionTodos(ChangeEventArgs e)
    {
        var isChecked = (bool)(e.Value ?? false);
        if (isChecked)
        {
            foreach (var r in Registros)
                Seleccionados.Add(r.Id);
        }
        else
        {
            foreach (var r in Registros)
                Seleccionados.Remove(r.Id);
        }
    }

    private async Task EliminarSeleccionados()
    {
        var count = Seleccionados.Count;
        if (count == 0) return;
        var ok = await JS.InvokeAsync<bool>("confirm", $"¿Eliminar {count} registro(s) seleccionados?");
        if (!ok) return;
        var eliminados = await ModuloService.EliminarMultiplesAsync(Seleccionados);
        Mensaje = $"{eliminados} registro(s) eliminados correctamente.";
        EsError = false;
        Seleccionados.Clear();
        await BuscarAsync();
    }

    private static string ClaseFila(TramiteLicencia r)
    {
        if (!r.Activo) return "f8-eliminado";
        if (r.Asiste != "SI") return "ta-pendiente";
        return ClaseEstado(r.EstadoCarpeta);
    }

    private static string ClaseEstado(string? estadoCarpeta)
    {
        if (string.IsNullOrWhiteSpace(estadoCarpeta)) return "";
        var idx = Array.IndexOf(Catalogos.EstadosCarpeta, estadoCarpeta);
        return idx >= 0 ? $"ec-{idx}" : "";
    }

    private static string ClaseSiNo(string? valor) => valor switch
    {
        "SI" => "ta-si",
        "NO" => "ta-no",
        _ => ""
    };

    private static string ClaseAprobada(string? valor) => valor switch
    {
        "APROBADA" => "ta-si",
        "REPROBADA" => "ta-no",
        _ => ""
    };

    private static string ChipOtorgamiento(string? otorgamiento) => otorgamiento switch
    {
        "OTORGADO" => "ta-chip-otorgado",
        "DENEGADO" => "ta-chip-denegado",
        "ESPERA EXAMEN" => "ta-chip-espera",
        "S/SGL" => "ta-chip-ssgl",
        _ => ""
    };

    private static string ClaseStat(string otorgamiento) => otorgamiento switch
    {
        "OTORGADO" => "ta-stat-ok",
        "DENEGADO" => "ta-stat-bad",
        "ESPERA EXAMEN" => "ta-stat-warn",
        "Subidas CONASET hoy" => "ta-stat-ok",
        "Subidas CONASET semana" => "ta-stat-ok",
        "Subidas CONASET mes" => "ta-stat-ok",
        "Carpetas pedidas" => "ta-stat-warn",
        _ => ""
    };

    private static string F(DateTime? d) => d?.ToString("dd/MM/yyyy") ?? "";

    private void OnFiltroTerminoChanged(ChangeEventArgs e)
    {
        var raw = e.Value?.ToString() ?? "";
        FiltroTermino = FormatearRutSiAplica(raw);
    }

    private static string FormatearRutSiAplica(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var digits = new string(input.Where(c => char.IsDigit(c) || c == 'k' || c == 'K').ToArray());
        if (digits.Length < 7) return input;
        var body = digits[..^1];
        var dv = digits[^1].ToString().ToUpper();
        var formatted = string.Empty;
        int count = 0;
        for (int i = body.Length - 1; i >= 0; i--)
        {
            if (count > 0 && count % 3 == 0) formatted = "." + formatted;
            formatted = body[i] + formatted;
            count++;
        }
        return formatted + "-" + dv;
    }

    private void AbrirPdf()
    {
        PdfOpcion = "";
        TipoListado = "";
        MostrarPdf = true;
    }

    private void CerrarPdf()
    {
        MostrarPdf = false;
        PdfOpcion = "";
        TipoListado = "";
    }

    private void OnPdfLugarChanged(ChangeEventArgs e)
    {
        PdfLugar = e.Value?.ToString() ?? "PLACILLA";
        PdfFuncionaria = FuncionariaPorLugar.GetValueOrDefault(PdfLugar, "");
    }

    private async Task<List<TramiteLicencia>> ObtenerTodosActivos()
    {
        var (todos, _) = await ModuloService.BuscarAsync(new BusquedaFilter
        {
            Pagina = 1,
            PageSize = 100000
        });
        return todos.ToList();
    }

    private async Task DescargarImpresas()
    {
        GenerandoPdf = true;
        try
        {
            var todos = await ObtenerTodosActivos();
            var bytes = ReporteImpresasPdf.Generar(todos, PdfLugar, PdfFuncionaria);
            var lugarSlug = PdfLugar.ToLowerInvariant().Replace(" ", "_").Replace(".", "");
            await DescargarArchivoPdf(bytes, $"reporte_impresas_{lugarSlug}.pdf");
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al generar PDF: {ex.Message}";
            EsError = true;
        }
        finally
        {
            GenerandoPdf = false;
        }
    }

    private async Task DescargarCarpetaPdf()
    {
        if (string.IsNullOrEmpty(TipoListado))
        {
            Mensaje = "Debes elegir una ubicación de carpeta.";
            EsError = true;
            return;
        }
        bool penultima = PdfOpcion == "penultima";
        GenerandoPdf = true;
        try
        {
            var todos = await ObtenerTodosActivos();
            var registros = FiltrarPorTipoListado(todos, penultima);
            var bytes = ReporteCarpetasPdf.Generar(registros, penultima);
            await DescargarArchivoPdf(bytes, penultima ? "reporte_penultima_carpeta.pdf" : "reporte_ultima_carpeta.pdf");
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al generar PDF: {ex.Message}";
            EsError = true;
        }
        finally
        {
            GenerandoPdf = false;
        }
    }

    private List<TramiteLicencia> FiltrarPorTipoListado(List<TramiteLicencia> registros, bool penultima)
    {
        Func<TramiteLicencia, DateTime?> fechaCampo = penultima
            ? r => r.FechaPenultimaCarpeta
            : r => r.FechaUltimaCarpeta;

        return TipoListado switch
        {
            "Oficina43"    => registros.Where(r => fechaCampo(r).HasValue && fechaCampo(r)!.Value >= new DateTime(2023, 7, 1)).ToList(),
            "Archivos"     => registros.Where(r => !fechaCampo(r).HasValue || fechaCampo(r)!.Value < new DateTime(2023, 7, 1)).ToList(),
            "NoSolicitadas"=> registros.Where(r => string.IsNullOrEmpty(r.CarpetaPedida) || r.CarpetaPedida != "SI").ToList(),
            _              => registros
        };
    }

    private async Task DescargarArchivoPdf(byte[] bytes, string nombre)
    {
        var b64 = Convert.ToBase64String(bytes);
        await JS.InvokeVoidAsync("eval",
            $"var a=document.createElement('a');a.href='data:application/pdf;base64,{b64}';" +
            $"a.download='{nombre}';a.click()");
        MostrarPdf = false;
        Mensaje = $"PDF generado: {nombre}";
        EsError = false;
    }

    private async Task AbrirDominios()
    {
        Dominios = (await JS.InvokeAsync<string[]>("taEmail.getDomains")).ToList();
        MostrarDominios = true;
    }

    private async Task AgregarDominio()
    {
        if (string.IsNullOrWhiteSpace(NuevoDominio)) return;
        Dominios = (await JS.InvokeAsync<string[]>("taEmail.addDomain", NuevoDominio)).ToList();
        NuevoDominio = "";
    }

    private async Task QuitarDominio(string dominio)
    {
        Dominios = (await JS.InvokeAsync<string[]>("taEmail.removeDomain", dominio)).ToList();
    }

    private void AbrirPegado()
    {
        TextoPegado = "";
        NombreArchivoExcel = null;
        RegistrosImportados = 0;
        LugarAtencionImport = "";
        MostrarPegado = true;
    }

    private void CerrarPegado() => MostrarPegado = false;

    private async Task ExportarPlantilla()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<table><tr>" +
            "<th>NOMBRE</th><th>APELLIDO</th><th>NOMBRE COMPLETO</th><th>RUT</th>" +
            "<th>FECHA DE LA CITACIÓN</th><th>TELÉFONO</th><th>CORREO ELECTRÓNICO</th>" +
            "<th>LUGAR DE ATENCIÓN (manual)</th><th>ASISTE (manual)</th><th>FECHA ÚLTIMA CARPETA</th>" +
            "<th>ESTADO DE LA CARPETA</th><th>CIUDADES</th><th>FECHA DIGITALIZACIÓN CARPETA</th>" +
            "<th>FECHA SUBIDA A CONASET</th><th>CARPETA PEDIDA</th>" +
            "<th>IDONEIDAD MORAL</th><th>CONTACTADO</th><th>NOTIFICADO</th>" +
            "<th>LUGAR DE ORIGEN CAMBIO DOMICILIO</th><th>PETICIÓN DE CAMBIO DOMICILIO</th>" +
            "<th>CAMBIO DOMICILIO PEDIDO POR</th><th>CAMBIO DE DOMICILIO PEDIDO</th>" +
            "<th>FOLIO (F8)</th><th>FECHA PENÚLTIMA CARPETA</th>" +
            "<th>ESTADO DEL F8</th><th>OTORGAMIENTO</th><th>FOLIO LICENCIA</th>" +
            "<th>FECHA IMPRESIÓN</th><th>IMPRESA POR</th><th>BAJADA A PAM</th><th>BAJADA POR</th>" +
            "<th>FECHA RECEPCIÓN PAM</th><th>RECIBIDA EN PAM POR</th>" +
            "<th>CONTACTADO CONTRIBUYENTE PARA ENTREGA</th><th>FECHA DE ENTREGA</th>" +
            "<th>SEXO</th><th>FECHA DE NACIMIENTO</th><th>EDAD</th><th>TIPO DE LICENCIA (manual)</th></tr>");
        sb.AppendLine("</table>");
        var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
        await JS.InvokeVoidAsync("eval",
            $"var a=document.createElement('a');a.href='data:application/vnd.ms-excel;base64,{b64}';" +
            "a.download='plantilla_licencias.xls';a.click()");
    }

    private async Task OnFileSelected(ChangeEventArgs e)
    {
        try
        {
            var fileInfo = await JS.InvokeAsync<string[]>("taReadExcel", "excel-file-input");
            if (fileInfo is null || fileInfo.Length < 2 || string.IsNullOrEmpty(fileInfo[1])) return;

            NombreArchivoExcel = fileInfo[0];
            StateHasChanged();

            var bytes = Convert.FromBase64String(fileInfo[1]);
            using var ms = new MemoryStream(bytes);
            using var workbook = new XLWorkbook(ms);
            await ProcesarLibroExcel(workbook);
        }
        catch (Exception ex)
        {
            EsError = true;
            Mensaje = $"Error al leer el archivo: {ex.Message}";
        }
    }

    private static readonly Dictionary<string, string[]> MapaAlias = new()
    {
        { "Nombre", new[] { "NOMBRE", "NOMBRES", "FIRSTNAME" } },
        { "Apellido", new[] { "APELLIDO", "APELLIDOS", "LASTNAME" } },
        { "NombreCompleto", new[] { "NOMBRE COMPLETO", "NOMBRES COMPLETOS", "NAME", "FULLNAME" } },
        { "RUT", new[] { "RUT", "RUN", "IDENTIFICACION", "DOC" } },
        { "FechaCitacion", new[] { "FECHA CITACION", "FECHA DE LA CITACION", "CITACION", "FECHA DE LA CITACIÓN", "FECHA DE LA CITACIÓN (manual)", "FECHA CITACIÓN" } },
        { "Telefono", new[] { "TELEFONO", "TELÉFONO", "CELULAR", "CONTACTO" } },
        { "Email", new[] { "CORREO", "EMAIL", "CORREO ELECTRONICO", "CORREO ELECTRÓNICO" } },
        { "LugarAtencion", new[] { "LUGAR ATENCION", "LUGAR DE ATENCION", "OFICINA", "SUCURSAL", "LUGAR DE ATENCIÓN", "LUGAR DE ATENCIÓN (manual)" } },
        { "Asiste", new[] { "ASISTE", "ASISTENCIA", "CONCURRE", "ASISTE (manual)" } },
        { "FechaUltimaCarpeta", new[] { "FECHA ULTIMA CARPETA", "FECHA ÚLTIMA CARPETA", "ULTIMA CARPETA" } },
        { "EstadoCarpeta", new[] { "ESTADO CARPETA", "ESTADO DE LA CARPETA", "ESTADO" } },
        { "CiudadCambioDomicilio", new[] { "CIUDADES", "CIUDAD", "COMUNA", "DESTINO" } },
        { "FechaDigitalizacionCarpeta", new[] { "FECHA DIGITALIZACION CARPETA", "FECHA DIGITALIZACIÓN CARPETA", "DIGITALIZACION" } },
        { "FechaSubidaConaset", new[] { "FECHA SUBIDA CONASET", "FECHA SUBIDA A CONASET", "CONASET", "FECHA CONASET" } },
        { "CarpetaPedida", new[] { "CARPETA PEDIDA", "SOLICITA CARPETA" } },
        { "IdoneidadMoral", new[] { "IDONEIDAD MORAL", "IDONEIDAD" } },
        { "Contactado", new[] { "CONTACTADO", "CONTACTO ESTABLECIDO" } },
        { "Notificado", new[] { "NOTIFICADO", "NOTIFICACION" } },
        { "LugarOrigenCambioDomicilio", new[] { "LUGAR ORIGEN CAMBIO DOMICILIO", "LUGAR DE ORIGEN CAMBIO DOMICILIO", "ORIGEN DOMICILIO" } },
        { "PeticionCambioDomicilio", new[] { "PETICION CAMBIO DOMICILIO", "PETICIÓN DE CAMBIO DOMICILIO", "CAMBIO DOMICILIO" } },
        { "CambioDomicilioPedidoPor", new[] { "CAMBIO DOMICILIO PEDIDO POR", "CAMBIO DE DOMICILIO PEDIDO POR", "SOLICITANTE CAMBIO" } },
        { "CambioDomicilioPedido", new[] { "CAMBIO DOMICILIO PEDIDO", "CAMBIO DE DOMICILIO PEDIDO", "PEDIDO CAMBIO" } },
        { "FolioF8", new[] { "FOLIO F8", "F8", "FOLIO", "FOLIO (F8)" } },
        { "FechaPenultimaCarpeta", new[] { "FECHA PENULTIMA CARPETA", "FECHA PENÚLTIMA CARPETA" } },
        { "EstadoF8", new[] { "ESTADO F8", "ESTADO DEL F8" } },
        { "Otorgamiento", new[] { "OTORGAMIENTO", "ESTADO OTORGAMIENTO" } },
        { "FolioLicencia", new[] { "FOLIO LICENCIA", "NUMERO LICENCIA" } },
        { "FechaImpresion", new[] { "FECHA IMPRESION", "FECHA IMPRESIÓN" } },
        { "ImpresaPor", new[] { "IMPRESA POR", "IMPRESO POR" } },
        { "BajadaAPam", new[] { "BAJADA A PAM", "PAM" } },
        { "BajadaPor", new[] { "BAJADA POR", "PAM POR" } },
        { "FechaRecepcionPam", new[] { "FECHA RECEPCION PAM", "FECHA RECEPCIÓN PAM" } },
        { "RecibidaEnPamPor", new[] { "RECIBIDA EN PAM POR" } },
        { "ContactadoParaEntrega", new[] { "CONTACTADO CONTRIBUYENTE PARA ENTREGA", "CONTACTADO PARA ENTREGA" } },
        { "FechaEntrega", new[] { "FECHA ENTREGA", "FECHA DE ENTREGA" } },
        { "Sexo", new[] { "SEXO", "GENERO" } },
        { "FechaNacimiento", new[] { "FECHA NACIMIENTO", "FECHA DE NACIMIENTO" } },
        { "TipoLicencia", new[] { "TIPO LICENCIA", "TIPO DE LICENCIA", "CLASE", "TIPO DE LICENCIA (manual)" } }
    };

    private static string NormalizarCabecera(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        var temp = texto.ToUpperInvariant().Trim();
        temp = temp.Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");
        temp = System.Text.RegularExpressions.Regex.Replace(temp, @"[^A-Z0-9\s]", "");
        temp = System.Text.RegularExpressions.Regex.Replace(temp, @"\s+", " ");
        return temp;
    }

    private static (string Nombre, string? Apellido) ProcesarNombreCompleto(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return ("", null);
        var partes = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 1) return (partes[0], null);
        if (partes.Length == 2) return (partes[0], partes[1]);
        if (partes.Length == 3) return (partes[0] + " " + partes[1], partes[2]);
        var nombre = string.Join(" ", partes.Take(partes.Length - 2));
        var apellido = string.Join(" ", partes.Skip(partes.Length - 2));
        return (nombre, apellido);
    }

    private static Dictionary<string, int> ConstruirMapaColumnas(string[] celdasCabecera)
    {
        var mapa = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < celdasCabecera.Length; i++)
        {
            var cabeceraNorm = NormalizarCabecera(celdasCabecera[i]);
            if (string.IsNullOrEmpty(cabeceraNorm)) continue;

            foreach (var kv in MapaAlias)
            {
                if (kv.Value.Any(alias => NormalizarCabecera(alias) == cabeceraNorm))
                {
                    if (!mapa.ContainsKey(kv.Key))
                        mapa[kv.Key] = i;
                    break;
                }
            }
        }
        return mapa;
    }

    private async Task ProcesarLibroExcel(XLWorkbook workbook)
    {
        var ws = workbook.Worksheet(1);
        var filas = new List<string[]>();
        string[]? cabecera = null;

        foreach (var row in ws.RowsUsed())
        {
            var celdas = new List<string>();
            var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 39;
            for (int i = 1; i <= lastCol; i++)
            {
                var cell = row.Cell(i);
                var val = cell.DataType == XLDataType.DateTime
                    ? cell.GetDateTime().ToString("yyyy-MM-dd")
                    : cell.DataType == XLDataType.Number
                        ? cell.GetValue<double>().ToString(CultureInfo.InvariantCulture)
                        : cell.GetString().Trim();
                celdas.Add(val);
            }

            if (celdas.All(string.IsNullOrWhiteSpace)) continue;

            if (cabecera is null)
            {
                cabecera = celdas.ToArray();
                continue;
            }

            filas.Add(celdas.ToArray());
        }

        if (cabecera is not null)
            await ImportarRegistros(filas, cabecera);
        else
        {
            EsError = true;
            Mensaje = "No se encontraron filas con datos en el archivo Excel.";
        }
    }

    private async Task SeleccionarArchivoExcel()
    {
        await JS.InvokeVoidAsync("eval", "document.getElementById('excel-file-input').click()");
    }

    private async Task ProcesarPegado()
    {
        var lineas = (TextoPegado ?? "").Split('\n');
        var filas = new List<string[]>();
        string[]? cabecera = null;

        foreach (var linea in lineas)
        {
            var c = linea.TrimEnd('\r').Split('\t');
            if (c.All(string.IsNullOrWhiteSpace)) continue;

            bool esCabecera = c.Any(val => {
                var valNorm = NormalizarCabecera(val);
                return MapaAlias.Values.Any(aliasList => aliasList.Any(a => NormalizarCabecera(a) == valNorm));
            });

            if (esCabecera && cabecera is null)
            {
                cabecera = c;
                continue;
            }

            if (cabecera is not null && c.SequenceEqual(cabecera))
                continue;

            filas.Add(c);
        }

        if (cabecera is null)
        {
            cabecera = new[] {
                "NOMBRE", "APELLIDO", "NOMBRE COMPLETO", "RUT", "FECHA CITACION", "TELEFONO", "EMAIL", "LUGAR ATENCION",
                "ASISTE", "FECHA ULTIMA CARPETA", "ESTADO CARPETA", "CIUDAD", "FECHA DIGITALIZACION", "FECHA SUBIDA CONASET",
                "CARPETA PEDIDA", "IDONEIDAD MORAL", "CONTACTADO", "NOTIFICADO", "LUGAR ORIGEN CAMBIO DOMICILIO",
                "PETICION CAMBIO DOMICILIO", "CAMBIO DOMICILIO PEDIDO POR", "CAMBIO DOMICILIO PEDIDO", "FOLIO F8",
                "FECHA PENULTIMA CARPETA", "ESTADO F8", "OTORGAMIENTO", "FOLIO LICENCIA", "FECHA IMPRESION", "IMPRESA POR",
                "BAJADA A PAM", "BAJADA POR", "FECHA RECEPCION PAM", "RECIBIDA EN PAM POR", "CONTACTADO CONTRIBUYENTE PARA ENTREGA",
                "FECHA ENTREGA", "SEXO", "FECHA NACIMIENTO", "EDAD", "TIPO LICENCIA"
            };
        }

        await ImportarRegistros(filas, cabecera);
    }

    private async Task ImportarRegistros(List<string[]> filas, string[] cabecera)
    {
        var creados = 0;
        var errores = new List<string>();
        var nro = 0;

        var mapa = ConstruirMapaColumnas(cabecera);

        string ObtenerValor(string[] fila, string propiedad)
        {
            if (mapa.TryGetValue(propiedad, out var index) && index < fila.Length)
                return fila[index]?.Trim() ?? "";
            return "";
        }

        string? ObtenerValorNulo(string[] fila, string propiedad)
        {
            var v = ObtenerValor(fila, propiedad);
            return string.IsNullOrEmpty(v) ? null : v;
        }

        foreach (var c in filas)
        {
            nro++;

            string nombre = "";
            string? apellido = null;

            if (mapa.ContainsKey("NombreCompleto"))
            {
                var nomCompleto = ObtenerValor(c, "NombreCompleto");
                if (!string.IsNullOrWhiteSpace(nomCompleto))
                {
                    var res = ProcesarNombreCompleto(nomCompleto);
                    nombre = res.Nombre;
                    apellido = res.Apellido;
                }
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                nombre = ObtenerValor(c, "Nombre");
                apellido = ObtenerValorNulo(c, "Apellido");

                if (!string.IsNullOrWhiteSpace(nombre) && string.IsNullOrEmpty(apellido))
                {
                    var res = ProcesarNombreCompleto(nombre);
                    nombre = res.Nombre;
                    apellido = res.Apellido;
                }
            }

            if (string.IsNullOrWhiteSpace(nombre) && string.IsNullOrWhiteSpace(ObtenerValor(c, "RUT")))
                continue;

            var t = new TramiteLicencia
            {
                Nombre = nombre,
                Apellido = apellido,
                RUT = BaseTramite.FormatearRut(ObtenerValor(c, "RUT")),
                FechaCitacion = Fecha(ObtenerValor(c, "FechaCitacion")),
                Telefono = NormalizarTelefono(ObtenerValor(c, "Telefono")),
                Email = ObtenerValor(c, "Email"),
                LugarAtencion = ObtenerValorNulo(c, "LugarAtencion") ?? (string.IsNullOrWhiteSpace(LugarAtencionImport) ? null : LugarAtencionImport),
                Asiste = ObtenerValorNulo(c, "Asiste"),
                FechaUltimaCarpeta = Fecha(ObtenerValor(c, "FechaUltimaCarpeta")),
                EstadoCarpeta = ObtenerValorNulo(c, "EstadoCarpeta") ?? CalcularEstadoCarpeta(Fecha(ObtenerValor(c, "FechaUltimaCarpeta"))),
                CiudadCambioDomicilio = ObtenerValorNulo(c, "CiudadCambioDomicilio"),
                FechaDigitalizacionCarpeta = Fecha(ObtenerValor(c, "FechaDigitalizacionCarpeta")),
                FechaSubidaConaset = Fecha(ObtenerValor(c, "FechaSubidaConaset")),
                CarpetaPedida = ObtenerValorNulo(c, "CarpetaPedida"),
                IdoneidadMoral = ObtenerValorNulo(c, "IdoneidadMoral"),
                Contactado = ObtenerValorNulo(c, "Contactado"),
                Notificado = ObtenerValorNulo(c, "Notificado"),
                LugarOrigenCambioDomicilio = ObtenerValorNulo(c, "LugarOrigenCambioDomicilio"),
                PeticionCambioDomicilio = ObtenerValorNulo(c, "PeticionCambioDomicilio"),
                CambioDomicilioPedidoPor = ObtenerValorNulo(c, "CambioDomicilioPedidoPor"),
                CambioDomicilioPedido = ObtenerValorNulo(c, "CambioDomicilioPedido"),
                FolioF8 = ObtenerValorNulo(c, "FolioF8"),
                FechaPenultimaCarpeta = Fecha(ObtenerValor(c, "FechaPenultimaCarpeta")),
                EstadoF8 = ObtenerValorNulo(c, "EstadoF8"),
                Otorgamiento = ObtenerValorNulo(c, "Otorgamiento"),
                FolioLicencia = ObtenerValorNulo(c, "FolioLicencia"),
                FechaImpresion = Fecha(ObtenerValor(c, "FechaImpresion")),
                ImpresaPor = ObtenerValorNulo(c, "ImpresaPor"),
                BajadaAPam = ObtenerValorNulo(c, "BajadaAPam"),
                BajadaPor = ObtenerValorNulo(c, "BajadaPor"),
                FechaRecepcionPam = Fecha(ObtenerValor(c, "FechaRecepcionPam")),
                RecibidaEnPamPor = ObtenerValorNulo(c, "RecibidaEnPamPor"),
                ContactadoParaEntrega = ObtenerValorNulo(c, "ContactadoParaEntrega"),
                FechaEntrega = Fecha(ObtenerValor(c, "FechaEntrega")),
                Sexo = ObtenerValorNulo(c, "Sexo"),
                FechaNacimiento = Fecha(ObtenerValor(c, "FechaNacimiento")),
                TipoLicencia = ObtenerValorNulo(c, "TipoLicencia"),
                UsuarioId = UsuarioActual,
                TipoModulo = "TramiteLicencia"
            };

            var (ok, msg) = ModuloService.ValidarTramite(t);
            if (!ok)
            {
                errores.Add($"Línea {nro} (RUT: {t.RUT}): {msg}");
                continue;
            }

            await ModuloService.CrearAsync(t);
            creados++;
        }

        RegistrosImportados = creados;
        Mensaje = $"{creados} registro(s) creados."
            + (errores.Count > 0 ? $" Errores: {string.Join(" | ", errores)}" : "");
        EsError = errores.Count > 0;
        await BuscarAsync();
    }

    private static string Col(string[] a, int i) => i < a.Length ? a[i].Trim() : "";

    private static string? ColN(string[] a, int i)
    {
        var v = Col(a, i);
        return string.IsNullOrEmpty(v) ? null : v;
    }

    private static string? CalcularEstadoCarpeta(DateTime? fecha)
    {
        return fecha.HasValue && fecha.Value >= new DateTime(2023, 7, 1)
            ? "SE ENCUENTRA EN OF.43"
            : "SE ENCUENTRA EN ARCHIVOS";
    }

    private static DateTime? Fecha(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        if (DateTime.TryParseExact(valor, FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var f))
            return f;
        // ClosedXML returns date cells typed as Number (Excel OA serial, e.g. "46196")
        if (double.TryParse(valor, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var oa)
            && oa > 10000 && oa < 100000)
            return DateTime.FromOADate(oa);
        return null;
    }

    private async Task ExportarXls()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<table><tr>" +
            "<th>NOMBRE</th><th>APELLIDO</th><th>NOMBRE COMPLETO</th><th>RUT</th>" +
            "<th>FECHA DE LA CITACIÓN</th><th>TELÉFONO</th><th>CORREO ELECTRÓNICO</th>" +
            "<th>LUGAR DE ATENCIÓN</th><th>ASISTE</th><th>FECHA ÚLTIMA CARPETA</th>" +
            "<th>ESTADO DE LA CARPETA</th><th>CIUDADES</th><th>FECHA DIGITALIZACIÓN CARPETA</th>" +
            "<th>FECHA SUBIDA A CONASET</th><th>CARPETA PEDIDA</th>" +
            "<th>IDONEIDAD MORAL</th><th>CONTACTADO</th><th>NOTIFICADO</th>" +
            "<th>LUGAR DE ORIGEN CAMBIO DOMICILIO</th><th>PETICIÓN DE CAMBIO DOMICILIO</th>" +
            "<th>CAMBIO DOMICILIO PEDIDO POR</th><th>CAMBIO DE DOMICILIO PEDIDO</th>" +
            "<th>FOLIO (F8)</th><th>FECHA PENÚLTIMA CARPETA</th>" +
            "<th>ESTADO DEL F8</th><th>OTORGAMIENTO</th><th>FOLIO LICENCIA</th>" +
            "<th>FECHA IMPRESIÓN</th><th>IMPRESA POR</th><th>BAJADA A PAM</th><th>BAJADA POR</th>" +
            "<th>FECHA RECEPCIÓN PAM</th><th>RECIBIDA EN PAM POR</th>" +
            "<th>CONTACTADO CONTRIBUYENTE PARA ENTREGA</th><th>FECHA DE ENTREGA</th>" +
            "<th>SEXO</th><th>FECHA DE NACIMIENTO</th><th>EDAD</th><th>TIPO DE LICENCIA</th></tr>");
        foreach (var r in Registros)
        {
            sb.AppendLine("<tr>" +
                $"<td>{r.Nombre}</td><td>{r.Apellido}</td><td>{r.NombreCompleto}</td><td>{r.RUT}</td>" +
                $"<td>{F(r.FechaCitacion)}</td><td>{r.Telefono}</td><td>{r.Email}</td>" +
                $"<td>{r.LugarAtencion}</td><td>{r.Asiste}</td><td>{F(r.FechaUltimaCarpeta)}</td>" +
                $"<td>{r.EstadoCarpeta}</td><td>{r.CiudadCambioDomicilio}</td><td>{F(r.FechaDigitalizacionCarpeta)}</td>" +
                $"<td>{F(r.FechaSubidaConaset)}</td><td>{r.CarpetaPedida}</td>" +
                $"<td>{r.IdoneidadMoral}</td><td>{r.Contactado}</td><td>{r.Notificado}</td>" +
                $"<td>{r.LugarOrigenCambioDomicilio}</td><td>{r.PeticionCambioDomicilio}</td>" +
                $"<td>{r.CambioDomicilioPedidoPor}</td><td>{r.CambioDomicilioPedido}</td>" +
                $"<td>{r.FolioF8}</td><td>{F(r.FechaPenultimaCarpeta)}</td>" +
                $"<td>{r.EstadoF8}</td><td>{r.Otorgamiento}</td><td>{r.FolioLicencia}</td>" +
                $"<td>{F(r.FechaImpresion)}</td><td>{r.ImpresaPor}</td><td>{r.BajadaAPam}</td><td>{r.BajadaPor}</td>" +
                $"<td>{F(r.FechaRecepcionPam)}</td><td>{r.RecibidaEnPamPor}</td>" +
                $"<td>{r.ContactadoParaEntrega}</td><td>{F(r.FechaEntrega)}</td>" +
                $"<td>{r.Sexo}</td><td>{F(r.FechaNacimiento)}</td><td>{r.Edad}</td><td>{r.TipoLicencia}</td></tr>");
        }
        sb.AppendLine("</table>");
        var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
        await JS.InvokeVoidAsync("eval",
            $"var a=document.createElement('a');a.href='data:application/vnd.ms-excel;base64,{b64}';" +
            "a.download='gestionador_licencias_export.xls';a.click()");
    }
}
