using Native.Sdk.Cqp.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekonya.DBs;
using Native.Tool.Http;
using Newtonsoft.Json;
using Native.Sdk.Cqp.Model;

namespace Nekonya.Jitas
{
    public class Jitas
    {
        private static object _lock_obj = new object();
        private static Jitas _instance;
        public static Jitas Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock_obj)
                    {
                        if (_instance == null)
                        {
                            _instance = new Jitas();
                        }
                    }
                }
                return _instance;
            }
        }

        private EVEGameDB EVEDB = NekoCore.Instance.EVEDB;
        private MarketDB MarketDB = NekoCore.Instance.MarketDB;
        

        public void QueryFromGroup(CQGroupMessageEventArgs msg)
        {
            string message = msg.Message.Text;
            bool isHans = EVEUtil.IncludeChinese(message);
            if (!EVEUtil.IsSafeSqlString(message))
            {
                msg.FromGroup.SendGroupMessage(isHans ? $"您要查询的内容不安全：{message}" : $"The content you want to query is not secure: {message}");
                return;
            }
            if(EVEUtil.ParseJitaMsg(msg.Message.Text,out var source, out List<string> queryText))
            {
                //是否命中特殊词库
                if (this.MarketDB.TryGetValue(source,out string _v))
                {
                    queryText.Clear();
                    queryText.Add(_v);
                }

                long _id = -1;
                bool flag = false;
                string final_name_cn = string.Empty;
                string final_name_en = string.Empty;
                foreach(var item in queryText)
                {
                    if (EVEDB.TryGetIdByName_CN(item, out _id, out final_name_cn, out final_name_en)) 
                    {

                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    if (source.Length >= 2)
                    {
                        if (isHans)
                        {
                            List<string> fuzzy = EVEDB.LIKEQuery_CN(source);
                            if (fuzzy.Count == 1)
                            {
                                if (!EVEDB.TryGetIdByName_CN(fuzzy[0], out _id, out final_name_cn, out final_name_en))
                                {
                                    flag = true;
                                }
                            }
                            else if(fuzzy.Count > 1)
                            {
                                var __re_mes = "您是否要查询以下物品：";
                                foreach(var item in fuzzy)
                                {
                                    __re_mes += "\n" + item;
                                }
                                msg.FromGroup.SendGroupMessage(__re_mes);
                                return;
                            }
                        }
                        else
                        {
                            List<string> fuzzy = EVEDB.LIKEQuery_EN(source);
                            if (fuzzy.Count == 1)
                            {
                                if (!EVEDB.TryGetIdByName_EN(fuzzy[0], out _id, out final_name_cn, out final_name_en))
                                {
                                    flag = true;
                                }
                            }
                            else if (fuzzy.Count > 1)
                            {
                                var __re_mes = "Do you want to check the following items：";
                                foreach (var item in fuzzy)
                                {
                                    __re_mes += "\n" + item;
                                }
                                msg.FromGroup.SendGroupMessage(__re_mes);
                                return;
                            }
                        }
                    }
                }
                if (!flag)
                {
                    msg.FromGroup.SendGroupMessage(isHans ? $"未找到相关物品: {source}" : $"The content you want to query is not: {source}");
                    return;
                }
                //向EVE查询物价
                string url = $"https://www.ceve-market.org/api/market/region/10000002/type/{_id}.json";
                string url_world_server = $"https://www.ceve-market.org/tqapi/market/region/10000002/type/{_id}.json";
                var http_result = HttpWebClient.Get(url, true);
                var http_result_ws = HttpWebClient.Get(url_world_server, true); //世界服
                var http_text = Encoding.UTF8.GetString(http_result);
                var http_text_ws = Encoding.UTF8.GetString(http_result_ws);

                string result = "";
                bool flag_2 = false;
                try
                {
                    var model = JsonConvert.DeserializeObject<JitaJsonModel>(http_text);
                    result += $"吉他 - {final_name_cn}\n最低售价：{string.Format("{0:N2}", model.sell.min)} ISK\n最高收单：{string.Format("{0:N2}", model.buy.max)} ISK\n";
                    flag_2 = true;
                }
                catch
                {
                    result += "查询出错。\n";
                }

                try
                {
                    var model_ws = JsonConvert.DeserializeObject<JitaJsonModel>(http_text_ws);
                    result += $"\nJita - {final_name_en}\nMin Sell: {string.Format("{0:N2}", model_ws.sell.min)} ISK\nMax Buy: {string.Format("{0:N2}", model_ws.buy.max)} ISK";
                    flag_2 = true;
                }
                catch
                {
                    result += "\n查询出错。";
                }

                if (!flag_2)
                {
                    msg.FromGroup.SendGroupMessage("查询出错");
                }
                else
                {
                    msg.FromGroup.SendGroupMessage(result);
                }
            }
        }


        public void QueryFromPersion(CQPrivateMessageEventArgs msg)
        {

        }

        public string BindCommonlyName(string msg, QQ fromQQ) //绑定俗称词库
        {
            bool isHans = EVEUtil.IncludeChinese(msg);
            if(EVEUtil.ParseBindMsg(msg, out var key ,out var value))
            {
                if (!this.BindCommonlyAuthorize(fromQQ))
                    return isHans ? "您没有权限进行该操作" : "You do not have permission to do this.";

                //防注入
                if (!EVEUtil.IsSafeSqlString(key) || !EVEUtil.IsSafeSqlString(value))
                    return isHans ? $"输入内容不安全: {key} | {value}" : $"Input is not secure: {key} | {value}";

                //执行绑定
                this.MarketDB.AddRecord(key, value);
                return isHans ? "完成" : "Finished";
            }
            else
            {
                return isHans ? "无效的绑定语句，请使用如下语句绑定俗称词库：\n.bind 俗称,标准名称" : "For invalid binding statements, use the following statement to bind the common name thesaurus:\n.bind common_name,standard_name";
            }
        }

        public string RemoveCommonlyName(string msg, QQ fromQQ) //删除俗称词库的key
        {
            bool isHans = EVEUtil.IncludeChinese(msg);
            if(EVEUtil.ParseUnBindMsg(msg,out var key))
            {
                if (!this.BindCommonlyAuthorize(fromQQ))
                    return isHans ? "您没有权限进行该操作" : "You do not have permission to do this.";

                //防注入
                if (!EVEUtil.IsSafeSqlString(key))
                    return isHans ? $"输入内容不安全: {key}" : $"Input is not secure: {key}";

                //执行删除
                this.MarketDB.DeleteKey(key);
                return isHans ? "完成" : "Finished";
            }
            else
            {
                return isHans ? "无效的移除绑定语句，请使用如下语句绑定俗称词库：\n.unbind 俗称" : "For invalid unbinding statements, use the following statement to bind the common name thesaurus:\n.unbind common_name";
            }

        }

        public string AddAdmin(string msg, QQ fromQQ)
        {
            if(EVEUtil.ParseAddAdmin(msg,out long add_qq))
            {
                //鉴权
                if (!this.IsMasterQQ(fromQQ))
                    return "无权进行该操作";

                lock (NekoCore.Instance.Config)
                {
                    //添加
                    if (NekoCore.Instance.Config.AdminQQ == null)
                        NekoCore.Instance.Config.AdminQQ = new List<long>();
                    if (!NekoCore.Instance.Config.AdminQQ.Contains(add_qq))
                    {
                        NekoCore.Instance.Config.AdminQQ.Add(add_qq);
                        NekoCore.Instance.SaveConfig();
                    }
                }
                return "完成";
            }
            else
            {
                return "无效的语句，请使用如下格式添加市场姬管理：\n.addAdmin QQ号";
            }
        }

        private bool BindCommonlyAuthorize(QQ fromQQ)
        {
            long qq_id = fromQQ.Id;
            if (NekoCore.Instance.Config.MasterQQ.Equals(qq_id))
                return true;

            if (NekoCore.Instance.Config.AdminQQ != null && NekoCore.Instance.Config.AdminQQ.Contains(qq_id))
                return true;

            return false;
        }

        private bool IsMasterQQ(QQ fromQQ)
        {
            return NekoCore.Instance.Config.MasterQQ.Equals(fromQQ.Id);
        }
    }
}
