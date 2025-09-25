using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace FabrizioConni.BasketChallenge.TimerNamespace
{
    /// <summary>
    /// Timer class that manages a countdown timer, updating UI elements and invoking events on timer start, update, and end.
    /// </summary>
    public class Timer : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private float timeLimit = 60f; // The maximum time (in seconds) for the countdown timer.
        [SerializeField]
        private string timerName; // The name of the GameObject containing the UI_Timer component.
        [SerializeField]
        private bool updateText = false; // If true, updates the timer text UI element.
        [SerializeField]
        private TMP_Text timerText; // Reference to the TextMeshPro text component for displaying the timer.
        [SerializeField]
        private UI_TimeLeft uiTimeLeft; // Reference to a custom UI component for showing the time left.
        #endregion

        #region Private Fields
        private float timeRemaining; // The current time left on the timer.
        private bool timerIsRunning = false; // Indicates if the timer is currently running.
        private UI_Timer uTimer; // Reference to the UI_Timer component for updating the timer bar/progress.
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current time remaining on the timer.
        /// </summary>
        public float TimeRemaning { get { return timeRemaining; } }
        #endregion

        #region Actions
        public UnityAction OnTimerEnd; // Event invoked when the timer ends.
        public UnityAction OnTimerStart; // Event invoked when the timer starts.
        public UnityAction<float> OnTimerUpdatePerc; // Event invoked when the timer updates, passing the percentage left.
        #endregion

        #region Monobehaviour Callbacks
        /// <summary>
        /// Unity Start method. Initializes references to UI components based on serialized fields.
        /// </summary>
        void Start()
        {
            // If no timer name is set, do nothing.
            if (timerName == "") return;

            // Find the UI_Timer component by name and get its reference.
            uTimer = GameObject.Find(timerName).GetComponent<UI_Timer>();

            // If updateText is enabled, find the timer text UI element and get its reference.
            if (updateText)
            {
                timerText = GameObject.Find("timer_text").GetComponent<TMP_Text>();
            }
        }

        /// <summary>
        /// Unity Update method. Handles the countdown logic and triggers events when the timer updates or ends.
        /// </summary>
        void Update()
        {
            // Only update if the timer is running.
            if (timerIsRunning)
            {
                // If there is still time remaining, decrement it and update the UI.
                if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime;
                    UpdateTimerInternal();
                }
                else
                {
                    // If time has run out, set to zero, stop the timer, and invoke the end event.
                    timeRemaining = 0;
                    timerIsRunning = false;
                    OnTimerEnd?.Invoke();
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the timer UI elements and invokes the update event with the current percentage.
        /// </summary>
        private void UpdateTimerInternal()
        {
            float timerperc = timeRemaining / timeLimit; // Calculate the percentage of time left.

            // Update the UI timer bar if available.
            if (uTimer)
                uTimer.SetProgress(timerperc);

            // Update the timer text if enabled.
            if (updateText)
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

            // Update the custom UI_TimeLeft component if available.
            if (uiTimeLeft != null)
                uiTimeLeft.UpdateTimeLeft(timeRemaining);

            // Invoke the update event with the current percentage.
            OnTimerUpdatePerc?.Invoke(timerperc);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the timer, resetting the time remaining and setting the running flag.
        /// </summary>
        public void StartTimer()
        {
            timeRemaining = timeLimit;
            timerIsRunning = true;
        }

        /// <summary>
        /// Stops the timer, sets time to zero, updates the UI, and invokes the end event.
        /// </summary>
        public void StopTimer()
        {
            timeRemaining = 0;
            timerIsRunning = false;
            UpdateTimerInternal();
            OnTimerEnd?.Invoke();
        }

        /// <summary>
        /// Updates the timer bar to a specific percentage if the timer is not running.
        /// </summary>
        /// <param name="newTimePerc">The new percentage to set the timer bar to.</param>
        public void UpdateTimerBar(float newTimePerc)
        {
            if (timerIsRunning) return;
            uTimer.SetProgress(newTimePerc);
        }
        #endregion
    }
}
