using System.Web.Configuration;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Voice_API_OpenAI.Controllers
{
    public class HomeController : Controller
    {
        // POST method to get ephemeral token
        [HttpPost]
        public async Task<JsonResult> Session()
        {
            try
            {
                var apiKey = WebConfigurationManager.AppSettings["OpenAI_API_Key"];
                if (string.IsNullOrWhiteSpace(apiKey))
                    return Json(new { error = "API Key missing" }, JsonRequestBehavior.AllowGet);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

                    var body = new JObject
                    {
                        ["model"] = "gpt-4o-mini-realtime-preview",
                        ["voice"] = "alloy"
                        //["features"] = new JArray("transcribe")
                    }.ToString();

                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var resp = await client.PostAsync("https://api.openai.com/v1/realtime/sessions", content);

                    var text = await resp.Content.ReadAsStringAsync();
                    if (!resp.IsSuccessStatusCode)
                        return Json(new { error = text }, JsonRequestBehavior.AllowGet);

                    var json = JObject.Parse(text);
                    var ephemeralToken = json["client_secret"]?["value"]?.ToString();

                    if (string.IsNullOrEmpty(ephemeralToken))
                        return Json(new { error = "Failed to get ephemeral token" }, JsonRequestBehavior.AllowGet);

                    return Json(new { client_secret = new { value = ephemeralToken } }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (System.Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Voice()
        {
            return View();
        }
    }
}
