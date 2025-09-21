using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class Drive : MonoBehaviour
{

    public float CurrentSpeed = 5f;
    public float SteerSpeed = 200f;
    public float StartSpeed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    { 
        float move = Keyboard.current switch
        {
            { wKey: { isPressed: true } } or { upArrowKey: { isPressed: true } } => -1f,
            { sKey: { isPressed: true } } or { downArrowKey: { isPressed: true } } => 1f,
            _ => 0f
        };

        float steer = Keyboard.current switch
        {
            { aKey: { isPressed: true } } or { leftArrowKey: { isPressed: true } } => 1f,
            { dKey: { isPressed: true } } or { rightArrowKey: { isPressed: true } } => -1f,
            _ => 0f
        };

        float moveAmount = move * CurrentSpeed * Time.deltaTime;
        float steerAmount = steer * SteerSpeed * Time.deltaTime;

        transform.Translate(moveAmount, 0, 0);
        transform.Rotate(0, 0, steerAmount);
    }
}
