using System;
using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns
{
    [DataContract]
    public abstract class BaseGridColumnSettings : Attribute
    {
        [DataMember]
        public string Header { get; set; }

        [DataMember]
        public bool Visible { get; set; }

        /// <remarks>
        /// -1 for auto.
        /// </remarks>
        [DataMember]
        public int DisplayIndex { get; set; }

        /// <remarks>
        /// double.NaN for auto.
        /// </remarks>
        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public string ValueStringFormat { get; set; }

        protected BaseGridColumnSettings()
        {
            Width = double.NaN;
            DisplayIndex = -1;
            Visible = true;
        }
    }
}