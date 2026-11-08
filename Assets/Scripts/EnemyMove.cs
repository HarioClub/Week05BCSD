using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public int nextMove;
    public LayerMask platformLayer;
    public bool IsDead { get; private set; }

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    Collider2D enemyCollider;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        IsDead = false;
        enemyCollider.enabled = true;
        spriteRenderer.color = Color.white;
        spriteRenderer.flipY = false;
        Think();
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void FixedUpdate()
    {
        if (IsDead)
            return;

        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocity.y);
        if (nextMove == 0)
            return;

        Bounds bounds = enemyCollider.bounds;
        Vector2 front = new Vector2(bounds.center.x + nextMove * (bounds.extents.x + 0.15f), bounds.center.y);
        bool groundAhead = Physics2D.Raycast(front, Vector2.down, bounds.extents.y + 0.4f, platformLayer);
        bool wallAhead = Physics2D.Raycast(bounds.center, Vector2.right * nextMove, bounds.extents.x + 0.2f, platformLayer);
        if (!groundAhead || wallAhead)
            Turn();
    }

    private void Think()
    {
        nextMove = Random.Range(-1, 2);
        UpdateDirection();
        Invoke(nameof(Think), Random.Range(2f, 5f));
    }

    private void Turn()
    {
        nextMove *= -1;
        UpdateDirection();
        CancelInvoke(nameof(Think));
        Invoke(nameof(Think), 3f);
    }

    private void UpdateDirection()
    {
        anim.SetInteger("WalkSpeed", nextMove);
        if (nextMove != 0)
            spriteRenderer.flipX = nextMove > 0;
    }

    public void OnDamaged()
    {
        if (IsDead)
            return;

        IsDead = true;
        CancelInvoke();
        nextMove = 0;
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        spriteRenderer.flipY = true;
        enemyCollider.enabled = false;
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        Invoke(nameof(DeActive), 2f);
    }

    private void DeActive()
    {
        gameObject.SetActive(false);
    }
}
