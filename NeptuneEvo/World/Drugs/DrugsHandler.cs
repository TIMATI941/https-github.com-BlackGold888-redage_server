using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySqlConnector;
using Redage.SDK;
using NeptuneEvo.Handles;
using NeptuneEvo.Functions;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.World.Drugs.Models;
using NeptuneEvo.World.Drugs.Dealers;
using Newtonsoft.Json;

namespace NeptuneEvo.World.Drugs
{
    public class DrugsHandler : Script
    {
        private static readonly nLog Log = new nLog("DrugsHandler");

        public static List<Field> Fields = new List<Field>();
        public static Dictionary<int, FieldPlant> PlayersPlants = new Dictionary<int, FieldPlant>();

        private static List<Buyer> Buyers = new List<Buyer>();
        private static List<Changer> Changers = new List<Changer>();

        private static int LastIDPlant = 0;

        public static readonly string DBFields = "drugs_fields";
        public static readonly string DBPlants = "drugs_plants";

        public static readonly int MaxProgressPlant = 500; // Максимальный прогрес, который нужно достигнуть чтобы растение выросло!
        public static readonly int StepProgressPlant = 10; // Число, которое добавляется каждую секунду

        public static readonly int WateringSecondsTimeout = 30; // КД на поливание лейкой
        public static readonly int WateringMultiplayer = 5; // Добавочное число к параметру StepProgressPlant, которое добавляется при поливании куста

        public static readonly int ChanceToGiveSeed = 30; // Шанс, чтобы при срезании выдавалось семя. Максимум 100

        public static readonly int MinPriceForDrugSeed = 100; // Минимальная цена для продажи семян
        public static readonly int MaxPriceForDrugSeed = 1000; // Максимальная цена для продажи семян

        public static readonly int CountBuyersOnMap = 1; // Число скупщиков на карте
        public static readonly int CountChangersOnMap = 1; // Число обменщиков на карте

        public static void Initialize()
        {
            try
            {
                var data = MySQL.QueryRead($"SELECT * from `{DBFields}`");
                if (data != null && data.Rows.Count != 0)
                {
                    foreach(DataRow Row in data.Rows)
                    {
                        var field = new Field()
                        {
                            ID = Convert.ToInt32(Row["id"]),
                            Position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString()),
                            Range = Convert.ToInt32(Row["range"]),
                            Plants = JsonConvert.DeserializeObject<List<FieldPlant>>(Row["plants"].ToString()),
                        };

                        Fields.Add(field);

                        field.GTAElements();
                    }
                }

                Log.Write($"{Fields.Count} Drogenfelder wurden geladen");

                data = MySQL.QueryRead($"SELECT * from `{DBPlants}`");
                if (data != null && data.Rows.Count != 0)
                {
                    foreach (DataRow Row in data.Rows)
                    {
                        var plant = new FieldPlant()
                        {
                            ID = Convert.ToInt32(Row["id"]),
                            Progress = Convert.ToInt32(Row["progress"]),
                            Multiplayer = StepProgressPlant,
                            IsAlive = true,
                            Position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString())
                        };

                        if (plant.ID > LastIDPlant)
                            LastIDPlant = plant.ID;

                        plant.GTAElements();
                        plant.RespawnPot();

                        PlayersPlants.TryAdd(plant.ID, plant);
                    }
                }

                Log.Write($"{PlayersPlants.Count} Marihuanapflanzen wurden geladen");

                RespawnPlants();
                CreateDealler();

