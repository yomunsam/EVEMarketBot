using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekonya.Jitas
{
    [System.Serializable]
    public class JitaJsonModel
    {
        public Items all;
        public Items buy;
        public Items sell;

        [System.Serializable]
        public struct Items
        {
            public float max;
            public float min;
            public long volume;
        }
    }
}
