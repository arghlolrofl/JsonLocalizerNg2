using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace JsonLocalizer.Helpers {
    public class FilesystemHelper {
        public static async Task<IList<StorageFile>> FindSubLanguages(StorageFile mainLanguage) {
            StorageFolder parentFolder = await StorageFolder.GetFolderFromPathAsync((new FileInfo(mainLanguage.Path)).DirectoryName);

            List<StorageFile> subLanguages = new List<StorageFile>();

            foreach (StorageFile fileInParenFolder in (await parentFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery))) {
                if (fileInParenFolder.FolderRelativeId == mainLanguage.FolderRelativeId)
                    continue;

                subLanguages.Add(fileInParenFolder);
            }

            return subLanguages;
        }
    }
}
