using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class Drive : MonoBehaviour
{

    public float CurrentSpeed = 5f;
    public float SteerSpeed = 200f;
    public float StartSpeed = 5f;

    public GameObject SingleMoney;
    public GameObject DoubleMoney;
    public GameObject Pizza;
    public GameObject PizzaTop;

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
        //fix orientation when car flips
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pizza"))
        {
            Pizza.SetActive(false);
            PizzaTop.SetActive(true);
        }
        else if (collision.gameObject.CompareTag("SingleMoney"))
        {
            Destroy(collision.gameObject);
            PizzaTop.SetActive(false);
            SingleMoney.SetActive(true);
            Pizza.SetActive(true);
            Invoke("HideSingleMoney", 3f);
        }
        else if (collision.gameObject.CompareTag("DoubleMoney"))
        {
            Destroy(collision.gameObject);
            PizzaTop.SetActive(false);
            DoubleMoney.SetActive(true);
            Pizza.SetActive(true);
            Invoke("HideDoubleMoney", 3f);
        }
    }

    void HideSingleMoney()
    {
        SingleMoney.SetActive(false);
    }
    void HideDoubleMoney()
    {
        DoubleMoney.SetActive(false);
    }
}
