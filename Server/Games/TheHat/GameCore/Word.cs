namespace Server.Games.TheHat.GameCore
{
    public class Word
    {
        public int GuessTries { get; private set; }
        public string Value { get; } 
        
        public bool Guessed { get; set; }
        public Word(string value)
        {
            Value = value;
        }

        public int AddGuessTry() => ++GuessTries;
    }
}