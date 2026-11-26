namespace LHZ.FastJson.Enum
{
    /// <summary>
    /// Naming style applied to object property names during serialization and deserialization.
    /// A property marked with <see cref="Json.Attributes.JsonPropertyAttribute"/> always keeps its explicitly configured name.
    /// </summary>
    public enum CaseStyle
    {
        /// <summary>
        /// Property names are used exactly as declared (the default), e.g. <c>UserName</c> -> <c>UserName</c>.
        /// </summary>
        PascalCase = 0,

        /// <summary>
        /// camelCase, e.g. <c>UserName</c> -> <c>userName</c>, <c>URLValue</c> -> <c>urlValue</c>.
        /// </summary>
        CamelCase = 1,

        /// <summary>
        /// snake_case (lower case), e.g. <c>UserName</c> -> <c>user_name</c>, <c>URLValue</c> -> <c>url_value</c>.
        /// </summary>
        SnakeCase = 2,

        /// <summary>
        /// kebab-case (lower case), e.g. <c>UserName</c> -> <c>user-name</c>, <c>URLValue</c> -> <c>url-value</c>.
        /// </summary>
        KebabCase = 3
    }
}
