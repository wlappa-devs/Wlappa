namespace Server.Games.TheHat.GameCore
{
    public struct HatPlayer
    {
        public string Name { get; }
        
        public int Score { get; private set; }
        public int Id { get; }

        public HatPlayer(string name, int id)
        {
            Name = name;
            Id = id;
            Score = 0;
        }

        public int IncrementScore() => ++Score;
        public int DecrementScore() => Score == 0 ? 0 : --Score;
    }
}