using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.GraphicsBuffer;

public class Knight : Entity
{
    public float timer;
    public int deadcount;
    [SerializeField] Transform Warnning;
    [SerializeField] Entity Target,Bullets;
    [SerializeField] List<GameObject> Armors;
    [SerializeField] List<GameObject> ExistArmors;
    [SerializeField] GameObject[] Sprites;
    [Header("States")]
    [SerializeField] private State curState;
    List<SpriteRenderer> _spriteRenderer = new List<SpriteRenderer>();
    Vector2 targetposition;
    bool halfhealth;
    public enum State
    {
        Freeze,
        ShowingUp,
        Walking,
        Idle,
        SwingSword,
        LegSmash,
        DropHead,
        DropingStone,
        Regenerating,
    }

    private void Start()
    {
        curState = State.Idle;
        ChangeState(State.Freeze);
        maxgethitcd = 0.2f;
        Target = GameManager.Instance.Player;
        deadcount = 0;
        halfhealth = false;
    }
    protected override void Update()
    {
        base.Update();//

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (health <= 0&&deadcount>=2)
        {
            return;
        }
        FixedUpdateState(curState);
    }
    void ChangeState(State newState)
    {
        if (health <= 0&&deadcount>=2)
        {
            return;
        }
        if (curState != newState)
        {
            ExitState(curState);
            curState = newState;
            EnterState(curState);
        }

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

    void EnterState(State state)
    {
        
        timer = 0;
        if (state == State.Idle)
        {
            animator.speed = 0;

        }
        if (state == State.Freeze)
        {
            animator.speed = 0;
        }
        if (state == State.ShowingUp)
        {
            animator.Play("ShowingUp");
            animator.speed = 1;
        }
        if (state == State.Walking)
        {
            if (ExistArmors.Count > 0)
            {
                ExistArmors[0].GetComponent<KnightHead>().ChangeState(KnightHead.State.Chasing);
            }
            rb.AddForce((Target.transform.position - transform.position).normalized * speed, ForceMode2D.Impulse);
            animator.Play("Walk");
        }
        if (state == State.SwingSword)
        {
            targetposition = Target.transform.position;
            Vector2 vector = targetposition - (Vector2)transform.position;
            FaceTo(targetposition);
            if (transform.localScale.x>0)
                Warnning.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg);
            else
                Warnning.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg+180);

            animator.Play("Attack");
            
        }
        if (state == State.LegSmash)
        {
            animator.Play("Attack2");
        }
        if (state == State.DropHead)
        {
            isTarget = false;
            Sprites[0].SetActive(false);
            animator.Play("DropHead");
            tag = "Untagged";
        }
        if (state == State.Regenerating)
        {
            
            animator.Play("StandUp"); 
        }
        if (halfhealth)
        {
            animator.speed = animator.speed * 1.3f;
        }
    }

    void FixedUpdateState(State state)
    {
        if (!Target)
        {
            return;
        }
        if (state == State.Idle)
        {
            if (Vector2.Distance(Target.transform.position, transform.position) < 30)
            {
                ChangeState(State.Walking);
            }
        }
        if(state == State.Walking)
        {
            targetposition = Target.transform.position;
            FaceTo(targetposition);
            timer += Time.fixedDeltaTime;
            if (timer >= 4)
            {
                ChangeState(State.SwingSword);
            }
                if (Vector2.Distance(targetposition, transform.position) < 4f&& timer>1)
                {
                    ChangeState(State.LegSmash);
                }
                else
                {
                    if (Vector2.Distance(targetposition, transform.position) < 7f)
                    {
                        ChangeState(State.SwingSword);
                    }
                }
        }
        if(state == State.SwingSword)
        {

            timer += Time.fixedDeltaTime;

            if (timer >= 1.5f)
            {
                if (Random.Range(0, 3) == 0)
                {
                    ChangeState(State.LegSmash);
                }
                else
                {
                    ChangeState(State.Idle);
                }
            }
        }
        if (state == State.LegSmash)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= 1f)
            {

                if(Random.Range(0, 4) == 0)
                {
                    ChangeState(State.SwingSword);
                }
                else
                {
                    ChangeState(State.Idle);
                }
            }
        }
        if (state == State.ShowingUp)
        {
            FaceTo(Target.transform.position);
            timer += Time.fixedDeltaTime;

            if (timer >= 1.5f)
            {
                ChangeState(State.LegSmash);
            }
        }
        if (state == State.Freeze)
        {
            
            targetposition = Target.transform.position;
            if (Vector2.Distance(targetposition, transform.position) < 17f)
                ChangeState(State.ShowingUp);

        }
        if (state == State.DropHead)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= 3f)
            {
                ChangeState(State.Regenerating);
            }
        }
        if (state == State.Regenerating)
        {
            timer += Time.fixedDeltaTime;
            health += (maxhealth-health)*0.2f;
            if (timer >= 1f)
            {
                ChangeState(State.Idle);
            }
        }
    }
    public void AnimationTrigger(State state)
    {

        if (state == State.SwingSword)
        {
            if (Vector2.Distance(Warnning.transform.position, Target.transform.position) < Warnning.localScale.x * 11)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 11, true);
            }
        }
        if (state == State.LegSmash)
        {
            if (Vector2.Distance(Warnning.transform.position, Target.transform.position) < Warnning.localScale.x * 11)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 11, false);
            }
        }
        if (state == State.Walking)
        {
            rb.AddForce(((Vector3)targetposition - transform.position).normalized * speed,ForceMode2D.Impulse);


        }
        if (state == State.DropingStone)
        {
            StartCoroutine(DroppingStone(0.3f));
        }
        if(state == State.Idle)//ShakeScreen
        {
            GameManager.Instance.ScreenShake(0.5f, 1f);
        }
    }
    void ExitState(State state)
    {
        if (state == State.Idle)
        {
            animator.speed = 1;

        }
        if (state == State.DropHead)
        {
            
        }
        if (state == State.Regenerating)
        {
            animator.speed = 1;
            tag = "Enemy";
            isTarget = true;
            ExistArmors[0].tag = "Enemy";
        }
        if (state == State.Walking)
        {
            if (ExistArmors.Count > 0)
            {
                ExistArmors[0].GetComponent<KnightHead>().ChangeState(KnightHead.State.Idle);
            }
        }
    }
    public override void DestroyBehavior()
    {
        deadcount++;
        if (deadcount <2)
        {
            ChangeState(State.DropHead);
            maxhealth += 50;
            speed += 80;
            damage += 5;
            halfhealth = true;
            if (deadcount == 1)
            {
                Transform t = Instantiate(Armors[0]).transform;
                t.transform.position= transform.position;
                t.GetComponent<Rigidbody2D>().MovePosition(Sprites[0].transform.position);
                t.GetComponent<KnightHead>().Drop(transform.localScale.x*4,2f, transform.localScale.x*10,transform);
                ExistArmors.Add(t.gameObject);
                t.tag = "Untagged";
            }
        }

        if(deadcount==2)
        base.DestroyBehavior();
        if (deadcount == 3)
        {
            GameManager.Instance.isCutscene = false;
            if (Loot)
            {
                Instantiate(Loot).transform.position = transform.position;
            }
            Destroy(gameObject);
        }
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
    public IEnumerator DroppingStone(float intervals)
    {
        float counter = 0;
        for (int i = 0; i < 15; i++)
        {
            counter = 0;
            Bullets b = Instantiate(Bullets).GetComponent<Bullets>();
            b.SetProperty(damage, 0, Vector2.zero,false);
            if(Target)
            b.transform.position = Target.transform.position+new Vector3(Random.Range(-10,10), Random.Range(-10, 10));
            while (counter <= intervals)
            {
                counter += Time.unscaledDeltaTime;
                yield return null;
            }
            yield return null;
        }
        yield return null;
    }
}
