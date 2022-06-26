using UnityEngine;

public class BoardProperties : MonoBehaviour {
    [Range(3, 15)] 
    public int Width = 5;
    
    [Range(3, 15)] 
    public int Height = 5;

    [Range(3, 7)] 
    public int JewelsGenCount = 3;
}