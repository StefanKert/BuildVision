using System.Runtime.Serialization;

namespace BuildVision.Contracts
{
  /// <remarks>
  /// See https://msdn.microsoft.com/en-us/library/Microsoft.VisualStudio.Shell.Interop.BuildOutputGroup.aspx
  /// </remarks>
  public class BuildOutputFileTypes
  {
    /// <summary>
    /// Represents localized resource DLLs in an output group.
    /// </summary>
    public bool LocalizedResourceDlls { get; set; }

    /// <summary>
    /// XML-serializer assemblies.
    /// </summary>
    public bool XmlSerializer { get; set; }

    /// <summary>
    /// Represents content files in an output group.
    /// </summary>
    public bool ContentFiles { get; set; }

    /// <summary>
    /// Represents built files in an output group.
    /// </summary>
    public bool Built { get; set; }

    /// <summary>
    /// Represents source code files in an output group.
    /// </summary>
    public bool SourceFiles { get; set; }

    /// <summary>
    /// Represents a list of symbols in an output group.
    /// </summary>
    public bool Symbols { get; set; }

    /// <summary>
    /// Represents documentation files in an output group.
    /// </summary>
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