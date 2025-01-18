using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Creature Property")]
    [SerializeField] protected float maxhealth;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    [SerializeField] protected bool isTarget;
    protected float gethitcd, maxgethitcd;
    [SerializeField] public float health;
    public float CurrentSpeed;
    public float Size = 1;
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
        if (isTarget || GameManager.Instance.PlayerTarget == transform)
        {
            SetSelfAsTarget(2f);
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
        Destroy(gameObject);
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

}
