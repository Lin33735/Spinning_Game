using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UIElements;

public class Bug : Entity
{
    [Header("Movement")]

    [Header("Targeting")]
    [SerializeField] private GameObject target;
    [SerializeField] private float distanceFromTarget;
    [SerializeField] public Vector2 targetPos;
    [SerializeField] private Vector2 direction;

    [Header("Attack HitBoxes")]
    [SerializeField] private GameObject SlamHB;
    [SerializeField] private GameObject AcidHB;
    [SerializeField] private bool acidActive;
    [SerializeField] private bool canCharge;

    [Header("States")]
    [SerializeField] private BugState curState;
    
    public enum BugState
    {
        Idle,
        SpitAcid,
        GroundSlam,
        Charge,
    }

    protected override void Awake()
    {
        base.Awake();
        distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);
        AcidHB.transform.parent = null;
    }

    void Start()
    {
        ChangeState(BugState.Idle);
        canCharge = false;
    }

    protected override void Update()
    {
        base.Update();
        distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        FixedUpdateState(curState);
        direction = target.transform.position - transform.position;
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
        if (state == BugState.Idle)
        {
            speed = 0.2f;
        }
        if (state == BugState.GroundSlam)
        {
            StartCoroutine(SlamAttack());
        }
        if (state == BugState.SpitAcid)
        {
            targetPos = transform.position;
            StartCoroutine(AcidAttack());
        }
        if (state == BugState.Charge)
        {
            speed = 5;
        }
    }

    void FixedUpdateState(BugState state)
    {
        if (state == BugState.Idle)
        {
            Moving(direction);
            if (distanceFromTarget <= 3f)
            {
                ChangeState(BugState.GroundSlam);
            }
            if (distanceFromTarget >= 10f & !acidActive)
            {
                ChangeState(BugState.SpitAcid);
            }
            if (distanceFromTarget >= 10f & acidActive & canCharge)
            {
                ChangeState(BugState.Charge);
            }
        }
        if (state == BugState.Charge)
        {
            Moving(direction);
            if (distanceFromTarget <= 2f)
            {
                ChangeState(BugState.GroundSlam);
            }
        }
        transform.localScale = new Vector2(Mathf.Sign(direction.x) * localScale.x, localScale.y);
    }

    void ExitState(BugState state)
    {
        if (state == BugState.GroundSlam)
        {
            SlamHB.gameObject.SetActive(false);
            SlamHB.transform.position = this.transform.position;
        }
        if (state == BugState.SpitAcid)
        {

        }
    }

    void Moving(Vector2 dir)
    {
        rb.MovePosition((Vector2)transform.position + dir * speed * Time.deltaTime);

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