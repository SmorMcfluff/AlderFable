using UnityEngine;

public class Ladder : MonoBehaviour
{
    private BoxCollider2D box;
    private Bounds bounds;

    public float Top => bounds.max.y;
    public float Bottom => bounds.min.y;
    public Vector2 Center => bounds.center;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        bounds = box.bounds;
    }
}