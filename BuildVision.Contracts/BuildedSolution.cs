using System.IO;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class BuildedSolution
    {
        public string FullName { get; set; }

        public string Name { get; set; }

        public BuildedSolution(string fullName, string fileName)
        {
            FullName = fullName;
            Name = Path.GetFileNameWithoutExtension(fileName);
        }
    }
}