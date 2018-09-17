using System.Collections.Generic;
using NUnit.Framework;

namespace Timeline.Tests {
  public class TimelineStorageTest {
    [Test]
    public void TimelineStorageCanSaveItems() {
      var tls = new TimelineStorage<DState>(50) {[0] = new DState(5)};
      Assert.AreEqual(5, tls[0].Value);
    }

    [Test]
    public void TimelineCanSaveMaxItems() {
      var tls = new TimelineStorage<DState>(50);
      for(uint i = 0; i < 50; i++)
        tls[i] = new DState((int)i * 2);

      Assert.AreEqual(98, tls[49].Value);
      Assert.AreEqual(0, tls[0].Value);
    }

    [Test]
    public void TimelineCanSaveItemsAfterMax() {
      var tls = new TimelineStorage<DState>(5);
      for(uint i = 0; i < 11; i++)
        tls[i] = new DState((int)i);
      Assert.Throws<TimelineException>(() => {
        tls[0].Value = 0;
      });
    }

    [Test]
    public void TimelineCanRetrieveItemsInCurrentRange() {
      var tls = new TimelineStorage<DState>(5);
      for(uint i = 0; i < 11; i++)
        tls[i] = new DState((int)i);
      Assert.AreEqual(10, tls[10].Value);
      Assert.AreEqual(9, tls[9].Value);
      Assert.AreEqual(8, tls[8].Value);
      Assert.AreEqual(7, tls[7].Value);
      Assert.AreEqual(6, tls[6].Value);
      Assert.Throws<TimelineException>(() => {
        tls[5].Value = 0;
      });
      Assert.Throws<TimelineException>(() => {
        tls[12].Value = 12;
      });
      Assert.DoesNotThrow(() => {
        tls[11] = new DState(11);
      });
    }

    [Test]
    public void TimelineStorageRangeMoveAlong() {
      var tls = new TimelineStorage<DState>(2) {[0] = new DState(0), [1] = new DState(1)};
      Assert.AreEqual(0, tls[0].Value);
      tls[2] = new DState(2);
      Assert.Throws<TimelineException>(() => {
        tls[0].Value = 1;
      });
    }

    [Test]
    public void TimelineStorageHasStateTest() {
      var tls = new TimelineStorage<DState>(2) {[0] = new DState(0)};
      Assert.IsTrue(tls.ContainsKey(0));
      Assert.AreEqual(0, tls[0].Value);
    }

    private class DState {
      public int Value;

      public DState(int val) {
        Value = val;
      }
    }
  }
}