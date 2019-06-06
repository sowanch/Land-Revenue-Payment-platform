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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TangaDummy;
using TangaDummy.Models;
using TangaDummy.Safaricom;
using static TangaDummy.Safaricom.MPesaExpressRequest;

namespace TangaDummy.Controllers
{
    public class BuildingsController : Controller
    {
        private Tanga_City_CouncilEntities db = new Tanga_City_CouncilEntities();
        Result response = new Result();
        string consumeKey = ConfigurationManager.AppSettings["consumerkey"];
        string consumeSec = ConfigurationManager.AppSettings["consumersecret"];
        string bsc = ConfigurationManager.AppSettings["businessShortCode"];
        string pKey = ConfigurationManager.AppSettings["passKey"];
        string parA = ConfigurationManager.AppSettings["partyA"];
        string parB = ConfigurationManager.AppSettings["partyB"];

        // GET: Buildings
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
            var buildings = db.Buildings.Where(v => v.PersonTable.IDNumber.Equals(nationalID)).Include(b => b.PersonTable);
            return View(buildings.ToList());
        }

        // GET: Buildings/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            return View(building);
        }

        // GET: Buildings/Create
        public ActionResult Create()
        {
            ViewBag.BuildingOwner = new SelectList(db.PersonTables, "PersonID", "Person_Name");
            return View();
        }

        // POST: Buildings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BuildingID,LandRefNo,BuildingType,BuildingName,BuildingAddress,BuildingOwner,BuildingFloors,PaymentStatus,Arrears")] Building building)
        {
            if (ModelState.IsValid)
            {
                db.Buildings.Add(building);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BuildingOwner = new SelectList(db.PersonTables, "PersonID", "Person_Name", building.BuildingOwner);
            return View(building);
        }

        // GET: Buildings/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            ViewBag.BuildingOwner = new SelectList(db.PersonTables, "PersonID", "Person_Name", building.BuildingOwner);

            return View(building);
        }

        // POST: Buildings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BuildingID,LandRefNo,BuildingType,BuildingName,BuildingOwner,PaymentStatus,Arrears")] Building building, string payout)
        {

            oathAccount accYetu = new oathAccount(consumeKey, consumeSec);

            if (ModelState.IsValid)
            {
                db.Entry(building).State = EntityState.Modified;
                db.Entry(building).Property(uco => uco.BuildingAddress).IsModified = false;
                db.Entry(building).Property(uco => uco.BuildingFloors).IsModified = false;

                string val = payout;
                string annualAmount;

                //Get building owner number
                PersonTable payer = db.PersonTables.Where(v => v.PersonID.Equals(building.BuildingOwner)).First();

                //send MPesa Request
                MPesaExpressRequest requestSend = new MPesaExpressRequest();

                //Record DateTime
                DateTime wakati = DateTime.Now;

                string pwd = createPassword(bsc, pKey, wakati);

                var tf = requestSend.send(int.Parse(bsc), pwd, wakati, int.Parse(payout), payer.PhoneNumber, parB, "Express", "Building Payment", accYetu.Token, building);

                if (tf.Contains("errorCode"))
                {
                    ErrorInfo ef = JsonConvert.DeserializeObject<ErrorInfo>(tf);
                    PaymentTrailMessage ptm = new PaymentTrailMessage
                    {
                        ResponseCode = ef.errorCode,
                        ResponseDescription = ef.errorMessage,
                        CheckoutRequestID = ef.requestId,
                        IDNumber = building.LandRefNo.ToString(),
                        PaymentStamp = DateTime.Now
                    };
                    db.PaymentTrailMessages.Add(ptm);

                    return RedirectToAction("Edit");
                }
                else
                {
                    TokenInfo x = JsonConvert.DeserializeObject<TokenInfo>(tf);
                    //Insert Payment Trail Messages
                    PaymentTrailMessage ptm = new PaymentTrailMessage
                    {
                        ResponseCode = x.ResponseCode.ToString(),
                        ResponseDescription = x.ResponseDescription,
                        CheckoutRequestID = x.CheckoutRequestID,
                        MerchantRequestID = x.MerchantRequestID,
                        CustomerMessage = x.CustomerMessage,
                        IDNumber = building.LandRefNo.ToString(),
                        PaymentStamp = DateTime.Now
                    };

                    db.PaymentTrailMessages.Add(ptm);

                    //Update DB
                    db.SaveChanges();


                    TempData["CheckOutID"] = x.CheckoutRequestID;

                    return RedirectToAction("PaymentConfirmation", "Intermediate");

                    
                }
            }

            return RedirectToAction("Edit");
        }

        private string createPassword(string bsc, string pKey, DateTime now)
        {
            string pwd;
            String appKeySecret = bsc + pKey + now.ToString("yyyyMMddHHmmss");
            pwd = Convert.ToBase64String(Encoding.Default.GetBytes(appKeySecret));
            return pwd;
        }

        // GET: Buildings/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Building building = db.Buildings.Find(id);
            if (building == null)
            {
                return HttpNotFound();
            }
            return View(building);
        }

        // POST: Buildings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Building building = db.Buildings.Find(id);
            db.Buildings.Remove(building);
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
                var featureServiceURL = "https://services6.arcgis.com/HwQk19ysBarANM17/ArcGIS/rest/services/Tanga_buildings/FeatureServer";

                //define the URL for querying the feature service based on the plot number
                var queryURL = featureServiceURL + "/0/query";

                //define the URL for updating a feature in the Feature Service
                var updateURL = featureServiceURL + "/0/updateFeatures";


                var tokenz = currentPortal.Token;

                var queryData = new NameValueCollection();
                queryData["where"] = "OBJECTID = " + plotNumber + "";
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
                ",'PaymentArrears': " + arrears +
                ",'PaymentStatus': '" + annualAmount +
                "', 'LastPayment': " + amountPaid + "}}]";


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
