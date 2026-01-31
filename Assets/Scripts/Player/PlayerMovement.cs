using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// FPS Hareket Kontrolü - Stamina sistemi ile
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 7f;

    [Header("Stamina Ayarları")]
    public float maxStamina = 100f;
    public float sprintStaminaCost = 15f;
    public float jumpStaminaCost = 20f;
    public float staminaRegenRate = 10f;
    public float staminaRegenDelay = 1f;

    [Header("Yer Kontrolü")]
    public LayerMask groundMask;

    [Header("Events")]
    public UnityEvent<float, float> OnStaminaChanged;
    public UnityEvent<string> OnNotEnoughStamina;

    // Public Properties
    public float CurrentStamina { get; private set; }
    public float MaxStaminaValue => maxStamina;
    public bool IsSprinting { get; private set; }
    public bool IsGrounded => isGrounded;

    // Private
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpRequested;
    private bool sprintHeld;
    private float lastStaminaUseTime;
    private bool wasSprintBlocked;
    private float lastJumpTime;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            sprintAction = playerInput.actions["Sprint"];
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement: Rigidbody bulunamadı!");
            return;
        }
        
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        CurrentStamina = maxStamina;
        
        if (groundMask == 0)
        {
            groundMask = ~0;
        }
    }

    void Update()
    {
        ReadInput();
        CheckGrounded();
        HandleStaminaRegen();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    void ReadInput()
    {
        if (playerInput != null && moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
            if (jumpAction != null && jumpAction.WasPressedThisFrame())
            {
                TryJump();
            }
            sprintHeld = sprintAction != null && sprintAction.IsPressed();
        }
        else
        {
            moveInput = Vector2.zero;
            
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
                if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
                if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
                if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    TryJump();
                }
                sprintHeld = Keyboard.current.leftShiftKey.isPressed;
            }
        }
    }

    void TryJump()
    {
        if (CurrentStamina >= jumpStaminaCost)
        {
            jumpRequested = true;
        }
        else
        {
            OnNotEnoughStamina?.Invoke("Zıplamak için yeterli enerji yok!");
            jumpRequested = false;
        }
    }

    void CheckGrounded()
    {
        // Zıplama cooldown
        if (Time.time - lastJumpTime < 0.3f)
        {
            isGrounded = false;
            return;
        }
        
        // Yukarı hareket ediyorsak yerde değiliz
        if (rb != null && rb.linearVelocity.y > 0.1f)
        {
            isGrounded = false;
            return;
        }
        
        // Raycast ile zemin kontrolü
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 1.2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject != gameObject && !hit.collider.transform.IsChildOf(transform))
            {
                isGrounded = true;
                return;
            }
        }
        
        isGrounded = false;
    }

    void HandleMovement()
    {
        if (rb == null) return;

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        bool wantsToSprint = sprintHeld && moveInput.y > 0 && moveDirection.magnitude > 0.1f;
        bool canSprint = wantsToSprint && CurrentStamina > 0;
        
        if (wantsToSprint && CurrentStamina <= 0)
        {
            if (!wasSprintBlocked)
            {
                OnNotEnoughStamina?.Invoke("Koşmak için yeterli enerji yok!");
                wasSprintBlocked = true;
            }
        }
        else if (!wantsToSprint || CurrentStamina > 0)
        {
            wasSprintBlocked = false;
        }
        
        IsSprinting = canSprint;

        if (IsSprinting)
        {
            UseStamina(sprintStaminaCost * Time.fixedDeltaTime);
        }

        float currentSpeed = IsSprinting ? sprintSpeed : walkSpeed;

        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    void HandleJump()
    {
        if (rb == null) return;

        if (jumpRequested && isGrounded && CurrentStamina >= jumpStaminaCost)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            UseStamina(jumpStaminaCost);
            jumpRequested = false;
            lastJumpTime = Time.time;
        }
        else if (jumpRequested)
        {
            jumpRequested = false;
        }
    }

    void UseStamina(float amount)
    {
        CurrentStamina = Mathf.Max(0, CurrentStamina - amount);
        lastStaminaUseTime = Time.time;
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    void HandleStaminaRegen()
    {
        if (Time.time - lastStaminaUseTime >= staminaRegenDelay && CurrentStamina < maxStamina)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + staminaRegenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }
    }

    public void AddStamina(float amount)
    {
        CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + amount);
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }
}
