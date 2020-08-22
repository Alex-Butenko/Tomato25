using System;
using System.Threading;

namespace Tomato25 {
    static class OneProgramInstance {
        static readonly Mutex _mutex = new Mutex(true, "F0BFC810-85B0-4744-A290-4FC105F7F424");

        public static bool HasAnotherInstance =>
            !_mutex.WaitOne(TimeSpan.Zero, true);
    }
}