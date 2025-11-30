using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DarjeelingGameJam.Animals.Abilities
{
    public class MouseSelection : MonoBehaviour
    {
        [SerializeField]
        private LayerMask _selectableLayers;

        private Camera _camera;
        private MoveToMouse _moveToMouse;
        private MovementBehaviour[] _movementBehaviours;

        private bool _selected;
        
        private void Awake()
        {
            _camera = Camera.main;
            _moveToMouse = GetComponent<MoveToMouse>();
            _movementBehaviours = GetComponents<MovementBehaviour>();
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                DetectClick();
            }
        }

        private void DetectClick()
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var worldPos = _camera.ScreenToWorldPoint(mousePosition);
            
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, _selectableLayers);

            if (hit.transform != transform)
            {
                return;
            }
            
            // Disable all movement behaviours and enable MoveToMouse
            _selected = !_selected;
            _movementBehaviours.ForEach(x => x.enabled = !_selected);
            _moveToMouse.enabled = _selected;
        }
    }
}