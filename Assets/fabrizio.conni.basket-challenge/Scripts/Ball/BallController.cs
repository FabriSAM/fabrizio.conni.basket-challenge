using FabrizioConni.BasketChallenge.Utility;

using UnityEngine;
using UnityEngine.Events;

namespace FabrizioConni.BasketChallenge.Ball
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] 
        private float power;
        [SerializeField] private LineRenderer trajectoryLine;
        [SerializeField] private int lineSegmentCount = 30;
        [SerializeField] private float timeStep = 0.1f;
        [SerializeField] private LayerMask collisionMask;

        [SerializeField]
        private GameObject HoopCenter;
        #endregion

        #region Private Fields
        private Rigidbody rb;
        private Vector2 mouseStartPosition;
        private bool dragging;
        private bool shot;
        private bool isCancelled;
        private Transform aimOrigin;

        public Vector3 HoopCenterLocation { get { return HoopCenter.transform.position; } }
        #endregion

        public UnityAction<Transform> onResetComplete;
        public UnityAction<float> onShotPowerChange;

      
        #region Monobehaviour Callbacks
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            InputManager.Computer.Shoot.started += ShootStartCallback;
            InputManager.Computer.Shoot.canceled += ShootCanceledCallback;
        }

        private void OnDestroy()
        {
            InputManager.Computer.Shoot.started -= ShootStartCallback;
            InputManager.Computer.Shoot.canceled -= ShootCanceledCallback;
        }

        private void ShootCanceledCallback(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (shot) return;
            if (isCancelled) return;
            dragging = false;
            Shoot();
            isCancelled = true;
        }

        private void ShootStartCallback(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (shot) return;
            mouseStartPosition = InputManager.MousePosition;
            aimOrigin = transform;
            dragging = true;
            isCancelled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!dragging) return;
            Vector3 force = Vector3.zero;
            ShootTrajectory(ref force);
            ShowTrajectory(transform.position, force);
        }
        #endregion

        private void ShootTrajectory(ref Vector3 force)
        {            
            // Drag
            Vector2 end = InputManager.MousePosition;
            Vector2 delta = end - mouseStartPosition;

            // Normalizza il drag rispetto alla risoluzione (coerente su PC e mobile)
            Vector2 nd = new Vector2(
                delta.x / Screen.width,
                delta.y / Screen.height
            );

            // Parametri di sensibilità (tuning in Inspector)
            float horizSensitivity = 0.5f;   // quanto la palla devia lateralmente
            float vertSensitivity = 1.5f;   // quanto cresce la potenza/arco
            float baseForward = 1.5f;   // spinta minima in avanti
            float forwardBoost = 1;  // spinta extra in avanti legata alla potenza
            float upPower = 10;  // spinta verso l’alto legata alla potenza
            float lateralPower = 2f;   // forza laterale massima
            float deadzone = 0.005f; // ignora tocchi/piccoli movimenti



            //// Parametri di sensibilità (tuning in Inspector)
            //float horizSensitivity = 0.5f;   // quanto la palla devia lateralmente
            //float vertSensitivity = 2;   // quanto cresce la potenza/arco
            //float baseForward = 1;   // spinta minima in avanti
            //float forwardBoost = 1;  // spinta extra in avanti legata alla potenza
            //float upPower = 5;  // spinta verso l’alto legata alla potenza
            //float lateralPower = 2f;   // forza laterale massima
            //float deadzone = 0.005f; // ignora tocchi/piccoli movimenti

            // Estrai componenti intenzionali
            float horiz = Mathf.Clamp(nd.x * horizSensitivity, -1f, 1f);
            float vert = Mathf.Clamp(nd.y * vertSensitivity, 0f, 1); // solo su aumenta potenza
            onShotPowerChange?.Invoke(vert);

            // Applica deadzone
            if (Mathf.Abs(nd.x) < deadzone) horiz = 0f;
            if (Mathf.Abs(nd.y) < deadzone) vert = 0f;

            // Costruisci la forza nei 3 assi locali
            Vector3 fForward = aimOrigin.forward * (baseForward + vert * forwardBoost);
            Vector3 fUp = aimOrigin.up * (vert * upPower);
            Vector3 fRight = aimOrigin.right * (horiz * lateralPower);

            // Forza finale
            force = fForward + fUp + fRight;

        }

        private void Shoot()
        {
            // Forza finale
            Vector3 force = Vector3.zero;
            ShootTrajectory(ref force);

            // Spara
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.VelocityChange);

            shot = true;

        }

        public void ShowTrajectory(Vector3 startPos, Vector3 initialVelocity)
        {
            Vector3[] points = new Vector3[lineSegmentCount];
            points[0] = startPos;

            for (int i = 1; i < lineSegmentCount; i++)
            {
                float t = i * timeStep;
                Vector3 point = startPos + initialVelocity * t + 0.5f * Physics.gravity * t * t;

                // Optional: stop if hits something
                if (Physics.Raycast(points[i - 1], point - points[i - 1], out RaycastHit hit, (point - points[i - 1]).magnitude, collisionMask))
                {
                    points[i] = hit.point;
                    trajectoryLine.positionCount = i + 1;
                    trajectoryLine.SetPositions(points);
                    return;
                }

                points[i] = point;
            }

            trajectoryLine.positionCount = lineSegmentCount;
            trajectoryLine.SetPositions(points);
        }

        public void ResetBall()
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            transform.position = SpawnerFactory.GetPosition(HoopCenter.transform.position, Random.Range(1.2f, 1.5f));
            Vector3 direction = HoopCenter.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
            shot = false;
            dragging = false;
            trajectoryLine.positionCount = 0;
            onResetComplete?.Invoke(transform);
        }
    }
}

