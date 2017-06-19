using BuildVision.UI;
using System;

namespace BuildVision.UI.Helpers
{
    /// <summary>
    /// Specifies description for a member of the enum type for display to the UI.
    /// </summary>
    /// <see cref="EnumExtensions.DisplayString"/>
    /// <example>
    ///     <code>
    ///         enum OperatingSystem
    ///         {
    ///            [DisplayString("MS-DOS")]
    ///            Msdos
    ///         }
    ///         
    ///         public string GetMyOSName()
    ///         {
    ///             var myOS = OperatingSystem.Seven;
    ///             return myOS.DisplayString();
    ///         }
    ///     </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayStringAttribute : Attribute
    {
        /// <summary>
        /// The default value for the attribute <see cref="DisplayStringAttribute"/>, which is an empty string.
        /// </summary>
        public static readonly DisplayStringAttribute Default = new DisplayStringAttribute();

        private string _displayString;

        private string _resourceName;

        /// <summary>
        /// The value of this attribute.
        /// </summary>
        public string DisplayString
        {
            get { return _displayString; }
        }

        public string ResourceName
        {
            get { return _resourceName; }
            set 
            {
                _resourceName = value;
                _displayString = Resources.ResourceManager.GetString(value, Resources.Culture);
            }
        }

        /// <summary>
        /// Initializes a new instance of the class <see cref="DisplayStringAttribute"/> with default value (empty string).
        /// </summary>
        public DisplayStringAttribute()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class <see cref="DisplayStringAttribute"/> with specified value.
        /// </summary>
        /// <param name="displayString">The value of this attribute.</param>
        public DisplayStringAttribute(string displayString)
        {
            _displayString = displayString;
        }

        public override bool Equals(object obj)
        {
            var dsaObj = obj as DisplayStringAttribute;
            if (dsaObj == null)
                return false;

            return _displayString.Equals(dsaObj._displayString);
        }

        public override int GetHashCode()
        {
            return _displayString.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}