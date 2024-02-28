using UnityEngine;

namespace FishingMinigame.Core.UI
{
    public class CastingUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _castingUIObject;
        [SerializeField] private Transform _castingUIBar;

        // Method to reset the casting bar UI
        public void RestartCastingBar()
        {
            _castingUIObject.SetActive(false);
            _castingUIBar.localScale = new Vector3(_castingUIBar.localScale.x, 0f, _castingUIBar.localScale.z);
        }

        // Method to handle the casting UI based on the current force and maximum force
        public void HandleCastingUI(float currentForce, float maxForce)
        {
            _castingUIObject.SetActive(true);

            float barLocalScale = MapForceToUISize(currentForce, maxForce);
            _castingUIBar.localScale = new Vector3(_castingUIBar.localScale.x, barLocalScale, _castingUIBar.localScale.z);
        }

        // Method to map the force value to the size of the UI bar
        private static float MapForceToUISize(float currentForce, float maxForce)
        {
            float x = Mathf.InverseLerp(0f, maxForce, currentForce);
            float value = Mathf.Lerp(0f, 1f, x);

            return value;
        }
    }
}
