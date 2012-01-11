using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Snooze.Testing
{
    public class FakeHttpFileCollection : HttpFileCollectionBase
    {
        private readonly Dictionary<string, HttpPostedFileBase> files;

        public FakeHttpFileCollection()
        {
            files = new Dictionary<string, HttpPostedFileBase>();
        }

        public override string[] AllKeys
        {
            get
            {
                return files.Keys.ToArray();
            }
        }

        public override HttpPostedFileBase this[string name]
        {
            get
            {
                return files[name];
            }
        }

        public override HttpPostedFileBase this[int index]
        {
            get
            {
                return files[AllKeys[index]];
            }
        }

        public override int Count
        {
            get
            {
                return files.Count;
            }
        }

        

    }
}