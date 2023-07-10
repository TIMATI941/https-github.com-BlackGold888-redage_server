using System;
using System.Collections.Generic;
using System.Text;
using Redage.SDK;
using NeptuneEvo.Chars;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.Handles;
using Localization;
using NeptuneEvo.Functions;
using GTANetworkAPI;

namespace NeptuneEvo.World.Drugs.Dealers
{
    public class Changer
    {
        private static readonly nLog Log = new nLog("Deallers.Changer");
        private Ped Ped;

        public Vector3 Position;
        public Vector3 Rotation;

        public ExtColShape ColShape;
        public TextLabel Label;

        private Dictionary<ItemId, int> Items = new Dictionary<ItemId, int>()
        {
            // Предмет - Время ожидания (в секундах)
            { ItemId.DrugPlant, 10 }
        };

        private Dictionary<int, List<ChangerItem>> Inventory = new Dictionary<int, List<ChangerItem>>();

        public void Respawn()
        {
            try
            {
                if (Ped != null)
                    Ped.Delete();

                if (ColShape != null)
                    ColShape.Delete();

                if (Label != null)
                    Label.Delete();

                Ped = NAPI.Ped.CreatePed((uint)PedHash.G, Position, Rotation.Z, false, true, true, true, 0);

                string text = "";
                foreach (var data in Items)
                {
                    text += $"~r~{Repository.ItemsInfo[data.Key].Name} ~w~- Ожидание ~y~{data.Value} Секунд \n";
                }
                Label = NAPI.TextLabel.CreateTextLabel(text, Position + new Vector3(0, 0, .75), 10f, 1f, 0, new Color(255, 255, 255, 255), false, 0);

                ColShape = CustomColShape.CreateCylinderColShape(Position, 2.0f, 2, 0, ColShapeEnums.DrugChanger);
                ColShape.OnEntityEnterColShape += (s, e) =>
                {
                    e.SetData("drug.dealer.changer", this);
                };
                ColShape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("drug.dealer.changer");
                };
            }
            catch (Exception ex) { Log.Write("GTAElements: " + ex.ToString()); }
        }

        public void Interaction(ExtPlayer player)
        {
            try
            {
                if (!Inventory.ContainsKey(player.CharacterData.UUID))
                    Inventory[player.CharacterData.UUID] = new List<ChangerItem>();

                var playerDealerInventory = Inventory[player.CharacterData.UUID];
                string locationName = $"char_{player.CharacterData.UUID}";

                List<ItemStruct> itemsToRemove = new List<ItemStruct>();
                foreach (var data in Items)
                {
                    if (Repository.ItemsData.ContainsKey(locationName))
                    {
                        foreach (string Location in Repository.ItemsData[locationName].Keys)
                        {
                            foreach (var itemData in Repository.ItemsData[locationName][Location])
                            {
                                if (itemData.Value.ItemId == data.Key)
                                {
                                    itemsToRemove.Add(new ItemStruct(Location, itemData.Key, itemData.Value));
                                    playerDealerInventory.Add(new ChangerItem()
                                    {
                                        Count = itemData.Value.Count,
                                        Type = itemData.Value.ItemId,
                                        ReadyTime = DateTime.Now.AddSeconds(data.Value)
                                    });
                                }
                            }
                        }
                    }
                }

                if (itemsToRemove.Count == 0)
                {
                    if (playerDealerInventory.Count == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "У вас нечего предложить диллеру!", 3000);
                    }
                    else
                    {
                        foreach(var item in playerDealerInventory)
                        {
                            if (item.ReadyTime < DateTime.Now)
                            {
                                if (Chars.Repository.AddNewItem(player, $"char_{player.CharacterData.UUID}", "inventory", item.Type, item.Count) == -1)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.Ru, DataName.NoSpaceInventory), 6000);
                                    return;
                                }
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Дилер пока не готов отдать вам товар - {Repository.ItemsInfo[item.Type].Name}!", 3000);
                            }
                        }
                    }
                }
                else
                {
                    Repository.RemoveFix(player, locationName, itemsToRemove);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы отдали свои вещи диллеру. Приходите позже!", 3000);
                }
            }
            catch (Exception ex) { Log.Write("Interaction: " + ex.ToString()); }
        }

        public void Destroy()
        {
            try
            {
                if (Ped != null)
                    Ped.Delete();

                if (Label != null)
                    Label.Delete();

                if (ColShape != null)
                    ColShape.Delete();
            }
            catch (Exception ex) { Log.Write("Destroy: " + ex.ToString()); }
        }

        public class ChangerItem
        {
            public ItemId Type;
            public int Count;
            public DateTime ReadyTime;
        }
    }
}
