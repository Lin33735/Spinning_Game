using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NormalChase : Bullets
{
    [SerializeField] Entity Target;
    [SerializeField] float AttackTimer;
    public Entity Owner;
    Vector2 TargetPosition;

    // Update is called once per frame
    [Header("States")]
    [SerializeField] private State curState;

    public enum State
    {
        Idle,
        Flying,
    }
    protected override void Awake()
    {
        base.Awake();
        musthit = false;
    }
    private void Start()
    {
        ChangeState(State.Idle);
        maxgethitcd = 0.2f;
        Target = GameManager.Instance.Player;
    }

    protected override void Update()
    {
        base.Update();//

    }
    protected override void FixedUpdate()
    {

        FixedUpdateState(curState);
    }
    void ChangeState(State newState)
    {
        if (curState != newState)
        {
            ExitState(curState);
            curState = newState;
            EnterState(curState);
        }
    }

    void EnterState(State state)
    {
        timer = 0f;
        if (state == State.Flying)
        {
            rb.velocity = rb.velocity/2;
            rb.AddForce((Target.transform.position - transform.position).normalized * speed, ForceMode2D.Impulse);

        }
        if (state == State.Idle)
        {
            rb.velocity = rb.velocity/2;


        }
    }

    void FixedUpdateState(State state)
    {
        if (state == State.Idle)
        {
            if (!Owner)
            {
                DestroyBehavior();
                return;
            }
            rb.AddForce((Owner.transform.position - transform.position).normalized * speed);
            timer += Time.fixedDeltaTime;
            if (timer > AttackTimer)
            {
                ChangeState(State.Flying);
            }
        }
        if (state == State.Flying)
        {
            rb.AddForce((Target.transform.position - transform.position).normalized * speed);
            timer += Time.fixedDeltaTime;
            if (timer > AttackTimer)
            {
                ChangeState(State.Idle);
            }
        }
    }
    void ExitState(State state)
    {

    }
}
