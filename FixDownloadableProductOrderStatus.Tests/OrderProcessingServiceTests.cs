using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Plugins;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using NUnit.Framework;
using Rhino.Mocks;

using AllSystemsGo.Plugin.FixDownloadableProductOrderStatus;

namespace AllSystemsGo.Plugin.FixDownloadableProductOrderStatus.Tests
{
	[TestFixture]
	[Category("AllSystemsGo.Plugin.FixDownloadableProductOrderStatus")]
	public class OrderProcessingServiceTests
	{
		#region Fields

		private IWorkContext _workContext;
		private IStoreContext _storeContext;
		private ITaxService _taxService;
		private IShippingService _shippingService;
		private IShipmentService _shipmentService;
		private IPaymentService _paymentService;
		private ICheckoutAttributeParser _checkoutAttributeParser;
		private IDiscountService _discountService;
		private IGiftCardService _giftCardService;
		private IGenericAttributeService _genericAttributeService;
		private TaxSettings _taxSettings;
		private RewardPointsSettings _rewardPointsSettings;
		private ICategoryService _categoryService;
		private IProductAttributeParser _productAttributeParser;
		private IPriceCalculationService _priceCalcService;
		private IOrderTotalCalculationService _orderTotalCalcService;
		private IAddressService _addressService;
		private ShippingSettings _shippingSettings;
		private ILogger _logger;
		private IRepository<ShippingMethod> _shippingMethodRepository;
		private IRepository<DeliveryDate> _deliveryDateRepository;
		private IRepository<Warehouse> _warehouseRepository;
		private IOrderService _orderService;
		private IWebHelper _webHelper;
		private ILocalizationService _localizationService;
		private ILanguageService _languageService;
		private IProductService _productService;
		private IPriceFormatter _priceFormatter;
		private IProductAttributeFormatter _productAttributeFormatter;
		private IShoppingCartService _shoppingCartService;
		private ICheckoutAttributeFormatter _checkoutAttributeFormatter;
		private ICustomerService _customerService;
		private IEncryptionService _encryptionService;
		private IWorkflowMessageService _workflowMessageService;
		private ICustomerActivityService _customerActivityService;
		private ICurrencyService _currencyService;
		private PaymentSettings _paymentSettings;
		private OrderSettings _orderSettings;
		private LocalizationSettings _localizationSettings;
		private ShoppingCartSettings _shoppingCartSettings;
		private CatalogSettings _catalogSettings;
		private IOrderProcessingService _orderProcessingService;
		private IEventPublisher _eventPublisher;
		private CurrencySettings _currencySettings;
		private IAffiliateService _affiliateService;
		private IVendorService _vendorService;
		private IPdfService _pdfService;

		private IGeoLookupService _geoLookupService;
		private ICountryService _countryService;
		private CustomerSettings _customerSettings;
		private AddressSettings _addressSettings;

		private Store _store;

		private IOrderProcessingService _originalOrderProcessingService;

		#endregion

