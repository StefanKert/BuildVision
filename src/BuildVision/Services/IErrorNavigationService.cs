using BuildVision.Contracts;

namespace BuildVision.Services
{
    public interface IErrorNavigationService
    {
        void NavigateToErrorItem(ErrorItem errorItem);
    }  
}
