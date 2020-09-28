using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enemystate
{ idle, patrol, tracking, attack }
public class EnemyController : MonoBehaviour
{
    #region Enemy Control
    [Header("Enemy Variables")]
    public Enemystate state;
    private Rigidbody2D myRigidBody;
    public bool facingRight;

    public int maxHealth = 100;
    [SerializeField] int currentHealth;

    [SerializeField] private Animator Anim;

    Collider2D myCollider;

    public bool dead;

    ParticleSystem blood_small;
    ParticleSystem blood_large;
    #endregion

    #region Jump
    [Header("Jump Variables")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private GameObject groundChecker;
    public bool isOnGround;
    #endregion

    #region Attack
    [Header("Attack Variables")]

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 1;

    public LayerMask playerLayer;

    public float attackRate = 2f;
    float nextAttackTime = 0f;
    #endregion

    #region AI
    [Header("AI Variables")]
    public GameObject hotZone;
    public GameObject triggerArea;
    public float attackDistance;
    public float moveSpeed;
    public float timer;

    public Transform target;
    private float distance;
    private bool attackMode;
    public bool inRange;
    private bool cooling;
    private float intTimer;

    PlayerController player;

    public bool takenDamage;

    public Transform leftLimit;
    public Transform rightLimit;
    #endregion

    private void Awake()
    {
        intTimer = timer;
        SelectTargets();
    }
    // Start is called before the first frame update
    void Start()
    {
        state = Enemystate.idle;
        myRigidBody = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();

        facingRight = false;

        currentHealth = maxHealth;

        myCollider = GetComponent<CapsuleCollider2D>();

        player = GameObject.Find("Character").GetComponent<PlayerController>();
        blood_small = GameObject.Find("Quick Blood Splatter(hurt)").GetComponent<ParticleSystem>();
        blood_large = GameObject.Find("Quick Blood Splatter(death)").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        isOnGround = Physics2D.OverlapCircle(groundChecker.transform.position, 0.05f, whatIsGround);
        Anim.SetBool("Grounded", isOnGround);


        Vector3 characterScale = transform.localScale;
        if (currentHealth <= 0)
        {
            dead = true;
        }
        else if (!Anim.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
        {
            if (!attackMode)
            {
                Move();
            }
            if (!insideOfLimits() && !inRange && !Anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                SelectTargets();
            }
            if (inRange)
            {
                EnemyLogic(); ;
            }
            
        }
        if (target.position.x > gameObject.transform.position.x)
        {
            facingRight = true;
        }
        else
        {
            facingRight = false;
        }
    }
    void EnemyLogic()
    {
        distance = Vector2.Distance(transform.position, target.position);
        if (player.dead == false)
        {
            if (distance > attackDistance)
            {
                state = Enemystate.tracking;
                StopAttack();

            }
            else if (attackDistance >= distance && cooling == false)
            {
                state = Enemystate.attack;
                Attack();
            }

            if (cooling)
            {
                Cooldown();
                state = Enemystate.idle;
                Anim.ResetTrigger("Attack");
            }
        }
    }
    void Move()
    {
        Flip();
        Anim.SetInteger("AnimState", 2);
        if (!Anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            Vector2 targetPosition = new Vector2(target.position.x, transform.position.y);

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        takenDamage = true;
        Anim.SetTrigger("Hurt");
        blood_small.Play();

        if (currentHealth <= 0)
        {
            Die();
            blood_large.Play();
        }
    }
    void StopAttack()
    {
        cooling = false;
        attackMode = false;
        Anim.ResetTrigger("Attack");
    }
    public void Attack()
    {
        timer = intTimer;
        attackMode = true;

        Anim.Play("Attack");
        Anim.SetInteger("AnimState", 0);
    }
    public void dealDamage()
    {
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (Collider2D enemy in hitPlayer)
        {
            player.TakeDamage(attackDamage);
        }
    }
    void Die()
    {
        Anim.SetTrigger("Death");
        gameObject.layer = 10;
        myCollider.enabled = false;
        myRigidBody.bodyType = RigidbodyType2D.Static;
        Debug.Log("Enemy Died");
    }
    public void TriggerCooling()
    {
        cooling = true;
    }
    void Cooldown()
    {
        timer -= Time.deltaTime;

        if (timer <= 0 && cooling && attackMode)
        {
            cooling = false;
            timer = intTimer;
        }
    }
    void endOfHurt()
    {
        takenDamage = false;
    }
    private bool insideOfLimits()
    {
        return transform.position.x > leftLimit.position.x && transform.position.x < rightLimit.position.x; 
    }
    public void SelectTargets()
    {
        float distanceToLeft = Vector2.Distance(transform.position, leftLimit.position);
        float distanceToRight = Vector2.Distance(transform.position, rightLimit.position);

        if (distanceToLeft > distanceToRight)
        {
            target = leftLimit;
        }
        else
        {
            target = rightLimit;
        }
    }
    public void Flip()
    {
        Vector3 rotation = transform.eulerAngles;
        if(transform.position.x > target.position.x)
        {
            rotation.y = 180f;
        }
        else
        {
            rotation.y = 0f;
        }
        transform.eulerAngles = rotation;
    }
}
