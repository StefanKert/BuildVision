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
    [ProvideService(typeof(IProjectFileNavigationService), IsAsyncQueryable = true)]
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
            AddService(typeof(IProjectFileNavigationService), CreateServiceAsync, true);
            AddService(typeof(IErrorNavigationService), CreateServiceAsync, true);

        }

        async Task<object> CreateServiceAsync(IAsyncServiceContainer container, CancellationToken cancellation, Type serviceType)
        {
            if (serviceType == typeof(IPackageSettingsProvider))
            {
                return new PackageSettingsProvider(GetServiceProvider());
            }
            else if (serviceType == typeof(IWindowStateService))
            {
                return new WindowStateService(GetServiceProvider());
            }
            else if (serviceType == typeof(IBuildInformationProvider))
            {
                var sp = GetServiceProvider();
                var solutionProvider = GetService<ISolutionProvider>();
                var buildMessagesFactory = GetService<IBuildMessagesFactory>();
                var buildOutputLogger = GetService<IBuildOutputLogger>();
                var packageSettingsProvider = GetService<IPackageSettingsProvider>();
                var statusBarNotificationService = GetService<IStatusBarNotificationService>();
                var windowStateService = GetService<IWindowStateService>();
                var buildProgressViewModel = GetService<IBuildProgressViewModel>();
                var buildService = GetService<IBuildService>();
                var projectFileNavigationService = GetService<IProjectFileNavigationService>();
                return new BuildInformationProvider(sp, buildOutputLogger, statusBarNotificationService, buildMessagesFactory, windowStateService, packageSettingsProvider, buildProgressViewModel, projectFileNavigationService);
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
                var solutionProvider = GetService<ISolutionProvider>();
                var buildInformationProvider = GetService<IBuildInformationProvider>();
                var buildOutputLogger = GetService<IBuildOutputLogger>();
                var packageSettingsProvider = GetService<IPackageSettingsProvider>();
                var buildProgressViewModel = GetService<IBuildProgressViewModel>(); 
                var buildService = GetService<IBuildService>();
                var errorNavigationService = GetService<IErrorNavigationService>();
                return new BuildingProjectsProvider(solutionProvider, buildInformationProvider, buildOutputLogger, buildProgressViewModel, packageSettingsProvider, buildService, errorNavigationService);
            }
            else if (serviceType == typeof(IBuildMessagesFactory))
            {
                return new BuildMessagesFactory(new ControlSettings().BuildMessagesSettings);
            }
            else if (serviceType == typeof(IBuildOutputLogger))
            {
                return new BuildOutputLogger(_parsingErrorsLoggerId, Microsoft.Build.Framework.LoggerVerbosity.Quiet);
            }
            else if (serviceType == typeof(IProjectFileNavigationService))
            {
                return new ProjectFileNavigationService(GetServiceProvider(), GetService<IBuildingProjectsProvider>());
            }
            else if (serviceType == typeof(IErrorNavigationService))
            {
                return new ErrorNavigationService(GetServiceProvider());
            }
            else if (serviceType == typeof(IBuildProgressViewModel))
            {
                return new BuildProgressViewModel(GetService<IPackageSettingsProvider>());
            }
            else if (serviceType == typeof(IBuildService))
            {
                return new BuildService(GetServiceProvider());
            }
            else if (serviceType == typeof(IStatusBarNotificationService))
            {
                return new StatusBarNotificationService(GetServiceProvider());
            }
            else
            {
                throw new Exception("Not found");
            }
        }

        private ServiceProvider GetServiceProvider()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var sp = new ServiceProvider(Services.Dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            Assumes.Present(sp);
            return sp;
        }

        public T GetService<T>() where T: class
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var sp = GetServiceProvider();
            var service = sp.GetService(typeof(T)) as T;
            Assumes.Present(service);
            return service;
        }
    }
}
