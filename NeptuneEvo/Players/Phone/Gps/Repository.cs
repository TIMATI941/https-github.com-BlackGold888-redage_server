﻿using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using NeptuneEvo.Accounts;
using NeptuneEvo.Character;
using NeptuneEvo.Core;
using NeptuneEvo.Fractions;
using NeptuneEvo.Fractions.Player;
using NeptuneEvo.Handles;
using NeptuneEvo.Houses;
using NeptuneEvo.MoneySystem;
using NeptuneEvo.Organizations.Player;
using NeptuneEvo.Players.Phone.Gps.Models;
using Newtonsoft.Json;
using NeptuneEvo.Fractions.LSNews;

namespace NeptuneEvo.Players.Phone.Gps
{
    public class Repository
    {
        public static string GpsListJson = "[]";

        public static void Init()
        {
            var gpsList = new List<GpsCategory>();

             var recentCategory = new GpsCategory();
            recentCategory.Name = "Umgebung";
            recentCategory.Icon = "recent";
            recentCategory.Items = new List<GpsItem>();

            var posList = new List<List<object>>();

            foreach (var item in ATM.ATMs)
                posList.Add(new List<object>
                {
                    item.X,
                    item.Y
                });
            
            recentCategory.Items.Add(new GpsItem("Nächster Geldautomat", posList));
            
            //
            for (byte i = 0; i != BusinessManager.BusinessTypeNames.Length; i++)
            {
                posList = new List<List<object>>();

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    if (biz.Type != i) continue;
                    posList.Add(new List<object>
                    {
                        biz.EnterPoint.X,
                        biz.EnterPoint.Y
                    });
                }

                if (posList.Count > 0)
                    recentCategory.Items.Add(new GpsItem($"nächster '{BusinessManager.BusinessTypeNames[i]}'", posList));
            }
            
            //
            
            posList = new List<List<object>>();

            foreach (var item in Jobs.Bus.BusStations.Values)
                posList.Add(new List<object>
                {
                    item.X,
                    item.Y
                });

            recentCategory.Items.Add(new GpsItem("Nächste Bushaltestelle", posList));
            
            //
            
            posList = new List<List<object>>();
            var indexs = new List<RentCarId>()
                { RentCarId.Civilian, RentCarId.OffRoad, RentCarId.Holiday, RentCarId.Elite, RentCarId.Rally };
            
            foreach (var item in Rentcar.RentPedsData)
            {
                if (!indexs.Contains(item.Index)) continue;
                posList.Add(new List<object>
                {
                    item.Position.X,
                    item.Position.Y
                });
            }

            recentCategory.Items.Add(new GpsItem("Nächster Motorradverleih", posList));

            ///*
            /*
            posList = new List<List<object>>();
            indexs = new List<RentCarId>()
                { RentCarId.Cycling };
            
            foreach (var item in Rentcar.RentPedsData)
            {
                if (!indexs.Contains(item.Index)) continue;
                posList.Add(new List<object>
                {
                    item.Position.X,
                    item.Position.Y
                });
            }

            recentCategory.Items.Add(new GpsItem("Ближайшая аренда велосипеда", posList));
            */
            //
            
            posList = new List<List<object>>();
            indexs = new List<RentCarId>()
                { RentCarId.WaterBased };
            
            foreach (var item in Rentcar.RentPedsData)
            {
                if (!indexs.Contains(item.Index)) continue;
                posList.Add(new List<object>
                {
                    item.Position.X,
                    item.Position.Y
                });
            }

            recentCategory.Items.Add(new GpsItem("Nächster Bootsverleih", posList));
            
            //
            
            /*posList = new List<List<object>>();
            indexs = new List<RentCarId>()
                { RentCarId.Aeroplane };
            
            foreach (var item in Rentcar.RentPedsData)
            {
                if (!indexs.Contains(item.Index)) continue;
                posList.Add(new List<object>
                {
                    item.Position.X,
                    item.Position.Y
                });
            }

            recentCategory.Items.Add(new GpsItem("Ближайшая аренда самолета", posList));
            
            //
            
            posList = new List<List<object>>();
            indexs = new List<RentCarId>()
                { RentCarId.Helicopter };
            
            foreach (var item in Rentcar.RentPedsData)
            {
                if (!indexs.Contains(item.Index)) continue;
                posList.Add(new List<object>
                {
                    item.Position.X,
                    item.Position.Y
                });
            }

            recentCategory.Items.Add(new GpsItem("Ближайшая аренда вертолета", posList));*/
            
