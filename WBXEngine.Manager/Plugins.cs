using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBXEngine.Manager
{

    /// <summary>
    /// Represents a collection of plugins
    /// </summary>
    internal class Plugins //: IList<Plugin
    {
        private System.Collections.Hashtable _hashTable = null;
        public Plugins() 
        {
            _hashTable = new System.Collections.Hashtable();
        }

        public void Activate()
        {            
            string path = Application.Directory.Plugins;        
            string[] files = System.IO.Directory.GetFiles(path);
            if (files.Length == 0)
            {                
                //--- if we don't have any in the plugin's directory try the local directory
                path = Application.Directory.Assembly;                
                files = System.IO.Directory.GetFiles(path);
            }

            //--- make sure we have at least one file to deal with
            if (files.Length == 0) return;



            foreach (string file in files)
            {
                //--- get the namespaces in this dll that support the plugin
                string[] namespaces = PluginNameSpaces(file);
                foreach (string nameSpace in namespaces)
                {
                    if (nameSpace.Trim().Length > 0)
                    {
                        //--- get the plug in and load it
                        Plugin plugin = null;
                        string key = nameSpace; //WBX.Core.Utilities.File.FileOps.GetFileName(file);
                        //--- don't load the manager or any debug files
                        if (file.ToLower() != "WBXEngine.Manager.dll".ToLower() && !file.EndsWith(".pdb"))
                        {
                            if (IsInCollection(key))
                            {
                                //--- get the plugin in memory
                                plugin = (Plugin)_hashTable[key];
                            }
                            else
                            {
                                //--- load the plugin
                                plugin = new Plugin(file, nameSpace);
                            }

                            //--- make sure we have something to deal with
                            if (plugin != null && plugin.IsLoaded)
                            {
                                if (!plugin.IsActive)
                                {
                                    //--- see if it the plugin is on a different delay
                                    if (plugin.ReadyToRun && plugin.IsOnline)
                                    {

                                        //--- ok' we're in so update the last logged time
                                        plugin.LastRun = DateTime.Now;
                                        //--- start the plug in (not sure if I should add it to the collection first)
                                        plugin.Activate();
                                        //--- add it to the collection
                                        Add(key, plugin);
                                    }
                                }
                            }
                            else
                            {
                                //--- it's no longer active so we can remove it
                                Remove(key);
                            }
                        }
                    }
                }
            }
            
        }

        public string[] PluginNameSpaces(string file)
        {
            string names = String.Empty;
            
            if (System.IO.File.Exists(file))
            {
                try
                {
                    System.Reflection.Assembly pluginAssembly = System.Reflection.Assembly.LoadFrom(file);

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
                                    if (names.Trim().Length > 0) names += ",";
                                    names += pluginAssembly.GetType(pluginType.ToString()).ToString();
                                    
                                }
                                typeInterface = null;
                            }

                        }

                    }



                }
                catch { }
            }
            
            return names.Split(',');
        }

        public void Deactivate()
        {
            //--- go through each plugin and close it down
            System.Collections.IDictionaryEnumerator dicEnum = _hashTable.GetEnumerator();

            string[] keys = new string[_hashTable.Keys.Count];
            int i=0;
            
            while (dicEnum.MoveNext())
            {
                //Remove(dicEnum.Key.ToString());
                keys[i] = dicEnum.Key.ToString();
                i++;
            }

            foreach (string key in keys)
            {
                Remove(key);
            }
        }

        public string[] Keys
        {
            get
            {
                System.Collections.IDictionaryEnumerator dicEnum = _hashTable.GetEnumerator();
                string[] keys = new string[_hashTable.Keys.Count];
                int i = 0;

                while (dicEnum.MoveNext())
                {
                    //Remove(dicEnum.Key.ToString());
                    keys[i] = dicEnum.Key.ToString();
                    i++;
                }

                return keys;
            }
        }

        public void Add(string key, Plugin plugin)
        {
            if (!IsInCollection(key))
            {
                _hashTable.Add(key, plugin);
            }
        }

        public bool IsInCollection(string key)
        {
            System.Collections.IDictionaryEnumerator dicEnum = _hashTable.GetEnumerator();


            while (dicEnum.MoveNext())
            {
                if (key.ToLower() == dicEnum.Key.ToString().ToLower()) return true;
            }

            return false;
        }

        public void Remove(string key)
        {

            //Application.Log.Event(Logging.Category.Log, Logging.Action.Deactivating, "Removing: " + key, key);
            if (IsInCollection(key))
            {

                //Application.Log.Event(Logging.Category.Log, Logging.Action.Deactivating, "Shutting Down: " + key, key);

                Plugin plugin = (Plugin)_hashTable[key];
                
                bool closed = false;
                if (plugin != null && plugin.IsLoaded)
                {
                    //Application.Log.Event(Logging.Category.Log, Logging.Action.Deactivating, "Deactivating: " + key, key);
                    closed = plugin.Deactivate();
                    if (closed)
                    {
                        //Application.Log.Event(Logging.Category.Log, Logging.Action.Deactivating, "Deactivated (success): " + key, key);
                    }
                    else
                    {
                        //Application.Log.Event(Logging.Category.Log, Logging.Action.Deactivating, "Deactivated (failed): " + key, key);
                        //Application.Log.Event(Logging.Category.Error, Logging.Action.Deactivating, "Deactivated (failed): " + key, key);
                    }
                }
                else
                {
                    closed = true;
                }

                if(plugin != null) plugin.Dispose();
                plugin = null;


                if (closed) _hashTable.Remove(key);
            }
            else
            {
                //Application.Log.Event(Logging.Category.Log, Logging.Action.Shutdown, "Not Found: " + key, key);
            }
        }

        private Plugin item(string key)
        {
            Plugin p = null;

            if (IsInCollection(key))
            {
                p = (Plugin)_hashTable[key];
            }

            return p;
        }

        
        public Plugin this[string key]
        {
            get
            {
                return this.item(key);
            }
        }

        


        

        
    }

    
}
