using LHZ.FastJson.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LHZ.FastJson.JsonClass.Internal
{
    /// <summary>
    /// JSON array object
    /// </summary>
    internal sealed class JsonArray : JsonClass.JsonArray
    {
        private int _position;
        public JsonArray(int position)
        {
            _position = position;
        }
        internal override int Position => _position;
    }
}
