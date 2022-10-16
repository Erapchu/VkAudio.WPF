using System.Collections.Generic;

namespace VkAudio.WPF.Models.Catalog.GetSection
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Action
    {
        public string type { get; set; }
    }

    public class Ads
    {
        public string content_id { get; set; }
        public string duration { get; set; }
        public string account_age_type { get; set; }
        public string puid1 { get; set; }
        public string puid22 { get; set; }
    }

    public class Album
    {
        public int id { get; set; }
        public string title { get; set; }
        public int owner_id { get; set; }
        public string access_key { get; set; }
        public Thumb thumb { get; set; }
    }

    public class Audio
    {
        public string artist { get; set; }
        public int id { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public string access_key { get; set; }
        public Ads ads { get; set; }
        public bool is_explicit { get; set; }
        public bool is_focus_track { get; set; }
        public bool is_licensed { get; set; }
        public string track_code { get; set; }
        public string url { get; set; }
        public int date { get; set; }
        public Album album { get; set; }
        public List<MainArtist> main_artists { get; set; }
        public bool short_videos_allowed { get; set; }
        public bool stories_allowed { get; set; }
        public bool stories_cover_allowed { get; set; }
        public List<FeaturedArtist> featured_artists { get; set; }
        public int? genre_id { get; set; }
        public string subtitle { get; set; }
        public int? no_search { get; set; }
        public int? content_restricted { get; set; }
        public int? lyrics_id { get; set; }
    }

    public class Block
    {
        public string id { get; set; }
        public string data_type { get; set; }
        public Layout layout { get; set; }
        public string next_from { get; set; }
        public List<string> listen_events { get; set; }
        public List<Button> buttons { get; set; }
        public List<string> audios_ids { get; set; }
    }

    public class Button
    {
        public Action action { get; set; }
        public int ref_items_count { get; set; }
        public string ref_layout_name { get; set; }
        public string ref_data_type { get; set; }
    }

    public class FeaturedArtist
    {
        public string name { get; set; }
        public string domain { get; set; }
        public string id { get; set; }
    }

    public class Layout
    {
        public string name { get; set; }
        public int owner_id { get; set; }
    }

    public class MainArtist
    {
        public string name { get; set; }
        public string domain { get; set; }
        public string id { get; set; }
    }

    public class Response
    {
        public Section section { get; set; }
        public List<object> albums { get; set; }
        public List<Audio> audios { get; set; }
    }

    public class GetSectionWrapper
    {
        public Response response { get; set; }
    }

    public class Section
    {
        public string id { get; set; }
        public string title { get; set; }
        public List<Block> blocks { get; set; }
        public string next_from { get; set; }
        public List<object> buttons { get; set; }
    }

    public class Thumb
    {
        public int width { get; set; }
        public int height { get; set; }
        public string photo_34 { get; set; }
        public string photo_68 { get; set; }
        public string photo_135 { get; set; }
        public string photo_270 { get; set; }
        public string photo_300 { get; set; }
        public string photo_600 { get; set; }
        public string photo_1200 { get; set; }
    }


}
