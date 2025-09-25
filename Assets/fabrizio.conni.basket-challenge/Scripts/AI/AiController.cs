using System;
using System.Collections.Generic;
using UnityEngine;

namespace FabrizioConni.BasketChallenge.AI
{
    #region Structs
    // Struct representing a named shooting ability with its difficulty settings
    [Serializable]
    public struct ShootAbility
    {
        public string DifficoultyName; // Name of the difficulty level
        public AiDifficoulty Difficoulty; // Difficulty parameters
    }
    #endregion

    // Main AI controller for the basketball challenge
    [RequireComponent(typeof(Rigidbody))]
    public class AiController : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private GameObject HoopCenter; // Reference to the center of the basketball hoop
        [SerializeField]
        private List<ShootAbility> difficoulties; // List of available shooting difficulties
        #endregion

        #region Private Fields
        private AiDifficoulty currentDifficolutySelected; // Currently selected difficulty parameters
        private Rigidbody rb; // Rigidbody component for physics interactions
        private bool hasShot = true; // Flag to check if the AI has already shot
        private float timeToShot = 1.0f; // Time interval before shooting
        private float currentTime = 0.0f; // Timer for shooting
        #endregion

        #region Properties
        public string Difficoulty { get; private set; } // Name of the current difficulty
        #endregion

        #region Monobehaviour Callbacks
        // Called when the script instance is being loaded
        private void Awake()
        {
            rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
            rb.isKinematic = true; // Set Rigidbody to kinematic to prevent physics simulation
        }

        // Called once per frame
        private void Update()
        {
            if (hasShot) return; // If already shot, do nothing

            currentTime += Time.deltaTime; // Increment timer

            if (currentTime >= timeToShot)
            {
                Shoot(); // Perform the shot
                currentTime = 0.0f; // Reset timer
            }
        }
        #endregion

        #region Private Methods
        // Calculates the force vector needed to shoot the ball towards the hoop
        private void ShootTrajectory(ref Vector3 force)
        {
            Vector3 targetPos = HoopCenter.transform.position; // Target position (hoop)
            Vector3 startPos = transform.position; // Start position (ball)

            // Calculate horizontal distance (XZ plane)
            Vector3 directionXZ = new Vector3(targetPos.x - startPos.x, 0, targetPos.z - startPos.z);
            float d = directionXZ.magnitude;

            // Calculate height difference
            float h = targetPos.y - startPos.y;

            // Chosen angle for the shot (in radians)
            float angle = 75f * Mathf.Deg2Rad;

            // Use positive gravity value
            float g = Mathf.Abs(Physics.gravity.y);

            // Calculate the squared velocity needed using projectile motion formula
            float v2 = (g * d * d) / (2 * (d * Mathf.Tan(angle) - h) * Mathf.Cos(angle) * Mathf.Cos(angle));
            if (v2 <= 0f) return; // No valid solution, exit

            float v = Mathf.Sqrt(v2); // Initial velocity magnitude

            // Normalized direction on XZ plane
            Vector3 dirXZ = directionXZ.normalized;

            // Compose the initial velocity vector, adding arc height from difficulty
            Vector3 velocity = dirXZ * v * Mathf.Cos(angle) + Vector3.up * (v * Mathf.Sin(angle) + currentDifficolutySelected.arcHeight);

            force = velocity; // Set the output force
        }

        // Executes the shot by applying force to the Rigidbody
        private void Shoot()
        {
            Vector3 force = Vector3.zero;
            ShootTrajectory(ref force); // Calculate the force vector

            // Add random inaccuracy based on difficulty
            force.x += UnityEngine.Random.Range(-1f, 1f) * (1f - currentDifficolutySelected.accuracy);
            force.z += UnityEngine.Random.Range(-1f, 1f) * (1f - currentDifficolutySelected.accuracy);

            // Apply the force to the Rigidbody to simulate the shot
            rb.isKinematic = false; // Enable physics
            rb.velocity = Vector3.zero; // Reset velocity
            rb.angularVelocity = Vector3.zero; // Reset angular velocity
            rb.AddForce(force, ForceMode.VelocityChange); // Apply calculated force
            hasShot = true; // Mark as shot
        }
        #endregion

        #region Public Methods
        // Resets the ball to a new position and prepares for the next shot
        public void ResetBall()
        {
            hasShot = false; // Allow shooting again
            rb.isKinematic = true; // Disable physics
            rb.velocity = Vector3.zero; // Reset velocity
            rb.angularVelocity = Vector3.zero; // Reset angular velocity

            // Set a new random position around the hoop using SpawnerFactory
            transform.position = SpawnerFactory.GetPosition(HoopCenter.transform.position, UnityEngine.Random.Range(1.2f, 1.5f));
            Vector3 direction = HoopCenter.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction); // Face the hoop
            transform.rotation = rotation;
        }

        // Initializes the AI controller with a specific difficulty
        public void Init(string diffcoulty)
        {
            Difficoulty = diffcoulty; // Set the difficulty name
            foreach (var value in difficoulties)
            {
                if (value.DifficoultyName == Difficoulty)
                {
                    currentDifficolutySelected = value.Difficoulty; // Set the selected difficulty parameters
                    break;
                }
            }
            hasShot = false; // Ready to shoot
        }
        #endregion
    }
}
