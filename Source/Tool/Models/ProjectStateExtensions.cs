using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace AlekseyNagovitsyn.BuildVision.Tool.Models
{
    public static class ProjectStateExtensions
    {
        public static bool IsSuccessState(this ProjectState state)
        {
            return state.HasAttribute<ProjectStateSuccessAttribute>();
        }

        public static bool IsErrorState(this ProjectState state)
        {
            return state.HasAttribute<ProjectStateErrorAttribute>();
        }

        public static bool IsProgressState(this ProjectState state)
        {
            return state.HasAttribute<ProjectStateProgressAttribute>();
        }

        public static bool IsStandByState(this ProjectState state)
        {
            return state.HasAttribute<ProjectStateStandByAttribute>();
        }

        public static ControlTemplate GetAssociatedContent(this ProjectState state)
        {
            var atts = state.GetAttributes<AssociatedProjectStateVectorIconAttribute>();
            return (atts.Length != 0) ? atts[0].ControlTemplate : null;
        }

        private static T[] GetAttributes<T>(this ProjectState state)
        {
            FieldInfo fieldInfo = state.GetType().GetField(state.ToString());
            object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(T), false);
            return customAttributes.Cast<T>().ToArray();
        }

        private static bool HasAttribute<T>(this ProjectState state)
        {
            return state.GetAttributes<T>().Length != 0;
        }
    }
}