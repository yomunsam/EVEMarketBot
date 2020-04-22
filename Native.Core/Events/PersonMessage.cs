using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.EventArgs;

namespace Nekonya.Events
{
    /// <summary>
    /// 私信消息
    /// </summary>
    public class PersonMessage : IPrivateMessage
    {
        public void PrivateMessage(object sender, CQPrivateMessageEventArgs e)
        {
            string msg = e.Message.Text;
            if (msg.ToLower().StartsWith(".jita ")) //吉他市场查询
            {
                Jitas.Jitas.Instance.QueryFromPersion(e);
                e.Handler = true; //大概是中止pipeline的用途？
                return;
            }

            if (msg.ToLower().StartsWith(".bind ")) //绑定俗称词库
            {
                var result = Jitas.Jitas.Instance.BindCommonlyName(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                {
                    e.FromQQ.SendPrivateMessage(result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().StartsWith(".unbind ")) //移除俗称词库
            {
                var result = Jitas.Jitas.Instance.RemoveCommonlyName(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                    e.FromQQ.SendPrivateMessage(result);
                e.Handler = true;
                return;
            }

            if (msg.ToLower().StartsWith(".addadmin"))
            {
                var result = Jitas.Jitas.Instance.AddAdmin(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                    e.FromQQ.SendPrivateMessage(result);
                e.Handler = true;
                return;
            }
        }
    }
}
