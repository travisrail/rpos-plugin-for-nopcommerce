﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Menu;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.RailPointofSale
{
    public class RailPointofSalePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {
        private readonly RailPointofSaleSettings _railPosSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;

        public RailPointofSalePlugin(ISettingService settingService, RailPointofSaleSettings railPosSettings, IPermissionService permissionService)
        {
            this._settingService = settingService;
            this._railPosSettings = railPosSettings;
            this._permissionService = permissionService;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Configuration";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.RailPointofSale.Controllers" }, { "area", null } };
        }

        public void ManageSiteMap(Nop.Web.Framework.Menu.SiteMapNode rootNode)
        {
            if (_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) && _permissionService.Authorize(StandardPermissionProvider.ManageVendors) && _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
            {
                var menuItem = new Nop.Web.Framework.Menu.SiteMapNode()
                {
                    SystemName = "Misc.RailPointofSale",
                    Title = "Point-of-Sale",
                    ControllerName = "PointofSaleOrder",
                    ActionName = "List",
                    Visible = true,
                    IconClass = "fa-dot-circle-o",
                    RouteValues = new RouteValueDictionary() { { "area", null } },
                };

                var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Sales");
                if (pluginNode != null)
                    pluginNode.ChildNodes.Insert(2, menuItem);
                else
                    rootNode.ChildNodes.Add(menuItem);
            }
        }

        public override void Install()
        {
            var settings = new RailPointofSaleSettings
            {
                StoreId = 0,
                StoreAddress = "",
                StoreCity = "",
                StoreStateProvinceId = 0,
                StorePostalCode = "",
                StoreCountryId = 0,
                StoreTaxRate = 8.95M,
                StorePaymentMethodSystemName = ""
            };
            _settingService.SaveSetting(settings);

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreId", "Store");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreId.Hint", "Choose which store you will use as the POS store.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreAddress", "Store Address");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreAddress.Hint", "The street address of your point-of-sale store location. Can be useful for tax calculation.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCity", "Store City");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCity.Hint", "The city your point-of-sale store is located in. Can be useful for tax calculation.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreStateProvinceId", "Store State");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreStateProvinceId.Hint", "The state your point-of-sale store is located in. Can be useful for tax calculation.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePostalCode", "Store Postal Code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePostalCode.Hint", "The postal code of your point-of-sale store. Can be useful for tax calculation.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCountryId", "Store Country");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCountryId.Hint", "The country your point-of-sale store is located in. Can be useful for tax calculation.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreTaxRate", "Store Tax Rate");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreTaxRate.Hint", "The tax rate for your point-of-sale store. Can be useful for tax calculation.");

            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePaymentMethodSystemName", "Store Payment Method");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePaymentMethodSystemName.Hint", "Select the payment method that will be used for the point-of-sale store.");

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<RailPointofSaleSettings>();

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreId");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreId.Hint");

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreAddress");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreAddress.Hint");

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCity");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCity.Hint");

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreStateProvinceId");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreStateProvinceId.Hint");

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePostalCode");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePostalCode.Hint");

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCountryId");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreCountryId.Hint");

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreTaxRate");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StoreTaxRate.Hint");

            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePaymentMethodSystemName");
            this.DeletePluginLocaleResource("Plugins.Misc.RailPointofSale.StorePaymentMethodSystemName.Hint");

            base.Uninstall();
        }

    }
}