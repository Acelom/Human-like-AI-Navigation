using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlayerController
{

    public class Inputs : MonoBehaviour
    {

        private CameraControls controls;
        public Vector3 movementDirection;
        public Vector2 camDirection;
        public int speed;
        public bool controllingCamera;
        public float rotationChange; 

        public bool leftClick;
        public bool rightClick;


        private void Awake()
        {
            controls = new CameraControls();

            controls.Movement.Enable();

            controls.Movement.Movement.performed += context => movementDirection = Vector3.Normalize(context.ReadValue<Vector3>());
            controls.Movement.Movement.canceled += context => movementDirection = Vector3.zero;

            controls.Movement.Speed.performed += context => speed = 1;
            controls.Movement.Speed.canceled += context => speed = 0;

            controls.Movement.Look.performed += context => camDirection = context.ReadValue<Vector2>();
            controls.Movement.Look.canceled += context => camDirection = Vector2.zero;

            controls.Movement.Swap.started += context => controllingCamera = true;
            controls.Movement.Swap.canceled += context => controllingCamera = false;

            controls.Movement.LeftClick.started += context => leftClick = true;

            controls.Movement.RightClick.started += context => rightClick = true;
            controls.Movement.RotateObject.performed += context => rotationChange = context.ReadValue<float>();
            controls.Movement.RotateObject.canceled += context => rotationChange = 0;

        }

        private void LateUpdate()
        {
            leftClick = false;
            rightClick = false;
        }
    }
}
