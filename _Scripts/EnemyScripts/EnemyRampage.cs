using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRampage : MonoBehaviour
{
    [Header("Eye Renderers")]
    [Tooltip("Assign the Renderer(s) whose material you want tinted and emissive during rampage.")]
    [SerializeField] private Renderer[] eyeRenderers;

    private NavMeshAgent agent;
    private float originalSpeed;

    private MaterialPropertyBlock[] propertyBlocks;
    private Color[] originalBaseColors;
    private Color[] originalEmissionColors;

    private bool isRaging = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        originalSpeed = agent.speed;

        int count = eyeRenderers.Length;
        propertyBlocks = new MaterialPropertyBlock[count];
        originalBaseColors = new Color[count];
        originalEmissionColors = new Color[count];

        for (int i = 0; i < count; i++)
        {
            propertyBlocks[i] = new MaterialPropertyBlock();

            var mat = eyeRenderers[i].material;
            originalBaseColors[i] = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.GetColor("_Color");
            originalEmissionColors[i] = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;
        }
    }

    private void OnEnable()
    {
        if (RampageManager.Instance != null)
        {
            RampageManager.Instance.OnRampageStart += HandleRampageStart;
            RampageManager.Instance.OnRampageEnd += HandleRampageEnd;

            if (RampageManager.Instance.IsRampageActive)
            {
                ApplyRampage(RampageManager.Instance.SpeedMultiplier, RampageManager.Instance.RampageEyeColor);
            }
        }
    }

    private void OnDisable()
    {
        if (RampageManager.Instance != null)
        {
            RampageManager.Instance.OnRampageStart -= HandleRampageStart;
            RampageManager.Instance.OnRampageEnd -= HandleRampageEnd;
        }
    }

    private void HandleRampageStart(float mult, Color eyeColor) => StartCoroutine(ApplyRampageCoroutine(mult, eyeColor));
    private void HandleRampageEnd() => StartCoroutine(RevertRampageCoroutine());

    private IEnumerator ApplyRampageCoroutine(float speedMultiplier, Color eyeColor)
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.15f)); // spread load

        if (isRaging) yield break;
        isRaging = true;

        agent.speed = originalSpeed * speedMultiplier;

        for (int i = 0; i < eyeRenderers.Length; i++)
        {
            var block = propertyBlocks[i];
            block.SetColor("_BaseColor", eyeColor);
            block.SetColor("_EmissionColor", eyeColor);
            eyeRenderers[i].SetPropertyBlock(block);
        }
    }

    private IEnumerator RevertRampageCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.15f)); // spread load

        if (!isRaging) yield break;
        isRaging = false;

        agent.speed = originalSpeed;

        for (int i = 0; i < eyeRenderers.Length; i++)
        {
            var block = propertyBlocks[i];
            block.SetColor("_BaseColor", originalBaseColors[i]);
            block.SetColor("_EmissionColor", originalEmissionColors[i]);
            eyeRenderers[i].SetPropertyBlock(block);
        }
    }

    private void ApplyRampage(float mult, Color eyeColor) => StartCoroutine(ApplyRampageCoroutine(mult, eyeColor));
    private void RevertRampage() => StartCoroutine(RevertRampageCoroutine());
}
