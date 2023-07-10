using System;
using System.Collections.Generic;
using System.Linq;
using Database;
using GTANetworkAPI;
using NeptuneEvo.Functions;
using NeptuneEvo.Handles;
using NeptuneEvo.Organizations.FamilyZones.Models;
using NeptuneEvo.Organizations.Models;
using NeptuneEvo.Organizations.Player;
using NeptuneEvo.Players;
using NeptuneEvo.Table.Models;
using NeptuneEvo.World.War.Models;
using Newtonsoft.Json;
using Redage.SDK;

namespace NeptuneEvo.Organizations.FamilyZones
{
    public class Repository
    {
        public static int FamilyZoneMoney = 50;
        
        public static Dictionary<byte, FamilyZoneData> FamilyZones = new Dictionary<byte, FamilyZoneData>
        {
            {0, new FamilyZoneData(0, "Flughafen 1", 0, new Vector3(-1295.4832, -2691.4841, 13.94492))},
            {1, new FamilyZoneData(1, "Flughafen 2", 0, new Vector3(1700.8517, 3271.1328, 41.15212))},
            {2, new FamilyZoneData(2, "Autoservice", 0, new Vector3(492.03802, -1318.2998, 29.240974))},
            {3, new FamilyZoneData(3, "Busbahnhof", 0, new Vector3(436.60358, -624.1321, 28.70802))},
            {4, new FamilyZoneData(4, "Theater", 0, new Vector3(683.49567, 573.19855, 130.445))},
            {5, new FamilyZoneData(5, "Fahrradverleih", 0, new Vector3(-1109.7948, -1690.6531, 4.358889))},
            {6, new FamilyZoneData(6, "Bootsverleih", 0, new Vector3(-1610.045, -1132.2228, 2.1390169))},
            {7, new FamilyZoneData(7, "Offroad-Autovermietung", 0, new Vector3(-2039.9548, -130.7523, 27.585308))},
            {8, new FamilyZoneData(8, "Rasenmäher", 0, new Vector3(-1347.9476, 83.14306, 55.259502))},
            {9, new FamilyZoneData(9, "Markt", 0, new Vector3(-1716.4965, -742.76135, 10.189973))},
            {10, new FamilyZoneData(10, "Fahrzeugmarkt", 0, new Vector3(-1641.5175, -896.31067, 8.818815))},
            {11, new FamilyZoneData(11, "Staatliche Mine", 0, new Vector3(-592.01514, 2073.4902, 131.26495))},
            {12, new FamilyZoneData(12, "Mine 1", 0, new Vector3(1278.8195, 3010.0022, 43.17986))},
            {13, new FamilyZoneData(13, "Mine 2", 0, new Vector3(1877.9728, 3294, 45.00408))},
            {14, new FamilyZoneData(14, "Mine 3", 0, new Vector3(1906.1459, 3456.5725, 47.018585))},
            {15, new FamilyZoneData(15, "Mine 4", 0, new Vector3(2218.608, 3190.0356, 51.654343))},
            {16, new FamilyZoneData(16, "Rastplatz", 0, new Vector3(327.87695, 3423.2917, 36.498356))},
            {17, new FamilyZoneData(17, "Lager 1", 0, new Vector3(88.54718, 6367.858, 31.227015))},
            {18, new FamilyZoneData(18, "Lager 2", 0, new Vector3(2699.1382, 3446.9575, 55.79804))},
            {19, new FamilyZoneData(19, "Werk 1", 0, new Vector3(2766.718, 1572.8798, 30.652487))},
            {20, new FamilyZoneData(20, "Chemisches Labor", 0, new Vector3(3552.1526, 3687.4858, 33.879154))},
            {21, new FamilyZoneData(21, "Rastplatz für Kollektor", 0, new Vector3(-1468.5979, -492.9966, 32.79623))},
            {22, new FamilyZoneData(22, "Kasino", 0, new Vector3(932.60504, 3.8400362, 78.89889))},
            {23, new FamilyZoneData(23, "Waldressourcen 1", 0, new Vector3(1449.0823, -2647.9814, 44.34842))},
            {24, new FamilyZoneData(24, "Waldressourcen 2", 0, new Vector3(3371.8804, 4958.7773, 32.50877))},
            {25, new FamilyZoneData(25, "Waldressourcen 3", 0, new Vector3(-1319.7252, 4444.952, 23.111712))},
            {26, new FamilyZoneData(26, "Waldressourcen 4", 0, new Vector3(-2123.9744, 2667.676, 2.8356767))},
            {27, new FamilyZoneData(27, "Waldressourcen 5", 0, new Vector3(156.29825, 6913.223, 19.399305))},
            {28, new FamilyZoneData(28, "Arena", 0, new Vector3(-246.26744, -2019.1298, 29.92975))},
            {29, new FamilyZoneData(29, "Baumarkt", 0, new Vector3(-676.4967, 5799.8843, 17.330954))},
            {30, new FamilyZoneData(30, "Kraftwerk", 0, new Vector3(714.4781, 147.64595, 80.75447))},
            {31, new FamilyZoneData(31, "Immobilienagentur", 0, new Vector3(-717.3004, 263.70724, 84.09108))},
            {32, new FamilyZoneData(32, "Zentrum Kortz", 0, new Vector3(-2251.307, 261.29944, 174.60323))},
            {33, new FamilyZoneData(33, "Taxistand", 0, new Vector3(908.1477, -174.37341, 74.13617))},
            {34, new FamilyZoneData(34, "Verkauf von Marihuana 1", 0, new Vector3(60.558594, 3716.2566, 39.74053))},
            {35, new FamilyZoneData(35, "Verkauf von Marihuana 2", 0, new Vector3(3797.6487, 4462.025, 5.174584))},
            {36, new FamilyZoneData(36, "Schwarzmarkt", 0, new Vector3(740.91815, -924.249, 24.600727))},
            {37, new FamilyZoneData(37, "Hafen", 0, new Vector3(923.51135, -3084.5525, 5.8937206))},
            {38, new FamilyZoneData(38, "Werk 2", 0, new Vector3(496.56717, -2206.0745, 5.895211))},
            {39, new FamilyZoneData(39, "Lager 3", 0, new Vector3(208.43271, -3108.6245, 5.7901964))},
            {40, new FamilyZoneData(40, "Lager 4", 0,  new Vector3(4.406737, -2689.8086, 5.980951))},
            {41, new FamilyZoneData(41, "Lager 5", 0,  new Vector3(701.87915, -2317.9375, 26.2322))},
            {42, new FamilyZoneData(42, "Lager 6", 0, new Vector3(888.9269, -2286.9626, 28.709274))},
            {43, new FamilyZoneData(43, "Ölförderunternehmen 1", 0, new Vector3(1229.2949, -2427.5532, 44.548492))},
            {44, new FamilyZoneData(44, "Ölförderunternehmen 2", 0, new Vector3(1522.9775, -2187.4417, 77.42543))},
            {45, new FamilyZoneData(45, "Ölförderunternehmen 3", 0, new Vector3(1551.4207, -1847.17, 92.53648))},
            {46, new FamilyZoneData(46, "Ölförderunternehmen 4", 0, new Vector3(1695.3457, -1667.2291, 112.47394))},
            {47, new FamilyZoneData(47, "Strandbar", 0, new Vector3(-1516.8055, -1499.4799, 6.7985234))},
            {48, new FamilyZoneData(48, "Theater 2", 0, new Vector3(200.76822, 1165.519, 226.97711))},
            {49, new FamilyZoneData(49, "Windkraftanlage 1", 0, new Vector3(2263.0452, 1466.3461, 72.5129))},
            {50, new FamilyZoneData(50, "Windkraftanlage 2", 0, new Vector3(2138.1736, 1863.2523, 95.66101))},
            {51, new FamilyZoneData(51, "Windkraftanlage 3", 0, new Vector3(2101.839, 2309.9094, 94.259964))},
            {52, new FamilyZoneData(52, "Lager 7", 0, new Vector3(2878.1267, 4384.7876, 50.410137))},
            {53, new FamilyZoneData(53, "Farm 1", 0, new Vector3(2874.5, 4575.6504, 47.26746))},
            {54, new FamilyZoneData(54, "Farm 2", 0, new Vector3(2625.1882, 4489.501, 37.718826))},
            {55, new FamilyZoneData(55, "Farm 3", 0, new Vector3(2467.3267, 4709.1455, 34.303524))},
            {56, new FamilyZoneData(56, "Farm 4", 0, new Vector3(1948.225, 4814.4604, 43.911274))},
            {57, new FamilyZoneData(57, "Farm 5", 0, new Vector3(2408.647, 5042.2437, 45.974403))},
            {58, new FamilyZoneData(58, "Farm 6", 0, new Vector3(400.07214, 6438.4546, 31.265696))},
            {59, new FamilyZoneData(59, "Farm 7", 0, new Vector3(341.55212, 6638.297, 28.699541))},
            {60, new FamilyZoneData(60, "Farm 8", 0, new Vector3(-93.28161, 1916.7731, 196.75548))},
            {61, new FamilyZoneData(61, "Bahnhof", 0, new Vector3(30.403343, 6232.942, 31.787899))},
            {62, new FamilyZoneData(62, "Rennwagenvermietung", 0, new Vector3(-83.88101, -2113.844, 16.704899))},
        };


