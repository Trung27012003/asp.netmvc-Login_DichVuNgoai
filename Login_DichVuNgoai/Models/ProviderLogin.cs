using System.ComponentModel.DataAnnotations;

namespace Login_DichVuNgoai.Models
{
    public class ProviderLogin
    {
        [Key]
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public ICollection<ExternalLogin> ExternalLogin { get; set; }

    }
}
