using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Directory;
using Nop.Web.Framework.Controllers;

using Nop.Plugin.Misc.RailPointofSale.Model;
using Nop.Services.Payments;

namespace Nop.Plugin.Misc.RailPointofSale.Controllers
{

    public class ConfigurationController : BasePluginController
    {
        #region Services

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IPaymentService _paymentService;

        #endregion 

        public ConfigurationController(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            ISettingService settingService,
            IOrderService orderService,
            ILogger logger,
            ICategoryService categoryService,
            IProductAttributeParser productAttributeParser,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IPaymentService paymentService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._logger = logger;
            this._categoryService = categoryService;
            this._productAttributeParser = productAttributeParser;
            this._localizationService = localizationService;
            this._stateProvinceService = stateProvinceService;
            this._countryService = countryService;
            this._paymentService = paymentService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //Load settings for a chose store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var railPOSSettings = _settingService.LoadSetting<RailPointofSaleSettings>();
            var model = new ConfigurationModel();
            model.StoreId = railPOSSettings.StoreId;
            model.StoreAddress = railPOSSettings.StoreAddress;
            model.StoreCity = railPOSSettings.StoreCity;
            model.StoreStateProvinceId = railPOSSettings.StoreStateProvinceId;
            model.StorePostalCode = railPOSSettings.StorePostalCode;
            model.StoreCountryId = railPOSSettings.StoreCountryId;
            model.StoreTaxRate = railPOSSettings.StoreTaxRate;
            model.StorePaymentMethodSystemName = railPOSSettings.StorePaymentMethodSystemName;
            model.ActiveStoreScopeConfiguration = storeScope;

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "-- Select POS Store --", Value = "-1" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //states / provices
            model.AvailableStateProvice.Add(new SelectListItem { Text = "-- Select POS State/Province --", Value = "-1" });
            foreach (var sp in _stateProvinceService.GetStateProvinces())
                model.AvailableStateProvice.Add(new SelectListItem { Text = sp.Name, Value = sp.Id.ToString() });

            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "-- Select POS Store Country --", Value = "-1" });
            foreach (var cp in _countryService.GetAllCountries())
                model.AvailableCountries.Add(new SelectListItem { Text = cp.Name, Value = cp.Id.ToString() });

            //paymetn methods
            model.AvailablePaymentMethods.Add(new SelectListItem { Text = "-- Select POS Payment Method --", Value = "" });
            foreach (var pm in _paymentService.LoadActivePaymentMethods())
                model.AvailablePaymentMethods.Add(new SelectListItem { Text = pm.PluginDescriptor.FriendlyName, Value = pm.PluginDescriptor.SystemName });

            return View("~/Plugins/Misc.RailPointofSale/Views/Configuration/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            //Load settings for a chose store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var railPOSSettings = _settingService.LoadSetting<RailPointofSaleSettings>();

            railPOSSettings.StoreId = model.StoreId;
            railPOSSettings.StoreAddress = model.StoreAddress;
            railPOSSettings.StoreCity = model.StoreCity;
            railPOSSettings.StoreStateProvinceId = model.StoreStateProvinceId;
            railPOSSettings.StorePostalCode = model.StorePostalCode;
            railPOSSettings.StoreCountryId = model.StoreCountryId;
            railPOSSettings.StoreTaxRate = model.StoreTaxRate;
            railPOSSettings.StorePaymentMethodSystemName = model.StorePaymentMethodSystemName;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StoreId, model.StoreId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StoreAddress, model.StoreAddress_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StoreCity, model.StoreCity_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StoreStateProvinceId, model.StoreStateProvinceId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StorePostalCode, model.StorePostalCode_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StoreCountryId, model.StoreCountryId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StoreTaxRate, model.StoreTaxRate_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(railPOSSettings, x => x.StorePaymentMethodSystemName, model.StorePaymentMethodSystemName_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}
