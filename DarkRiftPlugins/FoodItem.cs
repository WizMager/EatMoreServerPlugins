
namespace DarkRiftPlugins
{
    public class FoodItem : IEdible
    {
        public ushort Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius => 0.1f;
        public  byte ColorR { get; set; }
        public  byte ColorG { get; set; }
        public  byte ColorB { get; set; }

        public FoodItem(ushort id, float x, float y, byte colorR, byte colorG, byte colorB)
        {
            Id = id;
            X = x;
            Y = y;
            ColorR = colorR;
            ColorG = colorG;
            ColorB = colorB;
        }
    }
}