using System;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json属性名称结构体
    /// </summary>
    public struct JsonPropertyName
    {
        /// <summary>
        /// 哈希码（惰性缓存）
        /// </summary>
        public int HashCode {get; private set;}
        /// <summary>
        /// 属性名称
        /// </summary>
        internal StringView Name { get; }
        /// <summary>
        /// 使用字符串初始化
        /// </summary>
        /// <param name="name">属性名称</param>
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
        /// 相等运算符
        /// </summary>
        public static bool operator ==(JsonPropertyName left, JsonPropertyName right)
        {
            return left.Name == right.Name;
        }
        /// <summary>
        /// 不等运算符
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
        /// 获取哈希码（惰性计算）
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
        /// 返回属性名称的字符串表示
        /// </summary>
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
