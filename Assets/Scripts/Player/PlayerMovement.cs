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
    float chargelevel;
    bool walkable, attackable, runable;
    bool isrunning, isspinning;
    public enum State { 
        Idle,
        Walking,
        Attacking,
    }
    
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
                _speed = FaceDirection * speed * 1.5f;
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
            isspinning = false;
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
        while (Target && counter <= 0.15f)
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
        float speed= this.speed;
        float charge=0;
        chargelevel = 0;
        animator.SetTrigger("attack");
        while (Target)
        {
            if (Vector2.Distance(transform.position, Target.transform.position) <= 1)
            {
                Target.GetComponent<Entity>().GetHit(speed>30? 1+chargelevel: 1 + chargelevel*2, rb.velocity/2);
                Target = null;
                break;
            }
            InputDirection = Mathf.Sign(Vector2.Dot(FaceDirection, startvector));
            
            direction = (Target.position - transform.position).normalized;
            if (RawInput == Vector2.zero)
            {
                rb.AddForce(direction * speed);
                if (speed >= 30)
                {
                    rb.AddForce(direction * speed);
                    direction = new Vector2(direction.y, -direction.x);
                    rb.AddForce(direction * speed * -InputDirection);
                    
                }
            }
            else
            {
                if(speed> this.speed)
                    rb.AddForce(direction * speed*4 * Vector2.Distance(transform.position, Target.transform.position) * 0.05f);
                else
                    rb.AddForce(direction * speed * Vector2.Distance(transform.position, Target.transform.position) * 0.05f);
            }
            if (RawInput == Vector2.zero)
                InputDirection = 0;
            direction = new Vector2(direction.y, -direction.x);
            rb.AddForce(direction * speed* InputDirection);
            
            if (RawInput == Vector2.zero)
            {
                if (speed < this.speed+5)
                {
                    speed = this.speed;
                    charge = 0;

                }
                
                //startvector = direction;
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
            if (charge >= 3&&speed<=30)
            {
                if (speed == this.speed)
                {
                    animator.SetTrigger("end");
                    isspinning=true;
                }
                speed +=5;
                chargelevel += 1;
                charge = 1;
                if (speed > 30)
                {
                    Effect.GetComponent<TrailRenderer>().enabled = true;
                    Effect.GetComponent<ParticleSystem>().Play();
                }

            }
            yield return null;
        }
        Effect.GetComponent<TrailRenderer>().enabled=false;

        lineRenderer.enabled = false;
        rb.drag = rb.drag / 3;
        while (isspinning&&rb.velocity.magnitude>=3f)
        {
            if (RawInput != Vector2.zero)
            {
                float magnitude = rb.velocity.magnitude;
                float currentAngle = Mathf.Atan2(rb.velocity.y, rb.velocity.x);
                float targetAngle = Mathf.Atan2(FaceDirection.y, FaceDirection.x);
                float newAngle = Mathf.LerpAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg, Time.deltaTime * 5) * Mathf.Deg2Rad;
                Vector2 newVelocity = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)) * magnitude;

                rb.velocity = newVelocity;
            }
            
            yield return null;
        }
        rb.drag = rb.drag *3;
        ChangeState(State.Idle);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isspinning&&collision.gameObject.tag=="Enemy")
        {
            collision.gameObject.GetComponent<Entity>().GetHit(0.2f + chargelevel/3 , rb.velocity/2);
            rb.velocity = rb.velocity * -1f;
           
        }
    }


}
