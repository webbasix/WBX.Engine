using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace WBXEngine.Manager
{
    /// <summary>
    /// Represents a single plugin loaded into memory
    /// </summary>
    internal class Plugin
    {
        private bool _pluginIsLoaded = false;
        private iPlugin _myInstance = null;

        //public Plugin(string fileName) 
        //{
        //    Init(fileName);
        //}

        /// <summary>
        /// Load a plugin based on just the filename, we'll look a class that matches the interface
        /// </summary>
        /// <param name="fileName"></param>
        internal Plugin(string fileName) { init(fileName, String.Empty); }

        /// <summary>
        /// Load a plugin file and a specific interface.  Usefull when you have one dll that supports multiple plugin points
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="nameSpace"></param>
        internal Plugin(string fileName, string nameSpace) { init(fileName, nameSpace); }


        public void Dispose()
        {
            if (_myInstance != null)
            {
                _myInstance.Deactivate();
                _myInstance.Dispose();
            }

            _myInstance = null;
        }

        public bool Activate()
        {

            if (_myInstance != null) _myInstance.Activate();
            return true;
        }

        public bool Deactivate()
        {
            if (_myInstance != null) _myInstance.Deactivate();
            return true;
        }

        public bool IsLoaded
        {
            get { return _pluginIsLoaded; }
        }

        public iPlugin Instance
        {
            get { return _myInstance; }
        }

        public bool IsActive
        {
            get
            {
                if (_myInstance != null) return _myInstance.IsActive;
                return false;
            }
        }

        public int Interval
        {
            get
            {
                int seconds = 60;

                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "Interval");
                string settingValue = setting.Value;
                if (settingValue.Trim().Length > 0 && WBX.Core.Utilities.Numbers.IsNumeric(settingValue)) seconds = WBX.Core.Utilities.Numbers.ConvertToInt(settingValue);
                if (settingValue.Trim().Length == 0) Interval = seconds;
                setting = null;

                return seconds;
            }

            set
            {
                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "Interval");
                //--- min right now is 60 seconds
                if (value < 60) value = 60;
                setting.Value = value.ToString();
                setting = null;
            }
        }

        public System.Nullable<DateTime> LastRun
        {
            get
            {
                //--- get a default time of minus the interval
                DateTime dt = DateTime.Now.AddSeconds(-(100));
                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "LastRun");
                string settingValue = setting.Value;
                if (settingValue.Trim().Length > 0 && WBX.Core.Utilities.DateTimeTools.IsDate(settingValue)) dt = WBX.Core.Utilities.DateTimeTools.Convert(settingValue);
                setting = null;

                return dt;
            }

            set
            {
                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "LastRun");
                setting.Value = (value == null) ? String.Empty : value.Value.ToString();
                setting = null;
            }
        }

        public enum MSQState
        {
            None,
            MSQOnly,
            MSQorInterval
        }

        public MSQState MSQInteraction
        {
            get
            {
                //--- get a default time of minus the interval
                
                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "MSQState");
                string settingValue = setting.Value;
                //if (settingValue.Trim().Length > 0 && WBX.Core.Utilities.DateTimeTools.IsDate(settingValue)) dt = WBX.Core.Utilities.DateTimeTools.Convert(settingValue);
                MSQState msq = MSQState.None;

                Enum.TryParse(settingValue, out msq);
                setting = null;

                return msq;
            }

            set
            {
                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "MSQState");
                setting.Value = (value == null) ? String.Empty : value.ToString();
                setting = null;
            }
        }


        /// <summary>
        /// Determine if the Plugin is ready to run.  This is based on an interval.  For example, most plugins will run every 60 seconds.  However you can change this to a different interval e.g. 2 hours 7200 = (60 *60 *2)
        /// </summary>
        public bool ReadyToRun
        {
            get
            {
                if(this.MSQInteraction == MSQState.None || this.MSQInteraction == MSQState.MSQorInterval && LastRun == null) return true;

                if(this.MSQInteraction == MSQState.None || this.MSQInteraction == MSQState.MSQorInterval && LastRun.Value < DateTime.Now.AddSeconds(Interval)) return true;

                if (this.MSQInteraction == MSQState.MSQOnly || this.MSQInteraction == MSQState.MSQorInterval)
                {
                    return WBX.Core.BackOffice.MessageQueue.Queue.MessagesWaiting(this._myInstance.ModuleGuid);
                }

                return false;
            }
        }

        /// <summary>
        /// Determines if a plugin is online and which server it's online for
        /// </summary>
        public bool IsOnline
        {
            get
            {
                //--- get a default time of minus the interval
                
                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "IsOnline", System.Environment.MachineName, true, GetDefaultValue("IsOnline", "false"));
                return WBX.Core.Utilities.Strings.ConvertToBoolean(setting.Value);
               
            }

            set
            {
                Application.DBSettings.PlugingSetting setting = new Application.DBSettings.PlugingSetting(_myInstance, "IsOnline", System.Environment.MachineName, true, GetDefaultValue("IsOnline", "false"));
                setting.Value = value.ToString();
                //setting.en = System.Environment.MachineName;
                setting = null;
            }
        }

        private string GetDefaultValue(string key, string defaultValueNotFound)
        {
            string settingName = "WBXEngine.Setting.Default." + this._myInstance.ApplicationName + "." + key;
            string value = WBX.Core.Utilities.AppConfigSettings.GetItem(settingName);
            if (value.Trim().Length == 0) value = defaultValueNotFound;
            return value;
        }

        private void init(string fileName, string nameSpace)
        {

            _myInstance = null;
            _pluginIsLoaded = false;
            int count = 0;
            try
            {
                //--- make sure we found something
                if (fileName.Length > 0)
                {

                    //--- try load the assembly from the file
                    Assembly pluginAssembly = Assembly.LoadFrom(fileName);
                    //Logging.LogIt("\tLoading Assembly: " + fileName);
                    try
                    {
                        if (nameSpace.Trim().Length > 0)
                        {
                            Type singleType = pluginAssembly.GetType(nameSpace);
                            if (singleType != null)
                            {
                                Type typeInterface = singleType.GetInterface("WBXEngine.Manager.iPlugin", true);
                                if (typeInterface != null)
                                {
                                    //Logging.LogIt("\tAssembly Has Interface: " + fileName);

                                    _myInstance = (iPlugin)Activator.CreateInstance(pluginAssembly.GetType(singleType.ToString()));
                                    _pluginIsLoaded = true;

                                    return;
                                    //Logging.LogIt("\t\tAssembly Loaded: " + fileName);
                                }
                                typeInterface = null;
                            }

                            //--- a namespace was specified but nothing was found
                            return;
                        }

                        //Next we'll loop through all the Types found in the assembly
                        foreach (Type pluginType in pluginAssembly.GetTypes())
                        {
                            if (pluginType.IsPublic) //Only look at public types
                            {
                                if (!pluginType.IsAbstract)  //Only look at non-abstract types
                                {
                                    //Gets a type object of the interface we need the plugins to match
                                    Type typeInterface = pluginType.GetInterface("WBXEngine.Manager.iPlugin", true);
                                    if (typeInterface != null)
                                    {
                                        //Logging.LogIt("\tAssembly Has Interface: " + fileName);

                                        _myInstance = (iPlugin)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));
                                        _pluginIsLoaded = true;

                                        //Logging.LogIt("\t\tAssembly Loaded: " + fileName);
                                        return;
                                    }
                                    typeInterface = null;
                                }

                            }

                        }



                    }
                    catch (ReflectionTypeLoadException ex)
                    {

                        StringBuilder sb = new StringBuilder();
                        foreach (Exception exSub in ex.LoaderExceptions)
                        {
                            sb.AppendLine(exSub.Message);
                            if (exSub is FileNotFoundException)
                            {
                                FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                                if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                                {
                                    sb.AppendLine("Fusion Log:");
                                    sb.AppendLine(exFileNotFound.FusionLog);
                                }
                            }
                            sb.AppendLine();
                        }
                        string errorMessage = sb.ToString();
                        //Display or log the error based on your application.

                        if (ex is System.Reflection.ReflectionTypeLoadException)
                        {
                            var typeLoadException = ex as ReflectionTypeLoadException;
                            var loaderExceptions = typeLoadException.LoaderExceptions;
                        }



                        //Logging.LogIt("\tError Loading Assembly: " + fileName);
                    }
                    finally
                    {

                        pluginAssembly = null;
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            
        }

    }
}
