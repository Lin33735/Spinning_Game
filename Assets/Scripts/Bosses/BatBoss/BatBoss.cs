using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions.Must;

using static UnityEngine.GraphicsBuffer;


public class BatBoss : Entity
{
    // Start is called before the first frame update

    [SerializeField] Transform Warnning;
    [SerializeField] Entity Target;
    [SerializeField] Transform Bat,Crystal;
    public Plants Owner;
    public Bullets Bullet;
    List<Transform> Bats = new List<Transform>();
    List<Transform> Crystals = new List<Transform>();
    Vector2 TargetPosition, StartPosition;
    float timer, AttackTimer;
    bool halfhealth;
    // Update is called once per frame
    [Header("States")]
    [SerializeField] private State curState;

    public enum State
    {
        Idle,
        ShowingUp,
        Flying,
        Attacking,
        MeleeAttack,
        ChargingAttack,
        MovingToMid,
    }
    protected override void Awake()
    {
        base.Awake();
        StartPosition = transform.position;
    }
    private void Start()
    {
        ChangeState(State.Idle);
        maxgethitcd = 0.2f;
        isTarget = true;
    }
    public void ShowUp()
    {
        animator.SetTrigger("ShowUp");

        ChangeState(State.ShowingUp);
        Target = GameManager.Instance.Player;
        animator.Play("ShowUp");
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
        if (state == State.Flying)
        {
            if (health <= maxhealth / 2&& !halfhealth)
            {
                halfhealth = true;
                animator.Play("Idle");
                ChangeState(State.MovingToMid);
                damage += 3;
                speed += 10;
                return;
            }
            animator.Play("Idle");
            timer = 0;
            AttackTimer = 0;
        }
        if (state == State.ChargingAttack)
        {
            rb.velocity = Vector2.zero;
            rb.mass += 100;
            timer = 0;
            AttackTimer = 0;
            animator.Play("Charging");
            isTarget = true;
            for (int i = 0;i < 4; i++)
            {
                Transform C = Instantiate(Crystal);
                C.position = (Vector2)transform.position + (new Vector2(Mathf.Cos(Mathf.Deg2Rad*i * 90), Mathf.Sin(Mathf.Deg2Rad*i * 90))*14);
                Crystals.Add(C);
                print(i * 45);
            }

        }
        if(state == State.MovingToMid)
        {
            for (int i = 0; i < Bats.Count; i++)
            {
                Bats[i].GetComponent<Entity>().GetHit(60,Vector2.zero,true);
            }
        }
        if (state == State.Attacking)
        {
            animator.Play("Attack");
            timer = 0;
            AttackTimer = 0;
            rb.velocity = Vector2.zero;
        }
        if (state == State.MeleeAttack)
        {
            animator.Play("Attack2");
            timer = 0;
            AttackTimer = 0;
            rb.velocity = Vector2.zero;
        }
    }

    void FixedUpdateState(State state)
    {
        if (!GameManager.Instance.Player)
        {
            return;
        }
        if (state == State.Idle)
        {
            if(Vector2.Distance(GameManager.Instance.Player.transform.position, transform.position) < 10)
            {
                ShowUp();
            }
            
        }
        if (state == State.Flying)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= 3)
            {
                TargetPosition = Target.transform.position;
            }
            if (Vector2.Distance(Target.transform.position, transform.position) > 4)
            {
                rb.AddForce((TargetPosition-(Vector2)transform.position).normalized*speed);
            }
            else
            {
                AttackTimer += Time.fixedDeltaTime;
                if (AttackTimer >= 0.5f)
                {
                    ChangeState(State.MeleeAttack);
                }
            }
            if(Vector2.Distance(Target.transform.position, transform.position) > 6)
            {
                AttackTimer += Time.fixedDeltaTime;
                if (AttackTimer >= 2)
                {
                    ChangeState(State.Attacking);
                }
            }
            if (Bats.Count > 0)
            {

                for (int i = 0; i < Bats.Count; i++)
                {
                    if (Bats[i] == null)
                    {
                        Bats.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (Bats.Count <= ((health <= maxhealth / 2) ? 2 : 0))
            {
                Transform b = Instantiate(Bat);
                b.position = transform.position;
                b.GetComponent<NormalChase>().Owner = this;
                Bats.Add(b);
            }
            
        }
        if (state == State.Attacking)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= 1f)
            {

                ChangeState(State.Flying);
            }
        }
        if (state == State.MeleeAttack)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= 1f)
            {

                ChangeState(State.Flying);
            }
        }
        if (state == State.ShowingUp)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= 3.7f)
            {
                isTarget = false;
                ChangeState(State.Flying);
            }
        }
        if (state == State.ChargingAttack)
        {
            for (int i = 0; i < Crystals.Count; i++)
            {
                if (Crystals[i] == null)
                {
                    Crystals.RemoveAt(i);
                    i--;
                }
            }
            if (health < maxhealth/2)
            {
                health += Time.fixedDeltaTime*2;
            }
            if (Crystals.Count <= 0)
            {
                ChangeState(State.Flying);
                return;
            }
            timer += Time.fixedDeltaTime;
            if (timer >= 30)
            {
                animator.Play("ChargeAttack");
                if (timer > 30+2.25f* Crystals.Count/4)
                {
                    animator.Play("Flying");
                    ChangeState(State.Flying);
                }
            }
        }
        if (state == State.MovingToMid)
        {
            rb.MovePosition((Vector2)transform.position+(StartPosition - (Vector2)transform.position).normalized*Time.fixedDeltaTime*10);
            if(Vector2.Distance(StartPosition, transform.position) < 1)
            {
                ChangeState(State.ChargingAttack);
            }
        }
    }
    void ExitState(State state)
    {
        if (state == State.ChargingAttack)
        {
            isTarget = false;
            foreach (Transform b in Crystals)
            {
                b.GetComponent<Entity>().GetHit(300, Vector2.zero, true);
            }
            rb.mass -= 100;
        }

    }
    public void AnimationTrigger(State state)
    {
        if (state == State.Attacking)
        {
            StartCoroutine(shootWave());
        }
        if (state == State.MeleeAttack)
        {
            if (Vector2.Distance(Target.transform.position, transform.position) < Warnning.localScale.x * 10)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, true);
            }
        }
    }
    
    public IEnumerator shootWave()
    {
        float timer=0;
        TargetPosition = Target.transform.position;
        Vector2 vector2 = (Target.transform.position + (Vector3)Target.GetComponent<Rigidbody2D>().velocity * 0.5f - transform.position);
        Bullets b = Instantiate(Bullet);
        b.transform.position = transform.position;
        b.SetProperty(10, 15, vector2.normalized, true);
        b.transform.eulerAngles = new Vector3(1f, 1f, Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg);
        for (int i = 0; i < 4; i++)
        {
            
            timer = 0;
            while (timer < 0.1f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            b = Instantiate(Bullet);
            b.transform.position = transform.position;
            b.SetProperty(10, 15, vector2.normalized, false);
            b.transform.eulerAngles = new Vector3(1f, 1f, Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg);
        }
        
        yield return null;
    }
   
}
