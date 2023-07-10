using GTANetworkAPI;
using NeptuneEvo.Handles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Data;
using Redage.SDK;
using MySqlConnector;
using System.Text.RegularExpressions;
using System.Threading;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.Chars;
using NeptuneEvo.Fractions;
using NeptuneEvo.Functions;
using NeptuneEvo.Accounts;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Players;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Character;
using Database;
using LinqToDB;
using System.Threading.Tasks;
using Localization;
using NeptuneEvo.Fractions.Models;
using NeptuneEvo.Fractions.Player;
using NeptuneEvo.Players.Phone.Messages.Models;
using NeptuneEvo.VehicleData.Data;
using NeptuneEvo.VehicleData.LocalData;
using NeptuneEvo.VehicleData.LocalData.Models;
using Newtonsoft.Json;


namespace NeptuneEvo.Core
{
    class Admin : Script
    {
        public static readonly nLog Log = new nLog("Core.Admin");
        public static bool IsServerStoping = false;
        public static bool TimeChanged = false;
        public static short[] SetTime = new short[3] { 0, 0, 0 };

        public static Dictionary<string, (string, string, int)> ChangeNameQueue = new Dictionary<string, (string, string, int)>(); // CurName, (newName, admName, admLvl)
        public static Dictionary<ExtPlayer, (int, string, int)> SetLeaderQueue = new Dictionary<ExtPlayer, (int, string, int)>(); // targetId (fracid, admName, admLvl)
        public static Dictionary<ExtPlayer, (string, int)> SendCreatorQueue = new Dictionary<ExtPlayer, (string, int)>(); // targetId (admName, admLvl)
        public static Dictionary<int, (string, string, int, DateTime)> GlobalQueue = new Dictionary<int, (string, string, int, DateTime)>(); // QueueID (globaltext, admName, admLvl, globalexp)
        public static int GlobalID = 0;



        public static void RemoveQueue(ExtPlayer player, string text = "Spieler Disconnect")

        {

            if (SetLeaderQueue.ContainsKey(player))

            {

                SetLeaderQueue.Remove(player);

                Trigger.SendToAdmins(2, $"{ChatColors.StrongOrange}[A] Der Leaderantrag von {player.Name} ({player.Value}) wurde aufgrund von {text} entfernt.");

            }

            if (SendCreatorQueue.ContainsKey(player))

            {

                SendCreatorQueue.Remove(player);

                Trigger.SendToAdmins(2, $"{ChatColors.StrongOrange}[A] Anfrage für das Ändern des Aussehens für {player.Name} ({player.Value}) wurde aufgrund von {text} gelöscht.");

            }

        }
        public static void AdminLog(int myadm, string message)
        {
            try
            {
                message = "!{#636363}[A] " + message;
                int hisadm = 0;
                foreach (ExtPlayer foreachPlayer in Main.AllAdminsOnline)
                {

                    var foreachCharacterData = foreachPlayer.GetCharacterData();

                    if (foreachCharacterData == null) 
                        continue;

                    hisadm = foreachCharacterData.AdminLVL;



                    var foreachAdminConfig = foreachCharacterData.ConfigData.AdminOption;

                    if (foreachAdminConfig.ALog && hisadm >= 6 && hisadm >= myadm)
                        Trigger.SendChatMessage(foreachPlayer, message);
                }
            }
            catch (Exception e)
            {
                Log.Write($"AdminLog Exception: {e.ToString()}");
            }
        }

        public static void AdminsLog(int myadm, string message, byte levels_to = 2, string color = "#FFB833", bool highRankActionHide = true, byte hideAdminLevel = 8)
        {
            try
            {
                if (myadm >= hideAdminLevel && highRankActionHide == true) return; // Не логировать в чат действия от спец.администратора и выше
                message = "!{" + color + "}[A] " + message;// "!{#FFB833}[A] " + message;
                foreach (ExtPlayer foreachPlayer in Main.AllAdminsOnline)
                {

                    var foreachCharacterData = foreachPlayer.GetCharacterData();

                    if (foreachCharacterData == null) continue;
                    if (foreachCharacterData.AdminLVL >= levels_to) Trigger.SendChatMessage(foreachPlayer, message);
                }
            }
            catch (Exception e)
            {
                Log.Write($"AdminsLog Exception: {e.ToString()}");
            }
        }

        public static void ErrorLog(string message)
        {
            try
            {
                message = "!{#d35400}[A][ELOG] " + message;
                foreach (ExtPlayer foreachPlayer in Main.AllAdminsOnline)
                {                    

                    var foreachCharacterData = foreachPlayer.GetCharacterData();

                    if (foreachCharacterData == null) 

                        continue;



                    var foreachAdminConfig = foreachCharacterData.ConfigData.AdminOption;

                    if (foreachAdminConfig.ELog && foreachCharacterData.AdminLVL >= 6) 
                        Trigger.SendChatMessage(foreachPlayer, message);
                }
            }
            catch (Exception e)
            {
                Log.Write($"ErrorLog Exception: {e.ToString()}");
            }
        }

        public static void WinLog(string message)
        {
            try
            {
                message = "!{#EAEAB3}[A][WINLOG] " + message;
                foreach (ExtPlayer foreachPlayer in Main.AllAdminsOnline)
                {
                    var foreachCharacterData = foreachPlayer.GetCharacterData();
                    if (foreachCharacterData == null)
                        continue;
                    
                    var foreachAdminConfig = foreachCharacterData.ConfigData.AdminOption;
                    if (foreachAdminConfig.WinLog && foreachCharacterData.AdminLVL >= 5) 
                        Trigger.SendChatMessage(foreachPlayer, message);
                }
            }
            catch (Exception e)
            {
                Log.Write($"WinLog Exception: {e.ToString()}");
            }
        }
        public static void onPlayerDeathHandler(ExtPlayer player, ExtPlayer entityKiller, uint weapon, Vector3 pos)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) 
                    return;

                var characterData = player.GetCharacterData();
                if (characterData == null) 
                    return;
                
                characterData.Deaths++;

                var killerCharacterData = entityKiller.GetCharacterData();
                if (killerCharacterData != null)
                {
                    killerCharacterData.Kills++;
                    sessionData.DeathData.LastDeath = $"{DateTime.Now.ToString("s")} | {entityKiller.Name} ({entityKiller.Value}) tötet {player.Name} mit {weapon}";
                    GameLog.Kills($"player({killerCharacterData.UUID})", weapon.ToString(), $"player({characterData.UUID})", $"{pos.X} {pos.Y} {pos.Z}");

                    var killerSessionData = entityKiller.GetSessionData();
                    if (killerSessionData != null && killerSessionData.KillData.DamageDisabled == true)
                    {
                        killerSessionData.KillData.Count++;
                        if (killerSessionData.KillData.Count == 1) Notify.Send(entityKiller, NotifyType.Info, NotifyPosition.BottomCenter, $"Wenn du noch eine Person tötest, bevor du Level 1 erreichst, wirst du gekickt", 10000);
                        else if (killerSessionData.KillData.Count >= 2) entityKiller.Kick();
                    }
                }
                else
                {
                    GameLog.Kills($"system", "", $"player({characterData.UUID})", $"{pos.X} {pos.Y} {pos.Z}");
                    //message = $"~r~{player.Name}({player.Value}) ~s~умер";
                }
                
