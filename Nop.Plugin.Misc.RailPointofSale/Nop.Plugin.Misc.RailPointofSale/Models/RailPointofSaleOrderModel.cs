using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.RailPointofSale.Models
{
    public class RailPointofSaleOrderModel : BaseNopModel
    {

        public RailPointofSaleOrderModel()
        {
            AvailableProducts = new List<rPOSProduct>();
        }

        public List<rPOSProduct> AvailableProducts { get; set; }
    }

    public class rPOSProduct
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public string Vendor { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ProductThumbnail { get; set; }
    }
}