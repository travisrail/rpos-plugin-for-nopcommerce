using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Catalog;
using Nop.Services.Orders;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Catalog;
using Nop.Services.Authentication;

using Nop.Plugin.Misc.RailPointofSale.Models;
using Nop.Services.Media;
using Nop.Services.Vendors;

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
        private readonly IProductService _productService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;

        private RailPointofSaleSettings _rposSettings;
        #endregion

        public PointofSaleOrderController(IOrderService orderService, IOrderProcessingService orderProcessingService, IOrderTotalCalculationService orderTotalCalculationService, ICustomerService customerService, ISettingService settingService, IProductService productService, IAuthenticationService authenticationService, IPictureService pictureService, IVendorService vendorService)
        {
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._customerService = customerService;
            this._settingService = settingService;
            this._productService = productService;
            this._authenticationService = authenticationService;
            this._pictureService = pictureService;
            this._vendorService = vendorService;

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
                CreatedOnUtc = DateTime.Now.ToUniversalTime(),
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
            RailPointofSaleOrderModel model = new RailPointofSaleOrderModel();

            //get a list of products for this store
            var products = _productService.SearchProducts(
                    storeId: _rposSettings.StoreId,
                    visibleIndividuallyOnly: true,
                    orderBy: ProductSortingEnum.NameAsc,
                    pageSize: 100 );

            //loop each product and add
            foreach (var prod in products)
            {
                //get picture
                var picture = prod.ProductPictures.FirstOrDefault();

                string imageUrl = "";

                if (picture != null)
                {
                    imageUrl = _pictureService.GetPictureUrl(picture.Id, targetSize: 75);
                }
                else
                {
                    imageUrl = _pictureService.GetDefaultPictureUrl(targetSize: 75);
                }

                //get vendor
                string vendorname = "";

                if (prod.VendorId != 0)
                {
                    vendorname = _vendorService.GetVendorById(prod.VendorId).Name;
                }

                //add item
                var rposp = new rPOSProduct {
                    Id = prod.Id,
                    Sku = prod.Sku,
                    Vendor = vendorname,
                    Name = prod.Name,
                    Price = prod.Price,
                    ProductThumbnail = imageUrl
                };

                //add to model
                model.AvailableProducts.Add(rposp);
            }


            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/Edit.cshtml", model);
        }

        #region Products



        #endregion

        #region Customer

        private Customer GetCustomer()
        {
            string posCustomerEmail = "rpos_customer@do_not_use.com";
            string posCustomerUserName = "rPOSCustomer_" + _authenticationService.GetAuthenticatedCustomer().Id.ToString();

            Customer posCustomer = _customerService.GetCustomerByUsername(posCustomerUserName);

            if (posCustomer == null)
            {
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
            }

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

            //add address
            posCustomer.Addresses.Add(posAddress);
            posCustomer.BillingAddress = posAddress;
            posCustomer.ShippingAddress = posAddress;
            _customerService.UpdateCustomer(posCustomer);

            //return customer with new address
            return posCustomer;

        }

        #endregion
    }
}