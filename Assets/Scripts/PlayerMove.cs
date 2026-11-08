using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float jumpPower = 12f;
    public LayerMask platformLayer;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    Collider2D playerCollider;
    bool isGrounded;
    bool isDamaged;
    float controlLockUntil;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Time.time < controlLockUntil)
            return;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            isGrounded = false;
            anim.SetBool("IsJumping", true);
        }

        if (Input.GetButtonUp("Horizontal"))
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);

        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
            spriteRenderer.flipX = h < 0;

        anim.SetBool("IsWalking", Mathf.Abs(rigid.linearVelocity.x) >= 0.3f);
    }

    private void FixedUpdate()
    {
        float h = Time.time >= controlLockUntil ? Input.GetAxisRaw("Horizontal") : 0;
        rigid.AddForce(Vector2.right * h * 2f, ForceMode2D.Impulse);
        rigid.linearVelocity = new Vector2(Mathf.Clamp(rigid.linearVelocity.x, -maxSpeed, maxSpeed), rigid.linearVelocity.y);

        Bounds bounds = playerCollider.bounds;
        RaycastHit2D hit = Physics2D.BoxCast(bounds.center,
            new Vector2(bounds.size.x * 0.8f, bounds.size.y * 0.95f),
            0, Vector2.down, 0.08f, platformLayer);
        isGrounded = rigid.linearVelocity.y <= 0.1f && hit.collider != null;
        anim.SetBool("IsJumping", !isGrounded);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isDamaged)
            OnDamaged(collision.transform.position);
    }

    private void OnDamaged(Vector2 targetPos)
    {
        isDamaged = true;
        gameObject.layer = LayerMask.NameToLayer("PlayerDamaged");
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        int direction = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(new Vector2(direction, 1) * 5, ForceMode2D.Impulse);
        controlLockUntil = Time.time + 0.2f;
        anim.SetTrigger("Damaged");
        Invoke(nameof(OffDamaged), 3f);
    }

    private void OffDamaged()
    {
        isDamaged = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
        spriteRenderer.color = Color.white;
    }
}
