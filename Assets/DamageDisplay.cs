using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DamageDisplay : MonoBehaviour
{
    [SerializeField] private HealthManager healthManager;
    private PostProcessVolume volume;
    private Vignette vignette;
    private float intensity = 0.4f;
    private Coroutine activeEffect;

    void Start()
    {
        volume = GetComponent<PostProcessVolume>();
        
        if (!volume || !volume.profile.TryGetSettings<Vignette>(out vignette))
        {
            Debug.LogError("No vignette found on PostProcessVolume!");
            return;
        }

        vignette.enabled.Override(false);

        if (healthManager != null)
        {
            healthManager.PlayerDamaged += OnPlayerDamaged;
        }
        else
        {
            GameObject hpObject = GameObject.Find("healthManager");
            if(hpObject == null){
                 Debug.LogError("No health manager!");
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
        vignette.enabled.Override(true);
        vignette.intensity.Override(intensity);
        
        yield return new WaitForSeconds(0.4f);

        while (intensity > 0)
        {
            intensity -= 0.01f;
            vignette.intensity.Override(intensity);
            yield return new WaitForSeconds(0.1f);
        }

        vignette.enabled.Override(false);
    }
}
