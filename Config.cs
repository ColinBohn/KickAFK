using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace KickAFK
{
    public class Config
    {
        public int KickTime = 600;
        public int WarnTime = 540;
        public string KickMsg = "Was inactive for too long";

        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            if (!File.Exists(path))
            {
                return new Config();
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}