using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// FPS Kamera Kontrolü - Titreme olmadan smooth takip
/// Bu script kameraya eklenir, player'a DEĞİL!
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Hedef")]
    [Tooltip("Takip edilecek karakter (Player)")]
    public Transform target;
    
    [Tooltip("Kameranın karaktere göre offset'i (göz pozisyonu)")]
    public Vector3 offset = new Vector3(0f, 1.6f, 0f);

    [Header("Mouse Ayarları")]
    [Tooltip("Mouse hassasiyeti")]
    public float mouseSensitivity = 2f;
    
    [Tooltip("Yukarı bakış limiti")]
    public float upperLookLimit = 85f;
    
    [Tooltip("Aşağı bakış limiti")]
    public float lowerLookLimit = 85f;

    [Header("Smooth Ayarları")]
    [Tooltip("Pozisyon takip hızı (yüksek = daha hızlı takip)")]
    public float followSpeed = 50f;
    
    [Tooltip("Rotasyon smooth süresi")]
    [Range(0f, 0.1f)]
    public float rotationSmoothTime = 0.02f;

    // Private değişkenler
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector2 lookInput;
    private float currentXRotation;
    private float currentYRotation;
    private float xRotationVelocity;
    private float yRotationVelocity;

    // Input
    private PlayerInput playerInput;
    private InputAction lookAction;

    void Start()
    {
        // Target yoksa Player tag'li objeyi bul
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogError("CameraController: Target bulunamadı! Lütfen Player'ı atayın veya 'Player' tag'i ekleyin.");
            }
        }

        // Input System
        if (target != null)
        {
            playerInput = target.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                lookAction = playerInput.actions["Look"];
            }
        }

        // Başlangıç rotasyonları
        if (target != null)
        {
            yRotation = target.eulerAngles.y;
        }
        currentXRotation = xRotation;
        currentYRotation = yRotation;

        // Mouse kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ReadInput();
    }

    // LateUpdate'de kamera hareketi - tüm fizik ve hareket bittikten sonra
    void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        HandlePosition();
    }

    void ReadInput()
    {
        if (playerInput != null && lookAction != null)
        {
            lookInput = lookAction.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue();
        }
    }

    void HandleRotation()
    {
        // Time.deltaTime == 0 olduğunda (pause) atla
        if (Time.deltaTime <= 0) return;

        // Mouse input
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime * 60f; // Frame-rate independent
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime * 60f;

        // Hedef rotasyonları güncelle
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -upperLookLimit, lowerLookLimit);

        // Smooth rotasyon
        if (rotationSmoothTime > 0)
        {
            currentXRotation = Mathf.SmoothDamp(currentXRotation, xRotation, ref xRotationVelocity, rotationSmoothTime);
            currentYRotation = Mathf.SmoothDamp(currentYRotation, yRotation, ref yRotationVelocity, rotationSmoothTime);
        }
        else
        {
            currentXRotation = xRotation;
            currentYRotation = yRotation;
        }

        // Kamera rotasyonunu uygula
        transform.rotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);

        // Player'ı da Y ekseninde döndür (karakter baktığı yöne dönmeli)
        if (target != null)
        {
            target.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }

    void HandlePosition()
    {
        // Hedef pozisyon
        Vector3 targetPosition = target.position + offset;

        // Smooth pozisyon takibi
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
