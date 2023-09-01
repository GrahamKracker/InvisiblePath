using System.Collections.Generic;
using BTD_Mod_Helper.Api.Helpers;
using Il2CppAssets.Scripts.Data.MapSets;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Map.Spawners;
using Il2CppAssets.Scripts.Simulation.SimulationBehaviors;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Simulation.Track.Spawners;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.Map;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace InvisiblePath;

public partial class Main
{
    [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.LoadScene))]
    [HarmonyPostfix]
    static void MapLoader_LoadScene(MapLoader __instance)
    {
        if (__instance.currentMapName == "Invisible Path")
        {
            __instance.currentMapName = "MuddyPuddles";
            _isMap = true;
        }
        else
        {
            _isMap = false;
        }
    }

    [HarmonyPatch(typeof(UnityToSimulation), nameof( Il2CppAssets.Scripts.Unity.Bridge.UnityToSimulation.InitMap))]
    [HarmonyPrefix]
    internal static void UnityToSimulation_InitMap(UnityToSimulation __instance, ref MapModel map)
    {
        if (!_isMap)
            return;

        GameObject.Find("Seasonal").Destroy();
        GameObject.Find("Particles").Destroy();
        GameObject.Find("Trees").Destroy();

        var original = GameObject.Find("MuddyPuddlesTerrain");

        foreach (var asset in _mapItems.AllAssetNames())
        {
            Object.Instantiate(_mapItems.LoadAsset(asset).Cast<GameObject>(), original.transform.parent);
        }
        
        GameObject.Find("map(Clone)").transform.localPosition = new Vector3(-20, 0, 19);

        var leaves = GameObject.Find("BlowingLeaves");
        leaves.transform.localPosition = new Vector3(17, 0, -5);
        leaves.transform.localScale = new Vector3(35, 1, 40);
        var particles = leaves.GetComponent<ParticleSystem>();
        particles.startLifetime = 4.1f;
        particles.emissionRate = 40;
        
        original.transform.parent.FindChild("GameObject").gameObject.Destroy();
        original.Destroy();
        
        map.mapName = "Invisible Path";
        map.mapDifficulty = (int)MapDifficulty.Beginner;
        
        map.areas = DataAnalyzer.GetAreas();
        map.paths = DataAnalyzer.GetPaths();
        map.spawner = MapHelper.CreateSpawner(DataAnalyzer.GetPaths());
        var forwardSplitter = new AlternateRoundSplitterModel("AlternateRoundSplitterModel_InvisiblePath", new Il2CppStringArray(map.paths.Count), false, 1, Random.Range(0, map.paths.Count));
        
        for (var i = 0; i < map.paths.Count; i++)
        {
            forwardSplitter.paths[i] = map.paths[i].pathId;
        }
        
        map.spawner.forwardSplitter = forwardSplitter;
    }
    
    [HarmonyPatch(typeof(PreGamePrep), nameof(PreGamePrep.OnMatchReady))]
    [HarmonyPrefix]
    private static bool PreGamePrep_OnMatchReady(PreGamePrep __instance)
    {
        if (InGame.Bridge.Simulation.map.mapModel.mapName != "Invisible Path")
        {
            return true;
        }
        
        return false;
    }

    private static Dictionary<int, int> RoundPaths = new();
    [HarmonyPatch(typeof(AlternateRoundSplitter), nameof(AlternateRoundSplitter.GetCurrentActivePath))]
    [HarmonyPrefix]
    private static bool AlternateRoundSplitter_GetCurrentActivePath(AlternateRoundSplitter __instance, int forRoundIndex, ref Il2CppAssets.Scripts.Simulation.Track.Path __result)
    {
        if(__instance.SplitterModel.name != "AlternateRoundSplitterModel_InvisiblePath")
            return true;
        if(RoundPaths.TryGetValue(forRoundIndex, out var roundPath))
        {
            __result = __instance.paths[roundPath];
            return false;
        }
        var path = Random.Range(0, __instance.paths.Length);
        __result = __instance.paths[path];
        RoundPaths.Add(forRoundIndex, path);
        return false;
    }
    
    
    [HarmonyPatch(typeof(TrackArrow), nameof(TrackArrow.AttachToPath))]
    [HarmonyPatch(typeof(TrackArrow), nameof(TrackArrow.Process))]
    [HarmonyPrefix]
    private static bool TrackArrow_Create(TrackArrow __instance)
    {
        if (InGame.Bridge.Simulation.map.mapModel.mapName != "Invisible Path")
        {
            return true;
        }
        
        __instance.Destroy();
        return false;
    }
}