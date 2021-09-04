using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

namespace RutonyChat
{
    public class Script
    {
        public bool AutoCreateIsActive = false;
        public bool ScriptisActive = false;

        public void InitParams(string param)
        {
            ScriptisActive = true;
            RutonyBot.SayToWindow("Скрипт автосоздания босса успешно подключен");
            doCreateBosses();

        }
        public void Closing()
        {
            ScriptisActive = false;
            AutoCreateIsActive = false;
            RutonyBot.SayToWindow("Скрипт автосоздания босса успешно отключен");
        }
        public void NewMessage(string site, string name, string text, bool system)
        {

        }

        public bool SetAutoCreateIsActive(bool toSet)
        {
            this.AutoCreateIsActive = toSet;
            return this.AutoCreateIsActive;
        }
        public void doCreateBosses()
        {
            /*while (this.ScriptisActive == true)
            {
                RutonyBot.SayToWindow(AutoCreateIsActive.ToString());
                if (this.AutoCreateIsActive == true)
                {
                    foreach (ScriptsControl.AutoScriptItem asItem in ScriptsControl.ListActiveScripts)
                    {
                        if (asItem.Script.ScriptName == "AtackBossAuto.cs")
                        {
                            if (asItem.ScriptThread.CurrentBoss == null){
                                asItem.ScriptThread.CreateBoss("", "twitch");
                            }
                        }

                    }
                }
                try
                {
                    Thread.Sleep(30 * 1000);
                }
                catch { }
            }*/

        }
    }
}