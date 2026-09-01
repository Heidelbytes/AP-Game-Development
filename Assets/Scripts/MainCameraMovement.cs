using UnityEngine;
using UnityEngine.InputSystem;

public class MainCameraMovement : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("References")]

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private BoxCollider fieldBounds;


    // =========================================================
    // MOVEMENT SETTINGS
    // =========================================================

    [Header("Movement")]

    [SerializeField]
    private float moveSpeed = 15f;

    [SerializeField]
    private float movementSmoothness = 10f;


    // =========================================================
    // ZOOM SETTINGS
    // =========================================================

    [Header("Zoom")]

    [SerializeField]
    private float zoomSpeed = 10f;

    [SerializeField]
    private float minZoom = 5f;

    [SerializeField]
    private float maxZoom = 30f;


    // =========================================================
    // ROTATION SETTINGS
    // =========================================================

    [Header("Rotation")]

    [SerializeField]
    private float rotationSpeed = 90f;

    [SerializeField]
    private float rotationSmoothness = 10f;


    // =========================================================
    // CAMERA ANGLE
    // =========================================================

    [Header("Camera Angle")]

    [SerializeField]
    [Range(10f, 80f)]
    private float pitch = 55f;


    // =========================================================
    // INPUT ACTIONS
    // =========================================================

    [Header("Input Actions")]

    [SerializeField]
    private InputActionReference moveAction;

    [SerializeField]
    private InputActionReference zoomAction;

    [SerializeField]
    private InputActionReference rotateAction;


    // =========================================================
    // INTERNAL VALUES
    // =========================================================

    // The point on the field that the camera is centered on.
    private Vector3 targetPosition;

    // Current and desired horizontal rotation.
    private float currentYaw;
    private float targetYaw;

    // Current and desired zoom.
    private float currentZoom;
    private float targetZoom;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void Awake()
    {
        // Automatically find the camera if none is assigned.
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        // Use an orthographic camera.
        cam.orthographic = true;


        // Set the initial zoom.
        currentZoom = Mathf.Clamp(
            (minZoom + maxZoom) * 0.5f,
            minZoom,
            maxZoom
        );

        targetZoom = currentZoom;


        // Use the current camera rotation as the starting yaw.
        currentYaw = transform.eulerAngles.y;
        targetYaw = currentYaw;


        // Start at the center of the play field.
        if (fieldBounds != null)
        {
            targetPosition = fieldBounds.bounds.center;

            // Keep the target on the ground plane.
            targetPosition.y = fieldBounds.bounds.min.y;
        }
        else
        {
            targetPosition = Vector3.zero;
        }
    }


    // =========================================================
    // INPUT ACTIONS
    // =========================================================

    private void OnEnable()
    {
        // Enable the assigned input actions.
        moveAction?.action.Enable();
        zoomAction?.action.Enable();
        rotateAction?.action.Enable();
    }


    private void OnDisable()
    {
        // Disable the input actions when the camera is disabled.
        moveAction?.action.Disable();
        zoomAction?.action.Disable();
        rotateAction?.action.Disable();
    }


    // =========================================================
    // MAIN UPDATE
    // =========================================================

    private void Update()
    {
        // Read all player input.
        HandleMovement();
        HandleZoom();
        HandleRotation();


        // Update the camera values.
        UpdateCameraValues();


        // Keep the visible area inside the field.
        ClampCameraToField();


        // Apply the final values to the camera.
        ApplyCameraTransform();
    }


    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement()
    {
        // Read movement input from the Input System.
        Vector2 input = Vector2.zero;

        if (moveAction != null)
        {
            input = moveAction.action.ReadValue<Vector2>();
        }


        // Create a rotation using only the horizontal camera angle.
        Quaternion yawRotation = Quaternion.Euler(
            0f,
            targetYaw,
            0f
        );


        // Get the camera-relative forward and right directions.
        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;


        // Convert the 2D input into a 3D movement direction.
        Vector3 movement =
            forward * input.y +
            right * input.x;


        // Prevent faster diagonal movement.
        if (movement.sqrMagnitude > 1f)
        {
            movement.Normalize();
        }


        // Move the target point across the field.
        targetPosition +=
            movement *
            moveSpeed *
            Time.deltaTime;

    }

    // =========================================================
    // ZOOM
    // =========================================================

    private void HandleZoom()
    {
        // Stop if no zoom action is assigned.
        if (zoomAction == null)
        {
            return;
        }


        // Read the mouse wheel.
        float zoomInput =
            zoomAction.action.ReadValue<float>();


        // Change the desired zoom.
        targetZoom -=
            zoomInput *
            zoomSpeed *
            Time.deltaTime;


        // Keep the zoom within the allowed range.
        targetZoom = Mathf.Clamp(
            targetZoom,
            minZoom,
            maxZoom
        );
    }


    // =========================================================
    // ROTATION
    // =========================================================

    private void HandleRotation()
    {
        // Stop if no rotation action is assigned.
        if (rotateAction == null)
        {
            return;
        }


        // Read Q/E input.
        float rotationInput =
            rotateAction.action.ReadValue<float>();


        // Change the desired horizontal rotation.
        targetYaw +=
            rotationInput *
            rotationSpeed *
            Time.deltaTime;


        // Keep the angle between 0 and 360 degrees.
        targetYaw %= 360f;

        if (targetYaw < 0f)
        {
            targetYaw += 360f;
        }
    }


    // =========================================================
    // SMOOTHING
    // =========================================================

    private void UpdateCameraValues()
    {
        // Move towards the desired zoom.
        currentZoom = Mathf.Lerp(
            currentZoom,
            targetZoom,
            1f - Mathf.Exp(
                -movementSmoothness *
                Time.deltaTime
            )
        );


        // Rotate towards the desired yaw.
        currentYaw = Mathf.LerpAngle(
            currentYaw,
            targetYaw,
            1f - Mathf.Exp(
                -rotationSmoothness *
                Time.deltaTime
            )
        );
    }


    // =========================================================
    // CAMERA TRANSFORM
    // =========================================================

    private void ApplyCameraTransform()
    {
        // Create the final camera rotation.
        Quaternion rotation = Quaternion.Euler(
            pitch,
            currentYaw,
            0f
        );


        // Calculate the camera position relative to the target.
        Vector3 offset =
            rotation *
            new Vector3(
                0f,
                0f,
                -currentZoom
            );


        // Move the camera according to the calculated vector
        transform.position =
            targetPosition + offset;


        // Apply the calculated rotation.
        transform.rotation = rotation;


        // Apply the current orthographic zoom.
        cam.orthographicSize = currentZoom;
    }


    // =========================================================
    // FIELD BOUNDARY
    // =========================================================

    private void ClampCameraToField()
    {
        // Stop if no field collider is assigned.
        if (fieldBounds == null)
        {
            return;
        }


        Bounds bounds = fieldBounds.bounds;


        // -----------------------------------------------------
        // CALCULATE THE VISIBLE AREA
        // -----------------------------------------------------

        float aspect =
            (float)Screen.width / Screen.height;


        // Calculate half of the visible vertical size.
        float halfHeight = currentZoom;


        // Calculate half of the visible horizontal size.
        float halfWidth =
            currentZoom * aspect;


        // Calculate the camera's horizontal directions.
        Quaternion yawRotation = Quaternion.Euler(
            0f,
            currentYaw,
            0f
        );

        Vector3 cameraRight =
            yawRotation * Vector3.right;

        Vector3 cameraForward =
            yawRotation * Vector3.forward;


        // -----------------------------------------------------
        // CALCULATE HOW MUCH OF THE FIELD IS VISIBLE
        // -----------------------------------------------------

        float pitchRadians =
            pitch * Mathf.Deg2Rad;


        // Prevent division by zero 
        float sinPitch =
            Mathf.Max(
                Mathf.Sin(pitchRadians),
                0.001f
            );


        // The vertical screen direction projects onto the
        // ground plane by this amount.
        float forwardProjection =
            halfHeight *
            Mathf.Cos(pitchRadians) /
            sinPitch;


        // The horizontal screen direction does not depend
        // on the pitch.
        float rightProjection =
            halfWidth;


        // -----------------------------------------------------
        // CALCULATE THE FOUR VISIBLE CORNERS
        // -----------------------------------------------------

        Vector3 topLeft =
            targetPosition +
            cameraForward * forwardProjection +
            cameraRight * rightProjection;

        Vector3 topRight =
            targetPosition +
            cameraForward * forwardProjection -
            cameraRight * rightProjection;

        Vector3 bottomLeft =
            targetPosition -
            cameraForward * forwardProjection +
            cameraRight * rightProjection;

        Vector3 bottomRight =
            targetPosition -
            cameraForward * forwardProjection -
            cameraRight * rightProjection;


        // -----------------------------------------------------
        // FIND THE OUTERMOST X/Z VALUES
        // -----------------------------------------------------

        float minX = Mathf.Min(
            topLeft.x,
            topRight.x,
            bottomLeft.x,
            bottomRight.x
        );

        float maxX = Mathf.Max(
            topLeft.x,
            topRight.x,
            bottomLeft.x,
            bottomRight.x
        );


        float minZ = Mathf.Min(
            topLeft.z,
            topRight.z,
            bottomLeft.z,
            bottomRight.z
        );

        float maxZ = Mathf.Max(
            topLeft.z,
            topRight.z,
            bottomLeft.z,
            bottomRight.z
        );


        // -----------------------------------------------------
        // CALCULATE THE REQUIRED CORRECTION
        // -----------------------------------------------------

        Vector3 correction = Vector3.zero;


        // Move right if the visible area crosses the left edge.
        if (minX < bounds.min.x)
        {
            correction.x +=
                bounds.min.x - minX;
        }


        // Move left if the visible area crosses the right edge.
        if (maxX > bounds.max.x)
        {
            correction.x -=
                maxX - bounds.max.x;
        }


        // Move forward if the visible area crosses the back edge.
        if (minZ < bounds.min.z)
        {
            correction.z +=
                bounds.min.z - minZ;
        }


        // Move backward if the visible area crosses the front edge.
        if (maxZ > bounds.max.z)
        {
            correction.z -=
                maxZ - bounds.max.z;
        }


        // Apply the correction to the camera target.
        targetPosition += correction;


        // Keep the target on the field's ground level.
        targetPosition.y = bounds.min.y;
    }
}