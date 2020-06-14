using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using YamlDotNet.RepresentationModel;
using DataConvert.Data;
using System.Linq;

namespace DataConvert
{
    public static class PropYamlHandler
    {
        private static YamlScalarNode ScalarNode_Name = new YamlScalarNode("name");
        private static YamlScalarNode ScalarNode_Name_En = new YamlScalarNode("en");
        private static YamlScalarNode ScalarNode_Name_Cn = new YamlScalarNode("zh");
        private static YamlScalarNode ScalarNode_GroupId = new YamlScalarNode("groupID");
        private static YamlScalarNode ScalarNode_Published = new YamlScalarNode("published");
        private static YamlScalarNode ScalarNode_Volume = new YamlScalarNode("volume");
        public static void StartHandler(string path)
        {
            using var file_stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var stream_reader = new StreamReader(file_stream);
            using var db = new EVEDBContext();
            var yaml = new YamlStream();
            yaml.Load(stream_reader);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            var start_time = DateTime.UtcNow;
            int i = 0;
            foreach(var entry in mapping.Children)
            {
                i++;
                Console.WriteLine("正在处理第：" + i + " 项");
                try
                {
                    var node = (YamlScalarNode)entry.Key;
                    if (long.TryParse(node.Value, out var id))
                    {
                        //忽略项
                        if (IgnoreDefine.Ignore_IDs.Contains(id))
                        {
                            Console.WriteLine($"    Id {id}的项目被强行忽略了╰（‵□′）╯");
                            continue;
                        }
                        //查询
                        if (db.Props.Any(p => p.ID == id))
                        {
                            Console.WriteLine($"    Id {id}的项目已存在，不读取了");
                            continue;
                        }

                        var prop_obj = new PropData();
                        var map_node = (YamlMappingNode)entry.Value;
                        var names = (YamlMappingNode)(map_node.Children[ScalarNode_Name]);
                        var name_en = names.Children[ScalarNode_Name_En].ToString();
                        string name_cn = name_en;
                        try
                        {
                            name_cn = names.Children[ScalarNode_Name_Cn].ToString();
                        }
                        catch
                        {
                            name_cn = name_en;
                        }
                        var group_id = (YamlScalarNode)(map_node.Children[ScalarNode_GroupId]);
                        var published = (YamlScalarNode)(map_node.Children[ScalarNode_Published]);

                        if(bool.TryParse(published.Value, out bool b_p))
                        {
                            if(!b_p)
                            {
                                Console.WriteLine($"    [{(string.IsNullOrEmpty(name_cn) ? name_cn : name_en)}] 未发布，已忽略");
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        prop_obj.Name = name_en;
                        prop_obj.Name_CN = name_cn;
                        prop_obj.ID = id;
                        if (int.TryParse(group_id.Value, out var _group_id))
                            prop_obj.GroupID = _group_id;
                        else
                            prop_obj.GroupID = -1;

                        try
                        {
                            var volume = (YamlScalarNode)(map_node.Children[ScalarNode_Volume]);
                            if (float.TryParse(volume.Value, out var _volume))
                                prop_obj.Volume = _volume;
                            else
                                prop_obj.Volume = 0f;
                        }
                        catch { prop_obj.Volume = 0f; }

                        Console.WriteLine("    " + prop_obj.Name_CN);
                        db.Props.Add(prop_obj);

                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"    出现异常：" + e.Message);
                }

            }

            db.SaveChanges();

            var use_time = DateTime.UtcNow - start_time;
            Console.WriteLine($"总用时：{use_time.TotalSeconds}秒 （{use_time.TotalMinutes}分钟）");
        }
    }
}
