using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TangaDummy;
using TangaDummy.Models;

namespace TangaDummy.Controllers
{
    public class WaterMetersController : Controller
    {
        private Tanga_City_CouncilEntities db = new Tanga_City_CouncilEntities();
        Result response = new Result();
        string consumeKey = ConfigurationManager.AppSettings["consumerkey"];
        string consumeSec = ConfigurationManager.AppSettings["consumersecret"];
        string bsc = ConfigurationManager.AppSettings["businessShortCode"];
        string pKey = ConfigurationManager.AppSettings["passKey"];
        string parA = ConfigurationManager.AppSettings["partyA"];
        string parB = ConfigurationManager.AppSettings["partyB"];
        string usename = ConfigurationManager.AppSettings["usename"];
        string pasword = ConfigurationManager.AppSettings["pasword"];

        // GET: WaterMeters
        public ActionResult Index()
        {
            // Create manager
            var manager = new UserManager<ApplicationUser>(
               new UserStore<ApplicationUser>(
                   new ApplicationDbContext()));

            // Find user
            var user = manager.FindById(User.Identity.GetUserId());

            //Find ID
            var nationalID = user.IDNumber;
            var waterMeters = db.WaterMeters.Where(v => v.PersonTable.IDNumber.Equals(nationalID)).Include(w => w.PersonTable);
            return View(waterMeters.ToList());
        }

        // GET: WaterMeters/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WaterMeter waterMeter = db.WaterMeters.Find(id);
            if (waterMeter == null)
            {
                return HttpNotFound();
            }
            return View(waterMeter);
        }

        // GET: WaterMeters/Create
        public ActionResult Create()
        {
            ViewBag.MeterOwner = new SelectList(db.PersonTables, "PersonID", "Person_Name");
            return View();
        }

        // POST: WaterMeters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MeterID,MeterNo,Meter_Status,BillName,Brand,MeterOwner,Ward,Street,Arrears,Meter_Account_No")] WaterMeter waterMeter)
        {
            if (ModelState.IsValid)
            {
                db.WaterMeters.Add(waterMeter);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MeterOwner = new SelectList(db.PersonTables, "PersonID", "Person_Name", waterMeter.MeterOwner);
            return View(waterMeter);
        }

        // GET: WaterMeters/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WaterMeter waterMeter = db.WaterMeters.Find(id);
            if (waterMeter == null)
            {
                return HttpNotFound();
            }
            ViewBag.MeterOwner = new SelectList(db.PersonTables, "PersonID", "Person_Name", waterMeter.MeterOwner);
            return View(waterMeter);
        }

        // POST: WaterMeters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MeterID,MeterNo,Meter_Status,BillName,Brand,MeterOwner,Ward,Street,Arrears,Meter_Account_No")] WaterMeter waterMeter, string payout)
        {
            if (ModelState.IsValid)
            {
                string billStatus;
                //Get building owner number
                PersonTable payer = db.PersonTables.Where(v => v.PersonID.Equals(waterMeter.MeterOwner)).First();

                TempData["CheckOutID"] = "ws_CO_DMZ_52859272_19072018112557828";

                //Register Payment
                Payment newPayment = new Payment { PaymentMade = decimal.Parse(payout), PermitType = 5, PaymentDate = DateTime.Now, PersonID = waterMeter.MeterOwner };
                db.Payments.Add(newPayment);

                //Update Wallet
                PersonWallet currentWallet = db.PersonWallets.Where(v => v.PersonTable.PersonID.Equals(waterMeter.MeterOwner)).First();
                currentWallet.CurrentAmount = currentWallet.CurrentAmount - decimal.Parse(payout);
                currentWallet.LastUpdate = DateTime.Now;

                //Update Building data
                waterMeter.Arrears = waterMeter.Arrears - decimal.Parse(payout);
                if (waterMeter.Arrears <= 0)
                {
                    billStatus = "Paid";
                }
                else
                {
                    billStatus = "Unpaid";
                }


                //Update GIS
                Result final = GetWithParams(usename, pasword, waterMeter.Arrears.ToString(), payout, billStatus, waterMeter.MeterNo.ToString());

                if (final.resultCode.Equals("Result Code: 0000"))
                {
                    //Update DB
                    db.Entry(waterMeter).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("MeterPaymentConfirmation", "Intermediate");
                }
                else
                {
                    return RedirectToAction("Edit");
                }

                


                /*db.Entry(waterMeter).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");*/
            }
            ViewBag.MeterOwner = new SelectList(db.PersonTables, "PersonID", "Person_Name", waterMeter.MeterOwner);
            return View(waterMeter);
        }

