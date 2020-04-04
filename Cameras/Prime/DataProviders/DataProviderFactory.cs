

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public class DataProviderFactory: IDataProviderFactory
    {
        public IDataProvider GetDataProvider(DataProviderContext context)
        {
            switch (context)
            {
                case DataProviderWithMetaDataMultiRoiContext roiContext:
                    return new DataWithMetaDataMultiRoiProvider(roiContext);
                case DataProviderWithMetaDataContext dataContext:
                    return new DataWithMetaDataSingleRoiProvider(dataContext);
                case DataProviderWithoutMetaDataContext metaDataContext:
                    return new DataWithoutMetaDataProvider(metaDataContext);
            }

            return null;
        }
    }
}
