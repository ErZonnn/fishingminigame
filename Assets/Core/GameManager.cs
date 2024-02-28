using UnityEngine;

namespace FishingMinigame.Core
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
