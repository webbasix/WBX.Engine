using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace WBXEngineService
{
    public partial class ServiceApp : ServiceBase
    {
        private WBXEngine.Manager.Assistant _assisetent = null;

        public ServiceApp()
        {
            InitializeComponent();
            this.ServiceName = WBX.Core.Utilities.AppConfigSettings.GetItem("ServiceName", Version.Name); //ConfigurationManager.AppSettings.Get("ServiceName");
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //--- start our components
                _assisetent = new WBXEngine.Manager.Assistant();
                _assisetent.Start();
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnStop()
        {
            try
            {
                //--- send a message to everything that we are stopping
                _assisetent.Stop();
            }
            catch (Exception ex)
            {

            }



        }
    }
}
