using UnityEngine;

[RequireComponent(typeof(Task4Properties))]
public class Task4 : MonoBehaviour {
    public GameObject EnemiesParent;
    public GameObject EnemyPrefab;
    
    private void Start() => Test();

    private void Test() {
        var props = GetComponent<Task4Properties>();

        for (var i = 0; i < props.EnemyCount; i++) {
            CreateEnemy();
        }
    }

    private void CreateEnemy() {
        var props = GetComponent<Task4Properties>();
        var rPoint = Utils.RandomPointInRing(Vector2.zero, props.EnemyRadiusIn, props.EnemyRadiusOut);
        var enemy = Instantiate(EnemyPrefab, transform);
        enemy.transform.position = new Vector3(rPoint.x, 1f, rPoint.y);
    }
}
