using System;
using UnityEngine.PlaymodeTests;

namespace Controls
{
    public static class ControlUtils
    {
        public static void SetPlayerSpeed(double xDir, double yDir)
        {
            ReduceSpeed(xDir);
            ReduceSpeed(yDir);
        }
        [Test]
        private static void ReduceSpeed(double dir)
        {
            if (Math.Abs(dir) < 0) return;
            if (dir > 0) dir = 1;
            if (dir < 0) dir = -1;
        }
    }
}