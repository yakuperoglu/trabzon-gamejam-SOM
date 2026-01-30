using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/// <summary>
/// FPS Hareket Kontrolü - Stamina sistemi ile
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [Tooltip("Yürüme hızı")]
    public float walkSpeed = 5f;
    
    [Tooltip("Koşma hızı")]
    public float sprintSpeed = 8f;
    
    [Tooltip("Zıplama kuvveti")]
    public float jumpForce = 7f;

    [Header("Stamina Ayarları")]
    [Tooltip("Maksimum stamina")]
    public float maxStamina = 100f;
    
    [Tooltip("Koşarken saniyede harcanan stamina")]
    public float sprintStaminaCost = 15f;
    
    [Tooltip("Zıplarken harcanan stamina")]
    public float jumpStaminaCost = 20f;
    
    [Tooltip("Saniyede yenilenen stamina")]
    public float staminaRegenRate = 10f;
    
    [Tooltip("Stamina yenilenmesi için bekleme süresi (saniye)")]
    public float staminaRegenDelay = 1f;

    [Header("Yer Kontrolü")]
    [Tooltip("Zemin kontrol noktası")]
    public Transform groundCheck;
    
    [Tooltip("Zemin yarıçapı")]
    public float groundDistance = 0.4f;
    
    [Tooltip("Zemin layer'ı")]
    public LayerMask groundMask;

    [Header("Events")]
    public UnityEvent<float, float> OnStaminaChanged; // current, max
    public UnityEvent<string> OnNotEnoughStamina; // bildirim mesajı

    // Public Properties
    public float CurrentStamina { get; private set; }
    public float MaxStaminaValue => maxStamina;
    public bool IsSprinting { get; private set; }
    public bool IsGrounded => isGrounded;

    // Private değişkenler
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpRequested;
    private bool sprintHeld;
    private float lastStaminaUseTime;
    private bool wasSprintBlocked; // Koşma engellendiğinde tekrar bildirim göstermemek için

    // Input Actions
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

        // Stamina başlat
        CurrentStamina = maxStamina;

        // Ground Check oluştur
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -0.9f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
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
        // Yeterli stamina var mı kontrol et
        if (CurrentStamina >= jumpStaminaCost)
        {
            jumpRequested = true;
        }
        else
        {
            // Yeterli stamina yok - bildirim gönder
            OnNotEnoughStamina?.Invoke("Zıplamak için yeterli enerji yok!");
            jumpRequested = false; // Buffer'a ekleme
        }
    }

    void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
        }
    }

    void HandleMovement()
    {
        if (rb == null) return;

        // Hareket yönünü hesapla
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // Sprint kontrolü - stamina > 0 ise koşabilir (sonuna kadar kullanabilir)
        bool wantsToSprint = sprintHeld && moveInput.y > 0 && moveDirection.magnitude > 0.1f;
        bool canSprint = wantsToSprint && CurrentStamina > 0;
        
        // Koşmak istiyor ama stamina yoksa bildirim göster (sadece bir kez)
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

        // Koşarken stamina harca
        if (IsSprinting)
        {
            UseStamina(sprintStaminaCost * Time.fixedDeltaTime);
        }

        // Hız hesapla
        float currentSpeed = IsSprinting ? sprintSpeed : walkSpeed;

        // Velocity uygula
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    void HandleJump()
    {
        if (rb == null) return;

        // Zıpla - yeterli stamina ve yerdeyse
        if (jumpRequested && isGrounded && CurrentStamina >= jumpStaminaCost)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            UseStamina(jumpStaminaCost);
            jumpRequested = false;
        }
        else if (jumpRequested)
        {
            // Yerde değilse veya stamina yetersizse - hemen sıfırla (bekleme yok)
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
        // Stamina kullanımından sonra bekleme süresi geçtiyse yenile
        if (Time.time - lastStaminaUseTime >= staminaRegenDelay && CurrentStamina < maxStamina)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + staminaRegenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        }
    }

    // Dışarıdan stamina eklemek için (power-up vb.)
    public void AddStamina(float amount)
    {
        CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + amount);
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
