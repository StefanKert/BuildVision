using System.Runtime.Serialization;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings
{
    /// <remarks>
    /// See https://msdn.microsoft.com/en-us/library/Microsoft.VisualStudio.Shell.Interop.BuildOutputGroup.aspx
    /// </remarks>
    [DataContract]
    public class BuildOutputFileTypes
    {
        /// <summary>
        /// Represents localized resource DLLs in an output group.
        /// </summary>
        [DataMember]
        public bool LocalizedResourceDlls { get; set; }

        /// <summary>
        /// XML-serializer assemblies.
        /// </summary>
        [DataMember]
        public bool XmlSerializer { get; set; }

        /// <summary>
        /// Represents content files in an output group.
        /// </summary>
        [DataMember]
        public bool ContentFiles { get; set; }

        /// <summary>
        /// Represents built files in an output group.
        /// </summary>
        [DataMember]
        public bool Built { get; set; }

        /// <summary>
        /// Represents source code files in an output group.
        /// </summary>
        [DataMember]
        public bool SourceFiles { get; set; }

        /// <summary>
        /// Represents a list of symbols in an output group.
        /// </summary>
        [DataMember]
        public bool Symbols { get; set; }

        /// <summary>
        /// Represents documentation files in an output group.
        /// </summary>
        [DataMember]
        public bool Documentation { get; set; }

        /// <summary>
        /// Checks whether all properties is <c>false</c>.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return !(LocalizedResourceDlls 
                            || XmlSerializer 
                            || ContentFiles 
                            || Built 
                            || SourceFiles 
                            || Symbols 
                            || Documentation);
            }
        }

        public BuildOutputFileTypes()
        {
            LocalizedResourceDlls = true;
            Built = true;
            Symbols = true;
            Documentation = true;
        }
    }
}