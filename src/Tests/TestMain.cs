using System;

namespace Timeline.Tests {
  public static class TestMain {
    private static void Main() {
      var tl = TimelineTest.GetTimeline();
      for(var i = 0; i < 21; i++)
        tl.Advance();
      tl.Rewind(19);
      Console.WriteLine($"{tl.CurrentState.Value}");
    }
  }
}