namespace SmartTicket.API.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(int userId, string message);
    }
}
