﻿using System;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Login;
using EPiServer.Reference.Commerce.Site.Features.Registration.Blocks;
using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using EPiServer.Web.Mvc;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using System.Threading.Tasks;
using EPiServer.Reference.Commerce.Site.Features.Login.Models;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Login.Controllers;

namespace EPiServer.Reference.Commerce.Site.Features.Registration.Controllers
{
    public class OrderConfirmationRegistrationBlockController : LoginControllerBase<OrderConfirmationRegistrationBlock>
    {
        [HttpGet]
        public ActionResult Index(OrderConfirmationRegistrationBlock currentBlock)
        {
            OrderConfirmationRegistrationModel model = null;
            var orderNumber = ControllerContext.ParentActionViewContext.ViewData["OrderNumber"] as int? ?? -1;
            var contactId = ControllerContext.ParentActionViewContext.ViewData["ContactId"] as Guid? ?? Guid.Empty;
            var order = OrderContext.Current.GetPurchaseOrder(orderNumber);

            if (order == null || CustomerContext.Current.GetContactById(order.CustomerId) != null)
            {
                return null;
            }

            ApplicationUser user = UserService.GetUser(order.OrderAddresses.First().Email);

            if (user != null)
            {
                model = new OrderConfirmationRegistrationModel
                {
                    CurrentBlock = currentBlock,
                    FormModel = new OrderConfirmationRegistrationFormModel
                    {
                        OrderNumber = orderNumber,
                        ContactId = contactId
                    }
                };

                return PartialView(model);
            }

            model = new OrderConfirmationRegistrationModel
            {
                CurrentBlock = currentBlock,
                FormModel = new OrderConfirmationRegistrationFormModel                        
                {
                    OrderNumber = orderNumber,
                    ContactId = contactId
                }
            };

            return PartialView("NewCustomer", model);
        }

        [HttpPost]
        public async Task<ActionResult> Register(OrderConfirmationRegistrationBlock currentBlock, OrderConfirmationRegistrationFormModel formModel)
        {
            var purchaseOrder = OrderContext.Current.GetPurchaseOrder(formModel.OrderNumber);
            
            var model = new OrderConfirmationRegistrationModel
            {
                CurrentBlock = currentBlock,
                FormModel = formModel
            };

            if (purchaseOrder == null)
            {
                ModelState.AddModelError("FormModel.Password2", "Something went wrong");
            }

            if (!ModelState.IsValid || purchaseOrder == null)
            {
                return PartialView("Index", model);
            }

            ContactIdentityResult registration = await UserService.RegisterAccount(new ApplicationUser(purchaseOrder)
            { 
                Password = formModel.Password, 
                RegistrationSource = "Order confirmation page"
            });

            if (registration.Result.Succeeded)
            {
                if (registration.Contact.PrimaryKeyId.HasValue)
                {
                    purchaseOrder.CustomerId = registration.Contact.PrimaryKeyId.Value;
                    purchaseOrder.CustomerName = registration.Contact.FullName;
                    purchaseOrder.AcceptChanges();
                }
                return PartialView("Complete", registration.Contact.Email);
            }


            return PartialView("Index", model);
        }

        [HttpPost]
        public ActionResult Assign(OrderConfirmationRegistrationBlock currentBlock, OrderConfirmationRegistrationFormModelBase formModel)
        {
            var purchaseOrder = OrderContext.Current.GetPurchaseOrder(formModel.OrderNumber);
            var contact = UserService.GetCustomerContact(purchaseOrder.OrderAddresses.First().Email);
            if (contact.PrimaryKeyId.HasValue)
            {
                purchaseOrder.CustomerId = contact.PrimaryKeyId.Value;
                purchaseOrder.CustomerName = contact.FullName;
                purchaseOrder.AcceptChanges();
            }
            return PartialView("Complete2");
        }
    }
}