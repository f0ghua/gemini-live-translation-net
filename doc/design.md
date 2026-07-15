# Design

## Runtime flow

```text
WPF HUD / tray
  -> AppController
  -> GeminiLiveClient
  -> ClientWebSocket
  -> Gemini Live API

AudioCaptureService
  -> NAudio WASAPI loopback or mic
  -> Pcm16Processor
  -> Pcm16Chunker
  -> GeminiLiveClient.SendAudio()

GeminiLiveClient
  -> transcript events
  -> HUD text
  -> output PCM16 audio
  -> AudioPlaybackService
```

## Modules

### `Ui`

- `HudWindow`: transparent topmost subtitle window.
- `SettingsWindow`: editable API, model, language, proxy, audio, and HUD settings.
- `AppController`: orchestrates tray menu, start/stop lifecycle, settings persistence, audio capture, playback, and Gemini events.

### `Settings`

- `AppSettings`: serializable runtime settings.
- `SettingsStore`: JSON load/save under `%APPDATA%`.

### `Gemini`

- `GeminiLiveClient`: stateful WebSocket client for Gemini Live Translate.
- `GeminiSessionOptions`: immutable session configuration.

Responsibilities:

- Build WebSocket URL from API base.
- Send setup message.
- Send 16 kHz mono PCM16 audio chunks.
- Parse input/output transcripts.
- Parse returned PCM16 audio.
- Reconnect with bounded exponential delay.
- Drop audio chunks under send backpressure.

### `Audio`

- `AudioCaptureService`: starts/stops WASAPI loopback or microphone capture.
- `Pcm16Processor`: converts captured bytes into 16 kHz mono PCM16.
- `Pcm16Chunker`: emits fixed 3200-byte chunks, matching the Python app.
- `AudioPlaybackService`: plays returned 24 kHz PCM16 through NAudio.

## Current limitations

- No transcript export yet.
- No DPAPI protection for API keys yet.
- Resampling uses linear interpolation. This is buildable and low-cost, but not final quality.
- Audio device selection is minimal: default system loopback or microphone device number.
- No installer; publish output is the first distribution unit.
- Proxy settings currently use .NET `WebProxy`, so the settings UI treats `host:port` as HTTP proxy syntax. SOCKS proxy support is not implemented yet.

## Build

Remote workspace:

```text
D:\work\ai\dev\gemini-live-translate-dotnet
```

Use the workspace-local SDK:

```powershell
$env:DOTNET_ROOT='D:\work\ai\dev\gemini-live-translate-dotnet\.dotnet'
$env:PATH="$env:DOTNET_ROOT;$env:PATH"
dotnet build .\GeminiLiveTranslate.sln
```

Publish:

```powershell
dotnet publish .\GeminiLiveTranslate\GeminiLiveTranslate.csproj -c Release -r win-x64 --self-contained false
```
