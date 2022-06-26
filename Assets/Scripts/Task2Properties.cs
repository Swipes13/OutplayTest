using UnityEngine;

public class Task2Properties : MonoBehaviour {
    public class Constants {
        public const float W_min = 10f;
        public const float W_max = 15f;
        public const float PosY_min = 5f;
        public const float PosY_max = 10f;

        public const float H_min = 3f;
        public const float H_max = 8f;

        public const float G = 9.8f;
    }

    [Range(Constants.W_min, Constants.W_max)]
    public float WidthBound = Constants.W_min;

    public Vector2 startPos = new Vector2(0, Constants.PosY_min);
    public Vector2 velocity;
    public float h_finishY = Constants.H_min;

    public float G = Constants.G;

    [InspectorButton("GenerateSmallRandomClick", 200)]
    public bool generateSmallR;
    private void GenerateSmallRandomClick() => genRandom(2.5f);

    [InspectorButton("GenerateBigRandomClick", 200)]
    public bool generateBigR;
    private void GenerateBigRandomClick() => genRandom(5f);

    [InspectorButton("GenerateTwoPointsProblem", 200)]
    public bool generateTwoPointProblem;
    private void GenerateTwoPointsProblem() {
        WidthBound = Constants.W_min; 
        startPos = new Vector2(WidthBound / 2f, Constants.H_min);
        h_finishY = Constants.H_min + 2f;
        velocity = new Vector2(7f, 11f);
        G = Constants.G;
    }

    private void genRandom(float multiplier) {
        WidthBound = Random.Range(Constants.W_min, Constants.W_max); 
        startPos = new Vector2(Random.Range(0, WidthBound), Random.Range(Constants.H_min, Constants.H_max));
        h_finishY = Random.Range(Constants.H_min, Constants.H_max);

        velocity = new Vector2(Random.Range(2.5f * multiplier, 4f * multiplier), Random.Range(3f * multiplier, 5f * multiplier));
        if (Utils.RandomBool()) velocity.x = -velocity.x;

        G = Constants.G;
    }
}