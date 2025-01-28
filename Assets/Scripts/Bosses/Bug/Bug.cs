using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Bug : Entity
{
    [Header("Movement")]
    float timer,attackcd;

    [Header("Targeting")]
    [SerializeField] private GameObject target;
    [SerializeField] private Transform Warnning;
    [SerializeField] private float distanceFromTarget;
    [SerializeField] public Vector2 targetPos;
    [SerializeField] private Vector2 direction;

    [Header("Attack HitBoxes")]
    [SerializeField] private GameObject SlamHB;
    [SerializeField] private GameObject AcidHB;
    [SerializeField] private bool acidActive;
    [SerializeField] private bool canCharge;
    [SerializeField] private Bullets bullet;

    [Header("States")]
    [SerializeField] private BugState curState;
    
    public enum BugState
    {
        Idle,
        Walking,
        GroundSlam,
        Charge,
        Jump,
        Split,
    }

    protected override void Awake()
    {
        base.Awake();
        
    }

    void Start()
    {
        target = GameManager.Instance.Player.gameObject;
        distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);
        AcidHB.transform.parent = null;
        ChangeState(BugState.Idle);
        canCharge = false;
    }

    protected override void Update()
    {
        base.Update();

    }

    protected override void FixedUpdate()
    {
        if (health <= 0)
        {
            return;
        }
        base.FixedUpdate();
        FixedUpdateState(curState);
        
    }

    void ChangeState(BugState newState)
    {
        if (curState != newState)
        {
            ExitState(curState);
            curState = newState;
            EnterState(curState);
        }
    }

    void EnterState(BugState state)
    {
        timer = 0;
        attackcd = 0;
        
        if (state == BugState.Idle)
        {
            animator.speed = 0;
            animator.Play("Walking");
        }
        if (state == BugState.GroundSlam)
        {
            animator.Play("Slam");
        }
        if (state == BugState.Walking)
        {
            animator.Play("Walking");
            direction = target.transform.position - transform.position;
            targetPos = target.transform.position;
        }
        if (state == BugState.Charge)
        {
            direction = target.transform.position - transform.position;
            animator.Play("Attacking");

        }
        if (state == BugState.Jump)
        {
            animator.Play("Jump");

        }
        if (state == BugState.Split)
        {
            animator.Play("SplitAcid");

        }
        FaceTo(targetPos);
    }

    void FixedUpdateState(BugState state)
    {
        distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);
        if (state == BugState.Idle)
        {
            timer += Time.fixedDeltaTime;
            if (timer >0.5f)
            {
                ChangeState(BugState.Walking);

            }
            
            
        }
        if(state == BugState.Walking)
        {

            rb.MovePosition((Vector2)transform.position + (targetPos-(Vector2)transform.position).normalized * speed*Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            if (Vector2.Distance(transform.position, targetPos) < 1f)
            {
                targetPos = target.transform.position;
                FaceTo(targetPos);
            }
            if (timer > 1)
            {
                if (distanceFromTarget <= 4f)
                {
                    ChangeState(BugState.GroundSlam);
                    return;
                }
                else if(distanceFromTarget<8)
                {
                    if (Random.Range(0, 2) == 0)
                        ChangeState(BugState.Charge);
                    else
                        ChangeState(BugState.Split);
                    return;
                }
                if (distanceFromTarget >=8f)
                {
                    if (Random.Range(0, 2) == 0)
                        ChangeState(BugState.Charge);
                    else
                        ChangeState(BugState.Jump);
                    return;
                }
            }
            if(timer>3)
            {
                if (Random.Range(0, 2) == 0)
                    ChangeState(BugState.Charge);
                else
                    ChangeState(BugState.Jump);
                return;
            }
        }
        if(state == BugState.GroundSlam)
        {
            timer += Time.fixedDeltaTime;
            if (timer > 1)
            {
                ChangeState(BugState.Idle);
            }
        }
        if (state == BugState.Charge)
        {
            FaceTo(targetPos);
            timer += Time.fixedDeltaTime;
            attackcd += Time.fixedDeltaTime;
            if (attackcd > 0.5f)
            {
                rb.AddForce(direction*120,ForceMode2D.Impulse);
                attackcd = -10;
            }
            if (timer > 0.8f)
            {
                if (distanceFromTarget <= 2f)
                {
                    ChangeState(BugState.GroundSlam);
                }
                else
                {
                    ChangeState(BugState.Idle);
                }
            }
           
        }
        if (state == BugState.Jump)
        {
            targetPos = target.transform.position;
            timer += Time.fixedDeltaTime;
            attackcd += Time.fixedDeltaTime;
            if (attackcd > 0.5f)
            {
                transform.position = targetPos;
                attackcd = -10;
            }
            if (timer > 2.5f)
            {
                if (distanceFromTarget <= 2f)
                {
                    ChangeState(BugState.GroundSlam);
                }
                else
                {
                    ChangeState(BugState.Idle);
                }
            }

        }
        if (state == BugState.Split)
        {

            timer += Time.fixedDeltaTime;
            if (timer > 0.5f)
            {
                ChangeState(BugState.Idle);
            }

        }
    }

    void ExitState(BugState state)
    {
        if (state == BugState.Idle)
            animator.speed = 1;

    }

    void Moving(Vector2 dir)
    {
        rb.MovePosition((Vector2)transform.position + dir * speed * Time.deltaTime);

    }
    public void AnimationTrigger(BugState state)
    {
        if (state == BugState.GroundSlam)
        {
            if (Vector2.Distance(target.transform.position, transform.position) < Warnning.localScale.x * 10)
            {
                target.GetComponent<Entity>().GetHit(damage, (target.transform.position - transform.position).normalized * 10, true);
            }
        }
        if(state == BugState.Split)
        {
            targetPos = target.transform.position;
            Lance b = Instantiate(bullet).GetComponent<Lance>();
            b.transform.position = transform.position;
            b.SetProperty(damage, 0.5f, targetPos, true);
        }  

    }
    private IEnumerator SlamAttack()
    {
        yield return new WaitForSeconds(0.3f);
        SlamHB.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        ChangeState(BugState.Idle);
    }
    private IEnumerator AcidAttack()
    {
        targetPos = target.transform.position;
        AcidHB.gameObject.SetActive(true);
        acidActive = true;
        yield return new WaitForSeconds(1);
        ChangeState(BugState.Idle);
        canCharge = true;
        yield return new WaitForSeconds(8);
        acidActive = false;
        AcidHB.gameObject.SetActive(false);
        AcidHB.transform.position = this.transform.position;
        AcidHB.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }
    private IEnumerator ChargeDuration()
    {
        canCharge = false;
        yield return new WaitForSeconds(3);
        ChangeState(BugState.Idle);
        yield return new WaitForSeconds(8);
        canCharge = true;
    }
}