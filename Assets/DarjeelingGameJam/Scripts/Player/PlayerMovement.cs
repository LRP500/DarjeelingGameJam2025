using Sirenix.OdinInspector;
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
            transform.position = worldPosition;
        }
    }
}
