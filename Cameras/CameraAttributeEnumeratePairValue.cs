namespace LabDrivers.Cameras
{
    public class CameraAttributeEnumeratePairValue
    {
        public CameraAttributeEnumeratePairValue(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public int Value { get;  }

        public string Name { get;  }
    }
}
