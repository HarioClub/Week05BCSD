using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public int nextMove;
    public LayerMask platformLayer;

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
        Think();
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void FixedUpdate()
    {
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
}
