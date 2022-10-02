using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject cursor;
    public Image spriteFade;
    public SpriteRenderer text;
    public ShopManager shop;

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
        text.gameObject.SetActive(false);
        
        yield return new WaitForSecondsRealtime(1f);
        
        text.gameObject.SetActive(true);
        
        yield return new WaitForSecondsRealtime(3f);
        
        _boss.GetComponentInChildren<Animator>().SetBool("Charging", true);
        
        yield return new WaitForSecondsRealtime(3f);
        
        Time.timeScale = 0f;
        text.gameObject.SetActive(false);
        shop.gameObject.SetActive(true);
    }

    private IEnumerator EndGame()
    {
        float duration = 3f;
        float timer = duration;

        _player.IsLock = true;
        _boss.IsLock = true;

        Time.timeScale = 0f;
        
        yield return new WaitForSecondsRealtime(2f);
        
        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            
            float ratio = 1f - Mathf.Clamp01(timer / duration);
            Time.timeScale = Mathf.Lerp(1f, 0f, ratio);
            Color col = spriteFade.color;
            col.a = Mathf.Lerp(1f, 0f, ratio);
            spriteFade.color = col;
            yield return new WaitForEndOfFrame();
        }

        spriteFade.color = Color.black;
        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
