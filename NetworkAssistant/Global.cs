using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetworkAssistantNamespace
{
    public static class Global
    {   
        public static MainAppContext Controller;
        public static Settings AppSettings;

        public static string ChangeIDBeingProcessed;
        public static bool CurrentlyProcessingAChangeRequest = false;
        
        //File & Folder names:
        public const string ConfigFilename = "config.json";

        public static readonly string AppDataDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\"
            + Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        public static class LoggingVarNames
        {
            public const string ChangeId = "changeId";
            public const string InterfaceType = "interfaceType";
            public const string ChangeType = "changeType";
            public const string CallerMethodName = "callerMethodName";
            public const string SpaceName = "spaceName";
        }

        public static string GetDescriptionString(this Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        public static void Log(Logger logger, LogLevel level, string message, int callDepth,
            KeyValuePair<string, string>[] originalProperties, params KeyValuePair<string, string>[] additionalProperties)
        {
            LogEventInfo eventInfo = new LogEventInfo(level, logger.Name, message);

            foreach (KeyValuePair<string, string> pair in originalProperties)
                eventInfo.Properties[pair.Key] = pair.Value;

            foreach (KeyValuePair<string, string> pair in additionalProperties)
                eventInfo.Properties[pair.Key] = pair.Value;

            eventInfo.Properties[LoggingVarNames.SpaceName] = GetSpace(callDepth);

            logger.Log(eventInfo);
        }

        static string GetSpace(int callDepth)
        {
            if (callDepth > 0)
                return GetSpace(callDepth - 1) + "  ";
            else
                return "";
        }

        public static KeyValuePair<string, string> Pair(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
