using BuildVision.Contracts;

namespace BuildVision.Services
{
    public interface IProjectFileNavigationService : IErrorNavigationService
    {
        void NavigateToFirstError();
    }
}
