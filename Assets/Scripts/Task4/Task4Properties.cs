
using UnityEngine;

public class Task4Properties : MonoBehaviour {
    public class Constants {
        public const int EnemiesMin = 50;
        public const int EnemiesMax = 200;
    }
    [Range(Constants.EnemiesMin, Constants.EnemiesMax)]
    public int EnemyCount = Constants.EnemiesMin;

    [Range(10f, 30f)]
    public float EnemyRadiusIn = 10f;
    [Range(30f, 50f)]
    public float EnemyRadiusOut = 30f;
}