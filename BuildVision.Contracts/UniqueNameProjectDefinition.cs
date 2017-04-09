namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public struct UniqueNameProjectDefinition
    {
        public string UniqueName;
        public string Configuration;
        public string Platform;

        public UniqueNameProjectDefinition(string uniqueName, string configuration, string platform)
        {
            UniqueName = uniqueName;
            Configuration = configuration;
            Platform = platform;
        }
    }
}