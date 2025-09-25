using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Drive : MonoBehaviour
{

    [Header("Movement Settings")]
    public float CurrentSpeed = 5f;
    public float SteerSpeed = 200f;
    public float StartSpeed = 5f;
    public float BoostSpeed = 8f;

    [Header("Game Objects")]
    public GameObject SingleMoney;
    public GameObject DoubleMoney;
    public GameObject Pizza;
    public GameObject PizzaTop;
    public UIDocument uiDocument;
    public ParticleSystem MoneyBoost;

    [Header("Star Objects")]
    public GameObject zeroStar;
    public GameObject oneStar;
    public GameObject twoStar;
    public GameObject threeStar;
    public GameObject fourStar;
    public GameObject fiveStar;

    // private variables
    private float _cash = 0f;
    private bool _hitPizza = false;
    private float _moneysCollected = 0f;
    private Vector3 _originalScale;

    // ui elements
    private Label feedbackText;
    private Label cashText;
    private Label endText;
    private Button restartButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize ui elements
        cashText = uiDocument.rootVisualElement.Q<Label>("CashLabel");
        feedbackText = uiDocument.rootVisualElement.Q<Label>("FeedbackLabel");
        endText = uiDocument.rootVisualElement.Q<Label>("EndLabel");
        restartButton = uiDocument.rootVisualElement.Q<Button>("RestartButton");
        restartButton.clicked += ReloadScene;

        // get original scale of car
        _originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // move car
        Move();

        // update cash text
        cashText.text = "$" + _cash;

        // check for end of game
        if (_moneysCollected == 15f)
        {
            endText.visible = true;
            cashText.visible = false;
            feedbackText.visible = false;
            endText.text = "You have completed all deliveries for this level and earned $" + _cash + "!";
            // determine star rating based on cash amount and display it
            if (_cash == 230f)
            {
                fiveStar.SetActive(true);
            }
            else if (_cash >= 200f)
            {
                fourStar.SetActive(true);
            }
            else if (_cash >= 150f)
            {
                threeStar.SetActive(true);
            }
            else if (_cash >= 100f)
            {
                twoStar.SetActive(true);
            }
            else if (_cash > 0f)
            {
                oneStar.SetActive(true);
            }
            else
            {
                zeroStar.SetActive(true);
            }
            restartButton.visible = true;
        }
    }

    void Move()
    {
        // move and steer based on keyboard input
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

        // flip car sprite based on rotation
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
    
    // triggers are moneys and pizzas
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Pizza"))
        {
            Pizza.SetActive(false);
            PizzaTop.SetActive(true);
        }
        else if (collision.gameObject.CompareTag("SingleMoney") | collision.gameObject.CompareTag("DoubleMoney"))
        {
            // if the car has not picked up a pizza, do not allow money collection
            if (PizzaTop.activeSelf == false)
            {
                feedbackText.visible = true;
                feedbackText.text = "You need to pick up a pizza first!";
                Invoke("HideNegativeFeedback", 2f);
                return;
            }

            // configure game objects, effects, and feedback for successful delivery
            Destroy(collision.gameObject);
            PizzaTop.SetActive(false);
            Pizza.SetActive(true);
            feedbackText.visible = true;
            MoneyBoost.Play();
            Invoke("HidePositiveFeedback", 3f);
            CurrentSpeed = BoostSpeed;
            _moneysCollected += 1f;

            // update cash amount, feedback, and game objects based on money type
            if (collision.gameObject.CompareTag("SingleMoney"))
            {
                Invoke("HideSingleMoney", 3f);
                SingleMoney.SetActive(true);
                _cash += 10f;
                feedbackText.text = "Successfully delivered Pizza! +$10";
            }
            else if (collision.gameObject.CompareTag("DoubleMoney"))
            {
                Invoke("HideDoubleMoney", 3f);
                DoubleMoney.SetActive(true);
                _cash += 20f;
                feedbackText.text = "Successfully delivered Pizza! +$20";
            }
        }

    }

    // many colliders, only care about pizza boxes
    void OnCollisionEnter2D(Collision2D collision)
    {
        // stop the boost, give negative feedback, and reduce cash
        if (collision.gameObject.CompareTag("PizzaBox") && _hitPizza == false)
        {
            _hitPizza = true;
            feedbackText.visible = true;
            feedbackText.text = "Hit empty pizza box! -$5 to repair car.";
            // limit negative effects to once every 1.5 seconds
            Invoke("HideNegativeFeedback", 1.5f);
            CurrentSpeed = StartSpeed;

            // cash cannot go below 0
            if (_cash >= 5f)
            {
                _cash -= 5f;
            }
            else
            {
                _cash = 0f;
            }
        }
    }

    // hide game objects and feedback, stop boosts after a delay
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
    
    // when the restart button is clicked, reload the scene
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
