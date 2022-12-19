using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [System.NonSerialized] public Camera cam;

    [Header("Properties")]
    public float mouseSensitivity = 2.5f;
    [Range(0, 120)] public float fieldOfView = 90;

    [Header("Input")]
    public Vector3 rawInput;
    public Vector3 cameraEuler;
    [NonSerialized] public float pitch, yaw, roll;
    [NonSerialized] public Ray centerOfScreenRay;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        centerOfScreenRay = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    }

    public void Move(Vector2 input)
    {
        rawInput = input;

        //input *= 0.5f;
        //input *= 0.1f;

        yaw += input.x * mouseSensitivity;
        pitch -= input.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -90, 90);

        cameraEuler = new Vector3(pitch, yaw, 0);
    }

    private void LateUpdate()
    {
        Move(new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y")));
        FirstPersonMode();
    }

    private void FirstPersonMode()
    {
        cameraEuler.z = roll;
        transform.eulerAngles = cameraEuler;
    }
}