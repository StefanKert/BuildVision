using System.IO;

using EnvDTE;

namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public class BuildedSolution
    {
        public string FullName { get; set; }

        public string Name { get; set; }

        public BuildedSolution(Solution solution)
        {
            FullName = solution.FullName;
            Name = Path.GetFileNameWithoutExtension(solution.FileName);
        }
    }
}