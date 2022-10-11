using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using ZyxMeBridge.Models.Common;

namespace ZyxMeBridge.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly AppSettings AppSettings;

        public ValuesController(IOptions<AppSettings> AppSettings)
        {
            this.AppSettings = AppSettings.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Json(new { version = "ZyxMe Bridge 2.6.14 - NET Core 3.1" });
        }

        [HttpGet]
        [Route("GetDirectory")]
        public IActionResult GetDirectory()
        {
            return Json(new { appLocation = AppDomain.CurrentDomain.BaseDirectory });
        }

        [HttpGet]
        [Route("isconnectedbd")]
        public ActionResult<string> IsConnectedBd()
        {
            try
            {
                decimal RetryDelay = 0;

                long RetryCount = 0;

                if (AppSettings.DatabaseSettings != null)
                {
                    RetryCount = AppSettings.DatabaseSettings.RetryCount;

                    RetryDelay = AppSettings.DatabaseSettings.RetryDelay;
                }

                while (RetryCount >= 0)
                {
                    try
                    {
                        return Ok("WORKING WITH DATABASE");
                    }
                    catch (Exception Exception)
                    {
                        if (RetryCount == 0)
                        {
                            throw Exception;
                        }
                        else
                        {
                            Random Random = new Random();

                            Task.Delay(Random.Next(0, (int)RetryDelay) * 1000).Wait();
                        }
                    }

                    RetryCount--;
                }
            }
            catch (Exception Exception)
            {
                return BadRequest($"WORKING WITHOUT DATABASE | {Exception.Message} | {JsonConvert.SerializeObject(Exception)}");
            }

            return BadRequest("WORKING WITHOUT DATABASE | GENERAL ERROR");
        }

        [HttpGet]
        [Route("CheckAppSettings")]
        public ActionResult CheckAppSettings()
        {
            return Ok(AppSettings);
        }
    }
}