using GTANetworkAPI;
using NeptuneEvo.Handles;
using NeptuneEvo.Accounts;
using NeptuneEvo.Chars;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.Players;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Character;
using Redage.SDK;
using System;
using System.Linq;

namespace NeptuneEvo.Core
{
    class Commands_new_shardban : Script
    {
        public static readonly nLog Log = new nLog("Core.Commands_new_shardban");

        [Command("shardban", GreedyArg = true)]
        public static void CMD_shardban(ExtPlayer player, int id, int time, string reason)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                if (characterData.AdminLVL < 5) return;

                string playerLogin = player.GetLogin();

                ExtPlayer target = Main.GetPlayerByID(id);
                var targetCharacterData = target.GetCharacterData();
                if (targetCharacterData == null) return;
                if (player == target) return;
                string targetLogin = target.GetLogin();

                int tadmlvl = targetCharacterData.AdminLVL;
                if (tadmlvl == 9)
                {
                    Trigger.SendToAdmins(1, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) versucht einen Hardban {target.Name} ({target.Value}).");
                    Admin.BanMe(player, 0);
                    return;
                }
                else if (tadmlvl != 0 && tadmlvl >= characterData.AdminLVL)
                {
                    Trigger.SendToAdmins(3, $"{ChatColors.StrongOrange}[A] {player.Name} ({player.Value}) Hardban {target.Name} ({target.Value}) und wurde vom System gebannt");

                    Character.BindConfig.Repository.DeleteAdmin(target);
                    Character.BindConfig.Repository.DeleteAdmin(player);

                    Ban.Online(target, DateTime.MaxValue, true, reason, player.Name);
                    Ban.Online(player, DateTime.MaxValue, true, $"Wurde vom System gebannt für Bann eines Admins {target.Name}", "server");

                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Du wurdest lebenslag gebannt weil du {player.Name} einen Admin bebannt hast.", 30000);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Du wurdest lebenslag gebannt weil du {target.Name} einen Admin bebannt hast.", 30000);

                    int AUUID = characterData.UUID;
                    GameLog.Ban(AUUID, targetCharacterData.UUID, targetLogin, DateTime.MaxValue, reason, true);
                    GameLog.Ban(-2, AUUID, playerLogin, DateTime.MaxValue, $"Wurde vom System ausgeschlossen für das Bannen eines Admins {target.Name}", true);

                    target.Kick(reason);
                    player.Kick("Wurde vom System ausgeschlossen für das Bannen eines Admins");
                }
                else
                {
                    if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                    {
                        Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System ausgeschlossen. Grund: {reason}");
                        Character.BindConfig.Repository.DeleteAdmin(player);
                        return;
                    }
                    
                    if (NeptuneEvo.Character.Repository.LoginsBlck.Contains(targetLogin))
                    {
                        Trigger.SendToAdmins(3, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) hat versucht {target.Name} hard zu bannen ({target.Value}).");
                        Admin.BanMe(player, 0);
                        return;
                    }
                   
                    if (!Admin.CheckMe(player, 4)) return;

                    DateTime unbanTime = (time >= 3650) ? DateTime.MaxValue : DateTime.Now.AddDays(time);
                    if (time >= 3650) Trigger.SendToAdmins(1, "!{#FFB833}" + $"[A] {player.Name} hat {target.Name} lebenslang gebannt. Grund: {reason}");
                    else Trigger.SendToAdmins(1, "!{#FFB833}" + $"[A] {player.Name} bant {target.Name} für {time} tage. Grund: {reason}");

                    Ban.Online(target, unbanTime, true, reason, player.Name);
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Du wurdest bis zum {unbanTime.ToString()} gebannt", 30000);
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Grund: {reason}", 30000);
                    
                    int AUUID = characterData.UUID;
                    int TUUID = targetCharacterData.UUID;
                    GameLog.Ban(AUUID, TUUID, targetLogin, unbanTime, reason, true);
                    
                    target.Kick(reason);
                }
            }
            catch (Exception e)
            {
                Log.Write($"CMD_shardban Exception: {e.ToString()}");
            }
        }
    }
}
