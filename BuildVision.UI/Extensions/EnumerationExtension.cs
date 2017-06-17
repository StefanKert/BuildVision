using BuildVision.UI.Helpers;
using System;
using System.Linq;
using System.Windows.Markup;

namespace BuildVision.UI.Extensions
{
    public class EnumerationExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumerationExtension(Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException("enumType");

            SetEnumType(enumType);
        }

        private void SetEnumType(Type value)
        {
            if (_enumType == value)
                return;

            var enumType = Nullable.GetUnderlyingType(value) ?? value;

            if (enumType.IsEnum == false)
                throw new ArgumentException("Type must be an Enum.");

            _enumType = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(_enumType);
            return (from object enumValue in enumValues
                    select new
                    {
                        Value = enumValue,
                        Description = GetDescription(enumValue)
                    }).ToArray();
        }

        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = _enumType.GetField(enumValue.ToString())
                .GetCustomAttributes(typeof(DisplayStringAttribute), false)
                .FirstOrDefault() as DisplayStringAttribute;
            return (descriptionAttribute != null)
                ? descriptionAttribute.DisplayString
                : enumValue.ToString();
        }
    }
}