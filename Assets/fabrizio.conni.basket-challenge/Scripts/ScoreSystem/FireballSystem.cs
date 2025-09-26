using UnityEngine;
using UnityEngine.Events;
using FabrizioConni.BasketChallenge.TimerNamespace;
using System.Collections;

namespace FabrizioConni.BasketChallenge.Score
{
    /// <summary>
    /// The FireballSystem class manages the fireball bonus mechanic in the basketball challenge game.
    /// It tracks points towards activating the fireball state, handles activation and deactivation,
    /// and communicates with a Timer component to manage the duration of the fireball state.
    /// </summary>
    public class FireballSystem : MonoBehaviour
    {
        #region Serialized Fields
        /// <summary>
        /// The maximum number of points required to activate the fireball state.
        /// When currentPoint reaches or exceeds this value, the fireball state is triggered.
        /// </summary>
        [SerializeField]
        private float maxPoint = 5f;

        /// <summary>
        /// Reference to the Timer component that manages the duration of the fireball state.
        /// The timer is started when the fireball state is activated and stopped when it ends.
        /// </summary>
        [SerializeField]
        private Timer fireballTimer;
        #endregion

        #region Private Fields
        /// <summary>
        /// The current number of points accumulated towards activating the fireball state.
        /// This value increases as the player earns points and is reset when the fireball state ends.
        /// </summary>
        private float currentPoint;
        #endregion

        #region Properties
        /// <summary>
        /// Indicates whether the fireball state is currently active.
        /// This property is set to true when the fireball is activated and false when it ends.
        /// </summary>
        public bool IsFireballActive { get; private set; }
        #endregion

        #region Actions
        /// <summary>
        /// UnityAction event invoked when the fireball state is activated.
        /// Other components can subscribe to this event to react to the activation.
        /// </summary>
        public UnityAction<bool> onFireballActivate;
        #endregion

        #region Monobehaviour Callbacks
        /// <summary>
        /// Unity's Start method. Subscribes the TimerEnded method to the fireballTimer's OnTimerEnd event.
        /// This ensures that when the timer ends, the fireball state is properly deactivated.
        /// </summary>
        private void Start()
        {
            StartCoroutine(SubscribeDelayed());
        }

        private IEnumerator SubscribeDelayed()
        {
            while (fireballTimer == null)
                yield return null;

            fireballTimer.OnTimerEnd += TimerEnded;
        }

        private void OnDestroy()
        {
            fireballTimer.OnTimerEnd -= TimerEnded;
        }
        /// <summary>
        /// Unity's Update method. Checks each frame if the fireball state should be activated.
        /// If the fireball is not active and the current points reach the maximum, it activates the fireball state,
        /// invokes the activation event, and starts the timer.
        /// </summary>
        void Update()
        {
            if (IsFireballActive) return;
            if (currentPoint >= maxPoint)
            {
                IsFireballActive = true;
                onFireballActivate?.Invoke(IsFireballActive);
                fireballTimer.StartTimer();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Callback method invoked when the fireball timer ends.
        /// Deactivates the fireball state and resets the current points to zero.
        /// </summary>
        private void TimerEnded()
        {
            IsFireballActive = false;
            currentPoint = 0f;
            onFireballActivate?.Invoke(IsFireballActive);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds points towards activating the fireball state.
        /// If the fireball is already active or the points to add are zero, the method does nothing.
        /// Otherwise, it increases currentPoint (clamped to maxPoint) and updates the timer bar UI.
        /// </summary>
        /// <param name="pointToAdd">The number of points to add towards the fireball activation.</param>
        public void AddFireballPoint(float pointToAdd)
        {
            if (pointToAdd == 0f || IsFireballActive) return;

            currentPoint = Mathf.Clamp(currentPoint + pointToAdd, 0f, maxPoint);
            fireballTimer.UpdateTimerBar(currentPoint / maxPoint);
        }

        /// <summary>
        /// Resets the fireball points and deactivates the fireball state if it is currently active.
        /// Stops the timer, sets IsFireballActive to false, and resets currentPoint to zero.
        /// </summary>
        public void ResetFireballPoints()
        {
            if (!IsFireballActive) return;

            fireballTimer.StopTimer();
            IsFireballActive = false;
            currentPoint = 0f;
        }
        #endregion
    }

}
