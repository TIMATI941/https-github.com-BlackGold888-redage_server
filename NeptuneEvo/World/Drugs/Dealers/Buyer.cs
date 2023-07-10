using System;
using System.Collections.Generic;
using System.Text;
using Redage.SDK;
using NeptuneEvo.Chars;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.Handles;
using NeptuneEvo.Functions;
using GTANetworkAPI;

namespace NeptuneEvo.World.Drugs.Dealers
{
    public class Buyer
    {
        private static readonly nLog Log = new nLog("Deallers.Buyer");
        private Ped Ped;

        public Vector3 Position;
        public Vector3 Rotation;

        public ExtColShape ColShape;
        public TextLabel Label;

        private Dictionary<ItemId, int> Prices = new Dictionary<ItemId, int>()
        {
            { ItemId.DrugSeed, 0 }
        };

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
                foreach (var data in Prices)
                {
                    text += $"~r~{Repository.ItemsInfo[data.Key].Name} ~w~- ~g~{data.Value}$ \n";
                }
                Label = NAPI.TextLabel.CreateTextLabel(text, Position + new Vector3(0, 0, .75), 10f, 1f, 0, new Color(255, 255, 255, 255), false, 0);

                ColShape = CustomColShape.CreateCylinderColShape(Position, 2.0f, 2, 0, ColShapeEnums.DrugBuyer);
                ColShape.OnEntityEnterColShape += (s, e) =>
                {
                    e.SetData("drug.dealer.buyer", this);
                };
                ColShape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("drug.dealer.buyer");
                };
            }
            catch (Exception ex) { Log.Write("GTAElements: " + ex.ToString()); }
        }

        public void ChangePrice(ItemId item, int price)
        {
            try
            {
                if (!Prices.ContainsKey(item)) return;
                Prices[item] = price;
            }
            catch(Exception ex) { Log.Write("ChangePrice: " + ex.ToString()); }
        }

        public void Interaction(ExtPlayer player)
        {
            try
            {
                string locationName = $"char_{player.CharacterData.UUID}";

                List<ItemStruct> itemsToRemove = new List<ItemStruct>();
                int finishPrice = 0;
                foreach (var data in Prices)
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
                                    finishPrice += data.Value;
                                }
                            }
                        }
                    }
                }

                Repository.RemoveFix(player, locationName, itemsToRemove);

                if (itemsToRemove.Count == 0)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы ничего не продали....", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы продали товара на {finishPrice}$", 3000);
                    MoneySystem.Wallet.Change(player, finishPrice);
                }
            } 
            catch(Exception ex) { Log.Write("Interaction: " + ex.ToString()); }
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
            catch(Exception ex) { Log.Write("Destroy: " + ex.ToString()); }
        }
    }
}
