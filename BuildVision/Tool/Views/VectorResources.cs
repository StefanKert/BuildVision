using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Windows;
using System.Windows.Controls;

namespace AlekseyNagovitsyn.BuildVision.Tool.Views
{
    public static class VectorResources
    {
        private static readonly string _baseUri = string.Format(@"/{0};component/", Resources.ProductName);

        private static readonly Dictionary<string, ResourceDictionary> _cachedResources = new Dictionary<string, ResourceDictionary>();

        private static object GetResource(string resourceDictionaryRelativeUri, string resourceKey)
        {
            ResourceDictionary dict;
            if (_cachedResources.TryGetValue(resourceDictionaryRelativeUri, out dict))
            {
                return dict[resourceKey];
            }

            dict = (ResourceDictionary)Application.LoadComponent(new Uri(_baseUri + resourceDictionaryRelativeUri.TrimStart('/'), UriKind.Relative));
            if (dict == null)
                throw new InstanceNotFoundException();

            _cachedResources[resourceDictionaryRelativeUri] = dict;
            return dict[resourceKey];
        }

        public static ControlTemplate Get(string resourceDictionaryRelativeUri, string resourceKey)
        {
            return (ControlTemplate)GetResource(resourceDictionaryRelativeUri, resourceKey);
        }

        public static ControlTemplate TryGet(string resourceDictionaryRelativeUri, string resourceKey)
        {
            try
            {
                return Get(resourceDictionaryRelativeUri, resourceKey);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}