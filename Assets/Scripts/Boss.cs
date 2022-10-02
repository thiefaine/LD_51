using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Boss : MonoBehaviour
{
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
    private int _indexTimeline;
    private float _currentCd = 0f;
    private EBoss _state = EBoss.Idle;
    private List<GameObject> _bullets = new List<GameObject>();
        
    // Start is called before the first frame update
    void Start()
    {
        _currentLife = maxLife;
        _currentCd = Random.Range(minCd, maxCd);
    }

    // Update is called once per frame
    void Update()
    {
        // squash - stretch
        Vector2 scaleSquash = new Vector2(curveScaleX.Evaluate(Time.time * scaleSpeed), curveScaleY.Evaluate(Time.time * scaleSpeed));
        sprite.transform.localScale = scaleSquash;
        
        // brain
        // TODO Cooldown, set state, etc...
        
        UpdateState();
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
            ChangeState(EBoss.Idle);
        }
        else if (_state == EBoss.Idle)
        {
            _targetDirection = Random.insideUnitCircle.normalized;
            _currentDirection = Random.insideUnitCircle.normalized;
            _durationChangeDirection = Random.Range(minDurationChangeDirection, maxDurationChangeDirection);
            _timerChangeDirection = _durationChangeDirection;
        }
        else if (_state == EBoss.Bulletts)
        {
            // TODO play anim attack
        }
        else if (_state == EBoss.Jump)
        {
            // TODO play anim jump
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
            // TODO chose bullet pattern 
        }
        else if (_state == EBoss.Jump)
        {
            
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
            _currentCd = Random.Range(minCd, maxCd);
        }
        else if (_state == EBoss.Jump)
        {
            _currentCd = Random.Range(minCd, maxCd);
        }
    }
    
    public void Damage(float damages)
    {
        _currentLife = Mathf.Max(_currentLife - damages, 0f);
        
        // TODO hit feedback
        
        if (_currentLife <= 0f)
        {
            // TODO dead !!
        }
    }
}
