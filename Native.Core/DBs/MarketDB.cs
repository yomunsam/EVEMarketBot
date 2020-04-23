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
                cmd.Connection = cn;
                //通俗名称词库表
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS commonly " +
                    "(key TEXT PRIMARY KEY NOT NULL, " +
                    "value TEXT NOT NULL)";
                cmd.ExecuteNonQuery();

                //套装表
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS suit " +
                    "(name_hans TEXT NOT NULL," +
                    "name_en TEXT NOT NULL," +
                    "suit_items TEXT NOT NULL)";
                cmd.ExecuteNonQuery();

                //套装通俗名称词库表
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS commonly_suit " +
                    "(key TEXT PRIMARY KEY NOT NULL, " +
                    "value TEXT NOT NULL)";
                cmd.ExecuteNonQuery();
            }
        }

        public bool TryGetCommonlyValue(string key,out string value)
        {
            using(var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM commonly WHERE key='{key}' COLLATE NOCASE";
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

        public void DeleteCommonlyKey(string key)
        {
            using(var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"DELETE FROM commonly WHERE key='{key}'";
                cmd.ExecuteNonQuery();
            }
        }

        public void AddCommonlyRecord(string key, string value)
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


        /// <summary>
        /// 查询 套装 通俗名称 词库
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet_SuitCommonly_Value(string key, out string value)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM commonly_suit WHERE key='{key}' COLLATE NOCASE";
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

        public void Delete_SuitCommonly_Key(string key)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"DELETE FROM commonly_suit WHERE key='{key}'";
                cmd.ExecuteNonQuery();
            }
        }

        public void Add_SuitCommonly_Record(string key, string value)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"REPLACE INTO commonly_suit (key,value) VALUES(@key,@value)";
                cmd.Parameters.Add("key", DbType.String).Value = key;
                cmd.Parameters.Add("value", DbType.String).Value = value;
                cmd.ExecuteNonQuery();
            }
        }

        public bool TryGetSuitInfo_CN(string name, out string suit_items, out string name_hans, out string name_en)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM suit WHERE name_hans='{name}'";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    suit_items = reader.GetString(2);
                    name_hans = reader.GetString(0);
                    name_en = reader.GetString(1);
                    return true;
                }
                suit_items = string.Empty;
                name_hans = string.Empty;
                name_en = string.Empty;
                return false;
            }
        }

        public bool TryGetSuitInfo_EN(string name, out string suit_items, out string name_hans, out string name_en)
        {
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM suit WHERE name_en='{name}' COLLATE NOCASE";
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    suit_items = reader.GetString(2);
                    name_hans = reader.GetString(0);
                    name_en = reader.GetString(1);
                    return true;
                }
                suit_items = string.Empty;
                name_hans = string.Empty;
                name_en = string.Empty;
                return false;
            }
        }

    }
}
