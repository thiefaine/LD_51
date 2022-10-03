using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Collections;
using Mono.Cecil.Cil;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class PlayerController : MonoBehaviour
{
    // UPGRADES
    [Header("Upgrade")]
    public static bool HasAutoHit = false;
    private float _timerAutoHit = 10f;
    
    public static float ExtraMovementSpeedFactor = 0f;
    public static bool HasDash = false;
    public static bool HasJump = false;
    // UPGRADES
        
    public bool IsLock;
    
    public enum EMoving
    {
        Free,
        Imposed,
        Ability,
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
    private bool _isDead = false;
    private bool _isVictorious = false;

    [Header("Moving")]
    public Rigidbody2D rb;
    public float velocityFactor;
    public float velocityFactorCharging;
    public float dampingStartVelocity;
    public float dampingStopVelocity;
    
    private Vector2 _movementDirection;
    private Vector2 _cursorDirection = Vector2.right;
    private Vector2 _imposedDirection;
    private bool _isMoving = false;
    private float _durationIsMoving = 0f;
    private EMoving _movingState = EMoving.Free;

    [Header("Ability")]
    public GameObject shadow;
    [FormerlySerializedAs("dashCoolDown")] public float abilityCoolDown;
    private float _timerAbilityCoolDown = 0f;
    private Vector2 _directionAbility;
    private bool _abilityReady = true;
    
    [Header("Jump")]
    public AnimationCurve curvePosDash;
    public AnimationCurve curveRotDash;
    public float dashPosFactor;
    public float dashRotFactor;
    public float durationJump;
    private float _timerJump = 0f;
    private bool _isJumping = false;

    [Header("Dash")]
    public TrailRenderer trail;
    public AnimationCurve curveDashVelocity;
    public float dashVelocityFactor;
    public float durationDash;
    private float _timerDash = 0f;

    [Header("I-Frame")]
    public Color colorHitIframes;
    public float durationIFrames;
    public float durationIFramesEffect;
    public float durationBlink;
    private bool _isInIFrames = false;
    private float _timerIFrames = 0f;
    private float _timerIFramesEffect = 0f;
    private float _timerBlink = 0f;

    [Header("Animation")]
    public SpriteRenderer sprite;
    public Animator dustFx;
    public Animator spriteAnimator;
    public Animator shadowAnimator;
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
        spriteAnimator.enabled = false;
        _currentLife = maxLife;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDead)
        {
            IsLock = true;
            rb.velocity = Vector2.zero;
            sprite.color = Color.red;
            return;
        }
        
        // Anim
        // sqash
        Vector2 scaleSquash = new Vector2(curveScaleX.Evaluate(Time.time * scaleSpeed), curveScaleY.Evaluate(Time.time * scaleSpeed));
        sprite.transform.localScale = scaleSquash;

        float targetTrail = _timerDash > 0f ? 0.5f : 0f;
        trail.time = MathHelper.Damping(trail.time, targetTrail, Time.deltaTime, 0.1f);

        if (GameManager.GameEnded)
        {
            IsLock = true;
            rb.velocity = Vector2.zero;
            if (!_isVictorious)
                SetVictory();
            return;
        }
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // _cursorDirection = (mousePos - transform.position);
        // _cursorDirection.Normalize();
        
        if (HasAutoHit && !IsLock)
        {
            _timerAutoHit -= Time.deltaTime;
            if (_timerAutoHit <= 0f)
            {
                _timerAutoHit = 10f;
                
                Boss boss = FindObjectOfType<Boss>();
                Vector2 dirToBoss = boss.transform.position - transform.position;
                Vector3 startPos = bowObject.transform.position;
                startPos.z = 0f;
                float a = Mathf.Atan2(dirToBoss.y, dirToBoss.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.AngleAxis(a, Vector3.forward);
                GameObject arrow = GameObject.Instantiate(arrowPrefab, startPos, rot);
                float chargeRatio = 0.75f;
                arrow.GetComponent<Arrow>().Shoot(dirToBoss, chargeRatio, false);
            
                bowFx.transform.SetParent(null);
                bowFx.transform.position = bowObject.transform.position;
                bowFx.transform.rotation = bowObject.transform.rotation;
                bowFx.SetTrigger("Shoot");

                float forceImpulse = Mathf.Lerp(0.03f, 0.1f, chargeRatio);
                float durationImpulse = Mathf.Lerp(0.05f, 0.08f, chargeRatio);
                Camera.main.GetComponent<CameraManager>().ApplyImpulse(forceImpulse, durationImpulse, -_cursorDirection);
            }
        }

        // IFrames
        if (_isInIFrames)
        {
            _timerBlink -= Time.deltaTime;
            _timerIFrames -= Time.deltaTime;
            _timerIFramesEffect -= Time.deltaTime;

            if (_timerIFramesEffect > 0f)
            {
                if (_timerBlink <= 0f)
                {
                    _timerBlink = durationBlink;
                    sprite.enabled = !sprite.enabled;
                }
            }
            else
            {
                sprite.enabled = true;
            }
            
            if (_timerIFrames <= 0f)
                _isInIFrames = false;

            float ratio = 1f - Mathf.Clamp01(_timerIFramesEffect / durationIFramesEffect);
            sprite.color = Color.Lerp(colorHitIframes, Color.white, ratio);
        }
        
        // Dash
        _timerAbilityCoolDown -= Time.deltaTime;
        if (_timerAbilityCoolDown <= 0f && !_abilityReady)
        {
            shadowAnimator.SetTrigger("Trigger");
            _abilityReady = true;
        }
        
        // Movement
        Vector2 targetMovementVeloc = Vector2.zero;
        
        float speedFactor = Mathf.Lerp(velocityFactor, velocityFactorCharging, _chargingRatio);
        speedFactor += speedFactor * ExtraMovementSpeedFactor;
        float dampFactor = (_movementDirection == Vector2.zero) ? dampingStopVelocity : dampingStartVelocity;
        Vector3 defaultVeloc = MathHelper.Damping(rb.velocity, _movementDirection * speedFactor , Time.deltaTime, dampFactor);
        
        switch (_movingState)
        {
            case EMoving.Free:
                targetMovementVeloc = defaultVeloc;
                break;
            case EMoving.Ability:
                _timerJump -= Time.deltaTime;
                _timerDash -= Time.deltaTime;
                
                if (HasDash && _timerDash > 0f)
                {
                    float dashRatio = 1f - Mathf.Clamp01(_timerDash / durationDash);
                    float speed = curveDashVelocity.Evaluate(dashRatio) * dashVelocityFactor;
                    targetMovementVeloc = _directionAbility * speed;
                } else 
                    targetMovementVeloc = defaultVeloc;
                
                if (_timerJump <= 0f)
                {
                    shadowAnimator.SetBool("Jumping", false);
                    _isJumping = false;
                }
                if (_timerDash <= 0f && _timerJump <= 0f)
                    _movingState = EMoving.Free;
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
        else if (_isJumping)
        {
            float ratioDash = Mathf.Clamp01(_timerJump / durationJump);
            sprite.transform.localPosition = new Vector3(0f, (curvePosDash.Evaluate(ratioDash) * dashPosFactor) - 0.25f, 0f);
            sprite.transform.RotateAround(Vector3.forward, curveRotDash.Evaluate(ratioDash) * dashRotFactor);
        }
    }

    private void SetVictory()
    {
        _isVictorious = true;
        spriteAnimator.enabled = true;
        spriteAnimator.SetTrigger("Victory");
    }

    public void IncreaseLife(int value)
    {
        maxLife += value;
        _currentLife += value;
    }

    public void IncreaseAmmo(int number)
    {
        maxAmmoArrows += number;
        currentAmmoArrows += number;
    }

    private void SetGaugeUIColor(Color col)
    {
        gaugeLeftPart.GetComponent<SpriteRenderer>().color = col;
        gaugeRightPart.GetComponent<SpriteRenderer>().color = col;      
        gaugeCenterPart.GetComponent<SpriteRenderer>().color = col;
    }

    public void OnLook(InputValue value)
    {
        if (IsLock)
            return;
        
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
        if (IsLock)
            return;
        
        var dir = value.Get<Vector2>();
        _movementDirection = dir.normalized;

        if (dir == Vector2.zero)
            _isMoving = false;
        else
            _isMoving = true;
    }
    
    public void OnFire(InputValue value)
    {
        if (IsLock)
            return;
        
        if (value.isPressed && currentAmmoArrows > 0)
        {
            Vector3 startPos = bowObject.transform.position;
            startPos.z = 0f;
            _currentArrow = GameObject.Instantiate(arrowPrefab, startPos, bowObject.transform.rotation);
            _isChargingBow = true;
        }
        else if (_isChargingBow)
        {
            _currentArrow.GetComponent<Arrow>().Shoot(_cursorDirection, _chargingRatio, true);
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
        if (IsLock)
            return;
        
        if (_isInIFrames || _isJumping || _isDead)
            return;

        _isInIFrames = true;
        _timerBlink = durationBlink;
        _timerIFrames = durationIFrames;
        _timerIFramesEffect = durationIFramesEffect;
        
        _currentLife = Mathf.Max(_currentLife - 1, 0);
        Camera.main.GetComponent<CameraManager>().ApplyShake(0.08f, 0.12f);
        StartCoroutine(FreezeFrame(0.15f));

        if (_currentLife <= 0f)
            SetDeath();
    }
    
    public void SetDeath()
    {
        if (_isDead)
            return;

        spriteAnimator.SetTrigger("Death");
        _isDead = true;
        IsLock = true;
    }

    public void OnDash()
    {
        if (IsLock)
            return;
        
        if ((!HasJump && !HasDash) || _timerAbilityCoolDown > 0)
            return;
            
        // nothing for the moment
        _movingState = EMoving.Ability;
        _timerAbilityCoolDown = abilityCoolDown;
        _abilityReady = false;

        if (HasDash)
            _timerDash = durationDash;
        if (HasJump)
        {
            _timerJump = durationJump;
            shadowAnimator.SetBool("Jumping", true);
            _isJumping = true;
        }

        if (HasDash)
        {
            if (GetComponent<PlayerInput>().currentControlScheme == "Gamepad")
                _directionAbility = _movementDirection;
            else
            {
                if (_movementDirection != Vector2.zero)
                    _directionAbility = _movementDirection;
                else
                    _directionAbility = _cursorDirection;
            }
            
            if (_directionAbility == Vector2.zero)
                _directionAbility = Vector2.right;
            _directionAbility.Normalize();
        }
    }
    
    private IEnumerator FreezeFrame(float duration)
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
    }
}
