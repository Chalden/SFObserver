using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FabricObserver
{
    class ConfigurationProvider
    {
        private static string GetStorageConnectionString(string name)
        {
            try
            {
                return RoleEnvironment.GetConfigurationSettingValue(name);
            }
            catch (SEHException)
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
        }

        public static string StorageConnectionString()
        {
            return GetStorageConnectionString("StorageConnectionString");
        }

        public static string DefaultConnection()
        {
            return GetStorageConnectionString("DefaultConnection");
        }
    }
}
