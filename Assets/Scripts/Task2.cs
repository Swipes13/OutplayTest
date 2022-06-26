using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Task2Properties))]
public class Task2 : MonoBehaviour {
    public GameObject OW;
    public GameObject HeightLine;
    public GameObject Point;
    public GameObject FinishPoint;
    public Material FinishMaterial;
    
    [InspectorButton("TestClick", 120)]
    public bool TestOne;
    private void TestClick() => Test();

    [InspectorButton("TestManyClick", 120)]
    public bool TestMany;
    private void TestManyClick() => TestManyPoints();

    private List<GameObject> instantiated = new List<GameObject>();

    private record QuadEquation {
        public float a; 
        public float b; 
        public float c;
        private float disc;

        public QuadEquation(float a, float b, float c) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.disc = Mathf.Pow(b, 2) - 4 * a * c;
        }

        public List<float> GetRoots() {
            List<float> roots = new List<float>();
            if (disc < 0) return roots;
            
            if (disc == 0) roots.Add( -b / (2 * a) );
            else {
                roots.Add( (-b + Mathf.Sqrt(disc)) / (2 * a) );
                roots.Add( (-b - Mathf.Sqrt(disc)) / (2 * a) );
            }
            return roots;
        }
    }

    private void Start() => Test();
    
    private void Test() {
        if (!Application.isPlaying) {
            Debug.Log("Only in Play mode!");
            return;
        }
        prepareTest();

        var props = this.GetComponent<Task2Properties>();
        var finishX = 0f;
        var hitHeight = TryCalculateXPositionAtHeight(props, ref finishX);
        
        afterTestEvaluate(props, hitHeight);
        
        if (hitHeight) visualizeNewPoint(new Vector2(finishX, props.h_finishY), true);
    }
    private void TestManyPoints() {
        if (!Application.isPlaying) {
            Debug.Log("Only in Play mode!");
            return;
        }
        prepareTest();

        var props = this.GetComponent<Task2Properties>();
        var finishXs = new List<float>();
        var hitHeight = TryCalculateXPositionAtHeightWithMany(props, finishXs);
        
        afterTestEvaluate(props, hitHeight);

        if (hitHeight) finishXs.ForEach(finishX => visualizeNewPoint(new Vector2(finishX, props.h_finishY), true));
    }

    private void prepareTest() {
        instantiated.ForEach(Destroy);
        instantiated.Clear();
    }
    private void afterTestEvaluate(Task2Properties props, bool hitHeight) {
        Physics.gravity = new Vector3(0, -props.G, 0);

        Point.transform.position = new Vector3(props.startPos.x, props.startPos.y, 0f);
        Point.GetComponent<Rigidbody>().velocity = new Vector3(props.velocity.x, props.velocity.y, 0f);
        OW.transform.position = new Vector3(props.WidthBound + 0.5f + 0.125f, OW.transform.position.y, 0f);
        HeightLine.transform.position = new Vector3(HeightLine.transform.position.x, props.h_finishY, 0f);

        // TODO: Gravity Factor to physics

        if (!hitHeight) Debug.Log($"No hit the target height by ball!"); 
    }
    bool TryCalculateXPositionAtHeight(Task2Properties props, ref float xPosition) =>
        TryCalculateXPositionAtHeight(props.h_finishY, props.startPos, props.velocity, props.G, props.WidthBound, ref xPosition);

    // TARGET FUNCTION. could be static if remove visualize functionality
    bool TryCalculateXPositionAtHeight(float h, Vector2 p, Vector2 v, float G, float w, ref float xPosition) {
        var roots = new QuadEquation(G / 2f, -v.y, - p.y + h).GetRoots();
        if (roots.Count == 0) return false;
        
        // TODO: try-catch?
        var time = roots.Where(r => r >= 0).First();
        xPosition = p.x + v.x * time;

        // First test xPosition out of bounds
        if (xPosition < 0 || xPosition > w) {
            calculateReflection(xPosition, w, p, v, G, out var velocityBounded, out var newPosition);
            visualizeNewPoint(newPosition);

            if (!TryCalculateXPositionAtHeight(h, newPosition, velocityBounded, G, w, ref xPosition)) return false;
        }

        return true;
    }

    bool TryCalculateXPositionAtHeightWithMany(Task2Properties props, List<float> xPositions) =>
        TryCalculateXPositionAtHeightWithMany(props.h_finishY, props.startPos, props.velocity, props.G, props.WidthBound, xPositions);

    bool TryCalculateXPositionAtHeightWithMany(float h, Vector2 p, Vector2 v, float G, float w, List<float> xPositions) {
        var roots = new QuadEquation(G / 2f, -v.y, - p.y + h).GetRoots();
        if (roots.Count == 0) return false;
        
        var tupled = roots.Where(r => r >= 0).Select(time => (time, xPosition: p.x + v.x * time)).ToList();
        
        var result = true;
        tupled.ForEach(tuple => {
            if (tuple.xPosition < 0 || tuple.xPosition > w) {
                calculateReflection(tuple.xPosition, w, p, v, G, out var velocityBounded, out var newPosition);
                visualizeNewPoint(newPosition);

                if (!TryCalculateXPositionAtHeightWithMany(h, newPosition, velocityBounded, G, w, xPositions)) result = false;
            } else {
                xPositions.Add(tuple.xPosition);
            }
        });

        return result;
    }
    private void calculateReflection(float currentX, float w, Vector2 p, Vector2 v, float G, out Vector2 velocityBounded, out Vector2 newPosition) {
        var newX = Mathf.Clamp(currentX, 0, w);
        var newTime = (newX - p.x) / v.x;
        var newY = p.y + v.y * newTime - (G * Mathf.Pow(newTime, 2)) / 2f;
        var newV_y = v.y - G * newTime;
        velocityBounded = new Vector2(-v.x, newV_y);
        newPosition = new Vector2(newX, newY);
    }
    private void visualizeNewPoint(Vector2 newPosition, bool finishMaterial = false) {
        var newPoint = Instantiate(FinishPoint, transform);
        newPoint.SetActive(true);
        instantiated.Add(newPoint);
        if (finishMaterial) {
            newPoint.GetComponent<MeshRenderer>().material = FinishMaterial;
        }
        newPoint.transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
    }
    private void Update() {
        if (Input.GetKeyUp(KeyCode.Space)) {
            Test();
        }
    }
}
