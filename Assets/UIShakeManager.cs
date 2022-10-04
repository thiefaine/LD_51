using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShakeManager : MonoBehaviour
{
    public AnimationCurve curveEffect;
    public GameObject panel;
    public float forceFactor;
    
    private Vector3 _startLocalPos;

    private bool _impulse = false;
    private Vector3 _directionImpulse;
    private float _forceImpusle;
    private float _durationImpulse;
    private float _timerImpulse;
    
    private bool _shake = false;
    private float _forceShake;
    private float _durationShake;
    private float _timerShake;
    
    void Start()
    {
        _startLocalPos = panel.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Manage Impulse
        Vector3 offsetImpulse = Vector2.zero;
        if (_impulse)
        {
            _timerImpulse += Time.unscaledDeltaTime;
            float ratioImpulse = Mathf.Clamp01(_timerImpulse / _durationImpulse); 
            Vector2 directionImp = _directionImpulse.normalized * _forceImpusle;
            offsetImpulse = directionImp * curveEffect.Evaluate(ratioImpulse) * forceFactor;
            if (ratioImpulse >= 1f)
                _impulse = false;
        }

        // Manage Shake
        Vector3 offsetShake = Vector2.zero;
        if (_shake)
        {
            _timerShake += Time.unscaledDeltaTime;
            float ratioShake = Mathf.Clamp01(_timerShake / _durationShake);
            Vector2 directionSh = Random.insideUnitCircle.normalized * _forceShake;
            offsetShake = directionSh * curveEffect.Evaluate(ratioShake) * forceFactor;
            if (ratioShake >= 1f)
                _shake = false;
        }
        
        panel.transform.localPosition = _startLocalPos + offsetImpulse + offsetShake;
    }

    public void ApplyImpulse(float force, float duration, Vector2 direction)
    {
        if (force == 0f)
            return;

        _forceImpusle = force;
        _directionImpulse = direction;
        _durationImpulse = duration;
        _timerImpulse = 0f;
        _impulse = true;
    }

    public void ApplyShake(float force, float duration)
    {
        if (force == 0f)
            return;

        _forceShake = force;
        _durationShake = duration;
        _timerShake = 0f;
        _shake = true;
    }
}
