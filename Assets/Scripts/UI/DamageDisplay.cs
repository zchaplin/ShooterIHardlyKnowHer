using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DamageDisplay : MonoBehaviour
{
    [SerializeField] private HealthManager healthManager;
    private Volume volume;
    private Vignette vignette;
    private float intensity = 0.4f;
    private Coroutine activeEffect;

    void Start()
    {
        volume = GetComponent<Volume>();

        if (!volume || !volume.profile.TryGet(out vignette))
        {
            Debug.LogError("No vignette found on Volume!");
            return;
        }

        vignette.active = false;

        if (healthManager != null)
        {
            healthManager.PlayerDamaged += OnPlayerDamaged;
        }
        else
        {
            GameObject hpObject = GameObject.FindGameObjectWithTag("HealthManager");
            if (hpObject == null)
            {
                Debug.LogError("No health manager!");
            }
            else
            {
                healthManager = hpObject.GetComponent<HealthManager>();
                healthManager.PlayerDamaged += OnPlayerDamaged;
            }
        }
    }

    void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.PlayerDamaged -= OnPlayerDamaged;
        }
    }

    private void OnPlayerDamaged()
    {
        if (activeEffect != null)
        {
            StopCoroutine(activeEffect);
        }
        activeEffect = StartCoroutine(DamageEffect());
    }

    private IEnumerator DamageEffect()
    {
        vignette.active = true;
        intensity = 0.4f;
        vignette.intensity.value = intensity;

        yield return new WaitForSeconds(0.4f);

        while (intensity > 0)
        {
            intensity -= 0.05f;
            vignette.intensity.value = intensity;
            yield return new WaitForSeconds(0.1f);
        }

        vignette.active = false;
    }
}
