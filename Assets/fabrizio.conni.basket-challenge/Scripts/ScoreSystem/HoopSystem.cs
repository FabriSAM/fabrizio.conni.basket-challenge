using UnityEngine;
using UnityEngine.Events;

namespace FabrizioConni.BasketChallenge.Score
{
    /// <summary>
    /// The HoopSystem class detects when a ball successfully passes through the hoop.
    /// It differentiates between player and enemy scores based on the colliding object's tag
    /// and invokes an event to notify other components of the score.
    /// </summary>
    public class HoopSystem : MonoBehaviour
    {
        public UnityAction<int> hitHoop;

        private void OnCollisionEnter(Collision collision)
        {
            int index = -1;

            switch (collision.gameObject.tag)
            {
                case "Player":
                    index = 0;
                    break;
                case "Enemy":
                    index = 1;
                    break;
                default:
                    break;
            }
            hitHoop?.Invoke(index);
        }
    }
}

