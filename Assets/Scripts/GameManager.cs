using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool GameEnded = false;
    
    public GameObject cursor;
    public SpriteRenderer spriteFade;
    public GameObject textPart1;
    public GameObject textPart2;
    public GameObject victory;
    public ShopManager shop;
    public float durationTextBeforeCharge;
    public float durationChargeBeforeShop;

    private PlayerInput _playerInput;
    private PlayerController _player;
    private Boss _boss;
    private bool _isEnding = false;
    
    private void Awake()
    {
        Cursor.visible = false;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        PlayerController.HasDash = false;
        PlayerController.HasJump = false;
        PlayerController.HasAutoHit = false;
        PlayerController.ExtraMovementSpeedFactor = 0f;

        Boss.DowngradeSpeedFactor = 0f;
        Boss.HasAutoHit = false;

        GameEnded = false; 
        
        _playerInput = FindObjectOfType<PlayerInput>();
        _player = FindObjectOfType<PlayerController>();
        _boss = FindObjectOfType<Boss>();
        
        _player.IsLock = true;
        _boss.IsLock = true;
        
        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerInput.currentControlScheme == "Gamepad")
        {
            cursor.SetActive(false);
        }
        else
        {
            cursor.SetActive(true);
        }

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 1f;
        cursor.transform.position = pos;

        if (!_isEnding && (_player.LifeRatio <= 0f || _boss.LifeRatio <= 0f))
            StartCoroutine(EndGame());
    }

    private IEnumerator StartGame()
    {
        textPart1.gameObject.SetActive(false);
        
        yield return new WaitForSecondsRealtime(0.75f);
        
        textPart1.gameObject.SetActive(true);
        
        yield return new WaitForSecondsRealtime(durationTextBeforeCharge);
        
        _boss.GetComponentInChildren<Animator>().SetBool("Charging", true);
        
        yield return new WaitForSecondsRealtime(durationChargeBeforeShop);
        
        Time.timeScale = 0f;
        textPart1.gameObject.SetActive(false);
        textPart2.gameObject.SetActive(true);
        shop.gameObject.SetActive(true);
    }

    private IEnumerator EndGame()
    {
        _isEnding = true;
        float duration = 2f;
        float timer = duration;

        GameEnded = true;
        _player.IsLock = true;
        _boss.IsLock = true;

        if (_player.LifeRatio > 0f)
            victory.SetActive(true);

        float durationWait = _player.LifeRatio > 0f ? 4f : 0.5f;
        yield return new WaitForSecondsRealtime(durationWait); // 1.5f
        
        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            
            float ratio = 1f - Mathf.Clamp01(timer / duration);
            Time.timeScale = Mathf.Lerp(1f, 0f, ratio * 2f);
            Color col = spriteFade.color;
            col.a = Mathf.Lerp(0f, 1f, ratio);
            spriteFade.color = col;
            yield return new WaitForEndOfFrame();
        }

        spriteFade.color = Color.black;
        yield return new WaitForSecondsRealtime(0.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
