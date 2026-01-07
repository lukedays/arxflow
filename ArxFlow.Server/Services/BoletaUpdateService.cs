namespace ArxFlow.Server.Services;

// Servico singleton para notificar componentes sobre mudancas em boletas
public class BoletaUpdateService
{
    public event Action? OnBoletasChanged;

    public void NotifyBoletasChanged()
    {
        OnBoletasChanged?.Invoke();
    }
}
