using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private PlayerController _player;
    private Boss _boss;

    [Header("UI'")]
    public Image playerLife;
    public Image bossLife;
    public Image arrowImage;
    public float offsetArrows;
        
    private List<Image> _arrowAmmos = new List<Image>();

    private int _maxAmmoUI = 10;
    
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
        
        arrowImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_boss != null)
            bossLife.fillAmount = _boss.LifeRatio;

        if (_player != null)
        {
            playerLife.fillAmount = _player.LifeRatio;
            for (int i = 0; i < _arrowAmmos.Count; i++)
            {
                bool active = i >= _player.maxAmmoArrows ? false : true;
                _arrowAmmos[i].gameObject.SetActive(active);
                _arrowAmmos[i].color = i >= _player.CurrentAmmoArrows ? Color.black : Color.white;
            }
        }
    }
}
