using System.Text.RegularExpressions;

public enum Action{idle,walk,jump,fall,attack};
public class Actor
// Actor = interfaz: le paso info a mi behavior y esta me dice como actuar (ya sea modificar mis fisicas, estado...)
{
    private PhysicalEntity body;

    private Behavior behavior;
    private Action currentAction;
    private List<Ability> abilities;

    public Actor(PhysicalEntity body, Behavior behavior, List<Ability> abilities)
    {
        this.body = body;
        this.behavior = behavior;
        this.abilities = abilities;
        currentAction = Action.idle;
    }
    public void Update(float dt)
    {
        foreach(var ability in abilities)
            ability.Tick(this, dt);
        behavior.Execute(this, dt);
    }
    //================================================================================================
    // funciones comunes entre varios actores
    //================================================================================================
    public void AddVelocity(float vx, float vy)
    {
        Body.AddVelocity(vx,vy);
    }
    public void SetVelocity(float vx, float vy)
    {
        Body.SetVelocity(vx,vy);
    }
    public void SwitchAction(Action newAction)
    {
        currentAction = newAction;
        // MODIFICO ANIMACION
    }
    
    // busca una ability por tipo, ej: GetAbility<JumpAbility>()
    public T GetAbility<T>() where T : Ability
    {
        return abilities.OfType<T>().FirstOrDefault();
    }

    // booleanos para acortar el código
    public bool StoppedJumpHeight
    {
        get => GetAbility<JumpAbility>()?.stoppedJumpHeight ?? false;
        set { var j = GetAbility<JumpAbility>(); if(j != null) j.stoppedJumpHeight = value; }
    }
    public bool AircurrentAction()
    {
        return currentAction == Action.jump || currentAction == Action.fall;
    }
    public bool GroundcurrentAction()
    {
        return currentAction == Action.idle || currentAction == Action.walk;
    }
    private bool CanAttack()
    {
        return currentAction != Action.jump && currentAction != Action.fall;
    }
    private bool CanMove()
    {
        return currentAction != Action.attack;
    }
    private bool CanJump()
    {
        return currentAction != Action.attack;
    }

    public PhysicalEntity Body => body;
    public Action Action => currentAction;
    // mismos getters que body para un trabajo mas limpio
    // actor = api publica
    public bool OnPlatform => body.OnPlatform;
    public (float X, float Y)   PosVector  => body.PosVector;
    public (float VX, float VY) MoveVector => body.MoveVector;

    // una behavior necesita info. de las habilidades del actor para llamar a las mismas
    public float MoveSpeed => GetAbility<MoveAbility>()?.MoveSpeed ?? 0f;
    //================================================================================================
    // wrappers = intencion cruda que expone Actor. Behavior solo llama esto, no importa si puede
    //================================================================================================
    public void MoveHorizontal(float dt, int dir, float speed) => GetAbility<MoveAbility>()?.MoveHorizontal(this, dt, dir, speed);
    public void ApplyHorizontalFriction(float dt) => GetAbility<MoveAbility>()?.ApplyHorizontalFriction(this, dt);
    public void Jump() => GetAbility<JumpAbility>()?.Jump(this);
    public void ReleaseJump() => GetAbility<JumpAbility>()?.OnJumpKeyReleased();
    public void Attack() => GetAbility<AttackAbility>()?.Attack(this);

}