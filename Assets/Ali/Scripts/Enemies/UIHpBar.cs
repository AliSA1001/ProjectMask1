using UnityEngine;
using UnityEngine.UI;
public class UIHpBar : MonoBehaviour
{
   [SerializeField] private Transform target;
   [SerializeField] private Vector3 offset;
   [SerializeField] private Slider hpSlider;
    [SerializeField] private Image foreground;
    [SerializeField] private Image background;
   private EnemyHP enemyHP;

    private void Start()
    {
        enemyHP = GetComponentInParent<EnemyHP>();
        hpSlider.value = enemyHP.GetHealth();
    }

    private void LateUpdate()
    {
        Vector3 direction = (target.position - Camera.main.transform.position).normalized;
        bool isBehind = Vector3.Dot(direction,Camera.main.transform.forward) < 0;
        if(isBehind)
        {
            foreground.enabled = false;
            background.enabled = false;
        }
        else
        {
            foreground.enabled = true;
            background.enabled = true;
        }
        transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
        hpSlider.value = enemyHP.GetHealth();
    }
}
