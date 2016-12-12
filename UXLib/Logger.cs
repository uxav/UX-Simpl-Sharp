﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace UXLib
{
    public class Logger
    {
        public Logger(string logPath, string name)
        {
            if (Directory.Exists(logPath))
            {
                LogPath = logPath;
                Name = name;
            }
            else
                ErrorLog.Error("Error in Logger.ctor(), Directory path {0} does not exist", logPath);
        }

        public string Name { get; protected set; }
        public string LogPath { get; protected set; }

        private StreamWriter GetLog()
        {
            DirectoryInfo dir = new DirectoryInfo(this.LogPath);

            if (dir.GetFiles("*" + this.Name + "*.log").Length > 0)
            {
                FileInfo file = dir.GetFiles("*" + this.Name + "*.log")
                 .OrderByDescending(f => f.LastWriteTime)
                 .First();

                int maxSize = 1024 * 1024;

                if (file.Length < maxSize)
                    return file.AppendText();
            }

            return new StreamWriter(CreateNewFile());
        }

        private FileStream CreateNewFile()
        {
            return File.Create(string.Format("{0}\\{1}-{2}.log", this.LogPath, this.Name, DateTime.Now.ToString("yyyy-M-dd HH-mm-ss")));
        }

        private void Write(string logText)
        {
            using (StreamWriter sw = GetLog())
            {
                sw.WriteLine("{0}{1}  {2}", CrestronEnvironment.NewLine, DateTime.Now.ToString("dd-M-yyyy HH-mm-ss"), logText);
            }
        }

        public void Log(string message)
        {
            this.Write(message);
        }

        public void Log(string format, params object[] args)
        {
            this.Write(string.Format(format, args));
        }
    }
}