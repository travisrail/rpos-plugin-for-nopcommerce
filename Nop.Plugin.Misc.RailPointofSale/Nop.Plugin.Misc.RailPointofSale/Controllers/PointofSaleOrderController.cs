using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Admin.Extensions;
using Nop.Admin.Helpers;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Checkout;

using Nop.Plugin.Misc.RailPointofSale.Models;
using Nop.Services.Configuration;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Core.Domain.Messages;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.RailPointofSale.Controllers
{
    public class PointofSaleOrderController : Nop.Admin.Controllers.BaseAdminController
    {
        #region fields
        private readonly IOrderService _orderService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IEncryptionService _encryptionService;
        private readonly IPaymentService _paymentService;
        private readonly IMeasureService _measureService;
        private readonly IPdfService _pdfService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductService _productService;
        private readonly IExportManager _exportManager;
        private readonly IPermissionService _permissionService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAffiliateService _affiliateService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ITokenizer _tokenizer;
        private readonly IQueuedEmailService _queuedEmailService;

        private readonly OrderSettings _orderSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;

        private readonly RailPointofSaleSettings _rposSettings;

        #endregion

        #region Cotr

        public PointofSaleOrderController(IOrderService orderService,
            IOrderReportService orderReportService,
            IOrderProcessingService orderProcessingService,
            IReturnRequestService returnRequestService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IDiscountService discountService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICurrencyService currencyService,
            IEncryptionService encryptionService,
            IPaymentService paymentService,
            IMeasureService measureService,
            IPdfService pdfService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IProductService productService,
            IExportManager exportManager,
            IPermissionService permissionService,
            IWorkflowMessageService workflowMessageService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IShoppingCartService shoppingCartService,
            IGiftCardService giftCardService,
            IDownloadService downloadService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStoreService storeService,
            IVendorService vendorService,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAffiliateService affiliateService,
            IPictureService pictureService,
            ICustomerActivityService customerActivityService,
            ICacheManager cacheManager,
            ISettingService settingService,
            IAuthenticationService authenticationService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IEventPublisher eventPublisher,
            IEmailAccountService emailAccountService,
            ITokenizer tokenizer,
            IQueuedEmailService queuedEmailService,

            OrderSettings orderSettings,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            MeasureSettings measureSettings,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings)
        {
            this._orderService = orderService;
            this._orderReportService = orderReportService;
            this._orderProcessingService = orderProcessingService;
            this._returnRequestService = returnRequestService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._discountService = discountService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._encryptionService = encryptionService;
            this._paymentService = paymentService;
            this._measureService = measureService;
            this._pdfService = pdfService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._productService = productService;
            this._exportManager = exportManager;
            this._permissionService = permissionService;
            this._workflowMessageService = workflowMessageService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._shoppingCartService = shoppingCartService;
            this._giftCardService = giftCardService;
            this._downloadService = downloadService;
            this._shipmentService = shipmentService;
            this._shippingService = shippingService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._addressAttributeParser = addressAttributeParser;
            this._addressAttributeService = addressAttributeService;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._affiliateService = affiliateService;
            this._pictureService = pictureService;
            this._customerActivityService = customerActivityService;
            this._cacheManager = cacheManager;
            this._settingService = settingService;
            this._authenticationService = authenticationService;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._orderSettings = orderSettings;
            this._currencySettings = currencySettings;
            this._taxSettings = taxSettings;
            this._measureSettings = measureSettings;
            this._addressSettings = addressSettings;
            this._shippingSettings = shippingSettings;
            this._messageTemplateService = messageTemplateService;
            this._messageTokenProvider = messageTokenProvider;
            this._eventPublisher = eventPublisher;
            this._emailAccountService = emailAccountService;
            this._tokenizer = tokenizer;
            this._queuedEmailService = queuedEmailService;

            this._rposSettings = _settingService.LoadSetting<RailPointofSaleSettings>();
        }

        #endregion

        // GET: PointofSaleOrder
        public ActionResult Index()
        {
            return List();
        }

        public ActionResult Add()
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

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
                OrderShippingExclTax = 0,
                OrderShippingInclTax = 0,
                PaymentStatusId = 10,
                PaymentMethodSystemName = _rposSettings.StorePaymentMethodSystemName,
                OrderStatus = OrderStatus.Pending,
                CustomerCurrencyCode = "USD",
                CurrencyRate = 1.00M,
                CreatedOnUtc = DateTime.Now.ToUniversalTime(),
            };

            //add order
            _orderService.InsertOrder(order);

            //get order we just created
            Order newOrder = _orderService.GetOrderByGuid(newOrderGuid);

            //redirect to order we are going to edit
            return RedirectToAction("Edit", new { id = newOrder.Id });
        }

        #region Order List

        [HttpGet]
        public ActionResult List(
            [ModelBinder(typeof(CommaSeparatedModelBinder))] List<string> orderStatusIds = null,
            [ModelBinder(typeof(CommaSeparatedModelBinder))] List<string> paymentStatusIds = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //check to make sure the store is configured
            if (_rposSettings.StoreId <= 0)
            {
                //the store is not configured so we need to re-direct to configuration
                return Redirect("/Admin/Plugin/ConfigureMiscPlugin?systemName=Misc.RailPointofSale");
            }
            else
            {
                //order statuses
                RailPointofSaleOrderListModel model = new RailPointofSaleOrderListModel();
                model.StoreCityStatePostal = _rposSettings.StoreCity + ", " + _stateProvinceService.GetStateProvinceById(_rposSettings.StoreStateProvinceId).Abbreviation + "  " + _rposSettings.StorePostalCode;
                model.StoreTaxRate = _rposSettings.StoreTaxRate.ToString() + "%";
                model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
                model.AvailableOrderStatuses.Insert(0, new SelectListItem
                { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0", Selected = true });
                if (orderStatusIds != null && orderStatusIds.Any())
                {
                    foreach (var item in model.AvailableOrderStatuses.Where(os => orderStatusIds.Contains(os.Value)))
                        item.Selected = true;
                    model.AvailableOrderStatuses.First().Selected = false;
                }

                //payment statuses
                model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
                model.AvailablePaymentStatuses.Insert(0, new SelectListItem
                { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0", Selected = true });
                if (paymentStatusIds != null && paymentStatusIds.Any())
                {
                    foreach (var item in model.AvailablePaymentStatuses.Where(ps => paymentStatusIds.Contains(ps.Value)))
                        item.Selected = true;
                    model.AvailablePaymentStatuses.First().Selected = false;
                }

                //vendors
                model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
                foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                    model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

                //a vendor should have access only to orders with his products
                model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

                //set default days
                DateTime currentDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                model.StartDate = (DateTime?)_dateTimeHelper.ConvertToUtcTime(currentDateTime, _dateTimeHelper.CurrentTimeZone);
                model.EndDate = (DateTime?)_dateTimeHelper.ConvertToUtcTime(currentDateTime, _dateTimeHelper.CurrentTimeZone).AddDays(1);

                return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/List.cshtml", model);
            }
        }


        [HttpPost]
        public ActionResult OrderList(DataSourceRequest command, RailPointofSaleOrderListModel model)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            //defaults?
            DateTime? startDateValue = (model.StartDate == null) ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(DateTime.Now.Date, _dateTimeHelper.CurrentTimeZone)
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(DateTime.Now.Date.AddDays(1), _dateTimeHelper.CurrentTimeZone)
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = !model.OrderStatusIds.Contains(0) ? model.OrderStatusIds : null;
            var paymentStatusIds = !model.PaymentStatusIds.Contains(0) ? model.PaymentStatusIds : null;
            var shippingStatusIds = !model.ShippingStatusIds.Contains(0) ? model.ShippingStatusIds : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && HasAccessToProduct(product))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: _rposSettings.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
                {
                    var store = _storeService.GetStoreById(_rposSettings.StoreId);
                    return new OrderModel
                    {
                        Id = x.Id,
                        StoreName = store != null ? store.Name : "Unknown",
                        OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                        OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                        OrderStatusId = x.OrderStatusId,
                        PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                        PaymentStatusId = x.PaymentStatusId,
                        ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                        ShippingStatusId = x.ShippingStatusId,
                        CustomerEmail = x.BillingAddress.Email,
                        CustomerFullName = string.Format("{0} {1}", x.BillingAddress.FirstName, x.BillingAddress.LastName),
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                    };
                }),
                Total = orders.TotalCount
            };

            //summary report
            //currently we do not support productId and warehouseId parameters for this report
            var reportSummary = _orderReportService.GetOrderAverageReportLine(
                storeId: _rposSettings.StoreId,
                vendorId: model.VendorId,
                orderId: 0,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);

            var profit = _orderReportService.ProfitReport(
                storeId: _rposSettings.StoreId,
                vendorId: model.VendorId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);
            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            gridModel.ExtraData = new OrderAggreratorModel
            {
                aggregatorprofit = _priceFormatter.FormatPrice(profit, true, false),
                aggregatorshipping = _priceFormatter.FormatShippingPrice(reportSummary.SumShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false),
                aggregatortax = _priceFormatter.FormatPrice(reportSummary.SumTax, true, false),
                aggregatortotal = _priceFormatter.FormatPrice(reportSummary.SumOrders, true, false)
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult ProductSearchAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (String.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content("");

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            //products
            const int productNumber = 15;
            var products = _productService.SearchProducts(
                vendorId: vendorId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true);

            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              productid = p.Id
                          })
                          .ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Edit

        public ActionResult Edit(int id)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            //get order
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                //No order found with the specified id
                return RedirectToAction("List");

            //create model
            RailPointofSaleOrderModel model = new RailPointofSaleOrderModel();
            PrepareOrderDetailsModel(model, order);

            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/Edit.cshtml", model);
        }

        [NonAction]
        protected virtual void PrepareOrderDetailsModel(RailPointofSaleOrderModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.StoreCityStatePostal = _rposSettings.StoreCity + ", " + _stateProvinceService.GetStateProvinceById(_rposSettings.StoreStateProvinceId).Abbreviation + "  " + _rposSettings.StorePostalCode;
            model.StoreTaxRate = _rposSettings.StoreTaxRate.ToString() + "%";
            model.Id = order.Id;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.OrderStatusId = order.OrderStatusId;
            model.OrderGuid = order.OrderGuid;
            var store = _storeService.GetStoreById(order.StoreId);
            model.StoreName = store != null ? store.Name : "Unknown";
            model.CustomerId = order.CustomerId;
            var customer = order.Customer;
            model.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            model.CustomerIp = order.CustomerIp;
            model.VatNumber = order.VatNumber;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = _taxSettings.TaxDisplayType;

            var affiliate = _affiliateService.GetAffiliateById(order.AffiliateId);
            if (affiliate != null)
            {
                model.AffiliateId = affiliate.Id;
                model.AffiliateName = affiliate.GetFullName();
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            //custom values
            model.CustomValues = order.DeserializeCustomValues();

            #region Order totals

            var primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency == null)
                throw new Exception("Cannot load primary store currency");

            //subtotal
            model.OrderSubtotalInclTax = _priceFormatter.FormatPrice(order.OrderSubtotalInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderSubtotalExclTax = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;
            //discount (applied to order subtotal)
            string orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            string orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderShippingExclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            }
            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;


            //tax
            model.Tax = _priceFormatter.FormatPrice(order.OrderTax, true, false);
            SortedDictionary<decimal, decimal> taxRates = order.TaxRatesDictionary;
            bool displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
            bool displayTax = !displayTaxRates;
            foreach (var tr in order.TaxRatesDictionary)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = _priceFormatter.FormatPrice(tr.Value, true, false),
                });
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            //discount
            if (order.OrderDiscount > 0)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-order.OrderDiscount, true, false);
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderModel.GiftCard
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-gcuh.UsedValue, true, false),
                });
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = _priceFormatter.FormatPrice(order.RefundedAmount, true, false);

            //used discounts
            var duh = _discountService.GetAllDiscountUsageHistory(orderId: order.Id);
            foreach (var d in duh)
            {
                model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                {
                    DiscountId = d.DiscountId,
                    DiscountName = d.Discount.Name
                });
            }

            //profit (hide for vendors)
            if (_workContext.CurrentVendor == null)
            {
                var profit = _orderReportService.ProfitReport(orderId: order.Id);
                model.Profit = _priceFormatter.FormatPrice(profit, true, false);
            }

            #endregion

            #region Payment info

            if (order.AllowStoringCreditCardNumber)
            {
                //card type
                model.CardType = _encryptionService.DecryptText(order.CardType);
                //cardholder name
                model.CardName = _encryptionService.DecryptText(order.CardName);
                //card number
                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                //cvv
                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                //expiry date
                string cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                if (!String.IsNullOrEmpty(cardExpirationMonthDecrypted) && cardExpirationMonthDecrypted != "0")
                    model.CardExpirationMonth = cardExpirationMonthDecrypted;
                string cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                if (!String.IsNullOrEmpty(cardExpirationYearDecrypted) && cardExpirationYearDecrypted != "0")
                    model.CardExpirationYear = cardExpirationYearDecrypted;

                model.AllowStoringCreditCardNumber = true;
            }
            else
            {
                string maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                if (!String.IsNullOrEmpty(maskedCreditCardNumberDecrypted))
                    model.CardNumber = maskedCreditCardNumberDecrypted;
            }


            //payment transaction info
            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;

            //payment method info
            var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            model.PaymentMethod = pm != null ? pm.PluginDescriptor.FriendlyName : order.PaymentMethodSystemName;
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);

            //payment method buttons
            model.CanCancelOrder = _orderProcessingService.CanCancelOrder(order);
            model.CanCapture = _orderProcessingService.CanCapture(order);
            model.CanMarkOrderAsPaid = _orderProcessingService.CanMarkOrderAsPaid(order);
            model.CanRefund = _orderProcessingService.CanRefund(order);
            model.CanRefundOffline = _orderProcessingService.CanRefundOffline(order);
            model.CanPartiallyRefund = _orderProcessingService.CanPartiallyRefund(order, decimal.Zero);
            model.CanPartiallyRefundOffline = _orderProcessingService.CanPartiallyRefundOffline(order, decimal.Zero);
            model.CanVoid = _orderProcessingService.CanVoid(order);
            model.CanVoidOffline = _orderProcessingService.CanVoidOffline(order);

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.MaxAmountToRefund = order.OrderTotal - order.RefundedAmount;

            //recurring payment record
            var recurringPayment = _orderService.SearchRecurringPayments(initialOrderId: order.Id, showHidden: true).FirstOrDefault();
            if (recurringPayment != null)
            {
                model.RecurringPaymentId = recurringPayment.Id;
            }
            #endregion

            #region Billing & shipping info

            model.BillingAddress = order.BillingAddress.ToModel();
            model.BillingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
            model.BillingAddress.FirstNameEnabled = true;
            model.BillingAddress.FirstNameRequired = true;
            model.BillingAddress.LastNameEnabled = true;
            model.BillingAddress.LastNameRequired = true;
            model.BillingAddress.EmailEnabled = true;
            model.BillingAddress.EmailRequired = true;
            model.BillingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.BillingAddress.CompanyRequired = _addressSettings.CompanyRequired;
            model.BillingAddress.CountryEnabled = _addressSettings.CountryEnabled;
            model.BillingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.BillingAddress.CityEnabled = _addressSettings.CityEnabled;
            model.BillingAddress.CityRequired = _addressSettings.CityRequired;
            model.BillingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.BillingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.BillingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.BillingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.BillingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.BillingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.BillingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.BillingAddress.PhoneRequired = _addressSettings.PhoneRequired;
            model.BillingAddress.FaxEnabled = _addressSettings.FaxEnabled;
            model.BillingAddress.FaxRequired = _addressSettings.FaxRequired;

            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext); ;
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;

                model.PickUpInStore = order.PickUpInStore;
                if (!order.PickUpInStore)
                {
                    model.ShippingAddress = order.ShippingAddress.ToModel();
                    model.ShippingAddress.FormattedCustomAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                    model.ShippingAddress.FirstNameEnabled = true;
                    model.ShippingAddress.FirstNameRequired = true;
                    model.ShippingAddress.LastNameEnabled = true;
                    model.ShippingAddress.LastNameRequired = true;
                    model.ShippingAddress.EmailEnabled = true;
                    model.ShippingAddress.EmailRequired = true;
                    model.ShippingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
                    model.ShippingAddress.CompanyRequired = _addressSettings.CompanyRequired;
                    model.ShippingAddress.CountryEnabled = _addressSettings.CountryEnabled;
                    model.ShippingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
                    model.ShippingAddress.CityEnabled = _addressSettings.CityEnabled;
                    model.ShippingAddress.CityRequired = _addressSettings.CityRequired;
                    model.ShippingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
                    model.ShippingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
                    model.ShippingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
                    model.ShippingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
                    model.ShippingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
                    model.ShippingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
                    model.ShippingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
                    model.ShippingAddress.PhoneRequired = _addressSettings.PhoneRequired;
                    model.ShippingAddress.FaxEnabled = _addressSettings.FaxEnabled;
                    model.ShippingAddress.FaxRequired = _addressSettings.FaxRequired;

                    model.ShippingAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}", Server.UrlEncode(order.ShippingAddress.Address1 + " " + order.ShippingAddress.ZipPostalCode + " " + order.ShippingAddress.City + " " + (order.ShippingAddress.Country != null ? order.ShippingAddress.Country.Name : "")));
                }
                else
                {
                    if (order.PickupAddress != null)
                    {
                        model.PickupAddress = order.PickupAddress.ToModel();
                        model.PickupAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}",
                            Server.UrlEncode(string.Format("{0} {1} {2} {3}", order.PickupAddress.Address1, order.PickupAddress.ZipPostalCode, order.PickupAddress.City,
                                order.PickupAddress.Country != null ? order.PickupAddress.Country.Name : string.Empty)));
                    }
                }
                model.ShippingMethod = order.ShippingMethod;

                model.CanAddNewShipments = order.HasItemsToAddToShipment();
            }

            #endregion

            #region Products

            model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
            bool hasDownloadableItems = false;
            var products = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products
                    .Where(orderItem => orderItem.Product.VendorId == _workContext.CurrentVendor.Id)
                    .ToList();
            }

            foreach (var orderItem in products)
            {
                if (orderItem.Product.IsDownload)
                    hasDownloadableItems = true;

                var orderItemModel = new OrderModel.OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.Product.Name,
                    Sku = orderItem.Product.FormatSku(orderItem.AttributesXml, _productAttributeParser),
                    Quantity = orderItem.Quantity,
                    IsDownload = orderItem.Product.IsDownload,
                    DownloadCount = orderItem.DownloadCount,
                    DownloadActivationType = orderItem.Product.DownloadActivationType,
                    IsDownloadActivated = orderItem.IsDownloadActivated
                };
                //picture
                var orderItemPicture = orderItem.Product.GetProductPicture(orderItem.AttributesXml, _pictureService, _productAttributeParser);
                orderItemModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(orderItemPicture, 75, true);

                //license file
                if (orderItem.LicenseDownloadId.HasValue)
                {
                    var licenseDownload = _downloadService.GetDownloadById(orderItem.LicenseDownloadId.Value);
                    if (licenseDownload != null)
                    {
                        orderItemModel.LicenseDownloadGuid = licenseDownload.DownloadGuid;
                    }
                }
                //vendor
                var vendor = _vendorService.GetVendorById(orderItem.Product.VendorId);
                orderItemModel.VendorName = vendor != null ? vendor.Name : "";

                //unit price
                orderItemModel.UnitPriceInclTaxValue = orderItem.UnitPriceInclTax;
                orderItemModel.UnitPriceExclTaxValue = orderItem.UnitPriceExclTax;
                orderItemModel.UnitPriceInclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.UnitPriceExclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                //discounts
                orderItemModel.DiscountInclTaxValue = orderItem.DiscountAmountInclTax;
                orderItemModel.DiscountExclTaxValue = orderItem.DiscountAmountExclTax;
                orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                //subtotal
                orderItemModel.SubTotalInclTaxValue = orderItem.PriceInclTax;
                orderItemModel.SubTotalExclTaxValue = orderItem.PriceExclTax;
                orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.PriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.PriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);

                orderItemModel.AttributeInfo = orderItem.AttributeDescription;
                if (orderItem.Product.IsRecurring)
                    orderItemModel.RecurringInfo = string.Format(_localizationService.GetResource("Admin.Orders.Products.RecurringPeriod"), orderItem.Product.RecurringCycleLength, orderItem.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
                //rental info
                if (orderItem.Product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : "";
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : "";
                    orderItemModel.RentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }

                //return requests
                orderItemModel.ReturnRequests = _returnRequestService
                    .SearchReturnRequests(orderItemId: orderItem.Id)
                    .Select(item => new OrderModel.OrderItemModel.ReturnRequestBriefModel
                    {
                        CustomNumber = item.CustomNumber,
                        Id = item.Id
                    }).ToList();

                //gift cards
                orderItemModel.PurchasedGiftCardIds = _giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id)
                    .Select(gc => gc.Id).ToList();

                model.Items.Add(orderItemModel);
            }
            model.HasDownloadableProducts = hasDownloadableItems;
            #endregion

            #region ProductsForSale

            //get a list of products for this store
            var sprod = _productService.SearchProducts(
                    storeId: _rposSettings.StoreId,
                    visibleIndividuallyOnly: true,
                    orderBy: ProductSortingEnum.NameAsc,
                    pageSize: 100);

            //loop each product for sale at the POS store and add
            foreach (var prod in sprod)
            {
                if (prod.LimitedToStores)
                { 
                    //get picture
                    string imageUrl = "";
                    var picture = _pictureService.GetPicturesByProductId(prod.Id, 1).FirstOrDefault();

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
                    var rposp = new rPOSProduct
                    {
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
            }

            #endregion
        }

        public ActionResult Delete(int id)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            //get order
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                //No order found with the specified id
                return RedirectToAction("List");

            //delete order
            _orderService.DeleteOrder(order);

            return RedirectToAction("List");
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        [ValidateInput(false)]
        public ActionResult EditOrderItem(int id, FormCollection form)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List", "PointofSaleOrder");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "PointofSaleOrder", new { id = id });

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnSaveOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnSaveOrderItem".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");


            decimal unitPriceExclTax, discountExclTax;
            int quantity;
            if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out unitPriceExclTax))
                unitPriceExclTax = orderItem.UnitPriceExclTax;
            if (!int.TryParse(form["pvQuantity" + orderItemId], out quantity))
                quantity = orderItem.Quantity;
            if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out discountExclTax))
                discountExclTax = orderItem.DiscountAmountExclTax;

            if (quantity > 0)
            {
                int qtyDifference = orderItem.Quantity - quantity;

                //adjust item prices
                orderItem.UnitPriceExclTax = unitPriceExclTax;
                orderItem.Quantity = quantity;
                orderItem.DiscountAmountExclTax = discountExclTax;
                _orderService.UpdateOrder(order);

                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, qtyDifference, orderItem.AttributesXml);

            }
            else
            {
                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml);

                //delete item
                _orderService.DeleteOrderItem(orderItem);
            }

            //update order totals
            UpdateOrderTotals(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            var model = new RailPointofSaleOrderModel();
            PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabName(persistForTheNextRequest: false);

            SuccessNotification("Item " + orderItem.Product.Name + " successfully modified on order!");

            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/Edit.cshtml", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteOrderItem")]
        [ValidateInput(false)]
        public ActionResult DeleteOrderItem(int id, FormCollection form)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List", "PointofSaleOrder");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "PointofSaleOrder", new { id = id });

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnDeleteOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnDeleteOrderItem".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //adjust inventory
            _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml);

            //delete item
            _orderService.DeleteOrderItem(orderItem);

            //update order totals
            UpdateOrderTotals(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order item has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            var model = new RailPointofSaleOrderModel();
            PrepareOrderDetailsModel(model, order);

            //selected tab
            SaveSelectedTabName(persistForTheNextRequest: false);

            SuccessNotification("Item successfully removed from order!");

            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/Edit.cshtml", model);
        }

        #endregion

        #region Add Products to Order

        public ActionResult AddProductToPOSOrderDetails(int orderId, int productId)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            //Setup Model
            var model = new AddPosProductModel
            {
                AddProduct = _productService.GetProductById(productId),
                ProductDetails = PrepareAddProductToPOSOrderModel(orderId, productId),
                ProductPicture = _pictureService.GetPictureUrl(_productService.GetProductById(productId).ProductPictures.FirstOrDefault().PictureId, targetSize: 200),
                ProductVendor = _vendorService.GetVendorById(_productService.GetProductById(productId).VendorId)
            };

            //Display View
            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/AddProductToPointofSaleOrder.cshtml", model);
        }

        [HttpPost]
        public ActionResult AddProductToPOSOrderDetails(int orderId, int productId, FormCollection form)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            var product = _productService.GetProductById(productId);
            //save order item

            //basic properties
            decimal unitPriceInclTax;
            decimal.TryParse(form["ProductDetails.UnitPriceInclTax"], out unitPriceInclTax);
            decimal unitPriceExclTax;
            decimal.TryParse(form["ProductDetails.UnitPriceExclTax"], out unitPriceExclTax);
            int quantity;
            int.TryParse(form["ProductDetails.Quantity"], out quantity);
            decimal priceInclTax;
            decimal.TryParse(form["ProductDetails.SubTotalInclTax"], out priceInclTax);
            decimal priceExclTax;
            decimal.TryParse(form["ProductDetails.SubTotalExclTax"], out priceExclTax);

            //warnings
            var warnings = new List<string>();

            //attributes
            var attributesXml = ParseProductAttributes(product, form);

            #region Gift cards

            string recipientName = "";
            string recipientEmail = "";
            string senderName = "";
            string senderEmail = "";
            string giftCardMessage = "";
            if (product.IsGiftCard)
            {
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals("giftcard.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.Message", StringComparison.InvariantCultureIgnoreCase))
                    {
                        giftCardMessage = form[formKey];
                        continue;
                    }
                }

                attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                    recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            #region Rental product

            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                throw new Exception("Rental support net yet implemented.");
            }

            #endregion

            //warnings
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(order.Customer, ShoppingCartType.ShoppingCart, product, quantity, attributesXml));
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemGiftCardWarnings(ShoppingCartType.ShoppingCart, product, attributesXml));
            warnings.AddRange(_shoppingCartService.GetRentalProductWarnings(product, rentalStartDate, rentalEndDate));
            if (!warnings.Any())
            {
                //no errors

                //attributes
                var attributeDescription = _productAttributeFormatter.FormatAttributes(product, attributesXml, order.Customer);

                //save item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    Order = order,
                    ProductId = product.Id,
                    UnitPriceInclTax = unitPriceInclTax,
                    UnitPriceExclTax = unitPriceExclTax,
                    PriceInclTax = priceInclTax,
                    PriceExclTax = priceExclTax,
                    OriginalProductCost = _priceCalculationService.GetProductCost(product, attributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = attributesXml,
                    Quantity = quantity,
                    DiscountAmountInclTax = decimal.Zero,
                    DiscountAmountExclTax = decimal.Zero,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate
                };
                order.OrderItems.Add(orderItem);
                _orderService.UpdateOrder(order);

                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, -orderItem.Quantity, orderItem.AttributesXml);

                //update order totals
                UpdateOrderTotals(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A new order item has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                //gift cards
                if (product.IsGiftCard)
                {
                    for (int i = 0; i < orderItem.Quantity; i++)
                    {
                        var gc = new GiftCard
                        {
                            GiftCardType = product.GiftCardType,
                            PurchasedWithOrderItem = orderItem,
                            Amount = unitPriceExclTax,
                            IsGiftCardActivated = false,
                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                            RecipientName = recipientName,
                            RecipientEmail = recipientEmail,
                            SenderName = senderName,
                            SenderEmail = senderEmail,
                            Message = giftCardMessage,
                            IsRecipientNotified = false,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        _giftCardService.InsertGiftCard(gc);
                    }
                }

                //Calculate Order Total
                UpdateOrderTotals(order);

                SuccessNotification("Item " + orderItem.Product.Name + " successfully added to order!");

                //redirect to order details page
                return RedirectToAction("Edit", "PointofSaleOrder", new { id = order.Id });
            }

            //errors
            var model = new AddPosProductModel
            {
                AddProduct = _productService.GetProductById(productId),
                ProductDetails = PrepareAddProductToPOSOrderModel(orderId, productId),
                ProductPicture = _pictureService.GetPictureUrl(_productService.GetProductById(productId).ProductPictures.FirstOrDefault().PictureId, targetSize: 200),
                ProductVendor = _vendorService.GetVendorById(_productService.GetProductById(productId).VendorId)
            };
            model.ProductDetails.Warnings.AddRange(warnings);
            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/AddProductTorPosOrderModel.cshtml", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GetProductAttributeCost(int productId, bool validateAttributeConditions, FormCollection form)
        {
            var product = _productService.GetProductById(productId);
            var attributeXml = ParseProductAttributes(product, form);
            var attcombos = _productAttributeService.GetAllProductAttributeCombinations(productId);

            string itemprice = "";

            foreach (var item in attcombos)
            {
                if (item.AttributesXml.ToUpper() == attributeXml.ToUpper())
                {
                    if (item.OverriddenPrice != null && item.OverriddenPrice > 0)
                    {
                        itemprice = item.OverriddenPrice.ToString();
                    }
                }
            }


            return Json(new
            {
                UnitPriceExclTax = itemprice
            });
        }

        [NonAction]
        protected virtual OrderModel.AddOrderProductModel.ProductDetailsModel PrepareAddProductToPOSOrderModel(int orderId, int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var presetQty = 1;
            var presetPrice = _priceCalculationService.GetFinalPrice(product, order.Customer, decimal.Zero, true, presetQty);
            decimal taxRate;
            decimal presetPriceInclTax = _taxService.GetProductPrice(product, presetPrice, true, order.Customer, out taxRate);
            decimal presetPriceExclTax = _taxService.GetProductPrice(product, presetPrice, false, order.Customer, out taxRate);

            var model = new OrderModel.AddOrderProductModel.ProductDetailsModel
            {
                ProductId = productId,
                OrderId = orderId,
                Name = product.Name,
                ProductType = product.ProductType,
                UnitPriceExclTax = presetPriceExclTax,
                UnitPriceInclTax = presetPriceInclTax,
                Quantity = presetQty,
                SubTotalExclTax = presetPriceExclTax,
                SubTotalInclTax = presetPriceInclTax,
                AutoUpdateOrderTotals = _orderSettings.AutoUpdateOrderTotalsOnEditingOrder
            };

            //attributes
            var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in attributes)
            {
                var attributeModel = new OrderModel.AddOrderProductModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    HasCondition = !String.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new OrderModel.AddOrderProductModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
            model.HasCondition = model.ProductAttributes.Any(a => a.HasCondition);
            //gift card
            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;
            }
            //rental
            model.IsRental = product.IsRental;
            return model;
        }

        [NonAction]
        protected virtual string ParseProductAttributes(Product product, FormCollection form)
        {
            var attributesXml = string.Empty;

            #region Product attributes

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = string.Format("product_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(form[controlId], out downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            #endregion

            return attributesXml;
        }



        #endregion

        #region Update Order Totals

        [NonAction]
        protected virtual void UpdateOrderTotals(Order order)
        {
            //Pos order so always clear shipping
            order.OrderShippingExclTax = 0.00M;
            order.OrderShippingInclTax = 0.00M;

            if (order.OrderItems.Count() != 0)
            {
                //Get Tax Rate
                decimal OrderTaxRate = 0.0000M;
                decimal OrderTaxMuti = 0.0000M;
                if (!String.IsNullOrEmpty(order.TaxRates))
                {
                    OrderTaxRate = _rposSettings.StoreTaxRate / 100.0000M;
                    OrderTaxMuti = OrderTaxRate + 1;
                }

                //Order Subtotal
                decimal OrderSubTotalExclTax = 0.00M;
                decimal OrderSubTotalInclTax = 0.00M;

                //Update Order Items Price
                foreach (var item in order.OrderItems)
                {
                    //Calculate Item Unit Price
                    item.PriceExclTax = (item.UnitPriceExclTax * item.Quantity) - item.DiscountAmountExclTax;

                    //Calculate Tax On Item
                    item.UnitPriceInclTax = Math.Round(item.UnitPriceExclTax * OrderTaxMuti, 2);
                    item.PriceInclTax = Math.Round(item.PriceExclTax * OrderTaxMuti, 2);
                    item.DiscountAmountInclTax = Math.Round(item.DiscountAmountExclTax, 2);

                    //Add Order Subtotals
                    OrderSubTotalExclTax = OrderSubTotalExclTax + item.PriceExclTax;
                    OrderSubTotalInclTax = OrderSubTotalInclTax + item.PriceInclTax;
                }

                //Set Order Subtotals
                order.OrderSubtotalExclTax = OrderSubTotalExclTax;
                order.OrderSubtotalInclTax = OrderSubTotalInclTax;

                //Order Shipping Tax
                order.OrderShippingInclTax = Math.Round(order.OrderShippingExclTax * OrderTaxMuti, 2);

                //Create Order Total Excl Tax
                decimal OrderTotalExclTax = (order.OrderSubtotalExclTax + order.OrderShippingExclTax) - order.OrderDiscount;

                //Order Tax
                order.OrderTax = Math.Round(OrderTotalExclTax * OrderTaxRate, 2);

                //Order Total
                order.OrderTotal = Math.Round(OrderTotalExclTax * OrderTaxMuti, 2);

                //Update Tax Rate
                order.TaxRates = Math.Round(OrderTaxRate * 100.000M, 2) + ":" + Math.Round(order.OrderTax, 2) + ";";

                //Update Database
                _orderService.UpdateOrder(order);
            }
            else
            {
                //we have no items so clear all totals
                ClearOrderTotals(order);
            }
        }

        [NonAction]
        protected virtual void ClearOrderTotals(Order order)
        {
            order.OrderSubtotalExclTax = 0M;
            order.OrderSubtotalInclTax = 0M;
            order.OrderSubTotalDiscountExclTax = 0M;
            order.OrderSubTotalDiscountInclTax = 0M;
            order.OrderTax = 0M;
            order.OrderDiscount = 0M;
            order.OrderTotal = 0M;

            //Update Database
            _orderService.UpdateOrder(order);
        }

        [NonAction]
        protected virtual TaxAmountResult CalculateTax(decimal ValueAmt)
        {
            return new TaxAmountResult
            {
                AmountIncTax = ValueAmt + (ValueAmt * (_rposSettings.StoreTaxRate / 100M)),
                AmountExcTax = ValueAmt,
                TaxAmount = ValueAmt * (_rposSettings.StoreTaxRate / 100M),
                TaxRateInfo = _rposSettings.StoreTaxRate.ToString() + ":" + (ValueAmt * (_rposSettings.StoreTaxRate / 100.0000M)).ToString() + ";"
            };
        }

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

                //update extended info - first and last name
                GenericAttribute gaFirstName = new GenericAttribute
                {
                    EntityId = posCustomer.Id,
                    KeyGroup = "Customer",
                    Key = "FirstName",
                    Value = "POS",
                    StoreId = _rposSettings.StoreId
                };

                _genericAttributeService.InsertAttribute(gaFirstName);

                GenericAttribute gaLastName = new GenericAttribute
                {
                    EntityId = posCustomer.Id,
                    KeyGroup = "Customer",
                    Key = "LastName",
                    Value = "Customer #" + posCustomer.Id.ToString(),
                    StoreId = _rposSettings.StoreId
                };

                _genericAttributeService.InsertAttribute(gaLastName);
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

        #region Process Payment
        [HttpGet]
        public ActionResult ProcessPointofSaleOrder(int id)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            //create order model
            var order = _orderService.GetOrderById(id);
            RailPointofSaleOrderModel _rPOSOrderModel = new RailPointofSaleOrderModel();
            PrepareOrderDetailsModel(_rPOSOrderModel, order);


            //load payment method
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new Exception("Null payment method not yet implemented.");

            //model
            var model = PreparePointofSalePaymentInfoModel(paymentMethod, _rPOSOrderModel);

            //years
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            //months
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = text,
                });
            }

            ViewBag.PublicKey = "";

            //return View
            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/ProcessPointofSaleOrder.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnterPaymentInfo(FormCollection form)
        {
            if (!HasAccessToPos())
                return AccessDeniedView();

            //create order model
            int orderId = Convert.ToInt32(form["rPosOrderModel.Id"]);
            string customerEmail = form["CustomerEmailAddress"];
            string customerFirstName = form["CustomerFirstName"];
            string customerLastName = form["CustomerLastName"];
            var order = _orderService.GetOrderById(orderId);
            RailPointofSaleOrderModel _rPOSOrderModel = new RailPointofSaleOrderModel();
            PrepareOrderDetailsModel(_rPOSOrderModel, order);

            //load payment method
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new Exception("Null payment method not yet implemented.");

            var paymentControllerType = paymentMethod.GetControllerType();
            var paymentController = DependencyResolver.Current.GetService(paymentControllerType) as BasePaymentController;
            if (paymentController == null)
                throw new Exception("Payment controller cannot be loaded");

            var warnings = paymentController.ValidatePaymentForm(form);
            foreach (var warning in warnings)
                ModelState.AddModelError("", warning);

            if (ModelState.IsValid)
            {
                //process payment request
                var processPaymentRequest = paymentController.GetPaymentInfo(form);
                processPaymentRequest.OrderGuid = order.OrderGuid;
                processPaymentRequest.OrderTotal = order.OrderTotal;
                processPaymentRequest.PaymentMethodSystemName = order.PaymentMethodSystemName;
                var paymentResult = _paymentService.ProcessPayment(processPaymentRequest);

                if (paymentResult.Success) //we successfully processed the credit card payment
                {
                    //update order payment status
                    UpdateSuccessfulOrder(order, paymentResult, processPaymentRequest, customerEmail, customerFirstName, customerLastName);

                    //email customer receipt
                    EmailCustomerReceipt(order, customerEmail);

                    //notify of process
                    SuccessNotification("Order #" + order.Id.ToString() + " payment has been processed! Email receipt sent to " + customerEmail + "!");

                    //redirect back to finished screen
                    return RedirectToAction("List");
                }
                else
                {
                    ErrorNotification("Failed to process payment for order #" + order.Id.ToString() + ". Reason: " + String.Join(" ; ", paymentResult.Errors.ToArray()));
                }
            }

            //model
            var model = PreparePointofSalePaymentInfoModel(paymentMethod, _rPOSOrderModel);
            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/ProcessPointofSaleOrder.cshtml", model);
        }

        [NonAction]
        protected virtual PointofSalePaymentInfoModel PreparePointofSalePaymentInfoModel(IPaymentMethod paymentMethod, RailPointofSaleOrderModel _rPOSOrderModel)
        {
            var model = new PointofSalePaymentInfoModel();
            string actionName;
            string controllerName;
            RouteValueDictionary routeValues;
            paymentMethod.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);
            model.PaymentInfoActionName = actionName;
            model.PaymentInfoControllerName = controllerName;
            model.PaymentInfoRouteValues = routeValues;
            model.DisplayOrderTotals = _orderSettings.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab;
            model.rPosOrderModel = _rPOSOrderModel; 
            return model;
        }

        [NonAction]
        protected virtual void UpdateSuccessfulOrder(Order order, ProcessPaymentResult result, ProcessPaymentRequest request, String customerEmail, string customerFirstName, string customerLastName)
        {
            //update order payment status
            //order status
            order.OrderStatus = OrderStatus.Complete;
            order.PaymentStatus = PaymentStatus.Paid;
            order.ShippingStatus = ShippingStatus.ShippingNotRequired;

            //order transaction data
            order.AuthorizationTransactionCode = result.AuthorizationTransactionCode;
            order.AuthorizationTransactionId = result.AuthorizationTransactionId;
            order.AuthorizationTransactionResult = result.AuthorizationTransactionResult;
            order.CaptureTransactionId = result.CaptureTransactionId;
            order.CaptureTransactionResult = result.CaptureTransactionResult;
            order.SubscriptionTransactionId = result.SubscriptionTransactionId;

            //order email address
            order.ShippingAddress.Email = customerEmail;
            order.BillingAddress.Email = customerEmail;

            //update ordername
            order.BillingAddress.FirstName = customerFirstName;
            order.BillingAddress.LastName = customerLastName;

            //create shipments
            Shipment shipm = new Shipment
            {
                AdminComment = "Auto shipment for POS sale!",
                CreatedOnUtc = DateTime.Now.ToUniversalTime(),
                ShippedDateUtc = DateTime.Now.ToUniversalTime(),
                DeliveryDateUtc = DateTime.Now.ToUniversalTime(),
                OrderId = order.Id,
                TotalWeight = 0M,
                TrackingNumber = ""
            };

            //add shipment items
            foreach (var item in order.OrderItems)
            {
                ShipmentItem si = new ShipmentItem
                {
                    OrderItemId = item.Id,
                    Quantity = item.Quantity
                };

                shipm.ShipmentItems.Add(si);
            }

            //update in database
            _shipmentService.InsertShipment(shipm);

            //update order
            _orderService.UpdateOrder(order);
        }

        [NonAction]
        protected virtual void EmailCustomerReceipt(Order order, String customerEmail)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateByName("OrderPaid.CustomerNotification", _rposSettings.StoreId);
            var store = _storeService.GetStoreById(_rposSettings.StoreId);

            //email account
            var emailAccount = _emailAccountService.GetEmailAccountById(messageTemplate.EmailAccountId);

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, 1);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //event notification
            _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

            var toEmail = order.BillingAddress.Email;
            var toName = string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName);
            SendNotification(messageTemplate, emailAccount, 1, tokens, toEmail, toName);
        }

        [NonAction]
        protected virtual int SendNotification(MessageTemplate messageTemplate, EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens, string toEmailAddress, string toName)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException("messageTemplate");
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            //retrieve localized message template data
            var bcc = messageTemplate.GetLocalized(mt => mt.BccEmailAddresses, languageId);
            var subject = messageTemplate.GetLocalized(mt => mt.Subject, languageId);
            var body = messageTemplate.GetLocalized(mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }

        #endregion

        #region Activity log

        [NonAction]
        protected void LogEditOrder(int orderId)
        {
            _customerActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), orderId);
        }

        #endregion

        #region Utilities

        private ActionResult AccessDeniedView()
        {
            throw new NotImplementedException();
        }

        [NonAction]
        protected virtual bool HasAccessToProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            return product.VendorId == vendorId;
        }

        [NonAction]
        protected virtual bool HasAccessToPos()
        {
            if (_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) && _permissionService.Authorize(StandardPermissionProvider.ManageVendors) && _permissionService.Authorize(StandardPermissionProvider.ManageOrders))
            {
                return true;
            }

            return false;
        }

        #endregion

    }

    #region Data Types

    public class TaxAmountResult
    {
        public decimal AmountIncTax { get; set; }
        public decimal AmountExcTax { get; set; }
        public decimal TaxAmount { get; set; }
        public string TaxRateInfo { get; set; }
    }

    #endregion
}