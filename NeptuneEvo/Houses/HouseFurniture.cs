﻿using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEvo.Handles;
using Newtonsoft.Json;
using NeptuneEvo.Core;
using Redage.SDK;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Database;
using LinqToDB;
using Localization;
using MySqlConnector;
using NeptuneEvo.Accounts;
using NeptuneEvo.Players.Models;
using NeptuneEvo.Players;
using NeptuneEvo.Character.Models;
using NeptuneEvo.Character;
using NeptuneEvo.Chars.Models;
using NeptuneEvo.Functions;
using NeptuneEvo.Quests.Models;

namespace NeptuneEvo.Houses
{
    public class HouseFurniture
    {
        public string Name { get; }
        public string Model { get; }
        public int Id { get; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool IsSet { get; set; }

        [JsonIgnore]
        public GTANetworkAPI.Object obj { get; private set; }

        public HouseFurniture(int id, string name, string model)
        {
            Name = name;
            Model = model;
            Id = id;
            IsSet = false;
        }

        public GTANetworkAPI.Object Create(uint Dimension)
        {
            try
            {
                obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(Model), Position, Rotation, 255, Dimension);
                Selecting.Objects.TryAdd(obj.Id, new Selecting.ObjData
                {
                    Type = (Name.Equals("Waffentresor") ? "WeaponSafe" : 
                            Name.Equals("Kleiderschrank") ? "ClothesSafe" : 
                            Name.Equals("Einbruchsicheren Tresor") ? "BurglarProofSafe" :
                            Name.Equals("Schrank") ? "SubjectSafe" :
                            "InteriorItem"),
                    entity = obj,

                });
                return obj;
            }
            catch (Exception e)
            {
                FurnitureManager.Log.Write($"Create Exception: {e.ToString()}");
                return null;
            }
        }
    }

    public class ShopFurnitureBuy
    {
        public string Prop { get; }
        public string Type { get; }
        public int Price;
        public Dictionary<ItemId, int> Items { get; }

