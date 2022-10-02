using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Velocity")]
    public float maxVelocity;
    public float minVelocity;
    public AnimationCurve velocityFactorOverTime;
    public float durationVelocity;
    public float velocityThreshold;
    
    [Header("Damages")]
    public float minDamage;
    public float maxDamage;

    private float _durationArrow;
    private Vector2 _currentVelocity;
    private Vector2 _currentDirection;
    private float _ratioForce;
    
    private bool _isLaunched = false;
    private bool _isOnGround = false;
    private bool _isPickupable = false;
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

        float ratioDuration = Mathf.Clamp01(_durationArrow / durationVelocity);
        _currentVelocity = GetVelocityWithRatio(ratioDuration);
        float velocityMagnitude = _currentVelocity.magnitude;
        
        if (_isLaunched && velocityMagnitude <= velocityThreshold)
            _isPickupable = true;
        
        if (_isLaunched && _currentVelocity.magnitude <= 0.001f)
            _isOnGround = true;

        Vector3 nextPos = transform.position + new Vector3(_currentVelocity.x, _currentVelocity.y, 0f);
        transform.position = nextPos;
        
        // TODO DEBUG : velocity jittery 
        // TODO : eject if grounded under Boss
    }

    public void Shoot(Vector2 direction, float ratioForce)
    {
        _isOnGround = false;
        _isLaunched = true;
        _isPickupable = false;
        trail.enabled = true;
        _durationArrow = 0f;
        _currentDirection = direction;
        _ratioForce = ratioForce;
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        _insideColliders.Remove(other);
    }

    public void OnTriggerStay2D(Collider2D col)
    {
        if (!_isLaunched)
            return;

        if (_insideColliders.Contains(col))
            return;
        
        _insideColliders.Add(col);
        
        // Boss case
        Boss boss = col.GetComponent<Boss>();
        if (boss && boss.LifeRatio > 0f)
        {
            // TODO small slo mo ??

            float damages = Mathf.Lerp(minDamage, maxDamage, _ratioForce);
            if (_currentVelocity.magnitude <= velocityThreshold)
                damages = 0f;
            
            boss.Damage(damages);

            if (damages > 0f)
            {
                float forceShake = Mathf.Lerp(0.02f, 0.1f, _ratioForce);
                float durationShake = Mathf.Lerp(0.05f, 0.1f, _ratioForce);
                Camera.main.GetComponent<CameraManager>().ApplyShake(forceShake, durationShake);
            }
        }
        
        // Walls case
        Wall wall = col.GetComponent<Wall>();
        if (wall)
        {
            _currentDirection = Vector2.Reflect(_currentDirection, wall.normal);
            float angle = Mathf.Atan2(_currentDirection.y, _currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            // TODO orientation
        }
    }

    private Vector2 GetVelocityWithRatio(float durationRatio)
    {
        float veloc = velocityFactorOverTime.Evaluate(durationRatio) * Mathf.Lerp(minVelocity, maxVelocity, _ratioForce);
        return _isOnGround ? Vector2.zero : _currentDirection.normalized * veloc * Time.deltaTime;
    }
}
