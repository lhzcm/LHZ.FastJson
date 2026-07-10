using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass.Internal
{
    /// <summary>
    /// JSON boolean object
    /// </summary>
    internal class JsonBoolean : JsonClass.JsonBoolean
    {
        private int _position;
        internal JsonBoolean(bool value, int position) : base(value)
        {
            this._position = position;
        }
        internal override int Position => _position;
    }
}
