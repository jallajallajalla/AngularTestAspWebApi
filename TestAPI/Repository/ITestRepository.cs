using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAPI.Repository
{
    public interface ITestRepository
    {
        int GetId();
        IEnumerable<string> GetData();
    }


    public class TestRepository : ITestRepository
    {
        public IEnumerable<string> GetData()
        {
            var a = new List<string>();
            a.Add("Jalla");
            a.Add("Jalla2");

            return a;
        }

        public int GetId()
        {
            return 4;
        }
    }
}
