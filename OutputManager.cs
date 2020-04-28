﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TitanixClient___Forms
{
    static class OutputManager
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true,
            CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        // ------------------ END OF PINVOKE REGION -----------------

        /*public static bool HasConsole = false;
        public static void CreateConsole()
        {
            AllocConsole();
            HasConsole = true;
        }
        public static void RemoveConsole()
        {
            FreeConsole();
            HasConsole = false;
        }*/

        public static void ProcessText(string text)
        {
            if (text == null) return;
            PartyMessages(text);
            GameMessages(text);
            Console.WriteLine(text);
        }
        static void PartyMessages(string text)
        {
            if (text.Contains("You left the party"))
            {
                Data.party.inParty = false;
                Data.party.members.Clear();
                Data.party.unknownUsers = true;
            }
            if (text.Contains("You joined") && text.Contains("'s party!"))
            {
                string user = text
                    .Split(new string[] { "[CHAT] You joined " }, StringSplitOptions.None)[1]
                    .Split(new string[] { "'s party!" }, StringSplitOptions.None)[0];

                Data.party.inParty = true;
                Data.party.members.Clear();
                HypixelUser hp = new HypixelUser(user);
                Data.party.members.Add(hp.name);
                Data.party.unknownUsers = false;
            }
            if (text.Contains(" joined the party!"))
            {
                // [21:18:01] [Client thread/INFO]: [CHAT] username joined the party!
                string user = text
                    .Split(new string[] { "[CHAT] " }, StringSplitOptions.None)[1]
                    .Split(new string[] { " joined the" }, StringSplitOptions.None)[0];
                Data.party.inParty = true;
                Data.party.unknownUsers = false;
                HypixelUser hp = new HypixelUser(user);
                Data.party.members.Add(hp.name);
            }
            if (text.Contains(" has disbanded the party!") ||
                text.Contains("The party was disbanded because all invites have expired and all members have left."))
            {
                Data.party.inParty = false;
                Data.party.members.Clear();
                Data.party.unknownUsers = true;
            }
            if (text.Contains(" has been removed from your party"))
            {
                string user = text
                    .Split(new string[] { "[CHAT] " }, StringSplitOptions.None)[1]
                    .Split(new string[] { " has been" }, StringSplitOptions.None)[0];
                HypixelUser hp = new HypixelUser(user);
                Data.party.members.Remove(hp.name);
            }
            if (text.Contains(" left the party."))
            {
                string user = text
                    .Split(new string[] { "[CHAT] " }, StringSplitOptions.None)[1]
                    .Split(new string[] { " left the party." }, StringSplitOptions.None)[0];
                HypixelUser hp = new HypixelUser(user);
                Data.party.members.Remove(hp.name);
            }
            if (text.Contains("[CHAT] Party members "))
            {
                Data.party.members.Clear();
                Data.party.inParty = true;
                Data.party.unknownUsers = false;
                // Parse full party.
                // [21:11:01] [Client thread/INFO]: [CHAT] Party members (5): [VIP] 7UKECREATOR ? [MVP+] Yuckr ? [VIP] Hypix__L ? [MVP+] Zenii ? cwxzyy ? 
                string partB = text.Split(new string[] { " [CHAT] " }, StringSplitOptions.None)[1];
                // Party members (5): [VIP] 7UKECREATOR ? [MVP+] Yuckr ? [VIP] Hypix__L ? [MVP+] Zenii ? cwxzyy ? 
                string partC = partB.Split(':')[1].Trim().TrimEnd('?', ' ');
                // [VIP] 7UKECREATOR ? [MVP+] Yuckr ? [VIP] Hypix__L ? [MVP+] Zenii ? cwxzyy
                string[] parts = partC.Split('?');
                foreach(string user in parts)
                {
                    HypixelUser partyMember =
                        new HypixelUser(user);
                    if(partyMember.name.Equals(Data.username))
                        continue;
                    Data.party.members.Add(partyMember.name);
                }
            }
        }
        static void GameMessages(string text)
        {
            foreach(var kv in HypixelGame.gameDetectionStrings)
            {
                string scan = kv.Key;
                string game = kv.Value;
                if (text.Contains(scan))
                    Data.game.gameName = game;
                break;
            }
        }
    }
}
