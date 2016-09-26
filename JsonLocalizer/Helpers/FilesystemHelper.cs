using System.Collections.Generic;
using System.IO;

namespace JsonLocalizer.Helpers
{
    public class FilesystemHelper
    {
        public static IEnumerable<FileInfo> FindSubLanguages(FileInfo mainLanguageFile)
        {
            foreach (FileInfo fileInParenFolder in mainLanguageFile.Directory.GetFiles("*.json", SearchOption.AllDirectories))
            {
                if (fileInParenFolder.FullName == mainLanguageFile.FullName)
                    continue;

                yield return fileInParenFolder;
            }
        }
    }
}
