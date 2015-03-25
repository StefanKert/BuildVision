using System;
using System.Linq.Expressions;

namespace AlekseyNagovitsyn.BuildVision.Helpers
{
    /// <summary>
    /// <see cref="object"/> extensions
    /// </summary>
    public static class ObjectExtensions
    {
        // <summary>
        // Get the name of a static or instance property from a property access lambda.
        // </summary>
        // <typeparam name="T">Type of the property</typeparam>
        // <param name="propertyLambda">lambda expression of the form: '() => Class.Property' or '() => object.Property'</param>
        // <returns>The name of the property</returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;
            if (me == null)
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");

            return me.Member.Name;
        }
    }
}