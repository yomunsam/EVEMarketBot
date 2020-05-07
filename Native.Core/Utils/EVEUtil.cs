using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Native.Core.Domain;

namespace Nekonya
{
    public static class EVEUtil
    {
        /// <summary>
        /// 解析.jita 消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool ParseJitaMsg(string msg, out string sourceStr, out List<string> queryStr, out long num, ref bool isHans)
        {
            num = 1;
            sourceStr = default;
            queryStr = new List<string>();
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".jita ")) return false;
            string pure_str = msg.Substring(5)?.Trim();
            if (string.IsNullOrEmpty(pure_str)) return false;
            sourceStr = pure_str;

            //检查数量
            if (TryGetQueryNum(pure_str, '*', out var _num, out var _text))
            {
                sourceStr = _text;
                num = _num;
            }
            else if (TryGetQueryNum(pure_str, ',', out var __num, out var __text))
            {
                sourceStr = __text;
                num = __num;
            }
            else if (TryGetQueryNum(pure_str, '×', out var ___num, out var ___text))
            {
                sourceStr = ___text;
                num = ___num;
            }

            if (num < 1)
                num = 1;

            string query_source_text = sourceStr; //用于查询的特殊词库

            //检查是否命中特殊词库
            if (IsSafeSqlString(sourceStr))
            {
                if (NekoCore.Instance.MarketDB.TryGetCommonlyValue(sourceStr, out string _v))
                {
                    query_source_text = _v;
                    isHans = EVEUtil.IncludeChinese(query_source_text);
                }
            }


            queryStr.Add(query_source_text);
            GetQueryStr(ref query_source_text, ref queryStr);

