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
            string msg = e.Message.Text;
            if (msg.StartsWith(".jita"))
            {
                //try
                //{
                //    Jitas.Jitas.Instance.QueryFromGroup(e);
                //}
                //catch (Exception ex)
                //{
                //    e.FromGroup.SendGroupMessage("内部异常\n", ex.Message);
                //}
                Jitas.Jitas.Instance.QueryFromGroup(e);
                e.Handler = true; //大概是中止pipeline的用途？
            }
        }
    }
}
