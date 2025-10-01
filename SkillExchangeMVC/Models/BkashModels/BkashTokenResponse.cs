//BkashTokenResponse.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.BkashModels
{
    public class BkashTokenResponse
    {
        public string? id_token { get; set; }
        public string? token_type { get; set; }
        public int expires_in { get; set; }
        public string? refresh_token { get; set; }
        public string? scope { get; set; }
        public string? statusCode { get; set; }
        public string? statusMessage { get; set; }
    }
}