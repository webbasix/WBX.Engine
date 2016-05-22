//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace WBXEngine.Manager
//{
//    public class Logging
//    {
//        private static System.Nullable<bool> _isLogging = null;
//        private static string _path = null;
//        public static void LogIt(string message)
//        {
//            string path = String.Empty;
//            if(Path != null) path = Path;
//            if (IsLogging && path.Length > 0)
//            {
//                //WBX.Core.Utilities.Log.Writer writer = new WBX.Core.Utilities.Log.Writer(Path);
//                //writer.WriteLog(WBX.Core.Utilities.Log.Writer.LogEntryType.Comment, DateTime.Now, message);
//            }
//        }

//        public static bool IsLogging
//        {
//            get 
//            {
//                if (_isLogging == null)
//                {
//                    _isLogging = WBX.Core.Utilities.Strings.ConvertToBoolean(WBX.Core.Utilities.AppConfigSettings.GetItem("WBXEngine.Manager.Logging.IsActive"));
//                }

//                return _isLogging.Value;
//            }
//        }

//        public static string Path
//        {
//            get
//            {
//                if (_path == null)
//                {
//                    _path = WBX.Core.Utilities.AppConfigSettings.GetItem("WBXEngine.Manager.Logging.Path");
//                }

//                return _path;
//            }
//        }

        


        
//    }
//}
