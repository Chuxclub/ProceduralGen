﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    [Range(0.2f, 10f)]
    public float mouseSensitivity = 1f;

    [SerializeField] [Range(0f, 1f)]
    private float lerpCoef = 0.3f;
    public float maxYAngle = 80f;
    public float speed = 12f;
    private Vector2 currentRotation;

    public Transform transform;
    private void FixedUpdate()
    {
        // TODO Mousewheel -> up/down global
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        transform.position = Vector3.Lerp(transform.position,transform.position + move, lerpCoef);

        if (Input.GetMouseButton(0) == false)
        {
            currentRotation.x += Input.GetAxis("Mouse X") * mouseSensitivity * 50f * Time.deltaTime;
            currentRotation.y -= Input.GetAxis("Mouse Y") * mouseSensitivity * 50f * Time.deltaTime;
            currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
            currentRotation.y = Mathf.Clamp(currentRotation.y, -maxYAngle, maxYAngle);
            Camera.main.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        }

    }
       


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}
