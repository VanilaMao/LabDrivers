using System;
using System.ComponentModel;

namespace HardDriveTestView
{
    class AttributeToEnumExtension
    {
        public static string GetValueFromDescription(Type type, string description)
        {
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return field.GetValue(null).ToString();
                }
                else
                {
                    if (field.Name == description)
                        return field.GetValue(null).ToString();
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }
    }
}
