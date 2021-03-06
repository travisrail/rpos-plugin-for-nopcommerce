﻿using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.RailPointofSale.Model
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public ConfigurationModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableStateProvice = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailablePaymentMethods = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StoreId")]
        public int StoreId { get; set; }
        public bool StoreId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StoreAddress")]
        [AllowHtml]
        public string StoreAddress { get; set; }
        public bool StoreAddress_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StoreCity")]
        [AllowHtml]
        public string StoreCity { get; set; }
        public bool StoreCity_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StoreStateProvinceId")]
        [AllowHtml]
        public int StoreStateProvinceId { get; set; }
        public bool StoreStateProvinceId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StorePostalCode")]
        [AllowHtml]
        public string StorePostalCode { get; set; }
        public bool StorePostalCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StoreCountryId")]
        [AllowHtml]
        public int StoreCountryId { get; set; }
        public bool StoreCountryId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StoreTaxRate")]
        [AllowHtml]
        public decimal StoreTaxRate { get; set; }
        public bool StoreTaxRate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RailPointofSale.StorePaymentMethodSystemName")]
        [AllowHtml]
        public string StorePaymentMethodSystemName { get; set; }
        public bool StorePaymentMethodSystemName_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableStateProvice { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailablePaymentMethods { get; set; }
    }
}
