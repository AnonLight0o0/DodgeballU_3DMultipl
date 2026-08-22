using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public bool ControlsEnabled = true;

    public Transform target;
    public float distance = 5.0f;
    public float heightOffset = 1.5f;

    public float xSpeed = 200.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -10f;
    public float yMaxLimit = 70f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!ControlsEnabled)
            return;

        if (target == null) return;

        if (target == null) return;

        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        
        Vector3 targetPos = target.position + Vector3.up * heightOffset;
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + targetPos;

        transform.rotation = rotation;
        transform.position = position;
    }
}