        // GET: WaterMeters/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WaterMeter waterMeter = db.WaterMeters.Find(id);
            if (waterMeter == null)
            {
                return HttpNotFound();
            }
            return View(waterMeter);
        }

        // POST: WaterMeters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            WaterMeter waterMeter = db.WaterMeters.Find(id);
            db.WaterMeters.Remove(waterMeter);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public Result GetWithParams(string username, string password, string arrears, string amountPaid, string annualAmount, string plotNumber)
        {

            try
            {
                //Get the Portal in question
                AGOL currentPortal = new AGOL(username, password);

                //define the feature service in question
                var featureServiceURL = "https://services.arcgis.com/CmINIEzurW7Tagtl/ArcGIS/rest/services/Tanga_Sample4Integration_WFL1/FeatureServer";

                //define the URL for querying the feature service based on the plot number
                var queryURL = featureServiceURL + "/0/query";

                //define the URL for updating a feature in the Feature Service
                var updateURL = featureServiceURL + "/0/updateFeatures";


                var tokenz = currentPortal.Token;

                var queryData = new NameValueCollection();
                queryData["where"] = "METER_NO = '" + plotNumber + "'";
                queryData["outFields"] = "*";
                queryData["token"] = tokenz;
                queryData["f"] = "json";

                string queryResult = @getQueryResponse(queryData, queryURL);

                var jObj = JsonConvert.DeserializeObject(queryResult) as JObject;
                JArray features = (JArray)jObj["features"];
                if (features.Count == 0)
                {
                    response.resultCode = "Result Code: 0002";
                    response.resultText = "Plot Number " + plotNumber + " not present in the Esri EA database";
                    return response;
                }
                string jsonGeom = ((JObject)features[0])["geometry"].ToString();


                //Update the feature in question

                //Create Features Name Value Collection


                var updateData = new NameValueCollection();
                updateData["token"] = tokenz;
                updateData["f"] = "json";



                string jsonf = "[{'geometry':" + Regex.Replace(((JObject)features[0])["geometry"].ToString(), @"\t\n\r", "") + "," +
                "'attributes': {'OBJECTID': " + ((JObject)features[0])["attributes"]["OBJECTID"].ToString() +
                ",'Bill_status': '" + annualAmount + "'}}]";


                updateData["features"] = jsonf;



                string updateResult = @getQueryResponse(updateData, updateURL);

                //Report back on the feature - whether updated or not
                var uObj = JsonConvert.DeserializeObject(updateResult) as JObject;

                if ((JArray)uObj["updateResults"] != null)
                {
                    JArray ufeatures = (JArray)uObj["updateResults"];

                    string boolResult = ((JObject)ufeatures[0])["success"].ToString();

                    response.resultText = updateResult;

                    if (boolResult == "True")
                    {
                        response.resultCode = "Result Code: 0000";
                        response.resultText = "OK! Feature number " + ((JObject)ufeatures[0])["objectId"].ToString() + " successfully updated";
                    }
                    else
                    {
                        response.resultCode = "Result Code: 0001";
                        response.resultText = updateResult + ". Contact Administrator";
                    }

                }
                else
                {
                    response.resultCode = "Result Code: 0003";
                    response.resultText = updateResult + ". Contact Administrator";
                }


            }
            catch (Exception ef)
            {
                response.resultCode = "Result Code: 0003";
                response.resultText = ef.Message + ". Contact Administrator";
            }

            return response;
        }

        private string getQueryResponse(NameValueCollection qData, string v)
        {
            string responseData;
            var webClient = new System.Net.WebClient();
            var response = webClient.UploadValues(v, qData);
            responseData = System.Text.Encoding.UTF8.GetString(response);
            return responseData;
        }
    }
}
