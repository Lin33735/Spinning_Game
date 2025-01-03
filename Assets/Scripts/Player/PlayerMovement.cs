using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : Entity
{
    
    Vector2 PlayerInput, RawInput;
    Vector2 FaceDirection;
    public Transform Target;   // 链子目标
    public Transform Effect;
    public int segmentCount = 10;  // 链子段数
    public LineRenderer lineRenderer;
    [SerializeField]private float CurveAmount;

    bool walkable, attackable, runable;
    public enum State { 
        Idle,
        Walking,
        Attacking,
    }
    bool isrunning;
    public State currentState;
    public void Start()
    {
        walkable=true;
        attackable=true;
        runable=true;
        if (GetComponent<LineRenderer>())
            lineRenderer = GetComponent<LineRenderer>();
        else
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount + 1;
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        StateUpdate(currentState);
    }
    protected override void Update()
    {
        base.Update();
        GetInput();
        
        
    }
    public void GetInput()
    {
        PlayerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        RawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (RawInput != Vector2.zero)
        {
            FaceDirection = RawInput.normalized;
            if(walkable)
            ChangeState(State.Walking);
        }
        else if (currentState == State.Walking)
        {
            ChangeState(State.Idle);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isrunning = isrunning ? false : true;
            animator.SetBool("running", isrunning);
        }
    }
    public void ChangeState(State state)
    {
        if (currentState != state)
        {
            ExitState(currentState);
            currentState = state;
            EnterState(currentState);
        }
    }
    public void EnterState(State state)
    {
        if (state == State.Walking)
        {
            animator.SetBool("walking", true);
        }
        if (state == State.Attacking)
        {
            animator.SetTrigger("attack");
            animator.SetBool("attacking", true);
            animator.SetBool("walking", true);
            walkable = false;
            
            StartCoroutine(AttachRope());
        }
        if (state == State.Idle)
        {
            animator.SetBool("walking", false);
            animator.SetBool("running", false);
        }
    }
    public void StateUpdate(State state)
    {
        if (state == State.Walking)
        {
            FaceTo((Vector2)transform.position + FaceDirection);
            Vector2 _speed;
            if (isrunning)
                _speed = FaceDirection * speed * 2;
            else
                _speed = FaceDirection * speed;
            if(rb.velocity.magnitude<= _speed.magnitude)
            {
                rb.velocity= _speed;
            }

        }
        if(state == State.Attacking)
        {
            
        }
        if(state == State.Idle)
        {
            rb.velocity = rb.velocity / 2;
        }
    }
    public void ExitState(State state)
    {
        if(state == State.Walking)
        {
            animator.SetBool("walking", false);
            animator.SetBool("running", false);
            isrunning = false;
        }
        if (state == State.Attacking)
        {
            animator.SetBool("attacking", false);
            walkable = true;
            Effect.GetComponent<ParticleSystem>().Stop();
            lineRenderer.enabled = false;
        }
    }
    
    
    public virtual IEnumerator AttachRope()
    {
        
        if (!Target)
        {
            ChangeState(State.Idle);
            yield break;
        }
        float counter = 0;
        CurveAmount = 0;
        rb.velocity = rb.velocity + (Vector2)(Target.position-transform.position).normalized*-5;
        while (Target && counter <= 0.3f)
        {
            counter += Time.deltaTime;

            yield return null;
        }
        counter = 0;
        lineRenderer.enabled = true;
        while (Target && counter <= 1f)
        {

            CurveAmount = Mathf.Sin(counter*6 * Mathf.PI)*(1- counter)*0.6f;
            for (int i = 0; i <= segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector3 position = Vector3.Lerp(transform.position, Target.position, t);
                position.y += Mathf.Sin((1-counter)*6*t * Mathf.PI) * CurveAmount; // 模拟弯曲效果
                lineRenderer.SetPosition(i, position);
            }
            counter += Time.deltaTime*2;
            yield return null;
        }
        if (!Target)
        {
            ChangeState(State.Idle);
            yield break;
        }


        rb.AddForce((Target.position - transform.position).normalized * 10);
        Vector2 direction = (Target.position - transform.position).normalized;
        Vector2 startvector = new Vector2(direction.y, -direction.x);
        float InputDirection = Mathf.Sign(Vector2.Dot(FaceDirection, startvector));
        if(RawInput==Vector2.zero)
            InputDirection = 0;
        float speed=5;
        float charge=0;
        animator.SetTrigger("attack");
        while (Target)
        {
            InputDirection = Mathf.Sign(Vector2.Dot(FaceDirection, startvector));
            if (RawInput == Vector2.zero)
                InputDirection = 0;
            direction = (Target.position - transform.position).normalized;
            if (InputDirection == 0)
            {
                rb.AddForce(direction * speed);
            }
            else
            {
                rb.AddForce(direction * speed * Vector2.Distance(transform.position, Target.transform.position) * 0.2f);
            }

            direction = new Vector2(direction.y, -direction.x);
            rb.AddForce(direction * speed* InputDirection);
            
            if (RawInput == Vector2.zero)
            {
                startvector = direction;
                FaceTo(Target.transform.position);
            }
            else
            {
                FaceTo((Vector2)transform.position + direction * speed * InputDirection);
            }
            for (int i = 0; i <= segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector3 position = Vector3.Lerp(transform.position, Target.position, t);
                //position.y += Mathf.Sin((1 - counter) * 6 * t * Mathf.PI) * CurveAmount; // 模拟弯曲效果
                lineRenderer.SetPosition(i, position);
            }

            charge += Time.deltaTime;
            if (charge >= 2&&speed<=20)
            {
                if (speed == 5)
                {
                    animator.SetTrigger("end");
                    Effect.GetComponent<TrailRenderer>().enabled=true;
                    Effect.GetComponent<ParticleSystem>().Play();
                }

                speed +=5;
                charge = 0;
                
            }
            yield return null;
        }
        Effect.GetComponent<TrailRenderer>().enabled=false;

        lineRenderer.enabled = false;
        while (rb.velocity.magnitude>=5f)
        {

            yield return null;
        }
        ChangeState(State.Idle);
    }
    
}
