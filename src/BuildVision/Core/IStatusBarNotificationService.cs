namespace BuildVision.Core
{
    public interface IStatusBarNotificationService
    {
        void ShowText(string str);
        void ShowTextWithFreeze(string str);
    }
}