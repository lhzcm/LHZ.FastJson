using LHZ.FastJson.JsonClass;
using LHZ.FastJson.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.IO;

namespace LHZ.FastJson.Test
{
    class Program
    {
        static int Main(string[] args)
        {

            string str = "";
            using (StreamReader reader = File.OpenText("../../../db.json"))
            {
                str = reader.ReadToEnd();
            }
            JsonObject obj = JsonConvert.Deserialize(str);
            string s = obj.ToJsonString();
            //Console.WriteLine(s);
            obj = JsonConvert.Deserialize(s);
            JsonArray arry = obj as JsonArray;

            foreach (var item in arry)
            {
                JsonContent content = item as JsonContent;
                if (content != null)
                {
                    foreach (var item2 in content)
                    {
                        Console.WriteLine(String.Format("Name={0}, Value={1}",item2.Key, item2.Value.ToJsonString()));
                    }
                }
            }
            return 1;
        }
    }
}
