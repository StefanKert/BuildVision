using BuildVision.Contracts;
using BuildVision.UI;
using System;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GridColumnAttribute : BaseGridColumnSettings
    {
        private object _exampleValue;

        public const string EmptyHeaderImageKey = "[empty]";

        public string ImageKey { get; set; }

        public string ImageDictionaryUri { get; set; }

        public object ExampleValue
        {
            get { return _exampleValue; }
            set { _exampleValue = value; }
        }

        public string TimeSpanExampleValue
        {
            get { throw new InvalidOperationException(); }
            set { _exampleValue = TimeSpan.Parse(value); }
        }

        public string DateTimeExampleValue
        {
            get { throw new InvalidOperationException(); }
            set { _exampleValue = DateTime.Parse(value); }
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