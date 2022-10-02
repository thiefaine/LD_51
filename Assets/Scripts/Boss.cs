using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Boss : MonoBehaviour
{
    public bool IsLock;

    [Header("Upgrade")]
    public static float DowngradeSpeedFactor = 0f;
    public static bool HasAutoHit = false;
    private float _timerAutoHit = 10f;
    
    [Header("Life")]
    public float maxLife;
    private float _currentLife;
    public float CurrentLife
    {
        get { return _currentLife; }
    }
    public float LifeRatio
    {
        get { return Mathf.Clamp01(_currentLife / maxLife); }
    }
    
    [Header("Animation")]
    public GameObject sprite;
    
    [Header("Stretch - squash")]
    public AnimationCurve curveScaleX;
    public AnimationCurve curveScaleY;
    public float scaleSpeed;

    [Header("Movement")]
    public Rigidbody2D rb;
    public LayerMask wallMask;
    public float distanceCast;
    public float minSpeed;
    public float maxSpeed;
    public float minDurationChangeDirection;
    public float maxDurationChangeDirection;
    private float _currentSpeed;
    private Vector2 _targetDirection;
    private Vector2 _currentDirection;
    private float _timerChangeDirection;
    private float _durationChangeDirection;
    

    public enum EBoss
    {
        Intro,
        Idle,
        Bulletts,
        Jump,
    }

    [Header("Attack")]
    public GameObject bullet;
    public List<EBoss> timeline = new List<EBoss>();
    public float minCd;
    public float maxCd;
    public AnimationCurve curveZigZag;
    private int _indexTimeline;
    private float _currentCd = 0f;
    private EBoss _state = EBoss.Idle;
    private List<GameObject> _bullets = new List<GameObject>();
    private bool _bulletsRunning = false;
    private bool _jumpRunning = false;
    private PlayerInput _playerInput;
        
    // Start is called before the first frame update
    void Start()
    {
        _currentLife = maxLife;
        _currentCd = Random.Range(minCd, maxCd);
        _playerInput = FindObjectOfType<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentLife <= 0f)
        {
            GetComponentInChildren<Animator>().SetTrigger("Death");
            IsLock = true;
            return;
        }
        
        // squash - stretch
        Vector2 scaleSquash = new Vector2(curveScaleX.Evaluate(Time.time * scaleSpeed), curveScaleY.Evaluate(Time.time * scaleSpeed));
        sprite.transform.localScale = scaleSquash;
        
        // brain
        if (IsLock)
            return;
        
        if (HasAutoHit)
        {
            _timerAutoHit -= Time.deltaTime;
            if (_timerAutoHit <= 0f)
            {
                _timerAutoHit = 10f;
                Damage(maxLife * 0.05f);
            }
        }
        
        UpdateState();
    }

    public void DowngradeBossLife(float factor)
    {
        maxLife = maxLife - maxLife * factor;
        _currentLife = _currentLife - _currentLife * factor;
    }

    private void ChangeState(EBoss newState)
    {
        EndState();
        _state = newState;
        StartState();
    }

    private void StartState()
    {
        if (_state == EBoss.Intro)
        {
        }
        else if (_state == EBoss.Idle)
        {
            _currentCd = Random.Range(minCd, maxCd);
            
            _targetDirection = Random.insideUnitCircle.normalized;
            _currentDirection = Random.insideUnitCircle.normalized;
            _durationChangeDirection = Random.Range(minDurationChangeDirection, maxDurationChangeDirection);
            _timerChangeDirection = _durationChangeDirection;
        }
        else if (_state == EBoss.Bulletts)
        {
            // TODO play anim attack
            
            int rnd = Random.Range(1, 5);
            switch (rnd)
            {
                case 1:
                    StartCoroutine(BulletPatternCircle());
                    break;
                case 2:
                    StartCoroutine(BulletPatternZigZag());
                    break;
                case 3:
                    StartCoroutine(BulletPatternRandom());
                    break;
                case 4:
                    StartCoroutine(BulletPatternSnipe());
                    break;
            }
        }
        else if (_state == EBoss.Jump)
        {
            // TODO play anim jump

            StartCoroutine(JumpAttack());
        }
    }

    private void UpdateState()
    {
        if (_state == EBoss.Intro)
        {
        }
        else if (_state == EBoss.Idle)
        {
            _currentCd -= Time.deltaTime;
            if (_currentCd <= 0f)
            {
                _indexTimeline++;
                if (_indexTimeline >= timeline.Count)
                    _indexTimeline -= timeline.Count;
                
                ChangeState(timeline[_indexTimeline]);
            }
            
            _timerChangeDirection += Time.deltaTime;
            float ratioDir = Mathf.Clamp01(_timerChangeDirection / _durationChangeDirection);
            Vector3 dir = Vector3.Lerp(_currentDirection, _targetDirection, ratioDir);
            dir.Normalize();
            
            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = wallMask;
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            Physics2D.Raycast(transform.position, _targetDirection, filter, hits, distanceCast);
            foreach (var hit in hits)
            {
                Wall wall = hit.collider.GetComponent<Wall>();
                if (wall)
                {
                    _currentDirection = _targetDirection;
                    _targetDirection = Vector2.Reflect(_targetDirection, wall.normal);
                }
            }
            
            if (_timerChangeDirection >= _durationChangeDirection)
            {
                _timerChangeDirection = 0f;
                _currentDirection = _targetDirection;
                _targetDirection = Random.insideUnitCircle.normalized;
                _durationChangeDirection = Random.Range(minDurationChangeDirection, maxDurationChangeDirection);
                _currentSpeed = Random.Range(minSpeed, maxSpeed);
            }
            
            // rb.velocity = dir;
            transform.position = transform.position + dir * _currentSpeed * Time.deltaTime;

            // TODO animate eye around
        }
        else if (_state == EBoss.Bulletts)
        {
            if (!_bulletsRunning)
                ChangeState(EBoss.Idle);
        }
        else if (_state == EBoss.Jump)
        {
            if (!_jumpRunning)
                ChangeState(EBoss.Idle);
        }
    }

    private void EndState()
    {

        if (_state == EBoss.Intro)
        {
        }
        else if (_state == EBoss.Idle)
        {
        }
        else if (_state == EBoss.Bulletts)
        {
        }
        else if (_state == EBoss.Jump)
        {
        }
    }

    private IEnumerator BulletPatternCircle()
    {
        // Classic circle around
        _bulletsRunning = true;
        GetComponentInChildren<Animator>().SetBool("Attack", true);
        
        yield return new WaitForSeconds(1f);
        
        float max = 14f;
        float offsetAngle = (360f / max);
        
        for (int j = 0; j < 7; j++)
        {
            for (int i = 0; i < max; i++)
            {
                float angle = i * offsetAngle + j * offsetAngle * 0.33f;
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
                GameObject bul = GameObject.Instantiate(bullet, transform.position, Quaternion.identity);
                float speed = 1.5f;
                speed -= speed * DowngradeSpeedFactor;
                bul.GetComponent<Bullet>().Shoot(dir, speed);
            }
            yield return new WaitForSeconds(0.5f);
        }
        
        yield return new WaitForEndOfFrame();
        _bulletsRunning = false;
        GetComponentInChildren<Animator>().SetBool("Attack", false);
    }
    
    private IEnumerator BulletPatternZigZag()
    {
        // zig zag cone toward player
        _bulletsRunning = true;
        GetComponentInChildren<Animator>().SetBool("Attack", true);
        
        yield return new WaitForSeconds(1f);

        float maxAngle = 35f;
        Vector2 baseDir = (_playerInput.transform.position - transform.position).normalized;
        Vector2 topDir = Quaternion.AngleAxis(maxAngle, Vector3.forward) * baseDir;
        Vector2 bottomDir = Quaternion.AngleAxis(-maxAngle, Vector3.forward) * baseDir;
        
        int maxJ = 30;
        for (int j = 0; j < maxJ; j++)
        {
            float r = curveZigZag.Evaluate(Mathf.Clamp01((float)j / (float)maxJ));
            Vector2 dir = Vector2.Lerp(topDir, bottomDir, r);
            GameObject bul = GameObject.Instantiate(bullet, transform.position, Quaternion.identity);
            float speed = 2.5f;
            speed -= speed * DowngradeSpeedFactor;
            bul.GetComponent<Bullet>().Shoot(dir, speed);
            yield return new WaitForSeconds(0.08f);
        }
        
        yield return new WaitForEndOfFrame();
        _bulletsRunning = false;
        GetComponentInChildren<Animator>().SetBool("Attack", false);
    }
    
    private IEnumerator BulletPatternRandom()
    {
        // random spray
        _bulletsRunning = true;
        GetComponentInChildren<Animator>().SetBool("Attack", true);
        
        yield return new WaitForSeconds(1f);
        
        for (int j = 0; j < 60; j++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 dir = Random.insideUnitCircle.normalized;
            GameObject bul = GameObject.Instantiate(bullet, transform.position, Quaternion.identity);
            float speed = 4f;
            speed -= speed * DowngradeSpeedFactor;
            bul.GetComponent<Bullet>().Shoot(dir, speed);
            yield return new WaitForSeconds(0.04f);
        }
        
        yield return new WaitForEndOfFrame();
        _bulletsRunning = false;
        GetComponentInChildren<Animator>().SetBool("Attack", false);
    }
    
    private IEnumerator BulletPatternSnipe()
    {
        // snipe player
        _bulletsRunning = true;
        GetComponentInChildren<Animator>().SetBool("Attack", true);
        
        yield return new WaitForSeconds(1f);
        Rigidbody2D playerRb = _playerInput.GetComponent<Rigidbody2D>();
            
        for (int j = 0; j < 3; j++)
        {
            Vector3 nextPosPlayer = _playerInput.transform.position + new Vector3(playerRb.velocity.x, playerRb.velocity.y, 0f) * 0.5f;
            Vector2 dir = (nextPosPlayer - transform.position).normalized;
            for (int i = 0; i < 3; i++)
            {
                GameObject bul = GameObject.Instantiate(bullet, transform.position, Quaternion.identity);
                float speed = 5.5f;
                speed -= speed * DowngradeSpeedFactor;
                bul.GetComponent<Bullet>().Shoot(dir, speed);
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(0.9f);
        }
        
        yield return new WaitForEndOfFrame();
        _bulletsRunning = false;
        GetComponentInChildren<Animator>().SetBool("Attack", false);
    }

    private IEnumerator JumpAttack()
    {
        _jumpRunning = true;
        
        yield return new WaitForSeconds(1f);
        
        
        
        yield return new WaitForEndOfFrame();
        _jumpRunning = false;
    }
    
    public void Damage(float damages)
    {
        if (_currentLife > 0f)
        {
            _currentLife = Mathf.Max(_currentLife - damages, 0f);
            
            if (_currentLife <= 0f)
            {
                // TODO dead !!
            }
        }
    }
}
