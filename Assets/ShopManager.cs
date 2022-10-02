using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        IncraseArrowNumber,
        Every10HitBoss,
        Every10ShootExtra,
        None,
    }

    [System.Serializable]
    public class Upgrades
    {
        public EUpgrades type;
        public int price;
        public string description;
        public bool forPlayer;
        public Sprite sprite;
    }

    [Header("upgrades")]
    public int cash = 10;
    public List<Upgrades> upgrades = new List<Upgrades>();
    
    private List<EUpgrades> _chosenUpgradesType = new List<EUpgrades>();
    private List<EUpgrades> _remainingUpgradesType = new List<EUpgrades>();
    private GameManager _gameManager;
    private PlayerController _player;
    private Boss _boss;

    [Header("Panel")]
    public Image panel;
    private HUDManager _hud;
    
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
    private Vector2 _startLocalPosChoice1;
    private bool _isOverChoice1 = false;
    private EUpgrades _upgradeChoice1 = EUpgrades.None;
    
    [Header("choice 2")]
    public GameObject choice2;
    public Image fillChoice2;
    public Image background2;
    public TextMeshProUGUI priceChoice2;
    public TextMeshProUGUI descChoice2;
    public Image iconChoice2;
    private Vector2 _startLocalPosChoice2;
    private bool _isOverChoice2 = false;
    private EUpgrades _upgradeChoice2 = EUpgrades.None;
    
    [Header("choice 3")]
    public GameObject choice3;
    public Image fillChoice3;
    public Image background3;
    public TextMeshProUGUI priceChoice3;
    public TextMeshProUGUI descChoice3;
    public Image iconChoice3;
    private Vector2 _startLocalPosChoice3;
    private bool _isOverChoice3 = false;
    private EUpgrades _upgradeChoice3 = EUpgrades.None;

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

    private Vector2 _startLocalPos;

    void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _player = FindObjectOfType<PlayerController>();
        _boss = FindObjectOfType<Boss>();
        _hud = FindObjectOfType<HUDManager>();
        
        _player.IsLock = true;
        _boss.IsLock = true;

        cash = maxCash;
        
        Time.timeScale = 0f;
    }


    private void Start()
    {
        GenerateChoice(fillChoice1, background1, iconChoice1, priceChoice1, descChoice1, 1);
        GenerateChoice(fillChoice2, background2, iconChoice2, priceChoice2, descChoice2, 2);
        GenerateChoice(fillChoice3, background3, iconChoice3, priceChoice3, descChoice3, 3);
        
        _startLocalPosChoice1 = choice1.transform.localPosition;
        _startLocalPosChoice2 = choice2.transform.localPosition;
        _startLocalPosChoice3 = choice3.transform.localPosition;
        
        _startLocalPos = panel.gameObject.transform.localPosition;
    }

    private void GenerateChoice(Image fill, Image bg, Image icon, TextMeshProUGUI price, TextMeshProUGUI description, int numberChoice)
    {
        _remainingUpgradesType = new List<EUpgrades>();
        for (int i = 0; i < upgrades.Count; i++)
            _remainingUpgradesType.Add(upgrades[i].type);
        
        _remainingUpgradesType.Remove(_upgradeChoice1);
        _remainingUpgradesType.Remove(_upgradeChoice2);
        _remainingUpgradesType.Remove(_upgradeChoice3);

        for (int i = 0; i < _chosenUpgradesType.Count; i++)
            _remainingUpgradesType.Remove(_chosenUpgradesType[i]);
        
        //FUCK THIS SHIT, I HAVE ENOUGH OF THIS
        // CANT PASS REFERENCE, OBJECT IS NEVER ATTRIBUTED
        // FUUUUUCK !
        int index = Random.Range(0, _remainingUpgradesType.Count);
        Upgrades up = GetUpgradeFromType(_remainingUpgradesType[index]);

        switch (numberChoice)
        {
            case 1:
                _upgradeChoice1 = _remainingUpgradesType[index];
                break;
            case 2 :
                _upgradeChoice2 = _remainingUpgradesType[index];
                break;
            case 3 :
                _upgradeChoice3 = _remainingUpgradesType[index];
                break;
        }
        _remainingUpgradesType.RemoveAt(index);

        fill.sprite = up.forPlayer ? fillPlayer : fillBoss;
        bg.sprite = up.forPlayer ? bgPlayer : bgBoss;
        // icon.sprite = up.forPlayer ? iconPlayer : iconBoss;
        price.text = up.price.ToString() + "<sprite=0>";
        icon.sprite = up.sprite;
        description.text = up.description;
    }

    public Upgrades GetUpgradeFromType(EUpgrades type)
    {
        foreach (var up in upgrades)
        {
            if (up.type == type)
                return up;
        }

        return null;
    }

    private IEnumerator RerollChoice(GameObject choice, Vector2 startPos, Image fill, Image bg, Image icon, TextMeshProUGUI price, TextMeshProUGUI desc, int upgradeChoice)
    {
        _lockAnim = true;
        
        float duration = 0.4f;
        float offst = 1000f;
        float timer = duration;
        while (timer > 0f)
        {
            float ratio = 1f - curveReroll.Evaluate(Mathf.Clamp01(timer / duration));
            choice.transform.localPosition = Vector2.Lerp(startPos, startPos + Vector2.right * offst, ratio);
            timer -= Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (_remainingUpgradesType.Count > 0)
        {
            GenerateChoice(fill, bg, icon, price, desc, upgradeChoice);
            
            timer = duration;
            while (timer > 0f)
            {
                float ratio = 1f - curveReroll.Evaluate(Mathf.Clamp01(timer / duration));
                choice.transform.localPosition = Vector2.Lerp(startPos - Vector2.right * offst, startPos, ratio);
                timer -= Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }

            choice.transform.localPosition = startPos;
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
            panel.transform.localPosition = Vector2.Lerp(_startLocalPos, _startLocalPos + Vector2.up * 1000f, ratio);
            
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
        
        _boss.GetComponentInChildren<Animator>().SetBool("Charging", false);
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

        float price1 = GetUpgradeFromType(_upgradeChoice1).price;
        float price2 = GetUpgradeFromType(_upgradeChoice1).price;
        float price3 = GetUpgradeFromType(_upgradeChoice1).price;
        
        priceChoice1.color = price1 > cash ? noCashColor : Color.white;
        priceChoice2.color = price2 > cash ? noCashColor : Color.white;
        priceChoice3.color = price3 > cash ? noCashColor : Color.white;
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
                targetPreview = Mathf.Clamp01(((float)cash - price1) / (float)maxCash);

            float target2 = _isOverChoice2 ? 1f : 0f;
            fillChoice2.fillAmount =
                MathHelper.Damping(fillChoice2.fillAmount, target2, Time.unscaledDeltaTime, damping);
            if (_isOverChoice2)
                targetPreview = Mathf.Clamp01(((float)cash - price2) / (float)maxCash);

            float target3 = _isOverChoice3 ? 1f : 0f;
            fillChoice3.fillAmount =
                MathHelper.Damping(fillChoice3.fillAmount, target3, Time.unscaledDeltaTime, damping);
            if (_isOverChoice3)
                targetPreview = Mathf.Clamp01(((float)cash - price3) / (float)maxCash);

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

    private void BuyUpgrade(EUpgrades type)
    {
        _chosenUpgradesType.Add(type);
        
        switch (type)
        {
            case EUpgrades.AddDash:
                PlayerController.HasDash = true;
                break;
            case EUpgrades.AddJump:
                PlayerController.HasJump = true;
                break;
            case EUpgrades.IncreaseDamage:
                Arrow.ExtraDamageFactor = 0.25f;
                break;
            case EUpgrades.IncreaseLife:
                _player.IncreaseLife(3);
                break;
            case EUpgrades.IncreaseSpeed:
                PlayerController.ExtraMovementSpeedFactor = 0.2f;
                break;
            case EUpgrades.BossDecreaseLife:
                _boss.DowngradeBossLife(0.15f);
                break;
            case EUpgrades.BossDecreaseSpeed:
                Boss.DowngradeSpeedFactor = 0.2f;
                break;
            case EUpgrades.Every10HitBoss:
                Boss.HasAutoHit = true;
                break;
            case EUpgrades.Every10ShootExtra:
                PlayerController.HasAutoHit = true;
                break;
            case EUpgrades.IncreaseDistanceShoot:
                Arrow.ExtraDurationVelocity = 0.5f;
                break;
            case EUpgrades.IncraseArrowNumber:
                _player.IncreaseAmmo(3);
                break;
        }
        
        _hud.SetImage(GetUpgradeFromType(type).sprite, _chosenUpgradesType.Count - 1);
        StartCoroutine(Real());
    }

    public IEnumerator Real()
    {
        Time.timeScale = 0f;
        
        yield return new WaitForEndOfFrame();
        
        Time.timeScale = 1f;
        // float duration = 1f;
        // float timer = duration;
        // while (timer > 0f)
        // {
        //     timer -= Time.unscaledDeltaTime;
        //
        //     float ratio = Mathf.Clamp01(timer / duration);
        //     Debug.Log(ratio);
        //     Time.timeScale = ratio;
        // }
        
        yield return new WaitForSecondsRealtime(0.4f);
        
        Time.timeScale = 0f;
    }
    
    public void OnOverChoice1()
    {
        _isOverChoice1 = true;
        if (GetUpgradeFromType(_upgradeChoice1).price > cash)
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

        if (GetUpgradeFromType(_upgradeChoice1).price > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        GetComponent<UIShakeManager>().ApplyImpulse(40f, 0.25f, Vector2.right);
        cash -= GetUpgradeFromType(_upgradeChoice1).price;
        BuyUpgrade(_upgradeChoice1);
        StartCoroutine(RerollChoice(choice1, _startLocalPosChoice1, fillChoice1, background1, iconChoice1, priceChoice1, descChoice1, 1));
    }
    
    public void OnOverChoice2()
    {
        _isOverChoice2 = true;
        if (GetUpgradeFromType(_upgradeChoice2).price > cash)
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
        
        if (GetUpgradeFromType(_upgradeChoice2).price > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        GetComponent<UIShakeManager>().ApplyImpulse(40f, 0.25f, Vector2.right);
        cash -= GetUpgradeFromType(_upgradeChoice2).price;
        BuyUpgrade(_upgradeChoice2);
        StartCoroutine(RerollChoice(choice2, _startLocalPosChoice2, fillChoice2, background2, iconChoice2, priceChoice2, descChoice2, 2));
    }
    
    public void OnOverChoice3()
    {
        _isOverChoice3 = true;
        if (GetUpgradeFromType(_upgradeChoice3).price > cash)
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
        
        if (GetUpgradeFromType(_upgradeChoice3).price > cash)
        {
            GetComponent<UIShakeManager>().ApplyShake(5f, 0.05f);
            return;
        }
        
        GetComponent<UIShakeManager>().ApplyImpulse(40f, 0.25f, Vector2.right);
        cash -= GetUpgradeFromType(_upgradeChoice3).price;
        BuyUpgrade(_upgradeChoice3);
        StartCoroutine(RerollChoice(choice3, _startLocalPosChoice3, fillChoice3, background3, iconChoice3, priceChoice3, descChoice3, 3));
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
        StartCoroutine(RerollChoice(choice1, _startLocalPosChoice1, fillChoice1, background1, iconChoice1, priceChoice1, descChoice1, 1));
        StartCoroutine(RerollChoice(choice2, _startLocalPosChoice2, fillChoice2, background2, iconChoice2, priceChoice2, descChoice2, 2));
        StartCoroutine(RerollChoice(choice3, _startLocalPosChoice3, fillChoice3, background3, iconChoice3, priceChoice3, descChoice3, 3));
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
