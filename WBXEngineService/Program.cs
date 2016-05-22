using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.Configuration.Install;
using System.Reflection;

namespace WBXEngineService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            bool isInstalled = false;
            bool serviceStarting = false; 
            string SERVICE_NAME = WBX.Core.Utilities.AppConfigSettings.GetItem("ServiceName");

            ServiceController[] services = ServiceController.GetServices();

            foreach (ServiceController service in services)
            {
                if (service.ServiceName.Equals(SERVICE_NAME))
                {
                    isInstalled = true;
                    if (service.Status == ServiceControllerStatus.StartPending)
                    {
                        // If the status is StartPending then the service was started via the SCM             
                        serviceStarting = true;
                    }
                    break;
                }
            }
            
            if (!serviceStarting)
            {
                if (!isInstalled)
                {
                    MessageBox.Show("This application is a service and it is not installed.  Please use the installer. ", "Status",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                /*
                if (_IsInstalled == true)
                {
                    // Thanks to PIEBALDconsult's Concern V2.0
                    DialogResult dr = new DialogResult();
                    dr = MessageBox.Show("Do you REALLY like to uninstall the " + SERVICE_NAME + "?", "Danger", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr == DialogResult.Yes)
                    {
                        SelfInstaller.UninstallMe();
                        MessageBox.Show("Successfully uninstalled the " + SERVICE_NAME, "Status",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    DialogResult dr = new DialogResult();
                    dr = MessageBox.Show("Do you REALLY like to install the " + SERVICE_NAME + "?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (dr == DialogResult.Yes)
                    {
                        SelfInstaller.InstallMe();
                        MessageBox.Show("Successfully installed the " + SERVICE_NAME, "Status",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                 * */

            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new ServiceApp() 
			    };
                ServiceBase.Run(ServicesToRun);
            }

            
        }
    }


    public static class SelfInstaller
    {
        private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;
        public static bool InstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { _exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UninstallMe()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(
                    new string[] { "/u", _exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

}
