using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSModLauncher
{
    public class JSONMod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public List<NamePath> Links { get; set; } = new List<NamePath>();
        public JSONModFiles Files { get; set; } = new JSONModFiles();
    }

    public class JSONModFiles
    {
        public string DoukutsuFile { get; set; } = "";
        public string ConfigFile { get; set; } = "";
    }

    public class Config
    {
        public List<ModListInfo> Mods { get; set; } = new List<ModListInfo>();
        public string ModsFolder { get; set; }
        public string Get_Mod_Path_By_ID(int ID, string file = "")
        {
            return Path.Combine(Mods.First(m => m.ID == ID).Path, file);
        }
    }
}
