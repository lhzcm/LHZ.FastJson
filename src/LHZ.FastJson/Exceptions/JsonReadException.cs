using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// Json读取异常
    /// </summary>
    public class JsonReadException : Exception
    {
        private int _position;
        /// <summary>
        /// 初始化Json读取异常
        /// </summary>
        /// <param name="position">异常位置</param>
        /// <param name="msg">异常消息</param>
        public JsonReadException(int position, string msg) : base(msg)
        {
            this._position = position;
        }
        /// <summary>
        /// 异常位置
        /// </summary>
        public int Position { get { return _position; } }
    }
}
