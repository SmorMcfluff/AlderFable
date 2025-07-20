using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    private Collider2D col;
    public List<Ladder> ladders;

    public Bounds bounds => col != null ? col.bounds : new Bounds(transform.position, Vector3.zero);

    public float Top => bounds.max.y;
    public float Bottom => bounds.min.y;
    public Vector2 Center => bounds.center;

    public bool isOneWay;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        isOneWay = GetComponent<PlatformEffector2D>();
    }

    public void AddLadder(Ladder ladder)
    {
        if (!ladders.Contains(ladder))
        {
            ladders.Add(ladder);
        }
    }
}
