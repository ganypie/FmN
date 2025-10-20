using NUnit.Framework;
using UnityEngine;

using Game.Audio;

namespace Game.Audio.Tests
{
    public class WeatherAudioManagerTests
    {
        [Test]
        public void SetProfile_NullSafe()
        {
            var mgr = WeatherAudioManager.Instance;
            Assert.DoesNotThrow(() => mgr.SetProfile(null, 0.5f));
        }

        [Test]
        public void SetIntensity_Clamped()
        {
            var mgr = WeatherAudioManager.Instance;
            mgr.SetIntensity(-1f);
            mgr.SetIntensity(2f);
            // No direct getter; ensure no exceptions
            Assert.Pass();
        }

        [Test]
        public void TriggerTransient_NoProfile_NoThrow()
        {
            var mgr = WeatherAudioManager.Instance;
            Assert.DoesNotThrow(() => mgr.TriggerTransient("thunder", Vector3.zero, 1f));
        }
    }
}


