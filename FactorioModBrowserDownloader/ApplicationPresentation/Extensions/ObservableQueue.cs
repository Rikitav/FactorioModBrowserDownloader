using System.Collections.Concurrent;
using System.Collections.Specialized;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public class ObservableQueue<T> : ConcurrentQueue<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public new bool TryDequeue(out T? result)
        {
            if (!base.TryDequeue(out result))
                return false;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, result));
            return true;
        }

        public new bool TryPeek(out T? result)
        {
            return base.TryPeek(out result);
        }
    }
}
