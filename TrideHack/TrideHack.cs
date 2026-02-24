using System.Globalization;
using System.Reflection;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using TDAPI;

[assembly: MelonInfo(typeof(TrideDashModder.TrideHack), "Tride Hack", TrideDashModder.TrideHack.version, "Ender(suzu)", "https://github.com/endert1099/TrideHack/releases/latest")]
[assembly: MelonGame("DefaultCompany", "Tride Dash")]

namespace TrideDashModder
{
	public class TrideHackMod : ModBase<TrideHackMod>
	{
		public readonly TrideHackVariables TrideHack = TrideHackVariables.GetInstance();
	}

	public class TrideHackVariables
	{
		// All variables used across the class
		// Most are only used in update
		// But I put them here for organization and so that the API can use them

		// Constants
		public readonly string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\LocalLow\DefaultCompany\Tride Dash";
		public readonly string datafolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\LocalLow\DefaultCompany\Tride Dash" + @"\TrideHackData";
		public readonly string bestsfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\LocalLow\DefaultCompany\Tride Dash" + @"\TrideHackData\bests.tdi";

		// All the variables used for updates
		// General
		public bool menuEnabled = true;
		public float minx = float.MaxValue;
		public float maxx = float.MinValue;
		public cube player = null;
		public bool inGame = false;

		// Speedhack
		public bool isSlowed = false;
		public float numspeed = 0.25f;
		public bool speedhackAudio = true;

		// NoClip
		public bool noclip = false;
		public bool disableBlocks = true;
		public bool lastDisableBlocks = true;
		public bool noclipChangedThisTick = false;

		// Progress Bar
		public bool loadProgressBar = true;
		public float progress = 0f;
		public float startingPercent = 0;
		public float lastProgress = 0f;

		// Endscreen display
		public bool noclipWasEnabled = false;
		public bool speedhackWasEnabled = false;

		// Windowed mode
		public bool windowed = false;
		public int newHeight = 0;
		public int newWidth = 0;

		// New best
		public float levelBest = 0f;

		// Startpos
		public float startX = 0f;
		public float originalStartX = 0f;
		public float startY = 0f;
		public float originalStartY = 0f;

		private static TrideHackVariables instance = null;

		private TrideHackVariables() { }

		public static TrideHackVariables GetInstance()
		{
			if (instance == null) { instance = new TrideHackVariables(); }
			return instance;
		}
	}

	public class TrideHack : MelonMod
	{
		// Mod Version
		public const string version = "0.3.3";

		// All the private variables used for updates
		private int lastFrameAttempts = 0;
		private bool displayNewBest = false;
		private IEnumerable<GameObject> objects = null; // These are private because its subject to change and not hard to get
		private IEnumerable<GameObject> blocks = null;
		private bool firstFrameOfScene = true;
		private List<TrideHackMod> mods; // Handled by plugins

		// All the variables used for UI
		private string speedhack = "0.25";
		private string lastspeedhack = "0.25";
		private string width = "";
		private string height = "";
		private string lastwidth = "";
		private string lastheight = "";
		private DateTime finishTime = DateTime.Now;
		private string newStartX = "0.00";
		private string lastStartX = "";
		private string newStartY = "0.00";
		private string lastStartY = "";

