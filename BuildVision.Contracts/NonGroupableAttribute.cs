using System;

namespace BuildVision.Contracts
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class NonGroupableAttribute : Attribute { }
}