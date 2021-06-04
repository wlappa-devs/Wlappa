namespace Server.Games.TheHat
{
    public class Word
    {
        public HatPlayer Author { get; }
        public int GuessTries { get; private set; }
        public string Value { get; } 
        
        public bool Guessed { get; set; }
        public Word(string value, HatPlayer author)
        {
            Value = value;
            Author = author;
        }

        public int AddGuessTry() => ++GuessTries;
    }
}