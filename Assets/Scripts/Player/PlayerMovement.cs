using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : Entity
{

    Vector2 PlayerInput, RawInput,LastPosition;
    Vector2 FaceDirection;
    [Header("Private Componets")]
    public Transform Target;   // 链子目标
    public Transform Effect;
    public int segmentCount = 10;  // 链子段数
    public LineRenderer lineRenderer;
    [SerializeField]private float CurveAmount;
    bool walkable, attackable, runable;
    bool isrunning, isspinning;
    public float charge,ChargeLevel,timer;
    bool fall;
    public enum State
    {
        Idle,
        Walking,
        Attacking,
        Falling
    }

    public State currentState;

    [Header("Sound Effects")]
    public AudioClip FootStep, Spinning, Charge, StartSpin;
    public enum SoundEffect
    {
        Walking,
        Spinning,
        Charge,
        StartSpin,
        Swing
    }

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
    protected override void Awake()
    {
        maxgethitcd = 2;
        base.Awake();
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
        if(state == State.Falling){
            walkable = false;
            fall = true;
            timer = 0;
            rb.velocity = Vector2.zero;
        }
    }
    public void StateUpdate(State state)
    {
        if (state == State.Walking)
        {
            FaceTo((Vector2)transform.position + FaceDirection);
            CurrentSpeed += (speed - CurrentSpeed) * 0.1f;
            Vector2 _speed;
            if (isrunning)
                _speed = FaceDirection * CurrentSpeed * 1.7f;
            else
                _speed = FaceDirection * CurrentSpeed;
            if(rb.velocity.magnitude<= _speed.magnitude)
            {
                rb.velocity = _speed;
            }

        }
        if(state == State.Attacking)
        {
            
        }
        if(state == State.Idle)
        {
            if (!fall)
            {
                LastPosition = transform.position;
            }
        }
        if (state == State.Falling)
        {
            timer += Time.fixedDeltaTime;
            if (transform.localScale.y > 0)
            {
                transform.localScale = transform.localScale * 0.9f;
                transform.eulerAngles += new Vector3(0, 0, 1);
            }
            if (fall&&timer >= 1)
            {
                fall = false;
                rb.MovePosition(LastPosition + (LastPosition - (Vector2)transform.position) * 0.05f);
                
            }
            if (timer >= 1.1f)
            {
                transform.localScale = localScale;
                transform.eulerAngles = new Vector3(0, 0, 0);
                GetHit(5, Vector2.zero, true);
                ChangeState(State.Idle);
                walkable = true;
            }
        }
    }
    public void ExitState(State state)
    {
        if(state == State.Walking)
        {
            animator.SetBool("walking", false);
            animator.SetBool("running", false);
            isrunning = false;
            rb.velocity = Vector2.zero;
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
            if (currentState == State.Attacking)
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
        while (Target && counter <= 0.5f)
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
            if (currentState == State.Attacking)
                ChangeState(State.Idle);
            yield break;
        }


        rb.AddForce((Target.position - transform.position).normalized * 10);
        float Distance = 1+(Target.position - transform.position).magnitude;
        Vector2 direction = (Target.position - transform.position).normalized;
        Vector2 startvector = new Vector2(direction.y, -direction.x);
        float InputDirection = Vector2.Dot(FaceDirection, startvector);
        if(RawInput==Vector2.zero)
            InputDirection = 0;
        float speed= this.speed;
        charge=2;
        float chargelevel=0;
        animator.SetTrigger("attack");
        // float deltatime = Time.deltaTime/(1f / 720f);
        float deltatime = 10;
        print(deltatime);
        while (Target)
        {
            if (Vector2.Distance(transform.position, Target.transform.position) <= Target.GetComponent<Entity>().Size)
            {
                if (Target.tag=="Enemy")
                {

                    Target.GetComponent<Entity>().GetHit(speed > 30 ? 1 + ChargeLevel * 3 : 1 + ChargeLevel, rb.velocity / 2, true);
                    
                    GameManager.Instance.PauseTime(0 + ChargeLevel * 1f, ChargeLevel / 2);
                    if (Target.GetComponent<Entity>().health > 0)
                    {
                        ChargeLevel = 0;
                    }

                    Target = null;
                    break;
                }
                
            }
            if (chargelevel > 0)
            {
                gethitcd = 0.5f;
            }

            direction = (Target.position - transform.position).normalized;
            InputDirection = Mathf.Sign(Vector2.Dot(FaceDirection, startvector));
            if (RawInput == Vector2.zero)
            {
                //startvector = new Vector2(direction.y, -direction.x);
                Distance = 1+(Target.position - transform.position).magnitude;
                rb.AddForce(direction * speed * rb.mass * deltatime);
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
                {
                    rb.AddForce(direction / (Distance * 0.05f) * (speed * speed) / 30 * rb.mass * deltatime);
                }
                else
                {
                    rb.AddForce(direction / (Distance * 0.05f) * rb.mass *  deltatime);

                }

            }

            if (RawInput == Vector2.zero)
                InputDirection = 0;
            direction = new Vector2(direction.y, -direction.x);
            rb.AddForce(direction * speed* InputDirection*rb.mass * deltatime);
            
            if (RawInput == Vector2.zero)
            {
                
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
            if ((charge >= 5||ChargeLevel> chargelevel) && chargelevel < 6)
            {
                if (speed == this.speed)
                {
                    animator.SetTrigger("end");
                    isspinning=true;
                }
                speed +=5;
                chargelevel += 1;
                SoundTrigger(SoundEffect.Charge);
                if (chargelevel > ChargeLevel)
                {
                    ChargeLevel += 1;
                }
                charge = 4;

                if (speed > 30)
                {
                    Effect.GetComponent<TrailRenderer>().enabled = true;
                    Effect.GetComponent<ParticleSystem>().Play();
                }

            }
            yield return null;
        }
        SoundTrigger(SoundEffect.StartSpin);
        rb.velocity = rb.velocity.magnitude*(GameManager.Instance.MousePosition-(Vector2)transform.position).normalized;
        Effect.GetComponent<TrailRenderer>().enabled=false;

        lineRenderer.enabled = false;
        rb.drag = rb.drag / 3;
        while (isspinning&&rb.velocity.magnitude>=3f)
        {
            if (RawInput != Vector2.zero&& rb.velocity.magnitude <= 15f)
            {
                float magnitude = rb.velocity.magnitude;
                float currentAngle = Mathf.Atan2(rb.velocity.y, rb.velocity.x);
                float targetAngle = Mathf.Atan2(FaceDirection.y, FaceDirection.x);
                float newAngle = Mathf.LerpAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg, Time.deltaTime * 5) * Mathf.Deg2Rad;
                Vector2 newVelocity = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)) * magnitude;

                rb.velocity = newVelocity;
            }
            else
            {
                float magnitude = rb.velocity.magnitude;
                Vector2 vector2 = (GameManager.Instance.MousePosition - (Vector2)transform.position).normalized;
                float currentAngle = Mathf.Atan2(rb.velocity.y, rb.velocity.x);
                float targetAngle = Mathf.Atan2(vector2.y, vector2.x);
                float newAngle = Mathf.LerpAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg, Time.deltaTime * 5) * Mathf.Deg2Rad;
                Vector2 newVelocity = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)) * magnitude;

                rb.velocity = newVelocity;
            }
            
            yield return null;
        }
        rb.drag = rb.drag *3;
        if(currentState == State.Attacking)
        ChangeState(State.Idle);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Cliff" )
        {
            fall = true;
            if (!isspinning)
            {
                ChangeState(State.Falling);
                Target = null;
            }

        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag=="Enemy"&& collision.GetComponent<Entity>())
        {
            if(isspinning)
                if (Target)
                {
                    collision.GetComponent<Entity>().GetHit((1 + ChargeLevel) / 20, rb.velocity / 2);
                }
                else
                {
                    collision.GetComponent<Entity>().GetHit(ChargeLevel >= 5 ? 1 + ChargeLevel * 3 : 1 + ChargeLevel, rb.velocity / 2,true);
                    GameManager.Instance.PauseTime(0 + ChargeLevel * 1.5f,ChargeLevel/2);
                    if (collision.gameObject.GetComponent<Entity>().health > 0)
                    {
                        ChargeLevel = 0;
                    }

                }
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && collision.gameObject.GetComponent<Entity>())
        {
            if (isspinning)
                if (Target)
                {

                    collision.gameObject.GetComponent<Entity>().GetHit((1 + ChargeLevel) / 20, rb.velocity / 2);
                }
                else
                {
                    collision.gameObject.GetComponent<Entity>().GetHit(ChargeLevel >= 5 ? 1 + ChargeLevel * 3 : 1 + ChargeLevel, rb.velocity / 2, true);
                    GameManager.Instance.PauseTime(0 + ChargeLevel * 1.5f, ChargeLevel / 2);
                    if (collision.gameObject.GetComponent<Entity>().health > 0)
                    {
                        ChargeLevel = 0;
                    }

                }
        }
    }
    public override bool GetHit(float damage)
    {
        return GetHit(damage,Vector2.zero,false);
    }
    public override bool GetHit(float damage, Vector2 knockback)
    {
        return GetHit(damage, knockback, false);
    }
    /// <summary>
    /// Do the get hit behavior, if health less than 0 call the die behavior
    /// </summary>
    public override bool GetHit(float damage, Vector2 knockback, bool MustHit)
    {
        if (base.GetHit(damage, knockback, MustHit))
        {
            GameManager.Instance.PauseTime(damage / 7, damage / 7);
            return true;
        }
        return false;

    }
    public void SoundTrigger(SoundEffect state)
    {
        if (state == SoundEffect.Walking)
        {
            audioSource.volume = 1.5f;
            audioSource.pitch = 1 + Random.Range(-0.10f, 0.10f);
            audioSource.clip = FootStep;
            audioSource.Play();
        }
        if (state == SoundEffect.StartSpin)
        {
            audioSource.volume = 0.1f;
            audioSource.pitch = 1;
            audioSource.clip = StartSpin;
            audioSource.Play();
        }
        if (state == SoundEffect.Spinning)
        {
            audioSource.pitch = 1 + Random.Range(-0.10f, 0.10f);
            audioSource.clip = Spinning;
            audioSource.Play();
        }
        if(state == SoundEffect.Charge)
        {
            audioSource.volume = 0.1f;
            audioSource.pitch = 1 + Random.Range(-0.10f, 0.10f);
            audioSource.clip = Charge;
            audioSource.Play();
        }
        if(state == SoundEffect.Swing)
        {
            audioSource.pitch = 1 + Random.Range(-0.10f, 0.10f);
            audioSource.clip = clip[0];
            audioSource.Play();
        }
    }


}
