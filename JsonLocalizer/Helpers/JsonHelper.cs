using JsonLocalizer.Contracts;
using JsonLocalizer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace JsonLocalizer.Helpers {
    public class JsonHelper {
        const string MALFORMED_ERROR = "Malformed localization file!";

        public static async Task<IList<ILocalizationKey>> ParseLanguage(StorageFile localizationFile) {
            IList<ILocalizationKey> parsedKeys = new List<ILocalizationKey>();
            bool keyValueSwitch = false;
            string fileContentBuffer = await FileIO.ReadTextAsync(localizationFile);
            string key = String.Empty;
            string value = String.Empty;


            JsonTextReader reader = new JsonTextReader(new StringReader(fileContentBuffer));
            while (reader.Read()) {
                switch (reader.TokenType) {
                    case JsonToken.PropertyName:
                        if (keyValueSwitch)
                            throw new Exception(MALFORMED_ERROR);
                        key = reader.Value.ToString();
                        keyValueSwitch = true;
                        break;
                    case JsonToken.String:
                        if (!keyValueSwitch)
                            throw new Exception(MALFORMED_ERROR);
                        value = reader.Value.ToString();
                        keyValueSwitch = false;
                        break;
                    default:
                        throw new Exception(MALFORMED_ERROR);
                }

                if (keyValueSwitch) {
                    parsedKeys.Add(new LocalizationKey() { Key = key, Value = value });
                }
            }

            return parsedKeys;
        }
    }
}
