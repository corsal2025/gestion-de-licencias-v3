namespace GestionLicencias.Web.Services;

/// <summary>
/// Singleton bus that notifies all connected Seguimiento circuits
/// when a record is saved from LicenciasList.
/// </summary>
public sealed class TramitesBroadcast
{
    private readonly List<Func<Task>> _handlers = [];

    public void Suscribir(Func<Task> handler)
    {
        lock (_handlers) _handlers.Add(handler);
    }

    public void Desuscribir(Func<Task> handler)
    {
        lock (_handlers) _handlers.Remove(handler);
    }

    public async Task NotificarAsync()
    {
        List<Func<Task>> copia;
        lock (_handlers) copia = [.._handlers];
        foreach (var h in copia)
        {
            try { await h(); }
            catch { /* circuit may have disconnected */ }
        }
    }
}
