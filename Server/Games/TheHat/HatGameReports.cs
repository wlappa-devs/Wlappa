namespace Server.Games.TheHat.GameCore
{
    public interface IHatGameReport
    {
        
    }

    public class StubHatGameReport : IHatGameReport
    {
        public string Finished => "На этом полномочия всё";
    }
}