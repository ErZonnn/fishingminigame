using UnityEngine;

namespace FishingMinigame.Core.Fishing
{
    public class FishingQTE : MonoBehaviour
    {
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;
        [SerializeField] private float _changeSpeed = 1f;
        [SerializeField] private float _successThreshold = 0.2f;
        [SerializeField] private float _successThresholdPos = 0.5f;

        #region PRIVATE VARIABLES

        private float _currentQTEValue = 0.5f;
        private float _fishingProgress = 0.2f;
        private float _curerntChargeSpeed;
        private float _currentSuccessThreshold;
        private float _currentSuccessThresholdPos;
        private bool isIncreasing = true;
        private bool _startQTE = false;

        #endregion

        private void Start()
        {
            // Initialize variables at the start
            _curerntChargeSpeed = _changeSpeed;
            _currentSuccessThreshold = _successThreshold;
            _currentSuccessThresholdPos = _successThresholdPos;
        }

        public void HandleFishingQTE(bool reelInput, ref bool successFishing, ref bool unsuccessFishing)
        {
            _startQTE = true;

            UpdateQTEValue();

            if (reelInput)
                CheckQTESuccess();

            if (_fishingProgress < 0f)
                unsuccessFishing = true;
            else if (_fishingProgress >= 1f)
                successFishing = true;
        }

        private void UpdateQTEValue()
        {
            // Update QTE value based on direction
            if (isIncreasing)
            {
                _currentQTEValue += _curerntChargeSpeed * Time.deltaTime;
                if (_currentQTEValue >= maxValue)
                {
                    _currentQTEValue = maxValue;
                    isIncreasing = false;
                }
            }
            else
            {
                _currentQTEValue -= _curerntChargeSpeed * Time.deltaTime;
                if (_currentQTEValue <= minValue)
                {
                    _currentQTEValue = minValue;
                    isIncreasing = true;
                }
            }
        }

        private void CheckQTESuccess()
        {
            // Check if QTE value falls within success threshold range
            if (_currentQTEValue >= _currentSuccessThresholdPos - (_currentSuccessThreshold / 2f) && _currentQTEValue <= _currentSuccessThresholdPos + (_currentSuccessThreshold / 2f))
            {
                // Update fishing progress and related parameters for successful QTE
                _fishingProgress += 0.2f;
                _curerntChargeSpeed += 0.1f;
                _currentSuccessThreshold -= 0.05f;

                _currentSuccessThresholdPos = Random.Range(0.2f, 0.8f);
            }
            else
            {
                // Update fishing progress and related parameters for unsuccessful QTE
                _fishingProgress -= 0.2f;
                _curerntChargeSpeed -= 0.1f;
                _currentSuccessThreshold += 0.05f;
            }
        }

        public void RestartFishingQTE()
        {
            // Reset QTE state and parameters
            _startQTE = false;
            _currentSuccessThreshold = _successThreshold;
            _currentSuccessThresholdPos = _successThresholdPos;
            _fishingProgress = 0.2f;
            _currentQTEValue = 0.5f;
            _curerntChargeSpeed = _changeSpeed;
        }

        // Getters for QTE status and parameters
        public bool GetQTEStatus() { return _startQTE; }
        public float GetSuccessThresholdPos() { return _currentSuccessThresholdPos; }
        public float GetSuccessThreshold() { return _currentSuccessThreshold; }
        public float GetCurrentQTEValue() { return _currentQTEValue; }
    }
}