            gpsList.Add(recentCategory);
            
            //

            var funCategory = new GpsCategory();
            funCategory.Name = "Unterhaltung";
            funCategory.Icon = "smiles";
            funCategory.Items = new List<GpsItem>();
            
            
            funCategory.Items.Add(new GpsItem("Kasino", -367.80692f, -241.19814f));
            funCategory.Items.Add(new GpsItem("Arena 3 in 1", -483.149f, -400.09946f));
            funCategory.Items.Add(new GpsItem("AirDrop", -483.149f, -400.09946f));
            funCategory.Items.Add(new GpsItem("Anfangsquest", -480.28732f, -305.2201f));
           // funCategory.Items.Add(new GpsItem("Клуб Diamond", 945.4526f, 16.871f));
            funCategory.Items.Add(new GpsItem("Club Vanilla", 141.3792f, -1281.576f));
            funCategory.Items.Add(new GpsItem("Club Tequilla", -564.5512f, 275.6993f));
            funCategory.Items.Add(new GpsItem("Club Bahama Mamas West", -1388.761f, -586.3921f));
            funCategory.Items.Add(new GpsItem("Halbinsel", -1497.7688f, -1484.525f));
            funCategory.Items.Add(new GpsItem("Tanzfläche", -1705.88f, -970.7998f));
            funCategory.Items.Add(new GpsItem("Strand", 1268.207f, -3325.8962f));
            funCategory.Items.Add(new GpsItem("Cafe Stand Up",  -439.601f, 271.704f));
            funCategory.Items.Add(new GpsItem("China Imbiss", -158.242f, 303.829f));
            
            gpsList.Add(funCategory);
            
            //

            var bizCategory = new GpsCategory();
            bizCategory.Name = "Business";
            bizCategory.Icon = "247";
            bizCategory.Items = new List<GpsItem>();
            
            bizCategory.Items.Add(new GpsItem("Exotic DonateRoom", VehicleModel.DonateAutoRoom.NpcBuyPosition[1].X, VehicleModel.DonateAutoRoom.NpcBuyPosition[1].Y));
            bizCategory.Items.Add(new GpsItem("Elite AutoRoom", VehicleModel.EliteAutoRoom.NpcBuyPosition.X, VehicleModel.EliteAutoRoom.NpcBuyPosition.Y));
            bizCategory.Items.Add(new GpsItem("AirRoom", VehicleModel.AirAutoRoom.NpcBuyPosition.X, VehicleModel.AirAutoRoom.NpcBuyPosition.Y));
            bizCategory.Items.Add(new GpsItem("Premium Clothes Shop", -1126.9141f, -1440.1637f));

            //
            
            for (byte i = 0; i != BusinessManager.BusinessTypeNames.Length; i++)
            {
                posList = new List<List<object>>();

                foreach (var biz in BusinessManager.BizList.Values)
                {
                    if (biz.Type != i) continue;
                    posList.Add(new List<object>
                    {
                        biz.EnterPoint.X,
                        biz.EnterPoint.Y,
                        biz.ID
                    });
                }

                if (posList.Count > 0) 
                    bizCategory.Items.Add(new GpsItem(BusinessManager.BusinessTypeNames[i], posList));
            }

            //
            
            gpsList.Add(bizCategory);
            
            
            //Гос
            var gosCategory = new GpsCategory();
            
            gosCategory.Name = "Staatliche Einrichtungen";
            gosCategory.Icon = "gos";
            gosCategory.Items = new List<GpsItem>();

