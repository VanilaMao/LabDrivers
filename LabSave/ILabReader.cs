
using System;
using System.Threading.Tasks;

namespace LabSave
{
    public interface ILabReader
    {
        Task ReadAsync(string fileName, Action<double> reportProgress);
    }
}
