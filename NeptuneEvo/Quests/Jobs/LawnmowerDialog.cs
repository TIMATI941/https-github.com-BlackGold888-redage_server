using System;
using GTANetworkAPI;
using Localization;
using NeptuneEvo.Character;
using NeptuneEvo.Chars;
using NeptuneEvo.Core;
using NeptuneEvo.Functions;
using NeptuneEvo.Handles;
using NeptuneEvo.Jobs;
using NeptuneEvo.Players;
using NeptuneEvo.Quests.Models;
using Redage.SDK;

namespace NeptuneEvo.Quests.Jobs
{
    public class LawnmowerDialog : Script
    {
        private static readonly nLog Log = new nLog("Quests.Jobs.LawnmowerDialog");
        public static readonly string QuestName = "npc_lawnmower";

        public static void Perform(ExtPlayer player)
        {
            try
            {
                if (!player.IsCharacterData()) return;
                var sessionData = player.GetSessionData();
                if (sessionData.WorkData.OnWork)
                {
                    NeptuneEvo.Jobs.Repository.JobEnd(player);
                    //WorkManager.Layoff(player);
                    UpdateData.Work(player, 0);
                }
                else
                {
                    // WorkManager.JobJoin(player, 1);
                    Lawnmower.StartWork(player);
                    UpdateData.Work(player, 5);
                }

                Log.Write($"Perfom from lawnmower job");
            }
            catch (Exception e)
            {
                Log.Write($"Perform Task.Run Exception: {e.ToString()}");
            }
        }

        public static void Action(ExtPlayer player)
        {
            try
            {
                if (!player.IsCharacterData()) return;
                var sessionData = player.GetSessionData();
                if (sessionData.WorkData.OnWork)
                {
                    Console.WriteLine("Du arbeitest");
                    Rentcar.RentCarToInterface(player, 95, 0, 0, 51);
                }
                else
                {
                    Console.WriteLine("Du arbeitest NICHT");
                }

                Log.Write($"Perfom from lawnmower job");
            }
            catch (Exception e)
            {
                Log.Write($"Perform Task.Run Exception: {e.ToString()}");
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            PedSystem.Repository.CreateQuest("a_m_y_golfer_01", new Vector3(-1331.020751953125, 47.38950729370117, 53.55971145629883), -157f, questName: QuestName, title: "~y~NPC~w~ Peter Rasen\nJob starten", colShapeEnums: ColShapeEnums.LawnmowerJob);
        }

        [Interaction(ColShapeEnums.LawnmowerJob)]
        private static void Open(ExtPlayer player, int index)
        {
            try
            {
                if (!player.IsCharacterData()) return;
                var sessionData = player.GetSessionData();
                if (sessionData == null)
                    return;

                var characterData = player.GetCharacterData();
                if (characterData == null)
                    return;

                if (sessionData.CuffedData.Cuffed)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.IsCuffed), 6000);
                    return;
                }
                if (sessionData.DeathData.InDeath)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.IsDying), 6000);
                    return;
                }

                if (Main.IHaveDemorgan(player, true)) return;
                bool isBool = qMain.SetQuests(player, QuestName, isInsert: true);
                if (!isBool) return;
                var questData = player.GetQuest();
                if (questData == null)
                    return;
                //Log.Write($"public static void RentCarToInterface(ExtPlayer player, int carId, int colorId, int hour)\r\n");

                if (sessionData.WorkData.OnWork)
                {
                    player.SelectQuest(new PlayerQuestModel(QuestName, 1, 0, false, DateTime.Now));
                    Trigger.ClientEvent(player, "client.quest.open", index, QuestName, 1, 0, 0);
                }
                else
                {
                    player.SelectQuest(new PlayerQuestModel(QuestName, 0, 0, false, DateTime.Now));
                    Trigger.ClientEvent(player, "client.quest.open", index, QuestName, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                Log.Write($"Open Task.Run Exception: {e.ToString()}");
            }
        }

    }
}