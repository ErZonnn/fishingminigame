using UnityEngine;

namespace FishingMinigame.Core.Character
{
    public class CameraSystem : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float _sensitivity = 1.5f;
        [SerializeField] private float _lookInertia = 15f;
        [SerializeField] private Vector2 _verticalRotClamp;
        [SerializeField] private AnimationCurve _headBobbingAnimCurve;
        [SerializeField] private float _headBobbingSpeed = 1f;
        [SerializeField] private float _headBobbingForce = 0.1f;

        #region PRIVATE VARIABLES

        private Transform _character;

        private Vector2 _lookInput;

        private Vector2 _smoothedCamRotation;
        private Vector2 _camRotation;

        private float _initialYCamLocalPos;

        #endregion

        private void Awake()
        {
            _character = transform.parent;
        }

        private void Start()
        {
            _initialYCamLocalPos = transform.localPosition.y;
        }

        private void Update()
        {
            HandleInput();
            HandleHeadBobbing();
        }

        private void HandleHeadBobbing()
        {
            float bobbingValue = _initialYCamLocalPos + _headBobbingAnimCurve.Evaluate(Time.time * _headBobbingSpeed) * _headBobbingForce;
            Vector3 headBobbing = new Vector3(transform.localPosition.x, bobbingValue, transform.localPosition.z);

            transform.localPosition = headBobbing;
        }

        private void LateUpdate()
        {
            HandleCameraRotation();
        }

        private void HandleCameraRotation()
        {
            _camRotation.x -= _lookInput.y * _sensitivity;
            _camRotation.x = Mathf.Clamp(_camRotation.x, _verticalRotClamp.x, _verticalRotClamp.y);
            _camRotation.y += _lookInput.x * _sensitivity;

            _smoothedCamRotation = Vector2.Lerp(_smoothedCamRotation, _camRotation, _lookInertia * Time.fixedDeltaTime);

            _character.rotation = Quaternion.Euler(0f, _smoothedCamRotation.y, 0f);
            transform.rotation = Quaternion.Euler(_smoothedCamRotation.x, _character.rotation.eulerAngles.y, 0f);
        }

        private void HandleInput()
        {
            _lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
    }
}
