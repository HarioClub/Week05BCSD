using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float jumpPower = 12f;
    public LayerMask platformLayer;
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    Collider2D playerCollider;
    AudioSource audioSource;
    bool isGrounded;
    bool isDamaged;
    bool isDead;
    float controlLockUntil;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isDead || gameManager.IsFinished || Time.time < controlLockUntil)
            return;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            isGrounded = false;
            anim.SetBool("IsJumping", true);
            PlaySound(audioJump);
        }

        if (Input.GetButtonUp("Horizontal"))
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);

        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        if (Mathf.Abs(rigid.linearVelocity.x) < 0.3)
            anim.SetBool("IsWalking", false);
        else
            anim.SetBool("IsWalking", true);
    }

    private void FixedUpdate()
    {
        if (isDead || gameManager.IsFinished)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        if (rigid.linearVelocity.x > maxSpeed)
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        else if (rigid.linearVelocity.x < maxSpeed * (-1))
            rigid.linearVelocity = new Vector2(maxSpeed * (-1), rigid.linearVelocity.y);

        Bounds bounds = playerCollider.bounds;
        float distance = bounds.extents.y + 0.08f;
        Vector2 center = bounds.center;
        Vector2 side = Vector2.right * bounds.extents.x * 0.8f;
        bool ground = Physics2D.Raycast(center, Vector2.down, distance, platformLayer)
            || Physics2D.Raycast(center - side, Vector2.down, distance, platformLayer)
            || Physics2D.Raycast(center + side, Vector2.down, distance, platformLayer);
        isGrounded = rigid.linearVelocity.y <= 0.1f && ground;
        anim.SetBool("IsJumping", !isGrounded);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || gameManager.IsFinished || !collision.gameObject.CompareTag("Enemy"))
            return;

        EnemyMove enemy = collision.gameObject.GetComponent<EnemyMove>();
        bool fromAbove = false;
        for (int i = 0; i < collision.contactCount; i++)
            fromAbove |= collision.GetContact(i).normal.y > 0.5f;

        if (enemy != null && !enemy.IsDead && fromAbove && transform.position.y > collision.transform.position.y)
        {
            gameManager.AddPoint(100);
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * 6, ForceMode2D.Impulse);
            isGrounded = false;
            anim.SetBool("IsJumping", true);
            PlaySound(audioAttack);
            enemy.OnDamaged();
        }
        else if (!isDamaged)
        {
            OnDamaged(collision.transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || gameManager.IsFinished)
            return;

        if (collision.CompareTag("Item") && collision.gameObject.activeSelf)
        {
            int point = collision.name.Contains("Gold") ? 300 : collision.name.Contains("Silver") ? 100 : 50;
            collision.gameObject.SetActive(false);
            gameManager.AddPoint(point);
            PlaySound(audioItem);
        }
        else if (collision.CompareTag("Finish"))
        {
            PlaySound(audioFinish);
            gameManager.NextStage();
        }
    }

    private void OnDamaged(Vector2 targetPos)
    {
        gameManager.HealthDown();
        if (isDead)
            return;

        isDamaged = true;
        gameObject.layer = LayerMask.NameToLayer("PlayerDamaged");
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        int direction = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(new Vector2(direction, 1) * 5, ForceMode2D.Impulse);
        controlLockUntil = Time.time + 0.2f;
        anim.SetTrigger("Damaged");
        PlaySound(audioDamaged);
        Invoke(nameof(OffDamaged), 3f);
    }

    private void OffDamaged()
    {
        isDamaged = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
        spriteRenderer.color = Color.white;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void SetVelocityZero()
    {
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0;
    }

    public void Respawn(Vector3 position)
    {
        CancelInvoke();
        isDead = false;
        isGrounded = false;
        controlLockUntil = 0;
        OffDamaged();
        spriteRenderer.flipY = false;
        playerCollider.enabled = true;
        SetVelocityZero();
        rigid.position = position;
        transform.position = position;
        anim.ResetTrigger("Damaged");
        anim.SetBool("IsJumping", true);
        anim.SetBool("IsWalking", false);
    }

    public void OnDie()
    {
        if (isDead)
            return;

        CancelInvoke();
        isDead = true;
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;
        playerCollider.enabled = false;
        SetVelocityZero();
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        PlaySound(audioDie);
        Invoke(nameof(StopGame), 2f);
    }

    private void StopGame()
    {
        Time.timeScale = 0;
    }
}
