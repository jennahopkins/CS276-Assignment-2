using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class Drive : MonoBehaviour
{

    public float CurrentSpeed = 5f;
    public float SteerSpeed = 200f;
    public float StartSpeed = 5f;

    public GameObject SingleMoney;
    public GameObject DoubleMoney;
    public GameObject Pizza;
    public GameObject PizzaTop;

    private Vector3 _originalScale;

    public UIDocument uiDocument;
    private Label scoreText;
    private float _score = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText = uiDocument.rootVisualElement.Q<Label>("ScoreLabel");
        _originalScale = transform.localScale;
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

        float zRotation = transform.eulerAngles.z;
        if (zRotation > 180f)
            zRotation -= 360f;

        if (Mathf.Abs(zRotation) > 90)
        {
            transform.localScale = new Vector3(
                _originalScale.x,
                -Mathf.Abs(_originalScale.y),
                _originalScale.z
            );
        }
        else
        {
            transform.localScale = _originalScale;
        }

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
            if (PizzaTop.activeSelf == false) return;
            Destroy(collision.gameObject);
            PizzaTop.SetActive(false);
            SingleMoney.SetActive(true);
            Pizza.SetActive(true);
            Invoke("HideSingleMoney", 3f);
            _score += 10f;
            scoreText.text = "Score: " + _score;
        }
        else if (collision.gameObject.CompareTag("DoubleMoney"))
        {
            if (PizzaTop.activeSelf == false) return;
            Destroy(collision.gameObject);
            PizzaTop.SetActive(false);
            DoubleMoney.SetActive(true);
            Pizza.SetActive(true);
            Invoke("HideDoubleMoney", 3f);
            _score += 20f;
            scoreText.text = "Score: " + _score;
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
