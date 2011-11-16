using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Spark;

namespace Snooze.MSpec
{
	public class TestableSparkViewEngine : IViewEngine
	{
		readonly string path;
		SparkViewEngine engine;

		public TestableSparkViewEngine(string path) {
			this.path = path;
			engine = new SparkViewEngine(Settings());
		}

		ISparkSettings Settings() { return new SparkSettings(); }

		public IEnumerable<string> ViewFolders { get; private set; }

		static IEnumerable<string> Namespaces()
		{
			return new string[] {};
		}

		static IEnumerable<string> Assemblies()
		{
			return new []
			       {
			       	typeof(string).Assembly.FullName,
			       	typeof(Controller).Assembly.FullName,
			       };
		}

		public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			throw new NotImplementedException();
		}



		public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			if(HasView(viewName))
				return new ViewEngineResult(null,null);

			return new ViewEngineResult(ViewFolders);
		}

		bool HasView(string viewName) { return false; }

		public void ReleaseView(ControllerContext controllerContext, IView view)
		{
			throw new NotImplementedException();
		}
	}
}