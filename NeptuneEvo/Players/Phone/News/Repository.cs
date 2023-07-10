﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Localization;
using NeptuneEvo.Character;
using NeptuneEvo.Fractions;
using NeptuneEvo.Handles;
using NeptuneEvo.Players.Phone.News.Models;
using Newtonsoft.Json;
using Redage.SDK;
using NeptuneEvo.Fractions.LSNews;

namespace NeptuneEvo.Players.Phone.News
{
    public class Repository
    {
        private static int PriceSymbol = 3;
        
        private static List<List<object>> NewsList = new List<List<object>>();
        private static string NewsListJson = JsonConvert.SerializeObject(NewsList);
            
        public static void Add(ExtPlayer player, string text, string link, sbyte type, bool isPremium)
        {
            var sessionData = player.GetSessionData();
            if (sessionData == null) 
                return;
            
            var characterData = player.GetCharacterData();
            if (characterData == null) 
                return;
            
            if (string.IsNullOrEmpty(text))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.VvedireCorrect), 6000);
                return;
            }
            if (DateTime.Now < sessionData.TimingsData.NextAD)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.CantGoAdNow), 6000);
                return;
            }
            if (LsNewsSystem.IsAdvertToName(player.Name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.AdAlreadyQueue), 6000);
                return;
            }
            if (characterData.Sim == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.NoSimcard), 6000);
                return;
            }
            if (text.Length < 15 || text.Length > 150)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.AdTooShort), 6000);
                return;
            }
            
            var price = text.Length * PriceSymbol;

            if (isPremium)
                price *= 2;
            
            if (!MoneySystem.Bank.Change(characterData.Bank, -price, false))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.NoBankMoney), 6000);
                return;
            }
            
            Trigger.ClientEvent(player, "client.phone.successNews");
            
            Trigger.SetTask(async () =>
            {
                await LsNewsSystem.AddAdvert(player, text, link, type, isPremium, price);
            });
            
        }

        public static void AddList(AdvertData advert, string message)
        {
            NewsList.Add(new List<object>
            {
                DateTime.Now.AddHours(1),//Делит тайм
                advert.Author,
                advert.AuthorSIM,
                message,
                advert.Link,
                advert.Type,
                advert.IsPremium,
                DateTime.Now,
            });

            NewsListJson = JsonConvert.SerializeObject(NewsList);
        }

        public static void Dell()
        {
            var newsList = NewsList
                .Where(d => DateTime.Now >= (DateTime) d[0])
                .ToList();

            if (newsList.Count > 0)
            {
                foreach (var item in newsList)
                    NewsList.Remove(item);
                
                NewsListJson = JsonConvert.SerializeObject(NewsList);   
            }
        }

        public static void LoadNews(ExtPlayer player)
        {
            if (!player.IsCharacterData())
                return;
            
            Trigger.ClientEvent(player, "client.phone.initNews", PriceSymbol, NewsListJson);
        }
    }
}