using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // All assets stored
    // Stores the input system script
    private InputSystem_Actions input = null;

    [SerializeField]
    GameObject leftBumper = null;

    [SerializeField]
    GameObject rightBumper = null;

    [SerializeField]
    GameObject ball = null;
    CircleCollider2D ballCollider = null;

    [SerializeField]
    GameObject[] coins;
    float[] coinRespawnTimes = null;

    [SerializeField]
    GameObject[] circleBumpers;

    [SerializeField]
    GameObject[] spinners;

    [SerializeField]
    GameObject[] boosters;

    [SerializeField]
    GameObject[] teleportIn;

    [SerializeField]
    GameObject[] teleportOut;

    [SerializeField]
    TMP_Text gameData;

    [SerializeField]
    Sprite coin1;

    [SerializeField]
    Sprite coin3;

    [SerializeField]
    Sprite coin5;

    bool gameStarted = false;
    int totalPoints = 0;
    int lives = 0;

    [SerializeField]
    TMP_Text startText;

    // sounds
    [SerializeField]
    AudioSource sfxExplosion = null;
    [SerializeField]
    AudioSource sfxNope = null;
    [SerializeField]
    AudioSource sfxWilhelm = null;
    [SerializeField]
    AudioSource sfxWarp = null;
    [SerializeField]
    AudioSource sfxInput = null;
    [SerializeField]
    AudioSource sfxBrah = null;

    bool hasWilled = true;

    /// <summary>
    /// Establish the input system 
    /// (must be done before OnEnable runs, so Awake() has to be used instead of Start()).
    /// </summary>
    private void Awake ()
    {
        // Sets the input system to be created when the object is made.
        input = new InputSystem_Actions();
    }

    /// <summary>
    /// When this object is enabled in the scene, including spawning in.
    /// (Subscribes the set functions to the input system).
    /// </summary>
    private void OnEnable ()
    {
        // Enables the newly created input system
        input.Enable();
        input.Player.GameStart.started += StartGame;
        input.Player.LeftBumper.started += FlipBumperLeft;
        input.Player.RightBumper.started += FlipBumperRight;
        input.Player.EndGame.started += EndingGame;
    }

    /// <summary>
    /// Runs when the object is disabled or the game ends
    /// (Unsubscribes the set functions from the input system,
    ///  think like how in GameMaker you have to destroy lists
    ///  when you are done using them for cleanup purposes).
    /// </summary>
    private void OnDisable ()
    {
        input.Disable();
        input.Player.GameStart.started -= StartGame;
        input.Player.LeftBumper.started -= FlipBumperLeft;
        input.Player.RightBumper.started -= FlipBumperRight;
        input.Player.EndGame.started += EndingGame;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ballCollider = ball.GetComponent<CircleCollider2D>();
        coinRespawnTimes = new float[coins.Length];
    }

    // Update is called once per frame
    void Update()
    {
        // Teleport ball if it touches a teleporter
        for (int _i = 0; _i < teleportIn.Length; ++_i)
        {
            if (teleportIn[_i].GetComponent<Collider2D>().IsTouching(ballCollider))
            {
                ball.GetComponent<Rigidbody2D>().position = teleportOut[_i].GetComponent<Transform>().position;
                ball.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

                sfxWarp.Stop();
                sfxWarp.Play();
            }
        }

        // Coin Collection
        for (int _i = 0; _i < coins.Length; ++_i)
        {
            if (coins[_i].GetComponent<Collider2D>().IsTouching(ballCollider))
            {
                coins[_i].SetActive(false);

                if (coins[_i].GetComponent<SpriteRenderer>().sprite == coin1)
                {
                    totalPoints++;
                }
                else if (coins[_i].GetComponent<SpriteRenderer>().sprite == coin3)
                {
                    totalPoints += 3;
                }
                else
                {
                    totalPoints += 5;
                }

                //sfxBrah.Stop();
                sfxBrah.Play();
            }
            if (!coins[_i].activeInHierarchy)
            {
                coinRespawnTimes[_i] += Time.deltaTime;
                if (coinRespawnTimes[_i] > 5)
                {
                    coins[_i].SetActive(true);
                    coinRespawnTimes[_i] = 0;
                }
            }
        }

        // Bounce sound
        for (int _i = 0; _i < circleBumpers.Length; ++_i)
        {
            if (circleBumpers[_i].GetComponent<Collider2D>().IsTouching(ballCollider))
            {
                if (!sfxExplosion.isPlaying) sfxNope.Play();
            }
        }

        // Change text for points
        gameData.text = "LIVES: " + lives + "\r\nPOINTS: " + totalPoints;

        if (ball.GetComponent<Transform>().position.y < -5.0f)
        {
            if (!hasWilled)
            {
                sfxWilhelm.Play();
                hasWilled = true;
            }

            if (lives == 0)
            {
                startText.text = (totalPoints > 0) ? "PRESS 'SPACE' TO RESTART" : "PRESS 'SPACE' TO START";
            }
            else
            {
                startText.text = "PRESS 'SPACE' TO CONTINUE";
            }
        }
        else
        {
            startText.text = "";
            hasWilled = false;
        }
    }
    private void FixedUpdate ()
    {
        // Reset bumpers
        rightBumper.GetComponent<Rigidbody2D>().AddTorque(10.0f);
        leftBumper.GetComponent<Rigidbody2D>().AddTorque(-10.0f);

        // Booster speed change
        for (int _i = 0; _i < boosters.Length; ++_i)
        {
            if (boosters[_i].GetComponent<Collider2D>().IsTouching(ballCollider))
            {
                ball.GetComponent<Rigidbody2D>().AddForceX(0.07f, ForceMode2D.Impulse);

                if (!sfxExplosion.isPlaying) sfxExplosion.Play();
            }
        }

        // Spinners... spin!
        for (int _i = 0; _i < spinners.Length; ++_i)
        {
            if (spinners[_i].GetComponent<Transform>().localScale.x < 0)
            {
                spinners[_i].GetComponent<Rigidbody2D>().SetRotation(spinners[_i].GetComponent<Rigidbody2D>().rotation + 0.7f);
            }
            else
            {
                spinners[_i].GetComponent<Rigidbody2D>().SetRotation(spinners[_i].GetComponent<Rigidbody2D>().rotation - 0.7f);
            }
        }
    }

    /// <summary>
    /// Event tied to the pressing of the chosen input in OnEnable();
    /// </summary>
    /// <param name="context"></param>
    public void StartGame (InputAction.CallbackContext context)
    {
        if (ball.GetComponent<Transform>().position.y < -5.0f)
        {
            ball.GetComponent<Rigidbody2D>().position = new Vector2(-3.3f, -3.3f);
            ball.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            if (gameStarted)
            {
                lives--;
                if (lives == 0)
                {
                    gameStarted = false;
                }
            }
            else
            {
                gameStarted = true;
                lives = 3;
                totalPoints = 0;

                for (int _i = 0; _i < coins.Length; ++_i)
                {

                    if (coins[_i].GetComponent<Collider2D>().IsTouching(ballCollider))
                    {
                        coins[_i].SetActive(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Event tied to the pressing of the chosen input in OnEnable();
    /// </summary>
    /// <param name="context"></param>
    public void FlipBumperLeft (InputAction.CallbackContext context)
    {
        leftBumper.GetComponent<Rigidbody2D>().AddTorque(1000.0f);
        sfxInput.Stop();
        sfxInput.Play();
    }

    /// <summary>
    /// Event tied to the pressing of the chosen input in OnEnable();
    /// </summary>
    /// <param name="context"></param>
    public void FlipBumperRight (InputAction.CallbackContext context)
    {
        rightBumper.GetComponent<Rigidbody2D>().AddTorque(-1000.0f);
        sfxInput.Stop();
        sfxInput.Play();
    }

    /// <summary>
    /// Event tied to the pressing of the chosen input in OnEnable();
    /// </summary>
    /// <param name="context"></param>
    public void EndingGame (InputAction.CallbackContext context)
    {
        Application.Quit();
    }
}
