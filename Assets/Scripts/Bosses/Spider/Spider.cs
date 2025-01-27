using System.Collections;
using System.Collections.Generic;

using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Spider : Entity
{
    [SerializeField] float timer;
    [SerializeField] Transform Warnning;
    [SerializeField] Entity Target;
    public Plants Owner;
    public Bullets Bullet;
    // Update is called once per frame
    [Header("States")]
    [SerializeField] private State curState,lastState;
    float AttackCount,AnimLength;
    bool HalfHealth, isattacking;
    Vector2 position,startposition;
    List<SpriteRenderer> _spriteRenderer = new List<SpriteRenderer>();
    public enum State
    {
        OnCell,
        OnCellAttack,
        OnCellChargeAttack,
        Idle,
        Prowling,
        Attacking,
        Walking,
        Falling,
        WalkingToMid,
        OnWall,
        OnWallAttack,
        Shooting,
        OnWallAttack2,
        Dying,
    }
    protected override void Awake()
    {
        base.Awake();

        maxgethitcd = 0.2f;
        isTarget = false;
        startposition = transform.position;
    }
    private void Start()
    {
        Target = GameManager.Instance.Player;
        ChangeState(State.OnCell);
        animator.Play("Celling");

        void GetAllSpriteRenderers(Transform parent)
        {
            foreach (Transform child in parent)
            {
                // 检查是否有 SpriteRenderer 组件
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    _spriteRenderer.Add(sr);
                }

                // 递归调用，遍历子级的子级
                GetAllSpriteRenderers(child);
            }
        }
        GetAllSpriteRenderers(transform);

    }


    protected override void Update()
    {
        base.Update();//

    }
    protected override void FixedUpdate()
    {

        base.FixedUpdate();
        if (health <= 0)
        {
            return;
        }
        FixedUpdateState(curState);
    }
    void ChangeState(State newState)
    {
        if (curState != newState)
        {
            ExitState(curState);
            lastState = curState;
            curState = newState;
            EnterState(curState);

        }
    }

    void EnterState(State state)
    {

        if (state == State.WalkingToMid)
        {
            animator.CrossFade("Walk", 0.25f);
        }
        if (state == State.Idle)
        {
            animator.Play("Idel");
        }
        if (state == State.OnWallAttack)
        {
            animator.Play("LayAttack");
        }
        if (state == State.OnWallAttack2)
        {
            animator.Play("LayAttack2");
        }
        if (state == State.OnCell)
        {
            animator.Play("Celling");
        }
        if (state == State.OnWall)
        {
            if(lastState==State.WalkingToMid)
            animator.CrossFade("Lay", 0.5f);
            else
                animator.Play("Lay");
        }
        if (state == State.OnCellChargeAttack)
        {

            animator.Play("CellingAttack2");
        }
        if (state == State.OnCellAttack)
        {
            animator.Play("CellingAttack");
            transform.position = Target.transform.position;

        }
        if(state == State.Walking)
        {
            position = Target.transform.position- transform.position;
            animator.CrossFade("Walk",0.25f);
        }
        if(state == State.Falling)
        {
            animator.Play("Fall");
            GetComponent<CircleCollider2D>().radius += 3;
            Size += 3;
        }
        if (state == State.Shooting)
        {
            AttackCount = 0;
        }
        timer = 0;

        AnimLength = 10;
    }

    void FixedUpdateState(State state)
    {


        if (state == State.WalkingToMid)
        {
            rb.MovePosition(transform.position + ((Vector3)startposition - transform.position).normalized * speed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            if (timer > 5 || Vector2.Distance((Vector3)startposition, transform.position) < 1)
            {
                transform.position = startposition;
                ChangeState(State.OnWall);
            }
            
        }
        if (state == State.OnWall)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= 1)
            {
                int i = Random.Range(0, 3);
                if (i == 0)
                {
                    ChangeState(State.OnWallAttack);
                }
                else if(i==1)
                {
                    ChangeState(State.Shooting);
                }else
                {
                    ChangeState(State.OnWallAttack2);
                }
            }

        }
        if(state== State.Shooting)
        {
            timer += Time.fixedDeltaTime;
            if (timer > 0.3f)
            {
                AttackCount++;
                timer = 0;
                Vector2 vector2 = (Target.transform.position + (Vector3)Target.GetComponent<Rigidbody2D>().velocity * 0.2f - transform.position);
                Bullets b = Instantiate(Bullet);
                b.transform.position = transform.position;
                b.SetProperty(5, 15, vector2.normalized, true);
                b.transform.eulerAngles = new Vector3(1f, 1f, Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg);
                if (AttackCount >= 3)
                {
                    ChangeState(lastState);
                }
            }
        }
        if (state == State.Idle)
        {
            if (health <= maxhealth *0.30f && !HalfHealth)
            {
                HalfHealth = true;
                damage += 5;
                ChangeState(State.WalkingToMid);
                return;
            }
            if (lastState == State.Falling)
            {
                ChangeState(State.Walking);
                return;
            }
            if (Vector2.Distance(Target.transform.position, transform.position) >16)
            {
                ChangeState(State.Walking);
            }
            else if (Vector2.Distance(Target.transform.position, transform.position) < 1)
            {
                ChangeState(State.Shooting);
            }
            timer += Time.fixedDeltaTime;
            if (timer > 1)
            {
                int i = Random.Range(0, 2);
                if (i==0)
                {
                    ChangeState(State.Shooting);
                }
                else if (i==1)
                {
                    ChangeState(State.Walking);
                }

            }
        }
        if (state == State.Walking)
        {
            rb.MovePosition(transform.position + (Vector3)position.normalized * speed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            if (timer > 3)
            {
                ChangeState(State.Idle);

            }
        }
        if (state == State.Attacking)
        {

        }
        if (state == State.OnCell)
        {
            if (health <= maxhealth *0.9f)
            {
                damage += 5;
                ChangeState(State.Falling);
            }
            timer += Time.fixedDeltaTime;

            if (timer > 1)
            {
                if (AttackCount < 2)
                {
                    AttackCount++;
                    ChangeState(State.OnCellAttack);
                }
                else
                {

                    AttackCount = 0;
                    ChangeState(State.OnCellChargeAttack);
                }
            }
        }
        if(state == State.OnCellAttack)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= AnimLength)
            {
                ChangeState(State.OnCell);
            }
            if (timer <= 0.3f)
            {
                transform.position += (Target.transform.position - transform.position) * 0.2f;
            }

        }
        if (state == State.OnWallAttack)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= 1.25f)
            {
                ChangeState(lastState);
            }
        }
        if (state == State.OnWallAttack2)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= 1.80f)
            {
                ChangeState(lastState);
            }
        }

        if (state == State.OnCellChargeAttack)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= AnimLength)
            {
                ChangeState(State.OnCell);
            }

            if (timer <= 1)
            {
                transform.position += (Target.transform.position - transform.position) * 0.2f;
                isTarget = false;
            }
            else
            {
                isTarget = true;
            }
        }
        if (state == State.Falling)
        {
            
            timer += Time.fixedDeltaTime;
            if (timer >= 1f)
            {
                ChangeState(State.Idle);
            }
        }
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimLength = stateInfo.length;
    }

    void ExitState(State state)
    {
        if (state == State.OnCellChargeAttack)
        {
            isTarget = false;
        }
        isattacking=false;
    }
    public override IEnumerator GetHitEffect(float duration)
    {
        float counter = 0;

        foreach (SpriteRenderer s in _spriteRenderer)
        {
            s.material.SetFloat("_HurtDuration", 1);
        }

        while (counter <= duration)
        {
            counter += Time.deltaTime;

            foreach (SpriteRenderer s in _spriteRenderer)
            {
                s.material.SetFloat("_HurtDuration", 1 * (1 - counter / duration));
            }
            yield return null;
        }

        foreach (SpriteRenderer s in _spriteRenderer)
        {
            s.material.SetFloat("_HurtDuration", 0);
        }


    }
    public void AnimationTrigger(State state)
    {

        if (state == State.Attacking)
        {
            if (Vector2.Distance(Warnning.transform.position, Target.transform.position) < Warnning.localScale.x * 10)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, false);
            }
        }
        if (state == State.OnWallAttack)
        {
            if (Vector2.Distance(Warnning.transform.position, Target.transform.position) < Warnning.localScale.x * 10)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, true);
            }
        }
        if (state == State.OnWall)
        {
            isattacking=true;
        }
        if (state == State.Walking)
        {
            GameManager.Instance.ScreenShake(0.5f, 1f);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        
        if (isattacking)
        {
            if (collision.tag=="Player")
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, true);
                isattacking = false;
            }
        }
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Wall")
        {
            if (curState == State.Walking)
            {
                ChangeState(lastState);
            }
        }
    }

}
