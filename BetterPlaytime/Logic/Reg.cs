using System.Text.RegularExpressions;
using Dalamud;

namespace BetterPlaytime.Logic;

public static class Reg
{
    private static Regex PlaytimeRegexEN = new Regex(@"^Total Play Time:(?: (?<days>\d+) days?,)?(?: (?<hours>\d+) hours?,)? (?<minutes>\d+) minutes?");
    private static Regex PlaytimeRegexDE = new Regex(@"^Gesamtspielzeit:(?: (?<days>\d+) Tage?,)?(?: (?<hours>\d+) Stunden?,)? (?<minutes>\d+) Minuten?");
    private static Regex PlaytimeRegexFR = new Regex(@"^Temps de jeu total:(?: (?<days>\d+) jours?,)?(?: (?<hours>\d+) heures?,)? (?<minutes>\d+) minutes?");
    private static Regex PlaytimeRegexJP = new Regex(@"^累積プレイ時間 (?:(?<days>\d+)日)?(?:(?<hours>\d+)時間)?(?<minutes>\d+)分");


    public static Match Match(string message, ClientLanguage language)
    {
        var reg = language switch
        {
            ClientLanguage.English => PlaytimeRegexEN,
            ClientLanguage.German => PlaytimeRegexDE,
            ClientLanguage.French => PlaytimeRegexFR,
            ClientLanguage.Japanese => PlaytimeRegexJP,
            _ => PlaytimeRegexEN
        };
        return reg.Match(message);
    }
}