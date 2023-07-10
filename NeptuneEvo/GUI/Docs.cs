﻿using GTANetworkAPI;
using NeptuneEvo.Handles;
using NeptuneEvo.Accounts;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Players;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Chars;
using NeptuneEvo.Core;
using Redage.SDK;
using System;
using System.Collections.Generic;
using Localization;
using NeptuneEvo.Fractions.Player;
using NeptuneEvo.Table.Tasks.Models;
using NeptuneEvo.Table.Tasks.Player;

namespace NeptuneEvo.GUI
{
    enum DocsEnum
    {
        passport = 0,
        licenses,
        idcard,
        certificate
    }

    class DocsClass
    {
        public string Request;
        public string SendFrom;
        public string Name;

        public DocsClass(string request, string sendFrom, string name)
        {
            Request = request;
            SendFrom = sendFrom;
            Name = name;
        }
    }

    class Docs : Script
    {
        private static readonly nLog Log = new nLog("GUI.Docs");

        private static List<DocsClass> DocsArray = new List<DocsClass>
        {
            new DocsClass("acceptPass", "mein Ausweis", "Ausweis"),
            new DocsClass("acceptLics", "meine Lizensen", "Lizensen"),
            new DocsClass("acceptIdcard", "meine ID-Karte", "ID-Karte"),
            new DocsClass("acceptCertificate", "meine Bescheinigung", "Bescheinigung")
        };


        public static void DocsFunction(ExtPlayer player, ExtPlayer target, int docsEnum)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                var targetSessionData = target.GetSessionData();
                if (targetSessionData == null) return;
                else if (!target.IsCharacterData()) return;

