using System;
using System.Web;

namespace Snooze.Testing
{
    public class FakeCachePolicy : HttpCachePolicyBase
    {
        private HttpCacheVaryByContentEncodings varyByContentEncodings;

        private HttpCacheVaryByHeaders varyByHeaders = new HttpCacheVaryByHeaders();

        private HttpCacheVaryByParams varyByParams;

        public override void AddValidationCallback(HttpCacheValidateHandler handler, object data)
        {
            throw new NotImplementedException();
        }

        public override void AppendCacheExtension(string extension)
        {
            throw new NotImplementedException();
        }

        public override void SetAllowResponseInBrowserHistory(bool allow)
        {
            throw new NotImplementedException();
        }

        public HttpCacheability Cachability { get; set; }

        public override void SetCacheability(HttpCacheability cacheability)
        {
            Cachability = cacheability;
        }

        public override void SetCacheability(HttpCacheability cacheability, string field)
        {
            throw new NotImplementedException();
        }

        public string Etag { get; set; }

        public override void SetETag(string etag)
        {
            Etag = etag;
        }

        public override void SetETagFromFileDependencies()
        {
            throw new NotImplementedException();
        }

        public DateTime Expires { get; set; }

        public override void SetExpires(DateTime date)
        {
            Expires = date;
        }

        public DateTime LastModified { get; set; }

        public override void SetLastModified(DateTime date)
        {
            LastModified = date;
        }

        public override void SetLastModifiedFromFileDependencies()
        {
            throw new NotImplementedException();
        }

        public override void SetMaxAge(TimeSpan delta)
        {
            throw new NotImplementedException();
        }

        public override void SetNoServerCaching()
        {
            throw new NotImplementedException();
        }

        public override void SetNoStore()
        {
            throw new NotImplementedException();
        }

        public override void SetNoTransforms()
        {
            throw new NotImplementedException();
        }

        public override void SetOmitVaryStar(bool omit)
        {
            throw new NotImplementedException();
        }

        public override void SetProxyMaxAge(TimeSpan delta)
        {
            throw new NotImplementedException();
        }

        public override void SetRevalidation(HttpCacheRevalidation revalidation)
        {
            throw new NotImplementedException();
        }

        public override void SetSlidingExpiration(bool slide)
        {
            throw new NotImplementedException();
        }

        public override void SetValidUntilExpires(bool validUntilExpires)
        {
            throw new NotImplementedException();
        }

        public override void SetVaryByCustom(string custom)
        {
            throw new NotImplementedException();
        }

        public override HttpCacheVaryByContentEncodings VaryByContentEncodings
        {
            get { return varyByContentEncodings; }
        }

        public override HttpCacheVaryByHeaders VaryByHeaders
        {
            get { return varyByHeaders; }
        }

        public override HttpCacheVaryByParams VaryByParams
        {
            get { return varyByParams; }
        }
    }
}