﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using ST = System.Threading;

namespace ConsoleApplication1
{
    [Serializable]
    public class AppSettings
    {
        private const int recentlyUsedCount = 10;
        private List<string> recentlyUsed = new List<string>(recentlyUsedCount);

        private Point _tspMenu;
        private Point _tspFile;
        private Point _tspPlayer;
        private Point _tspSearch;
        private int _splitterDistance;
        private ST.Timer timer;
        private static bool ignoreChanges = false;

        public Rectangle WindowPosition { get; set; }
        public FormWindowState WindowState { get; set; }

        public string Form2Placement { get; set; }

        public AppSettings()
        {
            timer = new ST.Timer(SaveSettings, null, ST.Timeout.Infinite, ST.Timeout.Infinite);
        }
        
        public Point tspFile
        {
            get
            {
                return _tspFile;
            }
            set
            {
                _tspFile = value;
                Changed();
            }
        }
        public Point tspSearch
        {
            get
            {
                return _tspSearch;
            }
            set
            {
                _tspSearch = value;
                Changed();
            }
        }

        public Point tspPlayer
        {
            get
            {
                return _tspPlayer;
            }
            set
            {
                _tspPlayer = value;
                Changed();
            }
        }
        public Point tspMenu
        {
            get
            {
                return _tspMenu;
            }
            set
            {
                _tspMenu = value;
                Changed();
            }
        }

        public int SplitterDistance
        {
            get
            {
                return _splitterDistance;
            }
            set
            {
                _splitterDistance = value;
                Changed();
            }
        }

        public string GetDefaultDirectory()
        {
            if (recentlyUsed.Count > 0)
                return Path.GetDirectoryName(recentlyUsed[0]);
            else
                return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".\\..\\.."));
        }

        public List<string> RecentlyUsed => recentlyUsed;

        public int[] ViewPort = { 0, -20, 140, 60, 0, 0 };

        public int GetViewPort(int ndx) => ViewPort[ndx];

        public void SetViewPort(int ndx, int value)
        {
            if (ViewPort[ndx] != value)
            {
                ViewPort[ndx] = value;
                Changed();
            }
        }

        void Changed()
        {
            if (!ignoreChanges)
                timer.Change(2000, ST.Timeout.Infinite);
        }

        void SaveSettings(object sender)
        {
            SaveAppSettings();
        }

        public void SetMostRecentlyUsed(string fullPath)
        {
            if ((recentlyUsed.Count > 0) && (recentlyUsed[0] == fullPath)) return;

            recentlyUsed.RemoveAll(n => n == fullPath);
            recentlyUsed.Insert(0, fullPath);

            while (recentlyUsed.Count >= recentlyUsedCount)
                recentlyUsed.RemoveAt(recentlyUsedCount - 1);

            Changed();
        }

        public void RemoveMostRecentlyUsed(string fullPath)
        {
            recentlyUsed.RemoveAll(n => n == fullPath);

            Changed();
        }

        public bool BypassUSBControllers { get; set; }

        public static AppSettings LoadAppSettings()
        {
            AppSettings settings = null;

            try
            {
                // Create an XmlSerializer for the ApplicationSettings type.
                var mySerializer = new XmlSerializer(typeof(AppSettings));
                FileInfo fi = new FileInfo(Application.LocalUserAppDataPath + @"\myApplication.config");

                // If the config file exists, open it.
                if (fi.Exists)
                {
                    ignoreChanges = true;
                    using (var myFileStream = fi.OpenRead())
                    {
                        // Create a new instance of the ApplicationSettings by
                        // deserializing the config file.
                        settings = (AppSettings)mySerializer.Deserialize(myFileStream);

                        // Assign the property values to this instance of the ApplicationSettings class.
                        myFileStream.Close();
                    }
                    ignoreChanges = false;
                }
            }
            catch (Exception)
            {
            }

            return settings;
        }

        public void SaveAppSettings()
        {
            timer.Change(ST.Timeout.Infinite, ST.Timeout.Infinite);

            if (true) //changed)
            {
                try
                {
                    // Create an XmlSerializer for the ApplicationSettings type.
                    XmlSerializer mySerializer = new XmlSerializer(typeof(AppSettings));
                    using (var myWriter = new StreamWriter(Application.LocalUserAppDataPath + @"\myApplication.config", false))
                    {

                        // Serialize this instance of the ApplicationSettings class to the config file.
                        mySerializer.Serialize(myWriter, this);
                        myWriter.Close();
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
