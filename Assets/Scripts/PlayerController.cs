using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Control")]
    private Rigidbody2D myRigidBody;
    public bool facingRight;
    public float maxSpeed = 5.0f;
    public float gravityScale = 1.0f;

    [SerializeField] private Animator Anim;
    public SpriteRenderer sprite;

    public bool beAbleToMove;
    public GameObject runDust;
    public ParticleSystem dust;

    public GameObject deathState;
    public bool dead;

    ParticleSystem blood_small;
    ParticleSystem blood_large;

    [Header("Stats")]
    public int maxHealth = 5;
    [SerializeField] int currentHealth;

    Animator healthBar;
    
    public float kaioKenMultiplier;

    [Header("Jump")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private GameObject groundChecker;
    public bool isOnGround;

    private bool jump;
    public float jumpForce = 10.0f;
    public float jumpTime;
    private float jumpTimeCounter;
    public bool canDoubleJump;

    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    public GameObject jumpDust;
    public ParticleSystem jumpDustP;

    [Header("Attack")]

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 40;

    public LayerMask enemyLayer;

    public float attackRate = 2f;
    float nextAttackTime = 0f;

    public bool canAttack;

    [Header("Dash")]
    public bool currentlyDashing;
    public float dashSpeed;
    public bool dashCooldown;
    public float dashDuration;
    public float dashCooldownTime;
    public bool backDash;

    public GameObject particlePrefab;
    public GameObject Particle;

    [Header("Camera")]
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        healthBar = GameObject.Find("Health Bar").GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        deathState = GameObject.Find("Game Over State");
        runDust = GameObject.Find("Running Dust");
        dust = runDust.GetComponent<ParticleSystem>();
        jumpDust = GameObject.Find("Jump dust");
        jumpDustP = jumpDust.GetComponent<ParticleSystem>();

        Particle = Instantiate(particlePrefab, transform.position, transform.rotation);
        Particle.SetActive(false);

        blood_small = GameObject.Find("Quick Blood Splatter(hurt)").GetComponent<ParticleSystem>();
        blood_large = GameObject.Find("Quick Blood Splatter(death)").GetComponent<ParticleSystem>();

        gravityScale = myRigidBody.gravityScale;

        facingRight = true;

        kaioKenMultiplier = 1f;

        beAbleToMove = false;
        Anim.Play("Recover");
        StartCoroutine(HealthRecover());

    }
    IEnumerator HealthRecover()
    {
        for (int i = 0; i < maxHealth; i++)
        {
            currentHealth++;
            yield return new WaitForSeconds(0.3f);
        }
        beAbleToMove = true;
        canAttack = true;
    }



    // Update is called once per frame
    void Update()
    {
        healthBar.SetInteger("Health", currentHealth);
        isOnGround = Physics2D.OverlapCircle(groundChecker.transform.position, 0.05f, whatIsGround);
        Anim.SetBool("Grounded", isOnGround);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            {
                if (isOnGround)
                {
                    jump = true;
                    canDoubleJump = true;

                }
                else
                {
                    if (canDoubleJump)
                    {
                        jump = true;
                        canDoubleJump = false;
                    }
                }
            }
        }
        if (beAbleToMove && !dead)
        {
            if (Time.time >= nextAttackTime)
            {
                if ((Input.GetKeyDown(KeyCode.P)) && (canAttack == true))
                {
                    beAbleToMove = false;
                    Anim.SetTrigger("Attack");
                    nextAttackTime = Time.time + 1f / attackRate;
                }
            }
        }
        if(!dashCooldown && !dead)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                currentlyDashing = true;
                Particle.SetActive(true);

                StartCoroutine(Dash());
            }

            else if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                backDash = true;
                Particle.SetActive(true);

                StartCoroutine(BackDash());
            }

            else
            {
                StartCoroutine(CoolDown());
            }
        }
        if (dashCooldown)
        {
            StartCoroutine(CoolDown());
        }

        Vector3 characterScale = transform.localScale;

        if (facingRight)
        {
            characterScale.x = -2.397341f;
        }
        else
        {
            characterScale.x = 2.397341f;
        }

        transform.localScale = characterScale;
        if (dead)
        {
            gameObject.layer = 10;
            beAbleToMove = false;
            deathState.transform.position = Vector2.MoveTowards(deathState.transform.position, new Vector2(deathState.transform.position.x, 540f), 10f);
            if (deathState.transform.position.y < 0)
            {
                deathState.transform.position = new Vector2(deathState.transform.position.x, 0f);
            }
        }
    }
    private void FixedUpdate()
    {
        if(beAbleToMove)
        {
            checkInputs();
        }
        if (currentlyDashing)
        {
            if (facingRight)
            {
                myRigidBody.velocity = Vector2.right * dashSpeed * kaioKenMultiplier;
            }
            else
            {
                myRigidBody.velocity = Vector2.left * dashSpeed * kaioKenMultiplier;
            }
        }
    }

    public void checkInputs()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (moveInput != 0)
        {
            Anim.SetInteger("AnimState", 2);
            if (moveInput > 0)
            {
                facingRight = true;
            }
            else if (moveInput < 0)
            {
                facingRight = false;
            }
        }
        else
        {
            Anim.SetInteger("AnimState", 0);
        }


        //sfloat movementValueX = 1.0f;
        myRigidBody.velocity = new Vector2(moveInput * maxSpeed * kaioKenMultiplier, myRigidBody.velocity.y);

        if (jump)
        {
            CreateJumpDust();
            Anim.SetTrigger("Jump");
            myRigidBody.velocity = new Vector2(myRigidBody.velocity.x , 0);
            myRigidBody.AddForce(new Vector3(0, jumpForce * kaioKenMultiplier));
            jump = false;
        }

        if (myRigidBody.velocity.y < 0)
        {
            CreateJumpDust();
            myRigidBody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        var main = dust.main;
        var vol = dust.velocityOverLifetime;
        if (isOnGround)
        {
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            vol.x = -0.2f;
            vol.y = 0.2f;
        }
        else
        {
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Custom;
            main.customSimulationSpace = groundChecker.transform;
            vol.x = 0.2f;
            vol.y = 0f;
        }
        if (myRigidBody.velocity.x > 0 && isOnGround)
        {
            CreateDust();
        }
    }
    public void Attack()
    {
        myRigidBody.velocity = Vector2.zero;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach(Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyController>().TakeDamage(attackDamage);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    IEnumerator Dash()
    {
        if(currentlyDashing)
        {
            beAbleToMove = false;
            Anim.SetBool("Dash", true);
            myRigidBody.velocity = Vector2.zero;
            yield return new WaitForSeconds(dashDuration);
            currentlyDashing = false;
            canAttack = true;
            Anim.SetBool("Dash", false);
            beAbleToMove = true;
            dashCooldown = true;
            if (currentHealth <= 0)
            {
                Die();
            }

            yield break;
        }
    }
    IEnumerator BackDash()
    {
        if (backDash)
        {
            beAbleToMove = false;
            Anim.SetBool("Dash", true);
            myRigidBody.velocity = Vector2.zero;

            if (facingRight)
            {
                myRigidBody.velocity = Vector2.left * dashSpeed/2 * kaioKenMultiplier;
            }
            else
            {
                myRigidBody.velocity = Vector2.right * dashSpeed/2 * kaioKenMultiplier;
            }

            yield return new WaitForSeconds(dashDuration);
            backDash = false;
            canAttack = true;
            Anim.SetBool("Dash", false);
            beAbleToMove = true;
            dashCooldown = true;
            if (currentHealth <= 0)
            {
                Die();
            }

            yield break;
        }
    }
    IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(dashCooldownTime);
        dashCooldown = false;
        //Destroy(GameObject.FindWithTag("Afterimage"));
        yield return null;
    }
    public void Move()
    {
        beAbleToMove = true;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Anim.SetTrigger("Hurt");
        beAbleToMove = false;
        blood_small.Play();
        myRigidBody.velocity = Vector2.zero;
        if (currentHealth <= 0)
        {
            Die();
            blood_large.Play();
        }
        else
        {
            if (facingRight)
            {
                myRigidBody.AddForce(Vector2.left * 100.0f);
            }
            else
            {
                myRigidBody.AddForce(Vector2.right * 100.0f);
            }
            StartCoroutine(Invinisble());
        }
    }
    IEnumerator Invinisble()
    {
        gameObject.layer = 0;
        float _progress = 0.0f;
        Color tmp = sprite.color;
        tmp.a = 0f;

        Mathf.Lerp(tmp.a, 255, _progress); //startAlpha = 0 <-- value is in tmp.a
        _progress += Time.deltaTime * 1.5f;

        yield return new WaitForSeconds(0.5f);

        myRigidBody.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.2f);

        beAbleToMove = true;
        gameObject.layer = 11;


        yield return null;
    }
    void Die()
    {
        myRigidBody.velocity = Vector2.zero;
        dead = true;
        Anim.SetTrigger("Death");
        Debug.Log("Player Died");
    }

    void CreateDust()
    {
        dust.Play();
    }
    void CreateJumpDust()
    {
        jumpDustP.Play();
    }
    public void lockOn()
    {

    }
}
