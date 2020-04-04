

using System;

namespace LabDrivers.Cameras
{
    public interface ICameraParameter
    {
        string Name { get; }
        bool IsReadOnly { get; }

        bool IsVisible { get; }

        bool IsRange { get;  }

        object Value { get; set; }

        Type Type { get; }

        bool HasItems { get; }
    }
}
