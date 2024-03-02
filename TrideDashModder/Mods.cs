using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using MelonLoader;
using System.Xml.Linq;
using UnityEngine.SceneManagement;
using TMPro;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.Networking;
using System.Collections;
using MelonLoader.TinyJSON;
using Harmony;
namespace TrideDashModder
{
    public class Plugin
    {
        static List<PluginInsert> plugins = new List<PluginInsert>();
        static int count;
        public static void CreatePlugin(PluginInsert nPlugin)
        {
            plugins.Add(nPlugin);
            count++;
        }
        public static int GetCount()
        {
            return count;
        }
        public static List<PluginInsert> GetPlugins()
        {
            return plugins;
        }
    }
    public class Mods : MelonMod
    {
        bool isSlowed = false;
        bool noclip = false;
        bool menu = true;
        string speedhack = "0.25";
        string lastspeedhack = "0.25";
        float numspeed = 0.25f;
        bool loadProgressBar = true;
        string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\LocalLow\DefaultCompany\Tride Dash";
        float progress = 0.00f;
        float minx = 9999999999999999;
        float maxx = -9999999999999999;
        float startingPercent = 0;
        bool firstFrameOfScene = true;
        bool noclipWasEnabled = false;
        bool hasFinishedLevel = false;
        int lastFrameAttempts = 0;
        bool windowed = false;
        int newHeight = 0;
        int newWidth = 0;
        string width = "";
        string height = "";
        string lastwidth = "";
        string lastheight = "";
        bool disableBlocks = true;
        float lastBest = 0.00f;
        bool shouldChangeBest = false;
        bool displayNewBest = false;
        DateTime finishTime = DateTime.Now;
        bool isDisplayingSplash = false;
        float bestStatic = 0.00f;
        float bestOfLevel = 0.00f;
        List<PluginInsert> plugins;
        string newStartX = "0.00";
        float startX = 0.00f;
        string lastStartX = "";
        float originalStartX = 0.00f;
        bool speedhackWasEnabled = false;
        bool startposWasEnabled = false;
        bool startposCurrentlyEnabled = false;
        public override void OnUpdate()
        {
            bool inGame = SceneManager.GetActiveScene().name == "playLevel";
            // Hackmenu
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                menu = !menu;
            }

            // Autowin
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                player.win();
            }

