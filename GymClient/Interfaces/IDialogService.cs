namespace GymClient.Interfaces
{
    public interface IDialogService
    {
        Task ShowMessage(string message);
        Task<bool> ShowConfirmation(string message);
        Task ShowMessageWithColoredEmail(string message, string email, System.Windows.Media.Brush? emailColor);
    }
}
