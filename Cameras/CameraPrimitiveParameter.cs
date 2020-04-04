using System;

namespace LabDrivers.Cameras
{

    // could be displayed in the UI
    // support localization
    public class CameraPrimitiveParameter : ICameraParameter
    {
        public CameraPrimitiveParameter(string name, object value, bool isReadOnly = false, bool isVisible = true)
        {
            Name = name;
            Value = value;
            IsReadOnly = isReadOnly;
            IsVisible = isVisible;
            IsRange = false;
            Type = value.GetType();
            HasItems = false;
        }


        public Type Type { get; }
        public bool HasItems { get; }
        public string Name { get; }

        public object Value { get; set; }
        public bool IsReadOnly { get; }
        public bool IsVisible { get; }
        public virtual bool IsRange { get;}
    }
}
