using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace NGSParser
{
    // Handles the logging section of the parser.
    // TODO: Optimise the rest of the codes
    public class Log
    {
        // File Setup Variables
        public bool valid, notEmpty, running;
        public string filename;
        public DirectoryInfo logDirectory;

        private const int pluginVersion = 5;

        // Logging Variables
        public static int startTimestamp = 0;
        public static int newTimestamp = 0;

        public List<Combatant> combatants = new List<Combatant>();
        public List<Combatant> backupCombatants = new List<Combatant>();

        private string encounterData;
        private List<int> instances = new List<int>();
        private StreamReader logReader;

        // Constructor
        public Log(string attemptDirectory)
        {
            valid      = false;
            notEmpty   = false; //idk what use this is
            running    = false;
            bool nagMe = false;

            // Setup first time warning
          /*  if (Properties.Settings.Default.BanWarning)
            {
                MessageBoxResult panicResult = MessageBox.Show("NGSParser is a 3rd-party tool that breaks PSO2's Terms and Conditions."
                                                             + "SEGA has confirmed in an official announcement that accounts found using parsing tools may be banned.\n\n"
                                                             + "If account safety is your first priority, do NOT use NGSParser. You use this tool entirely at your own risk.\n\n"
                                                             + "Would you like to continue with setup?", "NGSParser Setup", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (panicResult == MessageBoxResult.No)
                {
                    Environment.Exit(-1);
                }

                Properties.Settings.Default.BanWarning = false;
            }
          */
            // Invalid pso2_bin directory, prompting for new one...
            while (!File.Exists($"{attemptDirectory}\\pso2.exe"))
            {
                if (nagMe)
                {
                    MessageBox.Show("That doesn't appear to be a valid pso2_bin directory.\n\n" 
                                  + "If you installed the game using default settings, it will probably be in C:\\PHANTASYSTARONLINE2\\pso2_bin\\. " 
                                  + "Otherwise, find the location you installed to.", "NGSParser Setup", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Please select your pso2_bin directory. NGSParser uses this to read your damage logs.\n\n", "NGSParser Setup", MessageBoxButton.OK, MessageBoxImage.Information);
                    nagMe = true;
                }

                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select your pso2_bin folder. This will be inside the folder you installed PSO2 to."
                };

                System.Windows.Forms.DialogResult picked = dialog.ShowDialog();
                if (picked == System.Windows.Forms.DialogResult.OK)
                {
                    // Testing {attemptDirectory} as pso2_bin directory...
                    attemptDirectory = dialog.SelectedPath;
                    Properties.Settings.Default.Path = attemptDirectory;
                }
                else
                {
                    // Canceled out of directory picker
                    MessageBox.Show("NGSParser needs a valid PSO2 installation to function.\n" 
                                  + "The application will now close.", "NGSParser Setup", MessageBoxButton.OK, MessageBoxImage.Information);
                    Environment.Exit(-1); // ABORT ABORT ABORT
                    break;
                }
            }

            if (!File.Exists($"{attemptDirectory}\\pso2.exe")) { return; } // If pso2_bin isn't selected, exiting ...

            valid = true; // pso2_bin selected correctly!

            logDirectory = new DirectoryInfo($"{attemptDirectory}\\damagelogs"); // Making sure pso2_bin\damagelogs exists

           
            Properties.Settings.Default.LaunchMethod = "Manual";


            Properties.Settings.Default.FirstRun = false; // Passed first time setup, skipping above on future launch

            /* ---------------------------------------------------------------------------------------------------- */ 
            /*
            if (!logDirectory.Exists) 
            {
                logDirectory.Create();
               // return; 
            }                 // Abort if damage log directory doesn't exist - why
            if (logDirectory.GetFiles().Count() == 0) { return; } // this is not really necessary but also probably not a problem //todo ok it kinda is bad
            */
            notEmpty = true; // Log directory is not empty!

            FileInfo log = logDirectory.GetFiles().Where(f => Regex.IsMatch(f.Name, @"\d+\.")).OrderByDescending(f => f.Name).First(); //get latest file
            filename = log.Name; // Reading from {log.DirectoryName}\{log.Name}")

            FileStream fileStream = File.Open(log.DirectoryName + "\\" + log.Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fileStream.Seek(0, SeekOrigin.Begin);
            logReader = new StreamReader(fileStream);

            string existingLines = logReader.ReadToEnd(); // Gotta get the dummy line for current player name
            string[] result = existingLines.Split('\n');
            /* foreach (string s in result)
            {
                if (s == "")
                    continue;
                string[] parts = s.Split(',');
                /*if (parts[0] == "0" && parts[3] == "YOU")
                {
                    Hacks.currentPlayerID = parts[2]; // Found existing active player ID
                }
            } */
        }

        /* CLASS FUNCTIONS */

        /* LOG TEMP*/
        // Writes combat data to log file
        public string WriteLog()
        {
            if (combatants.Count != 0) // Players found
            {
                int elapsed = newTimestamp - startTimestamp;
                TimeSpan timespan = TimeSpan.FromSeconds(elapsed);

                // Logging the total time occured through out the encounter
                int totalDamage = combatants.Where(c => c.IsAlly || c.IsStatus || c.IsZanverse).Sum(x => x.Damage);
                double totalDPS = combatants.Where(c => c.IsAlly || c.IsStatus || c.IsZanverse).Sum(x => x.DPS);

                string timer = timespan.ToString(@"mm\:ss");
                string log = DateTime.Now.ToString("F") + " | " + timer + " | Total Damage: " + totalDamage.ToString("N0") + " dmg" + " | " + "Total DPS: " + totalDPS.ToString("N0") + Environment.NewLine + Environment.NewLine;

                log += "[ Encounter Overview ]" + Environment.NewLine; // Title

                foreach (Combatant c in combatants)
                {
                    if (c.IsAlly || c.IsStatus || c.IsZanverse)
                    {
                        log += Environment.NewLine + $"# {c.Name}" + Environment.NewLine + $"# Contrib: {c.PercentReadDPSReadout}% | Dealt: {c.ReadDamage.ToString("N0")} dmg | Taken: {c.Damaged.ToString("N0")} dmgd | {c.DPS.ToString("N0")} DPS | Critical: {c.WCRIPercent}% | Max: {c.MaxHitdmg} ({c.MaxHit})" + Environment.NewLine;
                    }
                }

                log += Environment.NewLine + Environment.NewLine;

                foreach (Combatant c in combatants)
                {
                    if (c.IsAlly || c.IsStatus || c.IsZanverse || true)
                    {
                        string header = $"[ {c.Name} - {c.PercentReadDPSReadout}% - {c.ReadDamage.ToString("N0")} dmg ]";
                        log += header + Environment.NewLine + Environment.NewLine;

                        List<string> attackNames = new List<string>();
                        List<string> finishNames = new List<string>();
                        List<string> statusNames = new List<string>();

                        List<Tuple<string, List<int>, List<int>, List<int>>> attackData = new List<Tuple<string, List<int>, List<int>, List<int>>>();

                        if (c.IsZanverse && Properties.Settings.Default.SeparateZanverse)
                        {
                            foreach (Combatant c2 in backupCombatants)
                            {
                                if (c2.ZvsDamage > 0)
                                    attackNames.Add(c2.ID);
                            }

                            foreach (string s in attackNames)
                            {
                                Combatant targetCombatant = backupCombatants.First(x => x.ID == s);
                                List<int> matchingAttacks = targetCombatant.Attacks.Where(a => a.ID == "2106601422").Select(a => a.Damage).ToList();
                                List<int> jaPercents = c.Attacks.Where(a => a.ID == "2106601422").Select(a => a.JA).ToList();
                                List<int> criPercents = c.Attacks.Where(a => a.ID == "2106601422").Select(a => a.Cri).ToList();
                                attackData.Add(new Tuple<string, List<int>, List<int>, List<int>>(targetCombatant.Name, matchingAttacks, jaPercents, criPercents));
                            }
                        }
                        else
                        {
                            foreach (Attack a in c.Attacks)
                            {
                                if (MainWindow.skillDict.ContainsKey(a.ID))
                                    a.ID = MainWindow.skillDict[a.ID]; // these are getting disposed anyway, no 1 cur
                                if (!attackNames.Contains(a.ID))
                                    attackNames.Add(a.ID);
                            }

                            foreach (string s in attackNames)
                            {
                                // Choose damage from matching attacks
                                List<int> matchingAttacks = c.Attacks.Where(a => a.ID == s).Select(a => a.Damage).ToList();
                                List<int> jaPercents = c.Attacks.Where(a => a.ID == s).Select(a => a.JA).ToList();
                                List<int> criPercents = c.Attacks.Where(a => a.ID == s).Select(a => a.Cri).ToList();
                                attackData.Add(new Tuple<string, List<int>, List<int>, List<int>>(s, matchingAttacks, jaPercents, criPercents));
                            }
                        }

                        attackData = attackData.OrderByDescending(x => x.Item2.Sum()).ToList();

                        foreach (var i in attackData)
                        {
                            if (i.Item1 != "Taken")
                            {
                                double percent = i.Item2.Sum() * 100d / c.ReadDamage;

                                string spacer = (percent >= 9) ? "" : " ";
                                string paddedPercent = percent.ToString("00.00");

                                string hits = i.Item2.Count().ToString("N0");
                                string sum = i.Item2.Sum().ToString("N0");
                                string min = i.Item2.Min().ToString("N0");
                                string max = i.Item2.Max().ToString("N0");
                                string avg = i.Item2.Average().ToString("N0");
                                string cri = (i.Item4.Average() * 100).ToString("N2") ?? "null";

                                log += $"{paddedPercent}% | {i.Item1} ({sum} dmg)" + Environment.NewLine;
                                log += $"       |   Critical : {cri}%" + Environment.NewLine;
                                log += $"       |   {hits} hits - {min} min, {avg} avg, {max} max" + Environment.NewLine;
                            }
                            else
                            {
                                string hits = i.Item2.Count().ToString("N0");

                                log += "Number of hits taken: " + hits + Environment.NewLine;
                            }
                        }

                        log += Environment.NewLine;
                    }
                }

                log += "Instance IDs: " + String.Join(", ", instances.ToArray());

                DateTime thisDate = DateTime.Now;
                string directory = string.Format("{0:yyyy-MM-dd}", DateTime.Now);

                Directory.CreateDirectory($"Logs/{directory}");
                string datetime = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
                string filename = $"Logs/{directory}/OverParse - {datetime}.txt";

                File.WriteAllText(filename, log);
                Console.WriteLine("Summary log written to " + filename);
                return filename;
            }

            return null;
        }


        // Returns log status messages
        public string LogStatus()
        {
            if (!valid) // If not valid ...
            {
                return "PSO2 directory not found";
            }

            if (!notEmpty) // If damage log is empty ...
            {
                return $"Waiting for combat data...";
                //return "Directory No logs: Enable plugin and check pso2_bin!";
            }

            if (!running) // If parser is running ...
            {
                return $"Waiting for combat data...";
            }

            return encounterData;
        }

        // Updates the logging display
        //@return true end encounter
        public bool UpdateLog(object sender, EventArgs e)
        {
            if (!valid || !notEmpty) { return false; }

            string newLines = logReader.ReadToEnd();

            if (newLines != "")
            {
                string[] result = newLines.Split('\n');
                foreach (string str in result)
                {
                    if (str != "")
                    {
                        string[] parts = str.Split(',');
                        if (parts.Length == 1) {
                            if (parts[0].Contains("end_encounter") && Properties.Settings.Default.ManualMode == true) 

                            {
                                return true;
                            }
                            else 
                            { 
                                return false; 
                            }
                        }
                        else {
                            string sourceID = parts[2];
                            string sourceName = parts[3];
                            string targetID = parts[4];
                            string targetName = parts[5];
                            string attackID = parts[6];

                            // string isMultiHit = parts[10];
                            // string isMisc = parts[11];
                            // string isMisc2 = parts[12];

                            int lineTimestamp = int.Parse(parts[0]);
                            int instanceID = int.Parse(parts[1]);
                            int hitDamage = int.Parse(parts[7]);
                        //    int justAttack = int.Parse(parts[8]);
                            int critical = (int.Parse(parts[9])>0 ? 1 : 0);

                            int index = -1;

                       /*     if (lineTimestamp == 0 && parts[3] == "YOU") {
                                Hacks.currentPlayerID = parts[2];
                                continue;
                            }*/

                      //      if (sourceID != Hacks.currentPlayerID && Properties.Settings.Default.Onlyme) { continue; }

                     //       if (!instances.Contains(instanceID)) { instances.Add(instanceID); }

                            if (hitDamage < 1) { continue; }

                            if (sourceID == "0" || attackID == "0") { continue; }

                            // Process start

                            if (10000000 < int.Parse(sourceID)) {
                                foreach (Combatant x in combatants) {
                                    if (x.ID == sourceID && x.isTemporary == "no") {
                                        index = combatants.IndexOf(x);
                                    }
                                }

                                if (index == -1) {
                                    combatants.Add(new Combatant(sourceID, sourceName));
                                    index = combatants.Count - 1;
                                }

                                Combatant source = combatants[index];

                                newTimestamp = lineTimestamp;
                                if (startTimestamp == 0) { startTimestamp = newTimestamp; }

                                source.Attacks.Add(new Attack(attackID, hitDamage, critical));
                                running = true;
                            }
                            else {
                                foreach (Combatant x in combatants) {
                                    if (x.ID == targetID && x.isTemporary == "no") {
                                        index = combatants.IndexOf(x);
                                    }
                                }

                                if (index == -1) {
                                    combatants.Add(new Combatant(targetID, targetName));
                                    index = combatants.Count - 1;
                                }

                                Combatant source = combatants[index];

                                newTimestamp = lineTimestamp;

                                if (startTimestamp == 0) { startTimestamp = newTimestamp; }

                                source.Damaged += hitDamage;
                                running = true;
                            }
                        }
                    }
                }

                combatants.Sort((x, y) => y.ReadDamage.CompareTo(x.ReadDamage));

                if (startTimestamp != 0) { encounterData = "0:00:00 - ∞ DPS"; }

                if (startTimestamp != 0 && newTimestamp != startTimestamp)
                {
                    foreach (Combatant x in combatants)
                    {
                        if (x.IsAlly) { x.ActiveTime = (newTimestamp - startTimestamp); }
                    }
                }
            }
            return false;
        }

        /* HELPER FUNCTIONS */ 

        ///* DEBUG MODE ONLY - Not used on production

        // Debug for ID mapping
        private void DebugMapping() 
        {
            foreach (Combatant c in combatants)
            {
                if (c.IsAlly)
                {
                    foreach (Attack a in c.Attacks)
                    {
                        if (!MainWindow.skillDict.ContainsKey(a.ID))
                        {
                          //  TimeSpan t = TimeSpan.FromSeconds(a.Timestamp);
                            //Console.WriteLine($"{t.ToString(@"dd\.hh\:mm\:ss")} unmapped: {a.ID} ({a.Damage} dmg from {c.Name})");
                        }
                    }
                }
            }
        }

        
    }
}
