using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WBXEngine.Manager
{
    public interface iPlugin
    {
        string Version { get; }
        string Namespace { get; }
        string ApplicationName { get; }
        //bool IsLoaded { get; }
        bool IsActive { get; }
        string ApplicationGuid { get; }
        string ModuleGuid { get; }

        bool Activate();        
        bool Deactivate();
        void Dispose();

        //--- TO DO:  On a major upgrade add these
        //System.Nullable<DateTime> ActivationDate { get; }
        //--- allow each plugin to set their default interval, which can still be overriden via db settings
        //int DefaultIntervalInSeconds {get;}
    }
}
