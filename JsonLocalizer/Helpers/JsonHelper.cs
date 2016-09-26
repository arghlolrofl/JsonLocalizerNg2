using JsonLocalizer.Contracts;
using JsonLocalizer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonLocalizer.Helpers
{
    public class JsonHelper
    {
        const string MALFORMED_ERROR = "Malformed localization file!";

        public static async Task<IList<ILocalizationKey>> ParseLanguage(FileInfo localizationFile)
        {
            IList<ILocalizationKey> parsedKeys = new List<ILocalizationKey>();
            bool keyValueSwitch = false;
            string fileContentBuffer = String.Empty;
            string key = String.Empty;

            using (var fileReader = localizationFile.OpenText())
                fileContentBuffer = await fileReader.ReadToEndAsync();

            JsonTextReader reader = new JsonTextReader(new StringReader(fileContentBuffer));
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                    case JsonToken.EndObject:
                        break;
                    case JsonToken.PropertyName:
                        if (keyValueSwitch)
                            throw new Exception(MALFORMED_ERROR);
                        key = reader.Value.ToString();
                        keyValueSwitch = true;
                        break;
                    case JsonToken.String:
                        if (!keyValueSwitch)
                            throw new Exception(MALFORMED_ERROR);
                        parsedKeys.Add(new LocalizationKey() { Key = key, Value = reader.Value.ToString() });
                        keyValueSwitch = false;
                        break;
                    default:
                        throw new Exception(MALFORMED_ERROR);
                }
            }

            return parsedKeys;
        }

        internal static void SaveLanguage(ILanguage language)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            var sortedItems = from lang in language.Items
                              orderby lang.Key ascending
                              select lang;

            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();
                foreach (var item in sortedItems)
                {
                    writer.WritePropertyName(item.Key);
                    writer.WriteValue(item.Value);
                }
                writer.WriteEndObject();
            }

            File.WriteAllText(language.Path, sb.ToString());
        }
    }
}
