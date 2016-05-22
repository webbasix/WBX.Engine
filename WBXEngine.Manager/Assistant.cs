using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace WBXEngine.Manager
{
    public class Assistant
    {
        private bool _isRunning = false;
        private bool _shuttingDown = false;
        private Plugins _plugins = null;
        private Thread _mainThread = null;


        public Assistant() { }

        public bool Start() {

            try
            {
                _isRunning = true;
                //Logging.LogIt("Assistant thread starting.");

                if (_mainThread == null || _mainThread.ThreadState == ThreadState.Aborted) _mainThread = new Thread(Run);
                _mainThread.Start();
            }
            catch (Exception ex)
            {

                string message = ex.Message;

                return false;
            }
        

            return true;
        }

        public bool Stop() {            

            return ShutDown();
        }

        private void Run()
        {

            try
            {
                while (_isRunning)
                {
                    //--- start a thread that launches the plugins

                    runPlugins();


                    //--- shut down and remove any inactive plugins
                    closeInActivePlugins();



                    //--- this will sleep for a total of 60 seconds but
                    //--- we'll break it up into 10, 6 second blocks.
                    //--- that way if we are caught in the middle of trying to shut down, it won't take so long
                    for (int i = 0; i < 10; i++)
                    {
                        if (!_isRunning) break;

                        //--- sleep for 6 seconds
                        Thread.Sleep(1000 * 6);
                    }


                }
            }catch(Exception ex)
            {
                string message = ex.Message;
            }
            

        }

        private void closeInActivePlugins()
        {
            //Logging.LogIt("\t\tClosing inactive plugins");
            int i = 0;
            if (_plugins != null)
            {
                string[] keys = _plugins.Keys;
                string inActiveKey = String.Empty;
                foreach(string key in keys)
                {
                    Plugin plugin = _plugins[key];
                    if (plugin != null)
                    {
                        //Logging.LogIt("\t\t\tChecking: " + key);
                        //Logging.LogIt("\t\t\tIs Loaded: " + plugin.IsLoaded);
                        //Logging.LogIt("\t\t\tIs Active: " + plugin.IsActive);

                        if (plugin.IsLoaded && (!plugin.IsActive || _shuttingDown))
                        {
                            //Logging.LogIt("\t\tDeactivating: " + key);
                            plugin.Deactivate();

                            inActiveKey += key + "|";
                        }
                    }
                    else
                    {
                        //Logging.LogIt("\t\t\tPlugin Object Not Available: " + key);

                        inActiveKey += key + "|";
                    }
                    
                }

                //Logging.LogIt("\t\tInactive Keys:" + inActiveKey);
                string[] inActiveKeys = inActiveKey.Split('|');
                foreach (string key in inActiveKeys)
                {
                    if (key.Trim().Length > 0)
                    {
                        i++;
                        //Logging.LogIt("\t\t\tRemoving From the Collection: " + key);
                        _plugins.Remove(key);
                    }
                }
            }



            //Logging.LogIt("\t\tExiting closing inactive plugins");
        }

        /// <summary>
        /// Check to see if we have the latest version of the plugin (e.g. are any upgrades available)
        /// </summary>
        private void runPlugins()
        {
            try
            {
                if (_isRunning)
                {
                    //Logging.LogIt("Running Plugins.");

                    string archivePath = String.Empty;
                    string pluginPath = Application.Directory.Plugins;
                    string upgradePath = WBX.Core.Utilities.File.FileOps.AppendPath(pluginPath, "upgrades");

                    if (System.IO.Directory.Exists(upgradePath))
                    {
                        string[] upgrades = System.IO.Directory.GetFiles(upgradePath);

                        //Logging.LogIt("\tLooking for upgrades");

                        //--- check to see if we have any upgrades to publish
                        foreach (string upgrade in upgrades)
                        {
                            //-- if the file exists in the plugin path move it first
                            string fileName = WBX.Core.Utilities.File.FileOps.GetFileName(upgrade);
                            string activePlugin = WBX.Core.Utilities.File.FileOps.AppendPath(pluginPath, fileName);

                            //--- this will error out if the plugin is in use.  We could do a check but I'm not going to worry about it now
                            try
                            {
                                bool canUpgrade = true;
                                if (System.IO.File.Exists(activePlugin))
                                {
                                    
                                    
                                    if (_plugins != null)
                                    {
                                        //Logging.LogIt("\t\tUpgrade found:" + activePlugin);

                                        //--- check to see if the plugin is active (in the middle of a process)
                                        if (!_plugins[fileName].IsActive)
                                        {
                                            //--- remove it, which will deactivate it
                                            _plugins.Remove(fileName);
                                        }
                                        else
                                        {
                                            canUpgrade = false;
                                        }
                                    }

                                    if (canUpgrade)
                                    {
                                        //--- the file exists, move it to the archive folder, to keep a history of it
                                        archivePath = WBX.Core.Utilities.File.FileOps.AppendPath(pluginPath, "archive\\" + WBX.Core.Utilities.DateTimeTools.DisplayTimeOnlyMilitaryTime(DateTime.Now.ToString()).Replace(":", "-") + ".bak");
                                        canUpgrade = WBX.Core.Utilities.File.FileOps.Archive(activePlugin, archivePath);
                                    }
                                }

                                //--- now move the file to the production plugin folder
                                if(canUpgrade) WBX.Core.Utilities.File.FileOps.Move(upgrade, activePlugin);
                            }
                            catch { }
                        }
                    }

                    //--- start any plugin/thread that's not running
                    //--- add any new plugins if they are not loaded
                    if (_plugins == null) _plugins = new Plugins();

                    //Logging.LogIt("Activating Plugins.");
                    _plugins.Activate();

                }

                if (_shuttingDown)
                {
                    //Logging.LogIt("Shutting down plugins.");

                    //--- shut it all down and get out
                    _plugins.Deactivate();

                    _shuttingDown = false;

                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;

                _plugins.Deactivate();

                _shuttingDown = false;
            }
        }


        /// <summary>
        /// Shutdown all objects
        /// </summary>
        /// <returns></returns>
        private bool ShutDown()
        {
            try
            {
                _isRunning = false;
                

                _mainThread.Abort();
                _mainThread = null;
                //--- shut it all down
                if (_plugins != null) _plugins.Deactivate();

                

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
