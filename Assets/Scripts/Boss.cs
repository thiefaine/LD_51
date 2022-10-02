using System.Collections;
using System.Collections.Generic;
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

    public enum EBoss
    {
        Idle,
        Bulletts,
        Jump,
    }
    public EBoss State
    {
        get { return _state; }
        set
        {
            EndState();
            _state = value;
            StartState();
        }
    }

    [Header("Attack")]
    public GameObject bullet;
    private EBoss _state;
    private List<GameObject> _bullets = new List<GameObject>();
        
    // Start is called before the first frame update
    void Start()
    {
        _currentLife = maxLife;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO Cooldown, set state, etc...
        UpdateState();
    }

    private void StartState()
    {
        if (_state == EBoss.Idle)
        {
            
        } else if (_state == EBoss.Bulletts)
        {
            
        } else if (_state == EBoss.Jump)
        {
            
        }
    }

    private void UpdateState()
    {
        if (_state == EBoss.Idle)
        {
            
        } else if (_state == EBoss.Bulletts)
        {
            
        } else if (_state == EBoss.Jump)
        {
            
        }
    }

    private void EndState()
    {
        if (_state == EBoss.Idle)
        {
            
        } else if (_state == EBoss.Bulletts)
        {
            
        } else if (_state == EBoss.Jump)
        {
            
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
