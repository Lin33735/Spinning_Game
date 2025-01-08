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
        base.Update();
        distanceFromTarget = Vector2.Distance(transform.position, target.transform.position);
    }

    protected override void FixedUpdate()
    {
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
        if (state == BugState.Idle)
        {

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
    }

    void FixedUpdateState(BugState state)
    {
        if (state == BugState.Idle)
        {
            if (distanceFromTarget <= 3f)
            {
                ChangeState(BugState.GroundSlam);
            }
            if (distanceFromTarget >= 10f & !acidActive)
            {
                ChangeState(BugState.SpitAcid);
            }
        }
    }

    void ExitState(BugState state)
    {
        if (state == BugState.GroundSlam)
        {
            SlamHB.gameObject.SetActive(false);
        }
        if (state == BugState.SpitAcid)
        {

        }
    } 

    private IEnumerator SlamAttack()
    {
        yield return new WaitForSeconds(1);
        SlamHB.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        ChangeState(BugState.Idle);
    }
    private IEnumerator AcidAttack()
    {
        targetPos = target.transform.position;
        AcidHB.gameObject.SetActive(true);
        acidActive = true;
        yield return new WaitForSeconds(1);
        ChangeState(BugState.Idle);
        yield return new WaitForSeconds(8);
        acidActive = false;
        AcidHB.gameObject.SetActive(false);
        AcidHB.transform.position = this.transform.position;
        AcidHB.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    }
}