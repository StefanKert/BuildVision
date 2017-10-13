namespace BuildVision.Contracts
{
    public class BuildedProject
    {
        public string UniqueName { get; set; }

        public string FileName { get; set; }

        public string Configuration { get; set; }

        public string Platform { get; set; }

        public bool? Success { get; set; }

        public ErrorsBox ErrorsBox { get; set; }

        public ProjectState ProjectState { get; set; }

        public BuildedProject(string uniqueProjectName, string fileName, string configuration, string platform)
        {
            UniqueName = uniqueProjectName;
            FileName = fileName;
            Configuration = configuration;
            Platform = platform;
            Success = null;
            ErrorsBox = new ErrorsBox();
        }   
    }
}
