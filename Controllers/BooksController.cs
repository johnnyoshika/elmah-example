using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ElmahTests.Controllers
{
    public class BooksController : ApiController
    {
        public string Get()
        {
            // http://stackoverflow.com/a/15583497/188740
            throw new InvalidProgramException();
            return "Books";
        }
    }
}
