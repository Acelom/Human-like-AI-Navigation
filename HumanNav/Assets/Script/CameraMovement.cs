using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine;
using PlayerController;

public class CameraMovement : MonoBehaviour
{

    private Inputs inputs; 

    [SerializeField] private float movementSpeed;
    [SerializeField] private float camSensitivity;

    private void Awake()
    {
        inputs = GetComponent<Inputs>(); 
    }

    private void Update()
    {
        CamMovement();

        if (inputs.controllingCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            CamRotation();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void CamRotation()
    {
        Vector3 newRot = new Vector3((transform.eulerAngles.x) - (inputs.camDirection.y * camSensitivity), transform.eulerAngles.y + (inputs.camDirection.x * camSensitivity), 0);
        if (newRot.x < 271 && newRot.x > 180)
        {
            newRot.x = 271;

        }

        if (newRot.x > 89 && newRot.x < 180)
        {
            newRot.x = 89;
        }
        transform.rotation = Quaternion.Euler(newRot);
    }

    private void CamMovement()
    {
        transform.Translate(inputs.movementDirection * Time.deltaTime * (movementSpeed + (inputs.speed * movementSpeed)));

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -100, 100),
            Mathf.Clamp(transform.position.y, 1, 200),
            Mathf.Clamp(transform.position.z, -100, 100));

    }

}
