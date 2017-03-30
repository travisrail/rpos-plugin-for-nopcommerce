using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Services.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;

namespace Nop.Plugin.Misc.RailPointofSale.Controllers
{
    public class PointofSaleOrderController : Controller
    {
        #region fields
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICustomerService _customerService;
        private readonly ISettingService _settingService;

        private RailPointofSaleSettings _rposSettings;
        #endregion

        public PointofSaleOrderController(IOrderService orderService, IOrderProcessingService orderProcessingService, IOrderTotalCalculationService orderTotalCalculationService, ICustomerService customerService, ISettingService settingService)
        {
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._customerService = customerService;
            this._settingService = settingService;

            _rposSettings = _settingService.LoadSetting<RailPointofSaleSettings>();
        }

        // GET: PointofSaleOrder
        public ActionResult Index()
        {
            return List();
        }

        [HttpGet]
        public ActionResult List()
        {
            //check to make sure the store is configured
            if (_rposSettings.StoreId <= 0)
            {
                //the store is not configured so we need to re-direct to configuration
                return Redirect("/Admin/Plugin/ConfigureMiscPlugin?systemName=Misc.RailPointofSale");
            }
            else
            {
                return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/List.cshtml", null);
            }
        }

        public ActionResult Add()
        {

            //get customer
            Customer posCustomer = GetCustomer();

            //create new blank order
            Guid newOrderGuid = Guid.NewGuid();

            var order = new Order
            {
                OrderGuid = newOrderGuid,
                CustomerId = posCustomer.Id,
                StoreId = _rposSettings.StoreId,
                BillingAddressId = posCustomer.BillingAddress.Id,
                ShippingAddressId = posCustomer.ShippingAddress.Id,
                ShippingStatusId = 10,
                PaymentStatusId = 10,
                PaymentMethodSystemName = "Payments.CheckMoneyOrder",
                OrderStatus = OrderStatus.Pending,
                CustomerCurrencyCode = "USD",
                CreatedOnUtc = DateTime.Now.ToUniversalTime()
            };

            //add order
            _orderService.InsertOrder(order);

            //get order we just created
            Order newOrder = _orderService.GetOrderByGuid(newOrderGuid);

            //redirect to order we are going to edit
            return RedirectToAction("Edit", new { id = newOrder.Id });
        }

        public ActionResult Edit(int id)
        {
            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/Edit.cshtml", null);
        }

        #region Customer

        private Customer GetCustomer()
        {
            string posCustomerEmail = "rpos_customer@do_not_use.com";
            string posCustomerUserName = "rPOSCustomer";

            Customer posCustomer = _customerService.GetCustomerByUsername(posCustomerUserName);

            if (posCustomer == null)
            {
                //create billing/shipping address
                Address posAddress = new Address
                {
                    Address1 = _rposSettings.StoreAddress,
                    City = _rposSettings.StoreCity,
                    StateProvinceId = _rposSettings.StoreStateProvinceId,
                    ZipPostalCode = _rposSettings.StorePostalCode,
                    CountryId = _rposSettings.StoreCountryId,
                    FirstName = "rPOS",
                    LastName = "Customer",
                    CreatedOnUtc = DateTime.UtcNow
                };

                //create customer
                Guid posCustomerGuid = Guid.NewGuid();

                posCustomer = new Customer
                {
                    CustomerGuid = posCustomerGuid,
                    Email = posCustomerEmail,
                    Username = posCustomerUserName,
                    VendorId = 0,
                    AdminComment = "Created for POS store sales, do not modify this customer!",
                    IsTaxExempt = false,
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                };
                _customerService.InsertCustomer(posCustomer);

                //get new customer
                posCustomer = _customerService.GetCustomerByGuid(posCustomerGuid);

                //add address
                posCustomer.Addresses.Add(posAddress);
                posCustomer.BillingAddress = posAddress;
                posCustomer.ShippingAddress = posAddress;
                _customerService.UpdateCustomer(posCustomer);

                //return customer
                return posCustomer;
            }
            else
            {
                //update billing / shipping address
                posCustomer.ShippingAddress.Address1 = _rposSettings.StoreAddress;
                posCustomer.ShippingAddress.City = _rposSettings.StoreCity;
                posCustomer.ShippingAddress.StateProvinceId = _rposSettings.StoreStateProvinceId;
                posCustomer.ShippingAddress.ZipPostalCode = _rposSettings.StorePostalCode;
                posCustomer.ShippingAddress.CountryId = _rposSettings.StoreCountryId;
                posCustomer.BillingAddress = posCustomer.ShippingAddress;
                _customerService.UpdateCustomer(posCustomer);

                //return POS Customer
                return posCustomer;
            }
        }

        #endregion
    }
}