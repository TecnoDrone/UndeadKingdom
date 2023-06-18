using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace MagicSandbox.TileTerrain
{
    public static class EditorSettings
    {
        public static string GetPackagePath()
        {
            // Check for UPM package
            string _packagePath = Path.GetFullPath("Packages/com.magicsandbox.magictileeditor");
            if (Directory.Exists(_packagePath))
            {
                return _packagePath;
            }


            // Check for Development Environment package
            _packagePath = FindDirectory(Application.dataPath, "Magic Tile Terrain");
            if (Directory.Exists(_packagePath))
            {
                string[] _seperator = new string[] { "Assets" };
                string[] _split = _packagePath.Split(_seperator, StringSplitOptions.RemoveEmptyEntries);

                if (_split.Length > 1)
                {
                    return _seperator[0] + _split[_split.Length - 1];
                }
                return _packagePath;
            }

            Debug.LogError("Cant Find 'Magic Tile Terrain' folder in this project. Reimport the Package through the Package Manager.");
            return string.Empty;
        }

        public static string FindDirectory(string startPath, string searchFolder)
        {
            // Data structure to hold names of subfolders to be
            // examined for files.
            Stack<string> dirs = new Stack<string>(20);

            if (!System.IO.Directory.Exists(startPath))
            {
                throw new ArgumentException();
            }
            dirs.Push(startPath);

            string _folderName;
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = System.IO.Directory.GetDirectories(currentDir);
                }
                // An UnauthorizedAccessException exception will be thrown if we do not have
                // discovery permission on a folder or file. It may or may not be acceptable
                // to ignore the exception and continue enumerating the remaining files and
                // folders. It is also possible (but unlikely) that a DirectoryNotFound exception
                // will be raised. This will happen if currentDir has been deleted by
                // another application or thread after our call to Directory.Exists. The
                // choice of which exceptions to catch depends entirely on the specific task
                // you are intending to perform and also on how much you know with certainty
                // about the systems on which this code will run.
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                _folderName = new DirectoryInfo(currentDir).Name;
                //Debug.Log("Found: " + _folderName);
                if (_folderName == searchFolder)
                { return currentDir; }


                foreach (string str in subDirs)
                    dirs.Push(str);
            }

            return string.Empty;
        }
    }
}