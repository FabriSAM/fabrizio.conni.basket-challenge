using FabrizioConni.BasketChallenge.Utility;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace FabrizioConni.BasketChallenge.Ball
{
    // Struct containing all parameters for the ball shot, allowing easy tuning from the Inspector
    [Serializable]
    public struct BallShotParams
    {
        public float horizSensitivity; // Sensitivity for horizontal drag
        public float vertSensitivity;  // Sensitivity for vertical drag
        public float baseForward;      // Base forward force
        public float forwardBoost;     // Additional forward force based on drag
        public float upPower;          // Upward force multiplier
        public float lateralPower;     // Lateral force multiplier
        public float deadzone;         // Minimum drag threshold to register input
    }

    // Main controller for the basketball, handles input, shooting, and trajectory visualization
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private float power; // Not used directly, but can be used for global power scaling
        [SerializeField]
        private LineRenderer trajectoryLine; // LineRenderer to show the predicted trajectory
        [SerializeField]
        private int lineSegmentCount = 25; // Number of segments for the trajectory line
        [SerializeField]
        private float timeStep = 0.1f; // Time step between trajectory points
        [SerializeField]
        private GameObject HoopCenter; // Reference to the hoop's center for aiming
        [SerializeField]
        private BallShotParams shotParams; // Parameters for the shot, editable in Inspector
        [SerializeField]
        private LayerMask collisionMask; // Mask for collision detection in trajectory
        #endregion

        #region Private Fields
        private Rigidbody rb; // Cached Rigidbody reference
        private Vector2 mouseStartPosition; // Mouse position at drag start
        private bool dragging; // Is the player currently dragging to aim?
        private bool shot; // Has the ball been shot?
        private bool isCancelled; // Has the shot been cancelled?
        private Transform aimOrigin; // Origin for calculating shot direction
        private UI_Timer uPower; // UI element to show shot power
        #endregion

        #region Properties
        // Returns the world position of the hoop center
        public Vector3 HoopCenterLocation { get { return HoopCenter.transform.position; } }
        #endregion

        #region Actions
        // Event invoked when the ball reset is complete
        public UnityAction<Transform> onResetComplete;
        #endregion

        #region Monobehaviour Callbacks
        private void Start()
        {
            // Find and cache the UI element for shot power
            uPower = GameObject.Find("ShootPower").GetComponent<UI_Timer>();
        }

        private void Awake()
        {
            // Cache Rigidbody and subscribe to input events
            rb = GetComponent<Rigidbody>();
            InputManager.Computer.Shoot.started += ShootStartCallback;
            InputManager.Computer.Shoot.canceled += ShootCanceledCallback;
        }

        private void OnDestroy()
        {
            // Unsubscribe from input events to avoid memory leaks
            InputManager.Computer.Shoot.started -= ShootStartCallback;
            InputManager.Computer.Shoot.canceled -= ShootCanceledCallback;
        }

        void Update()
        {
            // If not dragging, do nothing
            if (!dragging) return;
            Vector3 force = Vector3.zero;
            // Calculate the force based on current drag
            ShootTrajectory(ref force);
            // Show the predicted trajectory
            ShowTrajectory(transform.position, force);
        }
        #endregion

        #region Private Methods
        // Calculates the force vector for the shot based on mouse drag
        private void ShootTrajectory(ref Vector3 force)
        {
            // Get current mouse position and calculate drag delta
            Vector2 end = InputManager.MousePosition;
            Vector2 delta = end - mouseStartPosition;

            // Normalize drag to screen size for consistency across devices
            Vector2 nd = new Vector2(
                delta.x / Screen.width,
                delta.y / Screen.height
            );

            // Extract horizontal and vertical components, apply sensitivity and clamp
            float horiz = Mathf.Clamp(nd.x * shotParams.horizSensitivity, -1f, 1f);
            float vert = Mathf.Clamp(nd.y * shotParams.vertSensitivity, 0f, 1); // Only positive vertical increases power

            // Update UI with current power
            uPower.SetProgress(vert);

            // Apply deadzone to avoid accidental small drags
            if (Mathf.Abs(nd.x) < shotParams.deadzone) horiz = 0f;
            if (Mathf.Abs(nd.y) < shotParams.deadzone) vert = 0f;

            // Build force vector in local space
            Vector3 fForward = aimOrigin.forward * (shotParams.baseForward + vert * shotParams.forwardBoost);
            Vector3 fUp = aimOrigin.up * (vert * shotParams.upPower);
            Vector3 fRight = aimOrigin.right * (horiz * shotParams.lateralPower);

            // Combine all components for the final force
            force = fForward + fUp + fRight;
        }

        // Applies the calculated force to the ball to shoot it
        private void Shoot()
        {
            Vector3 force = Vector3.zero;
            ShootTrajectory(ref force);

            // Enable physics and reset velocities
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // Apply the force as an instant velocity change
            rb.AddForce(force, ForceMode.VelocityChange);

            shot = true;
        }
        #endregion

        #region Public Methods
        // Draws the predicted trajectory using physics simulation
        public void ShowTrajectory(Vector3 startPos, Vector3 initialVelocity)
        {
            Vector3[] points = new Vector3[lineSegmentCount];
            points[0] = startPos;

            for (int i = 1; i < lineSegmentCount; i++)
            {
                float t = i * timeStep;
                // Calculate next point using kinematic equation
                Vector3 point = startPos + initialVelocity * t + 0.5f * Physics.gravity * t * t;

                // If the trajectory hits something, stop and set the last point at the hit
                if (Physics.Raycast(points[i - 1], point - points[i - 1], out RaycastHit hit, (point - points[i - 1]).magnitude, collisionMask))
                {
                    points[i] = hit.point;
                    trajectoryLine.positionCount = i + 1;
                    trajectoryLine.SetPositions(points);
                    return;
                }

                points[i] = point;
            }

            // Set all points if no collision
            trajectoryLine.positionCount = lineSegmentCount;
            trajectoryLine.SetPositions(points);
        }

        // Resets the ball to a new position and state for the next shot
        public void ResetBall()
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Get a new spawn position near the hoop
            transform.position = SpawnerFactory.GetPosition(HoopCenter.transform.position, UnityEngine.Random.Range(1.2f, 1.5f));
            // Orient the ball to face the hoop
            Vector3 direction = HoopCenter.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
            shot = false;
            dragging = false;
            trajectoryLine.positionCount = 0;
            // Notify listeners that the reset is complete
            onResetComplete?.Invoke(transform);
        }
        #endregion

        #region Callbacks
        // Called when the shoot input is released/canceled
        private void ShootCanceledCallback(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (shot) return;
            if (isCancelled) return;
            dragging = false;
            Shoot();
            isCancelled = true;
        }

        // Called when the shoot input is started/pressed
        private void ShootStartCallback(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (shot) return;
            mouseStartPosition = InputManager.MousePosition;
            aimOrigin = transform;
            dragging = true;
            isCancelled = false;
        }
        #endregion
    }
}