            gosCategory.Items.Add(new GpsItem("City Hall", Fractions.Cityhall.GpsPosition.X, Fractions.Cityhall.GpsPosition.Y));
            gosCategory.Items.Add(new GpsItem("National Guard", -294.8565f, -2605.4185f));
            gosCategory.Items.Add(new GpsItem("LSPD", Fractions.Police.GunsPosition.X, Fractions.Police.GunsPosition.Y));
            gosCategory.Items.Add(new GpsItem("EMS", Fractions.Ems.emsCheckpoints[0].X, Fractions.Ems.emsCheckpoints[0].Y));
            gosCategory.Items.Add(new GpsItem("FIB", Fractions.Fbi.GpsPosition.X, Fractions.Fbi.GpsPosition.Y));
            gosCategory.Items.Add(new GpsItem("NEWS", LsNewsSystem.LSNewsCoords[0].X, LsNewsSystem.LSNewsCoords[0].Y));
            gosCategory.Items.Add(new GpsItem("SHERIFF 1", Fractions.Sheriff.FirstPosition.X, Fractions.Sheriff.FirstPosition.Y));
            gosCategory.Items.Add(new GpsItem("SHERIFF 2", Fractions.Sheriff.SecondPosition.X, Fractions.Sheriff.SecondPosition.Y));

            gpsList.Add(gosCategory);
            
            //
            
            var weaponsCategory = new GpsCategory();
            weaponsCategory.Name = "Gangs";
            weaponsCategory.Icon = "weapons";
            weaponsCategory.Items = new List<GpsItem>();
            
            weaponsCategory.Items.Add(new GpsItem("The Families", Fractions.Manager.FractionSpawns[1].X, Fractions.Manager.FractionSpawns[1].Y));
            weaponsCategory.Items.Add(new GpsItem("The Ballas Gang", Fractions.Manager.FractionSpawns[2].X, Fractions.Manager.FractionSpawns[2].Y));
            weaponsCategory.Items.Add(new GpsItem("Los Santos Vagos", Fractions.Manager.FractionSpawns[3].X, Fractions.Manager.FractionSpawns[3].Y));
            weaponsCategory.Items.Add(new GpsItem("Marabunta Grande", Fractions.Manager.FractionSpawns[4].X, Fractions.Manager.FractionSpawns[4].Y));
            weaponsCategory.Items.Add(new GpsItem("Blood Street", Fractions.Manager.FractionSpawns[5].X, Fractions.Manager.FractionSpawns[5].Y));
            
            gpsList.Add(weaponsCategory);
            
            //
            
            var mafiaCategory = new GpsCategory();
            mafiaCategory.Name = "Mafia";
            mafiaCategory.Icon = "mafia";
            mafiaCategory.Items = new List<GpsItem>();
            
            mafiaCategory.Items.Add(new GpsItem("La Cosa Nostra", Fractions.Manager.FractionSpawns[10].X, Fractions.Manager.FractionSpawns[10].Y));
            mafiaCategory.Items.Add(new GpsItem("Russian Mafia", Fractions.Manager.FractionSpawns[11].X, Fractions.Manager.FractionSpawns[11].Y));
            mafiaCategory.Items.Add(new GpsItem("Yakuza", Fractions.Manager.FractionSpawns[12].X, Fractions.Manager.FractionSpawns[12].Y));
            mafiaCategory.Items.Add(new GpsItem("Armenian Mafia", Fractions.Manager.FractionSpawns[13].X, Fractions.Manager.FractionSpawns[13].Y));
            
            gpsList.Add(mafiaCategory);
            
            //
            
            var licensesCategory = new GpsCategory();
            licensesCategory.Name = "Jobs";
            licensesCategory.Icon = "licenses";
            licensesCategory.Items = new List<GpsItem>();

            licensesCategory.Items.Add(new GpsItem("Arbeitsamt", 418.70874f, -624.9391f));
            licensesCategory.Items.Add(new GpsItem("Kraftwerk", 724.9625f, 133.9959f));
            licensesCategory.Items.Add(new GpsItem("Postamt", 133.0764f, 96.67652f));
            licensesCategory.Items.Add(new GpsItem("Rasenmäher", -1330.482f, 42.12986f));

            gpsList.Add(licensesCategory);
            
            //
            
            var jobsCategory = new GpsCategory();
            jobsCategory.Name = "Nebenjob";
            jobsCategory.Icon = "jobs";
            jobsCategory.Items = new List<GpsItem>();

            jobsCategory.Items.Add(new GpsItem("Mine", 128.004f, -394.953f));
            //jobsCategory.Items.Add(new GpsItem("Государственная шахта", -596, 2089));
            jobsCategory.Items.Add(new GpsItem("Holzfäller", -459.95f, -1154.8129f));
            gpsList.Add(jobsCategory);
            
           

            
            //
            
            
            var clubsCategory = new GpsCategory();
            clubsCategory.Name = "Andere";
            clubsCategory.Icon = "clubs";
            clubsCategory.Items = new List<GpsItem>();
            
