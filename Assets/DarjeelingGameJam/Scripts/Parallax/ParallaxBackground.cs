using System;
using UnityEngine;

namespace DarjeelingGameJam.Parallax
{
    public class ParallaxBackground : MonoBehaviour
    {
        private Camera _camera;
        private ParallaxLayer[] _layers;
        private Vector3 _previousCameraPosition;

        private void Awake()
        {
            _camera = Camera.main;
            _layers = GetComponentsInChildren<ParallaxLayer>();
        }

        private void Start()
        {
            _previousCameraPosition = _camera.transform.localPosition;
        }
        
        private void LateUpdate()
        {
            var cameraPosition = _camera.transform.position;
            var deltaMovement = cameraPosition - _previousCameraPosition;

            foreach (var layer in _layers)
            {
                layer.Move(deltaMovement);
            }

            _previousCameraPosition = cameraPosition;
        }
    }
}