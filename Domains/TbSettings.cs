using System.ComponentModel.DataAnnotations;

namespace lapshop.Domains
{
    public class TbSettings
    {
        [Key]
        public int SettingsId { get; set; }
        public string ContactEmail { get; set; }

        public string WebsiteName { get; set; }
        public string Logo { get; set; }
        public string WebsiteDescription { get; set; }
        public string FacebookLink { get; set; }
        public string TwitterLink { get; set; }
        public string InstagramLink { get; set; }
        public string YoutubeLink { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string MiddlePanner { get; set; }
        public string LastPanner { get; set; }
    }
}
