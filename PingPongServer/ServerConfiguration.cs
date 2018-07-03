using System;
using System.IO;
using NetworkLibrary;
using NetworkLibrary.Utility;
using Newtonsoft.Json.Linq;


namespace PingPongServer
{
    public class ServerConfiguration
    {
        public int MaximumNumberOfIncomingConnections;
        public int ServerPort;
        private readonly string ConfigurationFile = "server_config.json";
        private LogWriterConsole Logger { get; set; } = new LogWriterConsole();
        
        

        public ServerConfiguration()
        {
            ApplyDefaultConfiguration();
            ReadConfigurationFile();
        }

        private void ApplyDefaultConfiguration()
        {
            ServerPort = NetworkConstants.SERVER_PORT;
            MaximumNumberOfIncomingConnections = 1000;
        }

        public void ReadConfigurationFile()
        {
            string serverConfig = "";
            try
            {
                using (StreamReader serverConfigReadStream = new StreamReader(File.Open(ConfigurationFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)))
                {
                    serverConfig = serverConfigReadStream.ReadToEnd();
                    Logger.ConfigurationLog("Configuration file " + ConfigurationFile + " was read.");
                }
                JObject parsedServerConfiguration = JObject.Parse(serverConfig);
                Logger.ConfigurationLog("Configuration file " + ConfigurationFile + " was parsed by Json Parser.");
                try
                {
                    MaximumNumberOfIncomingConnections = (int)parsedServerConfiguration["maximumNumberOfIncomingConnections"];
                    Logger.ConfigurationLog("Applied all configurations from configuration file successfully");
                }

                catch (Exception exception)
                {
                    Logger.ConfigurationLog("Couldn't read maximumNumberOfConnections from configuration file ");
                    Logger.ConfigurationLog("Details : " + exception.Message);
                }

            }
            catch (FileNotFoundException)
            {
                Logger.ConfigurationLog("No configuration file for server found using default configuration");
            }
            catch (Newtonsoft.Json.JsonReaderException exception)
            {
                Logger.ConfigurationLog("Server configuration file is invalid");
                Logger.ConfigurationLog("Additional Information : " + exception.Message);
                Logger.ConfigurationLog("[Warning] Default Configuraiton will be used instead !\n");
            }

        }
    }
}
