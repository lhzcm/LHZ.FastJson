using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson.JsonClass.Internal
{
    /// <summary>
    /// JSON null object
    /// </summary>
    internal class JsonNull : JsonClass.JsonNull
    {
        private int _position;
        internal JsonNull(int position)
        {
            _position = position;
        }
        internal override int Position => _position;
    }
}
