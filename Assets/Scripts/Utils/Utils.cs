using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Utils { // It can be called RandomUtils :D
    public static bool RandomBool() => Random.value >= 0.5f;
    // Check if list non empty
    public static T RandomFromList<T>(List<T> list) => list[Random.Range(0, list.Count)];

    public static List<T> ShuffleList<T>(List<T> list) {
        var addIndexes = 0.To(list.Count - 1).ToList();
        var newList = new List<T>();
        while (addIndexes.Count > 0) {
            var ri = Random.Range(0, addIndexes.Count);
            var r = addIndexes[ri];
            newList.Add(list[r]);
            addIndexes.Remove(r);
        }
        return newList;
    }
    public static Vector2 RandomPointInRing(Vector2 origin, float minRadius, float maxRadius) {
        return Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
        // var rDir = (Random.insideUnitCircle * origin).normalized;
        // var rDist = Random.Range(minRadius, maxRadius);
        // var point = origin + rDir * rDist;
        // return point;
    }
}

public static class IntegerExtensions {
    public static IEnumerable<int> To(this int first, int last) {
        for (int i = first; i <= last; i++) yield return i;
    }
}