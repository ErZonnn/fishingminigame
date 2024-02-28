using FishingMinigame.Core.Character;
using FishingMinigame.Core.UI;
using System.Collections;
using UnityEngine;

namespace FishingMinigame.Core.Fishing
{
    // Enum defining different states of the fishing process
    public enum FishingState
    {
        Default,
        Idle,
        Casting,
        WaitingForCatch,
        Reeling
    };

    public class FishingSystem : MonoBehaviour
    {
        [Header("Fishing Settings")]
        [SerializeField] private FishingRodController _fishingRod;
        [SerializeField] private Animator _handleFishingRodAnimator;
        [Space]
        [SerializeField] private float _maxCastingForce = 10f;
        [SerializeField] private float _castingForceRate = 1.5f;
        [SerializeField] private float _castingDelay = 1.5f;
        [SerializeField] private float _reelingWindow = 2f;
        [Space]
        [SerializeField] private GameObject _bobberPrefab;

        [Header("Debug")]
        [SerializeField] private FishingState _lastFishingState;
        [SerializeField] private FishingState _currentFishingState;
        [SerializeField] private float _currentCastingForce;

        #region PRIVATE VARIABLES

        private FishingQTE _fishingQTE;
        private FishingScoreUIManager _fishingScoreUIManager;
        private CastingUIManager _castingUIManager;
        private MovementSystem _movementSystem;

        private bool _unsuccessfulFishing = false;
        private bool _successfulFishing = false;
        private bool _isCast = false;
        private bool _startFishing = false;
        private bool _castInput;
        private bool _reelInput;

        private float _waitingTimer;
        private int _waitingTime;

        #endregion

        private void Awake()
        {
            // Initialize fishing-related components
            _fishingQTE = GetComponent<FishingQTE>();
            _fishingScoreUIManager = GetComponent<FishingScoreUIManager>();
            _movementSystem = GetComponent<MovementSystem>();
            _castingUIManager = GetComponent<CastingUIManager>();
        }

        private void Start()
        {
            // Set initial waiting time
            _waitingTime = Random.Range(5, 15);
        }

        private void OnEnable()
        {
            // Initialize fishing state when enabled
            UpdateState(FishingState.Idle);
        }
        
        private void OnDisable()
        {
            // Reset fishing state when disabled
            UpdateState(FishingState.Default);
        }

        // Update fishing state if it changes
        private void UpdateState(FishingState newFishingState)
        {
            if (_currentFishingState == newFishingState)
                return;

            _lastFishingState = _currentFishingState;
            _currentFishingState = newFishingState;
        }

        private void Update()
        {
            HandleInput();
            ControlFishingState();

            switch (_currentFishingState)
            {
                case FishingState.Default: return;
                case FishingState.Idle:

                    if (_currentCastingForce != 0 && !_isCast)
                        StartCoroutine(CastBobber(_currentCastingForce, _castingDelay, _fishingRod));

                    break;
                case FishingState.Casting: HandleCastingState(); break;
                case FishingState.WaitingForCatch: HandleWaitingForCatchState(); break;
                case FishingState.Reeling: HandleReelingState(); break;
            }
        }

        // Control the state of fishing based on player input
        private void ControlFishingState()
        {
            if (_castInput && !_isCast)
            {
                UpdateState(FishingState.Casting);
                return;
            }
            else if (!_startFishing)
            {
                UpdateState(FishingState.Idle);
                return;
            }
        }

        // Handle the casting state, increasing casting force and initiating casting when maximum force is reached
        private void HandleCastingState()
        {
            _currentCastingForce += _castingForceRate * Time.deltaTime;

            if (_currentCastingForce >= _maxCastingForce)
                StartCoroutine(CastBobber(_currentCastingForce, _castingDelay, _fishingRod));

            _castingUIManager.HandleCastingUI(_currentCastingForce, _maxCastingForce);
        }

        // Coroutine to cast the bobber after a delay
        private IEnumerator CastBobber(float currentCastingForce, float castingDelay, FishingRodController fishingRodSystem)
        {
            _isCast = true;
            _castingUIManager.RestartCastingBar();
            _handleFishingRodAnimator.SetBool("Casting", true);

            yield return new WaitForSeconds(castingDelay);
            
            _handleFishingRodAnimator.SetBool("Casting", false);

            Vector3 castingPos = fishingRodSystem.GetLineAttachment().position;
            Vector3 castingDir = transform.forward + Vector3.up;

            GameObject spawnedBobber = Instantiate(_bobberPrefab, castingPos, Quaternion.identity);
            spawnedBobber.GetComponent<Rigidbody>().AddForce(castingDir * currentCastingForce, ForceMode.Impulse);
            spawnedBobber.GetComponent<BobberController>().SetFishingSystem(this);

            fishingRodSystem.AddBobber(spawnedBobber.transform);
            fishingRodSystem.ChangeState(FishingRodState.WaitingForCatch);

            _currentCastingForce = 0f;
        }

        // Handle waiting for catch state, including timing and player input for reeling
        private void HandleWaitingForCatchState()
        {
            _movementSystem.enabled = false;

            BobberController bobberController = _fishingRod.GetBobber().GetComponent<BobberController>();
            _waitingTimer += Time.deltaTime;

            if (_waitingTimer >= _waitingTime)
            {
                bobberController.StartFishBiteAnim();

                if (_waitingTimer >= _waitingTime + _reelingWindow)
                {
                    bobberController.EndFishBiteAnim();
                    _waitingTimer = 0f;
                    _waitingTime = Random.Range(5, 15);
                }
                else if (_reelInput)
                {
                    _waitingTimer = 0f;
                    bobberController.FishIsCaught();
                    UpdateState(FishingState.Reeling);
                    _fishingRod.ChangeState(FishingRodState.Reeling);
                }
            }
            else if (_reelInput)
                RestartFishing();
        }

        // Handle reeling state, including QTE and outcomes
        private void HandleReelingState()
        {
            
            _fishingQTE.HandleFishingQTE(_reelInput, ref _successfulFishing, ref _unsuccessfulFishing);
            _handleFishingRodAnimator.SetBool("Reeling", _reelInput);

            if (_unsuccessfulFishing)
            {
                _fishingQTE.RestartFishingQTE();
                RestartFishing();
            }

            if (_successfulFishing)
            {
                _fishingScoreUIManager.AddFishingScore(Random.Range(20, 60));
                _fishingQTE.RestartFishingQTE();
                RestartFishing();
            }
        }

        // Handle player input for casting and reeling
        private void HandleInput()
        {
            
            _castInput = Input.GetKey(KeyCode.Mouse0);
            _reelInput = Input.GetKeyDown(KeyCode.Mouse1);
        }

        // Method to restart the fishing process
        public void RestartFishing()
        {
            _handleFishingRodAnimator.SetBool("Reeling", false);
            _movementSystem.enabled = true;
            _isCast = false;
            _startFishing = false;
            _successfulFishing = false;
            _unsuccessfulFishing = false;
            _fishingRod.RestartFishing();
            UpdateState(FishingState.Idle);
        }

        // Method to start the fishing process
        public void StartFishing()
        {
            _startFishing = true;
            UpdateState(FishingState.WaitingForCatch);
        }
    }
}