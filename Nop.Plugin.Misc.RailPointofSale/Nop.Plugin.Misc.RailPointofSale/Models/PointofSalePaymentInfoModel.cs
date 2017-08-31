using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Checkout;
using Nop.Core.Domain.Orders;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.RailPointofSale.Models
{
    public class PointofSalePaymentInfoModel : CheckoutPaymentInfoModel
    {
        public PointofSalePaymentInfoModel()
        {
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }

        //additonal information
        [Required(ErrorMessage = "Please enter a billing first name.")]
        public string CustomerFirstName { get; set; }

        [Required(ErrorMessage = "Please enter a billing last name.")]
        public string CustomerLastName { get; set; }

        [Required(ErrorMessage = "Please enter a billing email address.")]
        [EmailAddress(ErrorMessage = "Invalid billing email address.")]
        public string CustomerEmailAddress { get; set; }

        public RailPointofSaleOrderModel rPosOrderModel { get; set; }

        #region Credit Card Info

        [NopResourceDisplayName("Payment.SelectCreditCard")]
        [AllowHtml]
        public string CreditCardType { get; set; }
        [NopResourceDisplayName("Payment.SelectCreditCard")]
        public IList<SelectListItem> CreditCardTypes { get; set; }

        [NopResourceDisplayName("Payment.CardholderName")]
        [AllowHtml]
        public string CardholderName { get; set; }

        [NopResourceDisplayName("Payment.CardNumber")]
        [AllowHtml]
        public string CardNumber { get; set; }

        [NopResourceDisplayName("Payment.ExpirationDate")]
        [AllowHtml]
        public string ExpireMonth { get; set; }
        [NopResourceDisplayName("Payment.ExpirationDate")]
        [AllowHtml]
        public string ExpireYear { get; set; }
        public IList<SelectListItem> ExpireMonths { get; set; }
        public IList<SelectListItem> ExpireYears { get; set; }

        [NopResourceDisplayName("Payment.CardCode")]
        [AllowHtml]
        public string CardCode { get; set; }

        #endregion
    }
}