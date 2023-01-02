using UnityEngine;

namespace Assets.Scripts.Utilities.ResponsabilityChain
{
  public interface IChaneinableHanlder<T>
  {
    IChaneinableHanlder<T> SetNext(IChaneinableHanlder<T> chainedHandler);

    void Handle(string spell, float value);
  }

  public abstract class ChainedHandler<T> : MonoBehaviour, IChaneinableHanlder<T>
  {
    private IChaneinableHanlder<T> nextChainedHandler;

    public IChaneinableHanlder<T> SetNext(IChaneinableHanlder<T> chainedHandler)
    {
      this.nextChainedHandler = chainedHandler;
      return this.nextChainedHandler;
    }

    public void Handle(string spell, float value)
    {
      if (CanHandle(spell))
      {
        Cooldown(value);
      }
      else
      {
        nextChainedHandler.Handle(spell, value);
      }
    }

    public abstract bool CanHandle(string spell);

    public abstract void Cooldown(float value);
  }
}
