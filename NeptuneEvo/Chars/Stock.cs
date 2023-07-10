﻿using GTANetworkAPI;
using NeptuneEvo.Handles;
using NeptuneEvo.Functions;
using NeptuneEvo.Players;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Quests;
using Redage.SDK;
using System;
using Localization;
using NeptuneEvo.Quests.Models;

namespace NeptuneEvo.Chars
{
    class Stock : Script
    {
        private static readonly nLog Log = new nLog("Chars.MergerToServer");
        [ServerEvent(Event.ResourceStart)]
        public void Init()
        {
            try
            {
                Main.CreateBlip(new Main.BlipData(676, "Lager 'GoPostal'", new Vector3(77.84161, 136.7562, 80.53349), 32, true));
                Main.CreateBlip(new Main.BlipData(525, "Arbeitsamt", new Vector3(436.5074, -627.4617, 28.707539), 0, true));
                //NAPI.Marker.CreateMarker(1, new Vector3(-545.02136, -204.28348, 38.2) - new Vector3(0, 0, 1.5), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                //CustomColShape.CreateCylinderColShape(new Vector3(132.9969, 96.3529, 83.5076), 1f, 2, 0, ColShapeEnums.WarehouseEnter);
                CustomColShape.CreateCylinderColShape(new Vector3(1048.2255, -3097.1624, -38.9999), 1f, 2, 5, ColShapeEnums.WarehouseExit);
                PedSystem.Repository.CreateQuest("s_m_m_postal_01", new Vector3(82.76877, 132.95116, 80.53349), 71.96516f, 0, title: "~y~NPC~w~ Postbote Alfred\nPakete Lager", colShapeEnums: ColShapeEnums.Warehouse);
                PedSystem.Repository.CreateQuest("s_m_m_postal_01", new Vector3(83.48839, 135.14343, 80.53347), 71.96516f, 0, title: "~y~NPC~w~ Postbote Olaf\nPakete Lager", colShapeEnums: ColShapeEnums.Warehouse);

            }
            catch (Exception e)
            {
                Log.Write($"StartWork Exception: {e.ToString()}");
            }
        }
        [Interaction(ColShapeEnums.WarehouseEnter)]
        public static void WarehouseEnter(ExtPlayer player)
        {
            var characterData = player.GetCharacterData();
            if (characterData == null) return;
            player.Position = new Vector3(1048.2255, -3097.1624, -38.9999);
            Trigger.Dimension(player, 5);
            characterData.IsInPostalStock = true;
        }
        [Interaction(ColShapeEnums.WarehouseExit)]
        public static void WarehouseExit(ExtPlayer player)
        {
            var characterData = player.GetCharacterData();
            if (characterData == null) return;
            player.Position = new Vector3(132.9969, 96.3529, 83.5076);
            Trigger.Dimension(player, 0);
            characterData.IsInPostalStock = false;
        }
        [Interaction(ColShapeEnums.Warehouse)]
        public static void OpenWarehouse(ExtPlayer player, int index)
        {
            var sessionData = player.GetSessionData();
            if (sessionData == null) return;
            else if (sessionData.CuffedData.Cuffed)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.IsCuffed), 6000);
                return;
            }
            else if (sessionData.DeathData.InDeath)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.IsDying), 6000);
                return;
            }
            else if (Main.IHaveDemorgan(player, true)) return;

            player.SelectQuest(new PlayerQuestModel("npc_stock", 0, 0, false, DateTime.Now));
            Trigger.ClientEvent(player, "client.quest.open", index, "npc_stock", 0, 0, 0);
            BattlePass.Repository.UpdateReward(player, 151);
        }
        public static void Perform(ExtPlayer player, int index)
        {
            try
            {
                var characterData = player.GetCharacterData();
                if (characterData == null) return;
                if (index == 0)
                {
                    if (!FunctionsAccess.IsWorking("warehouseopen"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.FunctionOffByAdmins), 6000);
                        return;
                    }

                    Repository.LoadOtherItemsData(player, "warehouse", characterData.UUID.ToString(), 10);
                }
                else Jobs.WorkManager.JobJoin(player, 2);
            }
            catch (Exception e)
            {
                Log.Write($"OnWarehouse Exception: {e.ToString()}");
            }
        }
    }
}