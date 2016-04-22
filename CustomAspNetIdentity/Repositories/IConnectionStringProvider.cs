using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDoc.Repositories.Providers
{
    public interface IAppSettingsProvider
    {
        string Get(string key);
    }

    public interface IConnectionStringProvider
    {
        string DefaultConnection { get; }
    }


    public class ConnectionStringProvider : IConnectionStringProvider, IAppSettingsProvider
    {
        public string DefaultConnection
        {
            get { return GetConnectionString("DefaultConnection"); }
        }

        public string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private string GetConnectionString(string key)
        {
            return ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }
    }
}