            clubsCategory.Items.Add(new GpsItem("Lager", -544.56104f, -203.99673f));
            clubsCategory.Items.Add(new GpsItem("Lizenz erhalten", 435.8382f, -984.11847f));
            clubsCategory.Items.Add(new GpsItem("Familie", -773.8519f, 312.5361f));
            clubsCategory.Items.Add(new GpsItem("Feuerwerk", -602.0729f, -347.30234f));
            clubsCategory.Items.Add(new GpsItem("Amphitheater", 679.7624f, 559.8024f));
            clubsCategory.Items.Add(new GpsItem("Humane Labs", 3420.554f, 3758.799f));
            clubsCategory.Items.Add(new GpsItem("Leuchtturm", 3320.067f, 5169.669f));
            clubsCategory.Items.Add(new GpsItem("Baumarkt", -827.79517f, -689.95886f));
            clubsCategory.Items.Add(new GpsItem("Markt", Inventory.Tent.Models.TentList.PositionGps[0].X, Inventory.Tent.Models.TentList.PositionGps[0].Y));
            clubsCategory.Items.Add(new GpsItem("Schwarzmarkt", Inventory.Tent.Models.TentList.PositionGps[1].X, Inventory.Tent.Models.TentList.PositionGps[1].Y));
            clubsCategory.Items.Add(new GpsItem("Kirche", -779.12885f, -709.137f));
            clubsCategory.Items.Add(new GpsItem("Haustierverkäufer", 268.109f, -641.3529f));
            clubsCategory.Items.Add(new GpsItem("Gericht", 243.40225f, -1073.7192f));
            clubsCategory.Items.Add(new GpsItem("Immobilienagentur", -710.49f, 267.8656f));
            clubsCategory.Items.Add(new GpsItem("Möbelgeschäft", Houses.FurnitureManager.FurnitureBuyPos.X, Houses.FurnitureManager.FurnitureBuyPos.Y));
            clubsCategory.Items.Add(new GpsItem("Post", 132.9969f, 96.3529f));
            clubsCategory.Items.Add(new GpsItem("Abschlepphof", Fractions.Ticket.PedPos.X, Fractions.Ticket.PedPos.Y));
            
            gpsList.Add(clubsCategory);
            
            
            //Минимизируем для отправки
            
            var minGpsList = new List<List<object>>();

            foreach (var gpsCategory in gpsList)
            {
                var minCategory = new List<object>();
                
                minCategory.Add(gpsCategory.Name);
                minCategory.Add(gpsCategory.Icon);

                var items = new List<object>();
                
                foreach (var item in gpsCategory.Items)
                {
                    var category = new List<object>();
                    
                    category.Add(item.Name);
                    
                    if (item.PosList == null)
                        category.Add(item.Pos);
                    else
                        category.Add(item.PosList);
                    
                    
                    items.Add(category);
                }
                
                minCategory.Add(items);
                
                minGpsList.Add(minCategory);
            }
            
            GpsListJson = JsonConvert.SerializeObject(minGpsList);
        }
        public static void OnPointDefault(ExtPlayer player, string name) 
        {
            var characterData = player.GetCharacterData();
            if (characterData == null) return;
			
            var memberFractionData = player.GetFractionMemberData();
            
            var position = new Vector3();

            if (name == "house")
            {
                var house = HouseManager.GetHouse(player);
                if (house != null)
                    position = house.Position;
            }

            if (name == "biz" && characterData.BizIDs.Count > 0)
            {
                var bizId = characterData.BizIDs[0];
                if (BusinessManager.BizList.ContainsKey(bizId))
                    position = BusinessManager.BizList[bizId].EnterPoint;
            }

            if (name == "frac" && memberFractionData != null)
            {
                position = Manager.FractionSpawns[memberFractionData.Id];
            }

            if (name == "org")
            {
                var organizationData = player.GetOrganizationData();
                if (organizationData != null  && organizationData.BlipId != -1) 
                    position = organizationData.BlipPosition;
                else 
                    position = new Vector3(-774.045, 311.2569, 85.70606);
            }
            
            if (position != new Vector3())
                Trigger.ClientEvent(player, "createWaypoint", position.X, position.Y);

        } 
    }
    
}