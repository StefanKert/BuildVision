using System;
using System.Runtime.Serialization;

namespace BuildVision.Contracts
{
  public abstract class BaseGridColumnSettings : Attribute
  {
    public string Header { get; set; }

    public bool Visible { get; set; }

    /// <remarks>
    /// -1 for auto.
    /// </remarks>
    public int DisplayIndex { get; set; }

    /// <remarks>
    /// double.NaN for auto.
    /// </remarks>
    public double Width { get; set; }

    public string ValueStringFormat { get; set; }

    protected BaseGridColumnSettings()
    {
      Width = double.NaN;
      DisplayIndex = -1;
      Visible = true;
    }
  }
}