		[SetUp]
		public void SetUp()
		{
			_workContext = null;

			_store = new Store { Id = 1 };
			_storeContext = MockRepository.GenerateMock<IStoreContext>();
			_storeContext.Expect(x => x.CurrentStore).Return(_store);

			var pluginFinder = new PluginFinder();

			_shoppingCartSettings = new ShoppingCartSettings();
			_catalogSettings = new CatalogSettings();

			var cacheManager = new NopNullCache();

			_productService = MockRepository.GenerateMock<IProductService>();

			//price calculation service
			_discountService = MockRepository.GenerateMock<IDiscountService>();
			_categoryService = MockRepository.GenerateMock<ICategoryService>();
			_productAttributeParser = MockRepository.GenerateMock<IProductAttributeParser>();
			_priceCalcService = new PriceCalculationService(_workContext, _storeContext,
				_discountService, _categoryService,
				_productAttributeParser, _productService,
				cacheManager, _shoppingCartSettings, _catalogSettings);

			_eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
			_eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

			_localizationService = MockRepository.GenerateMock<ILocalizationService>();

			//shipping
			_shippingSettings = new ShippingSettings();
			_shippingSettings.ActiveShippingRateComputationMethodSystemNames = new List<string>();
			_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add("FixedRateTestShippingRateComputationMethod");
			_shippingMethodRepository = MockRepository.GenerateMock<IRepository<ShippingMethod>>();
			_deliveryDateRepository = MockRepository.GenerateMock<IRepository<DeliveryDate>>();
			_warehouseRepository = MockRepository.GenerateMock<IRepository<Warehouse>>();
			_logger = new NullLogger();
			_shippingService = new ShippingService(_shippingMethodRepository,
				_deliveryDateRepository,
				_warehouseRepository,
				_logger,
				_productService,
				_productAttributeParser,
				_checkoutAttributeParser,
				_genericAttributeService,
				_localizationService,
				_addressService,
				_shippingSettings,
				pluginFinder,
				_storeContext,
				_eventPublisher,
				_shoppingCartSettings,
				cacheManager);
			_shipmentService = MockRepository.GenerateMock<IShipmentService>();


			_paymentService = MockRepository.GenerateMock<IPaymentService>();
			_checkoutAttributeParser = MockRepository.GenerateMock<ICheckoutAttributeParser>();
			_giftCardService = MockRepository.GenerateMock<IGiftCardService>();
			_genericAttributeService = MockRepository.GenerateMock<IGenericAttributeService>();

			_geoLookupService = MockRepository.GenerateMock<IGeoLookupService>();
			_countryService = MockRepository.GenerateMock<ICountryService>();
			_customerSettings = new CustomerSettings();
			_addressSettings = new AddressSettings();

			//tax
			_taxSettings = new TaxSettings();
			_taxSettings.ShippingIsTaxable = true;
			_taxSettings.PaymentMethodAdditionalFeeIsTaxable = true;
			_taxSettings.DefaultTaxAddressId = 10;
			_addressService = MockRepository.GenerateMock<IAddressService>();
			_addressService.Expect(x => x.GetAddressById(_taxSettings.DefaultTaxAddressId)).Return(new Address { Id = _taxSettings.DefaultTaxAddressId });
			_taxService = new TaxService(_addressService, _workContext, _taxSettings,
				pluginFinder, _geoLookupService, _countryService, _customerSettings, _addressSettings);

			_rewardPointsSettings = new RewardPointsSettings();

			_orderTotalCalcService = new OrderTotalCalculationService(_workContext, _storeContext,
				_priceCalcService, _taxService, _shippingService, _paymentService,
				_checkoutAttributeParser, _discountService, _giftCardService,
				_genericAttributeService,
				_taxSettings, _rewardPointsSettings, _shippingSettings, _shoppingCartSettings, _catalogSettings);

			_orderService = MockRepository.GenerateMock<IOrderService>();
			_webHelper = MockRepository.GenerateMock<IWebHelper>();
			_languageService = MockRepository.GenerateMock<ILanguageService>();
			_priceFormatter = MockRepository.GenerateMock<IPriceFormatter>();
			_productAttributeFormatter = MockRepository.GenerateMock<IProductAttributeFormatter>();
			_shoppingCartService = MockRepository.GenerateMock<IShoppingCartService>();
			_checkoutAttributeFormatter = MockRepository.GenerateMock<ICheckoutAttributeFormatter>();
			_customerService = MockRepository.GenerateMock<ICustomerService>();
			_encryptionService = MockRepository.GenerateMock<IEncryptionService>();
			_workflowMessageService = MockRepository.GenerateMock<IWorkflowMessageService>();
			_customerActivityService = MockRepository.GenerateMock<ICustomerActivityService>();
			_currencyService = MockRepository.GenerateMock<ICurrencyService>();
			_affiliateService = MockRepository.GenerateMock<IAffiliateService>();
			_vendorService = MockRepository.GenerateMock<IVendorService>();
			_pdfService = MockRepository.GenerateMock<IPdfService>();

			_paymentSettings = new PaymentSettings
			{
				ActivePaymentMethodSystemNames = new List<string>
                {
                    "Payments.TestMethod"
                }
			};
			_orderSettings = new OrderSettings();

			_localizationSettings = new LocalizationSettings();

			_eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
			_eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

			_currencySettings = new CurrencySettings();

			_originalOrderProcessingService = new Nop.Services.Orders.OrderProcessingService
			(
				_orderService, _webHelper,
				_localizationService, _languageService,
				_productService, _paymentService, _logger,
				_orderTotalCalcService, _priceCalcService, _priceFormatter,
				_productAttributeParser, _productAttributeFormatter,
				_giftCardService, _shoppingCartService, _checkoutAttributeFormatter,
				_shippingService, _shipmentService, _taxService,
				_customerService, _discountService,
				_encryptionService, _workContext,
				_workflowMessageService, _vendorService,
				_customerActivityService, _currencyService, _affiliateService,
				_eventPublisher, _pdfService,
				_shippingSettings, _paymentSettings, _rewardPointsSettings,
				_orderSettings, _taxSettings, _localizationSettings,
				_currencySettings
			);

			_orderProcessingService = new AllSystemsGo.Plugin.FixDownloadableProductOrderStatus.OrderProcessingService
			(
				_orderService, _webHelper,
				_localizationService, _languageService,
				_productService, _paymentService, _logger,
				_orderTotalCalcService, _priceCalcService, _priceFormatter,
				_productAttributeParser, _productAttributeFormatter,
				_giftCardService, _shoppingCartService, _checkoutAttributeFormatter,
				_shippingService, _shipmentService, _taxService,
				_customerService, _discountService,
				_encryptionService, _workContext,
				_workflowMessageService, _vendorService,
				_customerActivityService, _currencyService, _affiliateService,
				_eventPublisher, _pdfService,
				_shippingSettings, _paymentSettings, _rewardPointsSettings,
				_orderSettings, _taxSettings, _localizationSettings,
				_currencySettings
			);
		}

