using System.Configuration;

namespace TestAPI.Repository.Providers
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
