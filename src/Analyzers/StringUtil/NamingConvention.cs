using JetBrains.Annotations;

namespace Coding4fun.DataTools.Analyzers.StringUtil
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
        /// snake_case.
        /// </summary>
        SnakeCase,
        
        /// <summary>
        /// SCREAMING_SNAKE_CASE
        /// </summary>
        ScreamingSnakeCase
    }
}