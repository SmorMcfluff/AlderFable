using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Movement movement;

    private Vector2 currentInput = Vector2.zero;
    private bool isJumping;

    private void Awake()
    {
        movement = GetComponent<Movement>();

        var playerMap = inputActions.FindActionMap("Player 2D");
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
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
    }

    private void JumpStart(InputAction.CallbackContext context)
    {
        isJumping = true;
    }

    private void JumpStop(InputAction.CallbackContext context)
    {
        isJumping = false;
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
    }

    private void Unsubscribe()
    {
        moveAction.performed -= MoveStart;
        moveAction.canceled -= MoveStop;
        moveAction.Disable();

        jumpAction.performed -= JumpStart;
        jumpAction.canceled -= JumpStop;
        jumpAction.Disable();
    }
    #endregion
}
