using System;
using System.IO;
using System.Net.Http;
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

            while (extXKeyIndex != -1) // Several batches
            {
                var caretReturnIndex = m3u8Content.IndexOf('\n', extXKeyIndex);
                var extXKeyValue = m3u8Content[extXKeyIndex..caretReturnIndex];
                var methodIndex = extXKeyValue.IndexOf(METHOD);
                string publicKey = null;
                string parentUri = null;

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

                            if (parentUri is null)
                            {
                                var lastSegmentIndex = publicKeyUri.LastIndexOf('/');
                                parentUri = publicKeyUri[..lastSegmentIndex];
                            }

                            var client = _httpClientFactory.CreateClient();
                            using var request = new HttpRequestMessage(HttpMethod.Get, publicKeyUri);
                            using var response = await client.SendAsync(request, cancellationToken);
                            publicKey = await response.Content.ReadAsStringAsync(cancellationToken);
                        }
                    }
                }

                // .ts


                extXKeyIndex = m3u8Content.IndexOf(EXT_X_KEY, extXKeyIndex + 1); // Following batch
            }

            return null;
        }
    }
}
