using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.RailPointofSale
{
    public class RailPointofSaleSettings : ISettings
    {
        public int StoreId { get; set; }
        public string StoreAddress { get; set; }
        public string StoreCity { get; set; }
        public int StoreStateProvinceId { get; set; }
        public string StorePostalCode { get; set; }
        public int StoreCountryId { get; set; }
    }
}