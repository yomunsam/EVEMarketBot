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
    public class MarketDB
    {
        public string DBConnectString { get; private set; }
        private SQLiteConnection cn;

        public MarketDB()
        {
            this.DBConnectString = $"Data Source={Path.Combine(AppData.CQApi.AppDirectory, "eve_market.db")}";
            AppData.CQLog.Info("Debug", "Sqlite Connect Str:" + this.DBConnectString);
            cn = new SQLiteConnection(this.DBConnectString);
            cn.Open();

            //检查表
            using(var cmd = new SQLiteCommand())
            {
                //通俗名称表
                cmd.Connection = cn;
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS commonly " +
                    "(key TEXT PRIMARY KEY NOT NULL, " +
                    "value TEXT NOT NULL)";
                cmd.ExecuteNonQuery();
            }
        }

        public bool TryGetValue(string key,out string value)
        {
            using(var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM commonly WHERE key='{key}'";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    value = reader.GetString(1);
                    return true;
                }
                value = default;
                return false;
            }
        }

        public void DeleteKey(string key)
        {
            using(var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"DELETE FROM commonly WHERE key='{key}'";
                cmd.ExecuteNonQuery();
            }
        }

        public void AddRecord(string key, string value)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"REPLACE INTO commonly (key,value) VALUES(@key,@value)";
                cmd.Parameters.Add("key", DbType.String).Value = key;
                cmd.Parameters.Add("value", DbType.String).Value = value;
                cmd.ExecuteNonQuery();
            }
        }

    }
}
