using UnityEngine;
using DTT.UI.ProceduralUI;
using LevelDesign.Systems;

namespace LevelDesign.UI
{
    public class StaminaBarUI : MonoBehaviour
    {
        [Header("Scene Refs")]
        [SerializeField] private RoundedImage healthBar;
        [SerializeField] private RoundedImage healthBarBacker;
        [SerializeField] private Stamina stamina;

        private void FixedUpdate() {
            transform.LookAt(Camera.main.transform.position, Vector3.up);

            healthBar.fillAmount = Mathf.Clamp01((float)stamina.Current / stamina.Max);

            if(stamina.Current <= 0) {
                stamina.gameObject.SetActive(false);
                stamina.gameObject.SetActive(false);
            }
            else {
                stamina.gameObject.SetActive(true);
                stamina.gameObject.SetActive(true);
            }
        }
    }
}
