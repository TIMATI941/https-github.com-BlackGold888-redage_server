using System;
using GTANetworkAPI;
using Localization;
using NeptuneEvo.Character;
using NeptuneEvo.Chars;
using NeptuneEvo.Functions;
using NeptuneEvo.Handles;
using NeptuneEvo.Jobs;
using NeptuneEvo.Players;
using NeptuneEvo.Quests.Models;
using Redage.SDK;

namespace NeptuneEvo.Quests.Jobs
{
    public class Electric : Script
    {
        private static readonly nLog Log = new nLog("Quests.Jobs.Electric");
        public static string QuestName = "npc_electrician";

        public static void Perform(ExtPlayer player)
        {
            try
            {
                if (!player.IsCharacterData()) return;
                var sessionData = player.GetSessionData();
                if (sessionData.WorkData.OnWork)
                {
                    Electrician.EndWork(player);
                    //WorkManager.Layoff(player);
                    UpdateData.Work(player, 0);
                }
                else
                {
                    //WorkManager.JobJoin(player, 1);
                    Electrician.StartWork(player);
                    UpdateData.Work(player, 1);
                }

                Log.Write($"Perfom from electric job");
            }
            catch (Exception e)
            {
                Log.Write($"Perform Task.Run Exception: {e.ToString()}");
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            PedSystem.Repository.CreateQuest("u_m_m_partytarget", new Vector3(726.9292602539062, 139.1497802734375, 80.75456237792969), -157f, questName: QuestName, title: "~y~NPC~w~ Jenny Ingold\nJob starten", colShapeEnums: ColShapeEnums.ElectricJob);
        }

        [Interaction(ColShapeEnums.ElectricJob)]
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
                Log.Write($"Open from electric job2222");

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