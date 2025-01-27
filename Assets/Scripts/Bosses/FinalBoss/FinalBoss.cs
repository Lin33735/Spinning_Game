using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FinalBoss : Entity
{
    [SerializeField] Entity Target;
    [SerializeField] float AttackTimer;
    public Entity[] Bullets;
    public Transform Warnning;
    float timer,attackcd;
    Vector2 TargetPosition,StartPosition;
    List<SpriteRenderer> _spriteRenderer = new List<SpriteRenderer>();
    // Update is called once per frame
    [Header("States")]
    [SerializeField] private State curState,lastState;
    private Transform Lance;
    bool halfhealth,hasLance;
    int deadcount;
    public enum State
    {
        Idle,
        Walking,
        ShowingUp,
        ThrowingLance,
        Spinning,
        MeleeAttack,
        ThrowingDart,
        SpinningDart,
        Hide,
        Freeze,
        Tornado,
        HoldLance,
        SwingLance,
        SwingLance2,
        MoveToLance,
        Defeated,
        Defeated2,
    }
    protected override void Awake()
    {
        base.Awake();
        StartPosition = transform.position;
        if(GameManager.Instance.Boss == this)
        ChangeState(State.ShowingUp);

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
    void OnEnable()
    {
        if(GameManager.Instance.Boss==this)
        ChangeState(State.ShowingUp);
    }
    private void OnDisable()
    {
        ChangeState(State.Freeze);

    }
    private void Start()
    {
        maxgethitcd = 0.2f;
        Target = GameManager.Instance.Player;
    }

    protected override void Update()
    {
        base.Update();//

    }
    protected override void FixedUpdate()
    {
        base .FixedUpdate();
        FixedUpdateState(curState);
    }
    void ChangeState(State newState)
    {
        if (curState != newState)
        {
            ExitState(curState);
            lastState= curState;
            curState = newState;
            EnterState(curState);
        }
    }

    void EnterState(State state)
    {
        attackcd = 0;
        if (Target)
        {
            TargetPosition = Target.transform.position;
            FaceTo(TargetPosition);
        }

        timer = 0;
        if (state == State.Freeze)
        {
            animator.Play("Null");
        }
        if (state == State.ShowingUp)
        {
            animator.Play("ShowUp");
        }
        if (state == State.MeleeAttack)
        {
            animator.Play("Melee");
        }
        if (state == State.ThrowingLance)
        {
            animator.Play("AttackOne");
        }
        if (state == State.ThrowingDart)
        {
            animator.Play("Melee");
        }
        if (state == State.SpinningDart)
        {
            animator.Play("Spin");
        }
        if (state == State.Spinning)
        {
            animator.Play("Spin");
        }
        if (state == State.Walking)
        {
            animator.Play("Walking");
        }
        if (state == State.Tornado)
        {
            animator.Play("Spin2");
        }
        if (state == State.Idle)
        {
            rb.velocity = Vector2.zero;
            animator.Play("Walking");
        }
        if (state == State.Hide)
        {
            GameManager.Instance.LockPosition = Vector2.zero;
            GetComponent<Collider2D>().enabled = false;
            animator.Play("Range");
        }
        if (state == State.MoveToLance)
        {
            animator.Play("Walking");
        }
        if (state == State.HoldLance)
        {
            animator.Play("HoldLance");
        }
        if (state == State.SwingLance)
        {
            animator.Play("SwingLance");
        }
        if (state == State.SwingLance2)
        {
            animator.Play("SwingLance2");
        }
        if(state == State.Idle)
        {
            if(halfhealth)
            AttackTimer++;
        }
        if (state == State.Defeated)
        {
            animator.Play("Dead");
            GameManager.Instance.LockPosition = transform.position;
            GameManager.Instance.Player.walkable = false;
            StartCoroutine(CutScene());
        }
        if (state == State.Defeated2)
        {
            animator.Play("Maskoff");
            GameManager.Instance.LockPosition = transform.position;
            GameManager.Instance.Player.walkable = false;
        }
    }

    void FixedUpdateState(State state)
    {
        timer += Time.fixedDeltaTime;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animalength = stateInfo.length;

        if (state == State.Freeze)
        {
            if (GameManager.Instance.Boss == this)
            {
                GameManager.Instance.LockPosition = transform.position;
                ChangeState(State.ShowingUp);
            }
        }
        if (state == State.ShowingUp)
        {
            GameManager.Instance.LockPosition = transform.position;
            if (timer>= animalength)
            {
                ChangeState(State.ThrowingLance);
                
            }
        }
        if (state == State.Walking)
        {
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position)/2;
            FaceTo(TargetPosition);
            rb.AddForce((TargetPosition-(Vector2)transform.position).normalized*speed);
            
            if(Vector2.Distance(TargetPosition, transform.position) < 1)
            {
                TargetPosition = Target.transform.position;
            }
            if (timer >= 1)
            {
                if (Random.Range(0, 3)==0)
                {
                    ChangeState(State.Hide);
                    return;
                }
                if (Vector2.Distance(Target.transform.position, transform.position) < 1)
                {
                    if (hasLance)
                        ChangeState(State.SwingLance);
                    else
                        ChangeState(State.MeleeAttack);
                    return;
                }
                if (Random.Range(0, 2) == 0 && hasLance)
                {
                    ChangeState(State.HoldLance);
                }
                else
                {
                    if (Vector2.Distance(TargetPosition, transform.position) < 5)
                    {
                        ChangeState(State.ThrowingDart);
                    }
                    else
                    {
                        int i = Random.Range(0, 2);
                        if (i == 0)
                        {
                            ChangeState(State.SpinningDart);
                        }
                        else if (i == 1)
                        {

                                ChangeState(State.Tornado);
                        }

                    }
                }
                
            }
        }
        if (state == State.Hide)
        {
            if (0.5f<timer && timer <= 1)
            {
                transform.position = Target.transform.position;
                rb.MovePosition((Vector2)transform.position);
            }
            if (timer >= 1.5f)
            {
                if (Random.Range(0, 2) == 0 && hasLance)
                {
                    ChangeState(State.SwingLance);
                }

                else
                {
                    int i = Random.Range(0, 4);
                    if (i == 0)
                    {
                        ChangeState(State.SpinningDart);
                    }
                    else if (i == 1)
                    {
                        ChangeState(State.Tornado);
                    }
                    else if (i == 2)
                    {
                        if (Random.Range(0, 2) == 0 && hasLance)
                        {
                            ChangeState(State.SwingLance);
                        }
                        else
                        {
                            ChangeState(State.MeleeAttack);
                        }
                       
                    }
                    else
                    {
                        ChangeState(State.ThrowingDart);
                    }
                }

            }
        }
        if(state == State.Idle)
        {
            if (health<=maxhealth/2&&!halfhealth)
            {
                halfhealth = true;
                ChangeState(State.MoveToLance);
                return;
            }
            if (halfhealth)
            {
                if (hasLance && AttackTimer > 5)
                {
                    AttackTimer = 0;
                    ChangeState(State.ThrowingLance);
                    return;
                }
                else if (!hasLance && AttackTimer > 5)
                {
                    AttackTimer = 0;
                    ChangeState(State.MoveToLance);
                    return;
                }
            }
            
            ChangeState(State.Walking);
        }
        if (state == State.MeleeAttack|| state == State.ThrowingDart || state == State.ThrowingLance)
        {
            if(state != State.ThrowingLance||halfhealth)
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            if (timer >= animalength)
                ChangeState(State.Idle);
        }
        
        if (state == State.ThrowingDart)
        {
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            attackcd += Time.fixedDeltaTime;
            if (attackcd > 0.2f)
            {
                for (int i = -1; i <= 1; i++)
                {
                    Bullets b = Instantiate(Bullets[0]).GetComponent<Bullets>();
                    b.transform.position = transform.position;

                    float angleOffset = i * 15f;

                    Vector2 direction = (TargetPosition - (Vector2)transform.position + Target.GetComponent<Rigidbody2D>().velocity / 2.5f).normalized;
                    direction = Quaternion.Euler(0, 0, angleOffset) * direction;

                    b.SetProperty(damage, 15, direction, true);
                }
                attackcd = -20;
            }
        }
        if (state == State.MeleeAttack)
        {
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            attackcd += Time.fixedDeltaTime;
            if (attackcd > 0.2f)
            {
                if (Vector2.Distance(Target.transform.position, transform.position) < 2f)
                {
                    Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, true);
                }
                attackcd = -20;
            }
        }
        if (state == State.SpinningDart)
        {
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            TargetPosition = Target.transform.position;
            attackcd += Time.fixedDeltaTime;
            if (timer>0.5f&& attackcd > 0.1f)
            {
                attackcd = 0;
                Bullets b = Instantiate(Bullets[0]).GetComponent<Bullets>();
                b.transform.position = transform.position;
                b.SetProperty(damage, 15, (TargetPosition- (Vector2)transform.position+Target.GetComponent<Rigidbody2D>().velocity/2.5f).normalized, false);
            }
            if (timer >= 1.5f)
                ChangeState(State.Idle);
        }
        if(state == State.Tornado)
        {
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            rb.AddForce((TargetPosition - (Vector2)transform.position).normalized * speed*15);

            if (Vector2.Distance(TargetPosition, transform.position) < 1)
            {
                TargetPosition = Target.transform.position;
            }
            if(attackcd>0)
            attackcd -= Time.fixedDeltaTime;
            if (timer>1f&& attackcd<=0)
            {
                if (halfhealth)
                {
                    for (int i = -3; i <= 3; i++)
                    {
                        Bullets b = Instantiate(Bullets[2]).GetComponent<Bullets>();
                        b.transform.position = transform.position;

                        float angleOffset = i * 60f;

                        Vector2 direction = (TargetPosition - (Vector2)transform.position + Target.GetComponent<Rigidbody2D>().velocity / 2.5f).normalized;
                        direction = Quaternion.Euler(0, 0, angleOffset) * direction;

                        b.SetProperty(damage, 12, direction, false);
                    }
                }
                if(Vector2.Distance(Target.transform.position, transform.position) < 1.5f)
                {
                    Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, true);
                }
                attackcd = 0.5f;
            }
                if (timer >= 4)
                ChangeState(State.Idle);
        }
        if (state == State.MoveToLance)
        {
            GameManager.Instance.LockPosition = transform.position;
            rb.MovePosition((Lance.position - transform.position).normalized * speed*Time.fixedDeltaTime +transform.position);
            if (Vector2.Distance(Lance.position, transform.position) < 1||timer>5)
            {
                Destroy(Lance.gameObject);
                ChangeState(State.HoldLance);
                for (int i = -6; i <= 6; i++)
                {
                    Bullets b = Instantiate(Bullets[2]).GetComponent<Bullets>();
                    b.transform.position = transform.position;

                    float angleOffset = i * 30f;

                    Vector2 direction = (TargetPosition - (Vector2)transform.position + Target.GetComponent<Rigidbody2D>().velocity / 2.5f).normalized;
                    direction = Quaternion.Euler(0, 0, angleOffset) * direction;

                    b.SetProperty(damage * 2, 12, direction, false);
                }
            }
        }
        if (state == State.HoldLance)
        {

            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            if (timer >= animalength)
            {
                if (lastState == State.MoveToLance)
                {
                    rb.velocity = Vector2.zero;
                    ChangeState(State.Idle);
                    
                }
                else
                {
                    ChangeState(State.SwingLance);
                }
            }
        }
        if(state == State.SwingLance2)
        {
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            if (timer >= animalength)
            {
                if (Random.Range(0,4)==0)
                {
                    ChangeState(State.Idle);
                }
                else
                {
                    ChangeState(State.SwingLance);
                }
            }
        }
        if (state == State.SwingLance)
        {
            GameManager.Instance.LockPosition = (Target.transform.position + transform.position) / 2;
            if (timer >= animalength)
            {
                if (Random.Range(0, 4) == 0)
                {
                    ChangeState(State.Idle);
                }
                else
                {
                    ChangeState(State.SwingLance2);
                }
            }
        }
        if(state == State.Defeated)
        {

        }
    }
    void ExitState(State state)
    {
        if (state == State.MoveToLance)
        {
            GameManager.Instance.LockPosition = Vector2.zero;
            
        }
        if (state == State.Hide)
        {
            GetComponent<Collider2D>().enabled = true;
 
        }
        if (state == State.HoldLance)
        {
            hasLance = true;
        }
    }
    public void AnimationTrigger(State state)
    {
        if(state == State.ThrowingLance)
        {
            Vector2 distanation = (StartPosition + Vector2.down * 16);
            if (halfhealth)
            {
                distanation = Target.transform.position;
            }
            GameManager.Instance.LockPosition = Vector2.zero;
            Lance b = Instantiate(Bullets[1]).GetComponent<Lance>();
            Lance = b.transform;
            b.transform.position = transform.position;
            b.SetProperty(damage, 2, distanation, true);
            hasLance=false;
        }
        if (state == State.Walking)
        {
            rb.AddForce((TargetPosition - (Vector2)transform.position) * speed*10);
        }
        if(state == State.SwingLance)
        {
            if (Vector2.Distance(Warnning.transform.position, Target.transform.position) < Warnning.localScale.x * 10)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, true);
            }
        }
        if (state == State.SwingLance2)
        {
            if (Vector2.Distance(Warnning.transform.position, Target.transform.position) < Warnning.localScale.x * 10)
            {
                Target.GetHit(damage, (Target.transform.position - transform.position).normalized * 10, false);
            }
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
    public override void DestroyBehavior()
    {
        tag = "Player";
        deadcount++;
        if (deadcount == 1)
        {
            ChangeState(State.Defeated);
        }

        if (deadcount == 2)
        {
            ChangeState(State.Defeated2);
        }
        if (deadcount == 3)
        {
            Destroy(gameObject);
        }
    }
    public void SendText(string t)
    {
        GameManager.Instance.textMeshProUGUI.text = t;
        GameManager.Instance.textMeshProUGUI.fontSize = 30;
        GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().SetTrigger("Set");
        GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().speed = 1;
        GameManager.Instance.textMeshProUGUI.color = Color.red;
    }
    public void SendText(string t,Color color)
    {
        GameManager.Instance.textMeshProUGUI.text = t;
        GameManager.Instance.textMeshProUGUI.fontSize = 30;
        GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().SetTrigger("Set");
        GameManager.Instance.textMeshProUGUI.transform.parent.GetComponent<Animator>().speed = 1;
        GameManager.Instance.textMeshProUGUI.color = color;
    }
    IEnumerator CutScene()
    {
        Color color;
        UnityEngine.ColorUtility.TryParseHtmlString("FFE499", out color);
        float time=0;
        Time.timeScale = 0.5f;
        GameManager.Instance.isCutscene = true;
        Application.targetFrameRate = -1;
        GameManager.Instance.ScreenShake(2,2);
        GameManager.Instance.CameraSize.Insert(0,5);
        GameManager.Instance.audioSource.clip= null;
        GameManager.Instance.audioSource.Play();
        while (time < 4)
        {
            GameManager.Instance.LockPosition = transform.position;
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time=0;
        GameManager.Instance.Player.ChangeState(PlayerMovement.State.Idle);
        GameManager.Instance.PlaySound(GameManager.Instance.BGM[4],0.2f);
        Time.timeScale = 1;
        SendText("Fine, You win.");
        yield return null;
        while (time<6&&!Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        DestroyBehavior();
        GameManager.Instance.CameraSize.Insert(0, 4);
        yield return null;
        while (time < 4 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        SendText("I dont know what your purpose is.");
        yield return null;
        while (time < 6 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        SendText("But now you can do whatever you want.");
        yield return null;
        while (time < 6 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        SendText("Just kill me already...");
        yield return null;
        while (time < 6 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        GameManager.Instance.PlaySound(GameManager.Instance.BGM[5], 0.2f);
        
        GameManager.Instance.CameraSize.Insert(0, 6);
        GameManager.Instance.LockPosition = Target.transform.position;
        time = 0;
        SendText("...", color);
        yield return null;
        while (time < 5 && !Input.GetKeyUp(KeyCode.Mouse0))
        {

            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        SendText("I am here to hunt the vampire that threatens the safety of the village.", color);
        yield return null;
        while (time < 6 && !Input.GetKeyUp(KeyCode.Mouse0))
        {

            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        SendText("To make sure you won't be a threat of villagers, I have no choice but to...", color);
        yield return null;
        while (time < 6 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        GameManager.Instance.EnterCutScene();
        time = 0;
        yield return null;
        while (time < 3)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        GameManager.Instance.PlaySound(GameManager.Instance.BGM[6], 0.2f);
        time = 0;
        color =Color.white;
        SendText("In the end, the threat of villagers is gone.", color);
        yield return null;
        while (time < 3 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        SendText("While Adventurer and Vampire lived happily ever after.", color);
        yield return null;
        while (time < 3f && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        GameManager.Instance.EnterCutScene2();
        yield return null;
        while (time < 5 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        time = 0;
        SendText("Bravo, Bravo.", color);
        yield return null;
        while (time < 10 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        GameManager.Instance.PlayCG("ReverseCG");
        time = 0;
        while (time < 4)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        int minutes = (int)(GameManager.Instance.Timer / 60);
        int seconds = (int)(GameManager.Instance.Timer % 60);
        string formattedTime = $"{minutes}:{seconds:D2}";
        time = 0;
        SendText("Total Time Cost: " + formattedTime, color);
        yield return null;
        while (time < 3 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        if (!GameManager.Instance.BuffList.Contains(GameManager.Buff.FinalBoss))
        {
            time = 0;
            SendText("Thanks For Playing!", color);
            yield return null;
            while (time < 3 && !Input.GetKeyUp(KeyCode.Mouse0))
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            time = 0;
            SendText("Developed by: BaiMouse", color);
            yield return null;
            while (time < 3 && !Input.GetKeyUp(KeyCode.Mouse0))
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }


            GameManager.Instance.BuffList.Add(GameManager.Buff.FinalBoss);
            time = 0;
            SendText("You now get the ability from Vampire of shooting dart while spinning!", color);
            yield return null;
            while (time < 3 && !Input.GetKeyUp(KeyCode.Mouse0))
            {
                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        GameManager.Instance.Timer = 0;
        time = 0;
        SendText("you can now play again with your BossItems!", color);
        yield return null;
        while (time < 3 && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        GameManager.Instance.isCutscene = false;
        SceneManagerScript.instance.LoadScene("YiXuanScene");

    }
}
