using UnityEngine;

public class Ladder : MonoBehaviour
{
    private BoxCollider2D box;

    public Bounds bounds => box.bounds;

    public float Top => bounds.max.y;
    public float Bottom => bounds.min.y;
    public float Height => Top - Bottom;
    public Vector2 Center => bounds.center;

    public Platform topPlatform;
    public Platform bottomPlatform;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        AssignPlatforms();

        topPlatform?.AddLadder(this);
        bottomPlatform?.AddLadder(this);
    }

    private void AssignPlatforms()
    {
        Vector2 topCenter = new(Center.x, Top);
        Vector2 bottomCenter = new(Center.x, Bottom);
        int groundMask = LayerMask.GetMask("Ground");

        Collider2D topHit = Physics2D.OverlapCircle(topCenter, 1f, groundMask);
        if (topHit != null)
        {
            topPlatform = topHit.GetComponent<Platform>();
        }

        Collider2D bottomHit = Physics2D.OverlapCircle(bottomCenter, 0.5f, groundMask);
        if (bottomHit != null && bottomHit.TryGetComponent(out Platform foundBottomPlatform))
        {
            bottomPlatform = foundBottomPlatform;
        }
        else
        {
            RaycastHit2D rayHit = Physics2D.Raycast(bottomCenter, Vector2.down, 3f, groundMask);
            if (rayHit.collider != null)
            {
                bottomPlatform = rayHit.collider.GetComponent<Platform>();
            }
        }
    }
}
