using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Admin;
using Nop.Admin.Models.Orders;

namespace Nop.Plugin.Misc.RailPointofSale.Models
{
    public class RailPointofSaleOrderListModel : Nop.Admin.Models.Orders.OrderListModel
    {
        public String StoreCityStatePostal { get; set; }
        public String StoreTaxRate { get; set; }
    }
}