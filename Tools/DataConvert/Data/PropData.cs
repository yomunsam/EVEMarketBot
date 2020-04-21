using System;
using System.Collections.Generic;
using System.Text;

namespace DataConvert.Data
{
    /// <summary>
    /// 道具表
    /// </summary>
    public class PropData
    {
        public long ID { get; set; }
        public int GroupID { get; set; }
        public string Name { get; set; }
        public string Name_CN { get; set; }

        /// <summary>
        /// 体积
        /// </summary>
        public float Volume { get; set; }
    }
}
