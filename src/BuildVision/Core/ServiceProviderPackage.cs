using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BuildVision.Exports;
using BuildVision.Exports.Factories;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Tool.Building;
using BuildVision.UI.Helpers;
using BuildVision.UI.Settings.Models;
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
    [ProvideService(typeof(IBuildMessagesFactory), IsAsyncQueryable = true)]
    [ProvideService(typeof(IBuildOutputLogger), IsAsyncQueryable = true)]
    [ProvideService(typeof(IBuildService), IsAsyncQueryable = true)]
    [ProvideService(typeof(IStatusBarNotificationService), IsAsyncQueryable = true)]
    public sealed class ServiceProviderPackage : AsyncPackage
    {
        private readonly Guid _parsingErrorsLoggerId = new Guid("{64822131-DC4D-4087-B292-61F7E06A7B39}");

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await Task.CompletedTask;
            AddService(typeof(IPackageSettingsProvider), CreateServiceAsync, true);
            AddService(typeof(IBuildInformationProvider), CreateServiceAsync, true);
            AddService(typeof(ISolutionProvider), CreateServiceAsync, true);
            AddService(typeof(IBuildingProjectsProvider), CreateServiceAsync, true);
            AddService(typeof(IBuildMessagesFactory), CreateServiceAsync, true);
            AddService(typeof(IBuildOutputLogger), CreateServiceAsync, true);
            AddService(typeof(IBuildService), CreateServiceAsync, true);
            AddService(typeof(IStatusBarNotificationService), CreateServiceAsync, true);    
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
                var buildOutputLogger = sp.GetService(typeof(IBuildOutputLogger)) as IBuildOutputLogger;
                Assumes.Present(buildOutputLogger);
                var statusBarNotificationService = sp.GetService(typeof(IStatusBarNotificationService)) as IStatusBarNotificationService;
                Assumes.Present(statusBarNotificationService);
                var buildMessagesFactory = sp.GetService(typeof(IBuildMessagesFactory)) as IBuildMessagesFactory;
                Assumes.Present(buildMessagesFactory);
                return new BuildInformationProvider(sp, buildOutputLogger, statusBarNotificationService, buildMessagesFactory);
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
                var buildOutputLogger = sp.GetService(typeof(IBuildOutputLogger)) as IBuildOutputLogger;
                Assumes.Present(buildOutputLogger);
                return new BuildingProjectsProvider(solutionProvider, buildInformationProvider, buildOutputLogger);
            }
            else if (serviceType == typeof(IBuildMessagesFactory))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                return new BuildMessagesFactory(new ControlSettings().BuildMessagesSettings);
            }
            else if (serviceType == typeof(IBuildOutputLogger))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                return new BuildOutputLogger(_parsingErrorsLoggerId, Microsoft.Build.Framework.LoggerVerbosity.Quiet);
            }
            else if (serviceType == typeof(IBuildService))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                return new BuildManager();
            }
            else if (serviceType == typeof(IStatusBarNotificationService))
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
                var sp = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                Assumes.Present(sp);
                return new StatusBarNotificationService(sp);
            }
            else
            {
                throw new Exception("Not found");
            }
        }
    }
}
