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
            msg.FromGroup.SendGroupMessage(this.QueryJita(msg.Message.Text.Trim()));
        }


        public void QueryFromPersion(CQPrivateMessageEventArgs msg)
        {
            msg.FromQQ.SendPrivateMessage(this.QueryJita(msg.Message.Text.Trim()));
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
                this.MarketDB.AddCommonlyRecord(key, value);
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
                this.MarketDB.DeleteCommonlyKey(key);
                return isHans ? "完成" : "Finished";
            }
            else
            {
                return isHans ? "无效的移除绑定语句，请使用如下语句绑定俗称词库：\n.unbind 俗称" : "For invalid unbinding statements, use the following statement to bind the common name thesaurus:\n.unbind common_name";
            }

        }

        /// <summary>
        /// 套装查询
        /// </summary>
        /// <param name="message"></param>
        public string QuerySuit(string message)
        {
            bool isHans = EVEUtil.IncludeChinese(message);

            //防SQL注入
            if(!EVEUtil.IsSafeSqlString(message))
                return (isHans ? $"您要查询的内容不安全：{message}" : $"The content you want to query is not secure: {message}");

            //解析命令
            if (EVEUtil.ParseSuitMsg(message, out var source, out var queryText, ref isHans))
            {
                if(this.TryQuerySuit(queryText, isHans, out var suit_item,out var errcode ,out var name_hans, out var name_en))
                {
                    //查询到了，检查套装信息是否有效
                    if(errcode != -1)
                    {
                        if(errcode == 1)
                        {
                            return isHans ? "内部错误, 套装信息配置出错" : "Internal error, suit information configuration error";
                        }
                        else
                        {
                            return isHans ? "内部错误" : "Internal Error";
                        }
                    }

                    //挨个获取套装信息
                    return Get_Suit_Price_String(suit_item, isHans, name_hans, name_en);
                }
                else
                {
                    //查询失败
                    return isHans ? $"未找到套装信息: {queryText}" : $"Suit not found: {queryText}";
                }

            }
            else
                return isHans ? $"无效的查询语句: {message}" : $"Query Invalid: {message}";
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

        public string Bind_SuitCommonly_Name(string msg, QQ fromQQ) //绑定套装俗称词库
        {
            bool isHans = EVEUtil.IncludeChinese(msg);
            if (EVEUtil.ParseBindSuitMsg(msg, out var key, out var value))
            {
                if (!this.BindCommonlyAuthorize(fromQQ))
                    return isHans ? "您没有权限进行该操作" : "You do not have permission to do this.";

                //防注入
                if (!EVEUtil.IsSafeSqlString(key) || !EVEUtil.IsSafeSqlString(value))
                    return isHans ? $"输入内容不安全: {key} | {value}" : $"Input is not secure: {key} | {value}";

                //执行绑定
                this.MarketDB.Add_SuitCommonly_Record(key, value);
                return isHans ? "完成" : "Finished";
            }
            else
            {
                return isHans ? "无效的绑定语句，请使用如下语句绑定俗称词库：\n.bindSuit 俗称,标准名称" : "For invalid binding statements, use the following statement to bind the common name thesaurus:\n.bindSuit common_name,standard_name";
            }
        }

        public string Remove_SuitCommonly_Name(string msg, QQ fromQQ) //删除套装俗称词库的key
        {
            bool isHans = EVEUtil.IncludeChinese(msg);
            if (EVEUtil.ParseUnBindSuitMsg(msg, out var key))
            {
                if (!this.BindCommonlyAuthorize(fromQQ))
                    return isHans ? "您没有权限进行该操作" : "You do not have permission to do this.";

                //防注入
                if (!EVEUtil.IsSafeSqlString(key))
                    return isHans ? $"输入内容不安全: {key}" : $"Input is not secure: {key}";

                //执行删除
                this.MarketDB.Delete_SuitCommonly_Key(key);
                return isHans ? "完成" : "Finished";
            }
            else
            {
                return isHans ? "无效的移除绑定语句，请使用如下语句绑定俗称词库：\n.unbindSuit 俗称" : "For invalid unbinding statements, use the following statement to bind the common name thesaurus:\n.unbindSuit common_name";
            }

        }


        /// <summary>
        /// 吉他查询，私有方法总入口
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string QueryJita(string message)
        {
            bool isHans = EVEUtil.IncludeChinese(message);

            //防注入
            if (!EVEUtil.IsSafeSqlString(message))
                return (isHans ? $"您要查询的内容不安全：{message}" : $"The content you want to query is not secure: {message}");

            //解析命令
            if (EVEUtil.ParseJitaMsg(message, out var source, out List<string> queryText, ref isHans))
            {
                long prop_id;
                string prop_name_hans;
                string prop_name_en;

                //检查数据库
                if(!this.TryQueryTexts(ref queryText, isHans, out prop_id, out prop_name_hans, out prop_name_en))
                {
                    //未找到，准备混合搜索
                    if(this.TryFuzzyQuery(source,isHans,out string fuzzy_result, out bool _continue, out prop_id, out prop_name_hans, out prop_name_en))
                    {
                        if (!_continue)
                            return fuzzy_result;
                    }
                    else // 混合搜索没找到
                        return isHans ? $"未找到相关物品: {source}" : $"The content you want to query is not: {source}";
                }

                //可以继续执行
                PriceInfo? world_server_price = GetPriceInfo(prop_id, true);
                PriceInfo? cn_server_price = GetPriceInfo(prop_id, false);

                return GetPriceString(world_server_price, cn_server_price, isHans, prop_name_hans, prop_name_en);
            }
            else
                return isHans ? $"无效的查询语句: {message}" : $"Query Invalid: {message}";
        }

        /// <summary>
        /// 尝试模糊查询
        /// </summary>
        /// <param name="queryContent">查询内容</param>
        /// <param name="result_text">查询结果文本，查询成功会返回</param>
        /// <param name="_continue">是否继续往下执行</param>
        /// <returns></returns>
        private bool TryFuzzyQuery(string queryContent, bool isHans, out string result_text, out bool _continue, out long id, out string name_hans, out string name_en)
        {
            result_text = string.Empty;
            _continue = false;
            id = -1;
            name_hans = string.Empty;
            name_en = string.Empty;

            int length_min_limit = isHans ? 2 : 5; //满足模糊查询的最低长度
            if (queryContent.Length < length_min_limit) return false; //不满足查询条件
            List<string> fuzzy = isHans ? EVEDB.LIKEQuery_CN(queryContent) : EVEDB.LIKEQuery_EN(queryContent);
            if (fuzzy.Count == 0) return false;
            if(fuzzy.Count == 1)
            {
                //直接用唯一的模糊搜索结果当成最终结果
                if(isHans)
                {
                    if(EVEDB.TryGetIdByName_CN(fuzzy[0],out id, out name_hans,out name_en))
                    {
                        _continue = true;
                        return true;
                    }
                }
                else
                {
                    if(EVEDB.TryGetIdByName_EN(fuzzy[0],out id ,out name_hans, out name_en))
                    {
                        _continue = true;
                        return true;
                    }
                }    
            }
            else if (fuzzy .Count > 1)
            {
                result_text = isHans ? "您是否要查询以下物品：" : "Do you want to check the following items：";
                foreach(var item in fuzzy)
                {
                    result_text += "\n" + item;
                }
                _continue = false;
                return true;
            }

            return false;
        }

        private bool TryQueryTexts(ref List<string> queryText, bool isHans, out long id, out string hans_name, out string en_name)
        {
            foreach(var item in queryText)
            {
                if(isHans)
                {
                    if (this.EVEDB.TryGetIdByName_CN(item, out id, out hans_name, out en_name))
                        return true;
                }
                else
                {
                    if (this.EVEDB.TryGetIdByName_EN(item, out id, out hans_name, out en_name))
                        return true;
                }
            }
            id = -1;
            hans_name = default;
            en_name = default;
            return false;
        }

        /// <summary>
        /// 将获取到的售价转化为字符串
        /// </summary>
        /// <param name="world_server">世界服信息</param>
        /// <param name="cn_server">国服信息</param>
        /// <param name="isHans">查询语言是否为汉语（决定显示结果的次序，使用中文查询时将国服市场价格排在前面）</param>
        /// <returns></returns>
        private string GetPriceString(PriceInfo? world_server, PriceInfo? cn_server, bool isHans, string prop_name_hans, string prop_name_en)
        {
            string getHansString(PriceInfo info) => $"吉他 - {prop_name_hans}\n最低售价：{string.Format("{0:N2}", info.MinSell)} ISK\n最高收单：{string.Format("{0:N2}", info.MaxBuy)} ISK";
            string getEnString(PriceInfo info) => $"Jita - {prop_name_en}\nMin Sell: {string.Format("{0:N2}", info.MinSell)} ISK\nMax Buy: {string.Format("{0:N2}", info.MaxBuy)} ISK";

            if (world_server == null && cn_server == null)
                return isHans ? $"查询出错，内部错误或网络错误：{prop_name_hans}" : $"There is an error in the query, possibly an internal error or a network error: {prop_name_en}";

            return isHans ? $"{(cn_server == null ? "国服查询错误" : getHansString(cn_server.Value))}\n\n{(world_server == null ? "世界服查询错误" : getEnString(world_server.Value))}"
                : $"{(world_server == null ? "Query world server error." : getEnString(world_server.Value))}\n\n{(cn_server == null ? "Query cn server error" : getHansString(cn_server.Value))}";
        }

        /// <summary>
        /// 获取价格信息
        /// </summary>
        /// <param name="prop_id">道具ID</param>
        /// <param name="world_server">世界服</param>
        /// <returns></returns>
        private PriceInfo? GetPriceInfo(long prop_id, bool world_server)
        {
            try
            {
                string url = world_server ? $"https://www.ceve-market.org/tqapi/market/region/10000002/type/{prop_id}.json" : $"https://www.ceve-market.org/api/market/region/10000002/type/{prop_id}.json";
                var http_resp = HttpWebClient.Get(url, true);
                var json_text = Encoding.UTF8.GetString(http_resp);
                var json_model = JsonConvert.DeserializeObject<JitaJsonModel>(json_text);
                return new PriceInfo()
                {
                    MaxBuy = json_model.buy.max,
                    MinSell = json_model.sell.min,
                };
            }
            catch
            {
                return null;
            }
        }

        private string Get_Suit_Price_String(List<long> ids,bool isHans, string suit_name_hans, string suit_name_en)
        {
            string result = isHans ? $"套装：{suit_name_hans}" : $"Suit: {suit_name_en}";
            foreach(var item in ids)
            {
                if(this.EVEDB.TryGetNameByID(item,out var cname, out var ename))
                {
                    var ws_info = this.GetPriceInfo(item, true);
                    var cs_info = this.GetPriceInfo(item, false);
                    string GetItemText()
                    {
                        if (isHans)
                        {
                            return $"[{cname}]\n" +
                                $"{(cs_info == null ? "国服售价获取失败" : $"晨曦：{string.Format("{0:N2}", cs_info?.MinSell)} ISK / {string.Format("{0:N2}", cs_info?.MaxBuy)} ISK")}\n" +
                                $"{(ws_info == null?"世界服售价获取失败": $"宁静：{string.Format("{0:N2}", ws_info?.MinSell)} ISK / {string.Format("{0:N2}", ws_info?.MaxBuy)} ISK")}";
                        }
                        else
                        {
                            return $"[{ename}]\n" +
                                $"{(ws_info == null ? "Failed to get the world service price." : $"Tranquility：{string.Format("{0:N2}", ws_info?.MinSell)} ISK / {string.Format("{0:N2}", ws_info?.MaxBuy)} ISK")}\n" +
                                $"{(cs_info == null ? "Failed to get the cn server price." : $"CN Server：{string.Format("{0:N2}", cs_info?.MinSell)} ISK\n / {string.Format("{0:N2}", cs_info?.MaxBuy)} ISK")}";
                        }
                    }
                    result += $"\n{GetItemText()}";
                }
            }
            return result;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryContent"></param>
        /// <param name="suit_items"></param>
        /// <param name="err_code">错误码：-1，无错误， 1，配置错误导致解析失败</param>
        /// <returns></returns>
        private bool TryQuerySuit(string queryContent, bool isHans, out List<long> suit_items , out int err_code, out string name_hans, out string name_en)
        {
            err_code = -1;
            suit_items = new List<long>();

            string str_suit_item = string.Empty;

            if (isHans)
            {
                if(!this.MarketDB.TryGetSuitInfo_CN(queryContent,out str_suit_item, out name_hans, out name_en))
                {
                    return false;
                }
            }
            else
            {
                if (!this.MarketDB.TryGetSuitInfo_EN(queryContent, out str_suit_item, out name_hans, out name_en))
                    return false;
            }

            //解析字符串
            if(string.IsNullOrEmpty(str_suit_item))
            {
                err_code = 1;
                return true;
            }

            var str_arr = str_suit_item.Split(',');
            foreach(var item in str_arr)
            {
                if(long.TryParse(item,out long _l))
                {
                    suit_items.Add(_l);
                }
                else
                {
                    err_code = 1;
                    return true;
                }
            }

            return true;
        }
    }
}
