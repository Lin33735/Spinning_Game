using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class Bug : Entity
{
    [Header("Movement")]

    [Header("Targeting")]
    [SerializeField] private GameObject target;
    [SerializeField] private float distanceFromTarget;
    [SerializeField] private Vector2 direction;

    [Header("Attack HitBoxes")]
    [SerializeField] private GameObject GroundSlamHB;

    [Header("States")]
    [SerializeField] private BugState curState;

    public enum BugState
    {
        Idle,
        SpitAcid,
        GroundSlam,
        Charge,
    }

    public override void Awake()
    {
        base.Awake();
        distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);
    }

    void Start()
    {  
        ChangeState(BugState.Idle);
    }

    protected override void Update()
    {
        distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);
    }

    protected override void FixedUpdate()
    {
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
        if (state == BugState.Idle)
        {

        }
        if (state == BugState.GroundSlam)
        {
            StartCoroutine(JumpAttack());
        }
    }

    void FixedUpdateState(BugState state)
    {
        if (state == BugState.Idle)
        {
            if (distanceFromTarget <= 3f)
            {
                ChangeState(BugState.GroundSlam);
            }
        }
    }

    void ExitState(BugState state)
    {
        if (state == BugState.GroundSlam)
        {
            GroundSlamHB.gameObject.SetActive(false);
        }
    } 

    private IEnumerator JumpAttack()
    {
        yield return new WaitForSeconds(1);
        GroundSlamHB.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        ChangeState(BugState.Idle);
    }
}