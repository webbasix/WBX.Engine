using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WBXEngineService
{
    public class Version
    {
        public static string Number
        {
            get
            {
                try
                {
                    System.Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                    return "v" + v.Major + "." + v.Minor + "." + v.Build + "." + v.Revision;
                }
                catch { }

                return String.Empty;

            }
            
        }

        public static string Name
        {
            get
            {
                try
                {
                    AssemblyProductAttribute productAttribute = (AssemblyProductAttribute)AssemblyProductAttribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute));
                    return productAttribute.Product;
                }
                catch { }

                return String.Empty;
            }
        }

        public new static string ToString()
        {
            return Name + " " + Number;
        }

        static public string AssemblyDirectory 
        {   
            get 
            { 
                string codeBase = Assembly.GetExecutingAssembly().CodeBase; 
                UriBuilder uri = new UriBuilder(codeBase); 
                string path = Uri.UnescapeDataString(uri.Path); 
                return System.IO.Path.GetDirectoryName(path); 
            } 
        }

    }
}
