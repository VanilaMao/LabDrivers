

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public interface IDataProviderFactory
    {
        IDataProvider GetDataProvider(DataProviderContext context);
    }
}
