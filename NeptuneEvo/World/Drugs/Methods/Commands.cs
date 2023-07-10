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
    public class Commands : Script
    {
        private static readonly nLog Log = new nLog("drugs.methods.commands");

        [Command]
        private static void CreateField(ExtPlayer player, int radius)
        {
            try
            {
                if (player.HasData("drug.field"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Du befindest dich bereits auf einem Feld. Wähle einen anderen Ort", 3000);
                    return;
                }

                var field = new Field()
                {
                    ID = DrugsHandler.Fields.Count,
                    Position = player.Position,
                    Range = radius,
                };
                DrugsHandler.Fields.Add(field);
                field.GTAElements();
                NAPI.Marker.CreateMarker(1, new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 5), new Vector3(), new Vector3(), radius * 2, 255, 255, 255, false, 0);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie haben ein Feld mit der ID #{field.ID} erstellt", 3000);
                MySQL.Query($"INSERT INTO `{DrugsHandler.DBFields}` (`id`, `position`, `range`, `plants`) VALUES({field.ID}, '{JsonConvert.SerializeObject(field.Position)}', {field.Range}, '{JsonConvert.SerializeObject(field.Plants)}')");
            }
            catch(Exception ex) { Log.Write("CreateField: " + ex.ToString()); }
        }

        [Command]
        private static void AddFieldPlant(ExtPlayer player)
        {
            try
            {
                if (!player.HasData("drug.field")) return;
                if (player.HasData("field.plant"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Du befindest dich bereits auf einer Pflanze. Wähle einen anderen Ort", 3000);
                    return;
                }
                Field field = player.GetData<Field>("drug.field");

                var plant = new FieldPlant()
                {
                    ID = field.Plants.Count,
                    Position = player.Position,
                    Progress = DrugsHandler.MaxProgressPlant
                };
                field.Plants.Add(plant);

                field.Reload();

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Sie haben eine Feldplantage erstellt #{field.ID}", 3000);
                MySQL.Query($"UPDATE `{DrugsHandler.DBFields}` SET `plants` = '{JsonConvert.SerializeObject(field.Plants)}' WHERE `id`={field.ID}");
            }
            catch(Exception ex) { Log.Write("ReloadFields: " + ex.ToString()); }
        }
    }
}
