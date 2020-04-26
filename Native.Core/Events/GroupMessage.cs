using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Model;
using Native.Sdk.Cqp;
using Native.Core.Domain;

namespace Nekonya.Events
{
    public class GroupMessage : IGroupMessage
    {
        void IGroupMessage.GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            string msg = e.Message.Text.Trim();
            if(msg.ToLower().Equals(".jita"))
            {
                e.FromGroup.SendGroupMessage("您可使用如下命令查询商品物价:\nYou can use the following commands to check commodity prices\n\n.jita item_name");
                e.Handler = true;
                return;
            }
            if (msg.ToLower().StartsWith(".jita ")) //吉他市场查询
            {
                Jitas.Jitas.Instance.QueryFromGroup(e);
                e.Handler = true; //大概是中止pipeline的用途？
                return;
            }

            if(msg.ToLower().StartsWith(".bind ")) //绑定俗称词库
            {
                var result = Jitas.Jitas.Instance.BindCommonlyName(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                {
                    var at_msg = e.FromQQ.CQCode_At();
                    e.FromGroup.SendGroupMessage(at_msg," ", result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().StartsWith(".unbind ")) //移除俗称词库
            {
                var result = Jitas.Jitas.Instance.RemoveCommonlyName(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                {
                    var at_msg = e.FromQQ.CQCode_At();
                    e.FromGroup.SendGroupMessage(at_msg," ", result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().StartsWith(".addadmin ")) //移除俗称词库
            {
                var result = Jitas.Jitas.Instance.AddAdmin(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                {
                    var at_msg = e.FromQQ.CQCode_At();
                    e.FromGroup.SendGroupMessage(at_msg," ", result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().StartsWith(".suit ")) //套装查询
            {
                var result = Jitas.Jitas.Instance.QuerySuit(msg);
                if (!string.IsNullOrEmpty(result))
                {
                    e.FromGroup.SendGroupMessage(result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().StartsWith(".bindsuit ")) //绑定套装俗称词库
            {
                var result = Jitas.Jitas.Instance.Bind_SuitCommonly_Name(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                {
                    var at_msg = e.FromQQ.CQCode_At();
                    e.FromGroup.SendGroupMessage(at_msg, " ",result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().StartsWith(".unbindsuit ")) //移除套装俗称词库
            {
                var result = Jitas.Jitas.Instance.Remove_SuitCommonly_Name(msg, e.FromQQ);
                if (!string.IsNullOrEmpty(result))
                {
                    var at_msg = e.FromQQ.CQCode_At();
                    e.FromGroup.SendGroupMessage(at_msg, " ", result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().Equals(".plex")) //月卡查询
            {
                var result = Jitas.Jitas.Instance.QueryPLEX();
                if (!string.IsNullOrEmpty(result))
                {
                    e.FromGroup.SendGroupMessage(result);
                }
                e.Handler = true;
                return;
            }

            if (msg.ToLower().Equals(".help")) //帮助
            {
                e.FromGroup.SendGroupMessage(Jitas.Jitas.Instance.GetHelpText());
                e.Handler = true;
                return;
            }
        }
    }
}
