using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace KickAFK
{
    [ApiVersion(1, 14)]
    public class KickAFK : TerrariaPlugin
    {
        private Config Config = new Config();
        public static AFKPlayer[] Players = new AFKPlayer[256];
        private DateTime LastCheck = DateTime.UtcNow;
        public override string Name
        {
            get
            {
                return "KickAFK";
            }
        }
        public override string Author
        {
            get
            {
                return "Colin";
            }
        }
        public override string Description
        {
            get
            {
                return "Kicks AFK players.";
            }
        }
        public override Version Version
        {
            get
            {
                return new Version("1.1");
            }
        }
        public KickAFK(Main game)
            : base(game)
        {
            Order = 1;
        }
        public override void Initialize()
        {
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            }
            base.Dispose(disposing);
        }
        public void OnInitialize(EventArgs args)
        {
            string path = Path.Combine(TShock.SavePath, "KickAFK.json");
            if (File.Exists(path))
            {
                Config = Config.Read(path);
            }
            Config.Write(path);
        }
        public void OnGreet(GreetPlayerEventArgs args)
        {
            lock (Players)
                Players[args.Who] = new AFKPlayer(args.Who);
        }
        public void OnLeave(LeaveEventArgs args)
        {
            Players[args.Who] = null;
        }
        public void OnUpdate(EventArgs args)
        {
            if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 5)
            {
                LastCheck = DateTime.UtcNow;
                lock (Players)
                    foreach (AFKPlayer p in Players)
                        if (p != null && p.TSPlayer != null)
                        {
                            if (p.TSPlayer.TileX == p.LastX && p.TSPlayer.TileY == p.LastY)
                            {
                                p.IdleTime = p.IdleTime + 5;
                            }
                            else
                            {
                                p.IdleTime = 0;
                            }
                            p.LastX = p.TSPlayer.TileX;
                            p.LastY = p.TSPlayer.TileY;
                            if (p.IdleTime > Config.KickTime && !p.TSPlayer.Group.HasPermission("tshock.admin.nokick"))
                            {
                                TShock.Utils.Kick(p.TSPlayer, Config.KickMsg, false, false, "Server", true);
                            }
                            else if (p.IdleTime > Config.WarnTime && !p.TSPlayer.Group.HasPermission("tshock.admin.nokick"))
                            {
                                p.TSPlayer.SendErrorMessage("You will be kicked for being inactive in " + (Config.KickTime - p.IdleTime) + " seconds.");
                            }

                        }
            }
        }
        public class AFKPlayer
        {
            public int Index { get; set; }
            public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
            public int IdleTime { get; set; }
            public int LastX { get; set; }
            public int LastY { get; set; }
            public AFKPlayer(int i)
            {
                Index = i;
                IdleTime = 0;
                LastX = TShock.Players[Index].TileX;
                LastY = TShock.Players[Index].TileY;
            }
        }
    }
}
