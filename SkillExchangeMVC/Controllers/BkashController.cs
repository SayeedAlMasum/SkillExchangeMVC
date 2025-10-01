//BkashController.cs
using Microsoft.AspNetCore.Mvc;
using SkillExchangeMVC.Models;
using SkillExchangeMVC.Models.BkashModels;
using SkillExchangeMVC.Models.Context;
using SkillExchangeMVC.Services;
using System.Security.Claims;

namespace SkillExchangeMVC.Controllers
{
    public class BkashController : Controller
    {
        private readonly IBkashService _bkashService;
        private readonly SkillExchangeContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BkashController> _logger;

        public BkashController(IBkashService bkashService, SkillExchangeContext context, IConfiguration configuration, ILogger<BkashController> logger)
        {
            _bkashService = bkashService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment(int courseId, decimal amount, string? payerReference = "")
        {
            try
            {
                _logger.LogInformation($"Bkash CreatePayment called with CourseId: {courseId}, Amount: {amount}, Reference: {payerReference}");

                // Get Bkash token
                var tokenResponse = await _bkashService.GetTokenAsync();
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.id_token))
                {
                    _logger.LogError("Failed to get Bkash token");
                    return Json(new { success = false, message = "Failed to get Bkash token" });
                }

                _logger.LogInformation("Successfully obtained Bkash token");

                // Create payment request
                var merchantInvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{courseId}";
                var callbackUrl = Url.Action("Callback", "Bkash", null, Request.Scheme);

                var createPaymentRequest = new BkashCreatePaymentRequest
                {
                    amount = amount,
                    merchantInvoiceNumber = merchantInvoiceNumber,
                    payerReference = payerReference ?? "",
                    callbackURL = callbackUrl ?? ""
                };

                _logger.LogInformation($"Creating Bkash payment with invoice: {merchantInvoiceNumber}, callback: {callbackUrl}");

                // Create payment
                var paymentResponse = await _bkashService.CreatePaymentAsync(createPaymentRequest, tokenResponse.id_token);
                if (paymentResponse == null || string.IsNullOrEmpty(paymentResponse.paymentID))
                {
                    _logger.LogError("Failed to create Bkash payment");
                    return Json(new { success = false, message = "Failed to create payment" });
                }

                _logger.LogInformation($"Bkash payment created successfully with ID: {paymentResponse.paymentID}");

                // Store payment info in session for later processing
                HttpContext.Session.SetString("BkashPaymentId", paymentResponse.paymentID);
                HttpContext.Session.SetString("BkashToken", tokenResponse.id_token);
                HttpContext.Session.SetInt32("CourseId", courseId);
                HttpContext.Session.SetString("MerchantInvoiceNumber", merchantInvoiceNumber);

                return Json(new 
                { 
                    success = true, 
                    paymentId = paymentResponse.paymentID,
                    bkashURL = paymentResponse.bkashURL 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Bkash CreatePayment");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Callback(string paymentID, string status)
        {
            try
            {
                _logger.LogInformation($"Bkash callback received. PaymentID: {paymentID}, Status: {status}");

                var token = HttpContext.Session.GetString("BkashToken");
                var courseId = HttpContext.Session.GetInt32("CourseId");
                var merchantInvoiceNumber = HttpContext.Session.GetString("MerchantInvoiceNumber");

                if (string.IsNullOrEmpty(token) || !courseId.HasValue)
                {
                    _logger.LogWarning("Session expired or missing data in Bkash callback");
                    TempData["PaymentError"] = "Session expired. Please try again.";
                    return RedirectToAction("IndexCourse", "Course");
                }

                if (status.ToLower() == "success")
                {
                    // Execute payment
                    var executeRequest = new BkashExecutePaymentRequest { paymentID = paymentID };
                    var executeResponse = await _bkashService.ExecutePaymentAsync(executeRequest, token);

                    if (executeResponse != null && executeResponse.transactionStatus == "Completed")
                    {
                        _logger.LogInformation($"Bkash payment executed successfully. TrxID: {executeResponse.trxID}");

                        // Save payment to database
                        var email = User.FindFirstValue(ClaimTypes.Email);
                        var userId = _context.UserInfo.FirstOrDefault(u => u.Email == email)?.UserInfoId;
                        var course = _context.Course.FirstOrDefault(c => c.CourseId == courseId.Value);

                        if (userId != null && course != null)
                        {
                            var payment = new Payment
                            {
                                CourseId = courseId.Value,
                                UserInfoId = userId,
                                PaymentMethod = "Bkash",
                                Amount = executeResponse.amount,
                                PaymentStatus = "Completed",
                                BkashPaymentId = executeResponse.paymentID,
                                BkashTransactionId = executeResponse.trxID,
                                BkashMerchantInvoiceNumber = merchantInvoiceNumber,
                                BkashPayerReference = executeResponse.payerReference,
                                PaymentDate = DateTime.Now
                            };

                            _context.Payment.Add(payment);

                            // Enroll user in course
                            var existingEnrollment = _context.Enrollments
                                .FirstOrDefault(e => e.CourseId == courseId.Value && e.UserInfoId == userId);

                            if (existingEnrollment == null)
                            {
                                _context.Enrollments.Add(new Enrollment
                                {
                                    CourseId = courseId.Value,
                                    UserInfoId = userId
                                });
                            }

                            await _context.SaveChangesAsync();

                            // Clear session
                            HttpContext.Session.Remove("BkashPaymentId");
                            HttpContext.Session.Remove("BkashToken");
                            HttpContext.Session.Remove("CourseId");
                            HttpContext.Session.Remove("MerchantInvoiceNumber");

                            TempData["PaymentSuccess"] = $"Payment successful! Transaction ID: {executeResponse.trxID}";
                            
                            if (User.IsInRole("Admin"))
                            {
                                return RedirectToAction("CourseContents", "Content", new { courseId = courseId.Value });
                            }
                            
                            return RedirectToAction("IndexCourse", "Course");
                        }
                    }
                    else
                    {
                        _logger.LogError("Payment execution failed");
                        TempData["PaymentError"] = "Payment execution failed. Please try again.";
                    }
                }
                else if (status.ToLower() == "failure")
                {
                    _logger.LogWarning("Bkash payment failed");
                    TempData["PaymentError"] = "Payment failed. Please try again.";
                }
                else if (status.ToLower() == "cancel")
                {
                    _logger.LogInformation("Bkash payment cancelled");
                    TempData["PaymentInfo"] = "Payment was cancelled.";
                }

                return RedirectToAction("IndexCourse", "Course");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Bkash callback");
                TempData["PaymentError"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("IndexCourse", "Course");
            }
        }

        [HttpPost]
        public async Task<IActionResult> QueryPayment(string paymentId)
        {
            try
            {
                var tokenResponse = await _bkashService.GetTokenAsync();
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.id_token))
                {
                    return Json(new { success = false, message = "Failed to get Bkash token" });
                }

                var queryResponse = await _bkashService.QueryPaymentAsync(paymentId, tokenResponse.id_token);
                if (queryResponse != null)
                {
                    return Json(new { success = true, payment = queryResponse });
                }

                return Json(new { success = false, message = "Payment not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QueryPayment");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefundPayment(string paymentId, decimal amount, string trxId, string reason)
        {
            try
            {
                var tokenResponse = await _bkashService.GetTokenAsync();
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.id_token))
                {
                    return Json(new { success = false, message = "Failed to get Bkash token" });
                }

                var refundRequest = new BkashRefundRequest
                {
                    paymentID = paymentId,
                    amount = amount,
                    trxID = trxId,
                    reason = reason
                };

                var refundResponse = await _bkashService.RefundPaymentAsync(refundRequest, tokenResponse.id_token);
                if (refundResponse != null && refundResponse.transactionStatus == "Completed")
                {
                    return Json(new { success = true, refund = refundResponse });
                }

                return Json(new { success = false, message = "Refund failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefundPayment");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}