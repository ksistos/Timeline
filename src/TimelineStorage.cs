namespace Timeline {
  public class TimelineStorage<T> : ITimelineStorage<T> {
    private readonly T[] _items;
    private readonly uint _maxSize;

    public TimelineStorage(uint maxSize) {
      _items = new T[maxSize];
      MaxFrame = 0;
      _maxSize = maxSize;
      MinFrame = 0;
    }

    public uint MinFrame { get; private set; }
    public uint MaxFrame { get; private set; }

    public T this[uint index] {
      get { return GetItem(index); }
      set { Store(index, value); }
    }

    public void Store(uint index, T item) {
      if(index > MaxFrame + 1
         || index < MinFrame)
        throw new TimelineException(
          $"Cannot store item at index {index}. Index must be between {MinFrame} and {MaxFrame}+1");

      if(index == MaxFrame + 1) {
        MaxFrame = index;

        if(MaxFrame - MinFrame > _maxSize - 1)
          MinFrame = MaxFrame - _maxSize + 1;
      }

      _items[index % _maxSize] = item;
    }

    public T GetItem(uint index) {
      if(index < MinFrame)
        throw new TimelineException($"Cannot get items older than {MinFrame}");
      if(index > MaxFrame)
        throw new TimelineException($"Cannot get items younger than {MaxFrame}");

      return _items[index % _maxSize];
    }
    
    public bool ContainsKey(uint index) {
      return index >= MinFrame && index <= MaxFrame;
    }

    public uint GetNearestLowerKey(uint index) {
      return MinFrame;
    }
  }
}