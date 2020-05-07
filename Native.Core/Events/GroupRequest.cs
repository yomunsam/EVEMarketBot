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
    public class GroupRequest : IGroupAddRequest
    {
        public void GroupAddRequest(object sender, CQGroupAddRequestEventArgs e)
        {
            if (e.SubType == Native.Sdk.Cqp.Enum.CQGroupAddRequestType.RobotBeInviteAddGroup) //被邀请入群
            {
                switch (NekoCore.Instance.Config.JoinGroups.HandleType)
                {
                    case Config.EVEMarketConfig.InvitedHandle.Accept:
                        e.Request.SetGroupAddRequest(Native.Sdk.Cqp.Enum.CQGroupAddRequestType.RobotBeInviteAddGroup, Native.Sdk.Cqp.Enum.CQResponseType.PASS);
                        break;
                    case Config.EVEMarketConfig.InvitedHandle.Refuse:
                        e.Request.SetGroupAddRequest(Native.Sdk.Cqp.Enum.CQGroupAddRequestType.RobotBeInviteAddGroup, Native.Sdk.Cqp.Enum.CQResponseType.FAIL, NekoCore.Instance.Config.JoinGroups.RefuseMessage ?? "");
                        break;

                }
            }
            
        }
    }
}