                foreach (ExtPlayer foreachPlayer in Main.AllAdminsOnline)
                {
                    var foreachCharacterData = foreachPlayer.GetCharacterData();
                    if (foreachCharacterData == null)
                        continue;

                    var foreachAdminConfig = foreachCharacterData.ConfigData.AdminOption;
                    if (foreachAdminConfig.KillList == 0) 
                        continue;
                    if (foreachAdminConfig.KillList == 2 && foreachPlayer.Position.DistanceTo2D(player.Position) > 300) 
                        continue;

                    SendKillLog(foreachPlayer, entityKiller, player, weapon);
                }
            }
            catch (Exception e)
            {
                Log.Write($"onPlayerDeathHandler Exception: {e.ToString()}");
            }
        }
        
        public static void SendKillLog(ExtPlayer player, ExtPlayer killer, ExtPlayer victim, uint weapon)
        {
            Trigger.ClientEvent(player, "hud.kill", killer != null ? killer.Id : -1, victim != null ? victim.Id : -1, weapon);
        }
        public static void sendRedbucks(ExtPlayer player, ExtPlayer target, int amount)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Givereds)) return;
                var targetAccountData = target.GetAccountData();
                if (targetAccountData == null) return;
                if (!target.IsCharacterData()) return;
                if (targetAccountData.RedBucks + amount < 0) amount = 0;
                UpdateData.RedBucks(target, amount, msg: "Versenden von RC");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RbOutcome, target.Name, amount), 6000);
                //Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RbIncome, amount, player.Name), 6000);
                GameLog.Admin(player.Name, $"givereds({amount})", target.Name);
                Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.RedAge, LangFunc.GetText(LangType.De, DataName.RbIncome, amount, player.Name), DateTime.Now);
            }
            catch (Exception e)
            {
                Log.Write($"sendRedbucks Exception: {e.ToString()}");
            }
        }
        public static void giveFreeCase(ExtPlayer player, ExtPlayer target, byte caseid)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Givecase)) return;

                var targetAccountData = target.GetAccountData();
                if (targetAccountData == null) return;
                if (caseid < 0 || caseid > 2) return;
                Chars.Repository.AddNewItemWarehouse(target, ItemId.Case0 + caseid, 1);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Sie geben {target.Name} ({target.Value}) das öffnen einer Free Case #{caseid}", 6000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Sie haben von {player.Name} die möglichkeit bekommen ein Free Case #{caseid} zu öffnen", 6000);
                GameLog.Admin(player.Name, $"givecase{caseid}", target.Name);
            }
            catch (Exception e)
            {
                Log.Write($"giveFreeCase Exception: {e.ToString()}");
            }
        }
        public static void stopServer(ExtPlayer sender, string reason = "Es findet ein geplanter Server-Neustart statt!")
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(sender, AdminCommands.Restart)) return;
                stopServer($"{sender.Name}", reason);
            }
            catch (Exception e)
            {
                Log.Write($"stopServer Exception: {e.ToString()}");
            }
        }

        public static async void SaveServer()
        {
            try
            {
                foreach (var foreachPlayer in Character.Repository.GetPlayers())
                    foreachPlayer.SavePlayerPosition();

                var vehiclesLocalData = RAGE.Entities.Vehicles.All.Cast<ExtVehicle>()
                    .Where(v => v.VehicleLocalData != null)
                    .Where(v => v.VehicleLocalData.Access == VehicleAccess.Garage || v.VehicleLocalData.Access == VehicleAccess.Personal)
                    .ToList();

                foreach (var vehicle in vehiclesLocalData)
                    vehicle.SavePosition();

                await NAPI.Task.WaitForMainThread();

                Trigger.SetTask(async () =>
                { 
                    try
                    {
                        Log.Write("Saving Database...");
                        
                        await using var db = new ServerBD("MainDB");//В отдельном потоке
                        
                        foreach (ExtPlayer foreachPlayer in Character.Repository.GetPlayers())
                        {
                            try
                            {
                                var targetSessionData = foreachPlayer.GetSessionData();
                                if (targetSessionData == null) continue;
                                if (!targetSessionData.IsConnect)
                                    continue;
                                
                                var targetCharacterData = foreachPlayer.GetCharacterData();
                                if (targetCharacterData == null) 
                                    continue;
                                
                                await Character.Save.Repository.SaveSql(db, foreachPlayer);
                                await Accounts.Save.Repository.SaveSql(db, foreachPlayer);
                            }
                            catch (Exception e)
                            {
                                Log.Write($"saveDatabase Foreach #1 Exception: {e.ToString()}");
                            }
                        }
                        Log.Write("Players and Accounts has been saved to DB");
                        /*foreach (int acc in MoneySystem.Bank.Accounts.Keys)
                        {
                            try
                            {
                                if (!MoneySystem.Bank.Accounts.ContainsKey(acc)) continue;
                                MoneySystem.Bank.Save(acc);
                            }
                            catch (Exception e)
                            {
                                Log.Write($"saveDatabase Foreach #2 Exception: {e.ToString()}");
                            }
                        }
                        Log.Write("Bank Saved");*/
                        foreach (var number in VehicleManager.Vehicles.Keys)
                        {
                            if (!VehicleData.LocalData.Repository.IsVehicleToNumber(VehicleAccess.Personal, number)) continue;
                            await VehicleManager.SaveSql(db, number);
                        }
                        Log.Write("Vehicles has been saved to DB");
                        BusinessManager.SavingBusiness();
                        await Organizations.Manager.SaveOrganizations(db);
                        Houses.HouseManager.SavingHouses();
                        await Stocks.SaveFractions(db);
                        WeaponRepository.SaveWeaponsDB();
                        AlcoFabrication.SaveAlco();
                        await Main.SaveDoorsControl(db);
                        await Players.Phone.Tinder.Repository.Saves(db);
                        //
                        Ban.Delete();
                        
                        Log.Write("Database was saved");
                    }
                    catch (Exception e)
                    {
                        Log.Write($"saveDatabase Exception: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Write($"saveDatabase Exception: {e.ToString()}");
            }            
        }
        
        
        public static void stopServer(string Admin = "server", string reason = "Es findet ein geplanter Server-Neustart statt!")
        {
            try
            {
                Log.Write("Force saving database...", nLog.Type.Warn);
                IsServerStoping = true;
                GameLog.Admin(Admin, $"stopServer({reason})", "");

                Log.Write("Force kicking players...", nLog.Type.Warn);
                foreach (var foreachPlayer in RAGE.Entities.Players.All.Cast<ExtPlayer>())
                {
                    Ems.ReviveFunc(foreachPlayer, true);
                    Trigger.ClientEvent(foreachPlayer, "restart");
                    Trigger.Dimension(foreachPlayer, Dimensions.RequestPrivateDimension(foreachPlayer.Value));
                    
                }

                        
                foreach (var foreachPlayer in RAGE.Entities.Players.All.Cast<ExtPlayer>())
                    Players.Disconnect.Repository.OnPlayerDisconnect(foreachPlayer, DisconnectionType.Kicked, reason, isSave: false);

                Trigger.SetTask(async () =>
                {
                    var speedSave = DateTime.Now;
                    Log.Write($"[{DateTime.Now - speedSave}] All players has kicked!", nLog.Type.Success);
                    
                    await using var db = new ServerBD("MainDB");//В отдельном потоке
                        
                    foreach (var foreachPlayer in RAGE.Entities.Players.All.Cast<ExtPlayer>())
                    {
                        try
                        {
                            var targetSessionData = foreachPlayer.GetSessionData();
                            if (targetSessionData == null) 
                                continue;

                            await Character.Save.Repository.SaveSql(db, foreachPlayer);
                            await Accounts.Save.Repository.SaveSql(db, foreachPlayer);
                        }
                        catch (Exception e)
                        {
                            Log.Write($"saveDatabase Foreach #1 Exception: {e.ToString()}");
                        }
                    }
                    
                    Log.Write($"[{DateTime.Now - speedSave}] All players saved!", nLog.Type.Success);
                    
                    await Accounts.Email.Repository.VerificationsDelete();
                    
                    BusinessManager.SavingBusiness();

                    await Organizations.Manager.SaveOrganizations(db);

                    Houses.HouseManager.SavingHouses(true);

                    await Stocks.SaveFractions(db);

                    WeaponRepository.SaveWeaponsDB();
                    
                    await Main.SaveDoorsControl(db);

                    await Players.Phone.Tinder.Repository.Saves(db);

                    //Log.Write($"[{DateTime.Now - speedSave}] Save property", nLog.Type.Success);
                    
                    //await SaveAllPlayersServer();
                    
                    Log.Write($"[{DateTime.Now - speedSave}] Restart All data has been saved!", nLog.Type.Success);
                    
                    Timers.StartOnce(1000 * 60, () => { Environment.Exit(0); }, true);
                });


            }
            catch (Exception e)
            {
                Log.Write($"stopServer Exception: {e.ToString()}");
            }
        }

        private static async Task SaveAllPlayersServer()
        {
            var isSave = true;
            Console.WriteLine("SaveAllPlayersServer 1");
            
            do
            {
                //Thread.Sleep(1000 * 10);
                
                isSave = RAGE.Entities.Players.All.Cast<ExtPlayer>()
                    .Any(p => !p.IsRestartSaveAccountData || !p.IsRestartSaveCharacterData);
                
                await Task.Delay(1000);
                
            } while (isSave);
            
            Console.WriteLine("SaveAllPlayersServer 2");
        }
        public static void saveCoords(ExtPlayer player, string msg)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Save)) return;
                Vector3 pos = NAPI.Entity.GetEntityPosition(player);
                //pos.Z -= 1.12f;
                Vector3 rot = NAPI.Entity.GetEntityRotation(player);
                if (NAPI.Player.IsPlayerInAnyVehicle(player))
                {
                    var vehicle = (ExtVehicle) player.Vehicle;
                    pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                    rot = NAPI.Entity.GetEntityRotation(vehicle);
                }
                using(StreamWriter saveCoords = new StreamWriter("coords.txt", true, Encoding.UTF8))
                {
                    saveCoords.Write($"-----------------------------------------------\r\n");
                    saveCoords.Write($"{msg}: Position({pos.X}, {pos.Y}, {pos.Z}),\r\n");
                    saveCoords.Write($"{msg}: Rotation({rot.X}, {rot.Y}, {rot.Z}),\r\n");
                    saveCoords.Write($"-----------------------------------------------\r\n");
                    saveCoords.Close();
                }
            }
            catch (Exception e)
            {
                Log.Write($"saveCoords Exception: {e.ToString()}");
            }
        }
        public static void setPlayerAdminGroup(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Setadmin)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) 

                    return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) 
                    return;
                
                if (targetCharacterData.AdminLVL >= 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat bereits Admin Rechte", 3000);
                    return;
                }
                
                Character.Save.Repository.SaveAdminLvl(targetCharacterData.UUID, 1);
                
                targetCharacterData.AdminLVL = 1;

                Character.BindConfig.Repository.InitAdmin(target);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie haben {target.Name} Admin Rechte gegeben", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} hat Ihnen Admin Rechte gegeben", 3000);
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) gibt {target.Name} Admin Rechte. Rang 1", 1, "#FFB833", false);
                GameLog.Admin($"{player.Name}", AdminCommands.Setadmin, $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"setPlayerAdminGroup Exception: {e.ToString()}");
            }
        }
        public static void OffdelPlayerAdminGroup(ExtPlayer player, string targetName)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Offdeladmin)) return;

                var sessionData = player.GetSessionData();
                if (sessionData == null) 
                    return;
                
                var characterData = player.GetCharacterData();
                if (characterData == null) 
                    return;
                
                var target = (ExtPlayer) NAPI.Player.GetPlayerFromName(targetName);
                if (target.IsCharacterData())
                {
                    delPlayerAdminGroup(player, target);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Spieler war Online, deswegen wurde offdeladmin auf deladmin geändert", 3000);
                    return;
                }
                if (!Main.PlayerUUIDs.ContainsKey(targetName))
                {
                    Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.CantFindMan), 6000);
                    return;
                }
                var targetuuid = Main.PlayerUUIDs[targetName];
                
                Trigger.SetTask(async () =>
                {	
                    try
                    {
                        await using var db = new ServerBD("MainDB");//В отдельном потоке

                        var character = await db.Characters
                            .Select(c => new
                            {
                                c.Uuid,
                                c.Firstname,
                                c.Lastname,
                                c.Adminlvl,
                            })
                            .Where(v => v.Uuid == targetuuid)
                            .FirstOrDefaultAsync();

                        if (character == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler wurde nicht gefunden", 3000);
                            return;
                        }
                    
                        if (character.Adminlvl <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat keine Admin Rechte", 3000);
                            return;
                        }
                    
                        if (character.Adminlvl >= characterData.AdminLVL)
                        {
                            Trigger.SendToAdmins(3, "!{#FF0000}" + $"[A] {sessionData.Name} ({sessionData.Value}) hat versucht die Admin Rechte von {targetName} (offline) zu entfernen. Dieser hat aber einen höheren Admin Rang");
                            return;
                        }
                    
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Die haben die Admin Rechte von {targetName} entfernt", 3000);
                        AdminsLog(characterData.AdminLVL, $"{sessionData.Name} ({sessionData.Value}) entfernt von {targetName} Admin Rechte", 1, "#FFB833", false);
                        GameLog.Admin($"{sessionData.Name}", $"OffDelAdmin", $"{targetName}");

                        await db.Characters
                            .Where(c => c.Uuid == character.Uuid)
                            .Set(c => c.Adminlvl, 0)
                            .UpdateAsync();
                    }
                    catch (Exception e)
                    {
                        Log.Write($"OffdelPlayerAdminGroup SetTask Exception: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Write($"OffdelPlayerAdminGroup Exception: {e.ToString()}");
            }
        }
        public static void delPlayerAdminGroup(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Deladmin)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) 
                    return;
                if (player == target)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Du kannst von sich selbst keine Admin Rechte entfernen", 3000);
                    return;
                }

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) 
                    return;
                if (targetCharacterData.AdminLVL >= characterData.AdminLVL)
                {
                    Trigger.SendToAdmins(3, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) hat versucht von {target.Name} ({target.Value}) die Admin Rechte zu entfernen. Dieser hat aber einen höheren Admin Rang");
                    return;
                }
                if (targetCharacterData.AdminLVL < 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat keine Admin Rechte", 3000);
                    return;
                }

                Character.BindConfig.Repository.DeleteAdmin(target);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie haben die Adminrechte von {target.Name} entzogen", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} hat deine Adminrechte enzogen", 3000);
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) enzieht {target.Name} seine Adminrechte", 1, "#FFB833", false);
                GameLog.Admin($"{player.Name}", $"delAdmin", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"delPlayerAdminGroup Exception: {e.ToString()}");
            }
        }
        public static void setPlayerAdminRank(ExtPlayer player, ExtPlayer target, int rank)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Arank)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (player == target)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Sie können sich selbst keinen Rang ausstellen", 3000);
                    return;
                }

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (targetCharacterData.AdminLVL < 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Spieler ist kein Administrator!", 3000);
                    return;
                }
                if (targetCharacterData.AdminLVL >= characterData.AdminLVL)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Du kannst bei diesem Admin keine Rechte ändern", 3000);
                    return;
                }
                if (rank < 1 || rank >= characterData.AdminLVL)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Du kannst diesen Rang nicht vergeben", 3000);
                    return;
                }
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie erteilen {target.Name} {rank} Adminrechte", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} gibt dir {rank} Adminrechte", 3000);
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) gibt dir {target.Name} {rank} Adminrechte", 1, "#FFB833", false);
                targetCharacterData.AdminLVL = rank;
                target.SetSharedData("ALVL", rank);
                Character.Save.Repository.SaveAdminLvl(targetCharacterData.UUID, rank);
                GameLog.Admin($"{player.Name}", $"setAdminRank({rank})", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"setPlayerAdminRank Exception: {e.ToString()}");
            }
        }

        public static void offSetPlayerAdminRank(ExtPlayer player, string targetName, int rank)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Offarank)) return;

                var sessionData = player.GetSessionData();
                if (sessionData == null) 
                    return;
                
                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                if (rank < 1 || rank >= characterData.AdminLVL)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Du kannst diesen Rang nicht vergeben", 3000);
                    return;
                }

                ExtPlayer target = (ExtPlayer) NAPI.Player.GetPlayerFromName(targetName);

                if (target.IsCharacterData())
                {
                    setPlayerAdminRank(player, target, rank);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Spieler war Online, deswegen wurde offarank auf arank geändert", 3000);
                    return;
                }

                if (!Main.PlayerUUIDs.ContainsKey(targetName))
                {
                    Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.CantFindMan), 6000);
                    return;
                }
                var targetuuid = Main.PlayerUUIDs[targetName];

                Trigger.SetTask(async () =>
                {
                    try
                    {
                        await using var db = new ServerBD("MainDB");//В отдельном потоке

                        var character = await db.Characters
                            .Select(c => new
                            {
                                c.Uuid,
                                c.Firstname,
                                c.Lastname,
                                c.Adminlvl,
                            })
                            .Where(v => v.Uuid == targetuuid)
                            .FirstOrDefaultAsync();

                        if (character == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler wurde nicht gefunden", 3000);
                            return;
                        }
                        if (character.Adminlvl <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler ist kein Administrator!", 3000);
                            return;
                        }
                
                        if (character.Adminlvl >= characterData.AdminLVL)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Du kannst bei diesem Admin keine Rechte ändern", 3000);
                            return;
                        }

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie geben {targetName} {rank} Adminrechte", 3000);
                        AdminsLog(characterData.AdminLVL, $"{sessionData.Name} ({sessionData.Value}) gibt {targetName} {rank} Adminrechte", 1, "#FFB833", false);
                        GameLog.Admin($"{sessionData.Name}", $"offSetAdminRank({rank})", $"{targetName}");

                        await db.Characters
                            .Where(c => c.Uuid == character.Uuid)
                            .Set(c => c.Adminlvl, rank)
                            .UpdateAsync();
                    }
                    catch (Exception e)
                    {
                        Log.Write($"offSetPlayerAdminRank SetTask Exception: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Write($"offSetPlayerAdminRank Exception: {e.ToString()}");
            }

        }
        public static void setPlayerVipLvl(ExtPlayer player, ExtPlayer target, int rank, ushort days)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Givevip)) return;



                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (rank > 5 || rank < 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Es ist unmöglich, ein VIP-Konto dieser Stufe auszustellen", 3000);
                    return;
                }
                if(days > 1095)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Die maximale Laufzeit für die VIP-Ausstellung beträgt 3 Jahre (1095 Tage).", 3000);
                    return;
                }

                var targetAccountData = target.GetAccountData();
                if (targetAccountData == null) return;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie geben {target.Name} {Group.GroupNames[rank]}", 3000);
                EventSys.SendCoolMsg(target,"Administrator", "VIP Ausgabe", $"Administrator {player.Name} gibt Ihnen {Group.GroupNames[rank]}", "", 7000);  
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) setzt {target.Name} Status {Group.GroupNames[rank]} auf {days} Tage");
                targetAccountData.VipLvl = rank;
                if (targetAccountData.VipDate > DateTime.Now) targetAccountData.VipDate = targetAccountData.VipDate.AddDays(days);
                else targetAccountData.VipDate = DateTime.Now.AddDays(days);
                GameLog.Admin($"{player.Name}", $"setVipLvl({rank},{days})", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"setPlayerVipLvl Exception: {e.ToString()}");
            }
        }

        public static void setFracLeader(ExtPlayer player, ExtPlayer target, int fractionId)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Setleader)) return;

                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                
                var fractionData = Manager.GetFractionData(fractionId);
                if (fractionData != null)
                {
                    if (!target.IsCharacterData()) return;
                    
                    if (characterData.AdminLVL == 5 && player == target)
                    {
                        AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) ernennt Sie zum Leader der Fraktion ({fractionId}) {target.Name} ({target.Value})");

                        GameLog.Admin($"{player.Name}", $"setFracLeader({fractionId})", $"{target.Name}");

                        if (SetLeaderQueue.ContainsKey(target))
                            SetLeaderQueue.Remove(target);
                        
                        target.RemoveFractionMemberData();
                        target.ClearAccessories();
                        target.AddFractionMemberData(fractionId, fractionData.LeaderRank());
                        Customization.ApplyCharacter(target);
                        
                        EventSys.SendCoolMsg(target,"Administrator", "Leader", $"Sie wurden zum Leader von {Manager.GetName(fractionId)} ernannt", "", 7000);
                        //Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы стали лидером фракции {Manager.GetName(fractionId)}", 3000);
         
                        
                        if (Fractions.FractionClothingSets.FractionMainCloakrooms.ContainsKey(fractionId)) {
                            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.StartWorkDay), 6000);
                            Manager.SetSkin(target);
                        }
                    }
                    else if (characterData.AdminLVL >= 5 || (SetLeaderQueue.ContainsKey(target) && SetLeaderQueue[target].Item1 == fractionId))
                    {
                        if (characterData.AdminLVL <= 4)
                        {
                            if (!SetLeaderQueue.ContainsKey(target)) return;
                            if (SetLeaderQueue[target].Item2 == player.Name)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Das kann nur ein anderer Administrator bestätigen!", 3000);
                                return;
                            }
                            AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) bestätigte den Antrag auf Herausgabe Adminrechte {SetLeaderQueue[target].Item2}");
                            AdminsLog(SetLeaderQueue[target].Item3, $"{SetLeaderQueue[target].Item2} gibt Leader ({fractionId}) {target.Name} ({target.Value})");
                            GameLog.Admin($"{player.Name}", $"setFracLeaderAccept({fractionId})", $"{SetLeaderQueue[target].Item2}");
                            GameLog.Admin($"{Admin.SetLeaderQueue[target].Item2}", $"setFracLeader({fractionId})", $"{target.Name}");
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Sie haben die Anfrage zur Herausgabe der Adminrechte bestätigt", 3000);
                        }
                        else
                        {
                            AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) gibt Leader ({fractionId}) {target.Name} ({target.Value})");
                            GameLog.Admin($"{player.Name}", $"setFracLeader({fractionId})", $"{target.Name}");
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie stellen {target.Name} ale Leader von  {Manager.GetName(fractionId)} ein", 3000);
                        }
                        
                        if (SetLeaderQueue.ContainsKey(target)) 
                            SetLeaderQueue.Remove(target);

                        target.RemoveFractionMemberData();
                        target.ClearAccessories();
                        target.AddFractionMemberData(fractionId, fractionData.LeaderRank());
                        Customization.ApplyCharacter(target);

                        EventSys.SendCoolMsg(target,"Administrator", "Leader", $"Sie sind jetzt Leader der Fraktion {Manager.GetName(fractionId)}", "", 7000);
                       // Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы стали лидером фракции {Manager.GetName(fractionId)}", 3000);

                        if (Fractions.FractionClothingSets.FractionMainCloakrooms.ContainsKey(fractionId)) {
                            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.StartWorkDay), 6000);
                            Manager.SetSkin(target);
                        }
                    }
                    else
                    {
                        if (SetLeaderQueue.ContainsKey(target) && SetLeaderQueue[target].Item1 != fractionId)
                        {
                            SetLeaderQueue.Remove(target);
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Eine alte Leaderanfrage für diesen Charakter wurde entfernt.", 3000);
                        }
                        SetLeaderQueue.Add(target, (fractionId, player.Name, characterData.AdminLVL));
                        Trigger.SendToAdmins(2, "!{#FFB833}" + $"[A] Anfrage von {player.Name} ({player.Value}) vergabe auf Leader ({fractionId}) für {target.Name} ({target.Value}). Um zu bestätigen: /setleader {target.Value} {fractionId}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"setFracLeader Exception: {e.ToString()}");
            }
        }
        public static void delFracLeader(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Delleader)) return;

                var characterData = player.GetCharacterData();
                if (characterData == null) 
                    return;

                var targetMemberFractionData = target.GetFractionMemberData();
                if (targetMemberFractionData == null) 
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat keine Fraktion", 3000);
                    return;
                }
                
                var fractionData = Manager.GetFractionData(targetMemberFractionData.Id);
                if (fractionData == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat keine Fraktion", 3000);
                    return;
                }


                if (targetMemberFractionData.Rank < fractionData.LeaderRank())
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler ist kein Leader", 3000);
                    return;
                }
                
                target.RemoveFractionMemberData();
                target.ClearAccessories();
                Customization.ApplyCharacter(target);
                
                EventSys.SendCoolMsg(target,"Administrator", "Leader", $"{player.Name.Replace('_', ' ')} hat Sie vom Posten Leader entfernt", "", 7000); 
                //Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name.Replace('_', ' ')} снял Вас с поста лидера фракции", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie entfernen {target.Name.Replace('_', ' ')} vom Posten Leader", 3000);
                //Chars.Repository.PlayerStats(target);
                Admin.AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) entfernt Leader von {target.Name} ({target.Value})");
                GameLog.Admin($"{player.Name}", $"delFracLeader", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"delFracLeader Exception: {e.ToString()}");
            }
        }
        public static void delJob(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Deljob)) return;

                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();
                if (targetSessionData == null) return;

                var targetCharacterData = target.GetCharacterData();
                if (targetCharacterData == null) return;
                
                if (targetCharacterData.WorkID != 0)
                {
                    if (targetSessionData.WorkData.OnWork)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler muss keine Arbeitskleidung tragen", 3000);
                        return;
                    }
                    UpdateData.Work(target, 0);
                    //Chars.Repository.PlayerStats(target);
                    Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name.Replace('_', ' ')} entfernt den Job von Ihrem Charakter", 3000);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie entfernen {target.Name.Replace('_', ' ')} von seinem Job", 3000);
                    //Chars.Repository.PlayerStats(target);
                    Admin.AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) entfernt {target.Name} ({target.Value}) vom Job");
                    GameLog.Admin($"{player.Name}", $"delJob", $"{target.Name}");
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler ist Arbeitslos", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"delJob Exception: {e.ToString()}");
            }
        }
        public static void delFrac(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Delfrac)) return;

                var characterData = player.GetCharacterData();
                if (characterData == null) return;

                var targetMemberFractionData = target.GetFractionMemberData();
                if (targetMemberFractionData == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat keine Fraktion", 3000);
                    return;
                }
                
                var fractionData = Manager.GetFractionData(targetMemberFractionData.Id);
                if (fractionData == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat keine Fraktion", 3000);
                    return;
                }

                if (targetMemberFractionData.Rank >= fractionData.LeaderRank())
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler ist Leader der Fraktion", 3000);
                    return;
                }

                target.RemoveFractionMemberData();
                target.ClearAccessories();
                Customization.ApplyCharacter(target);

                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Administrator {player.Name.Replace('_', ' ')} entfernt Sie aus der Fraktion", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie entfernen {target.Name.Replace('_', ' ')} aus der Fraktion", 3000);
                //Chars.Repository.PlayerStats(target);
                Admin.AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) entfernt {target.Name} ({target.Value}) aus der Fraktion");
                GameLog.Admin($"{player.Name}", $"delFrac", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"delFrac Exception: {e.ToString()}");
            }
        }

        public static void giveBankMoney(ExtPlayer player, int bankid, int amount)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Givemoney)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!MoneySystem.Bank.Accounts.ContainsKey(bankid))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ein Spieler mit diser Kontonummer wurde nicht gefunden", 3000);
                    return;
                }
                MoneySystem.Bank.Data data = MoneySystem.Bank.Accounts[bankid];
                if (data.Type != 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Das Bankkonto das Sie eingeben gehört nicht der Person", 3000);
                    return;
                }
                MoneySystem.Bank.Change(bankid, amount, false);
                GameLog.Money($"player({characterData.UUID})", $"bank({bankid})", amount, "admin");
                GameLog.Admin($"{player.Name}", $"giveBankMoney({amount})", $"{data.Holder}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Sie geben {data.Holder} {amount}$ aufs Bankkonto {bankid}", 3000);
                
                //if (amount >= 1000000)
                //    Admin.AdminsLog(1, $"[ВНИМАНИЕ] Игрок {data.Holder} получил {amount}$ единой операцией от {player.Name}({player.Value}) (giveBankMoney)", 1, "#FF0000");
                
                // var target = (ExtPlayer)NAPI.Player.GetPlayerFromName(data.Holder);
                // if (target.IsCharacterData())
                // {
                //     var targetSessionData = target.GetSessionData();
                //     if (targetSessionData == null) return;
                //     
                //     if (amount >= 10000 && targetSessionData.LastBankOperationSum == amount)
                //     {
                //         Admin.AdminsLog(1, $"[ВНИМАНИЕ] Игрок {target.Name}({target.Value}) два раза подряд получил по {amount}$ от {player.Name}({player.Value}) (giveBankMoney)", 1, "#FF0000");
                //         targetSessionData.LastBankOperationSum = 0;
                //     }
                //     else
                //     {
                //         targetSessionData.LastBankOperationSum = amount;
                //     }
                // }
            }
            catch (Exception e)
            {
                Log.Write($"giveBankMoney Exception: {e.ToString()}");
            }
        }
        public static void giveMoney(ExtPlayer player, ExtPlayer target, int amount)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Givemoney)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                MoneySystem.Wallet.Change(target, amount);
                GameLog.Money($"player({characterData.UUID})", $"player({targetCharacterData.UUID})", amount, "admin");
                GameLog.Admin($"{player.Name}", $"giveMoney({amount})", $"{target.Name}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Sie geben {target.Name} {amount}$", 3000);
                Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.Bank, LangFunc.GetText(LangType.De, DataName.MoneyIncome, amount), DateTime.Now);
            }
            catch (Exception e)
            {
                Log.Write($"giveMoney Exception: {e.ToString()}");
            }
        }
        public static void OffGiveMoney(ExtPlayer player, string name, int amount)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Offgivemoney)) return;

                var sessionData = player.GetSessionData();
                if (sessionData == null) 
                    return;
                
                var characterData = player.GetCharacterData();
                if (characterData == null) 
                    return;
                
                if (!Main.PlayerNames.Values.Contains(name) || !Main.PlayerUUIDs.ContainsKey(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ein Spieler mit diesen Namen wurde nicht gefunden", 3000);
                    return;
                }
                ExtPlayer target = (ExtPlayer) NAPI.Player.GetPlayerFromName(name);
                if (target.IsCharacterData())
                {
                    giveMoney(player, target, amount);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Spieler war Online, deswegen wurde offgivemoney заменён на givemoney", 3000);
                    return;
                }
                int targetuuid = Main.PlayerUUIDs[name];
                Trigger.SetTask(async () =>
                {
                    try 
                    {
                        await using var db = new ServerBD("MainDB");//В отдельном потоке

                        var character = await db.Characters
                            .Select(c => new
                            {
                                c.Uuid,
                                c.Money,
                            })
                            .Where(v => v.Uuid == targetuuid)
                            .FirstOrDefaultAsync();

                        if (character == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler wurde nicht gefunden", 3000);
                            return;
                        }
                        
                        var money = Convert.ToInt32(character.Money) + amount;
                        if (money < 0) 
                            money = 0;
                        
                        GameLog.Money($"player({characterData.UUID})", $"player({character.Uuid})", amount, "admin");
                        GameLog.Admin($"{sessionData.Name}", $"offGiveMoney({amount})", $"{name}");
                        //Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.Bank, LangFunc.GetText(LangType.De, DataName.MoneyIncome, amount), DateTime.Now); //НЕ РАБОТАЕТ
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Sie setzten {name} {money}$ (+{amount}$)", 3000);
                        
                        await db.Characters
                            .Where(c => c.Uuid == character.Uuid)
                            .Set(c => c.Money, money)
                            .UpdateAsync();
                    }
                    catch (Exception e)
                    {
                        Log.Write($"OffGiveMoney SetTask Exception: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Write($"OffGiveMoney Exception: {e.ToString()}");
            }
        }
        public static void mutePlayer(ExtPlayer player, ExtPlayer target, int time, string reason)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Mute)) return;
                if (!player.IsCharacterData()) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (player == target) return;
                if (time < 5 || time > 10080)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Sie können einen Mute nicht länger als 10080 Minuten vergeben", 3000);
                    return;
                }
                if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                {
                    Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System entfernt. Grund: {reason}");
                    Character.BindConfig.Repository.DeleteAdmin(player);
                    return;
                }
                if (!CheckMe(player, 0)) return;
                int firstTime = time * 60;
                string deTimeMsg = " Minuten";
                if (time > 60)
                {
                    deTimeMsg = " Stunden";
                    time /= 60;
                    if (time > 24)
                    {
                        deTimeMsg = " Tage";
                        time /= 24;
                    }
                }
                targetCharacterData.Unmute = firstTime;
                targetCharacterData.VoiceMuted = true;
                if (targetSessionData.TimersData.MuteTimer != null) Timers.Stop(targetSessionData.TimersData.MuteTimer);
                targetSessionData.TimersData.MuteTimer = Timers.Start(1000, () => timer_mute(target));
                target.SetSharedData("vmuted", true);
                Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} Muted {target.Name}({target.Value}) auf {time} {deTimeMsg}. Grund: {reason}", target);
                GameLog.Admin($"{player.Name}", $"mutePlayer({time}{deTimeMsg}, {reason})", $"{target.Name}");
                Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.RedAge,$"{player.Name} setzt Mute auf {time} {deTimeMsg}. Grund: {reason}", DateTime.Now); 
            }
            catch (Exception e)
            {
                Log.Write($"mutePlayer Exception: {e.ToString()}");
            }
        }
        public static void unmutePlayer(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!player.IsCharacterData()) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (targetCharacterData.Unmute >= 0)
                {
                    targetCharacterData.Unmute = 1;
                    Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} entfernt Mute von {target.Name}({target.Value})");
                    GameLog.Admin($"{player.Name}", $"unmutePlayer", $"{target.Name}");
                    Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.RedAge,$"{player.Name} entfernt Ihren Mute.", DateTime.Now);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Spieler ist nicht gemutet", 1500);
            }
            catch (Exception e)
            {
                Log.Write($"unmutePlayer Exception: {e.ToString()}");
            }
        }

        public static void OffMutePlayer(ExtPlayer player, string targetName, int time, string reason)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Offmute)) return;
                ExtPlayer target = (ExtPlayer) NAPI.Player.GetPlayerFromName(targetName);
                if (target.IsCharacterData())
                {
                    mutePlayer(player, target, time, reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Spieler war Online, deswegen wurde offmute заменён на mute", 3000);
                    return;
                }
                if (!Main.PlayerNames.Values.Contains(targetName) || !Main.PlayerUUIDs.ContainsKey(targetName))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ein Spieler mit diesen Namen wurde nicht gefunden", 3000);
                    return;
                }
                if (time < 5 || time > 10080)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Sie können den Mute nicht länger als 10080 Minuten vergeben", 3000);
                    return;
                }
                if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                {
                    Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System entfernt. Grund: {reason}");
                    Character.BindConfig.Repository.DeleteAdmin(player);
                    return;
                }
                if (player.Name.Equals(targetName)) return;
                if (!CheckMe(player, 0)) return;
                int firstTime = time * 60;
                string deTimeMsg = " Minuten";
                if (time > 60)
                {
                    deTimeMsg = " Stunden";
                    time /= 60;
                    if (time > 24)
                    {
                        deTimeMsg = " Tage";
                        time /= 24;
                    }
                }
                int targetuuid = Main.PlayerUUIDs[targetName];
                string[] split = targetName.Split('_');

                Character.Save.Repository.SaveUnMute(targetuuid, firstTime);

                Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} Muted {targetName} Offline auf {time} {deTimeMsg}. Grund: {reason}");
                GameLog.Admin($"{player.Name}", $"mutePlayer({time}{deTimeMsg}, {reason})", $"{targetName}");
            }
            catch (Exception e)
            {
                Log.Write($"OffMutePlayer Exception: {e.ToString()}");
            }

        }
        public static void OffUnMutePlayer(ExtPlayer player, string targetName)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Offunmute)) return;
                ExtPlayer target = (ExtPlayer) NAPI.Player.GetPlayerFromName(targetName);
                if (target.IsCharacterData())
                {
                    unmutePlayer(player, target);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Spieler war Online, deswegen wurde offunmute geändert auf unmute", 3000);
                    return;
                }
                if (!Main.PlayerNames.Values.Contains(targetName) || !Main.PlayerUUIDs.ContainsKey(targetName))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ein Spieler mit diesen Namen wurde nicht gefunden", 3000);
                    return;
                }
                if (player.Name.Equals(targetName)) return;
                int targetuuid = Main.PlayerUUIDs[targetName];
                string[] split = targetName.Split('_');

                Character.Save.Repository.SaveUnMute(targetuuid, 0);
                
                Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} Mute von {targetName} aufgehoben. Offline");
                GameLog.Admin($"{player.Name}", $"unmutePlayer", $"{targetName}");
            }
            catch (Exception e)
            {
                Log.Write($"OffUnMutePlayer Exception: {e.ToString()}");
            }
        }



        public static void getRb(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Getlogin)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetAccountData = target.GetAccountData();

                if (targetAccountData == null) return;
                int targetRb = targetAccountData.RedBucks;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"{target.Name} ({target.Value}) - {targetRb} Rockford-Coins", 5000);
                AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) zeigt sich die RC von {target.Name} ({target.Value}) - {targetRb}");
            }
            catch (Exception e)
            {
                Log.Write($"getRb Exception: {e.ToString()}");
            }
        }

        public static void getVip(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.GetVip)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetAccountData = target.GetAccountData();

                if (targetAccountData == null) return;
                int targetVip = targetAccountData.VipLvl;
                DateTime targetVipDate = targetAccountData.VipDate;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"{target.Name} ({target.Value}) - VIP {targetVip} Level bis {targetVipDate} ", 5000);
                AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) zeigt den VIP status von {target.Name} ({target.Value}) - {targetVip}");
            }
            catch (Exception e)
            {
                Log.Write($"getRb Exception: {e.ToString()}");
            }
        }

        public static void getLogin(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Getlogin)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (player == target) return;
                string targetlogin = target.GetLogin();
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Sieler Login {target.Name} ({target.Value}) - {targetlogin}", 5000);
                AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) kontrolliert den Login von {target.Name} ({target.Value}) - {targetlogin}");
            }
            catch (Exception e)
            {
                Log.Write($"getLogin Exception: {e.ToString()}");
            }
        }
        public static bool CheckMe(ExtPlayer player, byte type)
        {
            try
            {

                var sessionData = player.GetSessionData();

                if (sessionData == null) return false;

                var characterData = player.GetCharacterData();

                if (characterData == null) return false;
                if (characterData.AdminLVL == 0) return false;
                if (Main.ServerNumber == 0 || characterData.AdminLVL == 9) return true;
                int alvl = characterData.AdminLVL;
                int limit = (alvl >= 1 && alvl <= 5) ? 5 : 10;
                switch(type)
                {
                    case 0:
                        if(sessionData.AdminData.MuteCount == limit)
                        {
                            Trigger.SendToAdmins(3, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen Überschreitung der Verwarnungen entfernt");
                            BanMe(player, 1);
                            return false;
                        }
                        sessionData.AdminData.MuteCount++;
                        break;
                    case 1:
                        if (sessionData.AdminData.KickCount == limit)
                        {
                            Trigger.SendToAdmins(3, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen Überschreitung der Verwarnungen entfernt");
                            BanMe(player, 1);
                            return false;
                        }
                        sessionData.AdminData.KickCount++;
                        break;
                    case 2:
                        if (sessionData.AdminData.JailCount == limit)
                        {
                            Trigger.SendToAdmins(3, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen Überschreitung der Verwarnungen entfernt");
                            BanMe(player, 1);
                            return false;
                        }
                        sessionData.AdminData.JailCount++;
                        break;
                    case 3:
                        if (sessionData.AdminData.WarnCount == limit)
                        {
                            Trigger.SendToAdmins(3, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen Überschreitung der Verwarnungen entfernt");
                            BanMe(player, 1);
                            return false;
                        }
                        sessionData.AdminData.WarnCount++;
                        break;
                    case 4:
                        if (sessionData.AdminData.BansCount == limit)
                        {
                            Trigger.SendToAdmins(3, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen Überschreitung der Verwarnungen entfernt");
                            BanMe(player, 1);
                            return false;
                        }
                        sessionData.AdminData.BansCount++;
                        break;
                    case 5:
                        if (sessionData.AdminData.AclearCount == 5)
                        {
                            Trigger.SendToAdmins(3, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen Überschreitung der Verwarnungen entfernt");
                            BanMe(player, 1);
                            return false;
                        }
                        sessionData.AdminData.AclearCount++;
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Write($"CheckMe Exception: {e.ToString()}");
                return false;
            }
        }
        public static void BanMe(ExtPlayer player, byte type)
        {
            try
            {

                var accountData = player.GetAccountData();
                if (accountData == null) return;



                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                string msg = "vom System gebannt ";
                switch(type)
                {
                    case 0:
                        msg += "für den Versuch, nicht blockierbare Charaktere zu verbieten";
                        break;
                    case 1:
                        msg += "wegen Überschreitung der Verwarnungen pro Minute";
                        break;
                    default:
                        break;
                }
                Character.BindConfig.Repository.DeleteAdmin(player);
                Ban.Online(player, DateTime.MaxValue, true, msg, "server");
                GameLog.Ban(-2, characterData.UUID, accountData.Login, DateTime.MaxValue, msg, true);
                player.Kick(msg);
            }
            catch (Exception e)
            {
                Log.Write($"BanMe Exception: {e.ToString()}");
            }
        }

        public static void banPlayer(ExtPlayer player, ExtPlayer target, int time, string reason, bool isSBan = false)
        {
            try
            {

                var accountData = player.GetAccountData();
                if (accountData == null) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (isSBan == true && !CommandsAccess.CanUseCmd(player, AdminCommands.Sban)) return;

                if (!isSBan && !CommandsAccess.CanUseCmd(player, AdminCommands.Ban)) return;
                if (player == target) return;
                int tadmlvl = targetCharacterData.AdminLVL;
                if (tadmlvl == 9)
                {
                    Trigger.SendToAdmins(1, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) versuchte {target.Name} ({target.Value}) zu Bannen.");
                    BanMe(player, 0);
                    return;
                }
                else if(tadmlvl != 0 && tadmlvl >= characterData.AdminLVL)
                {
                    Trigger.SendToAdmins(3, $"{ChatColors.StrongOrange}[A] {player.Name} ({player.Value}) Bannt {target.Name} ({target.Value}) und wurde deshalb vom System gebannt");

                    Character.BindConfig.Repository.DeleteAdmin(target);
                    Character.BindConfig.Repository.DeleteAdmin(player);

                    Ban.Online(target, DateTime.MaxValue, false, reason, player.Name);
                    Ban.Online(player, DateTime.MaxValue, false, $"Wurde vom System gebannt für das Bannen eines Administrators {target.Name}", "server");

                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Sie haben {player.Name} Blockiert", 30000);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Sie wurden Blockiert! Grund: Bann eines Admins: {target.Name}.", 30000);

                    GameLog.Ban(characterData.UUID, targetCharacterData.UUID, target.GetLogin(), DateTime.MaxValue, reason, false);
                    GameLog.Ban(-2, characterData.UUID, accountData.Login, DateTime.MaxValue, $"Wurde vom System gebannt für das Bannen eines Administrators {target.Name}", false);
                    
                    target.Kick(reason);
                    player.Kick("Wurde vom System gebannt für das Bannen eines Administrators");
                }
                else
                {
                    if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                    {
                        Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System entfernt. Grund: {reason}");
                        Character.BindConfig.Repository.DeleteAdmin(player);
                        return;
                    }
                    if (!CheckMe(player, 4)) return;
                    DateTime unbanTime = (time >= 3650) ? DateTime.MaxValue : DateTime.Now.AddDays(time);

                    

                    if (time >= 3650)

                    {

                        if (isSBan == true) AdminsLog(characterData.AdminLVL, $"{player.Name} bannt {target.Name}({target.Value}) Grund: {reason}", 1, "#FFB833", false);

                        else Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} blockiert Charakter {target.Name}({target.Value}) Lifetime. Grund: {reason}", target);

                    }

                    else

                    {

                        if (isSBan == true) AdminsLog(characterData.AdminLVL, $"{player.Name} bannt {target.Name}({target.Value}) für {time} Tage. Grund: {reason}", 1, "#FFB833", false);

                        else Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} blockiert Charakter {target.Name}({target.Value}) für {time} Tage. Grund: {reason}.", target);

                    }



                    Ban.Online(target, unbanTime, false, reason, player.Name);

                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Du bist bis zum {unbanTime.Day} {unbanTime.ToString("MMMM")} {unbanTime.Year}y. {unbanTime.Hour}:{unbanTime.Minute}:{unbanTime.Second} (UTC+3). Blockiert", 30000);
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Grund: {reason}", 30000);

                    GameLog.Ban(characterData.UUID, targetCharacterData.UUID, target.GetLogin(), unbanTime, reason, false);
                    target.Kick(reason);
                }
            }
            catch (Exception e)
            {
                Log.Write($"banPlayer Exception: {e.ToString()}");
            }
        }
        public static void banLoginPlayer(ExtPlayer player, string login, int time, string reason)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Banlogin)) return;



                var characterData = player.GetCharacterData();

                if (characterData == null) 
                    return;

                Trigger.SetTask(async () =>
                {
                    try
                    {
                        var ban = await Ban.GetBanToLogin(login);
                        
                        if (ban != null)
                        {
                            var hard = (ban.Ishard > 0) ? "Hard " : "";
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Spieler ist bereits im {hard} Bann", 3000);
                            return;
                        }

                        NAPI.Task.Run(() =>
                        {
                            try
                            {

                                Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Sie Bannen {login} im Offline. Grund: {reason}.", 3000);

                                DateTime unbanTime = DateTime.Now.AddDays(time);

                                Ban.OfflineBanToLogin(login, unbanTime, true, reason, "System");

                                GameLog.Ban(-1, -1, login, unbanTime, reason, true);

                                var target = Accounts.Repository.GetPlayerToLogin(login);

                                if (target.IsAccountData())

                                {

                                    AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat den ({login}) {target.Name} ({target.Value}) Offline Gesperrt. Grund: {reason}");

                                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Du bist bis zum {unbanTime.Day} {unbanTime.ToString("MMMM")} {unbanTime.Year}г. {unbanTime.Hour}:{unbanTime.Minute}:{unbanTime.Second} Blockiert.", 30000);

                                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Grund: {reason}", 30000);

                                    target.Kick(reason);

                                }

                                else AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat den ({login}) Offline Gesperrt. Grund: {reason}");

                            }

                            catch (Exception e)

                            {

                                Log.Write($"banLoginPlayer NAPI.Task Exception: {e.ToString()}");

                            }



                        });

                    }

                    catch (Exception e)

                    {

                        Log.Write($"banLoginPlayer Task Exception: {e.ToString()}");

                    }

                });
            }
            catch (Exception e)
            {
                Log.Write($"banLoginPlayer Exception: {e.ToString()}");
            }
        }

        public static void unbanLoginPlayer(ExtPlayer player, string login)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Unbanlogin)) return;



                var characterData = player.GetCharacterData();

                if (characterData == null) 
                    return;

                if (!Main.Usernames.ContainsKey(login))
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Dieser Login wurde im System nicht gefunden", 3000);
                    return;
                }
                Trigger.SetTask(async () =>
                {
                    try
                    {
                        var isBan = await Ban.PardonLogin(login);

                        if (!isBan)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{login} ist nicht gebannt!", 3000);
                            return;
                        }
                        
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) entbannt ({login})");

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Login wieder freigegeben", 3000);
                            }
                            catch (Exception e)
                            {
                                Log.Write($"unbanLoginPlayer Task.Run Exception: {e.ToString()}");
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Log.Write($"unbanLoginPlayer Task.Run Exception: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Write($"unbanLoginPlayer Exception: {e.ToString()}");
            }
        }
        public static void hardbanPlayer(ExtPlayer player, ExtPlayer target, int time, string reason)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Hardban)) return;

                var accountData = player.GetAccountData();
                if (accountData == null) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (player == target) return;
                int tadmlvl = targetCharacterData.AdminLVL;
                string targetLogin = target.GetLogin();
                if (tadmlvl == 9)
                {
                    Trigger.SendToAdmins(1, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) versucht {target.Name} ({target.Value}) Hardbann.");
                    BanMe(player, 0);
                    return;
                }
                else if (tadmlvl != 0 && tadmlvl >= characterData.AdminLVL)
                {
                    Trigger.SendToAdmins(3, $"{ChatColors.StrongOrange}[A] {player.Name} ({player.Value}) bannt {target.Name} ({target.Value}) und wurde deswegen vom System gebannt");

                    Character.BindConfig.Repository.DeleteAdmin(target);
                    Character.BindConfig.Repository.DeleteAdmin(player);

                    Ban.Online(target, DateTime.MaxValue, true, reason, player.Name);
                    Ban.Online(player, DateTime.MaxValue, true, $"Wurde vom System gebannt für das Bannen eines Administrators {target.Name}", "server");
                    
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Du wurdest LIFETIME von {player.Name} gebannt.", 30000);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Du wurdest LIFETIME von {target.Name} gebannt", 30000);
                    
                    int AUUID = characterData.UUID;
                    GameLog.Ban(AUUID, targetCharacterData.UUID, targetLogin, DateTime.MaxValue, reason, true);
                    GameLog.Ban(-2, AUUID, accountData.Login, DateTime.MaxValue, $"Wurde vom System gebannt für das Bannen von {target.Name}", true);
                    
                    target.Kick(reason);
                    player.Kick("Wurde vom System gebannt für das Bannen eines Admin");
                }
                else
                {
                    if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                    {
                        Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom Systm entfernt. Grund: {reason}");
                        Character.BindConfig.Repository.DeleteAdmin(player);
                        return;
                    }
                    if (Character.Repository.LoginsBlck.Contains(targetLogin))
                    {
                        Trigger.SendToAdmins(3, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) hat versucht {target.Name} ({target.Value}) zu bannen.");
                        BanMe(player, 0);
                        return;
                    }
                    if (!CheckMe(player, 4)) return;
                    DateTime unbanTime = (time >= 3650) ? DateTime.MaxValue : DateTime.Now.AddDays(time);
                    if (time >= 3650) Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} erteilt {target.Name}({target.Value}) einen Bann. Grund: {reason}", target);
                    else Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} erteilt {target.Name}({target.Value}) einen Bann für {time} Tage. Grund: {reason}", target);
                    Ban.Online(target, unbanTime, true, reason, player.Name);
                    EventSys.SendCoolMsg(target,"Administrator", $"BANNHAMMER", $"Sie bekommen einen Bann bis zum {unbanTime.ToString()}. Grund: {reason}. Bis zum nächsten mal!", "", 15000);  
                    //Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Вы получили банхаммер до {unbanTime.ToString()}", 30000);
                    //Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Grund: {reason}", 30000);
                    int AUUID = characterData.UUID;
                    int TUUID = targetCharacterData.UUID;
                    GameLog.Ban(AUUID, TUUID, targetLogin, unbanTime, reason, true);
                    target.Kick(reason);
                }
            }
            catch (Exception e)
            {
                Log.Write($"hardbanPlayer Exception: {e.ToString()}");
            }
        }
        public static void isHardPlayer(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Ishard)) return;
                if (player == target) return;

                if (!target.IsCharacterData()) return;
                Trigger.SetTask(async () =>
                {
                    try
                    {
                        var targetSessionData = target.GetSessionData();

                        if (targetSessionData == null) return;

                        var ban = await Ban.GetIsHard(targetSessionData.RealHWID);

                        if (ban != null)
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"HWID ist mit einem gebannten Account {ban.Account} identisch", 3000);
                            return;
                        }

                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "HWID wurde im Bann nicht gefunden", 3000);

                    }
                    catch (Exception e)
                    {
                        Log.Write($"isHardPlayer Task.Run Exception: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Write($"isHardPlayer Exception: {e.ToString()}");
            }
        }
        public static void offBanPlayer(ExtPlayer player, string name, int time, string reason, bool isHard = false)
        {
            try
            {
                if (!isHard && !CommandsAccess.CanUseCmd(player, AdminCommands.Offban)) return;
                else if (isHard && !CommandsAccess.CanUseCmd(player, AdminCommands.Offhardban)) return;



                var accountData = player.GetAccountData();
                if (accountData == null) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (player.Name == name) return;



                ExtPlayer target = (ExtPlayer) NAPI.Player.GetPlayerFromName(name);

                if (target.IsCharacterData())
                {
                    if (isHard)

                    {

                        hardbanPlayer(player, target, time, reason);

                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Spieler war Online, deswegen wurde offhardban был заменён на hardban", 3000);

                    }
                    else

                    {

                        hardbanPlayer(player, target, time, reason);

                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Spieler war Online, deswegen wurde offhardban был заменён на hardban", 3000);

                    }
                    return;
                }


                string prefix = "";
                if (isHard) 
                    prefix = "Hard";

                string[] split = name.Split("_");



                Trigger.SetTask(async () =>
                {
                    try
                    {
                        await using var db = new ServerBD("MainDB");//В отдельном потоке

                        var targetCharacter = await db.Characters
                            .Select(v => new
                            {
                                v.Uuid,
                                v.Firstname,
                                v.Lastname,
                                v.Adminlvl,
                            })
                            .Where(c => c.Firstname == split[0] && c.Lastname == split[1])
                            .FirstOrDefaultAsync();
                        
                        Banneds ban = null;

                        if (targetCharacter != null)
                            ban = await Ban.GetBanToUUID(db, targetCharacter.Uuid);

                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (targetCharacter == null)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler wurde nicht gefunden", 3000);
                                    return;
                                }
                                if (targetCharacter.Adminlvl == 9)
                                {
                                    Trigger.SendToAdmins(1, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) hat versucht {prefix} {name} zu bannen (offline).");
                                    BanMe(player, 0);
                                    return;
                                }
                                if (targetCharacter.Adminlvl != 0 && targetCharacter.Adminlvl >= characterData.AdminLVL)
                                {

                                    string login = Main.GetLoginFromUUID(targetCharacter.Uuid);

                                    if (login != null && Character.Repository.LoginsBlck.Contains(login))
                                    {

                                        Trigger.SendToAdmins(3, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) hat versucht {prefix} {name} zu bannen (offline).");

                                        BanMe(player, 0);

                                        return;
                                    }

                                    Trigger.SendToAdmins(3, $"{ChatColors.StrongOrange}[A] {player.Name} ({player.Value}) bannt {name} (offline) und wurde vom System gebannt)");



                                    Character.BindConfig.Repository.DeleteAdmin(player);



                                    Ban.OfflineBanToNickName(name, DateTime.MaxValue, isHard, reason, player.Name);

                                    Ban.Online(player, DateTime.MaxValue, isHard, $"Wurde vom System gebannt für das Bannen eines Administrators {name}", "server");



                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Sie wurden LIFETIME gebannt! {prefix} für das bannen eines Admins: {name}.", 30000);



                                    if (login != null) GameLog.Ban(characterData.UUID, targetCharacter.Uuid, login, DateTime.MaxValue, reason, isHard);

                                    else GameLog.Ban(characterData.UUID, targetCharacter.Uuid, "-", DateTime.MaxValue, reason, isHard);



                                    GameLog.Ban(-2, characterData.UUID, accountData.Login, DateTime.MaxValue, $"Wurde vom Sytem gebannt {prefix} für das bannen eines Admins: {name}", isHard);



                                    player.Kick("Wurde gebannt wegen Bann eines Administrators");

                                    return;

                                }



                                if (ban != null)

                                {

                                    string hard = (ban.Ishard > 0) ? "Hard " : "";

                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Spieler ist bereits {hard}Bann", 3000);

                                    return;

                                }



                                if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))

                                {

                                    Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System entfernt. Grund: {reason}");

                                    Character.BindConfig.Repository.DeleteAdmin(player);

                                    return;

                                }



                                if (!CheckMe(player, 4)) return;

                                DateTime unbanTime = (time >= 3650) ? DateTime.MaxValue : DateTime.Now.AddDays(time);

                                if (isHard)
                                {
                                    if (time >= 3650) Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} hat dem Offline-Spieler {name} einen lebenslangen Bann verpasst. Grund: {reason}", target);
                                    else Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} hat dem Offline-Spieler {name} für {time} Tage gebannt. Grund: {reason}", target);
                                }
                                else
                                {
                                    if (time >= 3650) Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} hat dem Offline-Spieler {name} einen lebenslangen Bann verpasst. Grund:  {reason}");
                                    else Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} hat dem Offline-Spieler {name} einen Ban für {time} Tage verpasst. Grund: {reason}");
                                }



                                Ban.OfflineBanToNickName(name, unbanTime, isHard, reason, player.Name);

                                string login1 = Main.GetLoginFromUUID(targetCharacter.Uuid);

                                if (login1 != null) 
                                    GameLog.Ban(characterData.UUID, targetCharacter.Uuid, login1, unbanTime, reason, isHard);
                                else 
                                    GameLog.Ban(characterData.UUID, targetCharacter.Uuid, "-", unbanTime, reason, isHard);

                            }
                            catch (Exception e)
                            {
                                Log.Write($"offBanPlayer NAPI.Task.Run Exception: {e.ToString()}");

                            }

                        });
                    }
                    catch (Exception e)
                    {
                        Log.Write($"offBanPlayer NAPI.Task.Run 1 Exception: {e.ToString()}");

                    }
                    



                });               
            }
            catch (Exception e)
            {
                Log.Write($"offBanPlayer Exception: {e.ToString()}");
            }
        }
        public static void unbanPlayer(ExtPlayer player, string name)
        {
            try
            {
                if (!Main.PlayerNames.Values.Contains(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Dieser Name existiert nicht", 3000);
                    return;
                }
                Trigger.SetTask(async () => {
                    try
                    {
                        var isBan = await Ban.Pardon(name);
                        if (!isBan)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} ist nicht gebannt", 3000);
                            return;
                        }

                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                Trigger.SendToAdmins(1, $"~r~[A] {player.Name} ({player.Value}) entfernt den Bann von {name}");

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Spieler wurde entsperrt", 3000);

                                GameLog.Admin($"{player.Name}", $"unban", $"{name}");
                            }
                            catch (Exception e)
                            {
                                Log.Write($"unbanPlayer NAPI.Task.Run Exception: {e.ToString()}");
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Log.Write($"unbanPlayer Task.Run Exception: {e.ToString()}");
                    }

                });
            }
            catch (Exception e)
            {
                Log.Write($"unbanPlayer Exception: {e.ToString()}");
            }
        }
        public static void unhardbanPlayer(ExtPlayer player, string name)
        {
            try
            {
                if (!Main.PlayerNames.Values.Contains(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Dieser Name existiert nicht", 3000);
                    return;
                }

                Trigger.SetTask(async () =>
                {
                    try
                    {
                        var isBan = await Ban.PardonHard(name);
                        if (!isBan)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} ist nicht gebannt", 3000);
                            return;
                        }

                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                Trigger.SendToAdmins(1, $"~r~[A] {player.Name} ({player.Value}) entfernt Hardbann von {name}");

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Bann wurde vom Spieler entfernt", 3000);
                            }
                            catch (Exception e)
                            {
                                Log.Write($"unhardbanPlayer NAPI.Task.Run Exception: {e.ToString()}");
                            }
                        });

                    }
                    catch (Exception e)
                    {
                        Log.Write($"unhardbanPlayer Task.Run Exception: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Write($"unhardbanPlayer Exception: {e.ToString()}");
            }
        }
        public static async void unbanIp(ExtPlayer player, string ip)
        {
            try
            {
                await using var db = new ServerBD("MainDB");//В отдельном потоке

                var ban = await db.Banned
                    .Where(v => v.Ip == ip)
                    .FirstOrDefaultAsync();
                
                if (ban != null)
                {
                    await db.Banned
                        .Where(b => b.Ip == ip)
                        .Set(b => b.Ip, "-")
                        .UpdateAsync();

                    NAPI.Task.Run(() =>
                    {
                        Trigger.SendToAdmins(1, $"~r~[A] {player.Name} ({player.Value}) entsperrt die IP Adresse: {ip}");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "IP Adresse wieder freigeschaltet", 3000);
                    });
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Diese IP Adresse ist nicht gesperrt", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"unbanIp Exception: {e.ToString()}");
            }
        }
        public static void kickPlayer(ExtPlayer player, ExtPlayer target, string reason, bool isSilence)
        {
            try
            {

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (target == player) return;
                if (target.IsCharacterData())
                {
                    if (targetCharacterData.AdminLVL >= characterData.AdminLVL)
                    {
                        Trigger.SendToAdmins(3, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) hat versucht {target.Name} ({target.Value}) zu kicken, der einen höheren Administratoren-Level hat");
                        return;
                    }
                }

                if (isSilence == true && Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                {
                    Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System entfernt. Grund: {reason}");
                    Character.BindConfig.Repository.DeleteAdmin(player);
                    return;
                }

                if (!CheckMe(player, 1)) return;
                if (!isSilence)
                {
                    Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} hat {target.Name}({target.Value}) gekickt. Grund: {reason}", target);
                    GameLog.Admin($"{player.Name}", $"kickPlayer({reason})", $"{target.Name}");
                }
                else
                {
                    Trigger.SendToAdmins(1, "!{#FFB833}" + $"[A] {player.Name} hat {target.Name}({target.Value}) gekickt");

                    GameLog.Admin($"{player.Name}", $"skickPlayer", $"{target.Name}");
                }
                NAPI.Player.KickPlayer(target, reason);
            }
            catch (Exception e)
            {
                Log.Write($"kickPlayer Exception: {e.ToString()}");
            }
        }
        public static void warnPlayer(ExtPlayer player, ExtPlayer target, string reason)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Warn)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (player == target) return;
                if (targetCharacterData.AdminLVL >= characterData.AdminLVL)
                {
                    Trigger.SendToAdmins(3, "!{#FF0000}" + $"[A] {player.Name} ({player.Value}) hat versucht {target.Name} ({target.Value}) zu warnen, der einen höheren Administrator-Status hat");
                    return;
                }
                if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                {
                    Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen folgender Verwarnung entfernt: {reason}");
                    Character.BindConfig.Repository.DeleteAdmin(player);
                    return;
                }
                if (!CheckMe(player, 3)) return;
                targetCharacterData.WarnInfo.Admin[targetCharacterData.Warns] = player.Name;
                targetCharacterData.WarnInfo.Reason[targetCharacterData.Warns] = reason;

                targetCharacterData.Warns++;
                targetCharacterData.Unwarn = DateTime.Now.AddDays(14);

                if (target.GetFractionId() > 0)
                    Fractions.Table.Logs.Repository.AddLogs(target, FractionLogsType.UnInvite, "bekommt eine Verwarnung");
                
                target.RemoveFractionMemberData();
                target.ClearAccessories();
                Customization.ApplyCharacter(target);

                Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} hat {target.Name}({target.Value}) verwarnt. | {targetCharacterData.Warns}/3. Grund: {reason}", target);

                if (targetCharacterData.Warns >= 3)
                {
                    DateTime unbanTime = DateTime.Now.AddMinutes(43200);
                    targetCharacterData.Warns = 0;
                    targetCharacterData.WarnInfo = new WarnInfo();
                    Ban.Online(target, unbanTime, false, "Warns 3/3", "Server");
                }
                GameLog.Admin($"{player.Name}", $"warnPlayer({reason})", $"{target.Name}");
                Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.RedAge,$"{player.Name} gibt dir eine Verwarnung | {targetCharacterData.Warns}/3. Grund: {reason}", DateTime.Now); 
                target.Kick("Verwarnungen");
            }
            catch (Exception e)
            {
                Log.Write($"warnPlayer Exception: {e.ToString()}");
            }
        }

        public static void killTarget(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Kill)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;
                if (!target.IsCharacterData()) return;
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) tötet {target.Name} ({target.Value})", 1, "#636363", hideAdminLevel: 9);
                if (!targetSessionData.DeathData.IsDying) NAPI.Player.SetPlayerHealth(target, 0);
                else Ems.ReviveFunc(target);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie töten {target.Name}({target.Value})", 3000);
                GameLog.Admin($"{player.Name}", $"killPlayer", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"killTarget Exception: {e.ToString()}");
            }
        }
        public static void ReviveMe(ExtPlayer player)
        {
            try
            {

                var sessionData = player.GetSessionData();

                if (sessionData == null) return;
                if (!sessionData.DeathData.IsDying) return;
                sessionData.DeathData.IsReviving = false;
                Ems.ReviveFunc(player, true);
            }
            catch (Exception e)
            {
                Log.Write($"ReviveMe Exception: {e.ToString()}");
            }
        }

        public static void reviveTarget(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Revive)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;
                if (!targetSessionData.DeathData.IsDying)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler ist nicht Tot", 3000);
                    return;
                }
                int id = target.Value;
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat {target.Name} ({target.Value}) wiederbelebt", 2, "#636363");
                GameLog.Admin($"{player.Name}", $"revivePlayer", $"{target.Name}");
                EventSys.SendCoolMsg(target,"Administrator", "Reanimation", $"Administrator {player.Name} hat Sie wiederbelebt", "", 7000); 
                //Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Administrator {player.Name} реанимировал Вас", 3000);
                ReviveMe(target);
                NAPI.Player.SetPlayerHealth(target, 100);
            }
            catch (Exception e)
            {
                Log.Write($"reviveTarget Exception: {e.ToString()}");
            }
        }
        public static void healTarget(ExtPlayer player, ExtPlayer target, int hp)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Sethp)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;
                if (!target.IsCharacterData() || targetSessionData.DeathData.IsDying || hp < 0 || hp > 100) return;
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat HP ({hp}) von {target.Name} ({target.Value}) gesetzt", 1, "#636363", hideAdminLevel: 9);
                NAPI.Player.SetPlayerHealth(target, hp);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Administrator {player.Name} hat Ihre HP auf {hp} geändert", 3000);
                GameLog.Admin($"{player.Name}", $"healPlayer({hp})", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"healTarget Exception: {e.ToString()}");
            }
        }
        public static void armorTarget(ExtPlayer player, ExtPlayer target, int ar)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Setar)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (ar <= 0 || ar > 100) return;
                if (Chars.Repository.itemCount(target, "inventory", ItemId.BodyArmor) < Chars.Repository.maxItemCount)
                {
                    Chars.Repository.AddNewItem(target, $"char_{targetCharacterData.UUID}", "inventory", ItemId.BodyArmor, 1, ar.ToString());
                    Admin.AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) gibt Rüstung ({ar}) {target.Name} ({target.Value})");
                    GameLog.Admin($"{player.Name}", $"armorPlayer({ar})", $"{target.Name}");
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Der Spieler hat bereits eine Schutzweste im Inventar", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"armorTarget Exception: {e.ToString()}");
            }
        }
        public static void checkGodmode(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Gm)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;
                if (!target.IsCharacterData() || targetSessionData.DeathData.IsDying) return;
                int targetHealth = target.Health;
                AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) kontrolliert {target.Name} ({target.Value}) auf GodMode");
                target.Eval($"global.localplayer.applyDamageTo({targetHealth - 1}, true);");
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (!target.IsCharacterData()) return;
                        if (target.Health >= targetHealth) Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"{target.Name} ({target.Value}) hat möglicherweise GodMode an", 3000);
                        else
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"{target.Name} ({target.Value}) hat kein GodMode", 3000);
                            target.Health = targetHealth;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write($"checkGodmode Task Exception: {e.ToString()}");
                    }
                }, 500);
                GameLog.Admin($"{player.Name}", $"checkGm", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"checkGodmode Exception: {e.ToString()}");
            }
        }

        public static void slapPlayer(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Slap)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;
                if (!target.IsCharacterData() || targetSessionData.DeathData.IsDying) return;
                NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 5));
                Admin.AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) schmeißt {target.Name} ({target.Value}) hoch");
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Administrator {player.Name} schmeißt dich hoch", 3000);
                GameLog.Admin($"{player.Name}", $"Slap", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"slapPlayer Exception: {e.ToString()}");
            }
        }

        public static void checkMoney(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Checkmoney)) return;

                var characterData = player.GetCharacterData();
                if (characterData == null) 
                    return;

                var targetCharacterData = target.GetCharacterData();
                if (targetCharacterData == null) 
                    return;
                
                MoneySystem.Bank.Data bankAcc = MoneySystem.Bank.Get(Main.PlayerBankAccs[target.Name]);
                
                int bankMoney = 0;
                if (bankAcc != null) 
                    bankMoney = (int)bankAcc.Balance;
                
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"{target.Name} {MoneySystem.Wallet.Format(targetCharacterData.Money)}$ | Bank: {MoneySystem.Wallet.Format(bankMoney)}", 3000);
                Admin.AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) kontrolliert das Geld von {target.Name} ({target.Value})");
                GameLog.Admin($"{player.Name}", $"checkMoney", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"checkMoney Exception: {e.ToString()}");
            }
        }
        
        public static void teleportTargetToPlayer(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Metp)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) 

                    return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) 
                    return;

                if (targetCharacterData.AdminLVL >= 6)
                {

                    var targetAdminConfig = targetCharacterData.ConfigData.AdminOption;
                    if (targetAdminConfig.HideMe && characterData.AdminLVL < targetCharacterData.AdminLVL)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ein Spieler mit dieser ID wurde nicht gefunden", 3000);
                        Notify.Send(target, NotifyType.Alert, NotifyPosition.BottomCenter, $"{player.Name} ({player.Value}) hat versucht Sie zu teleportieren", 3000);
                        return;
                    }
                }
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat {target.Name} ({target.Value}) zu sich teleportiert", 1, "#636363");
                GameLog.Admin($"{player.Name}", $"metp({player.Position.X}, {player.Position.Y}, {player.Position.Z})", $"{target.Name}");
                NAPI.Entity.SetEntityPosition(target, player.Position);
                Trigger.Dimension(target, UpdateData.GetPlayerDimension(player));
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Du hast {target.Name} zu dir teleportiert", 3000);
                EventSys.SendCoolMsg(target,"Teleport", "Teleport", $"{player.Name} hat Sie zu sich teleportiert", "", 7000); 
                //Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} телепортировал Вас к себе", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"teleportTargetToPlayer Exception: {e.ToString()}");
            }
        }
        
        public static void freezeTarget(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Fz)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!target.IsCharacterData()) return;
                Admin.AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) friert {target.Name} ({target.Value}) ein");
                Trigger.ClientEvent(target, "freeze", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie frieren {target.Name} ein", 3000);
                GameLog.Admin($"{player.Name}", $"freeze", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"freezeTarget Exception: {e.ToString()}");
            }
        }
        public static void unFreezeTarget(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.UnFz)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!target.IsCharacterData()) return;
                Admin.AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) unfrezze {target.Name} ({target.Value})");
                Trigger.ClientEvent(target, "freeze", false);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie unfrezze {target.Name}", 3000);
                GameLog.Admin($"{player.Name}", $"unfreeze", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"unFreezeTarget Exception: {e.ToString()}");
            }
        }
        
        public static void giveTargetGun(ExtPlayer player, ExtPlayer target, string weapon, string serial)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Givegun)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!target.IsCharacterData()) return;
                if (serial.Length != 9)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Die Seriennummer muss aus 9 Ziffern bestehen", 3000);
                    return;
                }
                Regex rg = new Regex(@"^[a-z0-9]+$", RegexOptions.IgnoreCase);
                if (!rg.IsMatch(serial))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Seriennummer ist fehlerhaft", 3000);
                    return;
                }
                ItemId wType = (ItemId)Enum.Parse(typeof(ItemId), weapon);
                if (wType == ItemId.CarCoupon) return;
                if ((Chars.Repository.StrongWeapons.Contains(wType) || Chars.Repository.HeavyWeapons.Contains(wType)) && characterData.AdminLVL <= 5) return;
                if (characterData.AdminLVL >= 8 || (characterData.AdminLVL <= 7 && (Chars.Repository.ItemsInfo[wType].functionType == newItemType.Weapons || Chars.Repository.ItemsInfo[wType].functionType == newItemType.MeleeWeapons)))
                {
                    serial = serial.Replace('\\', '\0');
                    serial = serial.Replace('\'', '\0');
                    serial = serial.Replace('/', '\0');
                    if ( WeaponRepository.GiveWeapon(target, wType, serial) == -1) return;
                    AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) vergibt ({weapon}|{serial}) {target.Name} ({target.Value})");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie geben {target.Name} eine Waffe ({weapon})", 3000);
                    GameLog.Admin($"{player.Name}", $"giveGun({weapon},{serial})", $"{target.Name}");
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Nur mit Admin lvl 8 verfügbar", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"giveTargetGun Exception: {e.ToString()}");
            }
        }

        public static void giveTargetCar(ExtPlayer player, ExtPlayer target, string vehicle)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Carcoupon)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (Chars.Repository.isFreeSlots(target, ItemId.CarCoupon) != 0) return;
                Chars.Repository.AddNewItem(target, $"char_{targetCharacterData.UUID}", "inventory", ItemId.CarCoupon, 1, vehicle);
                AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) vergibt ein Fahrzeug Coupon ({vehicle}) {target.Name} ({target.Value})");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie geben {target.Name} einen Fahrzeug Coupon ({vehicle})", 3000);
                GameLog.Admin($"{player.Name}", $"giveCarC({vehicle})", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"giveTargetCar Exception: {e.ToString()}");
            }
        }

        public static void giveTargetSkin(ExtPlayer player, ExtPlayer target, string pedModel)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Setskin)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (pedModel.Equals("-1"))
                {
                    if (targetSessionData.AdminSkin)
                    {
                        targetSessionData.AdminSkin = false;
                        target.SetDefaultSkin();
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Sie haben das Aussehen des Spielers wiederhergestellt", 3000);
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Der Spieler hat sein Aussehen nicht verändert", 3000);
                        return;
                    }
                }
                else
                {
                    PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                    if (pedHash != 0)
                    {
                        targetSessionData.AdminSkin = true;
                        target.SetSkin(pedHash);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Du hast das Aussehen von {target.Name} auf das Ped-Modell ({pedModel}) geändert", 3000);
                    }
                    else
                    {
                        if (characterData.AdminLVL <= 7) Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Es wurde kein Ped-Modell mit diesem Namen gefunden", 3000);
                        else
                        {
                            targetSessionData.AdminSkin = true;
                            target.SetSkin(NAPI.Util.GetHashKey(pedModel));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Du hast das Aussehen von {target.Name} auf das Ped-Modell ({pedModel}) geändert", 3000);
                        }
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"giveTargetSkin Exception: {e.ToString()}");
            }
        }
        public static void giveTargetClothes(ExtPlayer player, ExtPlayer target, string weapon, string serial)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Giveclothes)) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (serial.Length < 6 || serial.Length > 12)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Die Seriennummer besteht aus 6-12 Zeichen", 3000);
                    return;
                }
                ItemId wType = (ItemId)Enum.Parse(typeof(ItemId), weapon);
                if (Chars.Repository.ItemsInfo[wType].functionType != newItemType.Clothes)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Mit diesem Befehl können nur Kleidungsstücke ausgegeben werden", 3000);
                    return;
                }
                try
                {
                    serial = serial.Replace('\\', '\0');
                    serial = serial.Replace('\'', '\0');
                    serial = serial.Replace('/', '\0');
                }
                catch { Log.Write("giveTargetClothes ERROR"); }
                if (Chars.Repository.AddNewItem(target, $"char_{targetCharacterData.UUID}", "inventory", wType, 1, serial) == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Spieler hat nicht genug Platz im Inventar", 3000);
                    return;
                }
                GameLog.Admin($"{player.Name}", $"giveClothes({weapon},{serial})", $"{target.Name}");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie geben {target.Name} Kleidung ({weapon.ToString()})", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"giveTargetClothes Exception: {e.ToString()}");
            }
        }
        public static void takeTargetGun(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Delgun)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!target.IsCharacterData()) return;
                Chars.Repository.RemoveAllWeapons(target, true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie entnehmen von {target.Name} alle seine Waffen", 3000);
                Admin.AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat alle Waffen von {target.Name} ({target.Value}) entwendet");
                GameLog.Admin($"{player.Name}", $"takeGuns", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"takeTargetGun Exception: {e.ToString()}");
            }
        }
        #region HCmds
        public static void CMD_Cnum(ExtPlayer player, ExtPlayer target, string a)
        {
            try
            {

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!target.IsCharacterData()) return;
                if (characterData.AdminLVL <= 8 || !NewCasino.Roullete.Roulette.ContainsValue(a) || !NewCasino.Roullete.PlayerData.ContainsKey(target)) return;
                if (NewCasino.Roullete.RouletteTables.Count <= NewCasino.Roullete.PlayerData[target].SelectedTable) return;
                NewCasino.Table table = NewCasino.Roullete.RouletteTables[NewCasino.Roullete.PlayerData[target].SelectedTable];
                if (table.Process) return;
                KeyValuePair<int, string> pair = NewCasino.Roullete.Roulette.FirstOrDefault(p => p.Value.Equals(a));
                if (pair.Equals(default(KeyValuePair<int, string>))) return;
                table.Win = pair.Key;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"{pair.Value}", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"CMD_Cnum Exception: {e.ToString()}");
            }
        }
        public static void CMD_Chnum(ExtPlayer player, int a)
        {
            try
            {

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (characterData.AdminLVL <= 8 || a < 1 || a > 6 || NewCasino.Horses.curentScreen != 0) return;
                NewCasino.Horses.WinHorse = a;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Pferd #{a}", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"CMD_Chnum Exception: {e.ToString()}");
            }
        }
        public static void CMD_Cinum(ExtPlayer player, int a, byte b, byte c)
        {
            try
            {

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (characterData.AdminLVL <= 8) return;
                ExtPlayer target = Main.GetPlayerByID(a);

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;
                if (!target.IsCharacterData()) return;
                if (b == 255 || c == 255)
                {
                    targetSessionData.CaseWin = 255;
                    targetSessionData.CaseItemWin = 255;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "RESET", 1000);
                    return;
                }
                if (b >= Chars.Repository.RouletteCasesData.Count || Chars.Repository.RouletteCasesData[b].RouletteItemsData.Count <= c) return;
                RouletteItemData p = Chars.Repository.RouletteCasesData[b].RouletteItemsData[c];
                if (p == null) return;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"{Chars.Repository.RouletteCasesData[b].Name} - {p.Name}", 1000);
                targetSessionData.CaseWin = b;
                targetSessionData.CaseItemWin = c;
            }
            catch (Exception e)
            {
                Log.Write($"CMD_Cinum Exception: {e.ToString()}");
            }
        }
        #endregion
        public static void adminSMS(ExtPlayer player, ExtPlayer target, string message)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Asms)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!target.IsCharacterData() || player == target) return;
                Trigger.SendChatMessage(target, $"{ChatColors.Report}Administrator {player.Name} ({player.Value}): {message}");
                Trigger.SendToAdmins(characterData.AdminLVL, $"{ChatColors.Report}[A][ASMS] {player.Name} ({player.Value}) für {target.Name} ({target.Value}): {message}");
                //Notify.Send(target, NotifyType.Info, NotifyPosition.TopCenter, $"Administrator {player.Name}: {message}", 8000);
                EventSys.SendCoolMsg(target,"Administrator", $"{player.Name}", $"{message}", "", 8000);
                GameLog.Admin($"{player.Name}", $"aSMS({message})", $"{target.Name}");
                Trigger.ClientEvent(target, "StartDangerButtonSound_client", "sounds/icq.mp3");
            }
            catch (Exception e)
            {
                Log.Write($"adminSMS Exception: {e.ToString()}");
            }
        }
        public static void checkKill(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Checkkill)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;

                var targetSessionData = target.GetSessionData();

                if (targetSessionData == null) return;
                if (targetSessionData.DeathData.LastDeath == null)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"{target.Name} hat keine Logs für letzten Tod", 2500);
                    return;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, targetSessionData.DeathData.LastDeath, 7000);
                Admin.AdminLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat die Todeslogs von {target.Name} ({target.Value}) kontrolliert");
                GameLog.Admin($"{player.Name}", $"checkKill", $"{target.Name}");
            }
            catch (Exception e)
            {
                Log.Write($"checkKill Exception: {e.ToString()}");
            }
        }
        public static void adminChat(ExtPlayer player, string message)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.A)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                GameLog.AddInfo($"(AChat) player({characterData.UUID}) {message}");

                message = (characterData.AdminLVL >= 6) ? $"{ChatColors.AdminChat}[A] {ChatColors.HAdmin}{player.Name}{ChatColors.AdminChat} ({player.Value}): {message}" : $"{ChatColors.AdminChat}[A] {ChatColors.LAdmin}{player.Name}{ChatColors.AdminChat} ({player.Value}): {message}";

                foreach (ExtPlayer foreachPlayer in Main.AllAdminsOnline)
                {

                    var foreachCharacterData = foreachPlayer.GetCharacterData();

                    if (foreachCharacterData == null) continue;
                    if (foreachCharacterData.AdminLVL >= 1) Trigger.SendChatMessage(foreachPlayer, message);
                }
            }
            catch (Exception e)
            {
                Log.Write($"adminChat Exception: {e.ToString()}");
            }
        }
        public static void adminGlobal(ExtPlayer player, string message)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Global)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (characterData.AdminLVL <= 6)
                {
                    if (Main.stringGlobalBlock.Any(c => message.Contains(c)))
                    {
                        Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {player.Name} ({player.Value}) wurde vom System wegen einer globalen Ursache entfernt {message}");
                        Character.BindConfig.Repository.DeleteAdmin(player);
                        return;
                    }
                }
                if (characterData.AdminLVL >= 6)
                {
                    NAPI.Chat.SendChatMessageToAll($"{CommandsAccess.AdminPrefixChat}{player.Name.Replace('_', ' ')}: {message}");
                    GameLog.Admin($"{player.Name}", $"global({message})", "null");
                    EventSys.SendPlayersToEvent("Administrator", $"{player.Name}", $"{message}", "", 10000);
                }
                else
                {
                    GlobalQueue.Add(GlobalID, (message, player.Name, characterData.AdminLVL, DateTime.Now.AddSeconds(60)));
                    Trigger.SendToAdmins(2, "!{#FFB833}" + $"[A] Anfrage von {player.Name} ({player.Value}) auf global ({message}). Um die Aktion zu bestätigen, geben Sie ein: /accept {GlobalID}");
                    GlobalID++;
                }
            }
            catch (Exception e)
            {
                Log.Write($"adminGlobal Exception: {e.ToString()}");
            }
        }

        public static void adminGlobalAccept(ExtPlayer player, int id)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Accept)) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (!GlobalQueue.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Leider existiert keine Bestätigungsanfrage für die globale ID", 3000);
                    return;
                }
                if (GlobalQueue[id].Item4 < DateTime.Now)
                {
                    GlobalQueue.Remove(id);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Zeitlimit für Anfrage abgelaufen (60 Sekunden)", 3000);
                    return;
                }
                if (GlobalQueue[id].Item2 == player.Name)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Bestätigen kann nur ein anderer Administrator", 3000);
                    return;
                }
                AdminsLog(characterData.AdminLVL, $"{player.Name} ({player.Value}) hat die Anfrage zu Global ({GlobalQueue[id].Item2}) bestätigt");
                NAPI.Chat.SendChatMessageToAll($"{CommandsAccess.AdminPrefixChat}{GlobalQueue[id].Item2.Replace('_', ' ')}: {GlobalQueue[id].Item1}");
                GameLog.Admin($"{GlobalQueue[id].Item2}", $"global({GlobalQueue[id].Item1})", "null");
                GameLog.Admin($"{player.Name}", $"globalAccept({GlobalQueue[id].Item2})", "null");
                GlobalQueue.Remove(id);
            }
            catch (Exception e)
            {
                Log.Write($"adminGlobalAccept Exception: {e.ToString()}");
            }
        }
        public static void sendPlayerToDemorgan(ExtPlayer admin, ExtPlayer target, int time, string reason)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(admin, AdminCommands.Jail)) return;
                var targetSessionData = target.GetSessionData();
                if (targetSessionData == null) return;
                if (!target.IsCharacterData()) return;
                if (admin == target) return;

                if (time < 5 || time > 10080)
                {
                    Notify.Send(admin, NotifyType.Error, NotifyPosition.BottomCenter, $"Sie können die Gefängnisstrafe nicht länger als 10080 minuten machen", 3000);
                    return;
                }
                if (Main.stringGlobalBlock.Any(c => reason.Contains(c)))
                {
                    Trigger.SendToAdmins(1, $"{ChatColors.Red}[A] {admin.Name} ({admin.Value}) wurde vom System entfernt, Grund: {reason}");
                    Character.BindConfig.Repository.DeleteAdmin(admin);
                    return;
                }
                if (!CheckMe(admin, 2)) return;
                int firstTime = time * 60;
                string deTimeMsg = " Minuten";
                if (time > 60)
                {
                    deTimeMsg = " Stunde";
                    time /= 60;
                    if (time > 24)
                    {
                        deTimeMsg = " Tage";
                        time /= 24;
                    }
                }
                var targetCharacterData = target.GetCharacterData();

                Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{admin.Name} hat {target.Name}({target.Value}) für {time} {deTimeMsg}. ins Gefängnis gesetzt. Grund: {reason}", target);

                targetCharacterData.DemorganInfo.Admin = admin.Name;
                targetCharacterData.DemorganInfo.Reason = reason;
                targetCharacterData.ArrestTime = 0;
                targetCharacterData.ArrestType = 0;
                targetCharacterData.DemorganTime = firstTime;
                //
                VehicleManager.WarpPlayerOutOfVehicle(target, false);
                //
                FractionCommands.unCuffPlayer(target);
                //
                targetSessionData.CuffedData.CuffedByCop = false;
                targetSessionData.CuffedData.CuffedByMafia = false;
                //
                target.Position = DemorganPositions[Main.rnd.Next(55)] + new Vector3(0, 0, 1.5);
                target.SetSkin(DemorganSkins[Main.rnd.Next(14)]);
                //
                if (targetSessionData.TimersData.ArrestTimer != null) Timers.Stop(targetSessionData.TimersData.ArrestTimer);
                targetSessionData.TimersData.ArrestTimer = Timers.Start(1000, () => timer_demorgan(target));
                targetCharacterData.VoiceMuted = true;
                target.SetSharedData("vmuted", true);
                //
                Trigger.ClientEvent(target, "client.demorgan", true);
                target.Eval($"mp.game.audio.playSoundFrontend(-1, \"Deliver_Pick_Up\", \"HUD_FRONTEND_MP_COLLECTABLE_SOUNDS\", true);");
                target.SetSharedData("HideNick", true);
                //
                NAPI.Player.SetPlayerHealth(target, 3);
                Chars.Repository.RemoveAllWeapons(target, true, true, armour: true);
                GameLog.Admin(admin.Name, $"demorgan({time}{deTimeMsg},{reason})", target.Name);
                //Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.RedAge,$"{admin.Name} отправил Вас в деморган на {time} {deTimeMsg}. Grund: {reason}", DateTime.Now); 
                EventSys.SendCoolMsg(target,"Administrator", $"{admin.Name}", $"Sie wurden für {time} {deTimeMsg} ins Gefängnis geschickt. Grund: {reason}", "", 15000); 
                //
            }
            catch (Exception e)
            {
                Log.Write($"sendPlayerToDemorgan Exception: {e.ToString()}");
            }
        }
        public static void releasePlayerFromDemorgan(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Unjail)) return;

                var targetCharacterData = target.GetCharacterData();

                if (targetCharacterData == null) return;
                if (targetCharacterData.DemorganTime <= 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Spieler befindet sich nicht im Spezialgefängnis", 3000);
                    return;
                }
                targetCharacterData.DemorganTime = 1;
                Trigger.SendPunishment($"{CommandsAccess.AdminPrefix}{player.Name} hat {target.Name} ({target.Value}) aus dem Demorgan freigelassen");
                GameLog.Admin($"{player.Name}", $"undemorgan", $"{target.Name}");
                //Players.Phone.Messages.Repository.AddSystemMessage(target, (int)DefaultNumber.RedAge,$"{player.Name} выпустил Вас из деморгана.", DateTime.Now);
                EventSys.SendCoolMsg(target,"Administrator", $"{player.Name}", $"Sie wurden aus Demorgan freigelassen. Viel Spaß beim Spielen und verstoßen Sie nicht mehr gegen die Regeln! :)", "", 15000); 
            }
            catch (Exception e)
            {
                Log.Write($"releasePlayerFromDemorgan Exception: {e.ToString()}");
            }
        }

        public static PedHash[] DemorganSkins = new PedHash[14]
        {
            //Наземные животные
            PedHash.Cat,
            PedHash.Chop,
            PedHash.Husky,
            PedHash.Poodle,
            PedHash.Pug,
            PedHash.Rabbit,
            PedHash.Retriever,
            PedHash.Rottweiler,
            PedHash.Shepherd,
            PedHash.Westy,
            //Птицы
            PedHash.Seagull,
            PedHash.ChickenHawk,
            PedHash.Crow,
            PedHash.Pigeon
        };

        public static Vector3[] DemorganPositions = new Vector3[55]
        {
            new Vector3(-323.5328, 1370.41, 346.4068),
            new Vector3(791.201, 2311.601, 47.13044),
            new Vector3(383.2664, 773.5862, 183.8618),
            new Vector3(1301.362, 1447.964, 98.23167),
            new Vector3(-78.97045, 6597.737, 28.42492),
            new Vector3(1266.669, 1911.738, 77.65078),
            new Vector3(-1852.087, 1931.88, 149.0496),
            new Vector3(125.2143, 6593.171, 30.85273),
            new Vector3(-2599.767, 1689.187, 139.9411),
            new Vector3(1404.253, 2169.581, 96.66694),
            new Vector3(-2291.825, 2485.236, 1.914838),
            new Vector3(-44.12902, 6299.675, 30.50067),
            new Vector3(-98.26251, 2801.208, 52.12306),
            new Vector3(-248.8021, 6200.921, 30.36921),
            new Vector3(-396.2312, 6053.582, 30.38009),
            new Vector3(1212.61, 2651.768, 36.69186),
            new Vector3(143.9709, 1669.224, 227.5427),
            new Vector3(-514.4248, 6275.275, 8.819883),
            new Vector3(1471.917, 1318.306, 113.9579),
            new Vector3(1469.517, 2718.285, 36.45533),
            new Vector3(1316.378, 653.1463, 81.45456),
            new Vector3(2497.466, 1582.359, 31.60028),
            new Vector3(-214.658, 6561.28, 9.756557),
            new Vector3(2531.433, 2039.337, 18.69318),
            new Vector3(2062.655, 3481.989, 42.38651),
            new Vector3(2545.573, 2637.896, 36.82483),
            new Vector3(112.0649, 6842.445, 15.012),
            new Vector3(2682.535, 2803.837, 39.21159),
            new Vector3(2155.056, 3018.603, 44.15971),
            new Vector3(291.8854, 6793.59, 14.57676),
            new Vector3(1876.9, 3967.318, 35.13688),
            new Vector3(2446.471, 3768.721, 39.95733),
            new Vector3(778.8936, 6439.555, 30.84892),
            new Vector3(-1543.886, 1366.205, 124.9902),
            new Vector3(410.5818, 6457.222, 27.68898),
            new Vector3(-788.4865, 1672.407, 198.4508),
            new Vector3(2538.595, 4477.067, 36.10785),
            new Vector3(-262.9102, 2608.396, 62.22501),
            new Vector3(221.4821, 2961.687, 41.5982),
            new Vector3(485.4037, 3501.335, 33.06005),
            new Vector3(1557.167, 3753.676, 33.27285),
            new Vector3(2852.622, 3714.04, 47.53786),
            new Vector3(2732.895, 4414.241, 46.5253),
            new Vector3(2883.424, 347.9111, 1.930261),
            new Vector3(2943.639, 4655.853, 47.42477),
            new Vector3(2762.99, 1264.434, 2.839344),
            new Vector3(2083.99, 3838.34, 30.67446),
            new Vector3(2374.62, 2427.864, 61.85823),
            new Vector3(2872.061, 4873.417, 61.47282),
            new Vector3(1544.658, 3911.407, 30.57072),
            new Vector3(1801.522, 4524.824, 30.87487),
            new Vector3(-133.3322, 6336.445, 30.37035),
            new Vector3(-179.7547, 4295.339, 31.85094),
            new Vector3(-261.8268, 6303.704, 31.166),
            new Vector3(-345.2491, 6150.061, 30.363)
        };
        public static void timer_demorgan(ExtPlayer player)
        {
            try
            {

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (characterData.DemorganTime-- <= 0) Fractions.FractionCommands.freePlayer(player, true);
            }
            catch (Exception e)
            {
                Log.Write($"timer_demorgan Exception: {e.ToString()}");
            }
        }
        public static void timer_mute(ExtPlayer player)
        {
            try
            {

                var sessionData = player.GetSessionData();

                if (sessionData == null) return;

                var characterData = player.GetCharacterData();

                if (characterData == null) return;
                if (characterData.Unmute-- <= 0)
                {
                    if (sessionData.TimersData.MuteTimer != null)
                    {
                        Timers.Stop(sessionData.TimersData.MuteTimer);
                        sessionData.TimersData.MuteTimer = null;
                        characterData.Unmute = 0;
                        if (characterData.DemorganTime <= 0)
                        {
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    if (characterData == null || player == null) return;
                                    characterData.VoiceMuted = false;
                                    player.SetSharedData("vmuted", false);
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Die Stummschaltung wurde aufgehoben, verstoßen Sie nicht mehr dagegen", 3000);
                                }
                                catch (Exception e)
                                {
                                    Log.Write($"timer_mute Task Exception: {e.ToString()}");
                                }
                            });
                        }
                        else Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Der Stummschaltung wurde aufgehoben, aber Sie können den Sprachchat erst nach Ablauf einer anderen Strafe nutzen", 10000);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"timer_mute Exception: {e.ToString()}");
            }
        }
        /*
        public static void respawnAllCars(Player player)
        {
            try
            {
                if (!CommandsAccess.CanUseCmd(player, AdminCommands.Spvehall)) return;
                List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();

                foreach (Vehicle vehicle in all_vehicles)
                {
                    if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                    if (vehicleLocalData != null)
                    {
                        VehicleStreaming.VehiclesData data = vehicle.GetVehicleLocalData();

                        switch (vehicleLocalData.Access)
                        {
                            case "FRACTION":
                                RespawnFractionCar(vehicle);
                                break;
                            case "GANGDELIVERY":
                            case "MAFIADELIVERY":
                            case "BIKERDELIVERY":
                                VehicleStreaming.DeleteVehicle(vehicle);
                                break;
                            default:
                                continue;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"respawnAllCars Exception: {e.ToString()}");
            }
        }
        */

        [RemoteEvent("saveEspState")]
        public void ClientEvent_SaveEspState(ExtPlayer player, byte state)
        {
            try
            {

                var characterData = player.GetCharacterData();

                if (characterData == null)

                    return;



                var adminConfig = characterData.ConfigData.AdminOption;
                adminConfig.ESP = state;
            }
            catch (Exception e)
            {
                Log.Write($"ClientEvent_SaveEspState Exception: {e.ToString()}");
            }
        }
        public static void RespawnFractionCar(ExtVehicle vehicle)
        {
            try
            {
                var vehicleLocalData = vehicle.GetVehicleLocalData();
                if (vehicleLocalData != null)
                {
                    if (vehicleLocalData.LoaderMats != null)
                    {
                        ExtPlayer loader = vehicleLocalData.LoaderMats;
                        var loaderSessionData = loader.GetSessionData();

                        if (loaderSessionData != null)
                        {
                            Notify.Send(loader, NotifyType.Warning, NotifyPosition.BottomCenter, $"Die Mats wurden nicht geladen weil das Fahrzeug den Checkpoint verlassen hat", 3000);

                            if (loaderSessionData.TimersData.LoadMatsTimer != null)
                            {
                                Timers.Stop(loaderSessionData.TimersData.LoadMatsTimer);
                                loaderSessionData.TimersData.LoadMatsTimer = null;
                            }
                        }
                        vehicleLocalData.LoaderMats = null;
                    }
                    Fractions.Configs.RespawnFractionCar(vehicle);
                }
            }
            catch (Exception e)
            {
                Log.Write($"RespawnFractionCar Exception: {e.ToString()}");
            }
        }
    }

    public class Group
    {
        public static string[] GroupNames = new string[6]
        {
            "Spieler",
            "Silver VIP",
            "Gold VIP",
            "Platinum VIP",
            "Diamond VIP",
            "Media VIP",
        };
        public static float[] GroupPayAdd = new float[6]
        {
            1.0f,
            1.0f,
            1.15f,
            1.25f,
            1.35f,
            1.35f,
        };
        public static int[] GroupAddPayment = new int[6]
        {
            0,
            50,
            70,
            110,
            200,
            200,
        };

        public static int[] GroupMaxBusinesses = new int[6]
        {
            1,
            1,
            1,
            1,
            1,
            1,
        };
        public static int[] GroupEXP = new int[6]
        {
            1,
            1,
            2,
            2,
            3,
            3,
        };
    }
}
