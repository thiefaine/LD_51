using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerController _player;
    private Boss _boss;

    [Header("HUD")]
    public Image playerIcon;
    public Image bossLife;
    public Image arrowImage;
    public float offsetPlayerIcon;
    public float offsetArrows;
    public List<Image> upgrades = new List<Image>();
        
    private List<Image> _arrowAmmos = new List<Image>();
    private List<Image> _playerIcons = new List<Image>();

    private int _maxAmmoUI = 10;
    private int _maxLifePlayer = 15;
    
    // Start is called before the first frame update
    void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        _boss = FindObjectOfType<Boss>();

        for (int i = 0; i < _maxAmmoUI; i++)
        {
            var img = GameObject.Instantiate(arrowImage.gameObject, arrowImage.transform.position, arrowImage.transform.rotation, arrowImage.transform.parent);
            img.transform.position = arrowImage.transform.position + new Vector3(i * offsetArrows, 0f, 0f);
            img.SetActive(false);
            _arrowAmmos.Add(img.GetComponent<Image>());
        }

        for (int i = 0; i < _maxLifePlayer; i++)
        {
            Vector3 offset = new Vector3(i * offsetPlayerIcon, 0f, 0f);
            GameObject icon = GameObject.Instantiate(playerIcon.gameObject, playerIcon.transform.position + offset, playerIcon.transform.rotation, playerIcon.transform.parent);
            _playerIcons.Add(icon.GetComponent<Image>());
        }
        
        foreach (var img in upgrades)
            img.enabled = false;
        
        arrowImage.gameObject.SetActive(false);
    }
    
    public void SetImage(Sprite sprite, int index)
    {
        upgrades[index].enabled = true;
        upgrades[index].sprite = sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (_boss != null)
            bossLife.fillAmount = _boss.LifeRatio;

        if (_player != null)
        {
            
            playerIcon.fillAmount = _player.LifeRatio;
            
            for (int i = 0; i < _arrowAmmos.Count; i++)
            {
                bool active = i >= _player.maxAmmoArrows ? false : true;
                _arrowAmmos[i].gameObject.SetActive(active);
                _arrowAmmos[i].color = i >= _player.CurrentAmmoArrows ? Color.black : Color.white;
            }
            
            for (int i = 0; i < _playerIcons.Count; i++)
            {
                bool visible = i < _player.maxLife;
                bool active = i < _player.CurrentLife;
                _playerIcons[i].gameObject.SetActive(visible);
                _playerIcons[i].enabled = active;
                _playerIcons[i].transform.GetChild(0).GetComponent<Image>().color = active ? Color.white : Color.black;
            }
        }
    }
}
