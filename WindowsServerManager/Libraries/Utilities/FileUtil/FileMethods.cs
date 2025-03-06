﻿using WindowsServerManager.Libraries.Utilities.Expanders;

namespace WindowsServerManager.Libraries.Utilities.FileUtil
{
    public static class FileMethods
    {
        public static string[] GetFiles(string directory, string filter = "*.*") => Directory.GetFiles(directory, filter, SearchOption.AllDirectories);

        public static string GetFolderFromFile(string file) => Path.GetFileNameWithoutExtension(file).Split('\\').Last();

        public static string GetLastDirectory(string directoryPath) => directoryPath.Split('\\').Last();

        public static string[] GetDuplicates(string directory)
        {
            List<string> fileNames = [];
            string[] files = GetFiles(directory);
            string[] duplicates = files.Where(file => !fileNames.TryAdd(Path.GetFileNameWithoutExtension(file))).ToArray();
            return duplicates;
        }

        public static long GetFileSize(string file) => new FileInfo(file).Length;

        public static DateTime GetFileCreationDate(string file) => new FileInfo(file).CreationTime;

        public static DateTime GetFileModificationDate(string file) => new FileInfo(file).LastWriteTime;

    }
}
