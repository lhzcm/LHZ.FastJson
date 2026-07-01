using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// JSON read exception
    /// </summary>
    public class JsonReadException : Exception
    {
        private int _position;
        /// <summary>
        /// Initialize JSON read exception
        /// </summary>
        /// <param name="position">Exception position</param>
        /// <param name="msg">Exception message</param>
        public JsonReadException(int position, string msg) : base(msg)
        {
            this._position = position;
        }
        /// <summary>
        /// Exception position
        /// </summary>
        public int Position { get { return _position; } }
    }
}
