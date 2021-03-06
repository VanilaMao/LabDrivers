﻿using System.Collections.Generic;

namespace LabDrivers.Stages
{
    public class StageService :IStageService
    {
        // hide initialize
        public static IStageService Current { get; } = new StageService();

        public IEnumerable<ITrackingStage> GetStages()
        {
            var priorStage = new PriorTrackingStage(0,0);
            if (priorStage.Connect())
            {
                yield return priorStage;
            }
        }
    }
}
