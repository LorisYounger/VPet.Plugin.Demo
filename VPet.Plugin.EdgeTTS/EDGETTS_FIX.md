# EdgeTTS Fix Documentation

## Overview

This fix implements a custom EdgeTTS client with proper authentication to resolve issues with Microsoft's Edge TTS service. The implementation is based on the reference Python project: https://github.com/rany2/edge-tts

## Changes Made

### 1. Created EdgeTTSClient.cs

A new custom implementation of EdgeTTS client that includes:

- **Proper Authentication**: Implements the Sec-MS-GEC token generation algorithm
- **Updated Constants**: 
  - TrustedClientToken: `6A5AA1D4EAFF4E9FB37E23D68491D6F4`
  - ChromiumVersion: `143.0.3650.75`
  - Proper WebSocket URL with authentication parameters
  
- **Correct Request Headers**:
  - User-Agent with current Chrome/Edge version
  - Cookie with MUID (Machine User ID)
  - Origin from Chrome extension
  - Proper cache control and encoding headers

- **Sec-MS-GEC Generation**:
  - Time-based token generation using SHA256
  - Windows Epoch conversion (11644473600 seconds)
  - 5-minute interval rounding for token validity

### 2. Removed Old Dependency

Removed the outdated `EdgeTTS.Framework` package (version 1.0.4) which was not properly authenticated with Microsoft's service.

### 3. Updated Plugin Files

- **EdgeTTS.cs**: Removed `using EdgeTTS;` statement, now uses custom implementation
- **winSetting.xaml.cs**: Removed external EdgeTTS dependency
- **VPet.Plugin.VPetTTS.csproj**: Removed EdgeTTS.Framework package reference

## Technical Details

### Authentication Flow

1. Generate connection ID (GUID)
2. Generate Sec-MS-GEC token (time-based SHA256 hash)
3. Generate MUID (Machine User ID)
4. Construct WebSocket URL with all authentication parameters
5. Set proper request headers including Cookie
6. Connect and send speech configuration
7. Send SSML request with text and voice parameters
8. Receive binary audio data and parse protocol

### Sec-MS-GEC Algorithm

```csharp
// Get current Unix timestamp
long unixTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

// Convert to Windows Epoch
double ticks = unixTs + 11644473600;

// Round down to nearest 5 minutes
ticks -= ticks % 300;

// Convert to 100-nanosecond intervals
ticks *= 10_000_000;

// Hash with TrustedClientToken
string strToHash = $"{ticks:F0}{TrustedClientToken}";
SHA256 hash = SHA256.ComputeHash(Encoding.ASCII.GetBytes(strToHash));
```

## Compatibility

- Targets .NET 8.0 Windows
- Compatible with VPet-Simulator.Windows.Interface 1.1.0.50
- No external EdgeTTS dependencies required

## Benefits

1. **Fixed Authentication**: Resolves 403 errors due to missing or incorrect authentication
2. **Self-Contained**: No dependency on external EdgeTTS packages
3. **Up-to-Date**: Uses current Chrome version and authentication mechanism
4. **Maintainable**: Code is based on well-documented edge-tts project

## Notes

- The TrustedClientToken and Chromium version are hardcoded based on the reference implementation
- These values may need to be updated in the future if Microsoft changes their API
- The 403 error hint about system time accuracy is still relevant - ensure system time is synchronized
