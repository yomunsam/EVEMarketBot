using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Nekonya.DBs;
using Nekonya.Config;
using Native.Core.Domain;
using Newtonsoft.Json;

namespace Nekonya
{
    public class NekoCore
    {
        private static object _lock_obj = new object();
        private static NekoCore _instance;
        public static NekoCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock_obj)
                    {
                        if (_instance == null)
                        {
                            _instance = new NekoCore();
                        }
                    }
                }
                return _instance;
            }
        }

        public EVEGameDB EVEDB { get; private set; } = new EVEGameDB();

        public MarketDB MarketDB { get; private set; } = new MarketDB();

        public UserDB UserDB { get; private set; } = new UserDB();

        public EVEMarketConfig Config { get; private set; }

        public NekoCore()
        {
            //初始化配置文件
            string conf_path = Path.Combine(AppData.CQApi.AppDirectory, "config.json");
            if (File.Exists(conf_path))
            {
                try
                {
                    var json = File.ReadAllText(conf_path, Encoding.UTF8);
                    Config = JsonConvert.DeserializeObject<EVEMarketConfig>(json);
                }
                catch { }
            }

            if(Config == null)
            {
                Config = new EVEMarketConfig();
                File.WriteAllText(conf_path, JsonConvert.SerializeObject(this.Config, Formatting.Indented),Encoding.UTF8);
            }
        }

        public void SaveConfig()
        {
            string conf_path = Path.Combine(AppData.CQApi.AppDirectory, "config.json");
            File.WriteAllText(conf_path, JsonConvert.SerializeObject(this.Config, Formatting.Indented), Encoding.UTF8);
        }
    }
}
