using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    private Movement movement;
    private Attack attack;

    private Vector2 currentInput = Vector2.zero;
    private bool isJumping;
    private bool isAttacking;

    private void Awake()
    {
        movement = GetComponent<Movement>();

        var playerMap = inputActions.FindActionMap("Player 2D");
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        attackAction = playerMap.FindAction("Attack");
    }

    private void MoveStart(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
    }

    private void MoveStop(InputAction.CallbackContext context)
    {
        currentInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        movement.Move(currentInput);

        if (isJumping)
        {
            movement.Jump(currentInput);
        }

        if (isAttacking)
        {
            attack.ExecuteAttack
        }
    }

    private void JumpStart(InputAction.CallbackContext context)
    {
        isJumping = true;
    }

    private void JumpStop(InputAction.CallbackContext context)
    {
        isJumping = false;
    }

    private void AttackStart(InputAction.CallbackContext context)
    {
        isAttacking = false;
    }

    private void AttackStop(InputAction.CallbackContext context)
    {
        isAttacking = false;
    }

    #region Event Subscription/Unsubscription
    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        moveAction.Enable();
        moveAction.performed += MoveStart;
        moveAction.canceled += MoveStop;

        jumpAction.Enable();
        jumpAction.performed += JumpStart;
        jumpAction.canceled += JumpStop;

        attackAction.Enable();
        attackAction.performed += AttackStart;
        attackAction.canceled += AttackStop;
    }

    private void Unsubscribe()
    {
        moveAction.performed -= MoveStart;
        moveAction.canceled -= MoveStop;
        moveAction.Disable();

        jumpAction.performed -= JumpStart;
        jumpAction.canceled -= JumpStop;
        jumpAction.Disable();

        attackAction.performed -= AttackStart;
        attackAction.canceled -= AttackStop;
        attackAction.Disable();
    }
    #endregion
}
