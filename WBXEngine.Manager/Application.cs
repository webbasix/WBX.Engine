using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WBXEngine.Manager
{
    public class Application
    {

        /// <summary>
        /// Defines the applications unique id.  Primarily used for logging purposes.  Each application should have this unique id!
        /// </summary>
        public static string Guid
        {
            get 
            {
                object[] objects = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);

                if (objects.Length > 0)
                {
                    return ((System.Runtime.InteropServices.GuidAttribute)objects[0]).Value;
                }
                else { return String.Empty; }
            }
        }

        /// <summary>
        /// Get the assembly name of this application.  Primarily used for logging purposes
        /// </summary>
        public static string Name
        {
            get
            {
                try
                {
                    AssemblyProductAttribute attribute = (AssemblyProductAttribute)AssemblyProductAttribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));
                    return attribute.Product;
                }
                catch { }

                return String.Empty;
            }
        }


        /// <summary>
        /// Gets the current assembly path of the dll
        /// </summary>
        public static string Path
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return System.IO.Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Gets the app settings.  Tries the actual key or the fully qualified key e.g. Application.Name + .KeyName
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string AppSetting(string key)
        {
            //--- try getting the value with a fully qualified name (which includes the assembly name)
            string finalkey = Application.Name + "." + key;
            string value = WBX.Core.Utilities.AppConfigSettings.GetItem(finalkey);
            //--- if nothing is found try the original key
            if (value.Trim().Length == 0) value = WBX.Core.Utilities.AppConfigSettings.GetItem(key);

            return value;
            
        } 

        /// <summary>
        /// Gets the version information of this assembly
        /// </summary>
        public class Version
        {

            /// <summary>
            /// Number with the revision
            /// </summary>
            public static string NumberLong
            {
                get
                {
                    System.Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    return Number + "." + v.Revision;

                }

            }

            /// <summary>
            /// Version Number without the Revision
            /// </summary>
            public static string Number
            {
                get
                {
                    try
                    {
                        System.Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                        return "v" + v.Major + "." + v.Minor + "." + v.Build;
                    }
                    catch { }

                    return String.Empty;

                }

            }

            
            /// <summary>
            /// Gets the Name + the version number of this assembly
            /// </summary>
            /// <returns></returns>
            public static new string ToString()
            {
                return Name + " " + Number;
            }

            
            
        }

        public class DBSettings
        {
            public class PlugingSetting
            {
                private iPlugin _plugin = null;
                private string _settingName = String.Empty;
                private string _environment = null;
                private string _defaultEntry = String.Empty;
                private bool _createEntry = false;
                public PlugingSetting(iPlugin plugin, string settingName)
                {
                    //--- get the plugin name only, (remove any version info)                   
                    _plugin = plugin;
                    _settingName = settingName;
                }

                public PlugingSetting(iPlugin plugin, string settingName, string environment, bool createEntry, string defaultEntry)
                {                    
                    _plugin = plugin;
                    _settingName = settingName;
                    _environment = environment;
                    _createEntry = createEntry;
                    _defaultEntry = defaultEntry;
                }

                private string fullSettingName
                {
                    get { return _plugin.Namespace + "." + _settingName; }
                }
                public string Value
                {
                    get
                    {
                        string value = String.Empty;

                        if (dbCheck)
                        {
                            using (WBX.Core.DataLayer.Environment.Setting setting = new WBX.Core.DataLayer.Environment.Setting(fullSettingName, _environment))
                            {
                                if (!setting.Exists && _createEntry)
                                {
                                    setting.ApplicationGuid = _plugin.ApplicationGuid;
                                    setting.ModuleGuid = _plugin.ModuleGuid;
                                    setting.Environment = (_environment == null) ? String.Empty : _environment;
                                    setting.Value = _defaultEntry;
                                    setting.Save();
                                }
                                value = setting.Value;
                            }
                            //value = WBX.Core.DataLayer.Environment.Setting.GetValue(fullSettingName);
                        }

                        return value;
                    }

                    set
                    {
                        if (dbCheck) //WBX.Core.DataLayer.Environment.Setting.SetValue(fullSettingName, value, _plugin.ApplicationGuid, _plugin.ModuleGuid);
                        {
                            using (WBX.Core.DataLayer.Environment.Setting setting = new WBX.Core.DataLayer.Environment.Setting(fullSettingName, _environment))
                            {
                                setting.ApplicationGuid = _plugin.ApplicationGuid;
                                setting.ModuleGuid = _plugin.ModuleGuid;
                                setting.Environment = (_environment == null) ? String.Empty : _environment;
                                setting.Value = value;
                                setting.Save();
                            }
                        }
                    }
                }

                private bool dbCheck
                {
                    get
                    {
                        //--- currently this just checks to see if we have a db connection string, not if we actully have 
                        //--- a good connection
                        //return (base.Provider.ConnectionString.Trim().Length > 0);
                        using (WBX.Core.DataLayer.DataService ds = new WBX.Core.DataLayer.DataService(String.Empty, String.Empty))
                        {
                            return ds.Provider.ConnectionString.Trim().Length > 0;
                        }
                        return false;
                    }
                }
            }
        }


        public class Log
        {

            public static bool Verbose = false;

            

            
        }

        public class Directory
        {

            static public string Assembly
            {
                get
                {
                    string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    return System.IO.Path.GetDirectoryName(path);
                }
            }

            static public string Plugins
            {
                get
                {
                    string path = WBX.Core.Utilities.File.FileOps.AppendPath(Assembly, "plugins");

                    if (!System.IO.Directory.Exists(path)) WBX.Core.Utilities.File.FileOps.MakeDirectoryPath(path);

                    return path;
                }
            }
        }

    }
}
