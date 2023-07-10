using System;
using System.Collections.Generic;
using System.Text;
using Redage.SDK;
using NeptuneEvo.Handles;
using NeptuneEvo.Fractions.Player;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEvo.Chars.Models;
using Localization;

namespace NeptuneEvo.World.Drugs.Models
{
    public class FieldPlant
    {
        private static readonly nLog Log = new nLog("field-plant");
        public Vector3 Position;
        public int ID = -1;
        public int Progress = 0;

        [JsonIgnore]
        public bool IsAlive = true;

        [JsonIgnore]
        public bool IsPlayerCutting = false;

        [JsonIgnore]
        public int Multiplayer = -1;

        [JsonIgnore]
        private ColShape ColShape;

        [JsonIgnore]
        private GTANetworkAPI.Object Weed;
        public void GTAElements()
        {
            try
            {
                if (ColShape != null)
                    ColShape.Delete();

                ColShape = NAPI.ColShape.CreateCylinderColShape(Position, 2f, 2f, 0);
                ColShape.OnEntityEnterColShape += (s, e) =>
                {
                    e.SetData("field.plant", this);
                };
                ColShape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("field.plant");
                };

                IsPlayerCutting = false;
            }
            catch (Exception ex) { Log.Write("GTAElements: " + ex.ToString()); }
        }

        public void RespawnWeed()
        {
            try
            {
                if (!IsAlive) return;

                if (Weed != null)
                    Weed.Delete();

                Weed = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_weed_01"), Position - new Vector3(0, 0, 1.12), new Vector3(), 255, 0);
                Weed.SetSharedData("weed.data", new { IsPot = false, ID = ID, Progress = Progress, Multiplayer = Multiplayer });
                IsAlive = true;
            }
            catch(Exception ex) { Log.Write("RespawnWeed: " + ex.ToString()); }
        }

        public string GetModelProgress()
        {
            if (Progress >= DrugsHandler.MaxProgressPlant) return "bkr_prop_weed_lrg_01b";
            else if (Progress < DrugsHandler.MaxProgressPlant && Progress > Convert.ToInt32(DrugsHandler.MaxProgressPlant / 2)) return "bkr_prop_weed_med_01b";
            else return "bkr_prop_weed_01_small_01b";
        }

        public void RespawnPot()
        {
            if (!IsAlive) return;

            NAPI.Task.Run(() =>
            {
                try
                {
                    Progress += Multiplayer;

                    uint modelProgress = NAPI.Util.GetHashKey(GetModelProgress());

                    if (Weed is null || Weed.Model != modelProgress)
                    {
                        if (Weed != null)
                            Weed.Delete();

                        Weed = NAPI.Object.CreateObject(modelProgress, Position - new Vector3(0, 0, 1.12), new Vector3(), 255, 0);
                    }
                    Weed.SetSharedData("weed.data", new { IsPot = true, ID = ID, Progress = Progress, Multiplayer = Multiplayer });
                }
                catch (Exception ex) { Log.Write("RespawnPot: " + ex.ToString()); }
            });
        }

        private DateTime LastWatering = new DateTime(2000, 1, 1);
        public void WateringCan(ExtPlayer player)
        {
            try
            {
                if (player.HasData("block.action-drugs")) return;
                if (Progress >= DrugsHandler.MaxProgressPlant)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Этот куст уже вырос!", 3000);
                    return;
                }
                if (LastWatering.AddSeconds(DrugsHandler.WateringSecondsTimeout) > DateTime.Now)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Этот куст уже поливали недавно! Попробуйте позже...", 3000);
                    return;
                }

                LastWatering = DateTime.Now;
                player.SetData("block.action-drugs", true);

                player.PlayAnimation("missfbi3_waterboard", "waterboard_loop_player", 1);
                Chars.Attachments.AddAttachment(player, Chars.Attachments.AttachmentsName.WateringCan);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player is null) return;
                        player.StopAnimation();
                        Chars.Attachments.RemoveAttachment(player, Chars.Attachments.AttachmentsName.WateringCan);

                        Multiplayer += 5;

                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы полили куст с наркотой", 3000);
                        player.ResetData("block.action-drugs");
                    }
                    catch(Exception ex) { Log.Write("WateringCan: " + ex.ToString()); }
                }, 5000);
            }
            catch(Exception ex) { Log.Write("WateringCan: " + ex.ToString()); }
        }

        public void Destroy()
        {
            try
            {
                if (Weed is null) return;

                IsAlive = false;
                Weed.Delete();

                if (ColShape != null)
                    ColShape.Delete();

                if (DrugsHandler.PlayersPlants.ContainsKey(ID))
                    DrugsHandler.PlayersPlants.Remove(ID);

                MySQL.Query($"DELETE FROM `{DrugsHandler.DBPlants}` WHERE `id`={ID}");
            }
            catch(Exception ex) { Log.Write("Destroy: " + ex.ToString()); }
        }

        public void Take(ExtPlayer player)
        {
            try
            {
                if (Multiplayer == -1) return;
                if (Chars.Repository.AddNewItem(player, $"char_{player.CharacterData.UUID}", "inventory", ItemId.DrugPot, 1, $"{Progress}") == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.Ru, DataName.NoSpaceInventory), 6000);
                    return;
                }

                Destroy();
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы забрали горшок с марихуаной", 3000);

                player.ResetData("field.plant");
            }
            catch(Exception ex) { Log.Write("Take: " + ex.ToString()); }
        }

        public void Cut(ExtPlayer player)
        {
            try
            {
                if (player.HasData("block.action-drugs")) return;
                if (IsPlayerCutting)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Кто-то уже срезает куст...", 3000);
                    return;
                }

                var fractionData = player.GetFractionData();
                if (fractionData != null && (fractionData.Id == (int)Fractions.Models.Fractions.POLICE || fractionData.Id == (int)Fractions.Models.Fractions.FIB || fractionData.Id == (int)Fractions.Models.Fractions.CITY))
                {
                    Destroy();
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы успешно уничтожили куст с наркотиками. Поздравляю, еще на один куст в мире меньше!", 5000);
                    return;
                }

                if (Progress < DrugsHandler.MaxProgressPlant)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Куст еще не вырос!", 3000);
                    return;
                }

                IsPlayerCutting = true;

                player.PlayAnimation("amb@prop_human_movie_studio_light@base", "base", 1);
                player.SetData("block.action-drugs", true);

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player is null) return;

                        player.StopAnimation();
                        Destroy();

                        if (Multiplayer == -1)
                        {
                            int chance = new Random().Next(0, 101);
                            if (chance >= (100 - DrugsHandler.ChanceToGiveSeed))
                            {
                                if (Chars.Repository.AddNewItem(player, $"char_{player.CharacterData.UUID}", "inventory", ItemId.DrugSeed, 1) == -1)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.Ru, DataName.NoSpaceInventory), 6000);
                                    return;
                                }
                            }
                        }

                        if (Chars.Repository.AddNewItem(player, $"char_{player.CharacterData.UUID}", "inventory", ItemId.DrugPlant, 1) == -1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.Ru, DataName.NoSpaceInventory), 6000);
                            return;
                        }

                        player.ResetData("block.action-drugs");
                        player.ResetData("field.plant");
                    }
                    catch(Exception ex) { Log.Write("Cut: " + ex.ToString()); }
                }, 5000);
            }
            catch(Exception ex) { Log.Write("Cut: " + ex.ToString()); }
        }
    }
}
