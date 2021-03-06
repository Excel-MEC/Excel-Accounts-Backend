using System.ComponentModel.DataAnnotations;
using API.Models.Custom;

namespace API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Picture { get; set; }
        public string QRCodeUrl { get; set; }
        public int? InstitutionId { get; set; }
        public string Gender { get; set; }
        public string MobileNumber { get; set; }
        public int CategoryId { get; set; }
        public string Category => Constants.Category[CategoryId];
        public Ambassador Ambassador { get; set; }
        public int? ReferrerAmbassadorId { get; set; }
        public Ambassador Referrer { get; set; }
        public bool IsPaid { get; set; }
    }
}