        private static string[] TypeBattle = new[]
        {
            "Meiste Kills",
            "Meiste Punkte",
            "Gebietsbesitz",
            "Bis zum letzten Überlebenden"
        };

        private static string[] Composition = new[]
        {
            "Keine Grenzen",
            "1x1",
            "2x2",
            "3x3",
            "4x4",
            "5x5",
            "6x6",
            "7x7",
            "8x8",
            "9x9",
            "10x10"
        };

        private static string[] WeaponsCategory = new[]
        {
            "Keine Grenzen",
            "Nahkampf",
            "Pistolen",
            "Schrotflinten",
            "Maschinengewehre",
            "Sturmgewehre",
            "Scharfschützengewehre"
        };
        
        private static float Range = 150f;
        
        public static void OnResource()
        {
            
            using var db = new ServerBD("MainDB");//В отдельном потоке

            var familyZones = db.Familyzones.ToList();

            foreach (var familyZone in familyZones)
            {
                var id = (byte) familyZone.Id;
                if (!FamilyZones.ContainsKey(id))
                    continue;

                FamilyZones[id].OrganizationId = familyZone.Orgid;
            }
            
            foreach (var familyZone in FamilyZones.Values.ToList())
            {
                CustomColShape.CreateSphereColShape(familyZone.Position, Range, NAPI.GlobalDimension, ColShapeEnums.FamilyZone, familyZone.Id);
            }
        }

