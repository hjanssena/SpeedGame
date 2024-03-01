using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    float delta;
    //Movement on x
    [Header("Movement on x")]
    [SerializeField] float moveSpeed;
    [SerializeField] float currentXSpeed;
    [SerializeField] float maxSpeed;
    bool onFloor;
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
    }

    void Update()
    {
        delta = Time.unscaledDeltaTime * Time.timeScale;
        onFloor = CheckOnFloor();
        rightWall = CheckRightWalls();

        //xMovement
        if (Input.GetAxis("Horizontal") > 0f || Input.GetAxis("Horizontal") < 0f) 
        {
            MovementOnX();
        }
        else
        {
            StopMovement();
        }

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
        lastPosition = transform.position;
        transform.Translate(currentXSpeed * delta, 0, 0);
        transform.Translate(0, currentYSpeed * delta, 0);
        if(onFloor && !adjustedToFloor)
        {
           //AdjustToFloor();
        }
    }

    void MovementOnX() //1 for right, -1 for left, 0 for nothing
    {
        float xAxis = Input.GetAxis("Horizontal");
        currentXSpeed += moveSpeed * xAxis * delta;
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
            currentXSpeed -= moveSpeed * direction * delta;
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

    bool CheckOnFloor()
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y - .05f);
        Vector2 leftRayPos = new Vector2(transform.position.x - .035f, transform.position.y - .05f);
        Vector2 rightRayPos = new Vector2(transform.position.x + .035f, transform.position.y - .05f);

        //Centro, izquierda y derecha
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.down, .05f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(leftRayPos, Vector2.down, .05f, mask);
        RaycastHit2D hitR = Physics2D.Raycast(rightRayPos, Vector2.down, .05f, mask);

        if (hitC || hitL || hitR)
        {
            return true;
        }
        else
        {
            adjustedToFloor = false;
            return false;
        }
    }

    void AdjustToFloor()
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y - .05f);
        Vector2 leftRayPos = new Vector2(transform.position.x - .035f, transform.position.y - .05f);
        Vector2 rightRayPos = new Vector2(transform.position.x + .035f, transform.position.y - .05f);

        //Centro, izquierda y derecha
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.down, .05f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(leftRayPos, Vector2.down, .05f, mask);
        RaycastHit2D hitR = Physics2D.Raycast(rightRayPos, Vector2.down, .05f, mask);

        if (hitC)
        {
            transform.position = new Vector2(transform.position.x,hitC.point.y);
            Debug.Log(hitC.point.y);
            adjustedToFloor = true;
        }
        else if (hitL)
        {
            transform.position = new Vector2(transform.position.x,hitL.point.y);
            adjustedToFloor = true;
        }
        else
        {
            transform.position = new Vector2(transform.position.x,hitR.point.y);
            adjustedToFloor = true;
        }
    }

    bool CheckRightWalls()
    {
        LayerMask mask;
        mask = (1 << 6);

        Vector2 centerRayPos = new Vector2(transform.position.x + .02f, transform.position.y);
        Vector2 upperRayPos = new Vector2(transform.position.x + .02f, transform.position.y + .05f);
        Vector2 lowerRayPos = new Vector2(transform.position.x + .02f, transform.position.y - .05f);

        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.right, .035f, mask);
        RaycastHit2D hitU = Physics2D.Raycast(upperRayPos, Vector2.right, .035f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(lowerRayPos, Vector2.right, .035f, mask);

        if (hitC || hitU || hitL)
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

        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y - .05f);
        Vector2 leftRayPos = new Vector2(transform.position.x - .035f, transform.position.y - .05f);
        Vector2 rightRayPos = new Vector2(transform.position.x + .035f, transform.position.y - .05f);

        //Centro, izquierda y derecha
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.down, .035f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(leftRayPos, Vector2.down, .035f, mask);
        RaycastHit2D hitR = Physics2D.Raycast(rightRayPos, Vector2.down, .035f, mask);

        if (hitC || hitL || hitR)
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

        Vector2 centerRayPos = new Vector2(transform.position.x, transform.position.y - .05f);
        Vector2 leftRayPos = new Vector2(transform.position.x - .035f, transform.position.y - .05f);
        Vector2 rightRayPos = new Vector2(transform.position.x + .035f, transform.position.y - .05f);

        //Centro, izquierda y derecha
        RaycastHit2D hitC = Physics2D.Raycast(centerRayPos, Vector2.down, .035f, mask);
        RaycastHit2D hitL = Physics2D.Raycast(leftRayPos, Vector2.down, .035f, mask);
        RaycastHit2D hitR = Physics2D.Raycast(rightRayPos, Vector2.down, .035f, mask);

        if (hitC || hitL || hitR)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //void OnCollisionEnter2D(Collision2D col)
    //{
    //    Debug.Log("si");
    //    if(col.collider.gameObject.layer == 6)
    //    {
    //        onFloor = true;
    //        Debug.Log("si");
    //    }
    //}
}
