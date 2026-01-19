using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    public float MovementSpeed;

    [SerializeField] GameObject projectilePrefab;

    Transform playerTransform;
    Rigidbody2D playerRigidbody;
    Camera playerCamera;
    PlayerInput playerInput;

    Vector2 movementInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerTransform = transform;
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

        playerInput.actions["Aim"].performed += OnAim;
        playerInput.actions["Shoot"].performed += OnShoot;
    }

    // Update is called once per frame
    void Update()
    {
        // Get movement input from PlayerInput
        movementInput = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Move player based on input
        playerRigidbody.linearVelocity = movementInput * MovementSpeed;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        // Rotate player to face the cursor
        Vector3 mousePos = playerCamera.ScreenToWorldPoint(context.ReadValue<Vector2>());
        mousePos.z = 0;

        Vector3 direction = mousePos - playerTransform.position;
        float cameraAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

        playerTransform.rotation = Quaternion.Euler(0, 0, cameraAngle);
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        // Instantiating projectile on shoot
        GameObject projectile = Instantiate(projectilePrefab, playerTransform.position + playerTransform.up * 0.58f, playerTransform.rotation);
    }
}
