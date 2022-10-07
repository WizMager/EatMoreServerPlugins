namespace DarkRiftPlugins
{
    public class Player : IEdible
    {
        public ushort Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }

        public Player(ushort id, float x, float y, float radius, byte colorR, byte colorG, byte colorB)
        {
            Id = id;
            X = x;
            Y = y;
            Radius = radius;
            ColorR = colorR;
            ColorG = colorG;
            ColorB = colorB;
        }
    }
}