        public ShopFurnitureBuy(string prop, string type, int price, Dictionary<ItemId, int> items)
        {
            Prop = prop;
            Type = type;
            Price = price;
            Items = items;
        }
    }
    class FurnitureManager : Script
    {
        public static readonly nLog Log = new nLog("Houses.HouseFurniture");
        public static Dictionary<int, Dictionary<int, HouseFurniture>> HouseFurnitures = new Dictionary<int, Dictionary<int, HouseFurniture>>();
        public static string QuestName = "npc_furniture";
        public static Vector3 FurnitureBuyPos = new Vector3(-591.12317, -285.2158, 35.45478);
        public static void Init()
        {
            try
            {
                using MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "SELECT * FROM `furniture`"
                };

                using DataTable result = MySQL.QueryRead(cmd);
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                int id = 0;
                string furniture;
                foreach (DataRow Row in result.Rows)
                {
                    try
                    {
                        id = Convert.ToInt32(Row["uuid"].ToString());
                        furniture = Row["furniture"].ToString();
                        Dictionary<int, HouseFurniture> furnitures;
                        if (string.IsNullOrEmpty(furniture)) furnitures = new Dictionary<int, HouseFurniture>();
                        else furnitures = JsonConvert.DeserializeObject<Dictionary<int, HouseFurniture>>(furniture);
                        HouseFurnitures[id] = furnitures;
                    }
                    catch (Exception e)
                    {
                        Log.Write($"FurnitureManager Foreach Exception: {e.ToString()}");
                    }
                }
                Log.Write($"Loaded {HouseFurnitures.Count} players furnitures.", nLog.Type.Success);
                
                Main.CreateBlip(new Main.BlipData(566, "Möbelgeschäft",FurnitureBuyPos, 30, true));
                PedSystem.Repository.CreateQuest("s_m_y_airworker", FurnitureBuyPos, -64.57715f, questName: QuestName, title: "~y~NPC~w~ Rainer\nMöbel verkäufer", colShapeEnums: ColShapeEnums.FurnitureBuy);
            }
            catch (Exception e)
            {
                Log.Write($"FurnitureManager Exception: {e.ToString()}");
            }
        }
        [Interaction(ColShapeEnums.FurnitureBuy)]
        private static void Open(ExtPlayer player, int index)
        {
            var sessionData = player.GetSessionData();
            if (sessionData == null) return;
            if (!player.IsCharacterData()) return;
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

            player.SelectQuest(new PlayerQuestModel(QuestName, 0, 0, false, DateTime.Now));
            Trigger.ClientEvent(player, "client.quest.open", index, QuestName, 0, 0, 0);
        }
        public static Dictionary<string, ShopFurnitureBuy> NameModels = new Dictionary<string, ShopFurnitureBuy>()
		{
			{ "Waffentresor", new ShopFurnitureBuy("prop_ld_int_safe_01", "Lager", 0, new Dictionary<ItemId, int>()
				{
					{ ItemId.Iron, 200 },
					{ ItemId.Ruby, 5 },
					{ ItemId.Gold, 5 },
				}
			) },
			{ "Kleiderschrank", new ShopFurnitureBuy("bkr_prop_biker_garage_locker_01", "Lager", 1, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodOak, 44 },
				{ ItemId.WoodMaple, 20 },
				{ ItemId.WoodPine, 10 },
			}) },
			{ "Schrank", new ShopFurnitureBuy("hei_heist_bed_chestdrawer_04", "Lager", 2, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodOak, 44 },
				{ ItemId.WoodMaple, 20 },
				{ ItemId.WoodPine, 10 },
			}) },
			{ "Einbruchsicheren Tresor", new ShopFurnitureBuy("p_secret_weapon_02", "Lager", 3, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 1000 },
				{ ItemId.Gold, 200 },
				{ ItemId.Ruby, 70 },
			}) },

			{ "Ping Pong", new ShopFurnitureBuy("ch_prop_vault_painting_01a", "Gemälde", 5, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodOak, 80 },
			}) },
			{ "StreamSniper", new ShopFurnitureBuy("ch_prop_vault_painting_01b", "Gemälde", 6, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodMaple, 40 },
			}) },
			{ "Fabrik", new ShopFurnitureBuy("ch_prop_vault_painting_01f", "Gemälde", 7, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodPine, 25 },
			}) },
			{ "Verhandlung", new ShopFurnitureBuy("ch_prop_vault_painting_01h", "Gemälde", 8, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodOak, 80 },
			}) },
			{ "Girls", new ShopFurnitureBuy("ch_prop_vault_painting_01j", "Gemälde", 9, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodMaple, 40 },
			}) },

			{ "DAB", new ShopFurnitureBuy("vw_prop_casino_art_statue_01a", "Statuen", 10, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodPine, 250 },
			}) },
			{ "Twerk", new ShopFurnitureBuy("vw_prop_casino_art_statue_02a", "Statuen", 11, new Dictionary<ItemId, int>()
			{
				{ ItemId.Ruby, 150 },
			}) },
			{ "Nonne", new ShopFurnitureBuy("vw_prop_casino_art_statue_04a", "Statuen", 12, new Dictionary<ItemId, int>()
			{
				{ ItemId.Gold, 1000 },
			}) },

			{ "Paul Ridor", new ShopFurnitureBuy("hei_prop_drug_statue_01", "Figuren", 13, new Dictionary<ItemId, int>()
			{
				{ ItemId.Ruby, 7 },
			}) },
			{ "Oskar", new ShopFurnitureBuy("ex_prop_exec_award_gold", "Figuren", 14, new Dictionary<ItemId, int>()
			{
				{ ItemId.Emerald, 13 },
			}) },
			{ "Monkey King", new ShopFurnitureBuy("vw_prop_vw_pogo_gold_01a", "Figuren", 15, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "Unfall", new ShopFurnitureBuy("xs_prop_trophy_goldbag_01a", "Figuren", 16, new Dictionary<ItemId, int>()
			{
				{ ItemId.Ruby, 7 },
			}) },
			{ "Pokal FIFA", new ShopFurnitureBuy("sum_prop_ac_wifaaward_01a", "Figuren", 17, new Dictionary<ItemId, int>()
			{
				{ ItemId.Emerald, 13 },
			}) },
			{ "Champagner", new ShopFurnitureBuy("xs_prop_trophy_champ_01a", "Figuren", 18, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },

			{ "Palmen", new ShopFurnitureBuy("prop_fbibombplant", "Pflanzen", 19, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodMaple, 27 },
			}) },
			{ "Baum", new ShopFurnitureBuy("prop_plant_int_01a", "Pflanzen", 20, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodPine, 20 },
			}) },
			{ "Baum 1", new ShopFurnitureBuy("prop_plant_int_02b", "Pflanzen", 21, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodPine, 20 },
			}) },
			{ "Farn", new ShopFurnitureBuy("prop_plant_int_03b", "Pflanzen", 22, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodOak, 60 },
			}) },
			{ "Geldbaum", new ShopFurnitureBuy("prop_plant_int_04b", "Pflanzen", 23, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodMaple, 40 },
			}) },
			{ "Kaktus", new ShopFurnitureBuy("vw_prop_casino_art_plant_12a", "Pflanzen", 24, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodMaple, 27 },
			}) },

			{ "Tannenbaum", new ShopFurnitureBuy("prop_xmas_tree_int", "Tanne", 25, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodPine, 60 },
			}) },
			{ "Diamantbaum", new ShopFurnitureBuy("ch_prop_ch_diamond_xmastree", "Tanne", 26, new Dictionary<ItemId, int>()
			{
				{ ItemId.Ruby, 150 },
			}) },

			{ "Rechenmaschine", new ShopFurnitureBuy("bkr_prop_money_counter", "Schmuck", 27, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 200 },
			}) },
			{ "Geldberg", new ShopFurnitureBuy("bkr_prop_moneypack_03a", "Schmuck", 28, new Dictionary<ItemId, int>()
			{
				{ ItemId.Gold, 1000 },
			}) },
			{ "Geldberg 1", new ShopFurnitureBuy("ba_prop_battle_moneypack_02a", "Schmuck", 29, new Dictionary<ItemId, int>()
			{
				{ ItemId.Gold, 3000 },
			}) },
			{ "Geldkiste", new ShopFurnitureBuy("ex_prop_crate_money_bc", "Schmuck", 30, new Dictionary<ItemId, int>()
			{
				{ ItemId.Gold, 5000 },
			}) },
			{ "Goldkiste", new ShopFurnitureBuy("prop_ld_gold_chest", "Schmuck", 31, new Dictionary<ItemId, int>()
			{
				{ ItemId.Gold, 300 },
			}) },
			{ "Goldwagen", new ShopFurnitureBuy("p_large_gold_s", "Schmuck", 32, new Dictionary<ItemId, int>()
			{
				{ ItemId.Gold, 10000 },
			}) },
			{ "GeldCase", new ShopFurnitureBuy("prop_cash_case_02", "Schmuck", 33, new Dictionary<ItemId, int>()
			{
				{ ItemId.Gold, 1700 },
			}) },

			{ "Bierkasten", new ShopFurnitureBuy("hei_heist_cs_beer_box", "Alkohol", 34, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 50 },
			}) },
			{ "Romantisches Set", new ShopFurnitureBuy("ba_prop_club_champset", "Alkohol", 35, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodOak, 110 },
			}) },

			{ "Figuren", new ShopFurnitureBuy("vw_prop_casino_art_vase_08a", "Vasen", 36, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "Krug", new ShopFurnitureBuy("vw_prop_casino_art_vase_08a", "Vasen", 37, new Dictionary<ItemId, int>()
			{
				{ ItemId.Emerald, 13 },
			}) },
			{ "Geschwollen", new ShopFurnitureBuy("vw_prop_casino_art_vase_05a", "Vasen", 38, new Dictionary<ItemId, int>()
			{
				{ ItemId.Ruby, 7 },
			}) },
			{ "symmetrisch", new ShopFurnitureBuy("apa_mp_h_acc_vase_06", "Vasen", 39, new Dictionary<ItemId, int>()
			{
				{ ItemId.WoodOak, 40 },
			}) },
			{ "Spiegel", new ShopFurnitureBuy("apa_mp_h_acc_vase_05", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			/*{ "цветок", new ShopFurnitureBuy("apa_mp_h_acc_plant_tall_01", "Vasen", 41, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "цветок", new ShopFurnitureBuy("prop_fbibombplant", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "цветок", new ShopFurnitureBuy("prop_fbibombplant", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "свечи", new ShopFurnitureBuy("apa_mp_h_acc_candles_02", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "Maske", new ShopFurnitureBuy("apa_mp_h_acc_dec_head_01", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "пива", new ShopFurnitureBuy("beerrow_local", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "дом кинотеатр", new ShopFurnitureBuy("hei_heist_str_avunitl_03", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "дом кинотеатр", new ShopFurnitureBuy("apa_mp_h_str_avunitl_01_b", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "статуя голова", new ShopFurnitureBuy("hei_prop_hei_bust_01", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "статуя оружие", new ShopFurnitureBuy("ch_prop_ch_trophy_gunner_01a", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },
			{ "склад для оружия", new ShopFurnitureBuy("bkr_prop_gunlocker_01a", "Vasen", 40, new Dictionary<ItemId, int>()
			{
				{ ItemId.Iron, 100 },
			}) },*/
		};
        public static async Task Save(ServerBD db, int houseId)
        {
            try
            {
	            if (HouseFurnitures.ContainsKey(houseId))
	            {
		            await db.Furniture
			            .Where(f => f.Uuid == houseId)
			            .Set(f => f.Furniture, JsonConvert.SerializeObject(HouseFurnitures[houseId]))
			            .UpdateAsync();
	            }
            }
            catch (Exception e)
            {
                Log.Write($"Save Exception: {e.ToString()}");
            }
        }
        public static void Create(int id)
        {
            try
            {
                if (!HouseFurnitures.ContainsKey(id))
                {
                    using MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "INSERT INTO `furniture`(`uuid`,`furniture`,`access`) VALUES (@val0,@val1,@val3)"
                    };
                    cmd.Parameters.AddWithValue("@val0", id);
                    cmd.Parameters.AddWithValue("@val1", JsonConvert.SerializeObject(new Dictionary<int, HouseFurniture>()));
                    cmd.Parameters.AddWithValue("@val3", JsonConvert.SerializeObject(new List<string>()));
                    MySQL.Query(cmd);
                }
            }
            catch (Exception e)
            {
                Log.Write($"Create Exception: {e.ToString()}");
            }
        }

        public static void NewFurniture(int id, string name)
        {
            try
            {
                if (!HouseFurnitures.ContainsKey(id)) 
                    Create(id);
                
                var houseFurniture = HouseFurnitures[id];
                
                int i = 0;
                while (houseFurniture.ContainsKey(i)) 
                    i++;
                
                var furn = new HouseFurniture(i, name, NameModels[name].Prop);
                houseFurniture.Add(i, furn);
                
                if (NameModels[name].Type.Equals("Lager")) 
                    Chars.Repository.RemoveAll($"furniture_{id}_{i}"); //оставалось видимо в хранилище, тестануть так
            }
            catch (Exception e)
            {
                Log.Write($"newFurniture Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("acceptEdit")]
        public void ClientEvent_acceptEdit(ExtPlayer player, float X, float Y, float Z, float XX, float YY, float ZZ)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                if (!player.IsCharacterData()) return;
                if (!sessionData.HouseData.Editing) return;
                sessionData.HouseData.Editing = false;
                var house = HouseManager.GetHouse(player, true);
                if (house == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.NoHome), 6000);
                    return;
                }
                Vector3 pos = new Vector3(X, Y, Z);
                if (player.Position.DistanceTo(pos) >= 6f)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.MebelDomTooFar), 5000);
                    return;
                }
                if (!HouseFurnitures.ContainsKey(house.ID))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.MebelError), 5000);
                    return;
                }

                var furnitures = HouseFurnitures[house.ID];
                foreach (HouseFurniture p in furnitures.Values)
                {
                    if (p != null && p.IsSet && p.Position != null && p.Position.DistanceTo(pos) <= 0.5f)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.MebelTooNear), 6000);
                        return;
                    }
                }
                int id = sessionData.HouseData.EditID;
                furnitures[id].IsSet = true;
                Vector3 rot = new Vector3(XX, YY, ZZ);
                furnitures[id].Position = pos;
                furnitures[id].Rotation = rot;
                house.DestroyFurnitures();
                house.CreateAllFurnitures();
                house.IsFurnitureSave = true;
            }
            catch (Exception e)
            {
                Log.Write($"ClientEvent_acceptEdit Exception: {e.ToString()}");
            }
        }

        [RemoteEvent("cancelEdit")]
        public void ClientEvent_cancelEdit(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                sessionData.HouseData.Editing = false;
            }
            catch (Exception e)
            {
                Log.Write($"ClientEvent_cancelEdit Exception: {e.ToString()}");
            }
        }
    }
}
