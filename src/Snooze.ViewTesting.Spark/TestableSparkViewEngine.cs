using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Spark;
using Spark.Compiler;
using Spark.Configuration;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace Snooze.ViewTesting.Spark
{
	public class BuildDescriptorParams
    {
        private readonly string _targetNamespace;
        private readonly string _controllerName;
        private readonly string _viewName;
        private readonly string _masterName;
        private readonly bool _findDefaultMaster;
        private readonly IDictionary<string, object> _extra;
        private static readonly IDictionary<string, object> _extraEmpty = new Dictionary<string, object>();
        private readonly int _hashCode;

        public BuildDescriptorParams(string targetNamespace, string controllerName, string viewName, string masterName, bool findDefaultMaster, IDictionary<string, object> extra)
        {
            _targetNamespace = targetNamespace;
            _controllerName = controllerName;
            _viewName = viewName;
            _masterName = masterName;
            _findDefaultMaster = findDefaultMaster;
            _extra = extra ?? _extraEmpty;

            // this object is meant to be immutable and used in a dictionary.
            // the hash code will always be used so it isn't calculated just-in-time.
            _hashCode = CalculateHashCode();
        }

        public string TargetNamespace
        {
            get { return _targetNamespace; }
        }

        public string ControllerName
        {
            get { return _controllerName; }
        }

        public string ViewName
        {
            get { return _viewName; }
        }

        public string MasterName
        {
            get { return _masterName; }
        }

        public bool FindDefaultMaster
        {
            get { return _findDefaultMaster; }
        }

        public IDictionary<string, object> Extra
        {
            get { return _extra; }
        }

        private static int Hash(object str)
        {
            return str == null ? 0 : str.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private int CalculateHashCode()
        {
            return Hash(_viewName) ^
                   Hash(_controllerName) ^
                   Hash(_targetNamespace) ^
                   Hash(_masterName) ^
                   _findDefaultMaster.GetHashCode() ^
                   _extra.Aggregate(0, (hash, kv) => hash ^ Hash(kv.Key) ^ Hash(kv.Value));
        }

        public override bool Equals(object obj)
        {
            var that = obj as BuildDescriptorParams;
            if (that == null || that.GetType() != GetType())
                return false;

            if (!string.Equals(_viewName, that._viewName) ||
                !string.Equals(_controllerName, that._controllerName) ||
                !string.Equals(_targetNamespace, that._targetNamespace) ||
                !string.Equals(_masterName, that._masterName) ||
                _findDefaultMaster != that._findDefaultMaster ||
                _extra.Count != that._extra.Count)
            {
                return false;
            }
            foreach (var kv in _extra)
            {
                object value;
                if (!that._extra.TryGetValue(kv.Key, out value) ||
                    !Equals(kv.Value, value))
                {
                    return false;
                }
            }
            return true;
        }
    }


	public class TestableSparkViewEngine : IViewEngine
	{
		readonly string path;
		SparkViewEngine engine;

		public TestableSparkViewEngine(string path)
		{
			this.path = path;
		    ViewFolders = ViewFolders ?? Directory.GetDirectories(path, "Views", SearchOption.AllDirectories);
			engine = new SparkViewEngine(Settings());
			 _grammar = new UseMasterGrammar(engine.Settings.Prefix);
		}

	    static ISparkSettings Settings()
		{
            var setttings = new TestSparkSettings(
                CreateViewFolders(ViewFolders),
				Namespaces,
				Assemblies);
				
			return setttings;
		}

        static IEnumerable<IViewFolderSettings> CreateViewFolders(IEnumerable<string> viewFolders)
        {
            return viewFolders.Select(p => new ViewFolderElement()
                {
                    FolderType = ViewFolderType.FileSystem,
                    Parameters = new Dictionary<string, string>
                        {
                            {"basePath", p}
                        }
                });
        }

		public static IEnumerable<string> ViewFolders { get;  set; }

		public static IEnumerable<string> Namespaces { get; set; }

		public static IEnumerable<string> Assemblies { get; set; }

		public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			return FindViewInternal(controllerContext, partialViewName,"",false);
		}



		public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) { return FindViewInternal(controllerContext, viewName, masterName, true); }

		private ViewEngineResult FindViewInternal(ControllerContext controllerContext, string viewName, string masterName, bool findDefaultMaster)
		{
			var searchedLocations = new List<string>();
			var targetNamespace = controllerContext.Controller.GetType().Namespace;

			var controllerName = controllerContext.RouteData.GetRequiredString("controller");

			var descriptorParams = new BuildDescriptorParams(
				targetNamespace,
				controllerName,
				viewName,
				masterName,
				findDefaultMaster,
				GetExtraParameters(controllerContext));

			ISparkViewEntry entry;

			var descriptor = BuildDescriptor(
				descriptorParams,
				searchedLocations);

			if (descriptor == null)
				return new ViewEngineResult(searchedLocations);

			entry = engine.CreateEntry(descriptor);
			return BuildResult(controllerContext.RequestContext, entry);
		}

		private ViewEngineResult BuildResult(RequestContext requestContext, ISparkViewEntry entry)
		{
			var view = (IView)entry.CreateInstance();
			if (view is TestSparkView)
			{
				var sparkView = (TestSparkView)view;
				sparkView.ResourcePathManager = engine.ResourcePathManager;
			}
			return new ViewEngineResult(view, this);
		}

		public SparkViewDescriptor CreateDescriptor(
			ControllerContext controllerContext,
			string viewName,
			string masterName,
			bool findDefaultMaster,
			ICollection<string> searchedLocations)
		{
			var targetNamespace = controllerContext.Controller.GetType().Namespace;

			var controllerName = controllerContext.RouteData.GetRequiredString("controller");

			return BuildDescriptor(
				new BuildDescriptorParams(
					targetNamespace,
					controllerName,
					viewName,
					masterName,
					findDefaultMaster,
					GetExtraParameters(controllerContext)),
				searchedLocations);
		}

		public SparkViewDescriptor CreateDescriptor(string targetNamespace, string controllerName, string viewName,
													string masterName, bool findDefaultMaster)
		{
			var searchedLocations = new List<string>();
			var descriptor = BuildDescriptor(
				new BuildDescriptorParams(
					targetNamespace /*areaName*/,
					controllerName,
					viewName,
					masterName,
					findDefaultMaster, null),
				searchedLocations);

			if (descriptor == null)
			{
				throw new CompilerException("Unable to find templates at " +
											string.Join(", ", searchedLocations.ToArray()));
			}
			return descriptor;
		}

		public void ReleaseView(ControllerContext controllerContext, IView view)
		{
		}
		

        public virtual IDictionary<string, object> GetExtraParameters(ControllerContext controllerContext)
        {
            var extra = new Dictionary<string, object>();

            return extra;
        }

        public virtual SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations)
        {
            var descriptor = new SparkViewDescriptor
                                 {
                                     TargetNamespace = buildDescriptorParams.TargetNamespace
                                 };

            if (!LocatePotentialTemplate(
                     PotentialViewLocations(buildDescriptorParams.ControllerName,
                         buildDescriptorParams.ViewName,
                         buildDescriptorParams.Extra),
                     descriptor.Templates,
                     searchedLocations))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(buildDescriptorParams.MasterName))
            {
                if (!LocatePotentialTemplate(
                         PotentialMasterLocations(buildDescriptorParams.MasterName,
                             buildDescriptorParams.Extra),
                         descriptor.Templates,
                         searchedLocations))
                {
                    return null;
                }
            }
            else if (buildDescriptorParams.FindDefaultMaster && TrailingUseMasterName(descriptor) == null /*empty is a valid value*/)
            {
                LocatePotentialTemplate(
                    PotentialDefaultMasterLocations(buildDescriptorParams.ControllerName,
                        buildDescriptorParams.Extra),
                    descriptor.Templates,
                    null);
            }

            var trailingUseMaster = TrailingUseMasterName(descriptor);
            while (buildDescriptorParams.FindDefaultMaster && !string.IsNullOrEmpty(trailingUseMaster))
            {
                if (!LocatePotentialTemplate(
                         PotentialMasterLocations(trailingUseMaster,
                            buildDescriptorParams.Extra),
                         descriptor.Templates,
                         searchedLocations))
                {
                    return null;
                }
                trailingUseMaster = TrailingUseMasterName(descriptor);
            }

            return descriptor;
        }

        /// <summary>
        /// Simplified parser for &lt;use master=""/&gt; detection.
        /// TODO: get rid of this.
        /// switch to a cache of view-file to master location with iscurrent detection?
        /// </summary>
        class UseMasterGrammar : CharGrammar
        {
            public UseMasterGrammar(string _prefix)
            {
                var whiteSpace0 = Rep(Ch(char.IsWhiteSpace));
                var whiteSpace1 = Rep1(Ch(char.IsWhiteSpace));
                var startOfElement = !string.IsNullOrEmpty(_prefix) ? Ch("<" + _prefix + ":use") : Ch("<use");
                var startOfAttribute = Ch("master").And(whiteSpace0).And(Ch('=')).And(whiteSpace0);
                var attrValue = Ch('\'').And(Rep(ChNot('\''))).And(Ch('\''))
                    .Or(Ch('\"').And(Rep(ChNot('\"'))).And(Ch('\"')));

                var endOfElement = Ch("/>");

                var useMaster = startOfElement
                    .And(whiteSpace1)
                    .And(startOfAttribute)
                    .And(attrValue)
                    .And(whiteSpace0)
                    .And(endOfElement)
                    .Build(hit => new string(hit.Left.Left.Down.Left.Down.ToArray()));

                ParseUseMaster =
                    pos =>
                    {
                        for (var scan = pos; scan.PotentialLength() != 0; scan = scan.Advance(1))
                        {
                            var result = useMaster(scan);
                            if (result != null)
                                return result;
                        }
                        return null;
                    };
            }

            public ParseAction<string> ParseUseMaster { get; set; }
        }

        private UseMasterGrammar _grammar;
        public ParseAction<string> ParseUseMaster { get { return _grammar.ParseUseMaster; } }

        public string TrailingUseMasterName(SparkViewDescriptor descriptor)
        {
            var lastTemplate = descriptor.Templates.Last();
            var sourceContext = AbstractSyntaxProvider.CreateSourceContext(lastTemplate, engine.ViewFolder);
            if (sourceContext == null)
                return null;
            var result = ParseUseMaster(new Position(sourceContext));
            return result == null ? null : result.Value;
        }

        private bool LocatePotentialTemplate(
            IEnumerable<string> potentialTemplates,
            ICollection<string> descriptorTemplates,
            ICollection<string> searchedLocations)
        {
            var template = potentialTemplates.FirstOrDefault(t => engine.ViewFolder.HasView(t));
            if (template != null)
            {
                descriptorTemplates.Add(template);
                return true;
            }
            if (searchedLocations != null)
            {
                foreach (var potentialTemplate in potentialTemplates)
                    searchedLocations.Add(potentialTemplate);
            }
            return false;
        }

        protected IEnumerable<string> ApplyFilters(IEnumerable<string> locations, IDictionary<string, object> extra) { return locations; }

        protected virtual IEnumerable<string> PotentialViewLocations(string controllerName, string viewName, IDictionary<string, object> extra)
        {
            if (extra.ContainsKey("area") && !string.IsNullOrEmpty(extra["area"] as string))
            {
                return ApplyFilters(new[]
                                        {
                                            string.Format("{0}{1}{2}.spark", controllerName, Path.DirectorySeparatorChar, viewName),
                                            string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, viewName),
                                            string.Format("~{0}Areas{0}{1}{0}Views{0}{2}{0}{3}.spark", Path.DirectorySeparatorChar, extra["area"], controllerName, viewName),
                                            string.Format("~{0}Areas{0}{1}{0}Views{0}Shared{0}{2}.spark", Path.DirectorySeparatorChar, extra["area"], viewName)
                                        }, extra);
            }
            return ApplyFilters(new[]
                                    {
                                        string.Format("{0}{1}{2}.spark", controllerName,Path.DirectorySeparatorChar, viewName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar,viewName)
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialMasterLocations(string masterName, IDictionary<string, object> extra)
        {
            return ApplyFilters(new[]
                                    {
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar,masterName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar,masterName)
                                    }, extra);
        }

        protected virtual IEnumerable<string> PotentialDefaultMasterLocations(string controllerName, IDictionary<string, object> extra)
        {
            return ApplyFilters(new[]
                                    {
                                        string.Format("Layouts{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Shared{0}{1}.spark", Path.DirectorySeparatorChar, controllerName),
                                        string.Format("Layouts{0}Application.spark", Path.DirectorySeparatorChar),
                                        string.Format("Shared{0}Application.spark", Path.DirectorySeparatorChar)
                                    }, extra);
        }
    }
}