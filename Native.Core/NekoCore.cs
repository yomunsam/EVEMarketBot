using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekonya.DBs;

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
    }
}