                Vector3 pos = target.Position;
                if (player.Position.DistanceTo(pos) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerTooFar), 6000);
                    return;
                }
                if (targetSessionData.RequestData.IsRequested)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PersonHavBeenBusy), 7000);
                    return;
                }
                targetSessionData.RequestData.IsRequested = true;
                targetSessionData.RequestData.Request = DocsArray[docsEnum].Request;
                targetSessionData.RequestData.From = player;
                targetSessionData.RequestData.Time = DateTime.Now.AddSeconds(10);
                //Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.YouOfferShowDoc, DocsArray[docsEnum].SendFrom), 5000);
               // Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerOfferShowDoc, player.Value, DocsArray[docsEnum].Name), 6000);
                EventSys.SendCoolMsg(player,"Angebot", "Papiere", $"{LangFunc.GetText(LangType.De, DataName.YouOfferShowDoc, DocsArray[docsEnum].SendFrom)}", "", 7000);
                EventSys.SendCoolMsg(target,"Angebot", "Papiere", $"{LangFunc.GetText(LangType.De, DataName.PlayerOfferShowDoc, player.Value, DocsArray[docsEnum].Name)}", "", 8000);
                string genderWord = characterData.Gender == false ? LangFunc.GetText(LangType.De, DataName.dostal) : LangFunc.GetText(LangType.De, DataName.dostala);
                Commands.RPChat("sme", player, $"{genderWord} {DocsArray[docsEnum].Name}");
            }
            catch (Exception e)
            {
                Log.Write($"DocsFunction Exception: {e.ToString()}");
            }
        }
        [RemoteEvent("idcard")]
        public static void Event_Idcard(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                DocsFunction(player, target, (int)DocsEnum.idcard);
            }
            catch (Exception e)
            {
                Log.Write($"Event_Idcard Exception: {e.ToString()}");
            }
        }
        [RemoteEvent("certificate")]
        public static void Event_Certificate(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                var memberFractionData = player.GetFractionMemberData();
                if (memberFractionData == null)
                    return;

                if (memberFractionData.Id == (int)Fractions.Models.Fractions.None)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.OnlyFracCan), 6000);
                    return;
                }
                if (!Fractions.Manager.GovIds.Contains(memberFractionData.Id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.OnlyGosFracCan), 6000);
                    return;
                }
                DocsFunction(player, target, (int)DocsEnum.certificate);
            }
            catch (Exception e)
            {
                Log.Write($"Event_Certificate Exception: {e.ToString()}");
            }
        }
        [RemoteEvent("passport")]
        public static void Event_Passport(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                DocsFunction(player, target, (int)DocsEnum.passport);
            }
            catch (Exception e)
            {
                Log.Write($"Event_Passport Exception: {e.ToString()}");
            }
        }
        [RemoteEvent("licenses")]
        public static void Event_Licenses(ExtPlayer player, ExtPlayer target)
        {
            try
            {
                DocsFunction(player, target, (int)DocsEnum.licenses);
            }
            catch (Exception e)
            {
                Log.Write($"Event_Licenses Exception: {e.ToString()}");
            }
        }
        public static void AcceptPasport(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                if (!player.IsCharacterData()) return;
                ExtPlayer from = sessionData.RequestData.From;
                sessionData.RequestData = new RequestData();
                var fromCharacterData = from.GetCharacterData();
                if (fromCharacterData == null) return;
                if (player.Position.DistanceTo(from.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerTooFar), 6000);
                    return;
                }
                var fromMemberFractionData = from.GetFractionMemberData();
                string gender = (fromCharacterData.Gender) ? LangFunc.GetText(LangType.De, DataName.Mans) : LangFunc.GetText(LangType.De, DataName.Womens);
                string fraction = (fromMemberFractionData != null) ? Fractions.Manager.FractionNames[fromMemberFractionData.Id] : LangFunc.GetText(LangType.De, DataName.No);
                if (fromMemberFractionData != null && ((fromMemberFractionData.Id >= (int)Fractions.Models.Fractions.FAMILY && fromMemberFractionData.Id <= (int)Fractions.Models.Fractions.BLOOD) || fromMemberFractionData.Id == (int)Fractions.Models.Fractions.FIB || (fromMemberFractionData.Id >= (int)Fractions.Models.Fractions.LCN && fromMemberFractionData.Id <= (int)Fractions.Models.Fractions.ARMENIAN))) 
                    fraction = LangFunc.GetText(LangType.De, DataName.No);
                string work = (fromCharacterData.WorkID > 0) ? Jobs.WorkManager.JobStats[fromCharacterData.WorkID - 1] : LangFunc.GetText(LangType.De, DataName.NoWorker);
                List<object> data = new List<object>
                    {
                        fromCharacterData.UUID,
                        fromCharacterData.FirstName,
                        fromCharacterData.LastName,
                        fromCharacterData.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        fraction,
                        work
                    };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerShowYouPass, from.Value), 5000);
                Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.YouShowPassPlayer, player.Value), 5000);
                if (fromCharacterData.Gender) Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.HeShow, player.Name), player);
                else Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.SheShow, player.Name), player);
                player.AddTableScore(TableTaskId.Item12);
                Trigger.ClientEvent(player, "passport", json);
                BattlePass.Repository.UpdateReward(from, 101);
            }
            catch (Exception e)
            {
                Log.Write($"AcceptPasport Exception: {e.ToString()}");
            }
        }
        public static void AcceptLicenses(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                if (!player.IsCharacterData()) return;
                ExtPlayer from = sessionData.RequestData.From;
                sessionData.RequestData = new RequestData();
                var fromCharacterData = from.GetCharacterData();
                if (fromCharacterData == null) return;
                if (player.Position.DistanceTo(from.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerTooFar), 6000);
                    return;
                }

                string gender = (fromCharacterData.Gender) ? LangFunc.GetText(LangType.De, DataName.Mans) : LangFunc.GetText(LangType.De, DataName.Womens);
                string lic = "";
                for (int i = 0; i < fromCharacterData.Licenses.Count; i++)
                    if (fromCharacterData.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                if (lic == "") lic = LangFunc.GetText(LangType.De, DataName.None);

                List<string> data = new List<string>
                    {
                        fromCharacterData.FirstName,
                        fromCharacterData.LastName,
                        fromCharacterData.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        lic
                    };

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerShowLicyou, from.Value), 5000);
                Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.YouShowLicPlayer, player.Value), 5000);
                if (fromCharacterData.Gender) Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.HeShowLic, player.Name), player);
                else Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.SheShowLic, player.Name), player);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                Trigger.ClientEvent(player, "licenses", json);
            }
            catch (Exception e)
            {
                Log.Write($"AcceptLicenses Exception: {e.ToString()}");
            }
        }
        public static void AcceptIdcard(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                if (!player.IsCharacterData()) return;
                ExtPlayer from = sessionData.RequestData.From;
                sessionData.RequestData = new RequestData();
                var fromCharacterData = from.GetCharacterData();
                if (fromCharacterData == null) return;
                
                if (player.Position.DistanceTo(from.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerTooFar), 6000);
                    return;
                }

                string lic = "";
                for (int i = 0; i < fromCharacterData.Licenses.Count; i++)
                    if (fromCharacterData.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                if (lic == "") lic = LangFunc.GetText(LangType.De, DataName.Nothing);
                
                Dictionary<string, string> RequestData = new Dictionary<string, string>
                {
                    ["name"] = fromCharacterData.FirstName,
                    ["surname"] = fromCharacterData.LastName,
                    ["rank"] = from.GetFractionRankName(),
                    ["cardNO"] = player.Value.ToString(),
                    ["dateReg"] = fromCharacterData.CreateDate.ToString("dd.MM.yyyy"),
                    ["gender"] = (fromCharacterData.Gender) ? LangFunc.GetText(LangType.De, DataName.Mans) : LangFunc.GetText(LangType.De, DataName.Womens),
                    ["lic"] = lic
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(RequestData);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerShowIdToYou, from.Value), 5000);
                Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.YouShowIdToPlayer, player.Value), 5000);
                if (fromCharacterData.Gender) Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.HePokazal, player.Value), player);
                else Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.ShePokazala, player.Value), player);
                Trigger.ClientEvent(player, "client.docs", "passport", json);
            }
            catch (Exception e)
            {
                Log.Write($"AcceptIdcard Exception: {e.ToString()}");
            }
        }
        public static void AcceptCertificate(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                if (!player.IsCharacterData()) return;
                ExtPlayer from = sessionData.RequestData.From;
                sessionData.RequestData = new RequestData();
                var fromCharacterData = from.GetCharacterData();
                if (fromCharacterData == null) 
                    return;
                
                var fromMemberFractionData = from.GetFractionMemberData();
                if (fromMemberFractionData == null)
                    return;
                
                if (player.Position.DistanceTo(from.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerTooFar), 6000);
                    return;
                }
                Dictionary<string, string> RequestData = new Dictionary<string, string>
                {
                    ["name"] = fromCharacterData.FirstName,
                    ["surname"] = fromCharacterData.LastName,
                    ["rank"] = from.GetFractionRankName(),
                    ["cardNO"] = player.Value.ToString(),
                    ["gender"] = (fromCharacterData.Gender) ? "male" : "female",
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(RequestData);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PokazalVamUdostoverenie, from.Value), 5000);
                Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.YouPokazalUdostoverenie, player.Value), 5000);
                if (fromCharacterData.Gender) Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.PokazalUdo, player.Value), player);
                else Commands.RPChat("sme", from, LangFunc.GetText(LangType.De, DataName.PokazalaUdo, player.Value), player);

                string pageID = "";

                switch (fromMemberFractionData.Id)
                {
                    case 6:
                        pageID = "goverment";
                        break;
                    case 18:
                    case 7:
                        pageID = "lspd";
                        break;
                    case 8:
                        pageID = "ems";
                        break;
                    case 9:
                        pageID = "fib";
                        break;
                    case 14:
                        pageID = "army";
                        break;
                    case 17:
                        pageID = "msc";
                        break;
                    case 15:
                        pageID = "lsnews";
                        break;
                    default:
                        // Not supposed to end up here. 
                        break;
                }
                Trigger.ClientEvent(player, "client.docs", pageID, json);
            }
            catch (Exception e)
            {
                Log.Write($"AcceptCertificate Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("viewBadge")]
        public static void Event_viewBadge(ExtPlayer player, ExtPlayer target, string action)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                var targetSessionData = target.GetSessionData();
                if (targetSessionData == null) return;
                var targetCharacterData = target.GetCharacterData();
                if (targetCharacterData == null) return;

                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerTooFar), 6000);
                    return;
                }

                var targetMemberFractionData = target.GetFractionMemberData();

                switch (action)
                {
                    case "Dienstmarke ansehen":
                        if (targetMemberFractionData == null || (targetMemberFractionData.Id != (int)Fractions.Models.Fractions.POLICE && targetMemberFractionData.Id != (int)Fractions.Models.Fractions.SHERIFF) || !targetSessionData.WorkData.OnDuty)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerNoBadge), 6000);
                            return;
                        }

                        if (characterData.Gender) Commands.RPChat("sme", player, LangFunc.GetText(LangType.De, DataName.HeSeeZnak, target.Name), target);
                        else Commands.RPChat("sme", player, LangFunc.GetText(LangType.De, DataName.SheSeeZnak, target.Name), target);

                        Dictionary<string, string> lspdBadgeData = new Dictionary<string, string>
                        {
                            ["gender"] = targetCharacterData.Gender ? "male" : "female",
                            ["name"] = targetCharacterData.FirstName,
                            ["surname"] = targetCharacterData.LastName,
                            ["rank"] = target.GetFractionRankName(),
                            ["cardNO"] = target.Value.ToString()
                        };

                        Trigger.ClientEvent(player, "client.docs", "lspdbadge", Newtonsoft.Json.JsonConvert.SerializeObject(lspdBadgeData));
                        return;
                    case "Namensschild ansehen":
                        if (targetMemberFractionData == null || targetMemberFractionData.Id != (int)Fractions.Models.Fractions.FIB || !targetSessionData.WorkData.OnDuty)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.PlayerNoBadges), 6000);
                            return;
                        }

                        if (characterData.Gender) Commands.RPChat("sme", player, LangFunc.GetText(LangType.De, DataName.HeSeeBadge, target.Name), target);
                        else Commands.RPChat("sme", player, LangFunc.GetText(LangType.De, DataName.SheSeeBadge, target.Name), target);

                        Dictionary<string, string> fibbadgeData = new Dictionary<string, string>
                        {
                            ["gender"] = targetCharacterData.Gender ? "male" : "female",
                            ["name"] = targetCharacterData.LastName
                        };

                        Trigger.ClientEvent(player, "client.docs", "fibbadge", Newtonsoft.Json.JsonConvert.SerializeObject(fibbadgeData));
                        return;
                    default:
                        // Not supposed to end up here. 
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write($"Event_viewBadge Exception: {e.ToString()}");
            }
        }
    }
}
