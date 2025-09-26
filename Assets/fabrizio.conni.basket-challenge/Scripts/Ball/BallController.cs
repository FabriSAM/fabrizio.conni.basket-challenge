using Codice.CM.Common.Tree;
using FabrizioConni.BasketChallenge.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Utilities;

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
        private GameObject BackboardCenter; // Reference to the hoop's center for aiming
        [SerializeField]
        private BallShotParams shotParams; // Parameters for the shot, editable in Inspector
        [SerializeField]
        private LayerMask collisionMask; // Mask for collision detection in trajectory
        #endregion

        #region Private Fields
        private Rigidbody rb; // Cached Rigidbody reference
        private Vector2 mouseStartPosition; // Mouse position at drag start
        private Vector2 lastTickPosition; // Mouse position in the last frame   
        private bool dragging; // Is the player currently dragging to aim?
        private bool shot; // Has the ball been shot?
        private bool isCancelled; // Has the shot been cancelled?
        private Transform aimOrigin; // Origin for calculating shot direction
        private ShotbarUI uPower; // UI element to show shot power
        private float normalizedPerfectInputValue;
        private float vPerfect;
        private float normalizedBlackboardInputValue;
        private float vBlackboardPerfect;

        private float horiz; // Horizontal input value from drag
        private float vert;  // Vertical input value from drag
        private float angle; // Calculated angle for the shot in radians
        private float perfectAngle; // Angle for a perfect shot in radians
        private int tickCounter; // Counter for fixed update ticks
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

        private void Awake()
        {
            // Cache Rigidbody and subscribe to input events
            rb = GetComponent<Rigidbody>();
            InputManager.Computer.Shoot.started += ShootStartCallback;
            InputManager.Computer.Shoot.canceled += ShootCancelledCallback;

        }
        private void Start()
        {
            StartCoroutine(InitAfterFrame());
        }

        private IEnumerator InitAfterFrame()
        {
            yield return new WaitUntil(() => HoopCenter != null && BackboardCenter != null);
            UpdatePerfectScoreUI();
        }
        private void OnEnable()
        {
            // Find and cache the UI element for shot power
            uPower = GameObject.Find("ShootPower").GetComponent<ShotbarUI>();            
        }
        

        private void OnDestroy()
        {
            // Unsubscribe from input events to avoid memory leaks
            InputManager.Computer.Shoot.started -= ShootStartCallback;
            InputManager.Computer.Shoot.canceled -= ShootCancelledCallback;
        }

        void Update()
        {
        
            // If not dragging, do nothing
            if (!dragging) return;
            var force = Vector3.zero;
            InputUpdate();
            //CalculateForceFromInput(ref force);
        }

        private void FixedUpdate()
        {
            UpdateUI();
        }
        #endregion

        #region Private Methods

        private Vector3 GetDynamicTarget(float forceMagnitude)
        {
            Vector3 startPos = transform.position;
            Vector3 hoopCenter = HoopCenterLocation;

            // Punto corto (a metà distanza)
            Vector3 nearTarget = Vector3.Lerp(startPos, hoopCenter, 0.5f);
            Vector3 farTarget = hoopCenter + (hoopCenter - startPos).normalized * 0.4f;

            // Interpolazione in base all’input
            // vert = 0 → nearTarget
            // vert = 1 → hoopCenter
            Vector3 dynamicTarget = Vector3.Lerp(nearTarget, farTarget, forceMagnitude);
            return dynamicTarget;
        }

        private void InputUpdate()
        {
            Vector2 end = InputManager.MousePosition;
            Vector2 delta = end - mouseStartPosition;
            Vector2 deltaPositionTick = end - lastTickPosition;
            lastTickPosition = end;

            Debug.Log($"Delta: {deltaPositionTick}");
            // Normalizza il drag rispetto allo schermo
            Vector2 nd = new Vector2(
                delta.x / Screen.width,
                delta.y / Screen.height
            );

            // Applica la deadzone
            if (Mathf.Abs(nd.x) < shotParams.deadzone) nd.x = 0f;
            if (Mathf.Abs(nd.y) < shotParams.deadzone) nd.y = 0f;

            // Sensibilità input
            horiz = Mathf.Clamp(nd.x * shotParams.horizSensitivity, -1f, 1f);
            vert = Mathf.Clamp(nd.y, 0f, 1f);

            // Angolo modulato dal drag verticale
            float minAngle = 50f;
            float maxAngle = 60f;
            float angleDeg = Mathf.Lerp(minAngle, maxAngle, vert);
            angle = angleDeg * Mathf.Deg2Rad;

            if (deltaPositionTick.y <= 0f)
            {
                tickCounter++;
                if (tickCounter > 3) // 3 tick consecutivi di movimento verso il basso
                {
                    ShootCancelledCallback(new UnityEngine.InputSystem.InputAction.CallbackContext());
                    return;
                }
                return;
            }
            tickCounter = 0;

        }

        private void UpdateUI()
        {
            // Aggiorna UI potenza
            uPower.UpdateCursor(vert);
        }

        private void UpdatePerfectScoreUI()
        {
            vPerfect = CalculatePerfectShot(HoopCenter.transform.position, 75f);
            normalizedPerfectInputValue = uPower.SetGreenZone(vPerfect, 0.05f);
            vBlackboardPerfect = CalculatePerfectShot(BackboardCenter.transform.position, 50f);
            normalizedBlackboardInputValue = uPower.SetPurpleZone(vBlackboardPerfect, 0.05f);
        }

        private Vector3 GetDirection(Vector3 start, Vector3 end)
        {
            return new Vector3(end.x - start.x, 0, end.z - start.z);
        }

        private float GetVelocitySqrt (float direction, float deltaHeight)
        {
            // Gravità positiva
            float g = Mathf.Abs(Physics.gravity.y);
            float velocity = (g * direction * direction) / (2 * (direction * Mathf.Tan(angle) - deltaHeight) * Mathf.Cos(angle) * Mathf.Cos(angle));
            if (velocity <= 0f) return -1f;

            return Mathf.Sqrt(velocity);
        }

        private void CalculateForceFromInput(ref Vector3 force)
        {
            // Posizioni            
            Vector3 startPos = transform.position;
            Vector3 targetPos = GetDynamicTarget(vert);

            // Distanza orizzontale (XZ)
            Vector3 directionXZ = GetDirection(startPos, targetPos);
            float d = directionXZ.magnitude;

            // Altezza relativa
            float h = targetPos.y - startPos.y;

            float v = GetVelocitySqrt(d, h);
            if (v <= 0f)
            {
                force = Vector3.zero;
                return;
            }

            float perfectShoot = MathF.Abs(vert - normalizedPerfectInputValue);

            if (perfectShoot < 0.05)
            {
                Debug.Log("Perfetto!");
                angle = 75f * Mathf.Deg2Rad; // angolo scelto
                v = vPerfect;
            }
            else if (perfectShoot < 0.2)
            {
                Debug.Log("Molto bene!");
                // preferisco ricalcolare la velocità per il bordo (non forzare v qui)
                angle = 75f * Mathf.Deg2Rad;
                Vector3 center = HoopCenter.transform.position; // centro del cerchio
                float radius = 0.11f;                           // raggio (es. ferro canestro)
                float angleRad = Mathf.Deg2Rad * UnityEngine.Random.Range(0, 360);           // 90° in radianti

                // punto sulla circonferenza
                Vector3 point = new Vector3(
                    center.x + radius * Mathf.Cos(angleRad),
                    center.y,
                    center.z + radius * Mathf.Sin(angleRad)
                );

                v = vPerfect;
                targetPos = point;
                // Ricalcolo direzione e distanza
                directionXZ = GetDirection(startPos, targetPos);
            }

            float blackboardShoot = MathF.Abs(vert - normalizedBlackboardInputValue);
            if (blackboardShoot < 0.05)
            {
                Debug.Log("Perfetto sul tabellone!");
                angle = 50f * Mathf.Deg2Rad; // angolo scelto
                v = vBlackboardPerfect * 0.95f;

                targetPos = BackboardCenter.transform.position;

                // Ricalcolo direzione e distanza
                directionXZ = GetDirection(startPos, targetPos);
            }

            // Direzione ruotata dal drag orizzontale
            Vector3 dirXZ = directionXZ.normalized;
            Quaternion rotation = Quaternion.AngleAxis(horiz * 15f, Vector3.up); // max 15° deviazione
            dirXZ = rotation * dirXZ;

            // Composizione finale della forza
            force = dirXZ * v * Mathf.Cos(angle) + Vector3.up * (v * Mathf.Sin(angle) + 0.2f);
        }

        // Applies the calculated force to the ball to shoot it
        private void Shoot()
        {
            Vector3 force = Vector3.zero;
            CalculateForceFromInput(ref force);

            // Enable physics and reset velocities
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // Apply the force as an instant velocity change
            rb.AddForce(force, ForceMode.VelocityChange);

            lastTickPosition = Vector2.zero;
            shot = true;
        }

        private float CalculatePerfectShot(Vector3 targetPos, float angle)
        {
            Vector3 startPos = transform.position;
            float angleRad = angle * Mathf.Deg2Rad; // angolo scelto

            this.angle = angleRad;
            // Distanza orizzontale (XZ)
            Vector3 directionXZ = GetDirection(startPos, targetPos);
            float d = directionXZ.magnitude;

            // Altezza relativa
            float h = targetPos.y - startPos.y;

            float v = GetVelocitySqrt(d, h);
            if (v <= 0f)
            {
                return -1;
            }
            return v;

        }


        #endregion

        #region Public Methods    

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


            UpdatePerfectScoreUI();
        }
        #endregion

        #region Callbacks
        // Called when the shoot input is released/canceled
        private void ShootCancelledCallback(UnityEngine.InputSystem.InputAction.CallbackContext context)
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

