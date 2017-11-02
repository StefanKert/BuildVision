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
        private string _resourceName;

		/// <summary>
		/// The value of this attribute.
		/// </summary>
		public string DisplayString { get; private set; }

		public string ResourceName
        {
            get { return _resourceName; }
            set 
            {
                _resourceName = value;
                DisplayString = Resources.ResourceManager.GetString(value, Resources.Culture);
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
            DisplayString = displayString;
        }

        public override bool Equals(object obj)
        {
            var dsaObj = obj as DisplayStringAttribute;
            if (dsaObj == null)
                return false;

            return DisplayString.Equals(dsaObj.DisplayString);
        }

        public override int GetHashCode()
        {
            return DisplayString.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}
