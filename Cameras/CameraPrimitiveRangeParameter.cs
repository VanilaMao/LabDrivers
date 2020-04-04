namespace LabDrivers.Cameras
{
    public class CameraPrimitiveRangeParameter : CameraPrimitiveParameter
    {
        public CameraPrimitiveRangeParameter(string name, object value, object max, object min, bool isReadOnly = false, bool isVisible = true) : base(name, value, isReadOnly, isVisible)
        {
            Max = max;
            Min = min;
            IsRange = true;
        }

        public object Max { get; }

        public object Min { get; }

        public override bool IsRange { get; }
    }
}
