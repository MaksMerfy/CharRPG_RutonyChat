using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace RutonyChat
{
    public class Script
    {
        public string FileDirectory = "";
        public string filename = "";
        public string filenamejpg = "";
        public string fileLogi = "";
        public Random rnd = new Random();
        private string twnick = "";
        public Warriors players;
        public listBosses ListBosses;

        public currentBoss CurrentBoss;

        public void InitParams(string param)
        {
            FileDirectory = RutonyBotFunctions.GetScriptDirectory("AtackBossAuto.cs");
            if (FileDirectory == "")
            {
                FileDirectory = ProgramProps.dir_scripts;
            }
            filename = FileDirectory + @"\CurrentBoss.json";
            filenamejpg = FileDirectory + @"\CurrentBoss.png";
            fileLogi = FileDirectory + @"\Logirovanie.txt";
            twnick = RutonyBot.TwitchBot.NICK.ToLower();
            players = GetListWarriors();
            ListBosses = GetListBosses();
            RutonyBot.SayToWindow("Скрипт атаки босса успешно подключен");
        }

        public void Closing()
        {
            savePlayerList();
            players = new Warriors();
            ListBosses = new listBosses();
            RutonyBot.SayToWindow("Скрипт атаки босса отключен");
        }

        public void NewMessage(string site, string name, string text, bool system)
        {
            if (site == "twitch")
            {
                if (twnick == name.ToLower())
                {
                    return;
                }
            }
            /* bool TextHasAtack = text.ToLower().Contains("!атака");
             if (TextHasAtack == true)
             {
                 Atack(name, site);
             }
             bool TextHasHeal = text.ToLower().Contains("!хил");
             if (TextHasHeal == true)
             {
                 Heal(name, site, text);
             }
             bool TextHasMassHeal = text.ToLower().Contains("!массхил");
             if (TextHasMassHeal == true)
             {
                 MassHeal(name, site);
             }
             bool TextHasPersonag = text.ToLower().Contains("!персонаж");
             if (TextHasPersonag == true)
             {
                 Personag(name, site);
             }*/
            switch (text.ToLower())
            {
                case "!атака":
                    Atack(name, site);
                    break;
                case "!хил":
                    Heal(name, site, text);
                    break;
                case "!массхил":
                    MassHeal(name, site);
                    break;
                case "!персонаж":
                    Personag(name, site);
                    break;
            }
            if (text.ToLower().Contains("!прокачать"))
            {
                UpgradeSkill(name, site, text);
            }
        }
        public void UpgradeSkill(string name, string site, string text)
        {
            Hero thisWarrior = GetWarrior(name, site);

            string[] arrSplit = text.Split(' ');
            string NameSkill = text.Replace(arrSplit[0] + " ", "");
            int CurLvlUpSkill = 0;

            switch (NameSkill)
            {
                case "хп":
                    CurLvlUpSkill = thisWarrior.HP - 100;
                    CurLvlUpSkill = (CurLvlUpSkill / 10);
                    thisWarrior.HP += (CheckExpHero(thisWarrior, CurLvlUpSkill, site)) ? 10 : 0;
                    break;
                case "урон":
                    CurLvlUpSkill = thisWarrior.Damage - 1;
                    thisWarrior.Damage += (CheckExpHero(thisWarrior, CurLvlUpSkill, site)) ? 1 : 0;
                    break;
                case "броня":
                    CurLvlUpSkill = thisWarrior.Armor - 1;
                    thisWarrior.Armor += (CheckExpHero(thisWarrior, CurLvlUpSkill, site)) ? 1 : 0;
                    break;
                case "мана":
                    CurLvlUpSkill = thisWarrior.Mana - 0;
                    CurLvlUpSkill = (CurLvlUpSkill / 10);
                    thisWarrior.Mana += (CheckExpHero(thisWarrior, CurLvlUpSkill, site)) ? 10 : 0;
                    break;
                case "хил":
                    CurLvlUpSkill = thisWarrior.heroSkill.Heal - 0;
                    CurLvlUpSkill = (CurLvlUpSkill / 5);
                    thisWarrior.heroSkill.Heal += (CheckExpHero(thisWarrior, CurLvlUpSkill, site)) ? 5 : 0;
                    break;
                case "массхил":
                    CurLvlUpSkill = thisWarrior.heroSkill.MassHeal - 0;
                    thisWarrior.heroSkill.MassHeal += (CheckExpHero(thisWarrior, CurLvlUpSkill, site)) ? 1 : 0;
                    break;
                default:
                    RutonyBot.BotSay(site, name + ", не понимаю, что хочешь прокачать. Можно прокачать : хп, урон, броня, мана, хил, массхил");
                    return;
            }
            thisWarrior.Experience -= (100 + (CurLvlUpSkill * 10));
            Personag(name, site);

        }
        public Boolean CheckExpHero(Hero thisWarrior, int CurLvlUpSkill, string site)
        {
            if (thisWarrior.Experience < (100 + (CurLvlUpSkill * 10)))
            {
                RutonyBot.BotSay(site, "Не хватает опыта. Текущее количество " + Convert.ToString(thisWarrior.Experience) + ". Надо : " + Convert.ToString((100 + (CurLvlUpSkill * 10))));
                return false;
            }
            else
            {
                return true;
            }
        }
        public void Personag(string name, string site)
        {
            Hero thisWarrior = GetWarrior(name, site);
            RutonyBot.BotSay(site, thisWarrior.ToString());
        }
        public void Atack(string name, string site)
        {
            if (CurrentBoss == null || CurrentBoss.CurrentHP == 0)
            {
                if (!File.Exists(filename))
                {
                    RutonyBot.BotSay(site, "Босс еще не появился! Попросите администратора об этом!");
                    return;
                }
                else
                {
                    CurrentBoss = JsonConvert.DeserializeObject<currentBoss>(File.ReadAllText(filename));
                }
            }

            Hero thiswarrior = GetWarrior(name, site);
            if (thiswarrior.CurrentHP <= 0)
            {
                RutonyBot.BotSay(site, name + ", у тебя не осталось здоровья. You died.");
                return;
            }

            int rndAtack = rnd.Next(1, 100);
            int rndAvoid = rnd.Next(1, 100);
            int rndBlock = rnd.Next(1, 100);

            int CurDamage = 0;
            int CurDamageBoss = 0;

            string OutPutMessage;
            if (rndAvoid <= CurrentBoss.ChanceAvoid)
            {
                CurDamage = 0;
                OutPutMessage = string.Format("{1} уклоняется от удара {0}", name, CurrentBoss.Name);
            }
            else if (rndBlock <= CurrentBoss.ChanceBlock)
            {
                CurDamage = 0;
                CurDamageBoss = (CurrentBoss.Damage - thiswarrior.Armor);
                CurDamageBoss = Math.Max(1, CurDamageBoss);
                thiswarrior.CurrentHP -= CurDamageBoss;

                OutPutMessage = string.Format("{1} блокирует удар {0} и наносит в ответ {2} урона", name, CurrentBoss.Name, CurDamageBoss);
            }
            else
            {
                CurDamage += (thiswarrior.Damage - CurrentBoss.Armor);
                CurDamage = Math.Max(1, CurDamage);
                CurrentBoss.CurrentHP -= CurDamage;
                OutPutMessage = string.Format("{0} бьет {3}а на {1} урона! У {3} осталось {2} здоровья!", name, CurDamage, CurrentBoss.CurrentHP, CurrentBoss.Name);
            }
            thiswarrior.DoneDamage += CurDamage;
            RutonyBotFunctions.FileAddString(fileLogi, OutPutMessage);
            UpdateLabels();

            if (CurrentBoss.CurrentHP <= 0)
            {
                string format = "";
                RutonyBot.BotSay(site, string.Format("{0} добивает {1}а! Всем участники получают опыт!", name, CurrentBoss.Name));
                foreach (Hero player in players.ListWarriors)
                {
                    format += "" + player.Name + " + " + player.DoneDamage + " EXP" + Environment.NewLine;
                }
                savePlayerList();
                LabelBase.DictLabels[LabelBase.LabelType.Counter2].Format = format;
                LabelBase.DictLabels[LabelBase.LabelType.Counter2].Save();

                try
                {
                    File.Delete(filename);
                }
                catch { }
                try
                {
                    File.Delete(filenamejpg);
                }
                catch { }
            }
        }
        public void UpdateLabels()
        {
            LabelBase.DictLabels[LabelBase.LabelType.Counter1].Format = "Name : " + CurrentBoss.Name + Environment.NewLine + "HP : " + CurrentBoss.CurrentHP;
            LabelBase.DictLabels[LabelBase.LabelType.Counter1].Save();

            LabelBase.DictLabels[LabelBase.LabelType.Counter2].Format = "";
            foreach (Hero player in players.ListWarriors)
            {
                LabelBase.DictLabels[LabelBase.LabelType.Counter2].Format += player.Name + "(" + player.CurrentHP + "/" + player.Mana + ")" + Environment.NewLine;
            }
            LabelBase.DictLabels[LabelBase.LabelType.Counter2].Save();
        }
        public void savePlayerList()
        {
            foreach (Hero player in players.ListWarriors)
            {
                string fileplayerHeroe = FileDirectory + @"\HeroeList\" + player.Name + ".json";
                player.Experience += player.DoneDamage;
                player.DoneDamage = 0;
                player.CurrentHP = player.HP;
                player.CurrentMana = player.Mana;

                string serialized = JsonConvert.SerializeObject(player);
                File.WriteAllText(fileplayerHeroe, serialized);
            }
        }
        public void Heal(string name, string site, string text)
        {
            if (CurrentBoss.CurrentHP == 0)
            {
                if (!File.Exists(filename))
                {
                    RutonyBot.BotSay(site, "Босс еще не появился! Попросите администратора об этом!");
                    return;
                }
                else
                {
                    CurrentBoss = JsonConvert.DeserializeObject<currentBoss>(File.ReadAllText(filename));
                }
            }
            Hero thiswarrior = GetWarrior(name, site);
            if ((thiswarrior.CurrentMana / 5) <= 0)
            {
                RutonyBot.BotSay(site, name + ", у тебя не хватает маны");
                return;
            }

            string[] arrSplit = text.Split(' ');
            string VoteName = "";
            if (arrSplit.Length != 2)
            {
                VoteName = name;
            }
            else
            {
                arrSplit[1] = arrSplit[1].Replace('@', ' ').Trim();
                VoteName = arrSplit[1];
            }

            Hero warriorForheal = GetWarrior(VoteName, site);
            if (VoteName == name)
            {
                warriorForheal = thiswarrior;
            }

            if (warriorForheal.CurrentHP <= 0)
            {
                RutonyBot.BotSay(site, name + " извини, но " + VoteName + " мертв");
                return;
            }

            thiswarrior.CurrentMana -= 5;
            int CurHeal = 0;
            if ((warriorForheal.CurrentHP + thiswarrior.heroSkill.Heal) > warriorForheal.HP)
            {
                CurHeal = (warriorForheal.HP - warriorForheal.CurrentHP);
            }
            else
            {
                CurHeal = thiswarrior.heroSkill.Heal;
            }
            warriorForheal.CurrentHP += CurHeal;
            thiswarrior.DoneDamage += CurHeal;
            RutonyBotFunctions.FileAddString(fileLogi, name + " отхилил " + VoteName + " на " + CurHeal + " хп");
            UpdateLabels();
        }
        public void MassHeal(string name, string site)
        {
            if (CurrentBoss.CurrentHP == 0)
            {
                if (!File.Exists(filename))
                {
                    RutonyBot.BotSay(site, "Босс еще не появился! Попросите администратора об этом!");
                    return;
                }
                else
                {
                    CurrentBoss = JsonConvert.DeserializeObject<currentBoss>(File.ReadAllText(filename));
                }
            }

            Hero thiswarrior = GetWarrior(name, site);
            if ((thiswarrior.CurrentMana / 50) <= 0)
            {
                RutonyBot.BotSay(site, name + ", у тебя не хватает маны");
                return;
            }

            thiswarrior.CurrentMana -= 50;

            int SumHeal = 0;
            int CurHeal = 0;
            foreach (Hero player in players.ListWarriors)
            {
                if ((player.CurrentHP + thiswarrior.heroSkill.MassHeal) > player.HP)
                {
                    CurHeal = (player.HP - player.CurrentHP);
                }
                else
                {
                    CurHeal = thiswarrior.heroSkill.MassHeal;
                }
                SumHeal += CurHeal;
                player.CurrentHP += CurHeal;
                RutonyBot.BotSay(site, name + " отхилил " + player.Name + " на " + CurHeal + " хп");
            }
            thiswarrior.DoneDamage += SumHeal;
            UpdateLabels();
        }
        public void NewAlert(string site, string typeEvent, string subplan, string name, string text, float donate, string currency, int qty)
        {

        }
        public Warriors GetListWarriors()
        {
            players = new Warriors();
            return players;
        }
        public listBosses GetListBosses()
        {
            listBosses ListBosses = new listBosses();
            string fileListBosses = FileDirectory + @"\NOD\ListBoss.json";


            if (File.Exists(fileListBosses))
            {
                string[] filetexts = File.ReadAllLines(fileListBosses);

                ListBosses = JsonConvert.DeserializeObject<listBosses>(filetexts[0]);

            }

            foreach (currentBoss boss in ListBosses.ListBosses)
            {
                string filenameBoss = FileDirectory + @"\NOD\" + boss.Name + ".json";
                currentBoss fileBoss = JsonConvert.DeserializeObject<currentBoss>(File.ReadAllText(filenameBoss));
                boss.HP = fileBoss.HP;
                boss.Armor = fileBoss.Armor;
                boss.ChanceAvoid = fileBoss.ChanceAvoid;
                boss.ChanceBlock = fileBoss.ChanceBlock;
                boss.CurrentHP = fileBoss.CurrentHP;
                boss.Damage = fileBoss.Damage;
            }
            return ListBosses;
        }
        public Hero GetWarrior(string username, string site)
        {
            Hero thiswarrior = players.ListWarriors.Find(r => r.Name == username.Trim().ToLower());
            if (thiswarrior == null)
            {
                AddWarrior(username, 0, site);
                thiswarrior = players.ListWarriors.Find(r => r.Name == username.Trim().ToLower());
            }
            return thiswarrior;

        }
        public void AddWarrior(string username, int vklad, string site)
        {
            Hero thiswarrior = players.ListWarriors.Find(r => r.Name == username.Trim().ToLower());
            string serializedSkill = "";
            string serialized = "";

            if (thiswarrior == null)
            {
                string fileHeroe = FileDirectory + @"\HeroeList\" + username + ".json";
                string fileHeroeSkill = FileDirectory + @"\HeroeList\" + username + "Skill.json";
                string fileDefaultSkill = FileDirectory + @"\NOD\DefaultHeroSkill.json";
                string fileDefault = FileDirectory + @"\NOD\DefaultHero.json";

                thiswarrior = new Hero();

                if (!File.Exists(fileHeroe))
                {
                    thiswarrior = JsonConvert.DeserializeObject<Hero>(File.ReadAllText(fileDefault));
                }
                else
                {
                    thiswarrior = JsonConvert.DeserializeObject<Hero>(File.ReadAllText(fileHeroe));
                }

                if (!File.Exists(fileHeroeSkill))
                {
                    thiswarrior.heroSkill = JsonConvert.DeserializeObject<heroSkill>(File.ReadAllText(fileDefaultSkill));
                }
                else
                {
                    thiswarrior.heroSkill = JsonConvert.DeserializeObject<heroSkill>(File.ReadAllText(fileHeroeSkill));
                }
                thiswarrior.Name = username.Trim().ToLower();
                thiswarrior.DoneDamage = 0;

                serializedSkill = JsonConvert.SerializeObject(thiswarrior.heroSkill);
                File.WriteAllText(fileHeroeSkill, serializedSkill);
                serialized = JsonConvert.SerializeObject(thiswarrior);
                File.WriteAllText(fileHeroe, serialized);

                players.ListWarriors.Add(thiswarrior);
            }
            else
            {
                thiswarrior.DoneDamage += vklad;
            }
        }
    }
    public class Warriors
    {
        public List<Hero> ListWarriors = new List<Hero>();
    }
    public class listBosses
    {
        public List<currentBoss> ListBosses = new List<currentBoss>();
    }
    public class currentBoss
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int CurrentHP { get; set; }
        public int ChanceAvoid { get; set; }
        public int ChanceBlock { get; set; }
        public int Damage { get; set; }
        public int Armor { get; set; }
    }
    public class Hero
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int CurrentHP { get; set; }
        public int Mana { get; set; }
        public int CurrentMana { get; set; }
        public int Damage { get; set; }
        public int DoneDamage { get; set; }
        public int Armor { get; set; }
        public int Experience { get; set; }
        public heroSkill heroSkill { get; set; }

        override
        public String ToString()
        {
            return ("Name - " + Name + "; "
                    + "HP - " + HP + "; "
                    + "CurrentHP - " + CurrentHP + "; "
                    + "Mana - " + Mana + "; "
                    + "CurrentMana - " + CurrentMana + "; "
                    + "Damage - " + Damage + "; "
                    + "Armor - " + Armor + "; "
                    + "Experience - " + Experience);
        }

    }
    public class heroSkill
    {
        public int Heal { get; set; }
        public int MassHeal { get; set; }
    }

}