using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Native.Core.Domain;
using System.IO;

namespace Nekonya.DBs
{
    public class EVEGameDB
    {
        public string DBConnectString { get; private set; }
        private SQLiteConnection cn;

        public EVEGameDB()
        {
            this.DBConnectString = $"Data Source={Path.Combine(AppData.CQApi.AppDirectory, "eve_static.db")}";
            AppData.CQLog.Info("Debug", "Sqlite Connect Str:" + this.DBConnectString);
            cn = new SQLiteConnection(this.DBConnectString);
            cn.Open();
            
        }

        public bool TryGetIdByName_CN(string name,out long id, out string cn_name, out string en_name)
        {
            lock (this)
            {
                using(var cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"SELECT * FROM Props WHERE Name_CN=\"{name}\" COLLATE NOCASE";
                    var reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        id = -1;
                        cn_name = null;
                        en_name = null;
                        return false;
                    }
                    //AppData.CQLog.Info("debug", "Field Count:" + reader.FieldCount);
                    //AppData.CQLog.Info("debug", "Step Count" + reader.StepCount);
                    reader.Read();
                    id = reader.GetInt64(0);
                    cn_name = reader.GetString(3);
                    en_name = reader.GetString(2);
                    return true;
                }
            }
        }

        public bool TryGetIdByName_EN(string name, out long id, out string cn_name, out string en_name)
        {
            lock (this)
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"SELECT * FROM Props WHERE Name=\"{name}\" COLLATE NOCASE";
                    var reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        id = -1;
                        cn_name = null;
                        en_name = null;
                        return false;
                    }
                    //AppData.CQLog.Info("debug", "Field Count:" + reader.FieldCount);
                    //AppData.CQLog.Info("debug", "Step Count" + reader.StepCount);
                    reader.Read();
                    id = reader.GetInt64(0);
                    cn_name = reader.GetString(3);
                    en_name = reader.GetString(2);
                    return true;
                }
            }
        }
    
        public bool TryGetNameByID(long id, out string cn_name, out string en_name)
        {
            lock (this)
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"SELECT * FROM Props WHERE ID={id}";
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        cn_name = reader.GetString(3);
                        en_name = reader.GetString(2);
                        return true;
                    }
                    cn_name = string.Empty;
                    en_name = string.Empty;
                    return false;
                }
            }
        }

        /// <summary>
        /// 中文 模糊查询
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> LIKEQuery_CN(string name)
        {
            var result = new List<string>();
            if(string.IsNullOrEmpty(name))
            {
                return result;
            }

            lock (this)
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"SELECT * FROM Props WHERE Name_CN LIKE '%{name}%'";
                    var reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        return result;
                    }
                    int counter = 0;
                    while (reader.Read())
                    {
                        counter++;
                        result.Add(reader.GetString(3));
                        if (counter >= 5)
                            break;
                    }
                    
                    return result;
                }
            }
        }

        /// <summary>
        /// 英文 模糊查询
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<string> LIKEQuery_EN(string name)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(name))
            {
                return result;
            }

            lock (this)
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"SELECT * FROM Props WHERE Name LIKE '%{name}%' COLLATE NOCASE";
                    var reader = cmd.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        return result;
                    }
                    int counter = 0;
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(3));
                        counter++;
                        if (counter >= 5)
                            break;
                    }
                    return result;
                }
            }
        }

    }
}
