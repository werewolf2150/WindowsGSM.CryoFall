﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.IO;
using System.Linq;
using System.Net;



namespace WindowsGSM.Plugins
{
    public class CryoFall : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.CryoFall", // WindowsGSM.XXXX
            author = "Geekbee",
            description = "WindowsGSM plugin for supporting CryoFall Dedicated Server",
            version = "1.0",
            url = "https://github.com/werewolf2150/WindowsGSM.CryoFall", // Github repository link (Best practice)
            color = "#34c9ec" // Color Hex
        };

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "1061710"; // Game server appId

        // - Standard Constructor and properties
        public CryoFall(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;


        // - Game server Fixed variables
        public override string StartPath => @"Launch.bat"; // Game server start path
        public string FullName = "CryoFall Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 10; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()


        // - Game server default values
        public string Port = "6000"; // Default port
        public string QueryPort = "6100"; // Default query port
        public string Defaultmap = "map"; // Default map name
        public string Maxplayers = "100"; // Default maxplayers
        public string Additional = ""; // Additional server start parameter


        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            //No config file seems
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
			
            string shipExePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(shipExePath))
            {
                Error = $"{Path.GetFileName(shipExePath)} not found ({shipExePath})";
                return null;
            }			
			
		

            // Prepare start parameter

			string param = $"-batchmode {_serverData.ServerParam}" + (!AllowsEmbedConsole ? " -log" : string.Empty);	
	


            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = shipExePath,
                    Arguments = param,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;

                // Start Process
                try
                {
                    p.Start();
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null; // return null if fail to start
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                return p;
            }

            // Start Process
            try
            {
                p.Start();
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }
	
	   // - Stop server function
	   public async Task Stop(Process p) => await Task.Run(() => { p.Kill(); }); // It s not solution, first solution
    }
}
