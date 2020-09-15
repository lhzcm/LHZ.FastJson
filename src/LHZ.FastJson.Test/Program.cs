//using LHZ.FastJson.JsonClass;
//using LHZ.FastJson.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.IO;
using LHZ.FastJson.JsonClass;

namespace LHZ.FastJson.Test
{
    class Program
    {
        static int Main(string[] args)
        {

            //string str = "";
            //using (StreamReader reader = File.OpenText("../../../db.json"))
            //{
            //    str = reader.ReadToEnd();
            //}
            //JsonObject obj = JsonConvert.Deserialize(str);
            //string s = obj.ToJsonString();
            ////Console.WriteLine(s);
            //obj = JsonConvert.Deserialize(s);

            //Console.WriteLine(obj.ToJsonString());
            int a = 1;
            short b = 10;
            uint c = 11;
            ushort d = 12;
            byte e = 255;
            Char f = 't';

            var obj = new Test() { a = 1, b = 2, c = 3, d = 4, e = 5, f = 'T', g = null, h = 1000000000000L };
            Console.WriteLine(JsonConvert.Serialize(obj));
            Console.WriteLine(JsonConvert.Serialize(JsonConvert.Deserialize<Test>(JsonConvert.Serialize(obj))));

            return 1;

        }

    }

    public class Test
    {
        public int a { get; set; }
        public short b { get; set; }
        public uint c { get; set; }
        public ushort d { get; set; }
        public byte e { get; set; }
        public Char f { get; set; }
        public ulong? g { get; set; }
        public long? h { get; set; }
    }
}
