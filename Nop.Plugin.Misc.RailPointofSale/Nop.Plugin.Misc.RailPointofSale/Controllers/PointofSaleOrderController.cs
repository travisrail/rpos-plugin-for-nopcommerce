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
using Nop.Admin.Models.Orders;
using Nop.Services.Tax;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Shipping;
using Nop.Services.Logging;
using Nop.Services.Localization;

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
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;

        private RailPointofSaleSettings _rposSettings;

        private readonly OrderSettings _orderSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;
        #endregion

        public PointofSaleOrderController(IOrderService orderService, IOrderProcessingService orderProcessingService, IOrderTotalCalculationService orderTotalCalculationService, ICustomerService customerService, ISettingService settingService, IProductService productService, IAuthenticationService authenticationService, IPictureService pictureService, IVendorService vendorService, IPriceCalculationService priceCalculationService, ITaxService taxService, IProductAttributeService productAttributeService, IProductAttributeParser productAttributeParser, IShoppingCartService shoppingCartService, IProductAttributeFormatter productAttributeFormatter, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IGiftCardService giftCardService, IDownloadService downloadService,
            OrderSettings orderSettings,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            MeasureSettings measureSettings,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings)
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
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._shoppingCartService = shoppingCartService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._giftCardService = giftCardService;
            this._downloadService = downloadService;

            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._measureSettings = measureSettings;
            this._addressSettings = addressSettings;
            this._shippingSettings = shippingSettings;

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
            //create model
            RailPointofSaleOrderModel model = new RailPointofSaleOrderModel();
            model.Id = id;

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

        #region Add Products to Order

        public ActionResult AddProductToPOSOrderDetails(int orderId, int productId)
        {
            var model = PrepareAddProductToPOSOrderModel(orderId, productId);
            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/AddProductToPointofSaleOrder.cshtml", model);
        }

        [HttpPost]
        public ActionResult AddProductToPOSOrderDetails(int orderId, int productId, FormCollection form)
        {
            var order = _orderService.GetOrderById(orderId);
            var product = _productService.GetProductById(productId);
            //save order item

            //basic properties
            decimal unitPriceInclTax;
            decimal.TryParse(form["UnitPriceInclTax"], out unitPriceInclTax);
            decimal unitPriceExclTax;
            decimal.TryParse(form["UnitPriceExclTax"], out unitPriceExclTax);
            int quantity;
            int.TryParse(form["Quantity"], out quantity);
            decimal priceInclTax;
            decimal.TryParse(form["SubTotalInclTax"], out priceInclTax);
            decimal priceExclTax;
            decimal.TryParse(form["SubTotalExclTax"], out priceExclTax);

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
                var updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = orderItem,
                    PriceInclTax = unitPriceInclTax,
                    PriceExclTax = unitPriceExclTax,
                    SubTotalInclTax = priceInclTax,
                    SubTotalExclTax = priceExclTax,
                    Quantity = quantity
                };
                _orderProcessingService.UpdateOrderTotals(updateOrderParameters);

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

                //redirect to order details page
                TempData["nop.admin.order.warnings"] = updateOrderParameters.Warnings;
                return RedirectToAction("Edit", "PointofSaleOrder", new { id = order.Id });
            }

            //errors
            var model = PrepareAddProductToPOSOrderModel(order.Id, product.Id);
            model.Warnings.AddRange(warnings);
            return View("~/Plugins/Misc.RailPointofSale/Views/PointofSaleOrder/AddProductToPointofSaleOrder.cshtml", model);
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

        #region Activity log

        [NonAction]
        protected void LogEditOrder(int orderId)
        {
            _customerActivityService.InsertActivity("EditOrder", _localizationService.GetResource("ActivityLog.EditOrder"), orderId);
        }

        #endregion
    }
}