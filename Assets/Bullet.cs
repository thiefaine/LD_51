using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Animator animator;
    public TrailRenderer trail;
    
    [Header("Velocity")]
    public AnimationCurve velocityFactorOverTime;
    public float durationVelocity;
    
    private float _durationArrow;
    private float _speed;
    private Vector2 _currentVelocity;
    private Vector2 _currentDirection;
    
    private List<Collider2D> _insideColliders = new List<Collider2D>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        _durationArrow += Time.deltaTime;
        float ratioDuration = Mathf.Clamp01(_durationArrow / durationVelocity);
        _currentVelocity = GetVelocityWithRatio(ratioDuration);
        
        Vector3 nextPos = transform.position + new Vector3(_currentVelocity.x, _currentVelocity.y, 0f);
        transform.position = nextPos;
    }
    
    private Vector2 GetVelocityWithRatio(float durationRatio)
    {
        float veloc = velocityFactorOverTime.Evaluate(durationRatio) * _speed;
        return _currentDirection.normalized * veloc * Time.deltaTime;
    }
    
    public void Shoot(Vector2 direction, float speed)
    {
        _durationArrow = 0f;
        _currentDirection = direction;
        _speed = speed;
    }
    
    public void OnTriggerExit2D(Collider2D other)
    {
        _insideColliders.Remove(other);
    }
    
    public void OnTriggerStay2D(Collider2D col)
    {
        if (_insideColliders.Contains(col))
            return;
        
        _insideColliders.Add(col);
        
        // Boss case
        PlayerController player = col.GetComponent<PlayerController>();
        if (player && player.LifeRatio > 0f)
        {
            player.Damage();
            Camera.main.GetComponent<CameraManager>().ApplyShake(0.01f, 0.08f);
        }
        
        // Walls case
        Wall wall = col.GetComponent<Wall>();
        if (wall)
            StartCoroutine(HitWall());
    }

    private IEnumerator HitWall()
    {
        _speed = 0f;
        animator.SetTrigger("HitWall");

        yield return new WaitForSeconds(3f);
        
        GameObject.Destroy(gameObject);
    }
}
