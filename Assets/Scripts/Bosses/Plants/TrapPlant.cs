using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TrapPlant : Entity
{
    // Start is called before the first frame update
    [SerializeField] float timer;
    [SerializeField] Transform Warnning;
    [SerializeField] Entity Target;
    public Plants Owner;
    public Bullets Bullet;
    // Update is called once per frame
    [Header("States")]
    [SerializeField] private State curState;

    public enum State
    {
        Idle,
        Prowling,
        Attacking,

    }
    protected override void Awake()
    {
        base.Awake();
        maxgethitcd = 0.2f;
        isTarget = false;
    }
    private void Start()
    {
        Target = GameManager.Instance.Player;

        ChangeState(State.Idle);
    }


    protected override void Update()
    {
        base.Update();//

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
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
        if (state == State.Idle)
        {
            animator.Play("Idle");
            isTarget = false;
            timer = 0;
        }
        if (state == State.Attacking)
        {
            animator.Play("TrapTrigged");
            timer = 0;
            isTarget = true;

            Target.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            Target.CurrentSpeed = 0;
        }
    }

    void FixedUpdateState(State state)
    {
        if (state == State.Idle)
        {
            if (Vector2.Distance(Target.transform.position, transform.position) < Warnning.localScale.x * 9)
            {
                ChangeState(State.Attacking);
            }
            timer += Time.fixedDeltaTime;
            if (timer >= 5)
            {
                Bullets b = Instantiate(Bullet);
                b.transform.position = transform.position;
                b.SetProperty(2, 5, (Target.transform.position - transform.position).normalized);
                timer = 0;
            }
        }
        if (state == State.Attacking)
        {

            timer += Time.fixedDeltaTime;
            if (timer >= 20)
            {

                ChangeState(State.Idle);
            }
        }
    }
    public void AnimationTrigger()
    {
        if (curState == State.Attacking&&gethitcd==0)
        {
            if (Vector2.Distance(Target.transform.position, transform.position) < Warnning.localScale.x * 12)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 2, false);
            }
        }

    }
    void ExitState(State state)
    {

    }
    public override void DestroyBehavior()
    {
        if (Owner)
        {
            Owner.Traps.Remove(transform);
        }
        base.DestroyBehavior();
    }

}
