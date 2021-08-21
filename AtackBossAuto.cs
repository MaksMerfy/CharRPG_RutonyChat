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
        public string file = "";
        public string fileHeroe = "";
        public string fileHeroeSkill = "";
        public string format = "";
        public string fileLogi = "";

        public Random rnd = new Random();

        private string twnick = "";
        public Warriors players;


        public void InitParams(string param)
        {
            FileDirectory = RutonyBotFunctions.GetScriptDirectory("AtackBossAuto.cs");
            if (FileDirectory == "")
            {
                FileDirectory = ProgramProps.dir_scripts;
            }
            filename = FileDirectory + @"\CurrentBoss.json";
            filenamejpg = FileDirectory + @"\CurrentBoss.png";
            file = FileDirectory + @"\dragonwarriors.json";
            fileLogi = FileDirectory + @"\Logirovanie.txt";
            twnick = RutonyBot.TwitchBot.NICK.ToLower();
			players = GetListWarriors();
            RutonyBot.SayToWindow("Скрипт атаки босса успешно подключен");
        }

        public void Closing()
        {
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
            bool TextHasAtack = text.ToLower().Contains("!атака");
            if (TextHasAtack == true)
            {
                Atack();
            }

            bool TextHasHeal = text.ToLower().Contains("!хил");
            if (TextHasHeal == true)
            {
                Heal();
            }
            bool TextHasMassHeal = text.ToLower().Contains("!массхил");
            if (TextHasMassHeal == true)
            {
                MassHeal();
            }
        }

        public void Atack()
        {
            fileHeroe = FileDirectory + @"\HeroeList\" + name + ".json";
            if (!File.Exists(filename))
            {
                RutonyBot.BotSay(site, "Босс еще не появился! Попросите администратора об этом!");
                return;
            }
            if (!File.Exists(fileHeroe))
            {
                RutonyBot.BotSay(site, "В начале требуется создать своего героя");
                return;
            }

            STCurrentHeroe = JsonConvert.DeserializeObject<StatHeroes>(File.ReadAllText(fileHeroe));
            if (STCurrentHeroe.CurrentHP <= 0)
            {
                RutonyBot.BotSay(site, name + ", у тебя не осталось здоровья. You died.");
                return;
            }
            STCurrentBoss = JsonConvert.DeserializeObject<StatCurrentBoss>(File.ReadAllText(filename));

            int rndAtack = rnd.Next(1, 100);
            int rndAvoid = rnd.Next(1, 100);
            int rndBlock = rnd.Next(1, 100);

            int CurDamage = 0;
            int CurDamageBoss = 0;

            string OutPutMessage;
            string serializedTakeDamage;
            if (rndAvoid <= STCurrentBoss.ChanceAvoid)
            {
                CurDamage = 0;
                OutPutMessage = string.Format("{1} уклоняется от удара {0}", name, STCurrentBoss.Name);
            }
            else if (rndBlock <= STCurrentBoss.ChanceBlock)
            {
                CurDamage = 0;
                CurDamageBoss = (STCurrentBoss.Damage - STCurrentHeroe.Armor);
                CurDamageBoss = Math.Max(1, CurDamageBoss);
                STCurrentHeroe.CurrentHP -= CurDamageBoss;

                serializedTakeDamage = JsonConvert.SerializeObject(STCurrentHeroe);
                File.WriteAllText(fileHeroe, serializedTakeDamage);

                OutPutMessage = string.Format("{1} блокирует удар {0} и наносит в ответ {2} урона", name, STCurrentBoss.Name, CurDamageBoss);
            }
            else
            {
                CurDamage += (STCurrentHeroe.Damage - STCurrentBoss.Armor);
                CurDamage = Math.Max(1, CurDamage);
                STCurrentBoss.CurrentHP -= CurDamage;
                OutPutMessage = string.Format("{0} бьет {3}а на {1} урона! У {3} осталось {2} здоровья!", name, CurDamage, STCurrentBoss.CurrentHP, STCurrentBoss.Name);
            }
            AddWarrior(name, CurDamage, site);
            RutonyBotFunctions.FileAddString(fileLogi, OutPutMessage);

            players = GetListWarriors();
            format = "";
            if (STCurrentBoss.CurrentHP > 0)
            {
                try
                {
                    File.Delete(filename);
                }
                catch { }

                string serialized = JsonConvert.SerializeObject(STCurrentBoss);
                File.WriteAllText(filename, serialized);
                return;
            }

            if (STCurrentBoss.CurrentHP <= 0)
            {
                RutonyBot.BotSay(site, string.Format("{0} добивает {1}а! Всем участники получают опыт!", name, STCurrentBoss.Name));
                foreach (Warrior player in players.ListWarriors)
                {
                    string fileplayerHeroe = FileDirectory + @"\HeroeList\" + player.name + ".json";
                    STCurrentHeroe = JsonConvert.DeserializeObject<StatHeroes>(File.ReadAllText(fileplayerHeroe));
                    STCurrentHeroe.Experience += player.damage;
                    STCurrentHeroe.CurrentHP = STCurrentHeroe.HP;
                    STCurrentHeroe.CurrentMana = STCurrentHeroe.Mana;

                    string serialized = JsonConvert.SerializeObject(STCurrentHeroe);
                    File.WriteAllText(fileplayerHeroe, serialized);

                    RutonyBotFunctions.FileAddString(fileLogi, player.name + " получил " + player.damage + " опыта!");

                    format += "" + player.name + " + " + player.damage + " EXP" + Environment.NewLine;
                }
                LabelBase.DictLabels[LabelBase.LabelType.Counter2].Format = format;
                LabelBase.DictLabels[LabelBase.LabelType.Counter2].Save();

                try
                {
                    File.Delete(file);
                }
                catch { }

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

        public void Heal()
        {
            fileHeroe = FileDirectory + @"\HeroeList\" + name + ".json";
            fileHeroeSkill = FileDirectory + @"\HeroeList\" + name + "Skill.json";

            if (!File.Exists(filename))
            {
                RutonyBot.BotSay(site, "Босс еще не появился! Попросите администратора об этом!");
                return;
            }

            STCurrentHeroe = JsonConvert.DeserializeObject<StatHeroes>(File.ReadAllText(fileHeroe));
            if ((STCurrentHeroe.CurrentMana / 5) <= 0)
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

            StatHeroes STPlayerForHeal;
            string fileHeroForHeal = FileDirectory + @"\HeroeList\" + VoteName + ".json";
            if (!File.Exists(fileHeroForHeal))
            {
                RutonyBot.BotSay(site, name + ", не нашли игрока " + VoteName);
                return;
            }
            if (VoteName == name)
            {
                STPlayerForHeal = STCurrentHeroe;
            }
            else
            {
                STPlayerForHeal = JsonConvert.DeserializeObject<StatHeroes>(File.ReadAllText(fileHeroForHeal));
            }
            STCurrentHeroSkill = JsonConvert.DeserializeObject<StatHeroSkill>(File.ReadAllText(fileHeroeSkill));
            if (STPlayerForHeal.CurrentHP <= 0)
            {
                RutonyBot.BotSay(site, name + " извини, но " + VoteName + " мертв");
                return;
            }

            STCurrentHeroe.CurrentMana -= 5;
            int CurHeal = 0;
            if ((STPlayerForHeal.CurrentHP + STCurrentHeroSkill.Heal) > STPlayerForHeal.HP)
            {
                CurHeal = (STPlayerForHeal.HP - STPlayerForHeal.CurrentHP);
            }
            else
            {
                CurHeal = STCurrentHeroSkill.Heal;
            }
            STPlayerForHeal.CurrentHP += CurHeal;
            AddWarrior(name, CurHeal, site);

            if (name == VoteName)
            {
                string serialized = JsonConvert.SerializeObject(STCurrentHeroe);
                File.WriteAllText(fileHeroe, serialized);
            }
            else
            {
                string serialized = JsonConvert.SerializeObject(STPlayerForHeal);
                File.WriteAllText(fileHeroForHeal, serialized);
                serialized = JsonConvert.SerializeObject(STCurrentHeroe);
                File.WriteAllText(fileHeroe, serialized);
            }
            RutonyBotFunctions.FileAddString(fileLogi, name + " отхилил " + VoteName + " на " + CurHeal + " хп");
        }

        public void MassHeal()
        {
            fileHeroe = FileDirectory + @"\HeroeList\" + name + ".json";
            fileHeroeSkill = FileDirectory + @"\HeroeList\" + name + "Skill.json";

            if (!File.Exists(filename))
            {
                RutonyBot.BotSay(site, "Босс еще не появился! Попросите администратора об этом!");
                return;
            }

            STCurrentHeroe = JsonConvert.DeserializeObject<StatHeroes>(File.ReadAllText(fileHeroe));
            if ((STCurrentHeroe.CurrentMana / 50) <= 0)
            {
                RutonyBot.BotSay(site, name + ", у тебя не хватает маны");
                return;
            }

            STCurrentHeroe.CurrentMana -= 50;
            string serialized = JsonConvert.SerializeObject(STCurrentHeroe);
            File.WriteAllText(fileHeroe, serialized);

            players = GetListWarriors();
            StatHeroes STPlayerForHeal;
            STCurrentHeroSkill = JsonConvert.DeserializeObject<StatHeroSkill>(File.ReadAllText(fileHeroeSkill));

            int SumHeal = 0;
            int CurHeal = 0;

            string fileCurHero = "";
            string serializedPlayerForHeal = "";
            foreach (Warrior player in players.ListWarriors)
            {
                fileCurHero = FileDirectory + @"\HeroeList\" + player.name + ".json";
                STPlayerForHeal = JsonConvert.DeserializeObject<StatHeroes>(File.ReadAllText(fileCurHero));
                if ((STPlayerForHeal.CurrentHP + STCurrentHeroSkill.MassHeal) > STPlayerForHeal.HP)
                {
                    CurHeal = (STPlayerForHeal.HP - STPlayerForHeal.CurrentHP);
                }
                else
                {
                    CurHeal = STCurrentHeroSkill.MassHeal;
                }
                SumHeal += CurHeal;
                STPlayerForHeal.CurrentHP += CurHeal;

                serializedPlayerForHeal = JsonConvert.SerializeObject(STPlayerForHeal);
                File.WriteAllText(fileCurHero, serializedPlayerForHeal);

                RutonyBotFunctions.FileAddString(fileLogi, name + " отхилил " + player.name + " на " + CurHeal + " хп");
            }
            AddWarrior(name, SumHeal, site);
        }
        public void NewAlert(string site, string typeEvent, string subplan, string name, string text, float donate, string currency, int qty)
        {

        }

        public Warriors GetListWarriors()
        {
            players = new Warriors();
            if (File.Exists(file))
            {
                players = JsonConvert.DeserializeObject<Warriors>(File.ReadAllText(file));
            }
            return players;
        }

        public void AddWarrior(string username, int vklad, string site)
        {
            Warrior thiswarrior = players.ListWarriors.Find(r => r.name == username.Trim().ToLower());

            if (thiswarrior == null)
            {
                players.ListWarriors.Add(new Warrior() { name = username.Trim().ToLower(), damage = vklad });
                thiswarrior = players.ListWarriors.Find(r => r.name == username.Trim().ToLower());

                try
                {
                    File.Delete(file);
                }
                catch { }

                string serialized = JsonConvert.SerializeObject(players);
                RutonyBotFunctions.FileAddString(file, serialized);
            }
            else
            {
                thiswarrior.damage += vklad;
                try
                {
                    File.Delete(file);
                }
                catch { }
                string serialized = JsonConvert.SerializeObject(players);
                RutonyBotFunctions.FileAddString(file, serialized);
            }
        }
    }

    public class Warriors
    {
        public List<Warrior> ListWarriors = new List<Warrior>();
    }

    public class Warrior
    {
        public Hero hero { get; set; }
        public int damage { get; set; }
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
        public int Armor { get; set; }
        public int Experience { get; set; }
        public heroSkill HeroSkill { get; set; }
    }
    public class heroSkill
    {
        public int Heal { get; set; }
        public int MassHeal { get; set; }
    }
}