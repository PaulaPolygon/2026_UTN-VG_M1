using UnityEngine;

public interface IEnemyStrategy
{
    void Execute();
}

public interface IEnemyContactStrategy
{
    void OnContact(Collider other);
}
