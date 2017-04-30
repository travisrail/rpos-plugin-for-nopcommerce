using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Nop.Web.Models.Checkout;
using Nop.Core.Domain.Orders;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.RailPointofSale.Models
{
    public class PointofSalePaymentInfoModel : CheckoutPaymentInfoModel
    {
        //additonal information
        [Required(ErrorMessage = "Please enter a billing first name.")]
        public string CustomerFirstName { get; set; }

        [Required(ErrorMessage = "Please enter a billing last name.")]
        public string CustomerLastName { get; set; }

        [Required(ErrorMessage = "Please enter a billing email address.")]
        [EmailAddress(ErrorMessage = "Invalid billing email address.")]
        public string CustomerEmailAddress { get; set; }

        public RailPointofSaleOrderModel rPosOrderModel { get; set; }
    }
}