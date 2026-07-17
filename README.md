# Gemini Live Translate .NET

Windows 11 desktop client for Gemini Live Translate. It captures system audio or a microphone, sends 16 kHz PCM16 audio to Gemini Live, and displays live source and translated subtitles in a floating HUD.

## Download and Run

Open the GitHub Releases page and download one of the ZIP files:

- `GeminiLiveTranslate-win-x64.zip`
  - Smaller download.
  - Requires the .NET 8 Desktop Runtime to be installed on the Windows machine.
  - Extract the whole ZIP folder, then run `GeminiLiveTranslate.exe` from the extracted folder.

- `GeminiLiveTranslate-win-x64-self-contained.zip`
  - Larger download.
  - Does not require a separately installed .NET runtime.
  - Extract the whole ZIP folder, then run `GeminiLiveTranslate.exe` from the extracted folder.

Do not run the executable directly from inside the ZIP preview window. Extract the ZIP first so the executable can load its companion files correctly.

## First Use

1. Run `GeminiLiveTranslate.exe`.
2. Open `Settings`.
3. Enter a Gemini API key.
4. Choose the target language and audio source.
5. Click `Start`.

Settings are saved under:

```text
%APPDATA%\gemini-live-translate-dotnet\settings.json
```

## Proxy Setting

The `Proxy URL` field currently supports HTTP proxy syntax.

Examples:

```text
http://127.0.0.1:7890
sercomm.f0g.dev:2802
```

If only `host:port` is entered, it is treated as HTTP. SOCKS proxy support is not implemented yet.

## Build

Framework-dependent publish:

```powershell
dotnet publish .\GeminiLiveTranslate\GeminiLiveTranslate.csproj -c Release -r win-x64 --self-contained false
```

Self-contained publish:

```powershell
dotnet publish .\GeminiLiveTranslate\GeminiLiveTranslate.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## Release Workflow

Pushing a version tag such as `v0.1.0` triggers GitHub Actions to build and publish release ZIP files.
