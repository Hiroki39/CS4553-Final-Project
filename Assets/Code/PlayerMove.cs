using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    public Material Mat1;
    public Material Mat2;
    public GameObject[] checkPoints;
    public bool[] checkPointIsBlues;
    public float[] checkPointScales;
    public GameObject blueGemPicked;
    public AudioSource objectSound;
    public TimeManager timeManager;
    public int totalSlowmoDefault = 3;
    [HideInInspector] public float jumpForce = 6f;
    [HideInInspector] public bool jumped = false;

    Renderer rend;
    TrailRenderer trend;
    Rigidbody rb;
    CameraFollow cf;
    ParticleSystem ps;
    float force = 12f;
    float maxSpeed = 12f;
    bool isAlive = true;
    bool isBlue;
    bool grounded = false;
    // bool isBallSlowmo = false;
    float jumpTimer;
    float jumpDelay = 0.25f;
    bool isBallSlowmoActive = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        trend = GetComponent<TrailRenderer>();
        ps = GetComponentInChildren<ParticleSystem>();
        cf = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>();

        Physics.defaultContactOffset = 0.00000001f;

        isBlue = checkPointIsBlues[PublicVars.checkPoint];
        if (isBlue)
        {
            rend.material = Mat1;
            trend.startColor = Mat1.color;
            trend.endColor = Mat1.color;
        }
        else
        {
            rend.material = Mat2;
            trend.startColor = Mat2.color;
            trend.endColor = Mat2.color;
        }
        trend.enabled = false;
        trend.time = 0.5f;

        transform.localScale = new Vector3(checkPointScales[PublicVars.checkPoint], checkPointScales[PublicVars.checkPoint], checkPointScales[PublicVars.checkPoint]);

        transform.position = checkPoints[PublicVars.checkPoint].transform.position + new Vector3(0, 1.5f, 0);
        StartCoroutine(WaitToMove());
        for (int i = 0; i <= PublicVars.checkPoint; ++i)
        {
            Destroy(checkPoints[i]);
        }

        StartCoroutine(AutoSave());
    }

    void Update()
    {
        if (transform.position.y < -12)
        {
            isAlive = false;
        }
        if (!isAlive)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.E) && isBallSlowmoActive == false)
        {
            if (totalSlowmoDefault > 0)
            {
                isBallSlowmoActive = true;
                totalSlowmoDefault -= 1;
                ps.Play();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isBlue)
            {
                rend.material = Mat2;
                trend.startColor = Mat2.color;
                trend.endColor = Mat2.color;
            }
            else
            {
                rend.material = Mat1;
                trend.startColor = Mat1.color;
                trend.endColor = Mat1.color;
            }
            isBlue = !isBlue;

            // if (isBallSlowmo)
            // {
            //     timeManager.StopSlowmotion();
            //     isBallSlowmo = false;
            // }
        }
        if (grounded != isGrounded())
        {
            if (grounded == true)
            {
                trend.Clear();
                trend.enabled = true;
            }
            else
            {
                trend.enabled = false;
            }
            grounded = isGrounded();
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpTimer = Time.time + jumpDelay;

            if (isBallSlowmoActive)
            {
                Invoke("DoSlowmo", 0.5f);
                isBallSlowmoActive = false;
                Invoke("StopSlowmo", 0.6f);
            }
        }
    }

    void DoSlowmo()
    {
        timeManager.DoSlowmotion();
    }

    void StopSlowmo()
    {
        timeManager.StopSlowmotion();
    }

    private void FixedUpdate()
    {
        float zSpeed = Input.GetAxis("Vertical") * force;
        float xSpeed = Input.GetAxis("Horizontal") * force;
        rb.AddForce(new Vector3(xSpeed, 0, zSpeed));
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        if (jumpTimer > Time.time && (PublicVars.infinteJump || grounded))
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            jumpTimer = 0;
        }
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, transform.localScale.y / 2 + 0.1f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if ((other.gameObject.CompareTag("Ground1") && !isBlue) || (other.gameObject.CompareTag("Ground2") && isBlue))
        {
            isAlive = false;
        }
        if (other.gameObject.CompareTag("Ground2") && !PublicVars.movedToLastPlatform)
        {
            PublicVars.movedToLastPlatform = true;
        }
        if (other.relativeVelocity.y > 6 && !cf.shaking)
        {
            StartCoroutine(cf.ShakeCamera());
        }
    }
    private void OnCollisionStay(Collision other)
    {
        if ((other.gameObject.CompareTag("Ground1") && !isBlue) || (other.gameObject.CompareTag("Ground2") && isBlue))
        {
            isAlive = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Gem1"))
        {
            if (!PublicVars.infinteJump)
            {
                StartCoroutine(BlueGemFirstTime());
                PublicVars.infinteJump = true;
            }
        }

        if (other.gameObject.CompareTag("Gem2"))
        {
            ++PublicVars.checkPoint;
        }

        if (other.gameObject.CompareTag("Gem3"))
        {
            transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        }

        if (other.gameObject.CompareTag("Gem4"))
        {
            transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        }

        // if (other.gameObject.CompareTag("SlowmoPlane"))
        // {
        //     timeManager.DoSlowmotion();
        //     isBallSlowmo = true;
        // }

        if (other.gameObject.CompareTag("JumpPlane"))
        {
            jumped = true;
        }
    }

    IEnumerator WaitToMove()
    {
        yield return new WaitForSeconds(.2f);
    }

    public IEnumerator BlueGemFirstTime()
    {
        blueGemPicked.SetActive(true);
        yield return new WaitForSeconds(3f);
        blueGemPicked.SetActive(false);
    }

    IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            SaveLoad.Save();
        }
    }
}