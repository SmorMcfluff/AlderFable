using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public FacingDirection facingDirection = FacingDirection.Right;

    public Collider2D physicsCollider;

    [Header("Movement")]
    public float movementSpeed = 2.5f;
    public float climbingSpeed = 2.5f;
    public float jumpForce = 5f;
    public float decelerationRate = 10f;
    public float jumpDelay = 0.5f;
    public float knockbackStunDuration = 0.25f;
    public bool canClimbLadders = false;

    [Header("Ladder Jump")]
    public float ladderJumpHorizontalForce = 5f;
    public float ladderJumpVerticalFactor = 0.2f;
    public float ladderGrabDelay = 0.5f;

    [Header("Ground Check")]
    public Platform currentPlatform;
    public float groundCheckWidth = 1;
    public float groundCheckHeight = 0.1f;
    public float groundCheckOffset = 0.8f;
    public LayerMask groundMask;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Attack attack;

    private Ladder currentLadder;


    private bool isOnLadder = false;
    public bool IsOnLadder => isOnLadder;

    private bool readyToClimb = true;
    private bool readyToJump = true;

    private bool isStunned = false;

    private float defaultDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        attack = GetComponent<Attack>();

        defaultDirection = (facingDirection == FacingDirection.Right) ? 1f : -1f;
    }

    public void Move(Vector2 inputAxis)
    {
        if (!attack.ReadyToAttack) inputAxis = Vector2.zero;

        if (isStunned) return;

        float horizontalInput = (inputAxis.x == 0f) ? 0f : Mathf.Sign(inputAxis.x);
        float vertical = (inputAxis.y == 0f) ? 0f : Mathf.Sign(inputAxis.y);

        if (!isOnLadder)
        {
            if (ShouldFlip(horizontalInput))
            {
                FlipCharacter();
            }

            float currentHorizontalVel = rb.linearVelocity.x;
            float targetHorizontalVel = horizontalInput * movementSpeed;

            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(targetHorizontalVel, rb.linearVelocity.y);
            }
            else
            {
                float maxDelta = decelerationRate * Time.deltaTime;
                float newHorizontalVel = Mathf.MoveTowards(currentHorizontalVel, targetHorizontalVel, maxDelta);

                rb.linearVelocity = new Vector2(newHorizontalVel, rb.linearVelocity.y);
            }

            if (vertical != 0)
            {
                TriggerVertical(vertical);
            }
        }
        else if (canClimbLadders && currentLadder != null)
        {
            rb.linearVelocity = new Vector2(0f, vertical * climbingSpeed);
            CheckLadderPosition(vertical);
        }
    }

    private bool ShouldFlip(float direction)
    {
        if (direction == 0f) return false;
        return direction == defaultDirection && sr.flipX || direction != defaultDirection && !sr.flipX;
    }

    public void FlipCharacter()
    {
        sr.flipX = !sr.flipX;
        facingDirection = (facingDirection == FacingDirection.Right)
            ? FacingDirection.Left
            : FacingDirection.Right;
    }

    public bool IsGrounded()
    {
        Vector2 boxSize = new(groundCheckWidth, groundCheckHeight);

        bool isGrounded = Physics2D.BoxCast(transform.position, boxSize, 0, Vector2.down, groundCheckOffset, groundMask);
        return isGrounded;
    }

    public Bounds GetPlatformBounds()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckHeight, groundMask).collider.bounds;
    }

    private void TriggerVertical(float direction)
    {
        if (!canClimbLadders) return;
        if (currentLadder != null && readyToClimb)
        {
            bool bottomEntrance = rb.position.y < currentLadder.Top && direction > 0;
            bool topEntrance = rb.position.y > currentLadder.Top && direction < 0;

            if (bottomEntrance || topEntrance)
            {
                SetLadder(true, 0, false, topEntrance);
                return;
            }
        }
    }

    private void CheckLadderPosition(float direction)
    {
        if (!canClimbLadders) return;
        bool bottomExit = rb.position.y < currentLadder.Bottom && direction < 0;
        bool topExit = rb.position.y > currentLadder.Top && direction > 0;

        if (bottomExit || topExit)
        {
            SetLadder(false, 0, false);
        }
    }

    private void SetLadder(bool ladderGrabbed, float input = 0, bool jumpedOff = true, bool topEntrance = false)
    {
        if (!canClimbLadders && ladderGrabbed) return;
        if (ladderGrabbed && !readyToClimb) return;
        isOnLadder = ladderGrabbed;
        rb.bodyType = ladderGrabbed ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

        rb.linearVelocity = Vector2.zero;

        if (ladderGrabbed)
        {
            float targetY = topEntrance ? currentLadder.Top : rb.position.y;

            Vector2 grabbedPos = new(currentLadder.Center.x, targetY);
            rb.position = grabbedPos;
        }
        else
        {
            if (jumpedOff)
            {
                Vector2 ladderJumpForce = new(input * ladderJumpHorizontalForce, ladderJumpVerticalFactor);
                rb.AddForce(ladderJumpForce, ForceMode2D.Impulse);
            }
            StartCoroutine(JumpCooldown());
            StartCoroutine(LadderGrabCooldown());
        }
    }

    public void Jump(Vector2 input)
    {
        if (!readyToJump) return;
        Debug.Log(isOnLadder + ", " + IsGrounded());
        if (!isOnLadder && IsGrounded())
        {
            if (input != Vector2.down)
            {
                rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
            }
            else if (currentPlatform.GetComponent<PlatformEffector2D>())
            {
                StartCoroutine(JumpThroughPlatform());
            }
        }

        if (isOnLadder && input.x != 0)
        {
            SetLadder(false, input.x);
        }

        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown()
    {
        readyToJump = false;
        yield return new WaitForSeconds(jumpDelay);
        readyToJump = true;
    }

    private IEnumerator LadderGrabCooldown()
    {
        readyToClimb = false;
        yield return new WaitForSeconds(ladderGrabDelay);
        readyToClimb = true;
    }

    public void Knockback(float direction, float knockbackForce)
    {
        if (isOnLadder)
        {
            SetLadder(false, direction, true);
            Stun();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction * knockbackForce, 0), ForceMode2D.Impulse);
            Stun();
        }
    }

    public void Stun()
    {
        if (isStunned) return;
        StartCoroutine(StunTimer());
    }


    private IEnumerator StunTimer()
    {
        isStunned = true;
        yield return new WaitForSeconds(knockbackStunDuration);
        isStunned = false;
    }

    private IEnumerator JumpThroughPlatform()
    {
        physicsCollider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        SetCurrentPlatform(null);
        physicsCollider.enabled = true;
    }

    public Bounds GetColliderBounds()
    {
        return GetComponent<Collider2D>().bounds;
    }

    public void SetCurrentPlatform(Platform newPlatform)
    {
        currentPlatform = newPlatform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder") && other.TryGetComponent<Ladder>(out var ladder))
        {
            currentLadder = ladder;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            currentLadder = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = collision.collider.GetComponent<Platform>();
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + Vector3.down * groundCheckOffset, new(groundCheckWidth, groundCheckHeight));
    }
#endif
}
