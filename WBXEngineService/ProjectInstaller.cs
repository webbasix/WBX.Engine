using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;


namespace WBXEngineService
{
    [RunInstaller(true)]

    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();


            this.Installers.Add(GetServiceInstaller());
            this.Installers.Add(GetServiceProcessInstaller());
        }

        //[secur
        //[Security.Permissions.SecurityPermission(Security.Permissions.SecurityAction.Demand)]
        
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            //System.Diagnostics.Process.Start("http://www.microsoft.com");
        }



        private ServiceInstaller GetServiceInstaller()
        {
            ServiceInstaller installer = new ServiceInstaller();
            installer.ServiceName = GetConfigurationValue("ServiceName");

            string displayName = GetConfigurationValue("DisplayName");
            installer.DisplayName = (displayName.Trim().Length > 0) ? displayName : installer.ServiceName;
            installer.Description = "Web Basix XEngine Automation Service.  Uses WBXEngine Manager Plugins to execute custom background services.";
            installer.StartType = ServiceStartMode.Automatic;
            return installer;
        }

        private ServiceProcessInstaller GetServiceProcessInstaller()
        {
            ServiceProcessInstaller installer = new ServiceProcessInstaller();
            installer.Account = ServiceAccount.LocalSystem;            
            return installer;
        }

        private string GetConfigurationValue(string key)
        {
            Assembly service = Assembly.GetAssembly(typeof(ServiceApp));
            Configuration config = ConfigurationManager.OpenExeConfiguration(service.Location);
            if (config.AppSettings.Settings[key] != null)
            {
                return config.AppSettings.Settings[key].Value;
            }
            else
            {
                return Version.Name;
                //throw new IndexOutOfRangeException("Settings collection does not contain the requested key:" + key);
            }
        }

        

    }
}
