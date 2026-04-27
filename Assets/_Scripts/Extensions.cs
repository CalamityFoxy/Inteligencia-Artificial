using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 NoY(this Vector3 v3)
    {
        v3.y = 0;
        return v3;
    }

    public static T RouletteWheelSelection<T>(Dictionary<T, float> elemnens)
    {
        float totalchances = 0;
        foreach (var elem in elemnens.Values)
            totalchances += elem;
        
        float randomValue = Random.Range(0, totalchances);
        foreach (var elem in elemnens)
        {
            randomValue -= elem.Value;
            if(randomValue < 0)
                return elem.Key;
        }
        
        return default; 
    }
}