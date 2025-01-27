using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Entity : MonoBehaviour
{
    [Header("Creature Property")]
    [SerializeField] protected float maxhealth;
    [SerializeField] public float speed;
    [SerializeField] protected float damage;
    [SerializeField] protected bool isTarget;
    [SerializeField] protected bool isBoss=false;
    protected float gethitcd, maxgethitcd;
    [SerializeField] public float health;
    public float CurrentSpeed;
    public float Size = 1;
    public GameObject Loot;
    [Header("Private Componets")]
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Vector2 localScale;
    protected Animator animator;
    [Header("Audio Source")]
    public AudioSource audioSource;
    public AudioClip[] clip;
    protected virtual void Awake()
    {
        health = maxhealth;
        gethitcd = maxgethitcd;
        if (!GetComponent<Rigidbody2D>())
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        else
        {
            rb = GetComponent<Rigidbody2D>();
        }
        audioSource = GetComponent<AudioSource>();
        rb.gravityScale = 0;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        localScale = transform.localScale;
    }
    protected virtual void Update()
    {
        
    }
    protected virtual void FixedUpdate()
    {
        if (isTarget)
        {
            SetSelfAsTarget(2f);
        }
        else if(GameManager.Instance.Player.Target == transform)
        {
            GameManager.Instance.Player.Target = null;
            GameManager.Instance.PlayerTarget = null;
        }
        if (gethitcd > 0)
        {
            gethitcd -= Time.fixedDeltaTime;
        }
    }
    /// <summary>
    /// If distance to cursor is less than certain distance, set itself as the player's target
    /// </summary>
    public void SetSelfAsTarget(float distance)
    {
        if (Vector2.Distance(GameManager.Instance.MousePosition, transform.position) < distance)
        {
            if (GameManager.Instance.PlayerTarget == null)
            {
                GameManager.Instance.PlayerTarget = transform;
            }
        }
        else if (GameManager.Instance.PlayerTarget == transform)
        {
            GameManager.Instance.PlayerTarget = null;
        }
    }
    /// <summary>
    /// Do the get hit behavior, if health less than 0 call the die behavior
    /// </summary>
    public virtual bool GetHit(float damage)
    {
        return GetHit(damage, Vector2.zero, false);
    }
    public virtual bool GetHit(float damage,Vector2 knockback)
    {
        return GetHit(damage, knockback, false);
    }
    /// <summary>
    /// Do the get hit behavior, if health less than 0 call the die behavior
    /// </summary>
    public virtual bool GetHit(float damage,Vector2 knockback,bool MustHit)
    {
        if (gethitcd <= 0|| MustHit)
        {
            gethitcd = maxgethitcd;
            health -= damage;
            if (health > maxhealth)
            {
                health = maxhealth;
            }
            if (rb) rb.AddForce(knockback,ForceMode2D.Impulse);
            StartCoroutine(GetHitEffect(0.2f));
            if (health <= 0)
            {
                DestroyBehavior();
                
                    
            }
            return true;
        }
        else
        {
            return false;
        }
        
    }
    /// <summary>
    /// Die behavior
    /// </summary>
    public virtual void DestroyBehavior()
    {
        if (isBoss)
        {
            StartCoroutine(Dying(5));
        }
        else
        {
            if (Loot)
            {
                Instantiate(Loot).transform.position = transform.position;
            }
            Destroy(gameObject);
            
        }

    }
    /// <summary>
    /// When get hit, set the sprite to white 
    /// </summary>
    public virtual IEnumerator GetHitEffect(float duration)
    {
        float counter=0;
        spriteRenderer.material.SetFloat("_HurtDuration",1);
        while (counter <= duration)
        {
            counter+=Time.deltaTime;
            spriteRenderer.material.SetFloat("_HurtDuration", 1*(1-counter/duration));
            yield return null;
        }
        spriteRenderer.material.SetFloat("_HurtDuration", 0);
        
    }
    /// <summary>
    /// Face to direction, change the scale of x
    /// </summary>
    /// 
    IEnumerator Dying(float time)
    {
        GameManager.Instance.isCutscene = true;
        tag = "Untagged";
        float count = 0;
        Vector2 ve = transform.position;
        speed = 0;
        if (clip.Length>0)
        {
            audioSource.volume = 0.8f;
            audioSource.clip = clip[0];
            audioSource.Play();
        }
        if (animator)
        {
            animator.speed = 0;
        }
        while (count < time)
        {
            gethitcd = 10;
            count += Time.unscaledDeltaTime;
            localScale += (Vector2.zero- localScale) * 0.001f;
            transform.localScale = new Vector2(localScale.x, localScale.y);
            Vector2 newpoint = ve +  new Vector2(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f))*(1-count/time);
            transform.position = new Vector3(newpoint.x, newpoint.y);
            yield return null;
        }
        isBoss = false;
        GameManager.Instance.isCutscene = false;
        DestroyBehavior();
        yield return null;
    }
    public virtual void FaceTo(Vector2 position)
    {
        if (position.x > transform.position.x)
        {
            transform.localScale = new Vector2(localScale.x, localScale.y);
        }
        else
        {
            transform.localScale = new Vector2(-localScale.x, localScale.y);
        }
    }
    public void Shake(float level)
    {
        GameManager.Instance.ScreenShake(level/2,level);
    }
}
