using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BuildVision.Contracts.Models;
using BuildVision.Exports;
using BuildVision.Exports.Factories;
using BuildVision.Exports.Providers;
using BuildVision.Exports.Services;
using BuildVision.Services;
using BuildVision.Tool;
using BuildVision.Tool.Building;
using BuildVision.UI.Helpers;
using BuildVision.UI.Settings.Models;
using BuildVision.UI.ViewModels;
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
    [ProvideService(typeof(IWindowStateService), IsAsyncQueryable = true)]
    [ProvideService(typeof(IBuildProgressViewModel), IsAsyncQueryable = true)]
    [ProvideService(typeof(IErrorNavigationService), IsAsyncQueryable = true)]
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
            AddService(typeof(IWindowStateService), CreateServiceAsync, true);
            AddService(typeof(IBuildProgressViewModel), CreateServiceAsync, true);
            AddService(typeof(IErrorNavigationService), CreateServiceAsync, true);

        }

        async Task<object> CreateServiceAsync(IAsyncServiceContainer container, CancellationToken cancellation, Type serviceType)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
            var sp = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            Assumes.Present(sp);
            if (serviceType == typeof(IPackageSettingsProvider))
            {
                return new PackageSettingsProvider(sp);
            }
            else if (serviceType == typeof(IWindowStateService))
            {
                return new WindowStateService(sp, this);
            }
            else if (serviceType == typeof(ISolutionProvider))
            {
                return new SolutionProvider(sp);
            }
            else if (serviceType == typeof(IBuildOutputLogger))
            {
                return new BuildOutputLogger(_parsingErrorsLoggerId, Microsoft.Build.Framework.LoggerVerbosity.Quiet);
            }
            else if (serviceType == typeof(IErrorNavigationService))
            {
                return new ErrorNavigationService(sp);
            }
            else if (serviceType == typeof(IBuildService))
            {
                return new BuildService(sp);
            }
            else if (serviceType == typeof(IStatusBarNotificationService))
            {
                return new StatusBarNotificationService(sp);
            }
            else if (serviceType == typeof(IBuildMessagesFactory))
            {
                var packageSettingsProvider = await GetServiceAsync<IPackageSettingsProvider>(cancellation);
                return new BuildMessagesFactory(packageSettingsProvider);
            }
            else if (serviceType == typeof(IBuildProgressViewModel))
            {
                var packageSettingsProvider = await GetServiceAsync<IPackageSettingsProvider>(cancellation);
                return new BuildProgressViewModel(packageSettingsProvider);
            }
            else if (serviceType == typeof(IBuildInformationProvider))
            {
                var solutionProvider = await GetServiceAsync<ISolutionProvider>(cancellation);
                var buildMessagesFactory = await GetServiceAsync<IBuildMessagesFactory>(cancellation);
                var buildOutputLogger = await GetServiceAsync<IBuildOutputLogger>(cancellation);
                var packageSettingsProvider = await GetServiceAsync<IPackageSettingsProvider>(cancellation);
                var statusBarNotificationService = await GetServiceAsync<IStatusBarNotificationService>(cancellation);
                var windowStateService = await GetServiceAsync<IWindowStateService>(cancellation);
                var buildProgressViewModel = await GetServiceAsync<IBuildProgressViewModel>(cancellation);
                var buildService = await GetServiceAsync<IBuildService>(cancellation);
                var errorNavigationService = await GetServiceAsync<IErrorNavigationService>(cancellation);
                return new BuildInformationProvider(sp, buildOutputLogger, statusBarNotificationService, buildMessagesFactory, windowStateService, packageSettingsProvider, buildProgressViewModel, errorNavigationService);
            }
            else if (serviceType == typeof(IBuildingProjectsProvider))
            {
                var solutionProvider = await GetServiceAsync<ISolutionProvider>(cancellation);
                var buildInformationProvider = await GetServiceAsync<IBuildInformationProvider>(cancellation);
                var buildOutputLogger = await GetServiceAsync<IBuildOutputLogger>(cancellation);
                var packageSettingsProvider = await GetServiceAsync<IPackageSettingsProvider>(cancellation);
                var buildProgressViewModel = await GetServiceAsync<IBuildProgressViewModel>(cancellation); 
                var buildService = await GetServiceAsync<IBuildService>(cancellation);
                var errorNavigationService = await GetServiceAsync<IErrorNavigationService>(cancellation);
                return new BuildingProjectsProvider(solutionProvider, buildInformationProvider, buildOutputLogger, buildProgressViewModel, packageSettingsProvider, buildService, errorNavigationService);
            }
            else
            {
                throw new Exception("Not found");
            }
        }

        private async Task<T> GetServiceAsync<T>(CancellationToken cancellation) where T: class
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellation);
            var service = await GetServiceAsync(typeof(T)) as T;
            Assumes.Present(service);
            return service;
        }
    }
}
