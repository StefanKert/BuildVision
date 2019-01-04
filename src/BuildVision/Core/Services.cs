using System;
using System.ComponentModel.Composition.Hosting;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BuildVision.Core
{

    public static class Services
    {
        static TResult GetGlobalService<T, TResult>(IServiceProvider provider = null) where T : class where TResult : class
        {

            if(provider != null)
                return provider.GetService(typeof(T)) as TResult;
            return Package.GetGlobalService(typeof(T)) as TResult;
        }

        public static DTE Dte => GetGlobalService<DTE, DTE>();

        public static DTE2 Dte2 => Dte as DTE2;

        public static ExportProvider DefaultExportProvider => ComponentModel.DefaultExportProvider;

        public static IComponentModel ComponentModel => GetGlobalService<SComponentModel, IComponentModel>();

        public static IVsSolution GetSolution(this IServiceProvider provider)
        {
            return GetGlobalService<SVsSolution, IVsSolution>(provider);
        }
    }
}
