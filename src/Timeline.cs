using System;
using System.Collections.Generic;

namespace Timeline {
  public class Timeline<TStateType, TCommandType>
    where TStateType : ICloneable
    where TCommandType : IComparable {
    public delegate void AdvanceTimeAction(ref TStateType state);

    public delegate void ProcessCommandAction(ref TStateType state, TCommandType command);

    private readonly AdvanceTimeAction _advanceTimeAction;

    private readonly Dictionary<uint, List<TCommandType>> _commands;
    private readonly ITimelineStorage<TStateType> _history;
    private readonly ProcessCommandAction _processCommandAction;

    public Timeline(TStateType initialState, ITimelineStorage<TStateType> historyStorage,
      AdvanceTimeAction advanceTimeAction,
      ProcessCommandAction processCommandAction) {
      CurrentFrame = 0u;
      LatestFrame = 0u;

      OldestNewCommandFrame = 0;
      NeedRewind = false;

      _history = historyStorage;
      _history[CurrentFrame] = initialState;

      _advanceTimeAction = advanceTimeAction;
      _processCommandAction = processCommandAction;

      _commands = new Dictionary<uint, List<TCommandType>>();
      InitialState = initialState;
    }

    public Timeline(TStateType initialState, uint historySize, AdvanceTimeAction advanceTime,
      ProcessCommandAction processCommand) : this(initialState, new TimelineStorage<TStateType>(historySize),
      advanceTime, processCommand) {
    }

    public TStateType CurrentState => _history[CurrentFrame];
    public TStateType InitialState { get; }

    public uint LatestFrame { get; private set; }
    public uint CurrentFrame { get; private set; }
    public bool NeedRewind { get; private set; }
    public uint OldestNewCommandFrame { get; private set; }

    public void Advance() {
      if(NeedRewind) {
        var cf = CurrentFrame;

        Rewind(OldestNewCommandFrame);
        FastForward(cf);
      }

      var newState = (TStateType)CurrentState.Clone();

      newState = ModifyState(CurrentFrame + 1, newState);
      CurrentFrame++;

      StoreState(CurrentFrame, newState);

      OldestNewCommandFrame = CurrentFrame;
    }

    public void RegisterCommand(uint targetFrame, TCommandType command) {
      if(!_commands.ContainsKey(targetFrame))
        _commands[targetFrame] = new List<TCommandType>();

      _commands[targetFrame].RemoveAll(c => c.CompareTo(command) == 0);
      _commands[targetFrame].Add(command);
      _commands[targetFrame].Sort();

      if(targetFrame > LatestFrame)
        LatestFrame = targetFrame;
      if(targetFrame <= CurrentFrame)
        SetNeedRewind(targetFrame - 1);
    }

    public void Rewind(uint frame) {
      if(frame >= CurrentFrame)
        throw new TimelineException("Cannot rewind forward in time");
      if(_history.GetNearestOrLowerKey(frame) > frame)
        throw new TimelineException("Cannot rewind so far back in time");

      CurrentFrame = _history.GetNearestOrLowerKey(frame);

      NeedRewind = false;
    }

    public void FastForward(uint targetFrame) {
      while(targetFrame > CurrentFrame)
        Advance();
    }

    private void SetNeedRewind(uint frame) {
      NeedRewind = true;
      if(OldestNewCommandFrame > frame)
        OldestNewCommandFrame = frame;
    }

    private TStateType ModifyState(uint newFrame, TStateType state) {
      var modifiedState = state;
      if(_commands.ContainsKey(newFrame))
        foreach(var command in _commands[newFrame])
          _processCommandAction(ref modifiedState, command);

      _advanceTimeAction(ref modifiedState);

      return modifiedState;
    }

    private void StoreState(uint newFrame, TStateType newState) {
      _history[newFrame] = newState;
    }
  }
}