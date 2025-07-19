using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Sprite idleSprite;
    public Sprite walkSprite;
    public Sprite[] attackSprites;

    public float walkTimer = 0;
    public float walkSpriteDuration = 0.25f;

    SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite newSprite, bool resetTimer = false)
    {
        if (!IsCurrentSprite(newSprite))
        {
            sr.sprite = newSprite;
        }

        if (resetTimer && walkTimer > 0)
        {
            walkTimer = 0;
        }
    }

    public bool IsCurrentSprite(Sprite spriteToCheck)
    {
        return sr.sprite == spriteToCheck;
    }

    public void WalkAnimation()
    {
        walkTimer += Time.deltaTime;
        if (walkTimer > walkSpriteDuration)
        {
            sr.sprite = IsCurrentSprite(walkSprite)
                ? idleSprite
                : walkSprite;
            walkTimer %= walkSpriteDuration;
        }
    }
}
