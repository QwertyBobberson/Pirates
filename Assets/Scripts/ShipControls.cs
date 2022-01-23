using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipControls : MonoBehaviour
{
    private InputHandler manager;

    public float maxTurnSpeed;
    public float turnAcceleration;
    public float maxTilt;

    public float maxSpeed;
    public float moveAcceleration;
    
    private float rotation;

    public void Start()
    {
        manager = new InputHandler();
        manager.Enable();
        rotation = 0;
    }

    float speed = 0;

    public void Update()
    {
        Turn(manager.Ship.Turn.ReadValue<float>());
        Move(manager.Ship.Move.ReadValue<float>());
    }

    private float yRot = 0;
    private float zRot = 0;

    public void Turn(float turnInput)
    {
        yRot = Mathf.Lerp(yRot, turnInput * maxTurnSpeed, Time.deltaTime * turnAcceleration);
        rotation += yRot * Time.deltaTime;

        zRot = Mathf.Lerp(zRot, -maxTilt * yRot/maxTurnSpeed, turnAcceleration * Time.deltaTime);
        Quaternion quaternionRot = Quaternion.Euler(0, rotation, zRot);
        transform.rotation = quaternionRot;
    }

    public void Move(float moveInput)
    {
        speed = Mathf.Lerp(speed, moveInput * maxSpeed, Time.deltaTime * moveAcceleration);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}