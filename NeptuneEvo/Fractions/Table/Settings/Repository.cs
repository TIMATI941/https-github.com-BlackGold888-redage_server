using System;
using NeptuneEvo.Fractions.Models;
using NeptuneEvo.Fractions.Player;
using NeptuneEvo.Handles;
using NeptuneEvo.Table.Models;
using Redage.SDK;

namespace NeptuneEvo.Fractions.Table.Settings
{
    public class Repository
    {
        public static void UpdateStock(ExtPlayer player)
        {
            try
            {
                if (!player.IsFractionAccess(RankToAccess.OpenStock)) return;
                
                var fractionData = player.GetFractionData();
                if (fractionData == null) 
                    return;

                fractionData.IsOpenStock = !fractionData.IsOpenStock;

                if (fractionData.IsOpenStock)
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Du hast das Craften von Waffen erlaubt", 3000);
                    Fractions.Table.Logs.Repository.AddLogs(player, FractionLogsType.OpenStock, "Waffencraft erlaubt");
                    Manager.sendFractionMessage(fractionData.Id, "!{#ADFF2F}[F] " + $"{player.Name} ({player.Value}) hat das Craften von Waffen erlaubt", true);
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Du hast das Craften von Waffen verboten", 3000);
                    Fractions.Table.Logs.Repository.AddLogs(player, FractionLogsType.CloseStock, "Waffencraft verboten");
                    Manager.sendFractionMessage(fractionData.Id, "!{#ADFF2F}[F] " + $"{player.Name} ({player.Value}) hat das Craften von Waffen verboten", true);
                }

                Trigger.ClientEvent(player, "client.frac.main.isStock", fractionData.IsOpenStock);
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }
        public static void UpdateGunStock(ExtPlayer player)
        {
            try
            {
                if (!player.IsFractionAccess(RankToAccess.OpenGunStock)) return;
                
                var fractionData = player.GetFractionData();
                if (fractionData == null) 
                    return;

                fractionData.IsOpenGunStock = !fractionData.IsOpenGunStock;

                if (fractionData.IsOpenGunStock)
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Du hast das Fraktionslager geöffnet", 3000);
                    Fractions.Table.Logs.Repository.AddLogs(player, FractionLogsType.OpenStock, "Öffnet Lager");
                    Manager.sendFractionMessage(fractionData.Id, "!{#ADFF2F}[F] " + $"{player.Name} ({player.Value}) öffnet das Lager", true);
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Du hast das Fraktionslager geschlossen", 3000);
                    Fractions.Table.Logs.Repository.AddLogs(player, FractionLogsType.CloseStock, "Lager geshlossen");
                    Manager.sendFractionMessage(fractionData.Id, "!{#ADFF2F}[F] " + $"{player.Name} ({player.Value}) schließt das Lager", true);
                }

                Trigger.ClientEvent(player, "client.frac.main.isGunStock", fractionData.IsOpenGunStock);
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }
    }
}