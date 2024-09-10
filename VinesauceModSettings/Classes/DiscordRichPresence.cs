using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using static NetDiscordRpc.DiscordRPC;
using VinesauceModSettings;
using NetDiscordRpc;
using NetDiscordRpc.RPC;

namespace Dolphin.ShadowTheHedgehog.RPC
{
    public class VinesauceRpc : IDisposable
    {
        private System.Threading.Timer _timer;
        private NetDiscordRpc.DiscordRPC _rpcClient;

        private bool _suspended = false;

        /* Initialization / Teardown */
        public VinesauceRpc(Process process)
        {
            _rpcClient = new DiscordRPC("1283189895719813232");
            _rpcClient.Initialize();
            _timer = new System.Threading.Timer(Tick, null, new TimeSpan(0), new TimeSpan(0, 0, 0, 5));
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _rpcClient?.Dispose();
        }

        public void Suspend()
        {
            _suspended = true;
        }

        public void Resume()
        {
            _suspended = false;
        }

        private void Tick(object state)
        {
            if (!_suspended)
            {
                var richPresence = new RichPresence();

                // Details
                    richPresence.Details = "Scooting the Burbs";

                // Game State
                richPresence.State = "Playing";

                // Set Timestamp
                var timestamps = new Timestamps();
                var assets = new Assets();

                var stageStartTime = DateTime.UtcNow;
                timestamps.Start = stageStartTime;
                richPresence.Timestamps = timestamps;
                assets.LargeImageKey = "";
                richPresence.Assets = null;

                _rpcClient.SetPresence(richPresence);
            }
        }
    }
}