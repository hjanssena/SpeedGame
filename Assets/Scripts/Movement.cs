using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Animator animator;

    float delta;

    //Movement on x
    [Header("Movement on x")]
    [SerializeField] float moveSpeed;
    [SerializeField] float currentXSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxAirSpeed;
    [SerializeField] float airMovementPenalty;
    bool updateStart = false;
    bool adjustedToFloor;
    Vector2 lastPosition;
    int lastDirection = 1;

    //Movement on y
    [Header("Movement on y")]
    [SerializeField] float gravity;
    [SerializeField] float maxFallSpeed;
    [SerializeField] float jumpStartSpeed;
    [SerializeField] float jumpStaySpeed;
    [SerializeField] float maxJumpForce;
    float jumpForceApplied;
    float currentYSpeed;
    bool jumping;
    bool jumpInUse = false;

    [Header("Wall Grab")]
    [SerializeField] float wallGrabFriction;
    [SerializeField] float wallGrabMaxFallSpeed;

    [Header("Wall Jump")]
    [SerializeField] float wallJumpStartXForce;
    [SerializeField] float wallJumpStayXForce;
    [SerializeField] float wallJumpMaxXForce;
    float wallJumpXForceApplied;
    [SerializeField] float wallJumpStartYForce;
    [SerializeField] float wallJumpStayYForce;
    [SerializeField] float wallJumpMaxYForce;
    float wallJumpYForceApplied;
    bool wallJumping = false;
    int wallJumpDirection;

    //Walls and floor detection
    bool onFloor;
    bool rightWall;
    bool leftWall;
    bool ceiling;

    //Sounds
    [Header("Sound")]
    [SerializeField] AudioClip stepSound;
    [SerializeField] AudioClip jumpSound;
    AudioSource audioPlayer;

    void Start()
    {
        lastPosition = transform.position;
        animator = GetComponent<Animator>();
        audioPlayer = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(!updateStart)
        {
            StartCoroutine("WaitUntilLoad");
        }
        if(updateStart)
        {
            delta = Time.unscaledDeltaTime * Time.timeScale;
            onFloor = CheckOnFloor();
            rightWall = CheckRightWalls();
            leftWall = CheckLeftWalls();
            ceiling = CheckCeiling();

            //xMovement
            if (Input.GetAxis("Horizontal") > 0f || Input.GetAxis("Horizontal") < 0f) 
            {
                MovementOnX();
                if(onFloor){ animator.SetBool("walking", true); }
                else { animator.SetBool("walking", false); }
                if(Input.GetAxis("Horizontal") > 0f)
                {
                    lastDirection = 1;
                }
                else
                {
                    lastDirection = -1;
                }
            }
            else
            {
                StopMovement();
                animator.SetBool("walking", false);
            }

            //yMovement
            Gravity();
            if (Input.GetAxis("Jump") > 0)
            {
                Jump();
                WallJump();
            }
            else
            {
                jumping = false;
            }
            if (Input.GetAxis("Jump") == 0)
            {
                jumpInUse = false;
                wallJumping = false;
            }

            //movement
            ApplyMovementLimits();
            lastPosition = transform.position;
            transform.Translate(currentXSpeed * delta, currentYSpeed * delta, 0);
        }
    }

    void MovementOnX()
    {
        float xAxis = Input.GetAxis("Horizontal");
        if(!onFloor) //Movement while on air
        { 
            if (xAxis > 0)
            {
                if (currentXSpeed < 0)
                {
                    currentXSpeed += moveSpeed * xAxis * airMovementPenalty * delta;
                }
                else if (currentXSpeed < maxAirSpeed)
                {
                    currentXSpeed += moveSpeed * xAxis * airMovementPenalty * delta;
                }
            }
            else
            {
                if (currentXSpeed > 0)
                {
                    currentXSpeed += moveSpeed * xAxis * airMovementPenalty * delta;
                }
                else if (currentXSpeed > -maxAirSpeed)
                {
                    currentXSpeed += moveSpeed * xAxis * airMovementPenalty * delta;
                }
            }
            //Max speed limit check
            if (currentXSpeed >= maxAirSpeed)
            {
                currentXSpeed = maxAirSpeed * xAxis;
            }
            else if (currentXSpeed <= -maxAirSpeed)
            {
                currentXSpeed = maxAirSpeed * xAxis;
            }
        }
        else //Movement when standing on floor
        {
            if (xAxis > 0)
            {
                if (currentXSpeed < 0)
                {
                    currentXSpeed += moveSpeed * 1.5f * xAxis * delta;
                }
                else if (currentXSpeed < maxSpeed)
                {
                    currentXSpeed += moveSpeed * xAxis * delta;
                }
            }
            else
            {
                if (currentXSpeed > 0)
                {
                    currentXSpeed += moveSpeed * 1.5f * xAxis * delta;
                }
                else if (currentXSpeed > -maxSpeed)
                {
                    currentXSpeed += moveSpeed * xAxis * delta;
                }
            }
            //Max speed limit check
            if (currentXSpeed >= maxSpeed)
            {
                currentXSpeed = maxSpeed * xAxis;
            }
            else if (currentXSpeed <= -maxSpeed)
            {
                currentXSpeed = maxSpeed * xAxis;
            }
        }
    }

    void StopMovement() //When player is not pressing horizontal axis, apply force to stop the movement
    {
        float direction = 0;
        if(transform.position.x > lastPosition.x)
        {
            direction = 1;
        }
        else if(transform.position.x < lastPosition.x)
        {
            direction = -1;
        }

        if (onFloor) 
        { 
            if (currentXSpeed <= .05f && currentXSpeed >= -.05f)
            {
                currentXSpeed = 0;
            }
            else
            {
                currentXSpeed -= moveSpeed * 1.5f * direction * delta;
            } 
        }
        else if (!onFloor)
        {
            if (currentXSpeed <= .05f && currentXSpeed >= -.05f)
            {
                currentXSpeed = 0;
            }
            else
            {
                currentXSpeed -= moveSpeed * airMovementPenalty * direction * delta;
            } 
        }
    }

    void Gravity() //Applied every frame when not standing on floor
    {
        if (!onFloor)
        {
            currentYSpeed -= gravity * delta;
            if(currentYSpeed <= -maxFallSpeed)
            {
                currentYSpeed = -maxFallSpeed;
            }
        }
        else if (onFloor)
        {
            currentYSpeed = 0;
        }
    }

    void Jump()
    {
        //Initial jump push
        if (CheckForJump() && !jumping && !jumpInUse)
        {
            jumpInUse = true;
            transform.position = new Vector2(transform.position.x, transform.position.y + .003f);
            currentYSpeed = jumpStartSpeed;
            jumping = true;
            jumpForceApplied = 0;
            JumpSound();
        }
        //Aditional force if player holds jump axis
        if (jumping && maxJumpForce > jumpForceApplied + (jumpStaySpeed * delta))
        {
            jumpForceApplied += jumpStaySpeed * delta;
            currentYSpeed += jumpStaySpeed * delta;
        }
        //Add remaining force if player is still holding jump
        else if (jumping && maxJumpForce > jumpForceApplied)
        {
            float remainingForce = maxJumpForce - jumpForceApplied;
            jumpForceApplied += remainingForce;
            currentYSpeed += remainingForce;
            jumping = false;
        }
    }

    void ApplyMovementLimits() //If walls or ceiling is detected, stop movement towards them
    {
        if (rightWall && currentXSpeed > 0 && !onFloor && currentYSpeed <=0)
        {
            currentXSpeed = 0;
            WallGrab(); //Do wall grab if player is walking towards the wall and not standing on floor
        }
        else if (rightWall && currentXSpeed > 0)
        {
            currentXSpeed = 0;
        }
        if (leftWall && currentXSpeed < 0 && !onFloor && currentYSpeed <= 0)
        {
            currentXSpeed = 0;
            WallGrab(); //Do wall grab if player is walking towards the wall and not standing on floor
        }
        else if (leftWall && currentXSpeed < 0)
        {
            currentXSpeed = 0;
        }
        if (ceiling && currentYSpeed > 0)
        {
            currentYSpeed = 0;
        }

    }

    void WallGrab() //Reduce falling speed
    {
        currentYSpeed += wallGrabFriction * delta;

        if(currentYSpeed < wallGrabMaxFallSpeed)
        {
            currentYSpeed = -wallGrabMaxFallSpeed;
        }
    }

    void WallJump() //Can be done when near a wall
    {
        if (!onFloor && !jumpInUse && !wallJumping)
        {
            jumpInUse = true;
            wallJumpDirection = CheckBothWalls();
            if(wallJumpDirection > 0 || wallJumpDirection < 0)
            {
                currentXSpeed = wallJumpStartXForce * wallJumpDirection;
                wallJumpXForceApplied = 0;
                currentYSpeed = wallJumpStartYForce;
                wallJumpYForceApplied = 0;
                wallJumping = true;
                JumpSound();
            }
        }
        if (wallJumping && wallJumpMaxXForce > wallJumpXForceApplied + (wallJumpStayXForce * delta))
        {
            wallJumpXForceApplied += wallJumpStayXForce * delta;
            currentXSpeed += wallJumpStayXForce * delta * wallJumpDirection;
            Debug.Log("Aqui toy");
        }
        else if (wallJumping && wallJumpMaxXForce > wallJumpXForceApplied)
        {
            float remainingForce = wallJumpMaxXForce - wallJumpXForceApplied;
            wallJumpXForceApplied += remainingForce;
            currentXSpeed += remainingForce * wallJumpDirection;
        }
        if (wallJumping && wallJumpMaxYForce > wallJumpYForceApplied + (wallJumpStayYForce * delta))
        {
            wallJumpYForceApplied += wallJumpStayYForce * delta;
            currentYSpeed += wallJumpStayYForce * delta;
        }
        else if (wallJumping && wallJumpMaxYForce > wallJumpYForceApplied)
        {
            float remainingForce = wallJumpMaxYForce - wallJumpYForceApplied;
            wallJumpYForceApplied += remainingForce;
            currentYSpeed += remainingForce;
        }
        if(wallJumping && wallJumpXForceApplied >= wallJumpMaxXForce && wallJumpYForceApplied >= wallJumpMaxYForce)
        {
            wallJumping = false;
        }
    }

    //WALL AND FLOOR DETECTION FUNCTIONS
    bool CheckOnFloor()
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 leftRayPos = new Vector2(transform.position.x - .035f, transform.position.y);
        Vector2 rightRayPos = new Vector2(transform.position.x + .035f, transform.position.y);

        //Centro, izquierda y derecha
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.down, .08f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(leftRayPos, Vector2.down, .08f, mask);
        RaycastHit2D hitR = Physics2D.Raycast(rightRayPos, Vector2.down, .08f, mask);

        if (hitC)
        {
            if(!adjustedToFloor)
            {
              AdjustToFloor(hitC);
            }
            return true;
        }
        else if(hitL)
        {
            if(!adjustedToFloor)
            {
              AdjustToFloor(hitL);
            }
            return true;
        }
        else if(hitR)
        {
            if(!adjustedToFloor)
            {
              AdjustToFloor(hitR);
            }
            return true;
        }
        else
        {
            adjustedToFloor = false;
            animator.SetBool("walking", false);
            return false;
        }
    }

    bool CheckForJump()//Only to check if you can jump, its different from the normal floor check to make the input feel more responsive
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 leftRayPos = new Vector2(transform.position.x - .035f, transform.position.y);
        Vector2 rightRayPos = new Vector2(transform.position.x + .035f, transform.position.y);

        //Centro, izquierda y derecha
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.down, .09f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(leftRayPos, Vector2.down, .09f, mask);
        RaycastHit2D hitR = Physics2D.Raycast(rightRayPos, Vector2.down, .09f, mask);

        if (hitC || hitL || hitR)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void AdjustToFloor(RaycastHit2D hit)
    {
        transform.position = new Vector2(transform.position.x,hit.point.y + .08f);
        adjustedToFloor = true;
    }

    bool CheckRightWalls()
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 upperRayPos = new Vector2(transform.position.x, transform.position.y + .075f);
        Vector2 centerUpRayPos = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 centerLowRayPos = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 lowerRayPos = new Vector2(transform.position.x, transform.position.y - .075f);

        RaycastHit2D hitU = Physics2D.Raycast(upperRayPos, Vector2.right, .055f, mask);
        RaycastHit2D hitUC = Physics2D.Raycast(centerUpRayPos, Vector2.right, .055f, mask);
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.right, .055f, mask);
        RaycastHit2D hitLC = Physics2D.Raycast(centerLowRayPos, Vector2.right, .055f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(lowerRayPos, Vector2.right, .055f, mask);

        if (hitC)
        {
            AdjustToWall(hitC, -1);
            return true;
        }
        else if (hitUC)
        {
            AdjustToWall(hitUC, -1);
            return true;
        }
        else if (hitLC)
        {
            AdjustToWall(hitLC, -1);
            return true;
        }
        else if (hitU)
        {
            AdjustToWall(hitU, -1);
            return true;

        }
        else if (hitL)
        {
            AdjustToWall(hitL, -1);
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CheckLeftWalls()
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 upperRayPos = new Vector2(transform.position.x, transform.position.y + .075f);
        Vector2 centerUpRayPos = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 centerLowRayPos = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 lowerRayPos = new Vector2(transform.position.x, transform.position.y - .075f);

        RaycastHit2D hitU = Physics2D.Raycast(upperRayPos, Vector2.left, .055f, mask);
        RaycastHit2D hitUC = Physics2D.Raycast(centerUpRayPos, Vector2.left, .055f, mask);
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.left, .055f, mask);
        RaycastHit2D hitLC = Physics2D.Raycast(centerLowRayPos, Vector2.left, .055f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(lowerRayPos, Vector2.left, .055f, mask);

        if (hitC)
        {
            AdjustToWall(hitC, 1);
            return true;
        }
        else if (hitUC)
        {
            AdjustToWall(hitUC, 1);
            return true;
        }
        else if (hitLC)
        {
            AdjustToWall(hitLC, 1);
            return true;
        }
        else if (hitU)
        {
            AdjustToWall(hitU, 1);
            return true;

        }
        else if (hitL)
        {
            AdjustToWall(hitL, 1);
            return true;
        }
        else
        {
            return false;
        }
    }

    void AdjustToWall(RaycastHit2D hit, int dir)
    {
        transform.position = new Vector2(hit.point.x + .055f * dir, transform.position.y);
    }

    int CheckBothWalls() //Used for wall jumping
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 upperRayPosR = new Vector2(transform.position.x, transform.position.y + .075f);
        Vector2 centerUpRayPosR = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 centerRayPosR = new Vector2(transform.position.x, transform.position.y);
        Vector2 centerLowRayPosR = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 lowerRayPosR = new Vector2(transform.position.x, transform.position.y - .075f);
        Vector2 upperRayPosL = new Vector2(transform.position.x, transform.position.y + .075f);
        Vector2 centerUpRayPosL = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 centerRayPosL = new Vector2(transform.position.x, transform.position.y);
        Vector2 centerLowRayPosL = new Vector2(transform.position.x, transform.position.y + .0375f);
        Vector2 lowerRayPosL = new Vector2(transform.position.x, transform.position.y - .075f);

        RaycastHit2D hitUR = Physics2D.Raycast(upperRayPosR, Vector2.right, .065f, mask);
        RaycastHit2D hitUCR = Physics2D.Raycast(centerUpRayPosR, Vector2.right, .065f, mask);
        RaycastHit2D hitCR = Physics2D.Raycast(centerRayPosR, Vector2.right, .065f, mask);
        RaycastHit2D hitLCR = Physics2D.Raycast(centerLowRayPosR, Vector2.right, .065f, mask);
        RaycastHit2D hitLR = Physics2D.Raycast(lowerRayPosR, Vector2.right, .065f, mask);
        RaycastHit2D hitUL = Physics2D.Raycast(upperRayPosL, Vector2.left, .065f, mask);
        RaycastHit2D hitUCL = Physics2D.Raycast(centerUpRayPosL, Vector2.left, .065f, mask);
        RaycastHit2D hitCL = Physics2D.Raycast(centerRayPosL, Vector2.left, .065f, mask);
        RaycastHit2D hitLCL = Physics2D.Raycast(centerLowRayPosL, Vector2.left, .065f, mask);
        RaycastHit2D hitLL = Physics2D.Raycast(lowerRayPosL, Vector2.left, .065f, mask);

        if(lastDirection == 1)
        {
            if(hitCR || hitUCR || hitLCR || hitUR || hitLR)
            {
                return -1;
            }
            else if (hitCL || hitUCL || hitLCL || hitUL || hitLL)
            {
                return 1;
            }
        }
        else if(lastDirection == -1)
        {
            if (hitCL || hitUCL || hitLCL || hitUL || hitLL)
            {
                return 1;
            }
            else if (hitCR || hitUCR || hitLCR || hitUR || hitLR)
            {
                return -1;
            }
        }
        return 0;
    }

    bool CheckCeiling()
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y);
        Vector2 leftRayPos = new Vector2(transform.position.x - .035f, transform.position.y);
        Vector2 rightRayPos = new Vector2(transform.position.x + .035f, transform.position.y);

        //Centro, izquierda y derecha
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.up, .08f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(leftRayPos, Vector2.up, .08f, mask);
        RaycastHit2D hitR = Physics2D.Raycast(rightRayPos, Vector2.up, .08f, mask);

        if (hitC || hitL || hitR)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //SOUND FUNCTIONS
    void StepSound()
    {
        if (!audioPlayer.isPlaying)
        {
            audioPlayer.clip = stepSound;
            audioPlayer.Play();
        }
    }

    void JumpSound()
    {
        audioPlayer.clip = jumpSound;
        audioPlayer.Play();
    }
    
    //Wait a bit before level starts to avoid collision shenanigans
    IEnumerator WaitUntilLoad()
    {
        yield return new WaitForSeconds(.5f);
        updateStart = true;
    }
}