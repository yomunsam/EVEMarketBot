using Native.Sdk.Cqp.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekonya.DBs;
using Native.Tool.Http;
using Newtonsoft.Json;

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

        

        public void QueryFromGroup(CQGroupMessageEventArgs msg)
        {
            if(EVEUtil.ParseJitaMsg(msg.Message.Text,out var source, out List<string> queryText))
            {
                
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
                    msg.FromGroup.SendGroupMessage("未找到相关物品:" + source);
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

    }
}
