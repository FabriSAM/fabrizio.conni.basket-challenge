using log4net.Util;
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
        public float smoothSpeed = 0.125f; // Smoothing speed for camera movement
        #endregion

        #region Private Fields
        private GameObject player; // Reference to the player's transform
        private Vector3 offset; // Offset from the player
        private Vector3 HoopLocation;
        #endregion

        #region Monobehaviour Callbacks
        void FixedUpdate()
        {
            if (player == null) return;

            Vector3 lookingLocation = (player.transform.position + HoopLocation) * 0.5f;
            transform.LookAt(lookingLocation); // Optional: Make the camera look at the player
        }
        #endregion

        #region Public Methods
        public void SetPlayer(GameObject player)
        {
            if (player == null) return;
            print($"Player is {player}");
            this.player = player;
            //offset = transform.position - player.transform.position;

            offset = player.transform.InverseTransformPoint(transform.position);
            print($"Offset is {offset}");
        }

        public void SetHoopPosition(Vector3 location)
        {
            HoopLocation = location;
        }

        public void SetPosition()
        {
            transform.position = player.transform.TransformPoint(offset);
            transform.rotation = player.transform.rotation;
        }
        #endregion
    }
}

