using System;
using System.Collections.Generic;

namespace LabDrivers.Cameras
{
    public class CameraOptionParameter : ICameraParameter
    {
        public CameraOptionParameter(string name, List<string> options, int selected, bool isReadOnly = false, bool isVisible = true, bool isRange = false)
        {
            Options = options;
            Value = selected;
            IsReadOnly = isReadOnly;
            IsVisible = isVisible;
            IsRange = isRange;
            HasItems = true;
            Type = typeof(int);
            Name = name;
        }

        public List<string> Options { get; }
        public object Value { get; set; }
        public Type Type { get; }
        public bool HasItems { get; }
        public string Name { get; }
        public bool IsReadOnly { get; }
        public bool IsVisible { get; }
        public bool IsRange { get; }
    }
}
