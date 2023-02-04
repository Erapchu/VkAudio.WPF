using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkAudio.WPF.Helpers;

namespace VkAudio.WPF.Services
{
    public interface IM3U8ToMP3Service
    {
        Task<Stream> Convert(string m3u8Content, CancellationToken cancellationToken);
    }

    internal class M3U8ToMP3Service : IM3U8ToMP3Service
    {
        public const string EXT_X_KEY = "#EXT-X-KEY:";
        public const string EXT_X_MEDIA_SEQUENCE = "#EXT-X-MEDIA-SEQUENCE:";
        public const string EXTINF = "#EXTINF";
        public const string METHOD = "METHOD=";
        public const string Crypto_AES128 = "AES-128";
        public const string Crypto_NONE = "NONE";
        public const string URI = "URI=";

        private readonly IHttpClientFactory _httpClientFactory;

        public M3U8ToMP3Service(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Stream> Convert(string m3u8Content, CancellationToken cancellationToken)
        {
            int extXKeyIndex = m3u8Content.IndexOf(EXT_X_KEY);

            // Determine media sequence
            var extXMediaSequence = 1; // By default
            var extXMediaSequenceIndex = m3u8Content.IndexOf(EXT_X_MEDIA_SEQUENCE);
            if (extXMediaSequenceIndex != -1)
            {
                var extXMediaSequenceCaret = m3u8Content.IndexOf('\n', extXMediaSequenceIndex);
                if (extXMediaSequenceCaret != -1)
                {
                    var s = m3u8Content.Substring(
                        extXMediaSequenceIndex + EXT_X_MEDIA_SEQUENCE.Length,
                        extXMediaSequenceCaret - extXMediaSequenceIndex - EXT_X_MEDIA_SEQUENCE.Length);
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
                var batchInfo = m3u8Content.Substring(previousExtXKeyIndex, extXKeyIndex - previousExtXKeyIndex);
                var extInfIndex = batchInfo.IndexOf(EXTINF);
                var segments = new Dictionary<string, int>();

                while (extInfIndex != -1)
                {
                    var segStartIndex = batchInfo.IndexOf('\n', extInfIndex);
                    var segName = batchInfo[segStartIndex..].Trim();
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
                                var stringKey = s.Value.ToString();
                                while (stringKey.Length != 64)
                                {
                                    stringKey += "0";
                                }
                                var keyBytes = System.Convert.FromHexString(stringKey);
                                var publicKeyCopy = publicKey;
                                while (publicKeyCopy.Length != 32)
                                {
                                    publicKeyCopy += "0";
                                }
                                var ivBytes = System.Convert.FromHexString(publicKeyCopy);
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
