using R3;

namespace Modules.Toolbag.Extensions
{
    public static class ReactivePropertyExtensions
    {
        public static void Clear<T>(this ReactiveProperty<T> property)
        {
            property.Value = default;
        }
    }
}