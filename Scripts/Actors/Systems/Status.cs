using System.Collections.Generic;

using Godot;

public enum EffectType
{
  Pure,
  Physical,
  Wet,
  Fire,
  Frost,
  Poison,
  Electric,
}

// TODO: This only works for health related effects for now. Need to find a way to expand to other things (stun, buffs, invisibility etc)
public class Effect(string name, EffectType type, float strength = 1, int turns = 0, int interval = 100)
{
  public string Name = name;
  public EffectType Type = type;

  public float InitialStrength = strength;
  public float Strength = strength;

  public int InitialTurns = turns;
  public int Turns = turns;

  public int InitialInterval = interval;
  public int Interval = interval;

  public int PartialInterval = 0;

  public System.Action<Actor, Effect, int> Apply;
}

public class Status(Actor actor, int health, Dictionary<EffectType, float> factors = null)
{
  readonly Actor Actor = actor;

  public System.Action OnDeath;

  public int MaxHealth { get; private set; } = health;
  public int Health { get; private set; } = health;

  public readonly Dictionary<EffectType, float> Factors = factors ?? [];
  public readonly List<Effect> Effects = [];

  public void Damage(float strength, EffectType type = EffectType.Physical)
  {
    float multiplier = Factors.TryGetValue(type, out float factor) ? factor : 1f;
    int final = Mathf.Max(0, (int)(strength * multiplier));

    Health = Mathf.Min(MaxHealth, Mathf.Max(0, Health - final));

    if (Health == 0)
    {
      // TODO: Implement actual death
      OnDeath?.Invoke();
    }
  }

  public void AddEffect(Effect effect)
  {
    Effects.Add(effect);
  }

  public void FlowTurns(int turns)
  {
    List<Effect> remove = [];

    foreach (Effect effect in Effects)
    {
      HandleEffectFlow(effect, turns);

      // TODO: Is this rule to dispose an Effect Ok?
      if (effect.InitialTurns > 0 && effect.Turns <= 0)
      {
        remove.Add(effect);
      }
    }

    foreach (Effect effect in remove)
    {
      Effects.Remove(effect);
    }
  }

  public bool TryGetEffectOfType(EffectType type, out Effect effect)
  {
    effect = Effects.Find(e => e.Type == type);
    return effect != null;
  }

  void HandleEffectFlow(Effect effect, int turnsToFlow)
  {
    turnsToFlow += effect.PartialInterval;

    while (turnsToFlow > 0 && effect.Turns > 0)
    {
      int turns = Mathf.Min(Mathf.Min(turnsToFlow, effect.Interval), effect.Turns);

      turnsToFlow -= turns;
      effect.Turns -= turns;
      effect.Apply?.Invoke(Actor, effect, turns);
    }

    effect.PartialInterval = turnsToFlow;
  }
}