		TrideHackVariables v = TrideHackVariables.GetInstance();
		public override void OnUpdate()
		{
			v.inGame = SceneManager.GetActiveScene().name == "playLevel";
			// Hackmenu
			if (Input.GetKeyUp(KeyCode.Tab))
			{
				v.menuEnabled = !v.menuEnabled;
			}

			//Speedhack
			if (Input.GetKeyDown(KeyCode.T))
			{
				if (v.isSlowed)
				{
					Time.timeScale = 1f;
					// player.moveSpeed = 8;
				}
				else
				{
					Time.timeScale = v.numspeed;
					// player.moveSpeed = numspeed * 8;
				}
				v.isSlowed = !v.isSlowed;
			}

			// I refuse to make this in v because its only used once
			loadLevelToPlay[] levelLoaders = GameObject.FindObjectsByType<loadLevelToPlay>(FindObjectsSortMode.None);

			if (v.speedhackAudio && levelLoaders.Length != 0) levelLoaders[0].music.pitch = Time.timeScale;
			else if (levelLoaders.Length != 0) levelLoaders[0].music.pitch = 1f;

			//NoClip
			if (Input.GetKeyDown(KeyCode.N))
			{
				v.noclip = !v.noclip;
				v.noclipChangedThisTick = true;
			}

			// Backup levels
			if (Input.GetKeyDown(KeyCode.L))
			{
				string date = DateTime.Now.Date.ToLocalTime().ToShortDateString();
				string hour = DateTime.Now.Hour.ToString();
				string minute = DateTime.Now.Minute.ToString();
				string second = DateTime.Now.Second.ToString();
				if (hour.Length < 2) hour = "0" + hour;
				if (minute.Length < 2) minute = "0" + minute;
				if (second.Length < 2) second = "0" + second;
				string time = hour + minute + second;
				string datetime = date.Replace("/", "") + "-" + time;
				DirectoryInfo d = new DirectoryInfo(v.localappdata + @"\saves");
				FileInfo[] files = d.GetFiles("*.txt");

				if (!Directory.Exists(v.localappdata + @"\bckpsaves"))
				{
					Directory.CreateDirectory(v.localappdata + @"\bckpsaves");
				}
				if (!Directory.Exists(v.localappdata + @"\bckpsaves\" + datetime))
				{
					Directory.CreateDirectory(v.localappdata + @"\bckpsaves\" + datetime);
				}

				foreach (FileInfo file in files)
				{
					string contents = File.ReadAllText(file.FullName);
					File.WriteAllText(v.localappdata + @"\bckpsaves\" + datetime + @"\" + file.Name, contents);
				}
			}

			// Toggle percent bar
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				v.loadProgressBar = !v.loadProgressBar;
			}

			// Windowed mode
			if (Input.GetKeyDown(KeyCode.W) && SceneManager.GetActiveScene().name != "Editor")
			{
				v.windowed = !v.windowed;
				if (v.windowed) Screen.SetResolution(v.newWidth, v.newHeight, false);
				else Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
			}

			// Start of all the inGame required functions
			if (!v.inGame)
			{
				v.noclipWasEnabled = false;
				v.speedhackWasEnabled = false;
				return;
			}

			// Autowin
			if (Input.GetKeyDown(KeyCode.LeftControl))
			{
				v.player.win();
			}

			// Set noclip colliders
			if (firstFrameOfScene)
			{
				objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("spike")); //TODO: make this more objects than spike
				blocks = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("block"));

				if (v.noclip)
				{
					foreach (var obj in objects)
					{
						Collider2D collider = obj.GetComponent<Collider2D>();
						collider.enabled = false;
					}
					if (v.disableBlocks)
					{
						foreach (var obj in blocks)
						{
							Collider2D collider = obj.GetComponent<Collider2D>();
							collider.enabled = false;
						}
					}
				}
			}

			// Clear signs of me having a mental breakdown
			// 2 years later suzu here and I dont know whats wrong with this
			if (v.noclipChangedThisTick)
			{
				v.noclipChangedThisTick = false;
				foreach (var obj in objects)
				{
					obj.GetComponent<Collider2D>().enabled = !v.noclip;
				}
				if (v.disableBlocks)
				{
					foreach (var b in blocks)
					{
						b.GetComponent<Collider2D>().enabled = !v.noclip;
					}
				}
			}

			// Immediately change disabled blocks
			if (v.disableBlocks != v.lastDisableBlocks)
			{
				v.lastDisableBlocks = v.disableBlocks;
				if (v.disableBlocks)
				{
					foreach (var b in blocks)
					{
						b.GetComponent<Collider2D>().enabled = !v.noclip;
					}
				}
				else
				{
					foreach (var b in blocks)
					{
						b.GetComponent<Collider2D>().enabled = true;
					}
				}
			}

			// Restart key
			if (Input.GetKeyDown(KeyCode.R))
			{
				// Set iframes to 0!!
				Type cubetype = typeof(cube);
				FieldInfo iframes = cubetype.GetField("invincibilityFrames", BindingFlags.NonPublic | BindingFlags.Instance);
				iframes.SetValue(v.player, 0f);

				Rigidbody2D rb = v.player.rb;
				v.player.kill();
				v.player.rb.velocity = new Vector2(v.player.rb.velocity.x, 0);
				rb.gravityScale = Math.Abs(v.player.rb.gravityScale);
			}

			// Update progress bar
			if (v.loadProgressBar)
			{
				Rigidbody2D rb = v.player.rb;
				float lvlLen = v.maxx - v.minx;
				double newProgress = (rb.position.x - v.startX) / lvlLen;
				//newProgress = newProgress - v.startingPercent;
				v.progress = (float)Math.Round(newProgress, 4);
				v.progress = v.progress * 100;
			}

			// Test if hacks have been enabled
			// I used to have a series of if statments for this...
			// I had like 5 if(!noclip) and if(inGame) all changing 1 variable
			// crine
			if (v.noclip)
			{
				v.noclipWasEnabled = true;
			}
			if (v.isSlowed && v.numspeed < 1)
			{
				v.speedhackWasEnabled = true;
			}

			if (lastFrameAttempts != v.player.attemptCount)
			{
				v.noclipWasEnabled = false;
				v.speedhackWasEnabled = false;
				lastFrameAttempts = (int)Math.Floor(v.player.attemptCount); //Why is attemptCount stored as a float
			}

			// New Best and Best ever
			if (!Directory.Exists(v.datafolder)) Directory.CreateDirectory(v.datafolder);
			if (!File.Exists(v.bestsfile)) File.Create(v.bestsfile);
			if (v.levelBest < v.lastProgress && Mathf.Abs(v.startX - v.originalStartX) < 0.01 && v.progress < v.lastProgress)
			{
				v.levelBest = v.lastProgress;

				string filetext = File.ReadAllText(v.bestsfile);
				List<string> data = filetext.Split(',').ToList(); //TODO: Might cause some international decimal format problems
				List<float> bestvals = new List<float>();
				List<string> names = new List<string>();

				foreach (string obj in data)
				{
					List<string> objcontents = obj.Split(':').ToList();
					string last = objcontents.Last();
					float lastf;
					if (float.TryParse(last, out lastf))
					{
						bestvals.Add(lastf);
					}
					names.Add(objcontents.First());
				}

				string levelName = PlayerPrefs.GetString("levelName");
				bool isSameName = names.IndexOf(levelName) > -1; // Confusing ass variable name
				if (isSameName)
				{
					if (bestvals[names.IndexOf(levelName)] < v.levelBest)
					{
						bestvals[names.IndexOf(levelName)] = v.levelBest;
						string newFileText = "";
						foreach (string item in names)
						{
							int idx = names.IndexOf(item);
							string currstr = item + ":" + bestvals[idx].ToString();
							if (item != names.Last())
							{
								currstr += ",";
							}
							newFileText += currstr;
						}
						File.WriteAllText(v.bestsfile, newFileText);
					}
				}
				else
				{
					string txt = File.ReadAllText(v.bestsfile);
					string appendtxt = null;
					if (txt.Length > 0)
					{
						appendtxt = "," + levelName + ":" + v.levelBest;
					}
					else
					{
						appendtxt = levelName + ":" + v.levelBest;
					}
					File.AppendAllText(v.bestsfile, appendtxt);
				}

				displayNewBest = true;
			}
			v.lastProgress = v.progress;

			//Startpos
			Transform rp = v.player.respawnPoint;
			if (v.startX != v.originalStartX || v.startY != v.originalStartY)
			{
				rp.position = new Vector3(v.startX, v.startY);
			}
		}
		public override void OnSceneWasLoaded(int buildIndex, string sceneName)
		{
			firstFrameOfScene = true;
			if (sceneName == "playLevel")
			{
				// Percent bar
				// Todo make this good
				try
				{
					v.minx = float.MaxValue;
					v.maxx = float.MinValue;
					string name = PlayerPrefs.GetString("levelName");
					string path = v.localappdata + @"\saves\" + name + ".txt";
					string level = File.ReadAllText(path);
					level = level.Substring(level.IndexOf("§") + 1);
					level = level.Substring(level.IndexOf("§") + 1);
					level = level.Substring(level.IndexOf("§") + 1);
					level = level.Substring(level.IndexOf("{") + 3);
					string[] levelMap = level.Split(';');
					foreach (string prop in levelMap)
					{
						if (prop.Contains("pos:"))
						{
							int startidx = prop.IndexOf("(") + 1;
							int endidx = prop.IndexOf(",");
							string xpos = prop.Substring(startidx, endidx - startidx);
							float nxpos = float.Parse(xpos, CultureInfo.InvariantCulture);
							if (nxpos < v.minx) v.minx = nxpos;
							if (nxpos > v.maxx) v.maxx = nxpos;
						}
					}
				}
				catch { }

				v.player = GameObject.Find("Player").GetComponent<cube>();
				newStartX = v.player.respawnPoint.position.x.ToString();
				v.startX = v.player.respawnPoint.position.x;
				v.originalStartX = v.player.respawnPoint.position.x;
				newStartY = v.player.respawnPoint.position.y.ToString();
				v.startY = v.player.respawnPoint.position.y;
				v.originalStartY = v.player.respawnPoint.position.y;

				v.levelBest = v.lastProgress;

				string filetext = File.ReadAllText(v.bestsfile);
				List<string> data = filetext.Split(',').ToList(); //TODO: Might cause some international decimal format problems
				List<float> bestvals = new List<float>();
				List<string> names = new List<string>();

				foreach (string obj in data)
				{
					List<string> objcontents = obj.Split(':').ToList();
					string last = objcontents.Last();
					float lastf;
					if (float.TryParse(last, out lastf))
					{
						bestvals.Add(lastf);
					}
					names.Add(objcontents.First());
				}

				string levelName = PlayerPrefs.GetString("levelName");
				if (names.Contains(levelName)) v.levelBest = bestvals[names.IndexOf(levelName)];
				else v.levelBest = 0;
			}

		}
		public override void OnInitializeMelon()
		{
			MelonEvents.OnGUI.Subscribe(DrawMenu, 0); // The higher the value, the lower the priority.
													  // Why did I bother with the above comment
													  // Method patching
			HarmonyLib.Harmony harmonyInstance = this.HarmonyInstance;

			MethodInfo jumpOrbCollision = typeof(deathSpike).GetMethod("OnTriggerStay2D", BindingFlags.NonPublic | BindingFlags.Instance);
			Action<Collider2D> orbAction = orbSpaceBar;
			harmonyInstance.Patch(jumpOrbCollision, new HarmonyMethod(orbAction.GetMethodInfo()));

			MethodInfo keyBinds = typeof(editorObject).GetMethod("keyBinds", BindingFlags.NonPublic | BindingFlags.Instance);
			Action<editorObject> keybindAction = newKeyBinds;
			harmonyInstance.Patch(keyBinds, null, new HarmonyMethod(keybindAction.GetMethodInfo()));


			mods = ModList<TrideHackMod>.GetMods();
			foreach (TrideHackMod mod in mods)
			{
				bool loaded = mod.Load();
				mod.Callback(loaded);
				if (loaded)
				{
					MelonLogger.Msg("Mod loaded: " + mod.Name + " v" + mod.Version);
				}
				else
				{
					MelonLogger.Warning("Failed to load mod: " + mod.Name + " v" + mod.Version + " will not be included");
				}
			}
			MelonLogger.Msg("Successfully loaded TrideHack v" + TrideHack.version + " running TDAPI v" + TDAPIInfo.Version);

			/*
             * I Just wanna keep this code here as a relic for all to enjoy
             * Man i really improved at programming
             if(Plugin.GetCount() <= 0)
            {
                MelonLogger.Msg($"Currently {Plugin.GetCount()} plugins loaded, less than 1.");
            }*/
		}
		private void DrawWindowGUI(int windowID)
		{
			// This could probably be improved but I dont wanna
			GUI.Box(new Rect(0, 30, 300, 30), "Speedhack(T): " + v.isSlowed.ToString());
			GUI.Box(new Rect(0, 60, 300, 30), "NoClip(N): " + v.noclip.ToString());
			GUI.Box(new Rect(0, 90, 300, 30), "Progress Bar(0): " + v.loadProgressBar.ToString());
			GUI.Box(new Rect(0, 120, 300, 30), "Windowed Mode(W): " + v.windowed.ToString());
			GUI.Box(new Rect(0, 150, 300, 30), "Startpos: ");
			GUI.Box(new Rect(0, 210, 500, 30), "Press LCtrl to complete, R to restart, and L to backup levels");
			speedhack = GUI.TextField(new Rect(300, 30, 100, 30), speedhack, 4);
			width = GUI.TextField(new Rect(300, 120, 100, 30), width, 4);
			height = GUI.TextField(new Rect(400, 120, 99, 30), height, 4);
			v.disableBlocks = GUI.Toggle(new Rect(300, 60, 100, 100), v.disableBlocks, "Disable Blocks");
			newStartX = GUI.TextField(new Rect(300, 150, 100, 30), newStartX);
			newStartY = GUI.TextField(new Rect(400, 150, 99, 30), newStartY);
			v.speedhackAudio = GUI.Toggle(new Rect(400, 30, 100, 100), v.speedhackAudio, "Speedhack\naudio");

			if (!float.TryParse(speedhack, out v.numspeed))
			{
				if (speedhack == "")
				{
					speedhack = "1";
					v.numspeed = 1;
				}
				else
				{
					speedhack = lastspeedhack;
				}
			}
			lastspeedhack = speedhack;

			if (!Int32.TryParse(height, out v.newHeight))
			{
				if (height == "")
				{
					height = "480";
					v.newHeight = 480;
				}
				else
				{
					height = lastheight;
				}
			}
			else if (v.newHeight < 480 && v.windowed)
			{
				height = "";
				v.newHeight = 480;
			}
			lastheight = height;

			if (!Int32.TryParse(width, out v.newWidth))
			{
				if (width == "")
				{
					width = "854";
					v.newWidth = 854;
				}
				else
				{
					width = lastwidth;
				}
			}
			else if (v.newWidth < 854 && v.windowed)
			{
				width = "";
				v.newWidth = 480;
			}
			lastwidth = width;

			if (!float.TryParse(newStartX, out v.startX))
			{
				if (newStartX == "")
				{
					if (v.inGame) newStartX = v.originalStartX.ToString();
					else newStartX = "0";
				}
				else
				{
					newStartX = lastStartX;
				}
			}
			lastStartX = newStartX;

			if (!float.TryParse(newStartY, out v.startY))
			{
				if (newStartY == "")
				{
					if (v.inGame) newStartY = v.originalStartY.ToString();
					else newStartY = "0";
				}
				else
				{
					newStartY = lastStartY;
				}
			}
			lastStartY = newStartY;
		}
		private void DrawMenu()
		{
			if (v.menuEnabled)
			{
				Rect window = GUI.Window(0, new Rect(0, 300, 500, 700), DrawWindowGUI, "TrideHack v" + TrideHack.version);
				// Rect plugins = GUI.Window(0, new Rect(500, 300, 500, 700), DrawPluginsGUI, "Plugins");
			}

			bool inGame = SceneManager.GetActiveScene().name == "playLevel";
			if (inGame)
			{
				if (v.loadProgressBar) GUI.Box(new Rect((Screen.width / 2) - 75, 0, 150, 30), v.progress.ToString() + "%");

				if (v.player.winScreen.activeSelf)
				{
					GUI.Box(new Rect((Screen.width / 2) - 150, 650, 300, 30), "Using TrideHack v" + TrideHack.version);
					if (v.noclipWasEnabled) GUI.Box(new Rect((Screen.width / 2) - 150, 800, 300, 30), "NoClip was used");
					if (v.speedhackWasEnabled) GUI.Box(new Rect((Screen.width / 2) - 150, 830, 300, 30), "Speedhack was used");
				}
			}
			if (displayNewBest)
			{
				finishTime = DateTime.Now.AddSeconds(1.2);
			}
			if (finishTime.Subtract(DateTime.Now) > TimeSpan.Zero)
			{
				displayNewBest = false;
				GUI.Box(new Rect((Screen.width / 2) - 150, 500, 300, 30), "New Best: " + v.levelBest.ToString());
			}
		}
		private static void orbSpaceBar(Collider2D collision)
		{
			TrideHackVariables v = TrideHackVariables.GetInstance();
			if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) && !v.player.alreadyJumped)
			{
				if (collision.tag == "yellowOrb")
				{
					v.player.jumpOrb(0);
				}
				else if (collision.tag == "pinkOrb")
				{
					v.player.jumpOrb(1);
				}
				else if (collision.tag == "greenOrb")
				{
					v.player.jumpOrb(2);
				}
				else if (collision.tag == "blueOrb")
				{
					v.player.jumpOrb(3);
				}
			}
		}

