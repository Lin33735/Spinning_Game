using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plants : Entity
{
    // Start is called before the first frame update
    [Header("PrivateProperty")] 
    float timer,cd;
    [SerializeField] Entity Target;
    [SerializeField] Transform Warnning;
    [SerializeField] GameObject Trap;
    [SerializeField] Bullets Bullet;
    public List<Transform> Traps;
    [Header("States")]
    [SerializeField] private State curState;
    
    public enum State
    {
        Idle,
        SummonTrap,
        Attacking,
        Shooting,
    }
    protected override void Awake()
    {
        base.Awake();

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
        if (health <= maxhealth / 2&&isTarget==true)
        {
            isTarget = false;
            damage += 20;
        }
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
            timer = 0;
        }
        if (state == State.Attacking)
        {
            animator.Play("Attacking");
            timer = 0;
        }
        if (state == State.SummonTrap)
        {
            Transform t = Instantiate(Trap).transform;
            Traps.Add(t);
            t.GetComponent<Rigidbody2D>().MovePosition((Vector2)transform.position + new Vector2(Random.Range(-15, 15), Random.Range(-15, 15)));
            t.GetComponent<TrapPlant>().Owner = this;

            animator.Play("SummoningTrap");
        }
        if (state == State.Shooting)
        {
            timer = 0;
        }
    }

    void FixedUpdateState(State state)
    {
        if (state == State.Idle)
        {
            timer += Time.fixedDeltaTime;
            if (Traps.Count == 0)
            {
                ChangeState(State.SummonTrap);
            }
            if (timer >= 3)
            {
                if(Random.Range(0,2)==0)
                ChangeState(State.Attacking);
                else ChangeState(State.Shooting);
                if (Traps.Count < 5&&Random.Range(0,2)==0)
                {
                    ChangeState(State.SummonTrap);
                }
            }
        }
        if (state == State.Attacking)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= 3.5)
            {
                ChangeState(State.Idle);
            }
        }
        if (state == State.SummonTrap)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= 1)
            {
                
                ChangeState(State.Idle);
            }
        }
        if (state == State.Shooting)
        {
            timer += Time.fixedDeltaTime;
            cd+= Time.fixedDeltaTime;
            if (cd >= 0.3f)
            {
                Bullets b = Instantiate(Bullet);
                b.transform.position = transform.position;
                b.SetProperty(5, 5, (Target.transform.position - transform.position).normalized);
                cd = 0;
            }
            if (timer >= 3)
            {

                ChangeState(State.Idle);
            }
        }
    }
    public void AnimationTrigger()
    {
        if (curState == State.Attacking)
        {
            if (Vector2.Distance(Target.transform.position, transform.position) < Warnning.localScale.x*10)
            {
                Target.GetHit(damage,(Target.transform.position-transform.position).normalized*10,true);
            }
        }

    }
    void ExitState(State state)
    {

    }
}