            return true;
        }

        private static bool TryGetQueryNum(string text, char signStr, out long num, out string queryPropText)
        {
            //AppData.CQLog.Info("debug", "查询数量判断，标记符号：" + signStr);
            num = 1;
            queryPropText = string.Empty;
            if (text.IndexOf(signStr) != -1)
            {
                var str_arr = text.Split(signStr);
                if (str_arr.Length != 2)
                    return false;
                if (str_arr.Length == 2)
                {
                    if (long.TryParse(str_arr[0], out long _num) && !long.TryParse(str_arr[1], out _))
                    {
                        queryPropText = str_arr[1].Trim();
                        num = _num;
                        return true;
                    }
                    else if (long.TryParse(str_arr[1], out long __num) && !long.TryParse(str_arr[0], out _))
                    {
                        queryPropText = str_arr[0].Trim();
                        num = __num;
                        return true;
                    }
                }
            }
            return false;
        }

        public static void GetQueryStr(ref string sourceStr, ref List<string> queryStr)
        {
            if (sourceStr.StartsWith("海") && !sourceStr.StartsWith("海军型"))
            {
                if (sourceStr.EndsWith("级"))
                    queryStr.Add(sourceStr.Substring(1, sourceStr.Length - 1) + "海军型");
                else
                    queryStr.Add(sourceStr.Substring(1, sourceStr.Length - 1) + "级海军型");
            }

            if (!sourceStr.EndsWith("级") && !sourceStr.EndsWith("海军型"))
            {
                queryStr.Add(sourceStr + "级");
            }

            if (sourceStr.EndsWith("I") && !sourceStr.EndsWith(" I"))
                queryStr.Add(sourceStr.Substring(0, sourceStr.Length - 1) + " I");

            if (sourceStr.EndsWith("II") && !sourceStr.EndsWith(" II"))
                queryStr.Add(sourceStr.Substring(0, sourceStr.Length - 2) + " II");

            if (!sourceStr.EndsWith("I"))
                queryStr.Add(sourceStr + " I");

            if (sourceStr.ToLower().StartsWith("t2"))
                queryStr.Add(sourceStr.Substring(2, sourceStr.Length - 2) + " II");
        }

        public static bool ParseBindMsg(string msg, out string key, out string value)
        {
            key = default;
            value = default;
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".bind ")) return false;
            var _str = msg.Substring(5)?.Trim();
            if (string.IsNullOrEmpty(_str)) return false;
            var str_arr = _str.Split(',');
            if (str_arr.Length != 2) return false;
            key = str_arr[0].Trim();
            value = str_arr[1].Trim();
            return true;
        }

        public static bool ParseBindSuitMsg(string msg, out string key, out string value)
        {
            key = default;
            value = default;
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".bindsuit ")) return false;
            var _str = msg.Substring(9)?.Trim();
            if (string.IsNullOrEmpty(_str)) return false;
            var str_arr = _str.Split(',');
            if (str_arr.Length != 2) return false;
            key = str_arr[0].Trim();
            value = str_arr[1].Trim();
            return true;
        }

        public static bool ParseUnBindMsg(string msg, out string key)
        {
            key = default;
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".unbind ")) return false;
            var _str = msg.Substring(7)?.Trim();
            if (string.IsNullOrEmpty(_str)) return false;
            key = _str;
            return true;
        }

        public static bool ParseUnBindSuitMsg(string msg, out string key)
        {
            key = default;
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".unbind ")) return false;
            var _str = msg.Substring(11)?.Trim();
            if (string.IsNullOrEmpty(_str)) return false;
            key = _str;
            return true;
        }

        public static bool ParseAddAdmin(string msg, out long addQQ)
        {
            addQQ = -1;
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".addadmin ")) return false;
            var _str = msg.Substring(9)?.Trim();
            if (string.IsNullOrEmpty(_str)) return false;
            return long.TryParse(_str, out addQQ);
        }

        /// <summary>
        /// 解析 套装查询消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool ParseSuitMsg(string msg, out string sourceStr, out string query_text, ref bool isHans)
        {
            sourceStr = default;
            query_text = default;
            if (string.IsNullOrEmpty(msg)) return false;
            if (!msg.ToLower().StartsWith(".suit ")) return false;
            sourceStr = msg.Substring(5)?.Trim();
            if (string.IsNullOrEmpty(sourceStr)) return false;

            query_text = sourceStr; //用于查询的特殊词库
            //检查是否命中特殊词库
            if (NekoCore.Instance.MarketDB.TryGet_SuitCommonly_Value(sourceStr, out string _v))
            {
                query_text = _v;
                isHans = EVEUtil.IncludeChinese(query_text);
            }

            return true;
        }


        /// <summary>
        /// 检查是否有SQL注入风险
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSafeSqlString(string str)
        {
            return !Regex.IsMatch(str, @"[|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\=']");
        }

        /// <summary>
        /// 是否含有中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IncludeChinese(string str)
        {
            bool flag = false;
            foreach (var a in str)
            {
                if (a >= 0x4e00 && a <= 0x9fbb)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public static bool GetRandomBoolean()
        {
            try
            {
                Random rd = new Random();
                return (rd.Next() % 2 == 0);
            }
            catch { return false; }
        }

        /// <summary>
        /// 给定一个字符串"11,22,33"，尝试解析成List<long>
        /// </summary>
        public static bool TryGetLongListByString(string arr_str, out List<long> result)
        {
            if (string.IsNullOrEmpty(arr_str))
            {
                result = default;
                return false;
            }
            var str_arr = arr_str.Split(',');
            result = new List<long>();
            if(str_arr.Length == 0)
            {
                return false;
                //if (long.TryParse(arr_str, out var _l))
                //{
                //    result.Add(_l);
                //    return true;
                //}
                //else
                //    return false;
            }

            foreach(var item in str_arr)
            {
                if (!long.TryParse(item, out long _v))
                    return false;

                result.Add(_v);
            }
            return true;
        }

        public static string GetString(IEnumerable<long> long_enumberable)
        {
            string result = "";
            foreach(var item in long_enumberable)
            {
                result += item + ",";
            }
            return result.Substring(0, result.Length - 1);
        }

    }
}
