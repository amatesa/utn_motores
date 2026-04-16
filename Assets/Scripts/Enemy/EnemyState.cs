/// <summary>
/// Estados principales de IA del enemigo.
///
/// Se define como tipo compartido para que tanto el sistema legacy
/// (ShadowEnemy) como el sistema SRP (ShadowEnemyBrain y módulos)
/// puedan depender del mismo enum sin acoplarse entre sí.
/// </summary>
public enum EnemyState
{
    Idle,
    Investigate,
    Chase,
    Retreat
}
