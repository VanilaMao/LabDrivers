using System.IO;

namespace LabSave
{
    public static class FileExtensions
    {
        public static string SplitFileName(this string fileName)
        {
            return Path.GetDirectoryName(fileName) + "/" + Path.GetFileNameWithoutExtension(fileName);
        }
        public static string DirectorName(this string fileName)
        {
            return Path.GetDirectoryName(fileName) + "/";
        }
    }
}
