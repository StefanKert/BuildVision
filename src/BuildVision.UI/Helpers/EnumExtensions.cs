using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BuildVision.UI.Helpers
{
    /// <summary>
    /// Extension methods for the enum data type
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Iterate over values in Flags Enum.
        /// </summary>
        /// <param name="input">Field of Enumeration.</param>
        /// <returns>Returns established flags in Enumeration.</returns>
        public static IEnumerable<Enum> GetFlags(this Enum input)
        {
            var values = Enum.GetValues(input.GetType()).Cast<Enum>();
            return values.Where(input.HasFlag);
        }

        /// <summary>
        /// Description, specified by attribute <see cref="DisplayStringAttribute"/>.
        /// <para>If the attribute is not specified, returns the default name obtained by the method <c>ToString()</c>.</para>
        /// </summary>
        /// <param name="value">Enum value.</param>
        /// <returns>
        /// Returns the description given by the attribute <see cref="DisplayStringAttribute"/>. 
        /// <para>If the attribute is not specified, returns the default name obtained by the method <c>ToString()</c>.</para>
        /// </returns>
        /// <see cref="DisplayStringAttribute"/>
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
        ///             var myOS = OperatingSystem.Msdos;
        ///             return myOS.DisplayString();
        ///         }
        ///     </code>
        /// </example>
        public static string DisplayString(this Enum value)
        {
            FieldInfo info = value.GetType().GetField(value.ToString());
            var attributes = (DisplayStringAttribute[])info.GetCustomAttributes(typeof(DisplayStringAttribute), false);
            return (attributes.Length >= 1) ? attributes[0].DisplayString : value.ToString();
        }
    }
}