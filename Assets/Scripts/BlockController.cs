using UnityEngine;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float moveRange = 2.5f;
    public float gravityWhenDropped = 1.5f;

    private Rigidbody2D rb;
    private float stillTimer;
    private bool hasTouchedSomething;

    public bool IsDropped { get; private set; }
    public bool IsSettled => IsDropped && hasTouchedSomething && stillTimer > 0.35f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        if (IsDropped)
            return;

        float x = Mathf.Sin(Time.time * moveSpeed) * moveRange;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);

        bool pressed =
            Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (pressed)
        {
            Drop();
        }
    }

    private void FixedUpdate()
    {
        if (!IsDropped || !hasTouchedSomething)
            return;

        bool almostStill =
            rb.linearVelocity.magnitude < 0.05f &&
            Mathf.Abs(rb.angularVelocity) < 2f;

        if (almostStill)
            stillTimer += Time.fixedDeltaTime;
        else
            stillTimer = 0f;
    }

    private void Drop()
    {
        IsDropped = true;
        rb.gravityScale = gravityWhenDropped;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDropped)
        {
            hasTouchedSomething = true;
        }
    }
}