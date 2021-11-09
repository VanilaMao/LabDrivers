using System;
using System.Threading.Tasks;

namespace LabSave
{
    public interface ILabSave
    {
        void AddOneFrame(Frame frame, bool lastFrame = false);
        void AddFrames(Frame[] frames);
        void Save(Action completeSave);
        int MaxFrameAllowed { get; }
        void ClearSave();
        DateTime StartTime { get; }
        string FileName { get; }
        bool SaveAvi { get; set; }
        int FrameRate { get; }
        int TotalFrames { get; }
        string AviFilterName { get; set; }
        int BackgroundColor { get; set; }
    }
}

