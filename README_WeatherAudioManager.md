# Weather Audio System (Unity 6 LTS)

## Architecture (Short)
- Profiles (`WeatherAudioProfile`) define loop layers and transients with mixer routing and curves.
- Manager (`WeatherAudioManager`) is a singleton driving layers by intensity and indoor/outdoor.
- Backend (`UnityAudioBackend`) uses pooled `AudioSource` and `AudioMixerSnapshot` transitions.
- Pool (`PooledAudioSource`) fades and reuses sources.
- Editor tools: profile inspector with preview; mixer creator.

## Files
- `Assets/Scripts/Audio/WeatherAudioProfile.cs`: ScriptableObject data (layers, transients, mixer snapshots).
- `Assets/Scripts/Audio/WeatherAudioManager.cs`: runtime manager (SetProfile/SetIntensity/TriggerTransient/... ).
- `Assets/Scripts/Audio/IWeatherAudioBackend.cs`: middleware abstraction.
- `Assets/Scripts/Audio/UnityAudioBackend.cs`: Unity implementation.
- `Assets/Scripts/Audio/PooledAudioSource.cs`: pooled sources.
- `Assets/Scripts/Audio/Editor/WeatherAudioProfileEditor.cs`: inspector + preview.
- `Assets/Editor/WeatherAudioMixerCreator.cs`: creates default mixer with snapshots.
- `Assets/Scripts/Audio/Tests/WeatherAudioManagerTests.cs`: unit tests.

## Setup
1) Create mixer via menu Tools/WeatherAudio/Create Default Mixer.
2) Create profile: Right-click Project → Create → Game → Audio → Weather Audio Profile.
   - Assign `targetMixer`, `outdoorSnapshot`, `indoorSnapshot` (optional but recommended).
   - Configure layers (assign clips, groups, curves, crossfade).
   - Configure transients (clips, groups, cooldown).
3) Place `WeatherAudioManager` in scene (or it will auto-spawn at runtime).
4) Drive from code:
```csharp
using Game.Audio;
public class Example : UnityEngine.MonoBehaviour {
    public WeatherAudioProfile heavyRain;
    void Start(){
        var m = WeatherAudioManager.Instance;
        m.SetProfile(heavyRain, 2f);
        m.SetIntensity(0.8f);
    }
}
```

## External Weather Adapter
Call from your system when weather changes:
```csharp
WeatherAudioManager.Instance.SetIntensity(rainIntensity01);
WeatherAudioManager.Instance.SetIndoor(isIndoor);
```

## Addressables & Middleware
- Addressables: assign clips normally or via custom loader before setting profile.
- FMOD/Wwise: implement `IWeatherAudioBackend` alternative backend; swap in manager construction.

## Acceptance Criteria
- Smooth profile crossfades; intensity changes click-free.
- Transients play via pool without interrupting loops.
- Editor preview audible; snapshots switch indoor/outdoor.
- Unit tests pass in PlayMode.

## Optimization & Mobile
- Limit `maxSimultaneousSources`; prefer compressed streaming for long loops.
- Use 2D for global ambience; 3D only when needed.
- Avoid heavy reallocation; pool provided.

## QA Checklist
- Switch between light→heavy→storm profiles.
- Ramp intensity 0→1→0 within 5s.
- Spam 50 transients → no errors, capped by pool.
- Indoor toggle switches snapshots smoothly.
