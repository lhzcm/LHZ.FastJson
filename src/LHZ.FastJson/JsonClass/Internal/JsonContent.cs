using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.JsonClass.Internal
{
    /// <summary>
    /// JSON container object
    /// </summary>
    internal sealed class JsonContent : JsonClass.JsonContent
    {
        private int _position;
        public JsonContent(int position)
        {
            _position = position;
        }
        internal override int Position => _position;
    }
}
