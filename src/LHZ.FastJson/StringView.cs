using System;
using System.Data;
using System.Text;

/// <summary>
/// 字符串视图
/// </summary>
internal struct StringView
{
    public StringView(string sourceString, int startIndex, int endIndex)
    {
        SourceString = sourceString;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }
    public StringView(string sourceString)
    {
        SourceString = sourceString;
        StartIndex = 0;
        EndIndex = sourceString.Length - 1;
    }
    /// <summary>
    /// 源字符
    /// </summary>
    public string SourceString { get; }
    /// <summary>
    /// 开始位置
    /// </summary>
    /// <value></value>
    public int StartIndex { get; }
    public int Length => EndIndex - StartIndex + 1; 
    /// <summary>
    /// 结束位置
    /// </summary>
    /// <value></value>
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
            return Array.Empty<char>();
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