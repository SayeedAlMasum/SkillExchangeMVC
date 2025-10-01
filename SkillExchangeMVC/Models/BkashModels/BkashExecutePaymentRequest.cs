//BkashExecutePaymentRequest.cs
using System.ComponentModel.DataAnnotations;

namespace SkillExchangeMVC.Models.BkashModels
{
    public class BkashExecutePaymentRequest
    {
        public string paymentID { get; set; } = "";
    }
}