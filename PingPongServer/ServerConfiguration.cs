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
            ServerPort = NetworkConstants.SERVER_PORT;
        }

        private void ApplyDefaultConfiguration()
        {
            MaximumNumberOfIncomingConnections = 1000;
        }

        public void ReadConfigurationFromConfigurationFile()
        {
            string serverConfig = "";
            try
            {
                using (StreamReader serverConfigReadStream = new StreamReader(File.Open(ConfigurationFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read)))
                {
                    serverConfig = serverConfigReadStream.ReadToEnd();
                    Logger.Log("Configuration file " + ConfigurationFile + " was read.");
                }
                JObject parsedServerConfiguration = JObject.Parse(serverConfig);
                Logger.Log("Configuration file " + ConfigurationFile + " was parsed by Json Parser.");
                try
                {
                    MaximumNumberOfIncomingConnections = (int)parsedServerConfiguration["maximumNumberOfIncomingConnections"];
                    Logger.Log("Applied all configurations from configuration file successfully");
                }

                catch (Exception exception)
                {
                    Logger.Log("Couldn't read maximumNumberOfConnections from configuration file , details : ");
                    Logger.Log(exception.Message);
                }

            }
            catch (FileNotFoundException)
            {
                Logger.Log("No configuration file for server found using default configuration");
            }
            catch (Newtonsoft.Json.JsonReaderException exception)
            {
                Logger.Log("Server configuration file is invalid, Additional Information : ");
                Logger.Log(exception.Message);
                Logger.Log("[Warning] Default Configuraiton will be used instead !\n");
            }

        }
    }
}
