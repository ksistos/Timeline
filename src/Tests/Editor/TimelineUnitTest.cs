using System;
using NUnit.Framework;

namespace Timeline.Tests {
  public class TimelineTest {
    public static Timeline<DummyState, DummyCommand> GetTimeline() {
      return new Timeline<DummyState, DummyCommand>(new DummyState {Value = 0},
        100, (ref DummyState state) => {
          state.Value += 1;
        },
        (ref DummyState state, DummyCommand cmd) => {
          switch(cmd.Op) {
            case DummyCommand.OperationEnum.Mult:
              state.Value *= cmd.Value;
              break;
            case DummyCommand.OperationEnum.Div:
              state.Value /= cmd.Value;
              break;
            case DummyCommand.OperationEnum.Sum:
              state.Value += cmd.Value;
              break;
            case DummyCommand.OperationEnum.Subs:
              state.Value -= cmd.Value;
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      );
    }

    [Test]
    public void TimelineCanBeAdvanced() {
      var tl = GetTimeline();
      tl.Advance();
      Assert.AreEqual(1, tl.CurrentState.Value);
      tl.Advance();
      Assert.AreEqual(2, tl.CurrentState.Value);
      Assert.AreEqual(2u, tl.CurrentFrame);
    }

    [Test]
    public void TimelineCanRewindBackOneStep() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      tl.Rewind(1);
      Assert.AreEqual(1, tl.CurrentState.Value);
      Assert.AreEqual(1u, tl.CurrentFrame);
    }

    [Test]
    public void TimelineCanRewindBackMultipleTimes() {
      var tl = GetTimeline();
      for(var i = 0; i < 21; i++)
        tl.Advance();
      tl.Rewind(19);
      Assert.AreEqual(19, tl.CurrentState.Value);
      Assert.AreEqual(19u, tl.CurrentFrame);
      tl.Rewind(18);
      Assert.AreEqual(18, tl.CurrentState.Value);
      Assert.AreEqual(18u, tl.CurrentFrame);
      tl.Rewind(17);
      Assert.AreEqual(17, tl.CurrentState.Value);
      Assert.AreEqual(17u, tl.CurrentFrame);
    }

    [Test]
    public void TimelineCantRewindForward() {
      var tl = GetTimeline();
      for(var i = 0; i < 21; i++)
        tl.Advance();
      tl.Rewind(2);
      Assert.Throws<TimelineException>(() => tl.Rewind(3));
    }

    [Test]
    public void TimelineCanAdvanceAfterRewind() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      tl.Rewind(1);
      tl.Advance();
      Assert.AreEqual(2, tl.CurrentState.Value);
      Assert.AreEqual(2u, tl.CurrentFrame);
    }

    [Test]
    public void TimelineCanAdvanceAndRewind() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      tl.Rewind(1);
      tl.Rewind(0);
      tl.Advance();
      tl.Advance();
      tl.Advance();
      tl.Rewind(2);
      Assert.AreEqual(2, tl.CurrentState.Value);
      Assert.AreEqual(2u, tl.CurrentFrame);
    }

    [Test]
    public void TimelineCannotRewindIntoNegative() {
      var tl = GetTimeline();
      tl.Advance();
      Assert.Throws<TimelineException>(() => {
        tl.Rewind(unchecked((uint)-1));
      });
    }

