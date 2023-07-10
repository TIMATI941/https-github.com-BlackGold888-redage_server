using System.Collections.Generic;

namespace Redage.SDK.Models
{
    public class DonatePack
    {
        /// <summary>
        /// Множетель доната
        /// </summary>
        public int[] PriceRB = new int[] 
        {
            499,
            999,
            14999,
            4999,
            7999,
            8999,
            9999,
            17999,
            19999,
            29999,
            49999
        };
        /// <summary>
        /// Цены донат пакетов
        /// </summary>
        public int[] GiveMoney = new int[]
        {
            12500,
            25000,
            60000,
            120000,
            80000,
            200000,
            275000,
            125000,
            450000,
            450000,
            500000
        };

        public List<string>[] List =
        {
            new List<string>()
            {
                "12.500$",
                "Standard Case", 
                "5 Tage VIP Diamond", 
                "10 exp (Erfahrung)", 
                "Führerscheinklasse A", 
                "Führerscheinklasse B", 
                "Axt", 
                "verbesserte Spitzhacke", 
                "Vape",
            },
            new List<string>()
            {
                "25.000$",
                "Komischer Case",
                "VIP Diamond für 10 Tage",
                "10 exp (Erfahrung)",
                "Führerscheinklasse C",
                "Führerscheinklasse B",
                "Funkgerät",
                "Bong",
                "Fahrzeug Seminole",
            },
            new List<string>()
            {
                "60.000$",
                "Gewöhnlicher Case",
                "VIP Diamond für 15 Tage",
                "15 exp (Erfahrung)",
                "Führerscheinklasse A",
                "Führerscheinklasse B, C",
                "Krankenversicherung",
                "Shisha",
                "Fahrzeug Baller",
            },
            new List<string>()
            {
                "120000$",
                "Seltener Case",
                "30 Tage Diamond VIP",
                "20 exp (Erfahrung)",
                "Krankenversicherung",
                "Hubschrauber Lizenz",
                "Flugzeug Lizenz",
                "Waffenschein",
                "professionale Spitzhacke",
            },
            new List<string>()
            {
                "{0}$",
                "Fahrzeug Komoda",
                "30 Tage Diamond VIP",
                "35 exp (Erfahrung)",
            },
            new List<string>()
            {
                "{0}$",
                "45 Tage Diamond VIP",
                "30 exp (Erfahrung)",
                "Hubschrauber und Flugzeug Lizenz",
            },
            new List<string>()
            {
                "{0}$",
                "Ellite Case",
                "30 Tage Diamond VIP",
                "15 exp (Erfahrung)",
                "Krankenversicherung", 
                "Hubschrauber Lizenz",
                "Flugzeug Lizenz",
                "Fahrzeug Schafter2",
            },
            new List<string>()
            {
                "{0}$",
                "Fahrzeug Pariah",
                "60 Tage Diamond VIP",
                "45 exp (Erfahrung)",
            },
            new List<string>()
            {
                "{0}$",
                "Legendärer Case",
                "45 Tage Diamond VIP",
                "30 exp (Erfahrung)",
                "Zufälliger 6 stellige Telefonnummer",
                "Hubschrauber und Flugzeug Lizenz",
                "Tasche GUCCI",
                "Waffen- und Sanitäterschein",
                "Fahrzeug - Carbonizzare",
            },
            new List<string>()
            {
                "{0}$",
                "Fahrzeug Neon",
                "120 Tage Diamond VIP",
                "60 exp (Erfahrung)",
                "Zufälliger 5 stellige Telefonnummer",
            },
            new List<string>()
            {
                "{0}$",
                "90 Tage Diamond VIP",
                "60 exp (Erfahrung)",
                "Zufälliger 5 stellige Telefonnummer",
                "Hubschrauber und Flugzeug Lizenz",
                "Einzigartiges Accessoire - Bart",
                "Fahrzeug - Tesla",
                "Waffenschein und Krankenversicherung",
                "Rettungssanitäter-Lizenz",
            },
        };
    }
}