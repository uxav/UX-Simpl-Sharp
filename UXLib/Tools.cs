using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Diagnostics;

namespace UXLib
{
    public static class Tools
    {
        public static void PrintLibInfo(string projectName, Assembly projectAssembly)
        {
            Version version = projectAssembly.GetName().Version;
            Version libVersion = Assembly.LoadFrom(string.Format("{0}\\UXLib.dll", InitialParametersClass.ProgramDirectory)).GetName().Version;
            string versionInfo = string.Format("{0} v{1}.{2:D2}.{3:D2} ({4})", projectAssembly.GetName().Name, version.Major, version.Minor, version.Build, version.Revision);
            string libVersionInfo = string.Format("UXSimplSharp Library v{0}.{1:D2}.{2:D2} ({3})", libVersion.Major, libVersion.Minor, libVersion.Build, libVersion.Revision);

            CrestronConsole.PrintLine("");
            CrestronConsole.PrintLine(@"_________________________________________________________");
            CrestronConsole.PrintLine(@"                                                         ");
            CrestronConsole.PrintLine(@"  _   ___  __  ____  _       _ _        _                ");
            CrestronConsole.PrintLine(@" | | | \ \/ / |  _ \(_) __ _(_) |_ __ _| |               ");
            CrestronConsole.PrintLine(@" | | | |\  /  | | | | |/ _` | | __/ _` | |               ");
            CrestronConsole.PrintLine(@" | |_| |/  \ _| |_| | | (_| | | || (_| | |               ");
            CrestronConsole.PrintLine(@"  \___//_/\_(_)____/|_|\__, |_|\__\__,_|_|               ");
            CrestronConsole.PrintLine(@"                       |___/                             ");
            CrestronConsole.PrintLine(@"                                                         ");
            CrestronConsole.PrintLine(@"    UX Digital Systems Limited                           ");
            CrestronConsole.PrintLine(@"    www.ux.digital                                       ");
            CrestronConsole.PrintLine(@"_________________________________________________________");
            CrestronConsole.PrintLine(@"");
            CrestronConsole.PrintLine("Project: {0}", projectName);
            CrestronConsole.PrintLine(versionInfo);
            CrestronConsole.PrintLine(libVersionInfo);
            CrestronConsole.PrintLine(@"_________________________________________________________");
        }

        public static void PrintBytes(byte[] bytes, int length)
        {
            PrintBytes(bytes, length, false);
        }

        public static void PrintBytes(byte[] bytes, int length, bool showReadable)
        {
            for (int i = 0; i < length; i++)
            {
                CrestronConsole.Print(@"\x");
                if (showReadable && bytes[i] >= 32 && bytes[i] < 127)
                {
                    CrestronConsole.Print("{0}", (char)bytes[i]);
                }
                else
                {
                    CrestronConsole.Print(bytes[i].ToString("X2"));
                }
            }
            CrestronConsole.PrintLine("");
        }

        public static double ScaleRange(double Value,
          double FromMinValue, double FromMaxValue,
          double ToMinValue, double ToMaxValue)
        {
            try
            {
                return (Value - FromMinValue) *
                    (ToMaxValue - ToMinValue) /
                    (FromMaxValue - FromMinValue) + ToMinValue;
            }
            catch
            {
                return double.NaN;
            }
        }
    }
}