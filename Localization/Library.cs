﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Localization
{
    public class Library
    {
        public static Dictionary<DataName, string> DefaultText = new Dictionary<DataName, string>();
        public static void Init()
        {
            LibraryRu.Init();
            LibraryEn.Init();
            LibraryDe.Init();
            //
            DefaultText = LibraryDe.Data();
            
            foreach (DataName fruit in Enum.GetValues(typeof(DataName)))
            {
                try
                {
                    LangFunc.GetText(LangType.De, fruit, 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1);
                }
                catch (Exception e)
                {
                    Console.WriteLine(fruit);
                }
            }
        }
    }
}