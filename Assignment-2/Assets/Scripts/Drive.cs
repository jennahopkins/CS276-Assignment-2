using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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
    public ParticleSystem PizzaEffect;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip MoneySound;
    public AudioClip PizzaSound;
    public AudioClip CrashSound;
    public AudioClip LevelFailed;
    public AudioClip LevelComplete;

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
    private bool _beatLevel = false;
    private List<int> _levelmoneys = new List<int> { 7, 15 };
    private List<int> _level1cash = new List<int> {110, 100, 50, 25, 10};
    private List<int> _level2cash = new List<int> { 230, 200, 150, 100, 50 };
    private List<List<int>> _levelcash = new List<List<int>>();
    private Vector3 _originalScale;

    // ui elements
    private Label feedbackText;
    private Label cashText;
    private Label endText;
    private Button restartButton;
    private Button startButton;
    private Label startText;
    private Button continueButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize level cash requirements
        _levelcash.Add(_level1cash);
        _levelcash.Add(_level2cash);

        // initialize ui elements
        cashText = uiDocument.rootVisualElement.Q<Label>("CashLabel");
        feedbackText = uiDocument.rootVisualElement.Q<Label>("FeedbackLabel");
        endText = uiDocument.rootVisualElement.Q<Label>("EndLabel");
        restartButton = uiDocument.rootVisualElement.Q<Button>("RestartButton");
        startButton = uiDocument.rootVisualElement.Q<Button>("StartButton");
        startText = uiDocument.rootVisualElement.Q<Label>("StartLabel");
        continueButton = uiDocument.rootVisualElement.Q<Button>("ContinueButton");

        // determine which level the user is on and display appropriate start text
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            startText.visible = true;
            startButton.visible = true;
            startText.text = "Welcome to Pizza Delivery Pro! Use WASD or the arrow keys to drive the car to deliver pizzas to all houses in the neighborhood! \n Level 1: Earn at least $50 to beat the level!";
        }
        else
        {
            startText.visible = true;
            startButton.visible = true;
            startText.text = "Level 2: \n Earn at least $150 to beat the level!";
        }

        // functions to run when buttons are clicked
        startButton.clicked += StartGame;
        restartButton.clicked += ReloadScene;
        continueButton.clicked += NextLevel;
        

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

        // check for level completion
        if (SceneManager.GetActiveScene().buildIndex == 0 && _moneysCollected == _levelmoneys[0])
        {
            EndLevel(0);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1 && _moneysCollected == _levelmoneys[1])
        {
            EndLevel(1);
        }
    }

    void Move()
    {
        // do not allow movement if the user has not pressed start
        if (startButton.visible == true)
        {
            return;
        }
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
            PizzaEffect.Play();
            PizzaTop.SetActive(true);
            audioSource.PlayOneShot(PizzaSound);
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
            audioSource.PlayOneShot(MoneySound);
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
            audioSource.PlayOneShot(CrashSound);
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

    void StartGame()
    {
        startButton.visible = false;
        startText.visible = false;
    }

    void EndLevel(int level)
    {
        // display end level feedback
        endText.visible = true;
        cashText.visible = false;
        feedbackText.visible = false;
        restartButton.visible = true;

        // determine star rating based on cash amount and display it
        if (_cash == _levelcash[level][0])
        {
            fiveStar.SetActive(true);
            _beatLevel = true;
        }
        else if (_cash >= _levelcash[level][1])
        {
            fourStar.SetActive(true);
            _beatLevel = true;
        }
        else if (_cash >= _levelcash[level][2])
        {
            threeStar.SetActive(true);
            _beatLevel = true;
        }
        else if (_cash >= _levelcash[level][3])
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

        // display different end text and options based on level completion status
        if (_beatLevel && level == 1)
        {
            endText.text = "Congrats, you beat the level! You completed all deliveries and earned $" + _cash + ". There are currently no further levels, but try this one again to earn a higher star rating by clicking the restart button below.";
            audioSource.PlayOneShot(LevelComplete);
        }
        else if (_beatLevel)
        {
            continueButton.visible = true;
            endText.text = "Congrats, you beat the level! You completed all deliveries and earned $" + _cash + ". Press continue to move on to the next level, or restart this one to try to earn a higher star rating.";
            audioSource.PlayOneShot(LevelComplete);
        }
        else
        {
            endText.text = "You didn't earn enough cash to beat the level. You completed all deliveries and earned $" + _cash + ". Try again by clicking the restart button below.";
            audioSource.PlayOneShot(LevelFailed);
        }
    }    

    // when the continue button is clicked, load the next scene
    void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
    // when the restart button is clicked, reload the scene
    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
