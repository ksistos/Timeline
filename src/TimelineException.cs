using System;

namespace Timeline {
  public class TimelineException : Exception {
    public TimelineException(string message) : base(message) {
    }
  }
}