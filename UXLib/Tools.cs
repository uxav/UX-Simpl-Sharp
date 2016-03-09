using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;

namespace UXLib
{
    public class Tools
    {
        public static void PrintLibInfo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            Version libVersion = Assembly.LoadFrom(string.Format("{0}\\UXLib.dll", InitialParametersClass.ProgramDirectory)).GetName().Version;
            string versionInfo = string.Format("{0} v{1}.{2:D2}.{3:D2} ({4})", assembly.GetName().Name, version.Major, version.Minor, version.Build, version.Revision);
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
            CrestronConsole.PrintLine(versionInfo);
            CrestronConsole.PrintLine(libVersionInfo);
            CrestronConsole.PrintLine(@"_________________________________________________________");
        }
    }
}