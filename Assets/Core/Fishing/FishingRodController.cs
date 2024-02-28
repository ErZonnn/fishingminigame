using UnityEngine;

namespace FishingMinigame.Core.Fishing
{
    // Enum defining different states of the fishing rod
    public enum FishingRodState
    {
        Default,
        Idle,
        WaitingForCatch,
        Reeling,
    };

    [RequireComponent(typeof(Animator), typeof(LineRenderer))]
    public class FishingRodController : MonoBehaviour
    {
        [Header("Fishing Rod Settings")]
        [SerializeField] private Transform _lineAttachment;
        [SerializeField] private Vector2 _lineResolutionRange = new Vector2(50, 10);
        [SerializeField] private float _lineSimulateGravity = -1f;
        [SerializeField] private Vector2 _bendingRange = new Vector2(-90, 90);
        [SerializeField] private float _bendingSpeed = 10f;

        [Header("Debug")]
        [SerializeField] private FishingRodState _lastFishingState;
        [SerializeField] private FishingRodState _currentFishingState;
        [SerializeField] private Transform _bobber;

        private Animator _fishingRodAnimator;
        private LineRenderer _fishingLineRenderer;
        private Vector2 _bendingAngle;
        private float _smoothedSimGravity;

        private void Awake()
        {
            _fishingRodAnimator = GetComponent<Animator>();
            _fishingLineRenderer = GetComponent<LineRenderer>();
        }

        private void OnEnable()
        {
            UpdateState(FishingRodState.Idle);
        }

        private void OnDisable()
        {
            UpdateState(FishingRodState.Default);
        }

        private void UpdateState(FishingRodState newFishingRodState)
        {
            if (_currentFishingState == newFishingRodState)
                return;

            _lastFishingState = _currentFishingState;
            _currentFishingState = newFishingRodState;
        }

        private void LateUpdate()
        {
            switch (_currentFishingState)
            {
                case FishingRodState.Default:
                    return;
                case FishingRodState.Idle:
                    HandleIdleState();
                    break;
                case FishingRodState.WaitingForCatch:
                    ControlFishingLine(_currentFishingState);
                    break;
                case FishingRodState.Reeling:
                    ControlFishingRodBending(_currentFishingState);
                    ControlFishingLine(_currentFishingState);
                    break;
            }
        }

        private void HandleIdleState()
        {
            ClearFishingLine();
            ControlFishingRodBending(_currentFishingState);
        }

        private void ClearFishingLine()
        {
            _fishingLineRenderer.positionCount = 0;
        }

        // Method to control the fishing line based on its state
        private void ControlFishingLine(FishingRodState fishingRodState)
        {
            if (_bobber == null)
            {
                _fishingLineRenderer.positionCount = 0;
                return;
            }

            bool lootCaught = fishingRodState == FishingRodState.Reeling;

            float distance = Vector3.Distance(_lineAttachment.position, _bobber.position);
            int resolution = CalculateLineResolution(distance, _lineResolutionRange);

            _fishingLineRenderer.positionCount = resolution;

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 position = CalculatePointOnCurve(t, _lineAttachment.position, _bobber.position, lootCaught, _lineSimulateGravity);
                _fishingLineRenderer.SetPosition(i, position);
            }
        }

        // Method to calculate the resolution of the fishing line based on distance
        private static int CalculateLineResolution(float distance, Vector2 resolutionRange)
        {
            float minDis = 1f;
            float maxDis = 20f;

            float x = Mathf.InverseLerp(minDis, maxDis, distance);
            float value = Mathf.Lerp(resolutionRange.x, resolutionRange.y, x);

            return (int)value;
        }

        // Method to calculate a point on the curve for the fishing line
        private Vector3 CalculatePointOnCurve(float t, Vector3 attachmentPos, Vector3 bobberPos, bool lootCaught, float simulateGravity)
        {
            Vector3 pointA = attachmentPos;
            Vector3 pointB = bobberPos;

            float lineTensionSpeed = 2f;
            _smoothedSimGravity = Mathf.Lerp(_smoothedSimGravity, lootCaught ? 0f : simulateGravity, Time.deltaTime * lineTensionSpeed);

            Vector3 controlPoint = Vector3.Lerp(pointA, pointB, 0.5f) + Vector3.up * _smoothedSimGravity;
            Vector3 pointOnCurve = CalculateBezier(pointA, controlPoint, pointB, t, bobberPos);

            return pointOnCurve;
        }

        // Method to calculate a point on Bezier curve
        private Vector3 CalculateBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t, Vector3 floatPosition)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 point = uuu * p0;
            point += 3 * uu * t * p1;
            point += 3 * u * tt * p2;
            point += ttt * floatPosition;

            return point;
        }

        // Method to control the bending of the fishing rod
        private void ControlFishingRodBending(FishingRodState fishingRodState)
        {
            Vector2 bendingAngle = Vector2.zero;

            if (fishingRodState != FishingRodState.Idle)
                bendingAngle = CalculateAngle(_bobber.position);

            _bendingAngle = Vector2.Lerp(_bendingAngle, bendingAngle, _bendingSpeed * Time.deltaTime);

            float verticalBend = RemapAngleToBend(_bendingAngle.y, _bendingRange.x, _bendingRange.y);
            float horizontalBend = RemapAngleToBend(_bendingAngle.x, _bendingRange.x, _bendingRange.y);

            _fishingRodAnimator.SetFloat("VerticalBend", verticalBend);
            _fishingRodAnimator.SetFloat("HorizontalBend", horizontalBend);
        }

        // Method to calculate the angle between the fishing rod and the bobber
        private Vector2 CalculateAngle(Vector3 bobberPos)
        {
            Vector3 angleDir = transform.position - bobberPos;

            float verticalAngle = Vector3.Angle(angleDir, transform.up) - 90f;
            float horizontalAngle = Vector3.Angle(angleDir, transform.right) - 90f;

            return new Vector2(horizontalAngle, verticalAngle);
        }

        // Method to remap the angle to bending value
        private static float RemapAngleToBend(float angle, float minAngle, float maxAngle)
        {
            float x = Mathf.InverseLerp(minAngle, maxAngle, angle);
            float value = Mathf.Lerp(-1f, 1f, x);

            return value;
        }

        // Public methods for accessing and modifying rod and bobber properties
        public Transform GetLineAttachment() { return _lineAttachment; }
        public GameObject GetBobber() { return _bobber.gameObject; }
        public void AddBobber(Transform bobber) { _bobber = bobber; }
        public void ChangeState(FishingRodState fishingRodState) { UpdateState(fishingRodState); }

        // Method to restart the fishing process
        public void RestartFishing()
        {
            UpdateState(FishingRodState.Idle);
            Destroy(_bobber.gameObject);
            _bobber = null;
        }
    }
}
