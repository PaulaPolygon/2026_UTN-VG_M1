using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHudObserver : MonoBehaviour
{
    [Serializable]
    private class EnemyIconBinding
    {
        public EnemyHealth enemy = null;
        public RawImage icon = null;
        [NonSerialized] public RawImage eliminatedMark;
    }

    [SerializeField] private Texture2D eliminatedMarkTexture;
    [SerializeField] private EnemyIconBinding[] enemyIcons;

    private void Awake()
    {
        if (eliminatedMarkTexture == null)
        {
            Debug.LogError("EnemyHudObserver requires the eliminated mark texture.", this);
            enabled = false;
            return;
        }

        if (enemyIcons == null || enemyIcons.Length == 0)
        {
            Debug.LogError("EnemyHudObserver requires at least one enemy icon binding.", this);
            enabled = false;
            return;
        }

        foreach (EnemyIconBinding binding in enemyIcons)
        {
            if (binding == null || binding.enemy == null || binding.icon == null)
            {
                Debug.LogError(
                    "Every EnemyHudObserver binding requires an enemy and a RawImage icon.",
                    this
                );
                enabled = false;
                return;
            }
        }

        foreach (EnemyIconBinding binding in enemyIcons)
        {
            binding.eliminatedMark = CreateEliminationMark(binding.icon);
        }
    }

    private void OnEnable()
    {
        if (enemyIcons == null)
        {
            return;
        }

        foreach (EnemyIconBinding binding in enemyIcons)
        {
            if (binding != null && binding.enemy != null)
            {
                binding.enemy.Died += HandleEnemyDied;
            }
        }
    }

    private void OnDisable()
    {
        if (enemyIcons == null)
        {
            return;
        }

        foreach (EnemyIconBinding binding in enemyIcons)
        {
            if (binding != null && binding.enemy != null)
            {
                binding.enemy.Died -= HandleEnemyDied;
            }
        }
    }

    private RawImage CreateEliminationMark(RawImage enemyIcon)
    {
        GameObject markObject = new GameObject(
            "Enemy Eliminated Mark",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage)
        );
        markObject.transform.SetParent(enemyIcon.transform, false);
        markObject.transform.SetAsLastSibling();

        RectTransform markRect = (RectTransform)markObject.transform;
        markRect.anchorMin = Vector2.zero;
        markRect.anchorMax = Vector2.one;
        markRect.offsetMin = Vector2.zero;
        markRect.offsetMax = Vector2.zero;

        RawImage markImage = markObject.GetComponent<RawImage>();
        markImage.texture = eliminatedMarkTexture;
        markImage.raycastTarget = false;
        markImage.enabled = false;
        return markImage;
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        foreach (EnemyIconBinding binding in enemyIcons)
        {
            if (binding.enemy == enemy)
            {
                binding.eliminatedMark.enabled = true;
                return;
            }
        }
    }
}
