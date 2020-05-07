using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekonya.Config
{
    public class EVEMarketConfig
    {
        /// <summary>
        /// 最高管理权限的QQ
        /// </summary>
        public long MasterQQ;

        /// <summary>
        /// 全局管理权限的QQ
        /// </summary>
        public List<long> AdminQQ;

        public UserSystem Users = new UserSystem();

        public JoinGroup JoinGroups = new JoinGroup();

        /// <summary>
        /// 加入群组
        /// </summary>
        [Serializable]
        public class JoinGroup
        {
            public InvitedHandle HandleType = InvitedHandle.Master;

            public string RefuseMessage = "";
        }

        /// <summary>
        /// 被邀请时候的处理
        /// </summary>
        [Serializable]
        public enum InvitedHandle
        {
            /// <summary>
            /// 口头瓦鲁
            /// </summary>
            Refuse = 0,
            /// <summary>
            /// 直接通过
            /// </summary>
            Accept = 1,

            /// <summary>
            /// 需要Master通过
            /// </summary>
            Master = 2,

            /// <summary>
            /// Master或者Admin审核通过
            /// </summary>
            AdminOrMaster = 3,
        }

        /// <summary>
        /// 用户系统
        /// </summary>
        [Serializable] 
        public class UserSystem
        {
            /// <summary>
            /// 启用用户系统
            /// </summary>
            public bool EnableUserSystem = true;
        }

    }
}