    [Test]
    public void TimelineCanRewindToZeroFrame() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Rewind(0);
      Assert.AreEqual(0, tl.CurrentState.Value);
      Assert.AreEqual(0u, tl.CurrentFrame);
    }

    [Test]
    public void TimelineCanRegisterAndApplyCommands() {
      var tl = GetTimeline();
      tl.RegisterCommand(2, new DummyCommand {Value = 10});

      tl.Advance();
      tl.Advance();
      Assert.AreEqual(12, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineCanRegisterCommandsOnThisFrame() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      Assert.AreEqual(2, tl.CurrentState.Value);
      tl.RegisterCommand(2, new DummyCommand {Value = 10});
      Assert.AreEqual(2, tl.CurrentState.Value);
      tl.Rewind(1);
      tl.Advance();
      Assert.AreEqual(12, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineModifiersRegisterOnlyOnce() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      tl.RegisterCommand(2, new DummyCommand {Value = 10});
      tl.RegisterCommand(2, new DummyCommand {Value = 10});
      Assert.AreEqual(2, tl.CurrentState.Value);
      tl.Rewind(1);
      tl.Advance();
      Assert.AreEqual(12, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineCanNotModifyZeroStep() {
      var tl = GetTimeline();
      tl.Advance();
      tl.RegisterCommand(0, new DummyCommand {Value = 10});
      tl.Rewind(0);
      tl.Advance();
      Assert.AreEqual(1, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineCanRewriteHistory() {
      var tl = GetTimeline();
      tl.Advance();
      Assert.AreEqual(1, tl.CurrentState.Value);
      tl.RegisterCommand(1, new DummyCommand {Value = 10});
      tl.Rewind(0);
      tl.Advance();
      Assert.AreEqual(11, tl.CurrentState.Value);
      tl.RegisterCommand(1, new DummyCommand {Value = 99});
      tl.Rewind(0);
      tl.Advance();
      Assert.AreEqual(100, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineCanModifyAll() {
      var tl = GetTimeline();
      tl.Advance();
      Assert.AreEqual(1, tl.CurrentState.Value);
      tl.RegisterCommand(2, new DummyCommand {Value = 10});
      tl.Advance();
      Assert.AreEqual(12, tl.CurrentState.Value);
      tl.RegisterCommand(3, new DummyCommand {Value = 100});
      tl.Advance();
      tl.Advance();
      tl.Advance();
      Assert.AreEqual(115, tl.CurrentState.Value);
      tl.RegisterCommand(4, new DummyCommand {Value = 1000});
      tl.Rewind(0);
      tl.Advance();
      tl.Advance();
      tl.Advance();
      tl.Advance();
      tl.Advance();
      Assert.AreEqual(1115, tl.CurrentState.Value);
      tl.Rewind(1);
      tl.Advance();
      tl.Advance();
      tl.Advance();
      tl.Advance();
      Assert.AreEqual(1115, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineCanNotifyAboutFrameInvalidation() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      tl.Advance();
      Assert.AreEqual(3u, tl.OldestNewCommandFrame);
      tl.RegisterCommand(3, new DummyCommand {Value = 1000});
      Assert.AreEqual(3, tl.CurrentState.Value);
      Assert.IsTrue(tl.NeedRewind);
      Assert.AreEqual(2u, tl.OldestNewCommandFrame);
    }

    [Test]
    public void TimelineCanAdvanceIfNeedRewind() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      tl.Advance();
      Assert.AreEqual(3u, tl.OldestNewCommandFrame);
      tl.RegisterCommand(3, new DummyCommand {Value = 1000});
      Assert.AreEqual(3, tl.CurrentState.Value);
      Assert.IsTrue(tl.NeedRewind);
      Assert.AreEqual(2u, tl.OldestNewCommandFrame);
      tl.Advance();
      Assert.AreEqual(1004, tl.CurrentState.Value);
      Assert.AreEqual(4, tl.CurrentFrame);
    }

    [Test]
    public void TimelineCanBeFastForwarded() {
      var tl = GetTimeline();
      tl.FastForward(20);
      Assert.AreEqual(20, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineHasModifierOrderRight() {
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      tl.RegisterCommand(3, new DummyCommand {Value = 3, Op = DummyCommand.OperationEnum.Mult});
      tl.RegisterCommand(3, new DummyCommand {Value = 4});
      tl.Advance();
      Assert.AreEqual(11, tl.CurrentState.Value);
    }

    [Test]
    public void TimelineReportsLastReceivedFrame() {
      var tl = GetTimeline();
      tl.RegisterCommand(2, new DummyCommand {Value = 4});
      Assert.AreEqual(2u, tl.LatestFrame);
    }

    [Test]
    public void TimelineReportsLastReceivedFrameIfRegisterNotInOrder() {
      var tl = GetTimeline();
      tl.RegisterCommand(2, new DummyCommand {Value = 4});
      tl.RegisterCommand(3, new DummyCommand {Value = 4});
      tl.RegisterCommand(1, new DummyCommand {Value = 4});
      Assert.AreEqual(3u, tl.LatestFrame);
    }

    [Test]
    public void TimelineCanBeRewindedAndFfConstantly() {
      var rewindCount = 0;
      var tl = GetTimeline();
      tl.Advance();
      tl.Advance();
      for(uint i = 0; i < 10000; i++) {
        tl.RegisterCommand(i + 1u, new DummyCommand {Value = 1});
        if(tl.NeedRewind)
          rewindCount++;

        tl.Advance();
      }

      Assert.AreEqual(10002u, tl.CurrentFrame);
      Assert.AreEqual(20002, tl.CurrentState.Value);
      Assert.AreEqual(10000, rewindCount);
    }

    public class DummyState : ICloneable {
      public int Value;

      public object Clone() {
        return new DummyState {Value = Value};
      }
    }

    public class DummyCommand : IComparable {
      public enum OperationEnum {
        Mult = 0,
        Div = 1,
        Sum = 2,
        Subs = 3
      }

      public OperationEnum Op = OperationEnum.Sum;

      public int Value;

      public int CompareTo(object obj) {
        if(!(obj is DummyCommand))
          return -1;
        if(Op < ((DummyCommand)obj).Op)
          return -1;
        if(Op > ((DummyCommand)obj).Op)
          return 1;
        return 0;
      }
    }
  }
}