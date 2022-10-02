using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public enum EMoving
    {
        Free,
        Imposed,
        Dash,
    }

    [Header("Life")]
    public int maxLife;
    private int _currentLife;
    public int CurrentLife
    {
        get { return _currentLife; }
    }
    public float LifeRatio
    {
        get { return Mathf.Clamp01((float)_currentLife / (float)maxLife); }
    }

    [Header("Moving")]
    public Rigidbody2D rb;
    public float velocityFactor;
    public float velocityFactorCharging;
    public float dampingStartVelocity;
    public float dampingStopVelocity;
    
    private Vector2 _movementDirection;
    private Vector2 _cursorDirection;
    private Vector2 _imposedDirection;
    private bool _isMoving = false;
    private float _durationIsMoving = 0f;
    private EMoving _movingState = EMoving.Free;

    [Header("Dash")]
    public GameObject shadow;
    public AnimationCurve curveDashVelocity;
    public float dashVelocityFactor;
    public float durationDash;
    public float dashCoolDown;
    public AnimationCurve curvePosDash;
    public AnimationCurve curveRotDash;
    public float dashPosFactor;
    public float dashRotFactor;
    private float _timerDash = 0f;
    private float _timerDashCoolDown = 0f;
    private Vector2 _directionDash;
    private bool _dashReady = true;

    [Header("I-Frame")]
    public float durationIFrames;
    public float durationBlink;
    private bool _isInIFrames = false;
    private float _timerIFrames = 0f;
    private float _timerBlink = 0f;

    [Header("Animation")]
    public GameObject sprite;
    public Animator dustFx;
    public float durationDust;
    private float _timerDust = 0f;
    
    [Header("Stretch - squash")]
    public AnimationCurve curveScaleX;
    public AnimationCurve curveScaleY;
    public float scaleSpeed;

    [Header("Bumpy")]
    public AnimationCurve curvePosBumpy; 
    public float animBumpySpeed;
    [Space(10)]
    public float maxRotAngle;
    public float animRotSpeed;
    public ParticleSystem dustParticles;

    [Header("Bow")]
    public GameObject bowObject;
    public List<Sprite> bowSprites = new List<Sprite>();
    public Animator bowFx;
    public float bowDistanceToBody;
    public float speedCharging;
    
    private bool _isChargingBow = false;
    private float _chargingRatio = 0f;

    [Header("Bow UI")]
    public GameObject gauge;
    public GameObject gaugeLeftPart;
    public GameObject gaugeRightPart;
    public GameObject gaugeCenterPart;
    public Gradient gradientMaxCharge;
    public float speedGradientMaxCharge;
    public float maxOffsetGauge;

    [Header("Pickup Arrows")]
    public float radiusPickup;
    public LayerMask arrowMask;
    
    [Header("Arrow")]
    public GameObject arrowPrefab;
    public int maxAmmoArrows;
    
    private GameObject _currentArrow;
    public int currentAmmoArrows;
    public int CurrentAmmoArrows
    {
        get { return currentAmmoArrows; }
    }
    
    public bool IsChargingBow
    {
        get { return _isChargingBow; }
    }
    public float ChargingRatio
    {
        get { return _chargingRatio; }
    }

    // Start is called before the first frame update
    void Start()
    {
        _currentLife = maxLife;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // _cursorDirection = (mousePos - transform.position);
        // _cursorDirection.Normalize();
        
        // IFrames
        if (_isInIFrames)
        {
            _timerBlink -= Time.deltaTime;
            _timerIFrames -= Time.deltaTime;

            if (_timerBlink <= 0f)
            {
                _timerBlink = durationBlink;
                SpriteRenderer spr = sprite.GetComponent<SpriteRenderer>();
                spr.enabled = !spr.enabled;
            }

            if (_timerIFrames <= 0f)
            {
                sprite.GetComponent<SpriteRenderer>().enabled = true;
                _isInIFrames = false;
            }
        }
        
        // Dash
        _timerDashCoolDown -= Time.deltaTime;
        if (_timerDashCoolDown <= 0f && !_dashReady)
        {
            shadow.GetComponent<Animator>().SetTrigger("Trigger");
            _dashReady = true;
        }
        
        // Movement
        float factor = Mathf.Lerp(velocityFactor, velocityFactorCharging, _chargingRatio); 
        Vector2 targetMovementVeloc = Vector2.zero;
        switch (_movingState)
        {
            case EMoving.Free:
                float dampFactor = (_movementDirection == Vector2.zero) ? dampingStopVelocity : dampingStartVelocity;
                targetMovementVeloc = MathHelper.Damping(rb.velocity, _movementDirection * factor , Time.deltaTime, dampFactor);
                break;
            case EMoving.Dash:
                _timerDash -= Time.deltaTime;
                float dashRatio = Mathf.Clamp01(_timerDash / durationDash);
                float speed = curveDashVelocity.Evaluate(dashRatio) * dashVelocityFactor; 
                targetMovementVeloc = _directionDash * speed;
                if (_timerDash <= 0f)
                {
                    shadow.GetComponent<Animator>().SetBool("Dashing", false);
                    _movingState = EMoving.Free;
                }
                break;
            case EMoving.Imposed:
                targetMovementVeloc = _imposedDirection * velocityFactor;
                break;
        }
        rb.velocity = targetMovementVeloc;
        
        // Pickup arrows
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.layerMask = arrowMask;
        Collider2D[] results = new Collider2D[10];
        Physics2D.OverlapCircle(transform.position, radiusPickup, filter, results);
        foreach (Collider2D col in results)
        {
            if (col == null)
                continue;
            
            Arrow arrow = col.gameObject.GetComponent<Arrow>(); 
            if (arrow && arrow.IsPickupable)
            {
                currentAmmoArrows = Mathf.Min(currentAmmoArrows + 1, maxAmmoArrows);
                GameObject.DestroyImmediate(arrow.gameObject);
                // TODO : play pickup feedback
            }
        }
        
        // Bow
        float angle = Mathf.Atan2(_cursorDirection.y, _cursorDirection.x) * Mathf.Rad2Deg;
        bowObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Vector3 posBow = _cursorDirection * bowDistanceToBody;
        posBow.z = -0.1f;
        bowObject.transform.localPosition = posBow;

        int index = 0;
        if (_chargingRatio >= 1f || bowSprites.Count <= 1)
            index = bowSprites.Count - 1;
        else
            index = (int)Mathf.Lerp(0, bowSprites.Count - 1, _chargingRatio);
        bowObject.GetComponentInChildren<SpriteRenderer>().sprite = bowSprites[index];

        // Arrow
        if (_isChargingBow && _currentArrow != null)
        {
            _currentArrow.transform.position = bowObject.transform.position;
            _currentArrow.transform.rotation = bowObject.transform.rotation;
        }
        
        // Bow UI
        if (_isChargingBow)
        {
            _chargingRatio = Mathf.Clamp01(_chargingRatio + speedCharging * Time.deltaTime);
            gauge.SetActive(true);
            gaugeLeftPart.transform.localPosition = new Vector3(Mathf.Lerp(0f, -maxOffsetGauge, _chargingRatio), 0f, 0f);
            gaugeRightPart.transform.localPosition = new Vector3(Mathf.Lerp(0f, maxOffsetGauge, _chargingRatio), 0f, 0f);
            gaugeCenterPart.transform.localScale = new Vector3(Mathf.Lerp(0f, maxOffsetGauge * 2f, _chargingRatio), gaugeCenterPart.transform.localScale.y, 0f);

            if (_chargingRatio >= 1f)
            {
                float ratioCol = Mathf.PingPong(Time.time * speedGradientMaxCharge, 1f);
                Color col = gradientMaxCharge.Evaluate(ratioCol);
                SetGaugeUIColor(col);
            }
        }
        else
        {
            _chargingRatio = 0f;
            gauge.SetActive(false);
            SetGaugeUIColor(Color.white);
        }

        // Anim
        // sqash
        Vector2 scaleSquash = new Vector2(curveScaleX.Evaluate(Time.time * scaleSpeed), curveScaleY.Evaluate(Time.time * scaleSpeed));
        sprite.transform.localScale = scaleSquash;
        
        // bumpy
        if (_movingState == EMoving.Free)
        {
            if (_isMoving)
            {
                _durationIsMoving += Time.deltaTime;
                sprite.transform.localPosition =
                    new Vector2(0f, curvePosBumpy.Evaluate(_durationIsMoving * animBumpySpeed) - 0.25f);
                float rot = Mathf.PingPong(_durationIsMoving * animRotSpeed, maxRotAngle) - (maxRotAngle * 0.5f);
                sprite.transform.localRotation = Quaternion.Euler(0f, 0f, rot);

                _timerDust += Time.deltaTime;
                if (_timerDust >= durationDust)
                {
                    _timerDust = 0f;
                    dustFx.transform.SetParent(null);
                    dustFx.transform.position = transform.position;
                    dustFx.transform.rotation = quaternion.Euler(0f, 0f, Random.Range(-180f, 180f));
                    dustFx.transform.localScale = Vector3.one * Random.Range(0.7f, 2f);
                    dustFx.SetTrigger("Play");
                }
            }
            else
            {
                _durationIsMoving = Mathf.Max(_durationIsMoving - Time.deltaTime, 0f);
                sprite.transform.localPosition = MathHelper.Damping(sprite.transform.localPosition,
                    new Vector2(0f, -0.25f), Time.deltaTime, 0.25f);
                sprite.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        else if (_movingState == EMoving.Dash)
        {
            float ratioDash = Mathf.Clamp01(_timerDash / durationDash);
            sprite.transform.localPosition = new Vector3(0f, (curvePosDash.Evaluate(ratioDash) * dashPosFactor) - 0.25f, 0f);
            sprite.transform.RotateAround(Vector3.forward, curveRotDash.Evaluate(ratioDash) * dashRotFactor);
        }
    }

    private void SetGaugeUIColor(Color col)
    {
        gaugeLeftPart.GetComponent<SpriteRenderer>().color = col;
        gaugeRightPart.GetComponent<SpriteRenderer>().color = col;      
        gaugeCenterPart.GetComponent<SpriteRenderer>().color = col;
    }

    public void OnLook(InputValue value)
    {
        var val = value.Get<Vector2>();
        if (GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
        {
            if (val != Vector2.zero)
                _cursorDirection = val;
        }
        else
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(val);
            _cursorDirection = (mousePos - transform.position);
        }
        
        _cursorDirection.Normalize();
    }

    // 'Move' input action has been triggered.
    public void OnMove(InputValue value)
    {
        var dir = value.Get<Vector2>();
        _movementDirection = dir.normalized;

        if (dir == Vector2.zero)
            _isMoving = false;
        else
            _isMoving = true;
    }
    
    public void OnFire(InputValue value)
    {
        if (value.isPressed && currentAmmoArrows > 0)
        {
            Vector3 startPos = bowObject.transform.position;
            startPos.z = 0f;
            _currentArrow = GameObject.Instantiate(arrowPrefab, startPos, bowObject.transform.rotation);
            _isChargingBow = true;
        }
        else if (_isChargingBow)
        {
            _currentArrow.GetComponent<Arrow>().Shoot(_cursorDirection, _chargingRatio);
            currentAmmoArrows--;
            _currentArrow = null;
            _isChargingBow = false;
            
            bowFx.transform.SetParent(null);
            bowFx.transform.position = bowObject.transform.position;
            bowFx.transform.rotation = bowObject.transform.rotation;
            bowFx.SetTrigger("Shoot");

            float forceImpulse = Mathf.Lerp(0.03f, 0.18f, _chargingRatio);
            float durationImpulse = Mathf.Lerp(0.12f, 0.1f, _chargingRatio);
            Camera.main.GetComponent<CameraManager>().ApplyImpulse(forceImpulse, durationImpulse, -_cursorDirection);
            // Camera.main.GetComponent<CameraManager>().ApplyShake(0.1f, 0.1f);
        }
    }

    public void Damage()
    {
        if (_isInIFrames || _movingState == EMoving.Dash)
            return;

        _isInIFrames = true;
        _timerBlink = durationBlink;
        _timerIFrames = durationIFrames;
        
        // _currentLife = Mathf.Max(_currentLife - 1, 0);
    }

    public void OnDash()
    {
        if (_timerDashCoolDown > 0f)
            return;
            
        // nothing for the moment
        _movingState = EMoving.Dash;
        _timerDash = durationDash;
        _timerDashCoolDown = dashCoolDown;
        _dashReady = false;

        shadow.GetComponent<Animator>().SetBool("Dashing", true);
        _directionDash = _movementDirection;
        if (_directionDash == Vector2.zero)
            _directionDash = _cursorDirection;
        if (_directionDash == Vector2.zero)
            _directionDash = Vector2.right;
        _directionDash.Normalize();
    }
}
