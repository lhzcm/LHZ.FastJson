using System;

namespace LHZ.FastJson.JsonClass
{
    public struct JsonPropertyName
    {
        public int HashCode {get; private set;}
        /// <summary>
        /// Gets the name of the JSON property.
        /// </summary>
        internal StringView Name { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyName"/> class with the specified property name.
        /// </summary>
        /// <param name="name">The name of the JSON property.</param>
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

        public static bool operator ==(JsonPropertyName left, JsonPropertyName right)
        {
            return left.Name == right.Name;
        }
        public static bool operator !=(JsonPropertyName left, JsonPropertyName right)
        {
            return !(left == right);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is JsonPropertyName other))
                return false;
            return this == other;
        }
        public override int GetHashCode()
        {
            if(HashCode == 0)
            {
                HashCode = Name.GetHashCode();
            }
            return HashCode;
        }
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
