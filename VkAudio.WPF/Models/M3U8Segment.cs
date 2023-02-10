using System.Diagnostics;
using System.IO;

namespace VkAudio.WPF.Models
{
    [DebuggerDisplay("{MediaSequence}, {Name}, {Stream.Length}")]
    internal class M3U8Segment
    {
        public string Name { get; set; }
        public int MediaSequence { get; set; }
        public Stream Stream { get; set; }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
