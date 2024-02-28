using UnityEngine;

namespace FishingMinigame.Core.Fishing
{
    public class BobberController : MonoBehaviour
    {
        [Header("Bobber Settings")]
        [SerializeField] private LayerMask _waterMask;
        [SerializeField] private Transform _bobberModel;
        [SerializeField] private AnimationCurve _bobberAnimCurve;
        [SerializeField] private float _bobberAnimSpeed = 0.5f;
        [SerializeField] private float _bobberAnimForce = 0.05f;
        [SerializeField] private AnimationCurve _fishBiteAnimCurve;
        [SerializeField] private float _fishBiteAnimSpeed = 1f;
        [SerializeField] private float _fishBiteAnimForce = 0.1f;

        [SerializeField] private float _moveSpeed = 7f;
        [SerializeField] private float _newPointRadius = 20f;

        #region PRIVATE VARIABLES

        private FishingSystem _fishingSystem;
        private Rigidbody _bobberRB;

        private Vector3 _bobberModelInitialLocalPos;
        private Vector3 _targetPoint;

        private bool _inWater = false;
        private bool _isBite = false;
        private bool _fishIsCaught = false;

        #endregion

        private void Awake()
        {
            // Get the Rigidbody component
            _bobberRB = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Store the initial local position of the bobber model
            _bobberModelInitialLocalPos = _bobberModel.localPosition;
        }

        private void OnDrawGizmos()
        {
            // Draw a wire sphere for the target point in the editor
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_targetPoint, 0.2f);
        }

        private void Update()
        {
            // Control bobber animation if it's in water and not being bitten
            if (_inWater && !_isBite)
                ControlBobberAnimation();

            // Control fish behavior if it's caught
            if (_fishIsCaught)
                ControlFishBehavior();
        }

        private void ControlFishBehavior()
        {
            // Move the bobber towards the target point
            Vector3 moveDir = (_targetPoint - transform.position).normalized;
            transform.Translate(moveDir * _moveSpeed * Time.deltaTime);

            // If the bobber is close to the target point, select a new one
            float distance = Vector3.Distance(transform.position, _targetPoint);
            if (distance <= 1f)
                _targetPoint = GetTargetPoint(_newPointRadius, _waterMask);
        }

        private Vector3 GetTargetPoint(float radius, LayerMask waterMask)
        {
            // Get a random point within a radius and adjust it to avoid obstacles
            Vector2 point2D = Random.insideUnitCircle * radius;
            Vector3 point = new Vector3(point2D.x, 0f, point2D.y) + transform.position;

            RaycastHit hit;
            if (Physics.Linecast(transform.position, point, out hit, ~waterMask))
            {
                // If there's an obstacle, adjust the point to avoid it
                float offset = 1.5f;
                Vector3 offsetDir = hit.point - transform.position;
                Vector3 finalPoint = hit.point - offsetDir * offset;
                return finalPoint;
            }

            return point;
        }

        private void ControlBobberAnimation()
        {
            // Control bobber's vertical animation using animation curve
            Vector3 bobberNewPos = new Vector3(0f, _bobberModelInitialLocalPos.y + _bobberAnimCurve.Evaluate(Time.time * _bobberAnimSpeed) * _bobberAnimForce, 0f);
            _bobberModel.localPosition = bobberNewPos;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if ((_waterMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                // If the bobber collides with water, start fishing
                _inWater = true;
                _fishingSystem.StartFishing();
                _bobberRB.isKinematic = true; // Disable physics to keep the bobber floating
            }
            else
            {
                // If the bobber collides with something else, restart fishing
                _fishingSystem.RestartFishing();
            }
        }

        // Method to set the fishing system reference
        public void SetFishingSystem(FishingSystem fishingSystem) { _fishingSystem = fishingSystem; }

        // Method to start the fish biting animation
        public void StartFishBiteAnim()
        {
            _isBite = true;

            // Control bobber's vertical animation when fish is biting
            Vector3 bitePos = new Vector3(0f, _bobberModelInitialLocalPos.y + _fishBiteAnimCurve.Evaluate(Time.time * _fishBiteAnimSpeed) * _fishBiteAnimForce, 0f);
            _bobberModel.localPosition = bitePos;
        }

        // Method to end the fish biting animation
        public void EndFishBiteAnim() { _isBite = false; }

        // Method to handle fish being caught
        public void FishIsCaught()
        {
            // Select a new target point and set flags accordingly
            _targetPoint = GetTargetPoint(_newPointRadius, _waterMask);
            _isBite = false;
            _fishIsCaught = true;
        }
    }
}
