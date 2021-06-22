namespace Server.Domain.Games.TheHat
{
    public class Word
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public HatPlayer Author { get; }
        // ReSharper disable once MemberCanBePrivate.Global
        public int GuessTries { get; private set; }
        public string Value { get; }

        // ReSharper disable once UnusedMember.Global
        public bool Guessed { get; set; }

        public Word(string value, HatPlayer author)
        {
            Value = value;
            Author = author;
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public int AddGuessTry() => ++GuessTries;
    }
}