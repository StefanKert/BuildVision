using BuildVision.Contracts;

namespace BuildVision.Exports.Services
{
    public interface IErrorNavigationService
    {
        void NavigateToErrorItem(ErrorItem errorItem);
    }  
}
