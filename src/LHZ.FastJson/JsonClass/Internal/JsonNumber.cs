using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass.Internal
{
    /// <summary>
    /// JSON number object
    /// </summary>
    internal sealed class JsonNumber: JsonClass.JsonNumber
    {
        private int _position;
        internal JsonNumber(StringView value, NumberType numberType, int position) : base(value, numberType)
        {
            _position = position;
        }
        internal override int Position => _position;
    }
}
