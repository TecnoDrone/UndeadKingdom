using UnityEngine;

namespace Assets.Scripts.Utilities.ResponsabilityChain
{
  public interface IChaneinableHanlder<T>
  {
    IChaneinableHanlder<T> SetNext(IChaneinableHanlder<T> chainedHandler);

    void Handle(T request);
  }

  public abstract class ChainedHandler<T> : MonoBehaviour, IChaneinableHanlder<T>
  {
    private IChaneinableHanlder<T> nextChainedHandler;

    public IChaneinableHanlder<T> SetNext(IChaneinableHanlder<T> chainedHandler)
    {
      this.nextChainedHandler = chainedHandler;
      return this.nextChainedHandler;
    }

    public void Handle(T request)
    {
      if (CanHandle(request))
      {
        Process(request);
      }
      else
      {
        nextChainedHandler.Handle(request);
      }
    }

    protected abstract bool CanHandle(T request);

    protected abstract void Process(T request);
  }
}
