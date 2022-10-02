using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Tilemaps;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    public const int maxCash = 10;
    
    public enum EUpgrades
    {
        IncreaseDamage,
        IncreaseLife,
        BossDecreaseLife,
        AddDash,
        IncreaseSpeed,
        BossDecreaseSpeed,
        AddJump,
        IncreaseDistanceShoot,
        Every10FreezeBoss,
        Every10ShootExtra
    }

    [System.Serializable]
    public class Upgrades
    {
        public EUpgrades type;
        public int price;
        public string description;
        public bool forPlayer;
    }

    [Header("upgrades")]
    public int cash = 10;
    public List<Upgrades> upgrades = new List<Upgrades>();
    
    private List<Upgrades> _chosenUpgrades = new List<Upgrades>();
    private List<Upgrades> _remainingUpgrades = new List<Upgrades>();
    private GameManager _gameManager;
    private PlayerController _player;
    private Boss _boss;

    [Header("Panel")]
    public Image panel;
    
    [Header("Currency")]
    public TextMeshProUGUI currency;
    public Color noCashColor;
    public Image gaugeCurrent;
    public Image gaugePreview;

    [Header("Choices")]
    public Sprite bgPlayer;
    public Sprite bgBoss;
    public Sprite fillPlayer;
    public Sprite fillBoss;
    public Sprite iconPlayer;
    public Sprite iconBoss;

    [Header("choice 1")]
    public GameObject choice1;
    public Image fillChoice1;
    public Image background1;
    public TextMeshProUGUI priceChoice1;
    public TextMeshProUGUI descChoice1;
    public Image iconChoice1;
    private Vector2 _startPosChoice1;
    private bool _isOverChoice1 = false;
    private Upgrades _upgradeChoice1 = new Upgrades();
    // private Upgrades _newUpgradeChoice1;
    
    [Header("choice 2")]
    public GameObject choice2;
    public Image fillChoice2;
    public Image background2;
    public TextMeshProUGUI priceChoice2;
    public TextMeshProUGUI descChoice2;
    public Image iconChoice2;
    private Vector2 _startPosChoice2;
    private bool _isOverChoice2 = false;
    private Upgrades _upgradeChoice2 = new Upgrades();
    // private Upgrades _newUpgradeChoice2;
    
    [Header("choice 3")]
    public GameObject choice3;
    public Image fillChoice3;
    public Image background3;
    public TextMeshProUGUI priceChoice3;
    public TextMeshProUGUI descChoice3;
    public Image iconChoice3;
    private Vector2 _startPosChoice3;
    private bool _isOverChoice3 = false;
    private Upgrades _upgradeChoice3 = new Upgrades();
    // private Upgrades _newUpgradeChoice3;

    [Header("Reroll")]
    public Image rerollImg;
    public Image fillReroll;
    public TextMeshProUGUI priceReroll;
    private bool _isOverReroll = false;
    
    [Header("Lucky")]
    public TextMeshProUGUI luckyGain;
    public TextMeshProUGUI priceLucky;
    public Image fillLucky;
    private bool _isOverLucky = false;

    [Header("Coffee")]
    public Image coffee;
    public Color overCoffe;
    public Color outCoffe;
    private bool _isOverCoffee = false;

    [Header("Animation")]
    public AnimationCurve curveReroll;
    private float _timerReRoll1;
    private float _timerReRoll2;
    private float _timerReRoll3;
    private bool _lockAnim = false;

    private Vector2 _startPos;

    void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _player = FindObjectOfType<PlayerController>();
        _boss = FindObjectOfType<Boss>();
        
        _player.IsLock = true;
        _boss.IsLock = true;

        Time.timeScale = 0f;
        
        _remainingUpgrades.AddRange(upgrades);
        Debug.Log(_upgradeChoice1);
        GenerateChoice(fillChoice1, background1, iconChoice1, priceChoice1, descChoice1, _upgradeChoice1);
        GenerateChoice(fillChoice2, background2, iconChoice2, priceChoice2, descChoice2, _upgradeChoice2);
        GenerateChoice(fillChoice3, background3, iconChoice3, priceChoice3, descChoice3, _upgradeChoice3);
        Debug.Log(_upgradeChoice1);
    }


    private void Start()
    {
        _startPosChoice1 = choice1.transform.position;
        _startPosChoice2 = choice2.transform.position;
        _startPosChoice3 = choice3.transform.position;
        
        _startPos = panel.gameObject.transform.position;
    }

    private void GenerateChoice(Image fill, Image bg, Image icon, TextMeshProUGUI price, TextMeshProUGUI description, Upgrades upgradeChoice)
    {
        if (_remainingUpgrades.Count <= 0)
        {
            return;
        }
        
        int index = Random.Range(0, _remainingUpgrades.Count);
        upgradeChoice.description = _remainingUpgrades[index].description;
        upgradeChoice.price = _remainingUpgrades[index].price;
        upgradeChoice.forPlayer = _remainingUpgrades[index].forPlayer;
        upgradeChoice.type = _remainingUpgrades[index].type;
        _remainingUpgrades.Remove(upgradeChoice);

        fill.sprite = upgradeChoice.forPlayer ? fillPlayer : fillBoss;
        bg.sprite = upgradeChoice.forPlayer ? bgPlayer : bgBoss;
        icon.sprite = upgradeChoice.forPlayer ? iconPlayer : iconBoss;
        price.text = upgradeChoice.price.ToString() + "<sprite=0>";
        description.text = upgradeChoice.description;
    }

    private IEnumerator RerollChoice(GameObject choice, Vector2 startPos, Image fill, Image bg, Image icon, TextMeshProUGUI price, TextMeshProUGUI desc, Upgrades upgradeChoice)
    {
        _lockAnim = true;
        
        float duration = 0.4f;
        float offst = 1000f;
        float timer = duration;
        while (timer > 0f)
        {
            float ratio = 1f - curveReroll.Evaluate(Mathf.Clamp01(timer / duration));
            choice.transform.position = Vector2.Lerp(startPos, startPos + Vector2.right * offst, ratio);
            timer -= Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (_remainingUpgrades.Count > 0)
        {
            GenerateChoice(fill, bg, icon, price, desc, upgradeChoice);
            
            timer = duration;
            while (timer > 0f)
            {
                float ratio = 1f - curveReroll.Evaluate(Mathf.Clamp01(timer / duration));
                choice.transform.position = Vector2.Lerp(startPos - Vector2.right * offst, startPos, ratio);
                timer -= Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }

            choice.transform.position = startPos;
        }
        
        _lockAnim = false;
    }

    public IEnumerator EndShop()
    {
        _lockAnim = true;

        GetComponent<UIShakeManager>().enabled = false;
        
        float duration = 0.6f;
        float timer = duration;
        while (timer > 0f)
        {
            float ratio = 1f - curveReroll.Evaluate(Mathf.Clamp01((timer / duration)));
            panel.transform.position = Vector2.Lerp(_startPos, _startPos + Vector2.up * 1000f, ratio);
            
            Color c = panel.color;
            c.a = MathHelper.Damping(c.a, 0f, Time.unscaledDeltaTime, 0.1f);
            panel.color = c;
            
            gaugeCurrent.fillAmount = MathHelper.Damping(gaugeCurrent.fillAmount, 0f, Time.unscaledDeltaTime, 0.05f);
            gaugePreview.fillAmount = MathHelper.Damping(gaugePreview.fillAmount, 0f, Time.unscaledDeltaTime, 0.05f);
            
            timer -= Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1f;
        _player.IsLock = false;
        _boss.IsLock = false;
        _lockAnim = false;
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        gameObject.SetActive(false);
    }

    public IEnumerator Lucky()
    {
        _lockAnim = true;
        
        float duration = 0.15f;
        int gain = 0;
        for (int i = 0; i < 8; i++)
        {
            gain = Random.Range(0, 7);
            float timer = duration;
            while (timer > 0f)
            {
                float ratio = 1f - curveReroll.Evaluate(Mathf.Clamp01(timer / duration));
                luckyGain.text = gain.ToString();
                timer -= Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        gain = Random.Range(0f, 4f) < 3f ? 0 : 7;
        luckyGain.text = "+" + gain + "<sprite=0>";
        cash = Mathf.Min(cash + gain, maxCash);
        _lockAnim = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        float targetPreview = 1f;
        float target = Mathf.Clamp01((float)cash / (float)maxCash);
        gaugeCurrent.fillAmount = MathHelper.Damping(gaugeCurrent.fillAmount, target, Time.unscaledDeltaTime, 0.05f);
        if (gaugeCurrent.fillAmount - target < 0.05f)
            gaugeCurrent.fillAmount = target;
        
        currency.text = (int)(gaugeCurrent.fillAmount * maxCash) + "<sprite=0>";

        priceChoice1.color = _upgradeChoice1.price > cash ? noCashColor : Color.white;
        priceChoice2.color = _upgradeChoice2.price > cash ? noCashColor : Color.white;
        priceChoice3.color = _upgradeChoice3.price > cash ? noCashColor : Color.white;
        priceReroll.color = 4 > cash ? noCashColor : Color.white;
        priceLucky.color = 3 > cash ? noCashColor : Color.white;

        if (!_lockAnim)
        {
            if (!_isOverChoice1 && !_isOverChoice2 && !_isOverChoice3)
                targetPreview = gaugeCurrent.fillAmount;

            float damping = 0.1f;
            float target1 = _isOverChoice1 ? 1f : 0f;
            fillChoice1.fillAmount =
                MathHelper.Damping(fillChoice1.fillAmount, target1, Time.unscaledDeltaTime, damping);
            if (_isOverChoice1)
                targetPreview = Mathf.Clamp01(((float)cash - _upgradeChoice1.price) / (float)maxCash);

            float target2 = _isOverChoice2 ? 1f : 0f;
            fillChoice2.fillAmount =
                MathHelper.Damping(fillChoice2.fillAmount, target2, Time.unscaledDeltaTime, damping);
            if (_isOverChoice2)
                targetPreview = Mathf.Clamp01(((float)cash - _upgradeChoice2.price) / (float)maxCash);

            float target3 = _isOverChoice3 ? 1f : 0f;
            fillChoice3.fillAmount =
                MathHelper.Damping(fillChoice3.fillAmount, target3, Time.unscaledDeltaTime, damping);
            if (_isOverChoice3)
                targetPreview = Mathf.Clamp01(((float)cash - _upgradeChoice3.price) / (float)maxCash);

            float targetLucky = _isOverLucky ? 1f : 0f;
            fillLucky.fillAmount =
                MathHelper.Damping(fillLucky.fillAmount, targetLucky, Time.unscaledDeltaTime, damping);
            if (_isOverLucky)
                targetPreview = Mathf.Clamp01(((float)cash - 3f) / (float)maxCash);

            float targetReroll = _isOverReroll ? 1f : 0f;
            fillReroll.fillAmount =
                MathHelper.Damping(fillReroll.fillAmount, targetReroll, Time.unscaledDeltaTime, damping);
            if (_isOverReroll)
                targetPreview = Mathf.Clamp01(((float)cash - 4f) / (float)maxCash);

            Color targetCoffee = _isOverCoffee ? overCoffe : outCoffe;
            coffee.color = targetCoffee;
            if (_isOverCoffee)
                targetPreview = 0f;

            gaugePreview.fillAmount = MathHelper.Damping(gaugePreview.fillAmount, targetPreview, Time.unscaledDeltaTime, 0.05f);
        }
    }

    private void BuyUpgrade(Upgrades upgrade)
    {
    }
    
    public void OnOverChoice1()
    {
        _isOverChoice1 = true;
        if (_upgradeChoice1.price > cash)
            currency.color = noCashColor;
    }
    public void OnOutChoice1()
    {
        _isOverChoice1 = false;
        currency.color = Color.white;
    }
    public void OnClickChoice1()
    {
        if (_lockAnim)
            return;

        if (_upgradeChoice1.price > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        GetComponent<UIShakeManager>().ApplyImpulse(40f, 0.25f, Vector2.right);
        cash -= _upgradeChoice1.price;
        BuyUpgrade(_upgradeChoice1);
        StartCoroutine(RerollChoice(choice1, _startPosChoice1, fillChoice1, background1, iconChoice1, priceChoice1, descChoice1, _upgradeChoice1));
    }
    
    public void OnOverChoice2()
    {
        _isOverChoice2 = true;
        if (_upgradeChoice2.price > cash)
            currency.color = noCashColor;
    }
    public void OnOutChoice2()
    {
        _isOverChoice2 = false;
        currency.color = Color.white;
    }
    public void OnClickChoice2()
    {
        if (_lockAnim)
            return;
        
        if (_upgradeChoice2.price > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        GetComponent<UIShakeManager>().ApplyImpulse(40f, 0.25f, Vector2.right);
        cash -= _upgradeChoice2.price;
        BuyUpgrade(_upgradeChoice2);
        StartCoroutine(RerollChoice(choice2, _startPosChoice2, fillChoice2, background2, iconChoice2, priceChoice2, descChoice2, _upgradeChoice2));
    }
    
    public void OnOverChoice3()
    {
        _isOverChoice3 = true;
        if (_upgradeChoice3.price > cash)
            currency.color = noCashColor;
    }
    public void OnOutChoice3()
    {
        _isOverChoice3 = false;
        currency.color = Color.white;
    }
    public void OnClickChoice3()
    {
        if (_lockAnim)
            return;
        
        if (_upgradeChoice3.price > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        GetComponent<UIShakeManager>().ApplyImpulse(40f, 0.25f, Vector2.right);
        cash -= _upgradeChoice3.price;
        BuyUpgrade(_upgradeChoice3);
        StartCoroutine(RerollChoice(choice3, _startPosChoice3, fillChoice3, background3, iconChoice3, priceChoice3, descChoice3, _upgradeChoice3));
    }
    
    public void OnOverLucky()
    {
        _isOverLucky = true;
        if (3 > cash)
            currency.color = noCashColor;
    }
    public void OnOutLucky()
    {
        _isOverLucky = false;
        currency.color = Color.white;
    }
    public void OnClickLucky()
    {
        if (_lockAnim)
            return;
        
        if (3 > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        cash -= 3;
        GetComponent<UIShakeManager>().ApplyImpulse(50f, 0.25f, Vector2.down);
        StartCoroutine(Lucky());
    }
    
    public void OnOverReroll()
    {
        _isOverReroll = true;
        if (4 > cash)
            currency.color = noCashColor;
    }
    public void OnOutReroll()
    {
        _isOverReroll = false;
        currency.color = Color.white;
    }
    public void OnClickReroll()
    {
        if (_lockAnim)
            return;
        
        if (4 > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        cash -= 4;
        GetComponent<UIShakeManager>().ApplyShake(10f, 0.1f);
        StartCoroutine(RerollChoice(choice1, _startPosChoice1, fillChoice1, background1, iconChoice1, priceChoice1, descChoice1, _upgradeChoice1));
        StartCoroutine(RerollChoice(choice2, _startPosChoice2, fillChoice2, background2, iconChoice2, priceChoice2, descChoice2, _upgradeChoice2));
        StartCoroutine(RerollChoice(choice3, _startPosChoice3, fillChoice3, background3, iconChoice3, priceChoice3, descChoice3, _upgradeChoice3));
    }
    
    public void OnOverCoffee()
    {
        _isOverCoffee = true;
    }
    public void OnOutCoffee()
    {
        _isOverCoffee = false;
        currency.color = Color.white;
    }
    public void OnClickCoffee()
    {
        if (_lockAnim)
            return;
        
        // GetComponent<UIShakeManager>().ApplyImpulse(50f, 1.5f, Vector2.up);
        StartCoroutine(EndShop());
    }
}
