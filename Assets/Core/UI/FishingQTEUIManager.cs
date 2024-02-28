using FishingMinigame.Core.Fishing;
using UnityEngine;
using UnityEngine.UI;

namespace FishingMinigame.Core.UI
{
    public class FishingQTEUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _QTEUIObject;
        [SerializeField] private Image _successThresholdUI;
        [SerializeField] private Image _QTEValueUI;
        [SerializeField] private int _QTEBackgroundWidth = 500;

        private FishingQTE _fishingQTE;
        private Vector3 _QTEValueInitialLocalPos;

        private void Awake()
        {
            _fishingQTE = GetComponent<FishingQTE>();
        }

        private void Start()
        {
            _QTEValueInitialLocalPos = _QTEValueUI.rectTransform.localPosition;
        }

        private void Update()
        {
            // Check if the QTE is active
            if (_fishingQTE.GetQTEStatus())
            {
                _QTEUIObject.SetActive(true);
                ControlQTEUI();
            }
            else
                _QTEUIObject.SetActive(false);
        }

        // Method to control the appearance of the QTE UI
        private void ControlQTEUI()
        {
            float successThreshold = _fishingQTE.GetSuccessThreshold();
            float successThresholdPos = _fishingQTE.GetSuccessThresholdPos();
            float currentQTEValue = _fishingQTE.GetCurrentQTEValue();

            float successThresholdUISize = RemapUISize(successThreshold, _QTEBackgroundWidth);
            float successThresholdUIPos = RemapUIPos(successThresholdPos, _QTEBackgroundWidth);

            _successThresholdUI.rectTransform.sizeDelta = new Vector2(successThresholdUISize, _successThresholdUI.rectTransform.sizeDelta.y);
            _successThresholdUI.rectTransform.localPosition = new Vector3(successThresholdUIPos, 0f, 0f);

            float currentQTEValuePos = RemapUIPos(currentQTEValue, _QTEBackgroundWidth);
            _QTEValueUI.rectTransform.localPosition = new Vector3(_QTEValueInitialLocalPos.x + currentQTEValuePos, _QTEValueInitialLocalPos.y, _QTEValueInitialLocalPos.z);
        }

        // Method to remap the size of UI elements
        private static float RemapUISize(float size, float QTEBackgroundWidth)
        {
            float value = Mathf.Lerp(0f, QTEBackgroundWidth, size);
            return value;
        }

        // Method to remap the position of UI elements
        private static float RemapUIPos(float pos, float QTEBackgroundWidth)
        {
            float value = Mathf.Lerp(-QTEBackgroundWidth / 2f, QTEBackgroundWidth / 2f, pos);
            return value;
        }
    }
}
