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

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _playerInput.actions.Enable();
        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        private void Update()
        {
            var direction = new Vector3(_moveInput.x, _moveInput.y);
            var movement = direction * _speed * Time.deltaTime;
            gameObject.transform.position += movement;
        }
    }
}
