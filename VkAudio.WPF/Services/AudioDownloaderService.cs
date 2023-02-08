using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkAudio.WPF.Helpers;

namespace VkAudio.WPF.Services
{
    public interface IAudioDownloaderService
    {
        Task DownloadMP3(string m3u8Url, CancellationToken cancellationToken = default);
    }

    internal class AudioDownloaderService : IAudioDownloaderService
    {
        public const string EXT_X_KEY = "#EXT-X-KEY:";
        public const string EXT_X_MEDIA_SEQUENCE = "#EXT-X-MEDIA-SEQUENCE:";
        public const string EXTINF = "#EXTINF";
        public const string METHOD = "METHOD=";
        public const string Crypto_AES128 = "AES-128";
        public const string Crypto_NONE = "NONE";
        public const string URI = "URI=";

        private readonly IHttpClientFactory _httpClientFactory;

        public AudioDownloaderService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task DownloadMP3(string m3u8Url, CancellationToken cancellationToken)
        {
            var content = await DownloadM3U8Content(m3u8Url, cancellationToken);
            var mp3Stream = await ConvertToMP3(content, cancellationToken);
        }

        private async Task<string> DownloadM3U8Content(string m3u8Url, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, m3u8Url);
            using var response = await client.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async Task<Stream> ConvertToMP3(string m3u8Content, CancellationToken cancellationToken)
        {
            int extXKeyIndex = m3u8Content.IndexOf(EXT_X_KEY);

            // Determine media sequence
            var extXMediaSequence = 1; // By default
            var extXMediaSequenceIndexStart = m3u8Content.IndexOf(EXT_X_MEDIA_SEQUENCE);
            if (extXMediaSequenceIndexStart != -1)
            {
                var extXMediaSequenceIndexEnd = m3u8Content.IndexOf('\n', extXMediaSequenceIndexStart);
                if (extXMediaSequenceIndexEnd != -1)
                {
                    var s = m3u8Content.Substring(
                        extXMediaSequenceIndexStart + EXT_X_MEDIA_SEQUENCE.Length,
                        extXMediaSequenceIndexEnd - extXMediaSequenceIndexStart - EXT_X_MEDIA_SEQUENCE.Length);
                    int.TryParse(s, out extXMediaSequence);
                }
            }

            var streams = new ConcurrentDictionary<string, Stream>();
            string parentUrl = null;

            while (extXKeyIndex != -1) // Batch started
            {
                var caretReturnIndex = m3u8Content.IndexOf('\n', extXKeyIndex);
                var extXKeyValue = m3u8Content[extXKeyIndex..caretReturnIndex];
                var methodIndex = extXKeyValue.IndexOf(METHOD);
                string publicKey = null;

                // Determine cryptography
                if (methodIndex != -1)
                {
                    var methodNameEndIndex = extXKeyValue.IndexOf(',');
                    if (methodNameEndIndex == -1)
                    {
                        // We know just method name
                        methodNameEndIndex = extXKeyValue.Length;
                    }

                    string cryptoMethodName = extXKeyValue[(methodIndex + METHOD.Length)..methodNameEndIndex];
                    if (cryptoMethodName == Crypto_AES128)
                    {
                        // AES-128
                        var uriIndex = extXKeyValue.IndexOf(URI);
                        if (uriIndex != -1)
                        {
                            var fQuote = extXKeyValue.IndexOf('"', uriIndex);
                            var sQuote = extXKeyValue.IndexOf('"', fQuote + 1);
                            var publicKeyUri = extXKeyValue[(fQuote + 1)..sQuote];

                            if (parentUrl is null)
                            {
                                var lastSegmentIndex = publicKeyUri.LastIndexOf('/');
                                parentUrl = publicKeyUri[..lastSegmentIndex];
                            }

                            var client = _httpClientFactory.CreateClient();
                            using var request = new HttpRequestMessage(HttpMethod.Get, publicKeyUri);
                            using var response = await client.SendAsync(request, cancellationToken);
                            publicKey = await response.Content.ReadAsStringAsync(cancellationToken);
                        }
                    }
                }

                var previousExtXKeyIndex = extXKeyIndex;
                extXKeyIndex = m3u8Content.IndexOf(EXT_X_KEY, extXKeyIndex + 1); // Following batch

                // .ts
                string batchInfo = string.Empty;
                if (extXKeyIndex < 0)
                    batchInfo = m3u8Content.Substring(previousExtXKeyIndex);
                else
                    batchInfo = m3u8Content.Substring(previousExtXKeyIndex, extXKeyIndex - previousExtXKeyIndex);

                var extInfIndex = batchInfo.IndexOf(EXTINF);
                var segments = new Dictionary<string, int>();

                while (extInfIndex != -1)
                {
                    var segStartIndex = batchInfo.IndexOf('\n', extInfIndex);
                    var segEndIndex = batchInfo.IndexOf('\n', segStartIndex + 1);
                    var segName = batchInfo[segStartIndex..segEndIndex].Trim();
                    segments.TryAdd(segName, extXMediaSequence++);
                    extInfIndex = batchInfo.IndexOf(EXTINF, extInfIndex + 1); // Following segment in batch
                }

                await Parallel.ForEachAsync(segments, cancellationToken, async (s, cts) =>
                {
                    var client = _httpClientFactory.CreateClient();
                    var segmentUrl = parentUrl + '/' + s.Key;
                    using (var request = new HttpRequestMessage(HttpMethod.Get, segmentUrl))
                    {
                        using (var response = await client.SendAsync(request, cancellationToken))
                        {
                            if (publicKey != null)
                            {
                                var encryptedStream = response.Content.ReadAsStream(cts);
                                var latestByte = System.Convert.ToByte(s.Value);
                                var ivBytes = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, latestByte };
                                var publicKeyCopy = publicKey;
                                var keyBytes = Encoding.ASCII.GetBytes(publicKeyCopy);
                                var decryptedStream = AesCryptographyHelper.DecryptStream(encryptedStream, keyBytes, ivBytes);
                                streams.TryAdd(s.Key, decryptedStream);
                            }
                            else
                            {
                                var ms = new MemoryStream();
                                var rms = response.Content.ReadAsStream();
                                rms.CopyTo(ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                streams.TryAdd(s.Key, ms);
                            }
                        }
                    }
                });
            }

            using var file = File.Open(@"C:\Users\Andre\Desktop\test.mp3", FileMode.OpenOrCreate);
            file.Seek(0, SeekOrigin.Begin);
            foreach (var stream in streams)
            {
                stream.Value.CopyTo(file);
            }

            return null;
        }
    }
}
