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
                    cmd.CommandText = $"SELECT * FROM Props WHERE Name_CN=\"{name}\"";
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
    }
}
