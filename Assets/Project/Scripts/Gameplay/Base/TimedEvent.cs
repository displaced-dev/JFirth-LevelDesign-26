using UnityEngine;
using UnityEngine.Events;

namespace LevelDesign.Gameplay
{
    public class TimedEvent : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private UnityEvent startEvent;
        [SerializeField] private UnityEvent endEvent;
        [SerializeField] private float timeToWait;

        private float invokeTime;
        private bool running;

        public void Update() {
            if(Time.time >= invokeTime + timeToWait && running) {
                running = false;
                endEvent?.Invoke();
            }
        }
        
        public void StartSequence() {
            if(running) { return; }

            startEvent?.Invoke();
            invokeTime = Time.time;
            running = true;
        }
    }
}
