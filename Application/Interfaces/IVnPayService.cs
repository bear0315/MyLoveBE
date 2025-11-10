using Application.Request.Payment;
using Application.Response.Payment;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(VnPaymentRequest request, string ipAddress);
        VnPaymentResponse ProcessPaymentCallback(IQueryCollection queryParams);
        bool ValidateSignature(Dictionary<string, string> responseData, string secureHash);
    }
}
