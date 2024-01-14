using Abp.Auditing;
using Yootek.Controllers;
using Yootek.Web.Host.Controllers.ModelView;
using Microsoft.AspNetCore.Mvc;


namespace Yootek.Web.Host.Controllers
{
    [Route("api/[controller]/[action]")]
    [Audited]
    public class AuditLoginController: YootekControllerBase
    {
        public AuditLoginController() { }

        [HttpPost]
        [Route("SetLog")]
        public ActionResult SetLog(AuditLoginInput input)
        {
            return Ok();
        }
    }
}
