namespace Server.Domain.Games.TheHat
{
    public class Word
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public HatPlayer Author { get; }
        public int GuessTries { get; private set; }
        public string Value { get; } 
        
        // ReSharper disable once UnusedMember.Global
        public bool Guessed { get; set; }
        public Word(string value, HatPlayer author)
        {
            Value = value;
            Author = author;
        }

        public int AddGuessTry() => ++GuessTries;
    }
}