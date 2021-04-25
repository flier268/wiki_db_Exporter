using HunterPie.Core;
using HunterPie.Logger;
using HunterPie.Plugins;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static wiki_db_Exporter.Mapping;

namespace wiki_db_Exporter
{
    public class Main : IPlugin
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Game Context { get; set; }

        public void Initialize(Game context)
        {
            Context = context;
            context.Player.OnZoneChange += Player_OnZoneChange;
            Debugger.Log($"{nameof(wiki_db_Exporter)} Loaded.");
        }

        private void Player_OnZoneChange(object source, System.EventArgs args)
        {
            var DecorationsFromStorage = Context.Player.GetDecorationsFromStorage();
            if (DecorationsFromStorage.Length == 0)
                return;
            
            HashSet<int> NotFound = new HashSet<int>();
            HashSet<int> Repeat = new HashSet<int>();

            SaveTo(Language.Chinese);
            SaveTo(Language.English);
            SaveTo(Language.Japanese);
            SaveTo(Language.Korean);
            if (NotFound.Count > 0)
                Debugger.Log($"Not found ID：{string.Join(",", NotFound)}");
            if (Repeat.Count > 0)
                Debugger.Log($"Repeat add ID：{string.Join(",", Repeat)}");

            void SaveTo(Language language)
            {
                Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
                foreach (var d in DecorationsFromStorage)
                {
                    if (d.ItemId != 0)
                    {
                        bool success = Mapping.ConvertWithID(language,d.ItemId, out string key);
                        if (!success)
                            NotFound.Add(d.ItemId);
                        else
                        {
                            if (!keyValuePairs.ContainsKey(key))
                                keyValuePairs.Add(key, d.Amount);
                            else
                                Repeat.Add(d.ItemId);
                        }
                    }
                }
                if (keyValuePairs.Count == 0)
                    return;
                using (StreamWriter streamWriter = new StreamWriter($"DataExport\\wiki-db.{language}.json", false, Encoding.UTF8))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(keyValuePairs));
                    streamWriter.Flush();
                }
                Debugger.Log($"Export to HunterPie\\DataExport\\wiki-db.{language}.json");
            }
        }

        public void Unload()
        {

        }
    }
}
