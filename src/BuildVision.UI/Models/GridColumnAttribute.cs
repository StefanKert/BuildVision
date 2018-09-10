using BuildVision.Contracts;
using BuildVision.UI;
using System;

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
            get { return ""; }
            set { ExampleValue = TimeSpan.Parse(value); }
        }

        public string DateTimeExampleValue
        {
            get { return ""; }
            set { ExampleValue = DateTime.Parse(value); }
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