		private static void newKeyBinds(editorObject __instance)
		{
			editorObject e = __instance; // The param name is required by harmony

			if (Input.GetKeyDown(KeyCode.R))
			{
				e.editor.rotation = Quaternion.Euler(0f, 0f, Mathf.FloorToInt(e.editor.rotation.eulerAngles.z - 90f));
			}

			if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
			{
				e.editor.delete = true;
			}

			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				if (Input.GetKeyDown(KeyCode.W))
				{
					e.transform.position = new Vector2(e.transform.position.x, e.transform.position.y - 0.9f);
				}
				else if (Input.GetKeyDown(KeyCode.A))
				{
					e.transform.position = new Vector2(e.transform.position.x + 0.9f, e.transform.position.y);
				}
				else if (Input.GetKeyDown(KeyCode.S))
				{
					e.transform.position = new Vector2(e.transform.position.x, e.transform.position.y + 0.9f);
				}
				else if (Input.GetKeyDown(KeyCode.D))
				{
					e.transform.position = new Vector2(e.transform.position.x - 0.9f, e.transform.position.y);
				}

				if (Input.GetKeyDown(KeyCode.R))
				{
					e.editor.rotation = Quaternion.Euler(0f, 0f, Mathf.FloorToInt(e.editor.rotation.eulerAngles.z + 180f)); // Double bc we already did it
				}

				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
				{
					if (Input.GetKeyDown(KeyCode.R))
					{
						e.editor.rotation = Quaternion.Euler(0f, 0f, Mathf.FloorToInt(e.editor.rotation.eulerAngles.z - 177f));
					}
				}
			}

			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				if (Input.GetKeyDown(KeyCode.R))
				{
					e.editor.rotation = Quaternion.Euler(0f, 0f, Mathf.FloorToInt(e.editor.rotation.eulerAngles.z + 89f));
				}
			}
		}

		private static void debugMethod(levelEditor __instance)
		{
			MelonLogger.Msg(__instance.movement.x);
		}
	}
}