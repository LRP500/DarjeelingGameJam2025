using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerMovement : MonoBehaviour
    {
        [MinValue(0)]
        [SerializeField]
        private float _speed;
        
        [Vector2AsRange]
        [SerializeField]
        private Vector2 _horizontalLimits = new(0, 0);

        [Vector2AsRange]
        [SerializeField]
        private Vector2 _verticalLimits = new(0, 0);

        private PlayerInput _playerInput;

        private Camera _camera;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.actions.Enable();
            _camera = Camera.main;
        }

        private void Update()
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var worldPosition = _camera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = _camera.nearClipPlane;
            worldPosition.x = Mathf.Clamp(worldPosition.x, _horizontalLimits.x, _horizontalLimits.y);
            worldPosition.y = Mathf.Clamp(worldPosition.y, _verticalLimits.x, _verticalLimits.y);
            transform.position = worldPosition;
        }
    }
}
