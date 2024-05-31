using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrideDashModder;

public class ModBase
{
    public bool HasBeenLoaded = false;
    public virtual object CallbackOverride(Dictionary<string, object> kwargs)
    {
        return kwargs;
    }
    public virtual List<PluginInsert> LoadOverride()
    {
        Dictionary<string, object> kwargs = new Dictionary<string, object>();
        TrideDashMod.NewPlugin("Default Mod", "0.0.0", "Default Creator", kwargs);
        HasBeenLoaded = true;
        return Plugin.GetPlugins();
    }
}
public class TrideDashMod : ModBase
{
    public static ModBase mb = new ModBase();
    public static object CallbackMethod(Dictionary<string, object> kwargs)
    {
        return mb.CallbackOverride(kwargs);
    }
    public static List<PluginInsert> LoadAllPlugins()
    {
        return mb.LoadOverride();
    }
    public static (PluginInsert, object) NewPlugin(string name, string version, string developer, Dictionary<string, object> kwargs)
    {
        List<PluginInsert> plugins = API.InsertModOnPlugins(name, version, developer);
        object callback = CallbackMethod(kwargs);
        return (plugins.Last(), callback);
    }
}