        [Interaction(ColShapeEnums.FamilyZone, In: true)]
        public void InFamilyZone(ExtPlayer player, int index)
        {
            if (!player.IsOrganizationAccess(RankToAccess.IsWar, false))
                return;
            
            World.War.Repository.InWarZone(player, WarType.OrgWarZone, index);
        }

        [Interaction(ColShapeEnums.FamilyZone, Out: true)]
        public void OutFamilyZone(ExtPlayer player, int index)
            => World.War.Repository.OutWarZone(player, WarType.OrgWarZone, index);
        
        public static void Open(ExtPlayer player)
        {
            try
            {
                if (!player.IsOrganizationAccess(RankToAccess.FamilyZone))
                    return;
                
                var memberOrganizationData = player.GetOrganizationMemberData();
                if (memberOrganizationData == null) 
                    return;

                var zones = new List<List<object>>();
                var zonesTop = new Dictionary<int, int>();

                foreach (var familyZone in FamilyZones.Values.ToList())
                {
                    var organizationData = Manager.GetOrganizationData(familyZone.OrganizationId);
                    var zone = new List<object>();

                    zone.Add(familyZone.Id);
                    
                    if (organizationData == null)
                    {
                        zone.Add("-");
                        zone.Add(null);
                    }
                    else
                    {
                        zone.Add(organizationData.Name);
                        zone.Add(new [] {organizationData.Color.Red, organizationData.Color.Green, organizationData.Color.Blue});
                        
                        if (!zonesTop.ContainsKey(organizationData.Id))
                            zonesTop[organizationData.Id] = 1;
                        else 
                            zonesTop[organizationData.Id]++;
                    }
                    
                    var war = World.War.Repository.Wars.Values
                        .FirstOrDefault(w =>
                            w.Type == WarType.OrgWarZone && w.ObjectId == familyZone.Id);
                    
                    if (war != null)
                    {
                        zone.Add(((byte) war.Status) + 1);//isWar

                        var attackingOrganizationData = Manager.GetOrganizationData(war.AttackingId);
                        if (attackingOrganizationData != null)
                            zone.Add(new [] {attackingOrganizationData.Color.Red, attackingOrganizationData.Color.Green, attackingOrganizationData.Color.Blue});
                        else 
                            zone.Add(null);
                            
                        var protectingOrganizationData = Manager.GetOrganizationData(war.ProtectingId);
                        if (protectingOrganizationData != null)
                            zone.Add(new [] {protectingOrganizationData.Color.Red, protectingOrganizationData.Color.Green, protectingOrganizationData.Color.Blue});
                        else 
                            zone.Add(null);
                    }
                    else
                    {
                        zone.Add(null);
                        zone.Add(null);
                        zone.Add(null);
                    }
                    
                    zones.Add(zone);
                }
                
                var tops = zonesTop
                    .OrderByDescending(z => z.Value)
                    .Select(z => new {Id = z.Key, Count = z.Value})
                    .ToList();
                
               

                var i = 0;
                var topNames = new List<string>();

                foreach (var top in tops)
                {
                    var organizationData = Manager.GetOrganizationData(top.Id);

                    if (organizationData != null)
                        topNames.Add($"{i + 1}. {organizationData.Name} ({top.Count} gebiet)" + (top.Id == memberOrganizationData.Id? " (du)" : ""));
                    else
                        topNames.Add($"{i + 1}. -");
                    i++;
                }
                topNames = topNames.Take(4).ToList();

                var wars = World.War.Repository.Wars.Values
                    .Where(w => w.Type == WarType.OrgWarZone)
                    .Where(w => w.AttackingId == memberOrganizationData.Id || w.ProtectingId == memberOrganizationData.Id)
                    .ToList();

                var warsJson = new List<List<object>>();
                
                foreach (var war in wars)
                {
                    if (!FamilyZones.ContainsKey((byte) war.ObjectId))
                        continue;
                    
                    var familyZone = FamilyZones[(byte) war.ObjectId];
                        
                    var warJson = new List<object>();

                    var text = "";

                    
                    var compositionName = war.Composition == -1 ? Composition[0] : Composition[war.Composition];
                    
                    if (war.AttackingId == memberOrganizationData.Id)
                    {
                        var organizationName = "-";
                        var organizationData = Manager.GetOrganizationData(war.ProtectingId);
                        if (organizationData != null)
                            organizationName = organizationData.Name;

                        text =
                            $"Du greifst {familyZone.Name} der Familie {organizationName} um {war.Time.ToString("dd.MM HH:mm")} ({TypeBattle[(int) war.GripType]}, {compositionName}, {WeaponsCategory [war.WeaponsCategory]})";
                        
                        warJson.Add(true);
                    }
                    else
                    {
                        var organizationName = "-";
                        var organizationData = Manager.GetOrganizationData(war.AttackingId);
                        if (organizationData != null)
                            organizationName = organizationData.Name;

                        text =
                            $"Du verteitigst {familyZone.Name} von {organizationName} um {war.Time.ToString("dd.MM HH:mm")} ({TypeBattle[(int) war.GripType]}, {compositionName}, {WeaponsCategory [war.WeaponsCategory]})";
                        
                        warJson.Add(false);
                        
                    }
                    
                    warJson.Add(text);
                    warJson.Add(war.GripType); 
                    warJson.Add(war.WeaponsCategory);
                    warJson.Add(war.Time);
                    
                    warsJson.Add(warJson);
                }
                
                Trigger.ClientEvent(player, "client.familyZones", JsonConvert.SerializeObject(zones), JsonConvert.SerializeObject(topNames), JsonConvert.SerializeObject(warsJson));
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }

        public static void Update(byte id, int organizationId)
        {
            try
            {
                if (!FamilyZones.ContainsKey(id))
                    return;

                var familyZone = FamilyZones[id];
                if (familyZone.OrganizationId == organizationId)
                    return;

                familyZone.OrganizationId = organizationId;
                familyZone.Save();
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }
        private static byte MinMinutes = 15;
        private static byte MaxProtection = 1;
        private static byte MaxAttack = 1;
        public static bool IsAttack(ExtPlayer player, DateTime time, int addDay, ushort attackingId, ushort protectingId)
        {
            try
            {
                if (player == null)
                    return false;
                
                var warsCount = World.War.Repository.Wars.Values
                    .Where(w => w.Type == WarType.OrgWarZone)
                    .Where(w =>  (w.AttackingId == attackingId || w.ProtectingId == attackingId) || (w.AttackingId == protectingId || w.ProtectingId == protectingId))
                    .Count(w => World.War.Repository.GetTime(w.Time, time) <= MinMinutes);
                
                if (warsCount > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Du kannst gerade keinen Krieg erklären", 8000);
                    //Trigger.ClientEvent(player, "client.closeWar");
                    return false;
                }

                var sec = NeptuneEvo.Table.Tasks.Repository.GetTime(addDay: addDay);
                OrganizationData organizationDataAttack = null;
                if (attackingId > 0)
                {
                    organizationDataAttack = Organizations.Manager.GetOrganizationData(attackingId);
                    if (organizationDataAttack == null)
                        return false;
                    
                    if (organizationDataAttack.AttackingCount.ContainsKey(sec) && organizationDataAttack.AttackingCount[sec] >= MaxAttack)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter,
                            $"Fraktion {organizationDataAttack.Name} kann heute nicht mehr Angreifen", 8000);
                        Trigger.ClientEvent(player, "client.closeWar");
                        return false;
                    }
                }

                OrganizationData organizationDataProtecting = null;
                if (protectingId > 0)
                {
                    organizationDataProtecting = Organizations.Manager.GetOrganizationData(protectingId);
                    if (organizationDataProtecting == null)
                        return false;
                    
                    if (organizationDataProtecting.ProtectingCount.ContainsKey(sec) && organizationDataProtecting.ProtectingCount[sec] >= MaxProtection)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter,
                            $"Fraktion {organizationDataProtecting.Name} kann heute nicht mehr verteitigen", 8000);
                        Trigger.ClientEvent(player, "client.closeWar");
                        return false;
                    }

                }

