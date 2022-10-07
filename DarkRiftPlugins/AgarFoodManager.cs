using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;

namespace DarkRiftPlugins
{
    public class AgarFoodManager : Plugin
    {
        public override Version Version => new Version(1, 0, 0);
        public override bool ThreadSafe => false;
        public IEnumerable<FoodItem> Food => _foodItems;
        private readonly HashSet<FoodItem> _foodItems = new HashSet<FoodItem>();
        private const float MapWidth = 20;
        private const int FoodItemCount = 20;

        public AgarFoodManager(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            FillFoodItemsHashset(FoodItemCount);
            ClientManager.ClientConnected += ClientConnectedHandler;
        }

        private void ClientConnectedHandler(object sender, ClientConnectedEventArgs e)
        {
            using (var writer = DarkRiftWriter.Create())
            {
                foreach (var foodItem in _foodItems)
                {
                    writer.Write(foodItem.Id);
                    writer.Write(foodItem.X);
                    writer.Write(foodItem.Y);
                    writer.Write(foodItem.ColorR);
                    writer.Write(foodItem.ColorG);
                    writer.Write(foodItem.ColorB);
                }
            
                using (var message = Message.Create(Tags.FoodItemSendTag, writer))
                {
                    e.Client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        private void FillFoodItemsHashset(int foodItemCount)
        {
            var random = new Random();
            for (int i = 0; i < foodItemCount; i++)
            {
                var foodItem = new FoodItem(
                    (ushort)i,
                    (float) random.NextDouble() * MapWidth - MapWidth / 2,
                    (float) random.NextDouble() * MapWidth - MapWidth / 2,
                    (byte) random.Next(0, 200),
                    (byte) random.Next(0, 200),
                    (byte) random.Next(0, 200)
                    );
                _foodItems.Add(foodItem);
            }
        }

        public void Eat(FoodItem food)
        {
            var random = new Random();
            food.X = (float) random.NextDouble() * MapWidth - MapWidth / 2;
            food.Y = (float) random.NextDouble() * MapWidth - MapWidth / 2;
            food.ColorR = (byte) random.Next(0, 200);
            food.ColorG = (byte) random.Next(0, 200);
            food.ColorB = (byte) random.Next(0, 200);
            
            using (var writer = DarkRiftWriter.Create())
            {
                writer.Write(food.Id);
                writer.Write(food.X);
                writer.Write(food.Y);
                writer.Write(food.ColorR);
                writer.Write(food.ColorG);
                writer.Write(food.ColorB);
                using (var message = Message.Create(Tags.FoodEatTag, writer))
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