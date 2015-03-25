namespace AlekseyNagovitsyn.BuildVision.Tool.Building
{
    public struct FullNameProjectDefinition
    {
        public string FullName;
        public string Configuration;
        public string Platform;

        public FullNameProjectDefinition(string fullName, string configuration, string platform)
        {
            FullName = fullName;
            Configuration = configuration;
            Platform = platform;
        }
    }
}