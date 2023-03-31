using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace PIGNUMBERS
{
    // Handles the combat data assignments
    public class Combatant
    {
        // Static Variables
        public static float maxShare = 0;
        public static string Log;

    
        // General Variables
        private const float maxBGopacity = 0.6f;
        public List<Attack> Attacks;
        public string ID, isTemporary;
        public string Name { get; set; }
        public float PercentDPS, PercentReadDPS;
        public int ActiveTime;

        // Constructor #1
        public Combatant(string id, string name)
        {
            ID = id;
            Name = id;
            PercentDPS = -1;
            Attacks = new List<Attack>();
            isTemporary = "no";
            PercentReadDPS = 0;
            ActiveTime = 0;
            Damaged = 0;
        }

        // Constructor #2
        public Combatant(string id, string name, string temp)
        {
            ID = id;
            Name = id;
            PercentDPS = -1;
            Attacks = new List<Attack>();
            isTemporary = temp;
            PercentReadDPS = 0;
            ActiveTime = 0;
            Damaged = 0;
        }

        /* Common GET Data Properties */

        public int Damaged;   // Remon's fixes
        public int ZvsDamage  => GetDamageDealt(GetZanverseID());                // Zanverse total damage
       

        public int Damage     => GetGeneralDamage();  // General damage dealt
        public int MaxHitNum  => MaxHitAttack.Damage; // Max Hit damage
        public int ReadDamage => GetReadingDamage();  // Filtered damage dealt

        public Attack MaxHitAttack => GetMaxHitAttack(); // General max hit damage number

        public double DPS     => GetGeneralDPS(); // General DPS
        public double ReadDPS => GetReadingDPS(); // Filtered DPS

        public string DisplayName => GetDisplayName(); // Get player OR anon names

        public string DamageReadout => ReadDamage.ToString("N0"); // Damage dealt stringified
        public string ReadDamaged   => Damaged.ToString("N0");    // Damage taken stringified

        public string StringDPS             => ReadDPS.ToString("N0"); // DPS numbers stringified
        public string PercentReadDPSReadout => GetPercentReadDPS();    // DPS numbers percentified
        public string FDPSReadout           => GetDPSReadout();        // Formated DPS numbers
        public string DPSReadout            => PercentReadDPSReadout;  // Formated DPS (Percent)

        public string MaxHit    => GetMaxHit();    // Max hit name
        public string MaxHitID  => GetMaxHitID();  // Max hit attack ID
        public string MaxHitdmg => GetMaxHitdmg(); // Max hit numbers stringified 
        
        public string JAPercent  => GetJAPercent();  // Just Attack % in decimal point of 0|2
        public string WJAPercent => GetWJAPercent(); // Just Attack % in format of 00.00

        public string CRIPercent  => GetCRIPercent();  // Critical Rate % in decimal point of 0|2
        public string WCRIPercent => GetWCRIPercent(); // Critical Rate % in format of 00.00

        public bool IsYou => false;
        public bool IsAlly     => CheckIsAlly();              // Other players running
        public bool IsZanverse => CheckIsType("Zanverse");    // Zanverse being cast
        public bool IsStatus   => CheckIsType("Status Ailment");      // status occuring
        

        public Brush Brush  => GetBrushPrimary();   // Player-chan damage graph
        public Brush Brush2 => GetBrushSecondary(); // Other players damage graph

        /* HELPER FUNCTIONS */

        // Censors other players' name except the user
        private string AnonymousName()
        {
            if (IsYou)
                return Name;
            else
                return "----";
        }

        // Draw method for generating the damage graph
        private LinearGradientBrush GenerateBarBrush(Color c, Color c2)
        {
            if (!Properties.Settings.Default.ShowDamageGraph)
                c = new Color();

            if (IsYou && Properties.Settings.Default.HighlightYourDamage)
                c = Color.FromArgb(128, 0, 255, 255);

            LinearGradientBrush lgb = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(1, 0)
            };
            lgb.GradientStops.Add(new GradientStop(c, 0));
            lgb.GradientStops.Add(new GradientStop(c, ReadDamage / maxShare));
            lgb.GradientStops.Add(new GradientStop(c2, ReadDamage / maxShare));
            lgb.GradientStops.Add(new GradientStop(c2, 1));
            lgb.SpreadMethod = GradientSpreadMethod.Repeat;
            return lgb;
        }

        // Formating numbers to either K (1,000) or M (1,000,000)
        private String FormatNumber(double value)
        {
            int num = (int)Math.Round(value);

            if (value >= 100000000)
                return (value / 1000000).ToString("#,0") + "M";
            if (value >= 1000000)
                return (value / 1000000D).ToString("0.0") + "M";
            if (value >= 100000)
                return (value / 1000).ToString("#,0") + "K";
            if (value >= 1000)
                return (value / 1000D).ToString("0.0") + "K";
            return value.ToString("#,0");
        }

        // Fetch the technique "Zanverse" ID
        private IEnumerable<PIGNUMBERS.Attack> GetZanverseID() 
        {
            return Attacks.Where(a => a.ID == "2106601422"); // Zanverse
        }

        // Fetch the attack ID
        private IEnumerable<PIGNUMBERS.Attack> GetAttackID(string[] attackID) 
        {
            return Attacks.Where(a => attackID.Contains(a.ID));
        }

        // Returns the total damage taken for MPA
        private int GetTotalDamageTaken() 
        { 
            return Damaged;
        }
        
        // Fetch the total Damage Dealt value [ Use after (GetAttackID) function ]
        private int GetDamageDealt(IEnumerable<PIGNUMBERS.Attack> attackID) 
        {
            return attackID.Sum(x => x.Damage);
        }

        // Returns the general damage dealt
        private int GetGeneralDamage() 
        { 
            return Attacks.Sum(x => x.Damage); 
        }

        // Returns the damage dealt that has been filtered
        private int GetReadingDamage()
        {
            if (IsZanverse || IsStatus)
                return Damage;

            int temp = Damage;
            if (Properties.Settings.Default.SeparateZanverse)
                temp -= ZvsDamage;
            return temp;
        }

        // Returns the max damage hit done
        private Attack GetMaxHitAttack()
        {
            Attacks.Sort((x, y) => y.Damage.CompareTo(x.Damage));
            return Attacks.FirstOrDefault();
        }

        // Returns the general DPS
        private double GetGeneralDPS() 
        { 
            if (ActiveTime == 0)
            {
                return Damage;
            }
            else
            {
                return Damage / ActiveTime;
            }
        }

        // Returns the DPS that has been filtered
        private double GetReadingDPS() 
        { 
            if (ActiveTime == 0)
            {
                return ReadDamage;
            }
            else
            {
                return Math.Round(ReadDamage / (double)ActiveTime); 
            }
        }
        
        // Returns the display naming choices (Name or Anon)
        private string GetDisplayName()
        {
            if (Properties.Settings.Default.AnonymizeNames) { return AnonymousName(); }
            return Name;
        }


        // Percentifies the DPS numbers
        private string GetPercentReadDPS()
        {
            if (PercentReadDPS < -.5 || float.IsNaN(PercentReadDPS))
            {
                return "N/A ";
            }
            else
            {
                return $"{PercentReadDPS:0.00}";
            }
        }        

        // Stringifies the DPS numbers
        private string GetDPSReadout()
        {
            if (Properties.Settings.Default.DPSformat)
            {
                return FormatNumber(ReadDPS);
            } 
            else 
            {
                return StringDPS;
            }
        }

        // Returns the attack name that achieved max damage hit
        private string GetMaxHit()
        {
            if (MaxHitAttack == null)
                return "--";
            string attack = "Unknown";
            if (MainWindow.skillDict.ContainsKey(MaxHitID))
            {
                attack = MainWindow.skillDict[MaxHitID];
            }
            else {
        //        File.AppendAllText(Properties.Settings.Default.Path + "\\damagelogs\\unknowns.csv", MaxHitID+"\r\n");
            }
            return attack;
        }

        // Returns the nax hit attack ID
        private string GetMaxHitID()
        {
            return MaxHitAttack.ID;
        }
        
        // Returns the max hit damage number
        private string GetMaxHitdmg()
        {
            if (MaxHitAttack == null)
            {
                return "N/A";
            }
            else
            {
                return MaxHitAttack.Damage.ToString("N0");
            }
        }

        // Returns the Just Attack Percentage
        private string GetJAPercent()
        {
            IEnumerable<Attack> totalJA = Attacks.Where(a => !MainWindow.ignoreskill.Contains(a.ID));

            if (totalJA.Any())
            {
                Double averageJA = totalJA.Average(x => x.JA) * 100;

                if (Properties.Settings.Default.Nodecimal)
                {
                    return averageJA.ToString("N0");
                } 
                else 
                {
                    return averageJA.ToString("N2");
                }
            }
            else
            {
                if (Properties.Settings.Default.Nodecimal)
                {
                    return "0";
                }
                else
                {
                    return "0.00";
                }
            }
        }

        // Returns the Just Attack Percentage in "00.00"
        private string GetWJAPercent() 
        {
            IEnumerable<Attack> totalJA = Attacks.Where(a => !MainWindow.ignoreskill.Contains(a.ID));

            if (totalJA.Any())
            {
                Double averageJA = totalJA.Average(x => x.JA) * 100;
                return averageJA.ToString("00.00");
            }
            else
            {
                return "00.00";
            }
        }

        // Returns the Critical Rate Percentange
        private string GetCRIPercent()
        {
            IEnumerable<Attack> totalCri = Attacks;

            if (totalCri.Any())
            {

                Double averageCri = totalCri.Average(x => x.Cri) * 100.0;/// totalCri.Count((x => x.Cri == 1))* 100.0;
               // averageCri /= totalCri.Count();

                if (Properties.Settings.Default.Nodecimal)
                {
                    return averageCri.ToString("N0");
                }
                else
                {
                    return averageCri.ToString("N2");
                }
            }
            else
            {
                if (Properties.Settings.Default.Nodecimal)
                {
                    return "0";
                }
                else
                {
                    return "0.00";
                }
            }
        }

        // Returns the Critical Rate Percentage in "00.00"
        private string GetWCRIPercent() 
        {
            IEnumerable<Attack> totalCri = Attacks;

            if (totalCri.Any())
            {
                Double averageCri = totalCri.Average(x => x.Cri) * 100.0;
                //    averageCri /= totalCri.Count();
                return averageCri.ToString("00.00");
            }
            else
            {
                return "00.00";
            }
        }

       
        // Checks if this is a player
        private bool CheckIsAlly()
        {
            if (int.Parse(ID) >= 10000000)
            {
                return true;
            } 
            else 
            {
                return false;
            }
        }

        // Checks if this is using other modes of attack
        private bool CheckIsType(string currType) 
        { 
            return (isTemporary == currType); 
        }

        // Generates the damage bar graph for the user
        private Brush GetBrushPrimary()
        {
            if (Properties.Settings.Default.ShowDamageGraph && (IsAlly))
            {
                return GenerateBarBrush(Color.FromArgb(128, 0, 128, 128), Color.FromArgb(128, 30, 30, 30));
            } 
            else 
            {
                if (IsYou && Properties.Settings.Default.HighlightYourDamage)
                {
                    return new SolidColorBrush(Color.FromArgb(128, 0, 255, 255));
                }

                return new SolidColorBrush(Color.FromArgb(127, 30, 30, 30));
            }
        }

        // Generates the damage bar graph for other players
        private Brush GetBrushSecondary()
        {
            if (Properties.Settings.Default.ShowDamageGraph && (IsAlly && !IsZanverse))
            {
                return GenerateBarBrush(Color.FromArgb(128, 0, 64, 64), Color.FromArgb(0, 0, 0, 0));
            } 
            else 
            {
                if (IsYou && Properties.Settings.Default.HighlightYourDamage) 
                {
                    return new SolidColorBrush(Color.FromArgb(128, 0, 64,64));
                }

                return new SolidColorBrush(new Color());
            }
        }
        

        /* NOT USED - for now (Will think of a way to add the tabs back in more efficient method)

        // Fetch the Max Damage Hit that the player did
        private Attack GetMaxHit(string[] attackID) 
        {
            Attacks.RemoveAll(a => !attackID.Contains(a.ID));

            Attacks.Sort((x, y) => y.Damage.CompareTo(x.Damage));

            if (Attacks != null)
            {
                return Attacks.FirstOrDefault();
            } 
            else 
            {
                return null;
            }
        }

        // Fetch the Attack Name that achieved Max Damage Hit
        private string GetAttackName(Attack maxHit) 
        {
            if (maxHit == null) { return "--"; }
            string attack = "Unknown";

            if (MainWindow.skillDict.ContainsKey(maxHit.ID)) { attack = MainWindow.skillDict[maxHit.ID]; }
            return MaxHitAttack.Damage.ToString(attack);
        }

        // Calclate the actual Damage Per Second / DPS
        private string CalculateDPS(int damageDealt) 
        {
            return Math.Round(damageDealt / (double)ActiveTime).ToString("N0");
        }

        // Fetch the Just Attack value [ Use after (GetAttackID) function ]
        private string GetJAValue(IEnumerable<PIGNUMBERS.Attack> attackID) 
        {
            return (attackID.Average(x => x.JA) * 100).ToString("N2");
        }

        // Fetch the Critical Attack value [ Use after (GetAttackID) function ]
        private string GetCritValue(IEnumerable<PIGNUMBERS.Attack> attackID) 
        {
            return (attackID.Average(x => x.Cri) * 100).ToString("N2");    
        }

        */
    }

  
    // Attack class properties
    public class Attack
    {
        public string ID;
        public int Damage, JA, Cri;

        public Attack(string initID, int initDamage, int justAttack, int critical)
        {
            ID = initID;
            Damage = initDamage;
            JA = justAttack;
            Cri = critical;
        }
        
        public Attack(string initID, int initDamage, int critical)
        {
            ID = initID;
            Damage = initDamage;
            JA = 0;
            Cri = critical;
        }
    }
}
