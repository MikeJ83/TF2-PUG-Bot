using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TF2Pug;

namespace WebBot.Controllers
{
    public class PugController : Controller
    {
        //
        // GET: /Pug/

        [HttpPut]
		public ActionResult Finish(Guid id)
        {
			byte bluScore = Byte.Parse( Request.Form["bluScore"] );
			byte redScore = Byte.Parse( Request.Form["redScore"] );

			Pug.ReportScore( id, bluScore, redScore );

			return new ContentResult();
        }

    }
}