		/// <summary>
		/// Prepares an Order object for testing downloadable products
		/// </summary>
		/// <returns></returns>
		private Order PrepareDownloadableOrderForTesting()
		{
			var order = new Order
			{
				OrderTotal = 1,
				PaymentStatus = PaymentStatus.Paid,
				PaidDateUtc = DateTime.UtcNow.AddDays(-1),
				OrderStatus = OrderStatus.Pending,
				ShippingStatus = ShippingStatus.ShippingNotRequired
			};

			order.OrderItems.Add(new OrderItem()
			{
				Product = new Product()
				{
					IsDownload = true,
					DownloadActivationType = DownloadActivationType.Manually,
					IsShipEnabled = false
				},
				IsDownloadActivated = false
			});

			return order;
		}

		/// <summary>
		/// Prepares an Order object identical to the downloadable version except it is not marked as downloadable
		/// </summary>
		/// <returns></returns>
		private Order PrepareNonDownloadableOrderForTesting()
		{
			var order = new Order
			{
				OrderTotal = 1,
				PaymentStatus = PaymentStatus.Paid,
				PaidDateUtc = DateTime.UtcNow.AddDays(-1),
				OrderStatus = OrderStatus.Pending,
				ShippingStatus = ShippingStatus.ShippingNotRequired
			};

			order.OrderItems.Add(new OrderItem()
			{
				Product = new Product()
				{
					IsDownload = false,
					DownloadActivationType = DownloadActivationType.Manually,
					IsShipEnabled = false
				},
				IsDownloadActivated = false
			});

			return order;
		}

		[Test]
		public void Downloadable_Order_Should_Be_Complete_Without_Plugin()
		{
			// verify baseline Nop behavior
			Order order = this.PrepareDownloadableOrderForTesting();

			_originalOrderProcessingService.CheckOrderStatus(order);

			Assert.AreEqual(OrderStatus.Complete, order.OrderStatus);
		}

		[Test]
		public void Downloadable_Order_Should_Be_Processing_With_Plugin()
		{
			Order order = this.PrepareDownloadableOrderForTesting();

			_orderProcessingService.CheckOrderStatus(order);

			Assert.AreEqual(OrderStatus.Processing, order.OrderStatus);
		}

		[Test]
		public void Non_Downloadable_Order_Without_Shipping_Should_Be_Complete_With_Plugin()
		{
			Order order = this.PrepareNonDownloadableOrderForTesting();

			_orderProcessingService.CheckOrderStatus(order);

			Assert.AreEqual(OrderStatus.Complete, order.OrderStatus);
		}
	}
}
