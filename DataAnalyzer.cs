using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Map.Spawners;
using Il2CppAssets.Scripts.Simulation.SMath;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace InvisiblePath;

public static class DataAnalyzer
{
    private static readonly Random Random = new();
    
    private static string? _pathData;
    private static string PathData => _pathData ??= new StreamReader(Main.Assembly.GetManifestResourceStream(Main.Assembly.GetManifestResourceNames()
        .First(str => str.EndsWith("paths.txt")))!).ReadToEnd();
    
    private static string? _areaData;
    private static string AreaData => _areaData ??= new StreamReader(Main.Assembly.GetManifestResourceStream(Main.Assembly.GetManifestResourceNames()
        .First(str => str.EndsWith("areas.txt")))!).ReadToEnd();

    public static Il2CppReferenceArray<PathModel> GetPaths()
    {
        var pathsData = PathData.Split('\n');
        
        var numOfPaths = string.Join("", pathsData).Split('n').Length - 1;

        var paths = new PathModel[numOfPaths];
        for (var i = 0; i < numOfPaths; i++)
        {
            paths[i] = (new PathModel("track" + i, null, true, false, new Vector3(), new Vector3(), null, null));
        }
        
        var entry = new AlternateBloonsSplitterModel("AlternateRoundMultiPathSplitterModel_", new Il2CppStringArray(numOfPaths));
        
        for (var i = 0; i < numOfPaths; i++)
        {
            entry.paths[i] = paths[i].name;
        }

        var points = new List<PointInfo>();
        var pathindex = 0;
        foreach (var line in pathsData)
        {
            switch (line)
            {
                case "":
                    continue;
                case "next":
                    paths[pathindex].points = (Il2CppReferenceArray<PointInfo>) points.ToArray();
                    pathindex++;
                    points = new List<PointInfo>();
                    continue;
                default:
                {
                    var coords = line.Split(',');
                    points.Add(new PointInfo {bloonScale = 1, bloonsInvulnerable = false, distance = 0, id = $"{Random.NextDouble()}", moabScale = 1, moabsInvulnerable = false, rotation = 0, point = new Vector3(float.Parse(coords[0], CultureInfo.InvariantCulture), float.Parse(coords[1], CultureInfo.InvariantCulture)), bloonSpeedMultiplier = 1});
                    break;
                }
            }
            
        }

        return paths;
    }

    public static Il2CppReferenceArray<AreaModel> GetAreas()
    {
        var areasData = AreaData.Split('\n');

        List<AreaModel> newareas = new();

        var lineIndex = 0;
        foreach (var line in areasData)
        {
            if (line == "") continue;
            if (line.Contains(','))
            {
                //add the coords
                var coords = line.Split(',');
                var stuffToAdd = new Vector2(float.Parse(coords[0], CultureInfo.InvariantCulture), (float.Parse(coords[1], CultureInfo.InvariantCulture)));

                var oldpoints = newareas[^1].polygon.points;
                Il2CppStructArray<Vector2> newpoints = new(oldpoints.Count + 1);
                for (var i = 0; i < oldpoints.Count; i++) newpoints[i] = oldpoints[i];

                newpoints[oldpoints.Count] = stuffToAdd;
                newareas[^1].polygon.points = newpoints;
            }
            else
            {
                if (lineIndex != areasData.Length - 1 && areasData[lineIndex + 1].Contains(','))
                {
                    var type = (AreaType)int.Parse(line.Split(' ')[0], CultureInfo.InvariantCulture);
                    var blocker = line.Split(' ')[1] == "True";

                    var height = blocker ? 100 : 1;
                    var area = new AreaModel("lol0", new Polygon(new Il2CppSystem.Collections.Generic.List<Vector2>()),
                        new Il2CppReferenceArray<Polygon>(0), height, type) { isBlocker = blocker };
                    newareas.Add(area);
                }
            }

            lineIndex++;
        }


        return newareas.ToArray();
    }
}