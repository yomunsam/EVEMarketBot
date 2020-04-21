using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekonya
{
    public static class EVEUtil
    {
        /// <summary>
        /// 解析.jita 消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool ParseJitaMsg(string msg, out string sourceStr, out List<string> queryStr)
        {
            sourceStr = default;
            queryStr = new List<string>();
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".jita ")) return false;
            sourceStr = msg.Substring(5)?.Trim();
            if (string.IsNullOrEmpty(sourceStr)) return false;

            queryStr.Add(sourceStr);
            GetQueryStr(ref sourceStr, ref queryStr);

            return true;
        }

        public static void GetQueryStr(ref string scoreStr, ref List<string> queryStr)
        {
            if (scoreStr.StartsWith("海") && !scoreStr.StartsWith("海军型"))
            {
                if (scoreStr.EndsWith("级"))
                    queryStr.Add(scoreStr.Substring(1,scoreStr.Length -1) + "海军型");
                else
                    queryStr.Add(scoreStr.Substring(1, scoreStr.Length - 1) + "级海军型");
            }

            if (!scoreStr.EndsWith("级") && !scoreStr.EndsWith("海军型"))
            {
                queryStr.Add(scoreStr + "级");
            }
        } 
    }
}
