using System;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models.Settings.Columns
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class NonGroupableAttribute : Attribute { }
}