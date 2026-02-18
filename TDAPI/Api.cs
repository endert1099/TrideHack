using System.Collections.Generic;

namespace TDAPI
{
	// Allows for the same structure of mods but perhaps for other areas
	public abstract class ModBase<T> where T : ModBase<T>, new()
	{
		public string Name { get; private set; } = "Unnamed Mod";
		public string Version { get; private set; } = "0.0.0";
		public string Developer { get; private set; } = "Unknown Developer";
		public bool IsEnabled { get; private set; } = true;

		private static T instance = null;

		public static T GetInstance()
		{
			if (instance == null) instance = new T();
			return instance;
		}

		public virtual void Callback(bool success)
		{
			return;
		}

		public virtual bool Load()
		{
			return true;
		}

		public virtual bool OnUpdate()
		{
			return true;
		}
	}

	public static class ModList<T>
	{
		private static List<T> mods = new List<T>();

		public static void CreateMod(T mod)
		{
			mods.Add(mod);
		}
		public static int GetCount()
		{
			return mods.Count;
		}
		public static List<T> GetMods()
		{
			return mods;
		}
	}
}