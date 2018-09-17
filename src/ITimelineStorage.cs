namespace Timeline {
  public interface ITimelineStorage<T> {
    T this[uint index] { get; set; }

    void Store(uint index, T value);
    T GetItem(uint index);

    bool ContainsKey(uint index);
    uint GetNearestLowerKey(uint index);
  } 
}