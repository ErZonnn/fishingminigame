using System.Collections;
using TMPro;
using UnityEngine;

namespace FishingMinigame.Core.UI
{
    public class FishingScoreUIManager : MonoBehaviour
    {
        [SerializeField] private int _fishingScore = 0;
        [SerializeField] private TextMeshProUGUI _fishingScoreUIText;
        [SerializeField] private TextMeshProUGUI _singleFishScoreUIText;
        [SerializeField] private GameObject _congratulationsUIObject;

        private void Update()
        {
            // Update the displayed total fishing score
            _fishingScoreUIText.text = _fishingScore.ToString();
        }

        // Coroutine to handle displaying the congratulations message and single fish score
        private IEnumerator HandleCongratulationsUIInfo(int fishScore)
        {
            _congratulationsUIObject.SetActive(true);
            _singleFishScoreUIText.text = "+" + fishScore.ToString();

            yield return new WaitForSeconds(5f);
            _congratulationsUIObject.SetActive(false);
        }

        // Method to add fishing score and trigger displaying congratulations message
        public void AddFishingScore(int fishScore)
        {
            _fishingScore += fishScore;
            StartCoroutine(HandleCongratulationsUIInfo(fishScore));
        }
    }
}
