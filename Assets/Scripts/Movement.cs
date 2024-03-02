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
    bool onFloor;
    bool updateStart = false;
    bool adjustedToFloor;
    Vector2 lastPosition;

    //Movement on y
    [Header("Movement on y")]
    [SerializeField] float gravity;
    [SerializeField] float maxFallSpeed;
    [SerializeField] float jumpStartSpeed;
    [SerializeField] float jumpStaySpeed;
    float currentYSpeed;
    float jumpStartTime;
    [SerializeField] float maxJumpDuration;
    bool jumping;

    //Collisions with walls
    [SerializeField] bool rightWall;
    [SerializeField] bool leftWall;
    [SerializeField] bool ceiling;

    void Start()
    {
        lastPosition = transform.position;
        animator = GetComponent<Animator>();
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
            }
            else
            {
                jumping = false;
            }

            //movement
            ApplyMovementLimits();
            lastPosition = transform.position;
            transform.Translate(currentXSpeed * delta, 0, 0);
            transform.Translate(0, currentYSpeed * delta, 0);
        }
    }

    void MovementOnX() //1 for right, -1 for left, 0 for nothing
    {
        float xAxis = Input.GetAxis("Horizontal");
        if(xAxis > 0)
        {
            if(currentXSpeed < 0)
            {
                currentXSpeed += moveSpeed * 1.5f * xAxis * delta;
            }
            else
            {
                currentXSpeed += moveSpeed * xAxis * delta;
            }
        }
        else
        {
            if(currentXSpeed > 0)
            {
                currentXSpeed += moveSpeed * 1.5f * xAxis * delta;
            }
            else
            {
                currentXSpeed += moveSpeed * xAxis * delta;
            }
        }
        if (currentXSpeed >= maxSpeed)
        {
            currentXSpeed = maxSpeed * xAxis;
        }
        else if (currentXSpeed <= -maxSpeed)
        {
                currentXSpeed = maxSpeed * xAxis;
        }
    }

    void StopMovement()
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

        if (currentXSpeed <= .05f && currentXSpeed >= -.05f)
        {
            currentXSpeed = 0;
        }
        else
        {
            currentXSpeed -= moveSpeed * 1.5f * direction * delta;
        }
    }

    void Gravity()
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
        if (onFloor && !jumping)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + .003f);
            currentYSpeed = jumpStartSpeed;
            jumpStartTime = Time.time;
            jumping = true;
        }
        if (jumping && jumpStartTime + maxJumpDuration >= Time.time)
        {
            currentYSpeed += jumpStaySpeed * delta;
        }
    }

    void ApplyMovementLimits()
    {
        if(rightWall && currentXSpeed > 0)
        {
            currentXSpeed = 0;
        }
        if(leftWall && currentXSpeed < 0)
        {
            currentXSpeed = 0;
        }
        if(ceiling && currentYSpeed > 0)
        {
            currentYSpeed = 0;
        }
    }

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

        if (hitC || hitUC || hitLC || hitU || hitL)
        {
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

        if (hitC || hitUC || hitLC || hitU || hitL)
        {
            return true;
        }
        else
        {
            return false;
        }
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

    IEnumerator WaitUntilLoad()
    {
        yield return new WaitForSeconds(.5f);
        updateStart = true;
    }
}
