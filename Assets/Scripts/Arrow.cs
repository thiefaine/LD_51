using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Arrow : MonoBehaviour
{
    // UPGRADES
    public static float ExtraDurationVelocity = 0f;
    public static float ExtraDamageFactor = 0f;
    // UPGRADES

    private bool _stayOnGround = true;
    
    [Header("Velocity")]
    public float maxVelocity;
    public float minVelocity;
    public AnimationCurve velocityFactorOverTime;
    public float durationVelocity;
    public float velocityThreshold;
    
    [Header("Damages")]
    public float minDamage;
    public float maxDamage;

    [Header("Animation")]
    public Animator animator;

    private float _durationArrow;
    private Vector2 _currentVelocity;
    private Vector2 _currentDirection;
    private float _ratioForce;
    
    private bool _isLaunched = false;
    private bool _isOnGround = false;
    private bool _isPickupable = false;
    private bool _isFreezing = false;
    private bool _isUnderBoss = false;
    private Boss _boss;
    public bool IsOnGround { get { return _isOnGround; } }
    public bool IsPickupable { get { return _isPickupable; } }
    
    private List<Collider2D> _insideColliders = new List<Collider2D>();

    [Header("Visual")]
    public TrailRenderer trail;

    private void Awake()
    {
        trail.enabled = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _durationArrow += Time.deltaTime;

        float totalDuration = durationVelocity + ExtraDurationVelocity;
        float ratioDuration = Mathf.Clamp01(_durationArrow / totalDuration);
        _currentVelocity = GetVelocityWithRatio(ratioDuration);
        float velocityMagnitude = _currentVelocity.magnitude;
        
        if (_isLaunched && velocityMagnitude <= velocityThreshold)
            _isPickupable = true;

        if (_isLaunched && !_isFreezing && _currentVelocity.magnitude <= 0.001f)
        {
            if (!_stayOnGround)
                GameObject.DestroyImmediate(this.gameObject);
            _isOnGround = true;
        }

        if (_isUnderBoss && _isOnGround)
            _currentVelocity = (transform.position - _boss.transform.position).normalized * minVelocity * Time.deltaTime;
        
        Vector3 nextPos = transform.position + new Vector3(_currentVelocity.x, _currentVelocity.y, 0f);
        transform.position = nextPos;
    }

    public void Shoot(Vector2 direction, float ratioForce, bool stayOnGround)
    {
        _isOnGround = false;
        _isLaunched = true;
        _isPickupable = false;
        trail.enabled = true;
        _stayOnGround = stayOnGround;
        _durationArrow = 0f;
        _currentDirection = direction;
        _ratioForce = ratioForce;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        Boss boss = other.GetComponent<Boss>();
        if (boss)
        {
            _isUnderBoss = false;
            _boss = null;
        }
        
        _insideColliders.Remove(other);
    }

    public void OnTriggerStay2D(Collider2D col)
    {
        if (!_isLaunched || _insideColliders.Contains(col))
            return;

        _insideColliders.Add(col);
        
        // Boss case
        Boss boss = col.GetComponent<Boss>();
        if (boss)
        {
            _isUnderBoss = true;
            _boss = boss;
            
            if (!_isPickupable)
            {
                StartCoroutine(FreezeFrame(0.15f));

                float damages = Mathf.Lerp(minDamage, maxDamage, _ratioForce);
                damages += damages * ExtraDamageFactor;
                
                if (_currentVelocity.magnitude <= velocityThreshold)
                    damages = 0f;
                
                boss.Damage(damages);
                
                animator.transform.SetParent(null, true);
                animator.transform.rotation = quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                animator.SetTrigger("Play");

                if (damages > 0f)
                {
                    float forceShake = Mathf.Lerp(0.02f, 0.1f, _ratioForce);
                    float durationShake = Mathf.Lerp(0.05f, 0.1f, _ratioForce);
                    Camera.main.GetComponent<CameraManager>().ApplyShake(forceShake, durationShake);
                }
            }
        }
        
        // Walls case
        Wall wall = col.GetComponent<Wall>();
        if (wall)
        {
            _currentDirection = Vector2.Reflect(_currentDirection, wall.normal);
            float angle = Mathf.Atan2(_currentDirection.y, _currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private Vector2 GetVelocityWithRatio(float durationRatio)
    {
        float veloc = velocityFactorOverTime.Evaluate(durationRatio) * Mathf.Lerp(minVelocity, maxVelocity, _ratioForce);
        return _isOnGround ? Vector2.zero : _currentDirection.normalized * veloc * Time.deltaTime;
    }

    private IEnumerator FreezeFrame(float duration)
    {
        _isFreezing = true;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        _isFreezing = false;
    }
}
