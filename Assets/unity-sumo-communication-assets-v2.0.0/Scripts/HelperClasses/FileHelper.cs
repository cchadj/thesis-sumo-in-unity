using System.IO;

namespace Tomis.Utils
{
    public static class FileHelper
    {

        /// <summary>
        /// Creates directory at <see cref="directory"/> if it does not already exist. <para/>
        /// Checks if a file with name <see cref="filename"/> exists at said directory.<para/>
        /// Returns the first available name found and a counter which is added after filename and before postfix
        /// (to give it uniqueness)
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filename"></param>
        /// <param name="postfix"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        public static string GetAvailableFileName(string directory, string filename, string postfix, out int counter)
        {
            counter = 0;
            var pf = directory + @"\" + filename;
            var fullName = pf + counter + postfix;

            Directory.CreateDirectory(directory);
        
            while (File.Exists(fullName))
            {
                fullName = pf + counter++ + postfix;
            }

            return fullName;
        }
    }
}

