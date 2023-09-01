using System.Reflection;
using BTD_Mod_Helper.Api;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Data.MapSets;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using InvisiblePath;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(InvisiblePath.Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace InvisiblePath;

[HarmonyPatch]
public partial class Main : BloonsTD6Mod
{
    internal static MelonLogger.Instance Logger;
    private static bool _isMap;
    public new static Assembly Assembly => typeof(Main).Assembly;
    private bool _gameStart = true;
    private static AssetBundle? _mapItems;

    public override void OnInitialize()
    {
        Logger = LoggerInstance;
    }
    
    public override void OnMainMenu()
    {
        /*if (_gameStart)
        {
            InGameData.Editable.selectedMode = "Standard";
            InGameData.Editable.selectedMap = "Invisible Path";
            InGameData.Editable.selectedDifficulty = "Easy";
            UI.instance.LoadGame();
            _gameStart = false;
        }*/
    }

    public override void OnTitleScreen()
    {
        _mapItems ??= ModContent.GetBundle<Main>("mapitems");
        
        /*var button = GameObject.Find("Canvas/ScreenBoxer/TitleScreen/Start");
        button.GetComponent<Button>().onClick.Invoke();*/
        
        GameData.Instance.mapSet.Maps.items = GameData.Instance.mapSet.Maps.items.AddTo(new MapDetails
        {
            id = "Invisible Path",
            isBrowserOnly = false,
            isDebug = false,
            difficulty = MapDifficulty.Beginner,
            coopMapDivisionType = CoopDivision.DEFAULT,
            mapMusic = "MusicBTD5JazzA",
            mapSprite = ModContent.GetSpriteReference<Main>("InvisiblePath"),
            hasWater = true
        });
        Game.instance.ScheduleTask(() => Game.instance.GetBtd6Player().UnlockMap("Invisible Path"),
            () => Game.instance && Game.instance.GetBtd6Player() != null);
    }
}