                Timers.Start(1000, () => Interval());
            }
            catch(Exception ex) { Log.Write("Initialize: " + ex.ToString()); }
        }

        public static void CreateDealler()
        {
            try
            {
                if (CountChangersOnMap + CountBuyersOnMap > DealersPositions.Count + 3)
                {
                    Log.Write("Nicht genug freie Dealer Positionen...", nLog.Type.Error);
                    return;
                }

                Buyers.ForEach((x) => x.Destroy());
                Buyers.Clear();

                Changers.ForEach((x) => x.Destroy());
                Changers.Clear();

                var listOccupiedPositions = new List<int>();
                for (int i = 0; i < CountBuyersOnMap; i++)
                {
                    var idPosition = GetRandomPosition(listOccupiedPositions);
                    listOccupiedPositions.Add(idPosition);

                    var buyer = new Buyer()
                    {
                        Position = DealersPositions[idPosition][0],
                        Rotation = DealersPositions[idPosition][1],
                    };

                    buyer.ChangePrice(ItemId.DrugSeed, new Random().Next(MinPriceForDrugSeed, MaxPriceForDrugSeed));
                    buyer.Respawn();

                    Buyers.Add(buyer);
                }

                for (int i = 0; i < CountChangersOnMap; i++)
                {
                    var idPosition = GetRandomPosition(listOccupiedPositions);
                    listOccupiedPositions.Add(idPosition);

                    var changer = new Changer()
                    {
                        Position = DealersPositions[idPosition][0],
                        Rotation = DealersPositions[idPosition][1],
                    };

                    changer.Respawn();

                    Changers.Add(changer);
                }
            }
            catch(Exception ex) { Log.Write("CreateDeallers: " + ex.ToString()); }
        }

        [Interaction(ColShapeEnums.DrugBuyer)]
        public static void DealerBuyerInteraction(ExtPlayer player)
        {
            try
            {
                if (!player.HasData("drug.dealer.buyer")) return;

                var dealer = player.GetData<Buyer>("drug.dealer.buyer");
                if (dealer is null) return;

                dealer.Interaction(player);
            }
            catch(Exception ex) { Log.Write("DealerBuyerInteraction: " + ex.ToString()); }
        }

        [Interaction(ColShapeEnums.DrugChanger)]
        public static void DealerChangerInteraction(ExtPlayer player)
        {
            try
            {
                if (!player.HasData("drug.dealer.changer")) return;

                var dealer = player.GetData<Changer>("drug.dealer.changer");
                if (dealer is null) return;

                dealer.Interaction(player);
            }
            catch (Exception ex) { Log.Write("DealerChangerInteraction: " + ex.ToString()); }
        }

        public static int GetRandomPosition(List<int> list)
        {
            try
            {
                int id = new Random().Next(0, DealersPositions.Count);

                if (list.Contains(id)) 
                    return GetRandomPosition(list);
                else 
                    return id;
            }
            catch(Exception ex) { Log.Write("GetRandomPosition: " + ex.ToString()); return GetRandomPosition(list); }
        }

        public static void RespawnPlants()
        {
            try
            {
                foreach(var field in Fields)
                {
                    
                    field.Reload();
                }

                Log.Write("Alle Marihuana Pflanzen wurden gepflanzt");
            }
            catch(Exception ex) { Log.Write("RespawnPlants: " + ex.ToString()); }
        }

        public static void WateringPlant(ExtPlayer player)
        {
            try
            {
                var plant = player.GetData<FieldPlant>("field.plant");

                if (plant is null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Куст завял...", 3000);
                    return;
                }

                plant.WateringCan(player);
            }
            catch(Exception ex) { Log.Write("WateringPlant: " + ex.ToString()); }
        }

        public static FieldPlant GetPlant(ExtPlayer player, int id)
        {
            try
            {
                if (!player.HasData("drug.field")) return null;
                
                Field field = player.GetData<Field>("drug.field");
                return field.Plants.ElementAt(id);
            }
            catch(Exception ex) { Log.Write("GetPlant: " + ex.ToString()); return null; }
        }

        public static void CreatePlant(ExtPlayer player, int progress)
        {
            try
            {
                player.PlayAnimation("amb@world_human_gardener_plant@female@base", "base_female", 1);

                LastIDPlant++;
                var plant = new FieldPlant()
                {
                    Position = player.Position,
                    ID = LastIDPlant,
                    Progress = progress,
                    Multiplayer = StepProgressPlant,
                };

                player.SetData("block.action-drugs", true);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player is null) return;
                        player.StopAnimation();

                        plant.GTAElements();
                        plant.RespawnPot();
                        PlayersPlants.TryAdd(plant.ID, plant);

                        player.ResetData("block.action-drugs");
                    }
                    catch(Exception ex) { Log.Write("CreatePlant.Task.Run: " + ex.ToString()); }
                }, 5000);

                MySQL.Query($"INSERT INTO `{DBPlants}` (`id`,`progress`,`position`) VALUES({plant.ID}, {plant.Progress}, '{JsonConvert.SerializeObject(plant.Position)}')");
            }
            catch(Exception ex) { Log.Write("CreatePlant: " + ex.ToString()); }
        }

        public static void Interval()
        {
            Trigger.SetTask(() =>
            {
                try
                {
                    foreach(var item in PlayersPlants)
                    {
                        FieldPlant plant = item.Value;
                        int id = item.Key;

                        if (plant.Progress >= MaxProgressPlant) continue;

                        plant.RespawnPot();

                        Console.WriteLine($"ID: {id}; PROGRESS: {plant.Progress}");
                    }
                }
                catch(Exception ex) { Log.Write("Interval: " + ex.ToString()); }
            });
        }

        public static async Task Save()
        {
            try
            {
                foreach(var plant in PlayersPlants)
                {
                    MySqlCommand sqlCommand = new MySqlCommand($"UPDATE `{DBPlants}` SET progress=@PROGRESS WHERE `id`=@ID");
                    sqlCommand.Parameters.AddWithValue("@PROGRESS", plant.Value.Progress);
                    sqlCommand.Parameters.AddWithValue("@ID", plant.Value.ID);

                    await MySQL.QueryAsync(sqlCommand);
                }
            }
            catch(Exception ex) { Log.Write("Save: " + ex.ToString()); }
        }

        private static readonly List<Vector3[]> DealersPositions = new List<Vector3[]>()
        {
            new Vector3[]
            {
                new Vector3(2196.087, 5592.561, 53.749),
                new Vector3(0, 0, 0)
            }, 
            new Vector3[]
            {
                new Vector3(2191.6184, 5601.1753, 53.7108),
                new Vector3(0, 0, 0)
            },
            new Vector3[]
            {
                new Vector3(-1010.9905, -502.19833, 37.22274),
                new Vector3(0, 0, 55.846615)
            },
            new Vector3[]
            {
                new Vector3(2016.2273, 4996.1494, 41.031418),
                new Vector3(0, 0, 123.42699)
            },
        };
    }
}
