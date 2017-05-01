using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Nop.Admin.Models.Orders.OrderModel.AddOrderProductModel;

namespace Nop.Plugin.Misc.RailPointofSale.Models
{
    public class AddPosProductModel
    {
        public Product AddProduct { get; set; }

        public ProductDetailsModel ProductDetails { get; set; }

        public string ProductPicture { get; set; }

        public Vendor ProductVendor { get; set; }
    }
}