using UnityEngine;

public class EnemyStrategyController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour strategyComponent;

    private IEnemyStrategy activeStrategy;

    private void Awake()
    {
        if (!SetStrategy(strategyComponent))
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!IsSelectedStrategyEnabled())
        {
            return;
        }

        activeStrategy.Execute();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsSelectedStrategyEnabled())
        {
            return;
        }

        IEnemyContactStrategy contactStrategy = activeStrategy as IEnemyContactStrategy;
        if (contactStrategy != null)
        {
            contactStrategy.OnContact(other);
        }
    }

    public bool SetStrategy(MonoBehaviour newStrategyComponent)
    {
        if (newStrategyComponent == null)
        {
            Debug.LogError("EnemyStrategyController requires a strategy component.", this);
            return false;
        }

        if (newStrategyComponent.gameObject != gameObject)
        {
            Debug.LogError("Enemy strategies must be components on the same enemy.", this);
            return false;
        }

        IEnemyStrategy newStrategy = newStrategyComponent as IEnemyStrategy;
        if (newStrategy == null)
        {
            Debug.LogError(
                "The selected component does not implement IEnemyStrategy.",
                newStrategyComponent
            );
            return false;
        }

        if (!newStrategyComponent.isActiveAndEnabled)
        {
            Debug.LogError("The selected enemy strategy component is disabled.", newStrategyComponent);
            return false;
        }

        strategyComponent = newStrategyComponent;
        activeStrategy = newStrategy;
        return true;
    }

    private bool IsSelectedStrategyEnabled()
    {
        if (activeStrategy != null && strategyComponent != null &&
            strategyComponent.isActiveAndEnabled)
        {
            return true;
        }

        Debug.LogError("The selected enemy strategy is missing or disabled.", this);
        enabled = false;
        return false;
    }
}
