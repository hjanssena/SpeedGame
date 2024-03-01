using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    //Movement on x
    [Header("Movement on x")]
    [SerializeField] float moveSpeed;
    float currentXSpeed;
    [SerializeField] float maxSpeed;
    bool onFloor;
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
    bool rightWall;
    bool leftWall;
    bool ceiling;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        onFloor = CheckOnFloor();

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
        transform.Translate(currentXSpeed, 0, 0);
        transform.Translate(0, currentYSpeed, 0);
    }

    void MovementOnX() //1 for right, -1 for left, 0 for nothing
    {
        float xAxis = Input.GetAxis("Horizontal");
        currentXSpeed += moveSpeed * Time.deltaTime * xAxis;
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

        if (currentXSpeed <= .0005f && currentXSpeed >= -.0005f)
        {
            currentXSpeed = 0;
        }
        else
        {
            currentXSpeed -= moveSpeed * direction * Time.deltaTime;
        }
    }

    void Gravity()
    {
        if (!onFloor)
        {
            currentYSpeed -= gravity * Time.deltaTime;
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
            transform.position = new Vector2(transform.position.x, transform.position.y + .0015f);
            currentYSpeed = jumpStartSpeed;
            jumpStartTime = Time.time;
            jumping = true;
        }
        if (jumping && jumpStartTime + maxJumpDuration >= Time.time)
        {
            currentYSpeed += jumpStaySpeed * Time.deltaTime;
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

    bool CheckRightWalls()
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
}
