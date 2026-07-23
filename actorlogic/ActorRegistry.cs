public class ActorRegistry
{
    private List<Actor> actors = new();
    private Dictionary<PhysicalEntity, Actor> physEntLookup = new();

    public void AddActor(Actor actor)
    {
        actors.Add(actor);
        physEntLookup[actor.Body] = actor;
    }

    public Actor GetActorFor(PhysicalEntity pe)
    {
        physEntLookup.TryGetValue(pe, out var actor); 
        // try devuelve un bool, indico que si encuentra lo mande a variable actor
        
        return actor; // null si es una PE sin actor asociado
    }

    public void Update(float dt)
    {
        foreach (var actor in actors)
            actor.Update(dt);
    } 
}