using UnityEngine;
using UnityEngine.Events;

namespace FabrizioConni.BasketChallenge.Score 
{
    public class FailSystem : MonoBehaviour
    {
        public UnityAction<int> onFail;
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
                onFail?.Invoke(0);

            else
                onFail?.Invoke(1);
        }
    }
}
