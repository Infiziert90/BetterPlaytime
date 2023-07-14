namespace BetterPlaytime.Data;

public class Playtime
{
    public string Playername;
    public TimeSpan PTime;
    public DateTime LastUpdate;

    public Playtime(string playername, TimeSpan playtime)
    {
        Playername = playername;
        PTime = playtime;
        LastUpdate = DateTime.Now;
    }
}