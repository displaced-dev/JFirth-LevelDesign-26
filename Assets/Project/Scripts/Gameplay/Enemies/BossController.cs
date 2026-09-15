using System.Collections;
using UnityEngine;

namespace LevelDesign.Gameplay.Enemies
{
    public class BossController : MonoBehaviour
    {
        [SerializeField] private GameObject keyFab;
        [SerializeField] private GameObject keySpawnLocal;
        [Space]
        [SerializeField] private GameObject shield;
        [Space]
        [SerializeField] private TimedEvent timedEvent;
        private int phasesComplete;
        private int enemiesDead;

        private Coroutine shrinkRoutine;

        public void ShrinkPhase()
        {
            phasesComplete++;

            if (phasesComplete == 1)
            {
                StartShrink(0.42f, 1f);
            }
            if (phasesComplete == 2)
            {
                StartShrink(0.11f, 1f);
            }
        }

        private void StartShrink(float targetY, float duration)
        {
            if (shrinkRoutine != null)
            {
                StopCoroutine(shrinkRoutine);
            }
            shrinkRoutine = StartCoroutine(ShrinkYTo(targetY, duration));
        }

        private IEnumerator ShrinkYTo(float targetY, float duration)
        {
            Vector3 startScale = shield.transform.localScale;
            Vector3 targetScale = new Vector3(startScale.x, targetY, startScale.z);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                shield.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            shield.transform.localScale = targetScale;
        }

        public void EnemyKilled()
        {
            enemiesDead++;

            if (enemiesDead == 4)
            {
                Instantiate(keyFab, keySpawnLocal.transform);
                timedEvent.StartSequence();
            }
        }
    }
}