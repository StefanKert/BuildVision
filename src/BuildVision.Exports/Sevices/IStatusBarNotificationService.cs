namespace BuildVision.Exports.Services
{
    public interface IStatusBarNotificationService
    {
        void ShowText(string str);
        void ShowTextWithFreeze(string str);
    }
}
