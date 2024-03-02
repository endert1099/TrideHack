using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrideDashModder;
using UnityEngine;

public class API
{
    public bool test = true;
    public static List<PluginInsert> InsertModOnPlugins(string name, string version, string developer)
    {
        PluginInsert pluginInsert = new PluginInsert();
        pluginInsert.name = name;
        pluginInsert.version = version;
        pluginInsert.developer = developer;
        List<PluginInsert> plugins = new List<PluginInsert>();
        Plugin.CreatePlugin(pluginInsert);
        foreach (PluginInsert plugin in Plugin.GetPlugins())
        {
            plugins.Add(plugin);
        }
        return plugins;
    }

}
public class PluginInsert
{
    public string name = "Unnamed Plugin";
    public string version = "0.0.0";
    public string developer = "Unknown Developer";
    public bool isEnabled = true;
}
