using JetBrains.Annotations;

namespace Coding4fun.DataTools.Common
{
    /// <summary>
    /// Naming convention kinds.
    /// </summary>
    [PublicAPI]
    public enum NamingConvention
    {
        /// <summary>
        /// PascalCase.
        /// </summary>
        PascalCase,

        /// <summary>
        /// camelCase.
        /// </summary>
        CamelCase,

        /// <summary>
        /// kebab-case.
        /// </summary>
        KebabCase,

        /// <summary>
        /// snake_case.
        /// </summary>
        SnakeCase,
        
        /// <summary>
        /// SCREAMING_SNAKE_CASE
        /// </summary>
        ScreamingSnakeCase
    }
}