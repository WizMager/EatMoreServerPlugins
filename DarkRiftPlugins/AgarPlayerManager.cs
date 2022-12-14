using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;

namespace DarkRiftPlugins
{
    public class AgarPlayerManager : Plugin
    {
        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => false;
        private const float MapWidth = 20;
        private readonly Dictionary<IClient, Player> _players = new Dictionary<IClient, Player>(); 

        public AgarPlayerManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _players.Remove(e.Client);

            using (var writer = DarkRiftWriter.Create())
            {
                writer.Write(e.Client.ID);

                using (var message = Message.Create(Tags.DespawnPlayerTag, writer))
                {
                    foreach (var client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var random = new Random();
            var newPlayer = new Player(
                e.Client.ID,
                (float) random.NextDouble() * MapWidth - MapWidth / 2,
                (float) random.NextDouble() * MapWidth - MapWidth / 2,
                1f,
                (byte) random.Next(0, 200),
                (byte) random.Next(0, 200),
                (byte) random.Next(0, 200)
            );

            using (var newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(newPlayer.Id);
                newPlayerWriter.Write(newPlayer.X);
                newPlayerWriter.Write(newPlayer.Y);
                newPlayerWriter.Write(newPlayer.Radius);
                newPlayerWriter.Write(newPlayer.ColorR);
                newPlayerWriter.Write(newPlayer.ColorG);
                newPlayerWriter.Write(newPlayer.ColorB);

                using (var newPlayerMessage = Message.Create(Tags.SpawnPlayerTag, newPlayerWriter))
                {
                    foreach (var client in ClientManager.GetAllClients().Where(notCurrent => notCurrent != e.Client))
                    {
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                    }
                }
            }

            _players.Add(e.Client, newPlayer);

            using (var playerWriter = DarkRiftWriter.Create())
            {
                foreach (var player in _players.Values)
                {
                    playerWriter.Write(player.Id);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Y);
                    playerWriter.Write(player.Radius);
                    playerWriter.Write(player.ColorR);
                    playerWriter.Write(player.ColorG);
                    playerWriter.Write(player.ColorB);
                }

                using (var playerMessage = Message.Create(Tags.SpawnPlayerTag, playerWriter))
                {
                    e.Client.SendMessage(playerMessage, SendMode.Reliable);
                }
            }

            e.Client.MessageReceived += MovementMessageReceiveHandler;
        }

        private void MovementMessageReceiveHandler(object sender, MessageReceivedEventArgs e)
        {
            using (var message = e.GetMessage())
            {
                if (message.Tag == Tags.PlayerMoveTag)
                {
                    using (var reader = message.GetReader())
                    {
                        float newX = reader.ReadSingle();
                        float newY = reader.ReadSingle();

                        var currentPlayer = _players[e.Client];
                        
                        currentPlayer.X = newX;
                        currentPlayer.Y = newY;

                        var foodManager = PluginManager.GetPluginByType<AgarFoodManager>();

                        foreach (var food in foodManager.Food)
                        {
                            if (Math.Pow(currentPlayer.X - food.X, 2) + Math.Pow(currentPlayer.Y - food.Y, 2) < Math.Pow(currentPlayer.Radius, 2))
                            {
                                currentPlayer.Radius += food.Radius;
                                SendRadiusUpdate(currentPlayer);
                                foodManager.Eat(food);
                            }
                        }
                        
                        foreach (var listPlayer in _players.Values)
                        {
                            if (listPlayer != currentPlayer && Math.Pow(currentPlayer.X - listPlayer.X, 2) + Math.Pow(currentPlayer.Y - listPlayer.Y, 2) < Math.Pow(currentPlayer.Radius, 2))
                            {
                                currentPlayer.Radius += listPlayer.Radius;
                                SendRadiusUpdate(currentPlayer);
                                Kill(listPlayer);
                            }
                        }
                        
                        using (var writer = DarkRiftWriter.Create())
                        {
                            writer.Write(currentPlayer.Id);
                            writer.Write(currentPlayer.X);
                            writer.Write(currentPlayer.Y);
                            message.Serialize(writer);
                        }

                        foreach (var client in ClientManager.GetAllClients().Where(notCurrentPlayer => notCurrentPlayer != e.Client))
                        {
                            client.SendMessage(message, e.SendMode);
                        }
                    }
                }
            }
        }

        private void Kill(Player listPlayer)
        {
            using(var writer = DarkRiftWriter.Create())
            {
                writer.Write(listPlayer.Id);
                using (var message = Message.Create(Tags.KillPlayerTag, writer))
                {
                    foreach (var client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
        }

        private void SendRadiusUpdate(Player currentPlayer)
        {
            using (var writer = DarkRiftWriter.Create())
            {
                writer.Write(currentPlayer.Id);
                writer.Write(currentPlayer.Radius);
                using (var message = Message.Create(Tags.RadiusUpdateTag, writer))
                {
                    foreach (var client in ClientManager.GetAllClients())
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
        }
    }
}

