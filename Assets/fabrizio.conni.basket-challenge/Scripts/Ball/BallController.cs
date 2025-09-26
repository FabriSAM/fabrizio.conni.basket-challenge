using Codice.CM.Common.Tree;
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
        private GameObject BackboardCenter; // Reference to the hoop's center for aiming
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
        private ShotbarUI uPower; // UI element to show shot power
        private float normalizedPerfectInputValue;
        private float vPerfect;
        private float normalizedBlackboardInputValue;
        private float vBlackboardPerfect;
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
            
        }

        private void Awake()
        {
            // Cache Rigidbody and subscribe to input events
            rb = GetComponent<Rigidbody>();
            InputManager.Computer.Shoot.started += ShootStartCallback;
            InputManager.Computer.Shoot.canceled += ShootCanceledCallback;
           
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
            InputManager.Computer.Shoot.canceled -= ShootCanceledCallback;
        }

        void Update()
        {
            // If not dragging, do nothing
            if (!dragging) return;
            var force = Vector3.zero;
            CalculateForceFromInput(ref force);
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
            Debug.DrawLine(startPos, dynamicTarget, Color.red);
            Debug.DrawRay(dynamicTarget, Vector3.up * 0.5f, Color.green);
            return dynamicTarget;
        }

        private void CalculateForceFromInput(ref Vector3 force)
        {
            Vector2 end = InputManager.MousePosition;
            Vector2 delta = end - mouseStartPosition;

            // Normalizza il drag rispetto allo schermo
            Vector2 nd = new Vector2(
                delta.x / Screen.width,
                delta.y / Screen.height
            );

            // Applica la deadzone
            if (Mathf.Abs(nd.x) < shotParams.deadzone) nd.x = 0f;
            if (Mathf.Abs(nd.y) < shotParams.deadzone) nd.y = 0f;

            // Sensibilità input
            float horiz = Mathf.Clamp(nd.x * shotParams.horizSensitivity, -1f, 1f);
            float vert = Mathf.Clamp(nd.y, 0f, 1f);

            // Aggiorna UI potenza
            uPower.UpdateCursor(vert);

            // Posizioni
            
            Vector3 startPos = transform.position;
            Vector3 targetPos = GetDynamicTarget(vert);

            // Distanza orizzontale (XZ)
            Vector3 directionXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);
            float d = directionXZ.magnitude;

            // Altezza relativa
            float h = targetPos.y - startPos.y;

            // Angolo modulato dal drag verticale
            float minAngle = 50f;
            float maxAngle = 60f;
            float angleDeg = Mathf.Lerp(minAngle, maxAngle, vert);
            float angle = angleDeg * Mathf.Deg2Rad;

            // Gravità positiva
            float g = Mathf.Abs(Physics.gravity.y);

            // Calcolo velocità
            float v2 = (g * d * d) / (2 * (d * Mathf.Tan(angle) - h) * Mathf.Cos(angle) * Mathf.Cos(angle));
            if (v2 <= 0f)
            {
                force = Vector3.zero;
                return;
            }

            float v = Mathf.Sqrt(v2);

            if ( vert - normalizedPerfectInputValue < 0.05 && vert - normalizedPerfectInputValue > -0.05)
            {
                Debug.Log("Perfetto!");
                angle = 75f * Mathf.Deg2Rad; // angolo scelto
                v = vPerfect;
            }

            if (vert - normalizedBlackboardInputValue < 0.05 && vert - normalizedBlackboardInputValue > -0.05)
            {
                Debug.Log("Perfetto sul tabellone!");
                angle = 50f * Mathf.Deg2Rad; // angolo scelto
                v = vBlackboardPerfect * 0.95f;

                targetPos = BackboardCenter.transform.position;

                // Ricalcolo direzione e distanza
                directionXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);
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

            shot = true;
        }
        private float CalculatePerfectShotVelocity()
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = HoopCenter.transform.position;
            float angleRad = 75f * Mathf.Deg2Rad; // angolo scelto

            float g = Mathf.Abs(Physics.gravity.y);

            // distanza orizzontale (XZ)
            Vector3 dirXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);
            float d = dirXZ.magnitude;

            // differenza di altezza
            float h = targetPos.y - startPos.y;

            // formula fisica
            float numerator = g * d * d;
            float denominator = 2 * (d * Mathf.Tan(angleRad) - h) * Mathf.Cos(angleRad) * Mathf.Cos(angleRad);

            if (denominator <= 0) return -1f; // nessuna soluzione valida

            return Mathf.Sqrt(numerator / denominator);
        }

        private float CalculatePerfectBlackboardShotVelocity()
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = BackboardCenter.transform.position;
            float angleRad = 50f * Mathf.Deg2Rad; // angolo scelto
            float g = Mathf.Abs(Physics.gravity.y);
            // distanza orizzontale (XZ)
            Vector3 dirXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);
            float d = dirXZ.magnitude;
            // differenza di altezza
            float h = targetPos.y - startPos.y;
            // formula fisica
            float numerator = g * d * d;
            float denominator = 2 * (d * Mathf.Tan(angleRad) - h) * Mathf.Cos(angleRad) * Mathf.Cos(angleRad);
            if (denominator <= 0) return -1f; // nessuna soluzione valida
            return Mathf.Sqrt(numerator / denominator);
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
            vPerfect = CalculatePerfectShotVelocity();
            normalizedPerfectInputValue = uPower.SetGreenZone(vPerfect, 0.05f);
            vBlackboardPerfect = CalculatePerfectBlackboardShotVelocity();
            normalizedBlackboardInputValue = uPower.SetPurpleZone(vBlackboardPerfect, 0.05f);
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

