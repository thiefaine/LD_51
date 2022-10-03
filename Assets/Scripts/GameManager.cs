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
    public GameObject cursor;
    public SpriteRenderer spriteFade;
    public GameObject textPart1;
    public GameObject textPart2;
    public ShopManager shop;
    public float durationTextBeforeCharge;
    public float durationChargeBeforeShop;

    private PlayerInput _playerInput;
    private PlayerController _player;
    private Boss _boss;
    
    private void Awake()
    {
        Cursor.visible = false;
    }
    
    // Start is called before the first frame update
    void Start()
    {
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

        if (_player.LifeRatio <= 0f || _boss.LifeRatio <= 0f)
        {
            StartCoroutine(EndGame());
        }
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
        float duration = 3f;
        float timer = duration;

        _player.IsLock = true;
        _boss.IsLock = true;

        yield return new WaitForSecondsRealtime(1.5f);
        
        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            
            float ratio = 1f - Mathf.Clamp01(timer / duration);
            Time.timeScale = Mathf.Lerp(1f, 0f, ratio);
            Color col = spriteFade.color;
            col.a = Mathf.Lerp(1f, 0f, ratio);
            Debug.Log(ratio);
            spriteFade.color = col;
            yield return new WaitForEndOfFrame();
        }

        spriteFade.color = Color.black;
        yield return new WaitForSecondsRealtime(0.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
