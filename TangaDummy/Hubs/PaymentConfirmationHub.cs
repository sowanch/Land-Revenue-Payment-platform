using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace TangaDummy.Hubs
{
    public class PaymentConfirmationHub : Hub
    {
        private Tanga_City_CouncilEntities db = new Tanga_City_CouncilEntities();

        public void Send(string client, string message, string connID)
        {
            while (true) {
                int pmtCount = db.PaymentTrailMessages.Where(v => v.CheckoutRequestID.Equals(message.Trim())).Count();
                if(pmtCount > 1)
                {
                    PaymentTrailMessage pmt = db.PaymentTrailMessages.Where(v => v.CheckoutRequestID.Equals(message.Trim())).OrderByDescending(v => v.OnlinePaymentTrailID).First();
                    if(pmt.ResponseCode != "0")
                    {
                        Clients.Client(connID).addNewMessageToPage(pmt.CustomerMessage, message.Trim(), pmt.ResponseCode);
                    }
                    else
                    {
                        ConfirmedPayment cpTime = db.ConfirmedPayments.Where(v => v.CheckoutRequestID.Equals(message.Trim())).First();
                        Clients.Client(connID).addNewMessageToPage(pmt.CustomerMessage, cpTime.ReceiptNumber, pmt.ResponseCode);
                    }

                    break;
                }
                else
                {

                }
                
                
            }
        }
    }
}