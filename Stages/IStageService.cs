
using System.Collections.Generic;

namespace LabDrivers.Stages
{
    public interface IStageService
    {
        IEnumerable<ITrackingStage> GetStages();
    }
}
