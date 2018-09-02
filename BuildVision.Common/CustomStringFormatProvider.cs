using System;

namespace BuildVision.Common
{
    /// <remarks>
    /// U - String.ToUpper, 
    /// L - String.ToLower
    /// </remarks>
    /// <example>
    /// string.Format(new <see cref="CustomStringFormatProvider"/>(), "{0:U}", "Hello") == "HELLO";
    /// </example>
    public class CustomStringFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            return (formatType == typeof(ICustomFormatter)) ? this : null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null)
                return null;

            if (format != null && arg is string)
            {
                var strArg = (string) arg;
                switch (format.ToUpper())
                {
                    case "U":
                        return strArg.ToUpper();
                    case "L":
                        return strArg.ToLower();
                }
            }

            return arg is IFormattable
                       ? ((IFormattable) arg).ToString(format, formatProvider)
                       : arg.ToString();
        }
    }
}
