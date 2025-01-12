using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static UnityEngine.GraphicsBuffer;

public class SnakeAi : Entity
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] heads;
    [SerializeField] private Sprite[] bodies;
    [SerializeField] private Sprite[] Tails;
    [Header("Movement")]
    [SerializeField] private int BodyNumber;
    [SerializeField] private Transform[] Bodies;
    [SerializeField] public LineRenderer lineRenderer;
    [Header("Targeting")]
    [SerializeField] public Transform Target;
    [SerializeField] private Vector2 direction;
    private float radians,angle;
    private float distanceFromTarget;
    private float count;

    [Header("Attack HitBoxes")]
    

    [Header("States")]
    [SerializeField] private State curState;
    [SerializeField] private SubState subState;
    public enum State
    {
        Idle,
        Prowling,
        Attacking,

    }
    public enum SubState
    {
        Moving,
        Turning,

    }

    public override void Awake()
    {
        base.Awake();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = BodyNumber+1;
        Bodies = new Transform[BodyNumber];
        for (int i = 0; i < BodyNumber; i++)
        {
            GameObject newObject = new GameObject();
            Bodies b = newObject.AddComponent<Bodies>();
            Collider2D C = newObject.AddComponent<BoxCollider2D>();
            C.isTrigger = true;
            C.tag = tag;
            newObject.transform.position = transform.position;
            if (i == 0)
            {
                b.Parent = transform;
            }
            else
            {
                b.Parent = Bodies[i-1];
            }
            Bodies[i] = newObject.transform;
        }
    }

    void Start()
    {
        ChangeState(State.Prowling);
    }

    protected override void Update()
    {
        base.Update();
        
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        FixedUpdateState(curState);
        lineRenderer.SetPosition(0, transform.position);
        for (int i = 0; i < BodyNumber; i++)
        {
            lineRenderer.SetPosition(i+1, Bodies[i].position);
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

        }
        if (state == State.Prowling)
        {
            if (!Target)
            {
                ChangeState(State.Idle);
                return;
            }
            subState = SubState.Moving;
            Vector3 direction = Target.position - transform.position;
            radians = Mathf.Atan2(direction.y, direction.x);
            radians += Mathf.Deg2Rad * Random.Range(-45, 45);
        }
        if (state == State.Attacking)
        {
            count = 0;
        }
    }

    void FixedUpdateState(State state)
    {
        FixedUpdateSubState(state, subState);
        if (state == State.Idle)
        {
            Target = GameObject.FindGameObjectWithTag("Player").transform;
            if (Target != null)
            {
                if(Random.Range(0,1)==0)
                    ChangeState(State.Prowling);
                else
                    ChangeState(State.Attacking);
                return;
            }
            
        }
        if (state == State.Attacking)
        {
            if (!Target)
            {
                ChangeState(State.Idle);
                return;
            }
            Vector3 direc = Target.position - transform.position;
            radians = Mathf.Atan2(direc.y, direc.x);
            
            float currentAngle = angle;

            float newAngle = Mathf.LerpAngle(currentAngle, radians * Mathf.Rad2Deg, 4 * Time.fixedDeltaTime);
            direction = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
            angle = newAngle;
            count += Time.fixedDeltaTime;
            if (count >= 10)
            {
                ChangeState(State.Idle);
            }
        }

        if (state == State.Prowling)
        {
            if(Target == null)
            {
                ChangeState(State.Idle);
                return;
            }
            count += Time.fixedDeltaTime;
            if (count >= 3)
            {
                count = 0;
                if (Random.Range(0, 3) == 0)
                {
                    ChangeState(State.Attacking);
                }
                else
                {
                    ChangeState(State.Idle);
                }
                
                return;
            }
            
            
            float currentAngle = angle;

            float newAngle = Mathf.LerpAngle(currentAngle, radians * Mathf.Rad2Deg, 10 * Time.fixedDeltaTime);
            direction = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
            // 设置角色的旋转
            angle = newAngle;
            //transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        if (Vector2.Distance(transform.position, Target.transform.position) <= 1.5f)
        {
            Target.GetComponent<Entity>().GetHit(1, (Target.position - transform.position).normalized * 10);
        }
        if (direction.x <= 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (GettingAngle(direction) < 22.5)
        {
            spriteRenderer.sprite = heads[0];
        }
        else if (GettingAngle(direction) < 67.5)
        {
            if (direction.y <= 0)
                spriteRenderer.sprite = heads[1];
            else
                spriteRenderer.sprite = heads[2];
        }
        else
        {
            if (direction.y <= 0)
                spriteRenderer.sprite = heads[3];
            else
                spriteRenderer.sprite = heads[4];
        }
    }
    void FixedUpdateSubState(State state,SubState substate)
    {
        if (substate == SubState.Moving)
        {
            rb.MovePosition((Vector2)transform.position + direction * speed * Time.fixedDeltaTime);
        }
        if(substate == SubState.Turning)
        {
            
        }
    }
    void ExitState(State state)
    {

    }
    float GettingAngle(Vector2 direction)
    {
        direction = new Vector2(Mathf.Abs(direction.x), direction.y);
        Vector2 A = new Vector2(1, 0);

        float dotProduct = Vector2.Dot(A, direction);

        float magnitudeA = A.magnitude;
        float magnitudeB = direction.magnitude;

        float cosTheta = dotProduct / (magnitudeA * magnitudeB);
        float angleRadians = Mathf.Acos(cosTheta);

        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        return angleDegrees;
    }
    public override void GetHit(float damage)
    {
        GetHit(damage, Vector2.zero);
    }
    public override void GetHit(float damage, Vector2 knockback)
    {

        base.GetHit(damage, knockback);
        StartCoroutine(GetHitEffect2(1f));
    }
    public IEnumerator GetHitEffect2(float duration)
    {
        float counter = 0;
        lineRenderer.material.SetFloat("_HurtDuration", 1);
        while (counter <= duration)
        {
            counter += Time.fixedDeltaTime;
            lineRenderer.material.SetFloat("_HurtDuration", 1 * (1 - counter / duration));
            yield return null;
        }
        lineRenderer.material.SetFloat("_HurtDuration", 0);

    }
}
