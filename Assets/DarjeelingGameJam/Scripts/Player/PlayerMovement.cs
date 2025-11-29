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
        private Vector2 _moveInput;

        private Camera _camera;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.actions.Enable();

            _camera = Camera.main;

        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        private void Update()
        {

            gameObject.transform.position = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
    }
}
