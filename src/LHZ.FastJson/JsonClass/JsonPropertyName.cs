using System;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON property name struct
    /// </summary>
    public struct JsonPropertyName
    {
        /// <summary>
        /// Hash code (lazy caching)
        /// </summary>
        public int HashCode {get; private set;}
        /// <summary>
        /// Property name
        /// </summary>
        internal StringView Name { get; }
        /// <summary>
        /// Initialize with string
        /// </summary>
        /// <param name="name">Property name</param>
        public JsonPropertyName(string name)
        {
            Name = new StringView(name);
            HashCode = 0;
        }
        internal JsonPropertyName(StringView stringView)
        {
            Name = stringView;
            HashCode = 0;
        }
        internal JsonPropertyName(StringView stringView, int hashCode)
        {
            Name = stringView;
            HashCode = hashCode;
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator ==(JsonPropertyName left, JsonPropertyName right)
        {
            return left.Name == right.Name;
        }
        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(JsonPropertyName left, JsonPropertyName right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (!(obj is JsonPropertyName other))
                return false;
            return this == other;
        }
        /// <summary>
        /// Get hash code (lazy computation)
        /// </summary>
        public override int GetHashCode()
        {
            if(HashCode == 0)
            {
                HashCode = Name.GetHashCode();
            }
            return HashCode;
        }
        /// <summary>
        /// Returns the string representation of the property name
        /// </summary>
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
