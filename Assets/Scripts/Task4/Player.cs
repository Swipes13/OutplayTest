using UnityEngine;

public class Player : MonoBehaviour {
    public GameObject PathParentNode;
    public ParticleSystem pSystem;
    
    [Range(1f, 10f)]
    public float Speed = 2f;

    private AudioSource audioSource;
    private int currentPathIndex = 0;
    private bool died = false;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        if (PathParentNode == null || PathParentNode.transform.childCount == 0) currentPathIndex = -1;
    }

    private void Update() {
        if (currentPathIndex < 0 || died) return;
        var child = PathParentNode.transform.GetChild(currentPathIndex);
        
        var posTo = child.transform.position;
        posTo.y = 1f; 
        var dir = posTo - transform.position;
        
        if (dir.magnitude <= 0.1f) {
            currentPathIndex++;
            if (currentPathIndex >= PathParentNode.transform.childCount) {
                currentPathIndex = -1;
                Die();
            }
        } else {
            var dirN = dir.normalized;
            var dist = dirN * (Speed * Time.deltaTime);
            // If some problems with dt -> could be jump after point and then back
            transform.position += dist;
        }
    }
    private void Die() {
        Debug.Log("[DIE] KEKKOOO");
        died = true;
        pSystem.Play();
        audioSource.Play();
    }
    private void OnTriggerEnter(Collider other) {
        Debug.Log($"[TRUGGER] {other.CompareTag("Enemy")}");
        if (other.CompareTag("Enemy")) {
            Die();
        }
    }
}