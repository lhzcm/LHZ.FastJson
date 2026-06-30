using System;
using System.Data;
using System.Text;

/// <summary>
/// 轻量级字符串视图，避免字符串截取产生额外分配
/// </summary>
internal struct StringView
{
    private static char[] _charArrayEmpty = new char[0];
    /// <summary>
    /// 使用指定范围初始化
    /// </summary>
    /// <param name="sourceString">源字符串</param>
    /// <param name="startIndex">起始索引</param>
    /// <param name="endIndex">结束索引</param>
    public StringView(string sourceString, int startIndex, int endIndex)
    {
        SourceString = sourceString;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }
    /// <summary>
    /// 使用完整字符串初始化
    /// </summary>
    /// <param name="sourceString">源字符串</param>
    public StringView(string sourceString)
    {
        SourceString = sourceString;
        StartIndex = 0;
        EndIndex = sourceString.Length - 1;
    }
    /// <summary>
    /// 源字符串
    /// </summary>
    public string SourceString { get; }
    /// <summary>
    /// 起始索引
    /// </summary>
    public int StartIndex { get; }
    /// <summary>
    /// 视图长度
    /// </summary>
    public int Length => EndIndex - StartIndex + 1; 
    /// <summary>
    /// 结束索引
    /// </summary>
    public int EndIndex { get; }
    public override string ToString()
    {
        if (EndIndex < StartIndex)
            return string.Empty;
        int leng = EndIndex - StartIndex + 1;
        if (SourceString.Length == leng)
            return SourceString;
        return SourceString.Substring(StartIndex, leng);
    }
    public static bool operator ==(StringView left, StringView right)
    {
        var length = left.EndIndex - left.StartIndex + 1;
        if (length != (right.EndIndex - right.StartIndex + 1))
            return false;
        for (var i = 0; i < length; i++)
        {
            if (left.SourceString[left.StartIndex + i] != right.SourceString[right.StartIndex + i])
                return false;

        }
        return true;
    }
    public static bool operator !=(StringView left, StringView right)
    {
        return !(left == right);
    }
    public override bool Equals(object obj)
    {
        if (!(obj is StringView other))
            return false;
        return this == other;
    }
    public override int GetHashCode()
    {
        uint hash = 5381;
        if (EndIndex < StartIndex)
            return (int)hash;
        for (int i = StartIndex; i <= EndIndex; i++)
        {
            unchecked
            {
                hash = (hash << 5) + hash + SourceString[i];
            }
        }
        return (int)hash;
    }
    public StringBuilder ToStringBuilder()
    {
        if (EndIndex < StartIndex)
            return new StringBuilder();
        int leng = EndIndex - StartIndex + 1;
        if (SourceString.Length == leng)
            return new StringBuilder(SourceString);
        return new StringBuilder(SourceString, StartIndex, leng, leng);
    }
    public char[] ToCharArray()
    {
        if (EndIndex < StartIndex)
            return _charArrayEmpty;
        int length = EndIndex - StartIndex + 1;
        if (SourceString.Length == length)
            return SourceString.ToCharArray();
        return SourceString.ToCharArray(StartIndex, length);
    }
    public void AppendToStringBuilder(StringBuilder stringBuilder)
    {
        if(stringBuilder == null)
        {
            throw new ArgumentNullException(nameof(stringBuilder));
        }
        if (EndIndex < StartIndex)
            return;
        int leng = EndIndex - StartIndex + 1;
        if (SourceString.Length == leng)
        {
            stringBuilder.Append(SourceString);
        }
        else
        {
            stringBuilder.Append(SourceString, StartIndex, leng);
        }
    }
}