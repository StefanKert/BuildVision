using System;
using System.ComponentModel.Composition;
using BuildVision.Views.Settings;
using Microsoft.VisualStudio.Shell;

namespace BuildVision.Core
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ServiceProviderExports
    {
        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public ServiceProviderExports([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Export]
        public IPackageSettingsProvider PackageSettingsProvider => (IPackageSettingsProvider) _serviceProvider.GetService(typeof(IPackageSettingsProvider));

    }
}
