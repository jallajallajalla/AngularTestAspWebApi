using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TestAPI.Repository;

namespace TestAPI.Controllers.Api
{
    public class TestController : ApiController
    {

        private ITestRepository _testRepository;

        public TestController (ITestRepository testRepository)
        {
            _testRepository = testRepository;
        }

        // GET: api/Test
        public IEnumerable<string> Get()
        {

            return _testRepository.GetData();
            //return new string[] { "value1", "value2" };
        }

        // GET: api/Test/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Test
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Test/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Test/5
        public void Delete(int id)
        {
        }
    }
}
