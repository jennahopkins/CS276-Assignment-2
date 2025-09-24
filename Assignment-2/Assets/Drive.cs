using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Drive : MonoBehaviour
{

    public float CurrentSpeed = 5f;
    public float SteerSpeed = 200f;
    public float StartSpeed = 5f;
    public float BoostSpeed = 10f;

    public GameObject SingleMoney;
    public GameObject DoubleMoney;
    public GameObject Pizza;
    public GameObject PizzaTop;

    private Vector3 _originalScale;

    public UIDocument uiDocument;
    private Label cashText;
    private float _cash = 0f;

    private Label feedbackText;

    private bool _hitPizza = false;

    public ParticleSystem MoneyBoost;

    private float _moneysCollected = 0f;

    public Label endText;
    public Button restartButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cashText = uiDocument.rootVisualElement.Q<Label>("CashLabel");
        feedbackText = uiDocument.rootVisualElement.Q<Label>("FeedbackLabel");
        endText = uiDocument.rootVisualElement.Q<Label>("EndLabel");
        restartButton = uiDocument.rootVisualElement.Q<Button>("RestartButton");
        _originalScale = transform.localScale;
        restartButton.clicked += ReloadScene;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        if (_moneysCollected == 15f)
        {
            endText.visible = true;
            cashText.visible = false;
            feedbackText.visible = false;
            endText.text = "You have completed all deliveries for this level and earned $" + _cash + "!";
            restartButton.visible = true;
        }
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

        cashText.text = "$" + _cash;

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
            if (PizzaTop.activeSelf == false)
            {
                feedbackText.visible = true;
                feedbackText.text = "You need to pick up a pizza first!";
                Invoke("HideNegativeFeedback", 2f);
                return;
            }
            Destroy(collision.gameObject);
            PizzaTop.SetActive(false);
            SingleMoney.SetActive(true);
            Pizza.SetActive(true);
            Invoke("HideSingleMoney", 3f);
            _cash += 10f;
            feedbackText.visible = true;
            feedbackText.text = "Successfully delivered Pizza! +$10";
            MoneyBoost.Play();
            Invoke("HidePositiveFeedback", 3f);
            CurrentSpeed = BoostSpeed;
            _moneysCollected += 1f;
        }
        else if (collision.gameObject.CompareTag("DoubleMoney"))
        {
            if (PizzaTop.activeSelf == false)
            {
                feedbackText.visible = true;
                feedbackText.text = "You need to pick up a pizza first!";
                Invoke("HideNegativeFeedback", 2f);
                return;
            }
            Destroy(collision.gameObject);
            PizzaTop.SetActive(false);
            DoubleMoney.SetActive(true);
            Pizza.SetActive(true);
            Invoke("HideDoubleMoney", 3f);
            _cash += 20f;
            feedbackText.visible = true;
            feedbackText.text = "Successfully delivered Pizza! +$20";
            MoneyBoost.Play();
            Invoke("HidePositiveFeedback", 3f);
            CurrentSpeed = BoostSpeed;
            _moneysCollected += 1f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Collide with anything will stop the boost!
        if (collision.gameObject.CompareTag("PizzaBox") && _hitPizza == false)
        {
            _hitPizza = true;
            feedbackText.visible = true;
            feedbackText.text = "Hit empty pizza box! -$5 to repair car.";
            if (_cash >= 5f)
            {
                _cash -= 5f;
            }
            else
            {
                _cash = 0f;
            }
            Invoke("HideNegativeFeedback", 1.5f);
            CurrentSpeed = StartSpeed;
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
    void HidePositiveFeedback()
    {
        feedbackText.visible = false;
        MoneyBoost.Stop();
        CurrentSpeed = StartSpeed;
    }
    void HideNegativeFeedback()
    {
        feedbackText.visible = false;
        _hitPizza = false;
    }
    
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
