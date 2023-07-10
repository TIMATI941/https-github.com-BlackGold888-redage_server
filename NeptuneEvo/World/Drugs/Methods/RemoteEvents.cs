using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEvo.Handles;
using NeptuneEvo.World.Drugs.Models;
using Redage.SDK;

namespace NeptuneEvo.World.Drugs.Methods
{
    public class RemoteEvents : Script
    {
        private static readonly nLog Log = new nLog("drugs.methods.events");

        [RemoteEvent("server.field.weed.cut")]
        private static void Cut(ExtPlayer player, int id)
        {
            try
            {
                if (id < 0) return;    

                var plant = DrugsHandler.GetPlant(player, id);

                Console.WriteLine($"{plant}");
                if (plant is null || player.Position.DistanceTo(plant.Position) > 5)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Подойдите ближе к кусту", 3000);
                    return;
                }

                plant.Cut(player);
            }
            catch(Exception ex) { Log.Write("Cut: " + ex.ToString()); } 
        }

        [RemoteEvent("server.field.playerWeed.cut")]
        private static void PlayerCut(ExtPlayer player, int id)
        {
            try
            {
                if (id < 0 || !DrugsHandler.PlayersPlants.TryGetValue(id, out FieldPlant plant)) return;

                if (plant is null || player.Position.DistanceTo(plant.Position) > 5)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Подойдите ближе к кусту", 3000);
                    return;
                }

                plant.Cut(player);
            }
            catch (Exception ex) { Log.Write("Cut: " + ex.ToString()); }
        }

        [RemoteEvent("server.field.weed.take")]
        private static void Take(ExtPlayer player, int id)
        {
            try
            {
                if (id < 0 || !DrugsHandler.PlayersPlants.TryGetValue(id, out FieldPlant plant)) return;

                if (plant is null || player.Position.DistanceTo(plant.Position) > 5)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Подойдите ближе к кусту", 3000);
                    return;
                }

                plant.Take(player);
            }
            catch(Exception ex) { Log.Write("Take: " + ex.ToString()); }
        }
    }
}
