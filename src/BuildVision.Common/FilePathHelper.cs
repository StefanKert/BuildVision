using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BuildVision.Common
{
    public static class FilePathHelper
    {
        public static IEnumerable<string> ShortenPaths(IEnumerable<string> paths, int maxLength)
        {
            return paths.Select(x => ShortenPath(x, maxLength));
        }

        /// <summary>
        /// Shortens a file path to the specified length
        /// </summary>
        /// <param name="path">The file path to shorten</param>
        /// <param name="maxLength">The max length of the output path (including the ellipsis if inserted)</param>
        /// <returns>The path with some of the middle directory paths replaced with an ellipsis (or the entire path if it is already shorter than maxLength)</returns>
        /// <remarks>
        /// Shortens the path by removing some of the "middle directories" in the path and inserting an ellipsis. If the filename and root path (drive letter or UNC server name)     in itself exceeds the maxLength, the filename will be cut to fit.
        /// UNC-paths and relative paths are also supported.
        /// The inserted ellipsis is not a true ellipsis char, but a string of three dots.
        /// </remarks>
        /// <example>
        /// ShortenPath(@"c:\websites\myproject\www_myproj\App_Data\themegafile.txt", 50)
        /// Result: "c:\websites\myproject\...\App_Data\themegafile.txt"
        /// 
        /// ShortenPath(@"c:\websites\myproject\www_myproj\App_Data\theextremelylongfilename_morelength.txt", 30)
        /// Result: "c:\...gfilename_morelength.txt"
        /// 
        /// ShortenPath(@"\\myserver\theshare\myproject\www_myproj\App_Data\theextremelylongfilename_morelength.txt", 30)
        /// Result: "\\myserver\...e_morelength.txt"
        /// 
        /// ShortenPath(@"\\myserver\theshare\myproject\www_myproj\App_Data\themegafile.txt", 50)
        /// Result: "\\myserver\theshare\...\App_Data\themegafile.txt"
        /// 
        /// ShortenPath(@"\\192.168.1.178\theshare\myproject\www_myproj\App_Data\themegafile.txt", 50)
        /// Result: "\\192.168.1.178\theshare\...\themegafile.txt"
        /// 
        /// ShortenPath(@"\theshare\myproject\www_myproj\App_Data\", 30)
        /// Result: "\theshare\...\App_Data\"
        /// 
        /// ShortenPath(@"\theshare\myproject\www_myproj\App_Data\themegafile.txt", 35)
        /// Result: "\theshare\...\themegafile.txt"
        /// </example>
        /// <remarks>
        /// http://www.trsdomain.dk/blog/2011/1/9/shortening-file-paths---c-sharp-function.aspx
        /// </remarks>
        public static string ShortenPath(string path, int maxLength)
        {
            string ellipsisChars = "...";
            char dirSeperatorChar = Path.DirectorySeparatorChar;
            string directorySeperator = dirSeperatorChar.ToString();

            //simple guards
            if (path.Length <= maxLength)
            {
                return path;
            }
            int ellipsisLength = ellipsisChars.Length;
            if (maxLength <= ellipsisLength)
            {
                return ellipsisChars;
            }


            //alternate between taking a section from the start (firstPart) or the path and the end (lastPart)
            bool isFirstPartsTurn = true; //drive letter has first priority, so start with that and see what else there is room for

            //vars for accumulating the first and last parts of the final shortened path
            string firstPart = "";
            string lastPart = "";
            //keeping track of how many first/last parts have already been added to the shortened path
            int firstPartsUsed = 0;
            int lastPartsUsed = 0;

            string[] pathParts = path.Split(dirSeperatorChar);
            for (int i = 0; i < pathParts.Length; i++)
            {
                if (isFirstPartsTurn)
                {
                    string partToAdd = pathParts[firstPartsUsed] + directorySeperator;
                    if ((firstPart.Length + lastPart.Length + partToAdd.Length + ellipsisLength) > maxLength)
                    {
                        break;
                    }
                    firstPart = firstPart + partToAdd;
                    if (partToAdd == directorySeperator)
                    {
                        //this is most likely the first part of and UNC or relative path 
                        //do not switch to lastpart, as these are not "true" directory seperators
                        //otherwise "\\myserver\theshare\outproject\www_project\file.txt" becomes "\\...\www_project\file.txt" instead of the intended "\\myserver\...\file.txt")
                    }
                    else
                    {
                        isFirstPartsTurn = false;
                    }
                    firstPartsUsed++;
                }
                else
                {
                    int index = pathParts.Length - lastPartsUsed - 1; //-1 because of length vs. zero-based indexing
                    string partToAdd = directorySeperator + pathParts[index];
                    if ((firstPart.Length + lastPart.Length + partToAdd.Length + ellipsisLength) > maxLength)
                    {
                        break;
                    }
                    lastPart = partToAdd + lastPart;
                    if (partToAdd == directorySeperator)
                    {
                        //this is most likely the last part of a relative path (e.g. "\websites\myproject\www_myproj\App_Data\")
                        //do not proceed to processing firstPart yet
                    }
                    else
                    {
                        isFirstPartsTurn = true;
                    }
                    lastPartsUsed++;
                }
            }

            if (lastPart == "")
            {
                //the filename (and root path) in itself was longer than maxLength, shorten it
                lastPart = pathParts[pathParts.Length - 1];//"pathParts[pathParts.Length -1]" is the equivalent of "Path.GetFileName(pathToShorten)"
                lastPart = lastPart.Substring(lastPart.Length + ellipsisLength + firstPart.Length - maxLength, maxLength - ellipsisLength - firstPart.Length);
            }

            return firstPart + ellipsisChars + lastPart;
        }
    }
}
