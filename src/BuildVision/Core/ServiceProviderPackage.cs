using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BuildVision.Exports.Providers;
using BuildVision.Views.Settings;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace BuildVision.Core
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.GuidBuildVisionServiceProvider)]
    [ProvideService(typeof(IPackageSettingsProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(IBuildInformationProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(ISolutionProvider), IsAsyncQueryable = true)]
    [ProvideService(typeof(IBuildingProjectsProvider), IsAsyncQueryable = true)]
    public sealed class ServiceProviderPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await Task.CompletedTask;
            AddService(typeof(IPackageSettingsProvider), CreateServiceAsync, true);
            AddService(typeof(IBuildInformationProvider), CreateServiceAsync, true);
            AddService(typeof(ISolutionProvider), CreateServiceAsync, true);
            AddService(typeof(IBuildingProjectsProvider), CreateServiceAsync, true);
        }

        async Task<object> CreateServiceAsync(IAsyncServiceContainer container, CancellationToken cancellation, Type serviceType)
        {            
            if (serviceType == typeof(IPackageSettingsProvider))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                var sp = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                Assumes.Present(sp);
                return new PackageSettingsProvider(sp);
            }
            else if (serviceType == typeof(IBuildInformationProvider))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                var sp = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                Assumes.Present(sp);
                return new BuildInformationProvider(sp);
            }
            else if (serviceType == typeof(ISolutionProvider))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                var sp = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                Assumes.Present(sp);
                return new SolutionProvider(sp);
            }
            else if (serviceType == typeof(IBuildingProjectsProvider))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                var sp = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                Assumes.Present(sp);
                var solutionProvider = sp.GetService(typeof(ISolutionProvider)) as ISolutionProvider;
                Assumes.Present(solutionProvider);
                var buildInformationProvider = sp.GetService(typeof(IBuildInformationProvider)) as IBuildInformationProvider;
                Assumes.Present(buildInformationProvider);
                return new BuildingProjectsProvider(solutionProvider, buildInformationProvider);
            }         
            else
            {
                throw new Exception("Not found");
            }
        }
    }
}
