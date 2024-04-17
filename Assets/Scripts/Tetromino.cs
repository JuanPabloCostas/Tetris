using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    // Start is called before the first frame update

    float fall = 0;                     //- Countdown timer for fall speed
    private float fallSpeed;
    public bool allowRotation = true;   //- We use this to specify if we want to allow the tetromino to rotate
    public bool limitRotation = false;  //- We use this to limit the rotatui of the tetromino to a 90 / -90 rotation. (To / From)

    public int individualScore = 100;

    private float individualScoreTime;

    public AudioClip moveSound;         //- Sound for when the tetromino is moved
    public AudioClip rotateSound;       //- Sound for when the tetromino is rotated
    public AudioClip landSound;         //- Sound for when the tetromino lands

    private AudioSource audioSource;

    private float continuosVerticalSpeed = 0.05f;   //- The speed at which the tetromino will move when the down buttin is held down
    private float continuosHorizontalSpeed = 0.1f; //- The speed at which the tetromino will move when the left or right button is held down
    private float buttonDownWaitmax = 0.1f;         //~ How long to wait before the tetromino recognize that a button is being held down

    private float verticalTimer = 0;
    private float horizontalTimer = 0;
    private float buttonDownWaitTimerHorizontal = 0;
    private float buttonDownWaitTimerVertical = 0;

    private bool movedInmediateHorizontal = false;
    private bool movedInmediateVertical = false;


    /// <summary>
    /// Variables for Touch Movemet
    /// </summary>
    private int touchSensitivityHorizontal = 8;
    private int touchSensitivityVertical = 4;
    Vector2 previousUnitPosition = Vector2.zero;
    Vector2 direction = Vector2.zero;

    bool moved = false;
    

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Game.isPaused)
        {
            CheckUserInput();
            UpdateIndividualScore();
            UpdateFallSpeed();
        }
        
    }

    void UpdateFallSpeed ()
    {
        fallSpeed = Game.fallSpeed;
    }
    /// <summary>
    /// It makes the landing get points
    /// </summary>
    void UpdateIndividualScore()
    {
        if (individualScoreTime < 1)
        {
            individualScoreTime += Time.deltaTime;
        } else
        {
            individualScoreTime = 0;
            individualScore = Mathf.Max(individualScore - 10, 0);
        }
    }

    /// <summary>
    /// Plays audio clip when moves to left, right and down
    /// </summary>
    void PlayMoveAudio()
    {
        audioSource.PlayOneShot(moveSound);
    }
    /// <summary>
    /// Plays audio clip when tetromino rotates
    /// </summary>
    void PlayRotateAudio()
    {
        audioSource.PlayOneShot(rotateSound);
    }
    /// <summary>
    /// Play audio clip when the tetromino lands
    /// </summary>
    void PlayLandAudio()
    {
        audioSource.PlayOneShot(landSound);
    }

    /// <summary>
    /// Checks for whenever the user does an input
    /// </summary>
    void CheckUserInput()
    {
        //Si se compila para Andorid Hace lo del if e ignora lo del else
#if UNITY_ANDROID
        if (Input.touchCount > 0) {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                previousUnitPosition = new Vector2(t.position.x, t.position.y);
            } 
            else if (t.phase == TouchPhase.Moved) 
            {
                Vector2 touchDeltaPosition = t.deltaPosition;
                direction = touchDeltaPosition.normalized;

                if (Mathf.Abs(t.position.x -previousUnitPosition.x) >= touchSensitivityHorizontal && direction.x < 0 && t.deltaPosition.y > -10 && t.deltaPosition.y < 10)
                {
                    //- Move left
                    MoveLeft();
                    previousUnitPosition = t.position;
                    moved = true;
                }
                else if (Mathf.Abs(t.position.x - previousUnitPosition.x) >= touchSensitivityHorizontal && direction.x > 0 && t.deltaPosition.y > -10 && t.deltaPosition.y < 10)
                {
                    //- Move right
                    MoveRight();
                    previousUnitPosition = t.position;
                    moved = true;
                }
                else if (Mathf.Abs(t.position.y - previousUnitPosition.y) >= touchSensitivityVertical && direction.y < 0 && t.deltaPosition.x > -10 && t.deltaPosition.x < 10)
                {
                    //- Move down
                    MoveDown();
                    previousUnitPosition = t.position;
                    moved = true;
                }
            } 
            else if (t.phase == TouchPhase.Ended) 
            {
                if (!moved && t.position.x > Screen.width /4)
                {
                    Rotate();
                }
                moved = false;
            }
            
        }
        if (Time.time - fall >= fallSpeed)
        {
            MoveDown();
        }
        #else

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            movedInmediateHorizontal = false;
            horizontalTimer = 0;
            buttonDownWaitTimerHorizontal = 0;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            movedInmediateVertical = false;
            verticalTimer = 0;
            buttonDownWaitTimerVertical = 0;
        }

        if (Input.GetKey(KeyCode.RightArrow)) //Right
        {
            MoveRight();
        }
        if (Input.GetKey(KeyCode.LeftArrow)) //Left
        {
            MoveLeft();
        } 
        if (Input.GetKeyDown(KeyCode.UpArrow)) //Up (rotation)
        {
            Rotate();
        }  
        if (Input.GetKey(KeyCode.DownArrow) || Time.time - fall >= fallSpeed) // Down (also go down by itself)
        {
            MoveDown();
        }
        if (Input.GetKeyUp (KeyCode.Space))
        {
            SlamDown();
        }
        #endif
    }

    /// <summary>
    /// Moves the tetromino to the left
    /// </summary>
    void MoveLeft()
    {
        if (movedInmediateHorizontal)
        {
            if (buttonDownWaitTimerHorizontal < buttonDownWaitmax)
            {
                buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }
            if (horizontalTimer < continuosHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return;
            }
        }
        if (!movedInmediateHorizontal)
        {
            movedInmediateHorizontal = true;
        }
        horizontalTimer = 0;
        transform.position += new Vector3(-1, 0, 0);
        if (CheckIsValidPosition())
        {
            FindAnyObjectByType<Game>().UpdateGrid(this);
            PlayMoveAudio();
        }
        else
        {
            transform.position += new Vector3(1, 0, 0);
        }
    }
    /// <summary>
    /// Moves the tetromino to the right
    /// </summary>
    void MoveRight() 
    {
        if (movedInmediateHorizontal)
        {
            if (buttonDownWaitTimerHorizontal < buttonDownWaitmax)
            {
                buttonDownWaitTimerHorizontal += Time.deltaTime;
                return;
            }
            if (horizontalTimer < continuosHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return;
            }
        }
        if (!movedInmediateHorizontal)
        {
            movedInmediateHorizontal = true;
        }
        horizontalTimer = 0;
        transform.position += new Vector3(1, 0, 0);
        if (CheckIsValidPosition())
        {
            FindAnyObjectByType<Game>().UpdateGrid(this);
            PlayMoveAudio();
        }
        else
        {
            transform.position += new Vector3(-1, 0, 0);
        }
    }
    /// <summary>
    /// Move the tetromino down
    /// </summary>
    void MoveDown()
    {
        if (movedInmediateVertical)
        {
            if (buttonDownWaitTimerVertical < buttonDownWaitmax)
            {
                buttonDownWaitTimerVertical += Time.deltaTime;
                return;
            }
            if (verticalTimer < continuosVerticalSpeed)
            {
                verticalTimer += Time.deltaTime;
                return;
            }
        }
        if (!movedInmediateVertical)
        {
            movedInmediateVertical = true;
        }
        verticalTimer = 0;

        transform.position += new Vector3(0, -1, 0);


        if (CheckIsValidPosition())
        {
            FindAnyObjectByType<Game>().UpdateGrid(this);
            if (Input.GetKey(KeyCode.DownArrow))
            {
                PlayMoveAudio();
            }
        }
        else
        {
            transform.position += new Vector3(0, 1, 0);
            FindAnyObjectByType<Game>().DeleteRow();

            if (FindAnyObjectByType<Game>().CheckIsAboveGrid(this))
            {
                FindAnyObjectByType<Game>().GameOver();
            }


            FindAnyObjectByType<Game>().SpawnNextTetromino();

            Game.currentScore += individualScore;
            PlayLandAudio();

            enabled = false;

            tag = "Untagged";
        }
        fall = Time.time;
    }

    /// <summary>
    /// Rotates the tetromino
    /// </summary>
    void Rotate()
    {
        if (allowRotation) //Puede rotar?
        {
            if (limitRotation) //En cualquier angulo?
            {
                //Si no puede en cualquier angulo cambia entre -90 y 90
                if (transform.rotation.eulerAngles.z >= 90)
                {
                    transform.Rotate(0, 0, -90);
                }
                else
                {
                    transform.Rotate(0, 0, 90);
                }
            }
            else
            {
                transform.Rotate(0, 0, 90);
            }
            if (CheckIsValidPosition())
            {
                FindAnyObjectByType<Game>().UpdateGrid(this);
                PlayRotateAudio();
            }
            else
            {
                if (limitRotation)
                {
                    //Para evitar que el bloqueo realize un movimiento ilegal
                    if (transform.rotation.eulerAngles.z >= 90)
                    {
                        transform.Rotate(0, 0, -90);
                    }
                    else
                    {
                        transform.Rotate(0, 0, 90);
                    }
                }
                else
                {
                    transform.Rotate(0, 0, -90);
                }


            }

        }
    }

    public void SlamDown()
    {

        while (CheckIsValidPosition())
        {
            transform.position += new Vector3(0, -1, 0);
        }
        
        if (!CheckIsValidPosition())
        {
            transform.position += new Vector3(0,1,9);
            FindAnyObjectByType<Game>().UpdateGrid(this);
            FindAnyObjectByType<Game>().DeleteRow();

            if (FindAnyObjectByType<Game>().CheckIsAboveGrid(this))
            {
                FindAnyObjectByType<Game>().GameOver();
            }


            FindAnyObjectByType<Game>().SpawnNextTetromino();

            Game.currentScore += individualScore;
            PlayLandAudio();

            enabled = false;

            tag = "Untagged";
        }
        
    }

    //Check if the movement that the user is trying to do is valid
    bool CheckIsValidPosition()
    {
        foreach (Transform mino in transform) { 
            Vector2 pos = FindFirstObjectByType<Game>().Round(mino.position);
            if (FindAnyObjectByType<Game>().CheckIsInsideGrid(pos) == false)
            {
                return false;
            }
            if (FindAnyObjectByType<Game>().GetTransformAtGridPosition(pos) != null && FindAnyObjectByType<Game>().GetTransformAtGridPosition(pos).parent != transform) 
            {
                return false;
            }
        }
        return true;
        
    }
}
