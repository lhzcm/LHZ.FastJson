using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    public class JsonReadException : Exception
    {
        private int _position;
        public JsonReadException(int position, string msg) : base(msg)
        {
            this._position = position;
        }
        public int Position { get { return _position; } }
    }
}