                if (organizationDataAttack != null)
                {
                    if (!organizationDataAttack.AttackingCount.ContainsKey(sec))
                        organizationDataAttack.AttackingCount[sec] = 0;
                    
                    organizationDataAttack.AttackingCount[sec]++; 
                }

                if (organizationDataProtecting != null)
                {
                    if (!organizationDataProtecting.ProtectingCount.ContainsKey(sec))
                        organizationDataProtecting.ProtectingCount[sec] = 0;
                    
                    organizationDataProtecting.ProtectingCount[sec]++;
                }
                
                return true;

            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }

            return false;
        }
        
        
        public static void Attack(ExtPlayer player, byte id)
        {
            try
            {
                var sessionData = player.GetSessionData(); 
                if (sessionData == null) 
                    return;
                
                if (!player.IsOrganizationAccess(RankToAccess.FamilyZone))
                    return;
                
                var memberOrganizationData = player.GetOrganizationMemberData();
                if (memberOrganizationData == null) 
                    return;
                
                var organizationData = Organizations.Manager.GetOrganizationData(memberOrganizationData.Id);
                if (organizationData == null) 
                    return;
                
                if (!FamilyZones.ContainsKey(id))
                    return;

                var familyZone = FamilyZones[id];

                if (familyZone.OrganizationId == memberOrganizationData.Id)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Dieses Gebiet gehört bereits euch", 3000);
                    return;
                }

                var war = World.War.Repository.Wars.Values
                    .FirstOrDefault(w => w.Type == WarType.OrgWarZone && w.ObjectId == id);
                
                if (war!= null && war.AttackingId > 0 && war.ProtectingId > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Dieses Gebiet wird gerade erbobert", 3000);
                    Trigger.ClientEvent(player, "client.closeWar");
                    return;
                }
                
                if (familyZone.OrganizationId == 0)
                {
                    if (war == null)
                        World.War.Repository.Open(player, id, familyZone.Id, familyZone.Name, familyZone.Position, 150f, WarType.OrgWarZone, 0, (ushort) memberOrganizationData.Id);
                    else
                    {
                        if (war.ProtectingId == memberOrganizationData.Id)
                        {
                            return;
                        }
                        var warData = sessionData.WarData;

                        warData.ObjectId = id;

                        var compositionName = war.Composition == -1 ? Composition[0] : Composition[war.Composition];
                        Trigger.ClientEvent(player, "openDialog", "FamilyZones", $"Sind Sie sicher, dass das Gebiet {familyZone.Name} um {war.Time.ToString("dd.MM HH:mm")} angreifen möchten?.<br/>Kriegstyp: <b>{TypeBattle[(int) war.GripType]}</b><br/>Anzahl: <b>{compositionName}</b><br/>Waffen: <b>{WeaponsCategory [war.WeaponsCategory]}</b>");
                    }
                    return;
                }
                
                World.War.Repository.Open(player, id, familyZone.Id, familyZone.Name, familyZone.Position, Range, WarType.OrgWarZone, (ushort) memberOrganizationData.Id, (ushort) familyZone.OrganizationId);
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }

        public static void ConfirmAttack(ExtPlayer player)
        {
            try
            {
                var sessionData = player.GetSessionData(); 
                if (sessionData == null) 
                    return;
                
                if (!player.IsOrganizationAccess(RankToAccess.FamilyZone))
                    return;
                
                var memberOrganizationData = player.GetOrganizationMemberData();
                if (memberOrganizationData == null) 
                    return;
                
                var warData = sessionData.WarData;
                var id = (byte) warData.ObjectId;
                
                if (!FamilyZones.ContainsKey(id))
                    return;

                var familyZone = FamilyZones[id];

                if (familyZone.OrganizationId == memberOrganizationData.Id)
                {
                    return;
                }

                var war = World.War.Repository.Wars.Values
                    .FirstOrDefault(w => w.Type == WarType.OrgWarZone && w.ObjectId == id);
                
                if (war == null) 
                    return;
                
                if (war.AttackingId > 0 && war.ProtectingId > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Dieses Gebiet wird gerade Umkämpft", 3000);
                    Trigger.ClientEvent(player, "client.closeWar");
                    return;
                }
                
                if (familyZone.OrganizationId == 0)
                {
                    war.AttackingId = (ushort) memberOrganizationData.Id;
                    war.Update();
                    
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Krieg angekündigt", 3000);
                    return;
                }
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }
    }
}