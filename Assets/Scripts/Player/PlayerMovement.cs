using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : Entity
{

    Vector2 PlayerInput, RawInput,LastPosition;
    Vector2 FaceDirection;
    [Header("Public Componets")]
    public Transform Target;   // 链子目标
    public Transform Effect;
    public int segmentCount = 10;  // 链子段数
    public LineRenderer lineRenderer;
    public bool isrunning, isspinning;
    public float ChargeLevel;
    [Header("Private Componets")]
    [SerializeField]private float CurveAmount;
    [HideInInspector]public bool walkable, attackable, runable;
    float timer, charge,attackcd;
    bool fall;
    GameManager instance;
    public Vector2 SavePoint;
    public Bullets Bullet;
    Vector2 direction;
    Vector2 startvector;
    float InputDirection;
    float Distance;
    float deltatime;
    float spinspeed;

    public enum State
    {
        Idle,
        Walking,
        Attacking,
        Falling,
        SwingSword,
        Enter,
        Dead,
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
        Swing,
        Healing,
    }

    public void Start()
    {
        instance = GameManager.Instance;
        walkable =true;
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
        Application.targetFrameRate = 120;
        maxgethitcd = 2;
        SavePoint = transform.position;
        base.Awake();
    }
    protected override void FixedUpdate()
    {
        
        StateUpdate(currentState);
        if (instance.isCutscene)
        {
            return;
        }
        if (gethitcd > 0)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 60);
            gethitcd -= Time.fixedDeltaTime;
            if (gethitcd <= 0)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 100);
            }
        }

    }
    protected override void Update()
    {
        base.Update();
        if (instance.isCutscene)
        {
            return;
        }
        GetInput();
        
        
    }
    public override void DestroyBehavior()
    {
        if (currentState!=State.Dead)
        {
            ChangeState(State.Dead);
            StartCoroutine(DeadScene());
        }

    }
    IEnumerator DeadScene()
    {
        Target = null;
        ChargeLevel = 0;
        instance.DeathCount++;
        Color color = Color.white;
        float time = 0;
        Time.timeScale = 0.5f;
        GameManager.Instance.isCutscene = true;
        Application.targetFrameRate = -1;
        GameManager.Instance.ScreenShake(2, 2);
        GameManager.Instance.CameraSize.Insert(0, 5);
        GameManager.Instance.LockPosition = transform.position;
        while (time < 2)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        rb.velocity = Vector2.zero;
        time = 0;
        Time.timeScale = 1;
        while (time < 1)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        GameManager.Instance.EnterCutScene();
        while (time < 3)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        ChangeState(State.Idle);
        transform.position = SavePoint;
        health = maxhealth;
        GameManager.Instance.ResetBoss();
        GameManager.Instance.CameraSize.Remove(5);
        time = 0;
        Application.targetFrameRate = 120;
        instance.SendText("DeathCount: "+ instance.DeathCount);
        yield return null;
        while (time < 3 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        instance.SendText("");
        GameManager.Instance.BlackSceneFadeOut();
        GameManager.Instance.isCutscene = false;

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
        if (Input.GetKeyDown(KeyCode.LeftShift)&& (currentState != State.Attacking|| instance.BuffList.Contains(GameManager.Buff.SnakeBoss)))
        {
            isrunning = (isrunning) ? false : true;
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
            timer = 0;
            if (!instance.tutorials.Contains(GameManager.Tutorial.Spin))
            {
                instance.tutorials.Add(GameManager.Tutorial.Spin);
                instance.SendText("Press *WASD* to spin around the attached target.");
            }
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
        if(state == State.SwingSword)
        {
            SoundTrigger(SoundEffect.Charge);
            timer = 0;
            animator.Play("SwingSword");
            walkable = false;
        }
        if (state == State.Enter)
        {
            animator.SetBool("walking", true);
        }
        if(state == State.Dead)
        {
            animator.Play("Dead");
        }
    }
    public void StateUpdate(State state)
    {
        if(state == State.Enter)
        {
            CurrentSpeed += (speed * (instance.BuffList.Contains(GameManager.Buff.SpiderBoss) ? 1.5f : 1) - CurrentSpeed) * 1f;
            Vector2 _speed;
            _speed = Vector2.down * CurrentSpeed;
            if (rb.velocity.magnitude <= _speed.magnitude)
            {
                rb.velocity = _speed;
            }
            timer += Time.fixedDeltaTime;
            if (timer > 2)
            {
                SavePoint= transform.position;
                rb.velocity = Vector2.zero;
                animator.SetBool("walking", false);
                ChangeState(State.Idle);
                GameManager.Instance.isCutscene = false;
            }
        }
        if (state == State.SwingSword)
        {
            timer += Time.fixedDeltaTime;
            if (timer > 1)
            {
                ChangeState(State.Idle);
            }
        }
        if (state == State.Walking)
        {
            FaceTo((Vector2)transform.position + FaceDirection);
            CurrentSpeed += (speed*(instance.BuffList.Contains(GameManager.Buff.SpiderBoss)?1.5f:1) - CurrentSpeed) * 0.1f;
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

            if (instance.BuffList.Contains(GameManager.Buff.FinalBoss)){
                attackcd += Time.fixedDeltaTime;
                if (attackcd > 0.5f)
                {
                    Bullets b = Instantiate(Bullet).GetComponent<Bullets>();
                    b.transform.position = transform.position;
                    b.EnemyTag = "Enemy";
                    Vector2 direction = (GameManager.Instance.MousePosition- (Vector2)transform.position).normalized;
                    b.SetProperty(4, 30, direction, false);
                    attackcd = 0;
                }
            }
            timer += Time.fixedDeltaTime;
            if (Target&&timer>0.5f)
            {
                deltatime = 20f * (60f / Application.targetFrameRate);
                direction = (Target.position - transform.position).normalized;
                InputDirection = Mathf.Sign(Vector2.Dot(FaceDirection, startvector));
                
                if (RawInput == Vector2.zero || (fall && ChargeLevel == 0))
                {
                    //startvector = new Vector2(direction.y, -direction.x);
                    Distance = 2 + (Target.position - transform.position).magnitude;
                    rb.AddForce(direction * spinspeed * rb.mass * deltatime);
                    if (spinspeed >= 30)
                    {
                        rb.AddForce(direction * spinspeed);
                        direction = new Vector2(direction.y, -direction.x);
                        rb.AddForce(direction * spinspeed * -InputDirection);

                    }
                }
                else
                {
                    if (Vector2.Distance(Target.transform.position, transform.position) > Distance/2)
                    {
                        if (spinspeed > speed)
                        {
                            rb.AddForce(direction / (Distance * 0.05f) * (spinspeed * spinspeed) / 30 * rb.mass * deltatime);
                        }
                        else
                        {
                            rb.AddForce(direction / (Distance * 0.05f) * rb.mass * deltatime);
                        }
                    }
                    else
                    {
                        
                    }
                }

                if (RawInput == Vector2.zero || (fall&&ChargeLevel==0))
                    InputDirection = 0;
                direction = new Vector2(direction.y, -direction.x);

                rb.AddForce(direction * spinspeed * InputDirection * rb.mass * deltatime * (isrunning ? 1.5f : 1));
            }
           

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
            if (transform.localScale.y > 0.1f)
            {
                transform.localScale = transform.localScale * 0.9f;
                transform.eulerAngles += new Vector3(0, 0, 1);
            }
            if (timer >= 1)
            {
                fall = false;
                rb.MovePosition(LastPosition + (LastPosition - (Vector2)transform.position).normalized * 0.1f);
                
            }
            if (timer >= 1.1f)
            {
                GetHit(5, Vector2.zero, true);
                ChangeState(State.Idle);
                
            }
        }
        if(state == State.Dead)
        {
            GameManager.Instance.LockPosition = transform.position;
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
        if(state == State.Falling)
        {
            transform.localScale = localScale;
            transform.eulerAngles = new Vector3(0, 0, 0);
            
            walkable = true;
        }
        if (state == State.SwingSword)
        {
            animator.Play("Idle");
            walkable = true;
        }
        if (state == State.Dead)
        {
            animator.Play("Idle");
        }
    }
    public void animationTrigger(State state)
    {
        if (state == State.SwingSword)
        {
            SoundTrigger(SoundEffect.Spinning);
            GameManager.Instance.ScreenShake(0.5f, ChargeLevel / 2f);
            GameObject[] Enemys = GameObject.FindGameObjectsWithTag("Enemy");
            walkable = true;
            foreach (GameObject t in Enemys)
            {
                if (Vector2.Distance(t.transform.position, transform.position) < 3)
                {
                    t.GetComponent<Entity>().GetHit(ChargeLevel >= 5 ? 1 + ChargeLevel * 3 : 1 + ChargeLevel, (t.transform.position- transform.position).normalized*5, true);
                    Bullets b = t.GetComponent<Bullets>();
                    if (b)
                    {
                        if (b.isBullet)
                        {
                            b.Direction = new Vector2(-b.Direction.x, -b.Direction.y);
                            b.EnemyTag = "Enemy";
                            b.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(b.Direction.y, b.Direction.x) * Mathf.Rad2Deg);
                            b.GetComponent<Rigidbody2D>().velocity = b.Direction * b.speed;
                        }
                    }
                    ChargeLevel = 0;
                }
            }
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
        Distance = 1+(Target.position - transform.position).magnitude;
        direction = (Target.position - transform.position).normalized;
        startvector = new Vector2(direction.y, -direction.x);
        InputDirection = Vector2.Dot(FaceDirection, startvector);
        if(RawInput==Vector2.zero)
            InputDirection = 0;
        spinspeed= this.speed;
        charge=2;
        float chargelevel=0;
        animator.SetTrigger("attack");
        // float deltatime = Time.deltaTime/(1f / 720f);
        deltatime = 40f*(60f/Application.targetFrameRate);
        while (Target)
        {
            if (Vector2.Distance(transform.position, Target.transform.position) < Target.GetComponent<Entity>().Size)
            {
                if (Target.tag=="Enemy")
                {

                    Target.GetComponent<Entity>().GetHit(ChargeLevel >= 5 ? 1 + damage * ChargeLevel * 3 : 1 + damage * ChargeLevel, rb.velocity / 2, true);
                    instance.PauseTime(0 + ChargeLevel * 1f, ChargeLevel / 2);
                    if (Target.GetComponent<Entity>().health > 0)
                    {
                        ChargeLevel = 0;

                    }

                    Target = null;
                    break;
                }
                
            }
            if (instance.BuffList.Contains(GameManager.Buff.BugBoss) && chargelevel > 0)
            {
                gethitcd = 0.5f;
            }

            
            
            if (RawInput == Vector2.zero || fall)
            {
                
                FaceTo(Target.transform.position);
            }
            else
            {
                FaceTo((Vector2)transform.position + direction * spinspeed * InputDirection);
            }
            for (int i = 0; i <= segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                Vector3 position = Vector3.Lerp(transform.position, Target.position, t);
                //position.y += Mathf.Sin((1 - counter) * 6 * t * Mathf.PI) * CurveAmount; // 模拟弯曲效果
                lineRenderer.SetPosition(i, position);
            }
            if(RawInput != Vector2.zero)
            charge += Time.deltaTime* (instance.BuffList.Contains(GameManager.Buff.SpiderBoss) ? 1.3f : 1);

            if ((charge >= 5||ChargeLevel> chargelevel) && chargelevel < 6)
            {
                if (!instance.tutorials.Contains(GameManager.Tutorial.Release))
                {
                    instance.tutorials.Add(GameManager.Tutorial.Release);
                    instance.SendText("Once you start spinning, *Release the mouse button* to launch toward the cursor");
                }
                if (spinspeed == this.speed)
                {
                    animator.SetTrigger("end");
                    isspinning=true;
                }
                spinspeed += 5;
                chargelevel += 1;
                SoundTrigger(SoundEffect.Charge);
                if (chargelevel > ChargeLevel)
                {
                    ChargeLevel += 1;
                }
                charge = 4;

                if (spinspeed > 30)
                {
                    Effect.GetComponent<TrailRenderer>().enabled = true;
                    Effect.GetComponent<ParticleSystem>().Play();
                }

            }
            else if (chargelevel >= 6&&charge >=1)
            {
                charge = 0;
                if (instance.BuffList.Contains(GameManager.Buff.BatBoss))
                {
                    instance.PlayCG("ScreenEffectGetHeal");
                    health += 5;
                    if (health > maxhealth)
                    {
                        health = maxhealth;
                    }
                    SoundTrigger(SoundEffect.Healing);
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
            if (RawInput != Vector2.zero&& rb.velocity.magnitude <= 30f)
            {
                float magnitude = rb.velocity.magnitude;
                float currentAngle = Mathf.Atan2(rb.velocity.y, rb.velocity.x);
                float targetAngle = Mathf.Atan2(FaceDirection.y, FaceDirection.x);
                float newAngle = Mathf.LerpAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg, Time.deltaTime * 5) * Mathf.Deg2Rad;
                Vector2 newVelocity = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)) * magnitude;

                rb.velocity = newVelocity;
            }
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
            if (currentState!= State.Attacking)
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
                    if (instance.BuffList.Contains(GameManager.Buff.PlantBoss))
                        collision.GetComponent<Entity>().GetHit((1 + damage* ChargeLevel) / 20, rb.velocity / 2);
                }
                else
                {
                    collision.GetComponent<Entity>().GetHit(ChargeLevel >= 5 ? 1 + damage*ChargeLevel * 3 : 1 + damage * ChargeLevel, rb.velocity / 2,true);
                    if(!collision.GetComponent<Bullets>())
                    instance.PauseTime(0 + ChargeLevel * 1.5f,ChargeLevel/2);
                    if (collision.gameObject.GetComponent<Entity>().health > 0)
                    {
                        ChargeLevel = 0;
                        Target=null;
                    }

                }
        }
        
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Cliff")
        {
            fall = false;


        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy" && collision.gameObject.GetComponent<Entity>())
        {
            if (isspinning)
                if (Target)
                {
                    if(instance.BuffList.Contains(GameManager.Buff.PlantBoss))
                    collision.gameObject.GetComponent<Entity>().GetHit((1 + damage * ChargeLevel) / 20, rb.velocity / 2);
                }
                else
                {
                    collision.gameObject.GetComponent<Entity>().GetHit(ChargeLevel >= 5 ? 1 + damage * ChargeLevel * 3 : 1 + damage * ChargeLevel, rb.velocity / 2, true);
                    if (!collision.gameObject.GetComponent<Bullets>())
                        instance.PauseTime(0 + ChargeLevel * 1.5f, ChargeLevel / 2);
                    if (collision.gameObject.GetComponent<Entity>().health > 0)
                    {
                        ChargeLevel = 0;
                    }
                    Target = null;
                }
        }
    }

    /// <summary>
    /// Do the get hit behavior, if health less than 0 call the die behavior
    /// </summary>
    public override bool GetHit(float damage, Vector2 knockback, bool MustHit)
    {
        if (base.GetHit(damage, knockback, MustHit))
        {
            if (currentState!=State.Dead)
            if (damage>0)
                instance.PlayCG("ScreenEffect");
            else if (damage < 0)
                instance.PlayCG("ScreenEffectGetHeal");
            instance.PauseTime(damage / 7, damage / 7);
            return true;
        }
        return false;

    }
    public void SoundTrigger(SoundEffect state)
    {
        audioSource.volume = 1.5f;
        if (state == SoundEffect.Walking)
        {
            audioSource.volume = 3f;
            audioSource.pitch = 1 + Random.Range(-0.20f, 0.20f);
            audioSource.clip = FootStep;
            audioSource.Play();
        }
        if (state == SoundEffect.StartSpin)
        {
            audioSource.volume = 0.2f;
            audioSource.pitch = 1;
            audioSource.clip = StartSpin;
            audioSource.Play();
        }
        if (state == SoundEffect.Spinning)
        {
            audioSource.pitch = 1 + Random.Range(-0.30f, 0.30f);
            audioSource.clip = Spinning;
            audioSource.Play();
        }
        if(state == SoundEffect.Charge)
        {
            audioSource.volume = 0.3f;
            audioSource.pitch = 1 + Random.Range(-0.30f, 0.30f);
            audioSource.clip = Charge;
            audioSource.Play();
        }
        if(state == SoundEffect.Swing)
        {
            audioSource.pitch = 1 + Random.Range(-0.30f, 0.30f);
            audioSource.clip = clip[0];
            audioSource.Play();
        }
        if(state == SoundEffect.Healing)
        {
            audioSource.volume = 0.4f;
            audioSource.pitch = 1 + Random.Range(-0.20f, 0.20f);
            audioSource.clip = clip[1];
            audioSource.Play();
        }
    }


}
