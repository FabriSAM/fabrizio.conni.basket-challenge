using System.Net;
using UnityEngine;

namespace FabrizioConni.BasketChallenge.Camera
{
    /// <summary>
    /// FixedCamera class that follows a player GameObject with a fixed offset.
    /// The camera smoothly follows the player and can be set to look at a specific location.
    /// </summary>
    public class FixedCamera : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        public float smoothSpeed = 2f; // Smoothing speed for camera movement
        #endregion

        #region Private Fields
        private GameObject player; // Reference to the player's transform
        private Vector3 offset; // Offset from the player
        private Vector3 HoopLocation;
        private bool playerIsShooting;
        #endregion
        void FixedUpdate()
        {
            if (player == null) return;

            // Punto intermedio tra palla e canestro
            Vector3 midPoint = (player.transform.position + HoopLocation) * 0.5f;

            // Guarda sempre il punto intermedio
            transform.LookAt(midPoint);

            if (!playerIsShooting) return;

            // Direzione dal canestro verso la palla
            Vector3 dir = (player.transform.position - HoopLocation).normalized;

            // Posizione desiderata: dietro la palla, leggermente sopra
            Vector3 desiredPosition = player.transform.position + dir * 1 + Vector3.up * 0.2f;

            // Calcola la distanza tra la camera e la posizione desiderata
            float distance = Vector3.Distance(transform.position, HoopLocation);

            Debug.Log(distance);

            // Se la distanza è maggiore di una soglia, effettua il lerp, altrimenti posiziona direttamente
            float lerpThreshold = 1.5f;
            if (distance > lerpThreshold)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            }
            
        }
        #region Public Method
        public void SetPlayer(GameObject player)
        {
            if (player == null) return;
            this.player = player;
            offset = player.transform.InverseTransformPoint(transform.position);
        }

        public void SetHoopPosition(Vector3 location)
        {
            HoopLocation = location;
        }

        public void SetShooting()
        {
            playerIsShooting = true;
        }

        public void SetPosition()
        {
            playerIsShooting = false;
            transform.position = player.transform.TransformPoint(offset);
            transform.rotation = player.transform.rotation;
        }
        #endregion
    }
}

