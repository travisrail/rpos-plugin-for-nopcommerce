using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Nop.Web.Models.Checkout;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.RailPointofSale.Models
{
    public class PointofSalePaymentInfoModel : CheckoutPaymentInfoModel
    {
        //additonal information
        public string CustomerEmailAddress { get; set; }
        public RailPointofSaleOrderModel rPosOrderModel { get; set; }
    }
}