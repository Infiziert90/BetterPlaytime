using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CheapLoc;
using Dalamud.Logging;

namespace BetterPlaytime;

public class Localization
{
    public static readonly string[] ApplicableLangCodes = { "de", "ja", "fr" };
    
    private const string FallbackLangCode = "en";
    private readonly string locResourceDirectory = "loc";
    
    private readonly Assembly assembly;

    public Localization()
    {
        assembly = Assembly.GetCallingAssembly();
    }
    
    public void ExportLocalizable() => Loc.ExportLocalizableForAssembly(assembly);
    public void SetupWithFallbacks() => Loc.SetupWithFallbacks(assembly);
    
    public void SetupWithLangCode(string langCode)
    {
        if (langCode.ToLower() == FallbackLangCode || !ApplicableLangCodes.Contains(langCode.ToLower()))
        {
            SetupWithFallbacks();
            return;
        }

        try
        {
            Loc.Setup(ReadLocData(langCode), assembly);
        }
        catch (Exception)
        {
            PluginLog.Warning($"Could not load loc {langCode}. Setting up fallbacks.");
            SetupWithFallbacks();
        }
    }

    private string ReadLocData(string langCode)
    {
        
        
        return File.ReadAllText(Path.Combine(Plugin.PluginInterface.AssemblyLocation.DirectoryName!, locResourceDirectory, $"{langCode}.json"));
    }
}