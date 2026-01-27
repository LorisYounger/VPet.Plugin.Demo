using System;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VPet.Plugin.VPetTTS
{
    /// <summary>
    /// EdgeTTS Client with proper authentication
    /// Based on: https://github.com/rany2/edge-tts
    /// </summary>
    public class EdgeTTSClient : IDisposable
    {
        // Constants from edge-tts constants.py
        private const string TrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";
        private const string ChromiumVersion = "143.0.3650.75";
        private const string SecMsGecVersion = "1-" + ChromiumVersion;
        private const string WssUrlBase = "wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1";
        private static readonly string UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/{ChromiumVersion} Safari/537.36 Edg/{ChromiumVersion}";

        public EdgeTTSClient()
        {
        }

        /// <summary>
        /// Synthesize speech from text
        /// </summary>
        /// <param name="text">Text to synthesize</param>
        /// <param name="voice">Voice name (e.g., zh-CN-XiaoxiaoNeural)</param>
        /// <param name="pitch">Pitch adjustment (e.g., +0Hz)</param>
        /// <param name="rate">Rate adjustment (e.g., +0%)</param>
        /// <returns>Result containing audio data</returns>
        public async Task<EdgeTTSResult> SynthesisAsync(string text, string voice, string pitch = "+0Hz", string rate = "+0%")
        {
            try
            {
                var audioData = new MemoryStream();
                
                // Generate required parameters
                string connectionId = Guid.NewGuid().ToString("N");
                string secMsGec = GenerateSecMsGec();
                string muid = Guid.NewGuid().ToString("N").ToUpper();

                // Construct WebSocket URL with authentication parameters
                string url = $"{WssUrlBase}?TrustedClientToken={TrustedClientToken}&ConnectionId={connectionId}&Sec-MS-GEC={secMsGec}&Sec-MS-GEC-Version={SecMsGecVersion}";

                using (var ws = new ClientWebSocket())
                {
                    // Set request headers (from constants.py headers)
                    ws.Options.SetRequestHeader("User-Agent", UserAgent);
                    ws.Options.SetRequestHeader("Pragma", "no-cache");
                    ws.Options.SetRequestHeader("Cache-Control", "no-cache");
                    ws.Options.SetRequestHeader("Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold");
                    ws.Options.SetRequestHeader("Accept-Encoding", "gzip, deflate, br, zstd");
                    ws.Options.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
                    
                    // [CRITICAL] Add Cookie: muid header
                    ws.Options.SetRequestHeader("Cookie", $"muid={muid};");

                    // Connect to WebSocket
                    await ws.ConnectAsync(new Uri(url), CancellationToken.None);

                    // Send speech configuration
                    string dateString = GetJsDateString();
                    string configMsg =
                        $"X-Timestamp:{dateString}\r\n" +
                        "Content-Type:application/json; charset=utf-8\r\n" +
                        "Path:speech.config\r\n\r\n" +
                        "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{" +
                        "\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"false\"}," +
                        "\"outputFormat\":\"audio-24khz-48kbitrate-mono-mp3\"" +
                        "}}}}";

                    await SendMessageAsync(ws, configMsg);

                    // Send SSML request
                    string ssml =
                        $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>" +
                        $"<voice name='{voice}'><prosody pitch='{pitch}' rate='{rate}' volume='+0%'>" +
                        $"{System.Security.SecurityElement.Escape(text)}" +
                        $"</prosody></voice></speak>";

                    string ssmlMsg =
                        $"X-RequestId:{connectionId}\r\n" +
                        "Content-Type:application/ssml+xml\r\n" +
                        $"X-Timestamp:{dateString}Z\r\n" +
                        "Path:ssml\r\n\r\n" +
                        $"{ssml}";

                    await SendMessageAsync(ws, ssmlMsg);

                    // Receive and process audio stream
                    var buffer = new byte[8192]; // 8KB buffer
                    bool receivedAudio = false;

                    while (ws.State == WebSocketState.Open)
                    {
                        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
                            break;
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            if (msg.Contains("turn.end"))
                            {
                                break;
                            }
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            // Parse binary protocol [HeaderLength(2bytes) | Headers | Data]
                            if (result.Count < 2) continue;

                            // Read header length (big-endian)
                            int headerLen = (buffer[0] << 8) | buffer[1];

                            // Data starts at: 2 (length bytes) + headerLen
                            int dataOffset = 2 + headerLen;

                            if (result.Count > dataOffset)
                            {
                                // Write audio data to stream
                                await audioData.WriteAsync(buffer, dataOffset, result.Count - dataOffset);
                                receivedAudio = true;
                            }
                        }
                    }

                    if (!receivedAudio || audioData.Length == 0)
                    {
                        return new EdgeTTSResult
                        {
                            Code = ResultCode.Error,
                            Message = "No audio data received from server"
                        };
                    }

                    return new EdgeTTSResult
                    {
                        Code = ResultCode.Success,
                        Data = audioData,
                        Message = "Success"
                    };
                }
            }
            catch (Exception ex)
            {
                return new EdgeTTSResult
                {
                    Code = ResultCode.Error,
                    Message = $"Error: {ex.Message}",
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// Send text message to WebSocket
        /// </summary>
        private async Task SendMessageAsync(ClientWebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Generate Sec-MS-GEC token for authentication
        /// Based on drm.py algorithm
        /// </summary>
        private string GenerateSecMsGec()
        {
            // Windows Epoch: 11644473600 seconds
            const long WinEpochSeconds = 11644473600;

            // Get current Unix timestamp
            long unixTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Convert to Windows Epoch
            double ticks = unixTs + WinEpochSeconds;

            // Round down to nearest 5 minutes (300 seconds)
            ticks -= ticks % 300;

            // Convert to 100-nanosecond intervals
            ticks *= 10_000_000;

            // Create string to hash
            string strToHash = $"{ticks:F0}{TrustedClientToken}";

            // Compute SHA256 hash
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(strToHash);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash).ToUpper();
            }
        }

        /// <summary>
        /// Get JavaScript-style date string
        /// Format: Sat Jan 25 2026 14:00:00 GMT+0000 (Coordinated Universal Time)
        /// </summary>
        private string GetJsDateString()
        {
            return DateTime.UtcNow.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (Coordinated Universal Time)'", CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }

    /// <summary>
    /// Result code for EdgeTTS operations
    /// </summary>
    public enum ResultCode
    {
        Success,
        Error
    }

    /// <summary>
    /// Result of EdgeTTS synthesis operation
    /// </summary>
    public class EdgeTTSResult
    {
        public ResultCode Code { get; set; }
        public string Message { get; set; }
        public MemoryStream Data { get; set; }
        public Exception Exception { get; set; }
    }
}