            //Speedhack
            if (Input.GetKeyDown(KeyCode.T))
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                if (isSlowed)
                {
                    Time.timeScale = 1f;
                    // player.moveSpeed = 8;
                    isSlowed = false;
                }
                else
                {
                    Time.timeScale = numspeed;
                    // player.moveSpeed = numspeed * 8;
                    isSlowed = true;
                }
            }

            //NoClip
            if (Input.GetKeyDown(KeyCode.N))
            {
                noclip = !noclip;
            }
            // Clear signs of me having a mental breakdown
            if (noclip && inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                System.Collections.Generic.IEnumerable<GameObject> objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("spike"));
                System.Collections.Generic.IEnumerable<GameObject> blocks = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("block"));
                if(disableBlocks)
                {
                    foreach (var b in blocks)
                    {
                        Collider2D collider = b.GetComponent<Collider2D>();
                        collider.enabled = false;
                    }
                }
                else
                {
                    foreach (var b in blocks)
                    {
                        Collider2D collider = b.GetComponent<Collider2D>();
                        collider.enabled = true;
                    }
                }
                foreach (var obj in objects)
                {
                    Collider2D collider = obj.GetComponent<Collider2D>();
                    collider.enabled = false;
                }
            }
            else if (inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("spike") || obj.name.Contains("block"));
                foreach (var obj in objects)
                {
                    Collider2D collider = obj.GetComponent<Collider2D>();
                    collider.enabled = true;
                }
            }
            // Restart key
            if (Input.GetKeyDown(KeyCode.R) && inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                Rigidbody2D rb = player.rb;
                if(rb.position.x >= 0) player.kill();
                player.rb.velocity = new Vector2(player.rb.velocity.x, 0);
                rb.gravityScale = Math.Abs(player.rb.gravityScale);
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
                string time =  hour + minute + second;
                string datetime = date.Replace("/", "") + "-" + time;  
                DirectoryInfo d = new DirectoryInfo(localappdata + @"\saves");
                FileInfo[] files = d.GetFiles("*.txt");

                if (!Directory.Exists(localappdata + @"\bckpsaves"))
                {
                    Directory.CreateDirectory(localappdata + @"\bckpsaves");
                }
                if (!Directory.Exists(localappdata + @"\bckpsaves\" + datetime))
                {
                    Directory.CreateDirectory(localappdata + @"\bckpsaves\" + datetime);
                }

                foreach (FileInfo file in files)
                {
                    string contents = File.ReadAllText(file.FullName);
                    File.WriteAllText(localappdata + @"\bckpsaves\" + datetime + @"\" + file.Name, contents);
                }
            }

            // Toggle percent bar
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                loadProgressBar = !loadProgressBar;
            }

            // Update progress bar
            if (inGame && loadProgressBar)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                Rigidbody2D rb = player.rb;
                float lvlLen = maxx - minx;
                if (firstFrameOfScene)
                {
                    startingPercent = (float)Math.Round(rb.position.x / lvlLen, 4);
                    firstFrameOfScene = false;
                }
                double newProgress = rb.position.x / lvlLen;
                newProgress = newProgress - startingPercent;
                progress = (float)Math.Round(newProgress, 4);
                progress = progress * 100;
            }

            // Test if hacks have been enabled
            if (inGame && noclip)
            {
                noclipWasEnabled = true;
            }
            if (!inGame)
            {
                noclipWasEnabled = false;
            }
            if (inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                if (!noclip && lastFrameAttempts != player.attemptCount)
                {
                    noclipWasEnabled = false;
                }
            }
            if (inGame && isSlowed)
            {
                speedhackWasEnabled = true;
            }
            if (!inGame)
            {
                speedhackWasEnabled = false;
            }
            if (inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                if (!isSlowed && lastFrameAttempts != player.attemptCount)
                {
                    speedhackWasEnabled = false;
                }
            }
            if (inGame && startposCurrentlyEnabled)
            {
                startposWasEnabled = true;
            }
            if (!inGame)
            {
                startposWasEnabled = false;
            }
            if (inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                if (!startposCurrentlyEnabled && lastFrameAttempts != player.attemptCount)
                {
                    startposWasEnabled = false;
                }
            }

            // Windowed mode
            if (Input.GetKeyDown(KeyCode.W) && SceneManager.GetActiveScene().name != "Editor")
            {
                windowed = !windowed;
                if(windowed)
                {
                    Screen.SetResolution(newWidth, newHeight, false);
                }
            }

            // Set the last attempts
            if (inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                lastFrameAttempts = Int32.Parse(player.attemptCount.ToString());
            }
            // New Best and Best ever
            if (!Directory.Exists(localappdata + @"\TrideHackData")) Directory.CreateDirectory(localappdata + @"\TrideHackData");
            if (!File.Exists(localappdata + @"\TrideHackData\bests.tdi")) File.Create(localappdata + @"\TrideHackData\bests.tdi");
            if(inGame)
            {
                if(lastBest < progress && startX == originalStartX)
                {
                    lastBest = progress;
                    shouldChangeBest = true;
                }
                else if(shouldChangeBest)
                {
                    shouldChangeBest = false;
                    string filetext = File.ReadAllText(localappdata + @"\TrideHackData\bests.tdi");
                    List<string> data = filetext.Split(',').ToList();
                    List<float> bestvals = new List<float>();
                    List<string> names = new List<string>();
                    foreach (string obj in data)
                    {
                        List<string> objcontents = obj.Split(':').ToList();
                        string last = objcontents.Last();
                        float lastf = float.NaN;
                        float.TryParse(last, out lastf);
                        if (lastf != float.NaN)
                        {
                            bestvals.Add(lastf);
                        }
                        names.Add(objcontents.First());
                    }
                    bool isSameName = false;
                    isSameName = names.IndexOf(PlayerPrefs.GetString("levelName")) > -1;
                    if (isSameName)
                    {
                        if (bestvals[names.IndexOf(PlayerPrefs.GetString("levelName"))] < lastBest)
                        {
                            bestvals[names.IndexOf(PlayerPrefs.GetString("levelName"))] = lastBest;
                            string newFileText = "";
                            foreach(string item in names)
                            {
                                int idx = names.IndexOf(item);
                                string currstr = item + ":" + bestvals[idx].ToString();
                                if(item != names.Last())
                                {
                                    currstr += ",";
                                }
                                newFileText += currstr;
                            }
                            File.WriteAllText(localappdata + @"\TrideHackData\bests.tdi", newFileText);
                        }
                    }
                    else
                    {
                        string txt = File.ReadAllText(localappdata + @"\TrideHackData\bests.tdi");
                        string appendtxt = null;
                        if (txt.Length > 0)
                        {
                            appendtxt = "," + PlayerPrefs.GetString("levelName") + ":" + lastBest;
                        }
                        else
                        {
                            appendtxt = PlayerPrefs.GetString("levelName") + ":" + lastBest;
                        }
                        File.AppendAllText(localappdata + @"\TrideHackData\bests.tdi", appendtxt);
                    }
                    bestStatic = lastBest;
                    bestOfLevel = bestvals[names.IndexOf(PlayerPrefs.GetString("levelName"))];
                    displayNewBest = true;
                    lastBest = -300.0f;
                }
            }
            //Startpos
            if(inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                Transform rp = player.respawnPoint;
                if(startX != originalStartX)
                {
                    startposCurrentlyEnabled = true;
                    MelonLogger.Msg((startX, originalStartX));
                }
                rp.position = new Vector3(startX, rp.position.y);
            }
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {   
            firstFrameOfScene = true;
            if(sceneName == "playLevel")
            {
                if (noclip)
                {
                    var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("spike") || obj.name.Contains("block"));
                    foreach (var obj in objects)
                    {
                        Collider2D collider = obj.GetComponent<Collider2D>();
                        collider.enabled = false;
                    }
                }

                // Percent bar
                minx = 9999999999999999;
                maxx = -9999999999999999;
                string name = PlayerPrefs.GetString("levelName");
                string path = localappdata + @"\saves\" + name + ".txt";
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
                        if (nxpos < minx) minx = nxpos;
                        if (nxpos > maxx) maxx = nxpos;
                    }
                }
                cube player = GameObject.Find("Player").GetComponent<cube>();
                newStartX = player.respawnPoint.position.x.ToString();
                startX = player.respawnPoint.position.x;
                originalStartX = player.respawnPoint.position.x;
            }

        }
        public override void OnInitializeMelon()
        {
            MelonEvents.OnGUI.Subscribe(DrawMenu, 0); // The higher the value, the lower the priority.
            plugins =  TrideDashMod.LoadAllPlugins();
            foreach (PluginInsert plugin in plugins)
            {
                MelonLogger.Msg(plugin.name);
            }
            if(Plugin.GetCount() <= 0)
            {
                MelonLogger.Msg($"Currently {Plugin.GetCount()} plugins loaded, less than 1.");
            }
        }
        private void DrawWindowGUI(int windowID)
        {
            GUI.Box(new Rect(0, 30, 300, 30), "Speedhack(T): " + isSlowed.ToString());
            GUI.Box(new Rect(0, 60, 300, 30), "NoClip(N): " + noclip.ToString());
            GUI.Box(new Rect(0, 90, 300, 30), "Progress Bar(0): " + loadProgressBar.ToString());
            GUI.Box(new Rect(0, 120, 300, 30), "Windowed Mode(W): " + windowed.ToString());
            GUI.Box(new Rect(0, 150, 300, 30), "Startpos: ");
            GUI.Box(new Rect(0, 210, 500, 30), "Press LCtrl to complete, R to restart, and L to backup levels");
            speedhack = GUI.TextField(new Rect(300, 30, 100, 30), speedhack, 4);
            width = GUI.TextField(new Rect(300, 120, 100, 30), width, 4);
            height = GUI.TextField(new Rect(400, 120, 99, 30), height, 4);
            disableBlocks = GUI.Toggle(new Rect(300, 60, 100, 100), disableBlocks, "Disable Blocks?");
            newStartX = GUI.TextField(new Rect(300, 150, 300, 30), newStartX);
            if (!float.TryParse(speedhack, out numspeed))
            {
                if (speedhack == "")
                {
                    speedhack = "1";
                    numspeed = 1;
                }
                else
                {
                    speedhack = lastspeedhack;
                }
            }
            lastspeedhack = speedhack;
            if (!Int32.TryParse(height, out newHeight))
            {
                if (height == "")
                {
                    height = "480";
                    newHeight = 480;
                }
                else
                {
                    height = lastheight;
                }
            }
            else if(newHeight < 480 && windowed)
            {
                height = "";
                newHeight = 480;
            }
            lastheight = height;
            if (!Int32.TryParse(width, out newWidth))
            {
                if (width == "")
                {
                    width = "854";
                    newWidth = 854;
                }
                else
                {
                    width = lastwidth;
                }
            }
            else if (newWidth < 854 && windowed)
            {
                width = "";
                newWidth = 480;
            }
            lastheight = height;

            if(!windowed)
            {
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
            if (!float.TryParse(newStartX, out startX))
            {
                if (newStartX == "")
                {
                    newStartX = "0";
                }
                else
                {
                    newStartX = lastStartX;
                }
            }
            lastStartX = newStartX;
        }
        private void DrawMenu()
        {
            if (menu)
            {
                Rect window = GUI.Window(0, new Rect(0, 300, 500, 700), DrawWindowGUI, "TrideHack v0.2.1");
               // Rect plugins = GUI.Window(0, new Rect(500, 300, 500, 700), DrawPluginsGUI, "Plugins");
            }
            bool inGame = SceneManager.GetActiveScene().name == "playLevel";
            if (loadProgressBar && inGame)
            {
                GUI.Box(new Rect((Screen.width / 2) - 75, 0, 150, 30), progress.ToString() + "%");
            }
            if (inGame)
            {
                cube player = GameObject.Find("Player").GetComponent<cube>();
                if (noclipWasEnabled && player.winScreen.activeSelf)
                {
                    GUI.Box(new Rect((Screen.width / 2) - 150, 800, 300, 30), "NoClip was used");
                }
                if (speedhackWasEnabled && player.winScreen.activeSelf)
                {
                    GUI.Box(new Rect((Screen.width / 2) - 150, 830, 300, 30), "Speedhack was used");
                }
                if (startposWasEnabled && player.winScreen.activeSelf)
                {
                    GUI.Box(new Rect((Screen.width / 2) - 150, 860, 300, 30), "Startpos was used");
                }
            }
            if (displayNewBest)
            {
                finishTime = DateTime.Now.AddSeconds(1.2);
            }
            if (finishTime.Subtract(DateTime.Now) > TimeSpan.Zero && bestStatic >= bestOfLevel)
            {
                displayNewBest = false;
                GUI.Box(new Rect((Screen.width / 2) - 150, 500, 300, 30), "New Best: " + bestStatic.ToString());
            }
            else if(bestStatic >= bestOfLevel)
            {
                bestStatic = 0;
                bestOfLevel = 0;
            }
        }
    }
}
