using System.Collections.Generic;

namespace VkAudio.WPF.Models
{
    public class Action
    {
        public string type { get; set; }
        public string target { get; set; }
        public string url { get; set; }
        public string consume_reason { get; set; }
    }

    public class Action3
    {
        public string type { get; set; }
        public string location { get; set; }
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
        public int genre_id { get; set; }
        public bool short_videos_allowed { get; set; }
        public bool stories_allowed { get; set; }
        public bool stories_cover_allowed { get; set; }
        public Album album { get; set; }
        public List<MainArtist> main_artists { get; set; }
        public List<FeaturedArtist> featured_artists { get; set; }
        public int? no_search { get; set; }
        public string subtitle { get; set; }
        public int? lyrics_id { get; set; }
    }

    public class Badge
    {
        public string type { get; set; }
        public string text { get; set; }
    }

    public class Block
    {
        public string id { get; set; }
        public string data_type { get; set; }
        public Layout layout { get; set; }
        public List<int> catalog_banner_ids { get; set; }
        public List<string> listen_events { get; set; }
        public Badge badge { get; set; }
        public List<Button> buttons { get; set; }
        public string next_from { get; set; }
        public List<string> audios_ids { get; set; }
        public string url { get; set; }
        public List<string> playlists_ids { get; set; }
    }

    public class Button
    {
        public Action action { get; set; }
        public string section_id { get; set; }
        public string title { get; set; }
        public int ref_items_count { get; set; }
        public string ref_layout_name { get; set; }
        public string ref_data_type { get; set; }
    }

    public class Career
    {
        public int country_id { get; set; }
        public int group_id { get; set; }
    }

    public class Catalog
    {
        public string default_section { get; set; }
        public List<Section> sections { get; set; }
    }

    public class CatalogBanner
    {
        public int id { get; set; }
        public ClickAction click_action { get; set; }
        public List<Button> buttons { get; set; }
        public List<Image> images { get; set; }
        public string text { get; set; }
        public string title { get; set; }
        public string track_code { get; set; }
        public string image_mode { get; set; }
    }

    public class City
    {
        public int id { get; set; }
        public string title { get; set; }
    }

    public class ClickAction
    {
        public Action action { get; set; }
    }

    public class Country
    {
        public int id { get; set; }
        public string title { get; set; }
    }

    public class FeaturedArtist
    {
        public string name { get; set; }
        public string domain { get; set; }
        public string id { get; set; }
    }

    public class Group
    {
        public int id { get; set; }
        public int member_status { get; set; }
        public int verified { get; set; }
        public int members_count { get; set; }
        public string activity { get; set; }
        public int trending { get; set; }
        public string name { get; set; }
        public string screen_name { get; set; }
        public int is_closed { get; set; }
        public string type { get; set; }
        public int is_admin { get; set; }
        public int is_member { get; set; }
        public int is_advertiser { get; set; }
        public string photo_50 { get; set; }
        public string photo_100 { get; set; }
        public string photo_200 { get; set; }
    }

    public class Image
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Layout
    {
        public string name { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public TopTitle top_title { get; set; }
        public int? is_editable { get; set; }
    }

    public class MainArtist
    {
        public string name { get; set; }
        public string domain { get; set; }
        public string id { get; set; }
    }

    public class Meta
    {
        public string view { get; set; }
    }

    public class Permissions
    {
        public bool play { get; set; }
        public bool share { get; set; }
        public bool edit { get; set; }
        public bool follow { get; set; }
        public bool delete { get; set; }
        public bool boom_download { get; set; }
    }

    public class Photo
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

    public class Playlist
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public int type { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int count { get; set; }
        public int followers { get; set; }
        public int plays { get; set; }
        public int create_time { get; set; }
        public int update_time { get; set; }
        public List<object> genres { get; set; }
        public bool is_following { get; set; }
        public Permissions permissions { get; set; }
        public bool subtitle_badge { get; set; }
        public bool play_button { get; set; }
        public string access_key { get; set; }
        public string album_type { get; set; }
        public double match_score { get; set; }
        public List<Action> actions { get; set; }
        public Photo photo { get; set; }
        public string subtitle { get; set; }
        public Meta meta { get; set; }
    }

    public class Profile
    {
        public int id { get; set; }
        public string photo_200 { get; set; }
        public string activity { get; set; }
        public int followers_count { get; set; }
        public List<Career> career { get; set; }
        public int university { get; set; }
        public string university_name { get; set; }
        public int faculty { get; set; }
        public string faculty_name { get; set; }
        public int graduation { get; set; }
        public string screen_name { get; set; }
        public string photo_50 { get; set; }
        public string photo_100 { get; set; }
        public int verified { get; set; }
        public int trending { get; set; }
        public int friend_status { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public bool can_access_closed { get; set; }
        public bool is_closed { get; set; }
        public City city { get; set; }
        public Country country { get; set; }
    }

    public class RecommendedPlaylist
    {
        public int id { get; set; }
        public int owner_id { get; set; }
        public double percentage { get; set; }
        public string percentage_title { get; set; }
        public List<string> audios { get; set; }
        public string color { get; set; }
        public string cover { get; set; }
        public bool withOwner { get; set; }
    }

    public class Response
    {
        public Catalog catalog { get; set; }
        public List<Profile> profiles { get; set; }
        public List<Group> groups { get; set; }
        public List<object> albums { get; set; }
        public List<Audio> audios { get; set; }
        public List<RecommendedPlaylist> recommended_playlists { get; set; }
        public List<Playlist> playlists { get; set; }
        public List<CatalogBanner> catalog_banners { get; set; }
    }

    public class Root
    {
        public Response response { get; set; }
    }

    public class Section
    {
        public string id { get; set; }
        public string title { get; set; }
        public List<Block> blocks { get; set; }
        public string url { get; set; }
        public List<Button> buttons { get; set; }
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

    public class TopTitle
    {
        public string icon { get; set; }
        public string text { get; set; }
    }
}
