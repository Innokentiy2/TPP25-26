using System.Dynamic;
using System.Security;
using System.Collections.Generic;

class Player
{
    public int health {get; set;} = 100;
    public int maxHealth {get; set;} = 100;
}

class Location
{
    public string name {get;}
    public string description {get;}
    
}

public class GameState
{
    public int health;
    public int movesCount;
    public bool flag;
    public List<string> Inventory { get; } = new List<string>();
    public bool isGameOver {get; set;}
    public int turnCount {get; set;}
}

interface ICommand
{
    string name {get;}
    string description {get;}
    void execute(GameState state, string[] args){}

}

abstract class CommandBase : ICommand
{
    public string name{get;}
    public string description{get;}
    protected CommandBase(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
    public abstract void execute(GameState state, string[] args);   
}

public interface IInterractable
{
    public void InterractCommand(GameState state);
}

public interface ICondition
{
    bool IsMet(GameState State);
}

abstract class ConditionBase : ICondition
{
    public abstract bool IsMet(GameState state);
}

public interface IEffect
{
    void Apply(GameState state);
}

abstract class EffectBase : IEffect
{
    public abstract void Apply(GameState state);
}

public abstract class GameEventBase
{
    public string Id { get; }
    public bool IsOneTime { get; }

    private bool wasTriggered;

    protected ICondition condition { get; }
    protected List<IEffect> effects { get; }

    protected GameEventBase(string id, ICondition condition, IEnumerable<IEffect> effects, bool isOneTime = false)
    {
        Id = id;
        this.condition = condition;
        this.effects = new List<IEffect>(effects);
        IsOneTime = isOneTime;
    }

    public void TryTrigger(GameState state)
    {
        if (IsOneTime && wasTriggered)
            return;

        if (!condition.IsMet(state))
            return;

        foreach (var effect in effects)
            effect.Apply(state);

        wasTriggered = true;
    }
}
