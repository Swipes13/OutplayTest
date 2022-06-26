using UnityEngine;

public class Task1Properties : MonoBehaviour {
    public class Constants {
        public const int CarsMin = 100;
        public const int CarsMax = 10000;
    }
    [Range(Constants.CarsMin, Constants.CarsMax)]
    public int CarCount = Constants.CarsMin;

    [Range(0f, 1f)]
    public float CollisionPercent = 0.05f;

    [Range(0f, 1f)]
    public float StartAlivePercent = 0.95f;
    [Range(0f, 1f)]
    public float StartCollidablePercent = 0.95f;

}