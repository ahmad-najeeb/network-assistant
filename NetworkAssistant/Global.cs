using System;
using System.Reflection;

namespace NetworkAssistantNamespace
{
    public static class Global
    {   
        public static MainAppContext Controller = null;
        public static Settings AppSettings = null;

        public static string ChangeIDBeingProcessed;
        public static bool CurrentlyProcessingAChangeRequest = false;
        public static string AppDataDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\"
            + Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        //File & Folder names:
        public static string ConfigFilename = "config.json";
    }
}
