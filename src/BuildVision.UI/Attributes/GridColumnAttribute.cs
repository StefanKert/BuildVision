using System;
using BuildVision.Contracts;

namespace BuildVision.UI.Modelss
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GridColumnAttribute : BaseGridColumnSettingsAttribute
    {
        public const string EmptyHeaderImageKey = "[empty]";

        public string ImageKey { get; set; }

        public string ImageDictionaryUri { get; set; }

        public object ExampleValue { get; set; }

        public string TimeSpanExampleValue
        {
            get => "";
            set => ExampleValue = TimeSpan.Parse(value);
        }

        public string DateTimeExampleValue
        {
            get => "";
            set => ExampleValue = DateTime.Parse(value);
        }

        public GridColumnAttribute(
            string headerResourceName,
            ColumnsOrder displayOrder,
            bool visible)
        {
            Header = Resources.ResourceManager.GetString(headerResourceName, Resources.Culture);
            DisplayIndex = (int)displayOrder;
            Visible = visible;
        }
    }
}
