using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public FacingDirection facingDirection = FacingDirection.Right;

    [Header("Movement")]
    public float movementSpeed = 2.5f;
    public float climbingSpeed = 2.5f;
    public float jumpForce = 5f;
    public float decelerationRate = 10f;
    public float jumpDelay = 0.5f;

    [Header("Ladder Jump")]
    public float ladderJumpHorizontalForce = 5f;
    public float ladderJumpVerticalFactor = 0.2f;
    public float ladderGrabDelay = 0.5f;

    [Header("Ground Check")]
    public float groundCheckWidth = 1;
    public float groundCheckHeight = 0.1f;
    public float groundCheckOffset = 0.8f;
    public LayerMask groundMask;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Ladder currentLadder;

    private bool isOnLadder = false;
    private bool readyToClimb = true;
    private bool readyToJump = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Move(Vector2 inputAxis)
    {
        float horizontalInput = (inputAxis.x == 0f) ? 0f : Mathf.Sign(inputAxis.x);
        float vertical = (inputAxis.y == 0f) ? 0f : Mathf.Sign(inputAxis.y);

        if (!isOnLadder)
        {
            if (horizontalInput < 0 && !sr.flipX)
            {
                FlipCharacter();
            }
            else if (horizontalInput > 0 && sr.flipX)
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
        else if(currentLadder != null)
        {
            rb.linearVelocity = new Vector2(0f, vertical * climbingSpeed);
            CheckLadderPosition(vertical);
        }
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
        return Physics2D.BoxCast(transform.position, boxSize, 0, Vector2.down, groundCheckOffset, groundMask);
    }

    private void TriggerVertical(float direction)
    {
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
        bool bottomExit = rb.position.y < currentLadder.Bottom && direction < 0;
        bool topExit = rb.position.y > currentLadder.Top && direction > 0;

        if (bottomExit || topExit)
        {
            SetLadder(false, 0, false);
        }
    }

    private void SetLadder(bool ladderGrabbed, float input = 0, bool jumpedOff = true, bool topEntrance = false)
    {
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
        if(!readyToJump) return;
        if (!isOnLadder && IsGrounded())
        {
            rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + Vector3.down * groundCheckOffset, new(groundCheckWidth, groundCheckHeight));
    }
#endif
}
