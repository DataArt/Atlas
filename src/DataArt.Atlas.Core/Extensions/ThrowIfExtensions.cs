using System;
using System.Runtime.CompilerServices;

namespace DataArt.Atlas.Core.Extensions
{
    public static class ThrowIfExtensions
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public static void ThrowIfNull([ValidatedNotNull] this object value, string name, [CallerMemberName] string method = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException($"Method:{method} Parameter:{name}");
            }
        }

        // The naming is important to inform FxCop
        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}