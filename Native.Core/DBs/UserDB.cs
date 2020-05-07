using Native.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekonya.DBs
{
    public class UserDB
    {
        public string DBConnectString { get; private set; }
        private SQLiteConnection cn;

        public UserDB()
        {
            //if (!NekoCore.Instance.Config.Users.EnableUserSystem) return; //NekoCore在初始化阶段初始化了UserDB，所以这里构造函数里调用NekoCore会导致死循环

            this.DBConnectString = $"Data Source={Path.Combine(AppData.CQApi.AppDirectory, "users.db")}";
            AppData.CQLog.Info("Debug", "Sqlite Connect Str:" + this.DBConnectString);
            cn = new SQLiteConnection(this.DBConnectString);
            cn.Open();

            //检查表
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                //用户表
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS user " +
                    "(userid INTEGER PRIMARY KEY NOT NULL, " + //qq号
                    "name TEXT, " + //用户显示名
                    "groups TEXT)"; //用户所在群组
                cmd.ExecuteNonQuery();

                //.jita查询记录
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS query_record " +
                    "(userid INTEGER NOT NULL," +           //发起查询的用户id
                    "from_group INTEGER NOT NULL," +        //发起查询的群，以-1代表非来自群组的查询
                    "nickname_group TEXT," +                 //查询者在当前群的nickname
                    "query_content TEXT NOT NULL," +        //发起查询的文本内容
                    "query_prop_id INTEGER NOT NULL," +     //查询的道具id，如果是无效查询则为-1
                    "datetime DATETIME NOT NULL" +          //查询发起时间
                    ")"; 
                cmd.ExecuteNonQuery();

            }
        }


        public void AddRecord(long qq_id, string qq_name, string query_content, string name_in_group = "", long group_id = -1, long prop_id = -1)
        {
            if (!NekoCore.Instance.Config.Users.EnableUserSystem) return;
            //用户数据
            AddOrUpdateUSerInfo(qq_id, qq_name, group_id);
            //记录
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = "INSERT INTO query_record(userid,from_group,nickname_group,query_content,query_prop_id,datetime) VALUES(@userid,@from_group,@nickname_group,@query_content,@query_prop_id,@datetime)";
                cmd.Parameters.Add("userid", DbType.Int64).Value = qq_id;
                cmd.Parameters.Add("from_group", DbType.Int64).Value = group_id;
                cmd.Parameters.Add("nickname_group", DbType.String).Value = name_in_group;
                cmd.Parameters.Add("query_content", DbType.String).Value = query_content;
                cmd.Parameters.Add("query_prop_id", DbType.Int64).Value = prop_id;
                cmd.Parameters.Add("datetime", DbType.DateTime).Value = System.DateTime.UtcNow;

                cmd.ExecuteNonQuery();
            }
        }

        public void AddOrUpdateUSerInfo(long qq_id, string qq_name, long group_id = -1)
        {
            if (!NekoCore.Instance.Config.Users.EnableUserSystem) return;
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                cmd.CommandText = $"SELECT * FROM user WHERE userid = {qq_id}";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                var reader = cmd.ExecuteReader();
                bool flag = false;
                string group_str = "";
                if (reader.Read())
                {
                    //有数据,检查是否需要更新数据
                    if (reader.GetString(1) != qq_name)
                        flag = true;

                    //AppData.CQLog.Debug("Users", "解析群组数据");
                    group_str = reader.GetString(2) ?? "";
                    //AppData.CQLog.Debug("Users", "取到group 字符串:", group_str);
                    if (group_id != -1)
                    {
                        if(EVEUtil.TryGetLongListByString(group_str,out var group_list))
                        {
                            if (!group_list.Contains(group_id))
                            {
                                group_list.Add(group_id);
                                group_str = EVEUtil.GetString(group_list);
                                flag = true;
                            }
                        }
                        else
                        {
                            flag = true;
                            group_str = group_id.ToString();
                            //AppData.CQLog.Debug("Users", "解析group字符串失败");
                        }
                    }
                }
                else
                {
                    flag = true;
                    group_str = group_id == -1 ? "" : group_id.ToString();
                }

                reader.Close();

                if (flag)
                {
                    cmd.CommandText = "REPLACE INTO user (userid,name,groups) VALUES(@userid,@name,@groups)";
                    cmd.Parameters.Add("userid", DbType.Int64).Value = qq_id;
                    cmd.Parameters.Add("name", DbType.String).Value = qq_name;
                    cmd.Parameters.Add("groups", DbType.String).Value = group_str;
                    cmd.ExecuteNonQuery();
                }
            }

        }


    }
}
