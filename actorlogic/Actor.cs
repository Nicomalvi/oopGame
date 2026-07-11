using System.Text.RegularExpressions;

public enum ActorState{idle,walk,jump,fall,attack};
public enum HorizontalFacing{left,right};
public class Actor
// Actor = interfaz: le paso info a mi behavior y esta me dice como actuar (ya sea modificar mis fisicas, estado...)
{
    private PhysicalEntity body;

    // debido a coupling -> actor no conoce animaciones -> el animation controller cada turno chequea facing
    public HorizontalFacing facing;

    private Behavior behavior;
    private ActorState currentAction;
    private List<Ability> abilities;

    public Actor(PhysicalEntity body, Behavior behavior, List<Ability> abilities)
    {
        this.body = body;
        this.behavior = behavior;
        this.abilities = abilities;
        currentAction = ActorState.idle;
        facing = HorizontalFacing.right;
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
    public void SwitchAction(ActorState newAction)
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
        return currentAction == ActorState.jump || currentAction == ActorState.fall;
    }
    public bool GroundCurrentAction()
    {
        return currentAction == ActorState.idle || currentAction == ActorState.walk;
    }
    private bool CanAttack()
    {
        return currentAction != ActorState.jump && currentAction != ActorState.fall;
    }
    private bool CanMove()
    {
        return currentAction != ActorState.attack;
    }
    private bool CanJump()
    {
        return currentAction != ActorState.attack;
    }

    public PhysicalEntity Body => body;
    public ActorState Action => currentAction;
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