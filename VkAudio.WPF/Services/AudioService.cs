using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VkAudio.WPF.Services
{
    public interface IAudioService
    {
        Task Download(string url, CancellationToken cancellationToken = default);
    }

    internal class AudioService : IAudioService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IM3U8ToMP3Service _m3U8ToMP3Service;

        public AudioService(
            IHttpClientFactory httpClientFactory,
            IM3U8ToMP3Service m3U8ToMP3Service)
        {
            _httpClientFactory = httpClientFactory;
            _m3U8ToMP3Service = m3U8ToMP3Service;
        }

        public async Task Download(string url, CancellationToken cancellationToken)
        {
            var content = await DownloadM3U8(url, cancellationToken);
            await _m3U8ToMP3Service.Convert(content, cancellationToken);
        }

        private async Task<string> DownloadM3U8(string url, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
