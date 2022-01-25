using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControls : MonoBehaviour
{
    public float turnSpeed;
    public float maxRot;
    private float xRot;
    private float yRot;

    public GameObject water;

    public int cameraPosNum;
    public Transform[] cameraPos;

    public Transform waterPos;

    private InputHandler manager;

    void Start()
    {
        manager = new InputHandler();
        manager.Enable();
        manager.Camera.Turn.performed += TurnCamera;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Update()
    {
        water.transform.position = new Vector3(waterPos.position.x, 0, waterPos.position.z);
        transform.position = cameraPos[cameraPosNum].position;
    }

    public void TurnCamera(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        yRot += input.x * Time.deltaTime * turnSpeed;
        xRot -= input.y * Time.deltaTime * turnSpeed;
        xRot = Mathf.Clamp(xRot, -maxRot, maxRot);

        transform.localRotation = Quaternion.Euler(xRot, yRot, 0);
    }
}
